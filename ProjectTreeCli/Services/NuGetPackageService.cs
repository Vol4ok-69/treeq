using ProjectTreeCli.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

namespace ProjectTreeCli.Services;

public sealed class NuGetPackageService
{
    public List<ProjectPackageInfo> GetPackages(string rootPath)
    {
        var result = new List<ProjectPackageInfo>();

        var target = FindDotNetTarget(rootPath);

        if (target is not null)
        {
            var output = RunDotNet(target);

            if (!string.IsNullOrWhiteSpace(output))
            {
                try
                {
                    result = ParsePackages(output);
                }
                catch (JsonException)
                {
                    // Project may use packages.config.
                }
            }
        }


        var legacyPackages = GetLegacyPackages(rootPath);

        MergePackages(result, legacyPackages);

        return result
            .OrderBy(project => project.Project)
            .ToList();
    }

    private List<ProjectPackageInfo> GetLegacyPackages(string rootPath)
    {
        var result = new List<ProjectPackageInfo>();

        var projectFiles = Directory
            .EnumerateFiles(
                rootPath,
                "*.*proj",
                SearchOption.AllDirectories)
            .Where(IsSupportedProject)
            .ToList();

        foreach (var projectFile in projectFiles)
        {
            var projectDirectory = Path.GetDirectoryName(projectFile);

            if (projectDirectory is null)
            {
                continue;
            }

            var packagesConfigPath = Path.Combine(
                projectDirectory,
                "packages.config");

            if (!File.Exists(packagesConfigPath))
            {
                continue;
            }

            var packages = ParsePackagesConfig(packagesConfigPath);

            if (packages.Count == 0)
            {
                continue;
            }

            result.Add(new ProjectPackageInfo
            {
                Project = Path.GetFileNameWithoutExtension(projectFile),
                Packages = packages
            });
        }

        return result;
    }

    private List<PackageInfo> ParsePackagesConfig(string path)
    {
        var document = XDocument.Load(path);

        return document
            .Descendants()
            .Where(element =>
                element.Name.LocalName == "package")
            .Select(element =>
            {
                var name = element.Attribute("id")?.Value;
                var version = element.Attribute("version")?.Value;

                if (string.IsNullOrWhiteSpace(name))
                {
                    return null;
                }

                return new PackageInfo
                {
                    Name = name,
                    Requested = version ?? string.Empty,
                    Resolved = version ?? string.Empty
                };
            })
            .Where(package => package is not null)
            .Select(package => package!)
            .GroupBy(
                package => package.Name,
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(package => package.Name)
            .ToList();
    }

    private void MergePackages(
    List<ProjectPackageInfo> target,
    List<ProjectPackageInfo> source)
    {
        foreach (var sourceProject in source)
        {
            var targetProject = target.FirstOrDefault(
                project =>
                    project.Project.Equals(
                        sourceProject.Project,
                        StringComparison.OrdinalIgnoreCase));

            if (targetProject is null)
            {
                target.Add(sourceProject);
                continue;
            }

            foreach (var sourcePackage in sourceProject.Packages)
            {
                var exists = targetProject.Packages.Any(
                    package =>
                        package.Name.Equals(
                            sourcePackage.Name,
                            StringComparison.OrdinalIgnoreCase));

                if (!exists)
                {
                    targetProject.Packages.Add(sourcePackage);
                }
            }

            targetProject.Packages = targetProject.Packages
                .OrderBy(package => package.Name)
                .ToList();
        }
    }

    private bool IsSupportedProject(string path)
    {
        var extension = Path.GetExtension(path);

        return extension.Equals(
                   ".csproj",
                   StringComparison.OrdinalIgnoreCase)
               || extension.Equals(
                   ".fsproj",
                   StringComparison.OrdinalIgnoreCase)
               || extension.Equals(
                   ".vbproj",
                   StringComparison.OrdinalIgnoreCase);
    }

    private string? FindDotNetTarget(string rootPath)
    {
        var solution = Directory
            .EnumerateFiles(
                rootPath,
                "*.slnx",
                SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        if (solution is not null)
        {
            return solution;
        }

        solution = Directory
            .EnumerateFiles(
                rootPath,
                "*.sln",
                SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        if (solution is not null)
        {
            return solution;
        }

        return Directory
            .EnumerateFiles(
                rootPath,
                "*.csproj",
                SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
    }

    private string RunDotNet(string target)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Path.GetDirectoryName(target)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("list");
        startInfo.ArgumentList.Add(target);
        startInfo.ArgumentList.Add("package");
        startInfo.ArgumentList.Add("--format");
        startInfo.ArgumentList.Add("json");

        using var process = Process.Start(startInfo);

        if (process is null)
        {
            return string.Empty;
        }

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return process.ExitCode == 0
            ? output
            : string.Empty;
    }

    private List<ProjectPackageInfo> ParsePackages(string json)
    {
        using var document = JsonDocument.Parse(json);

        var result = new List<ProjectPackageInfo>();

        if (!document.RootElement.TryGetProperty(
                "projects",
                out var projects))
        {
            return result;
        }

        foreach (var project in projects.EnumerateArray())
        {
            if (!project.TryGetProperty(
                    "path",
                    out var pathProperty))
            {
                continue;
            }

            var projectPath = pathProperty.GetString();

            if (string.IsNullOrWhiteSpace(projectPath))
            {
                continue;
            }

            var projectName = Path.GetFileNameWithoutExtension(projectPath);

            var packageInfo = new ProjectPackageInfo
            {
                Project = projectName
            };

            if (!project.TryGetProperty(
                    "frameworks",
                    out var frameworks))
            {
                continue;
            }

            foreach (var framework in frameworks.EnumerateArray())
            {
                if (!framework.TryGetProperty(
                        "topLevelPackages",
                        out var packages))
                {
                    continue;
                }

                foreach (var package in packages.EnumerateArray())
                {
                    if (!package.TryGetProperty(
                            "id",
                            out var idProperty))
                    {
                        continue;
                    }

                    var name = idProperty.GetString();

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    var requested = package.TryGetProperty(
                        "requestedVersion",
                        out var requestedProperty)
                        ? requestedProperty.GetString() ?? string.Empty
                        : string.Empty;

                    var resolved = package.TryGetProperty(
                        "resolvedVersion",
                        out var resolvedProperty)
                        ? resolvedProperty.GetString() ?? string.Empty
                        : string.Empty;

                    packageInfo.Packages.Add(new PackageInfo
                    {
                        Name = name,
                        Requested = requested,
                        Resolved = resolved
                    });
                }
            }

            packageInfo.Packages = packageInfo.Packages
                .GroupBy(
                    package => package.Name,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(package => package.Name)
                .ToList();

            if (packageInfo.Packages.Count > 0)
            {
                result.Add(packageInfo);
            }
        }

        return result;
    }
}