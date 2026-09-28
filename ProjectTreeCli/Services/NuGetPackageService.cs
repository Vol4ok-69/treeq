using ProjectTreeCli.Models;
using System.Diagnostics;
using System.Text.Json;

namespace ProjectTreeCli.Services;

public sealed class NuGetPackageService
{
    public List<ProjectPackageInfo> GetPackages(string rootPath)
    {
        var target = FindDotNetTarget(rootPath);

        if (target is null)
        {
            return [];
        }

        var output = RunDotNet(target);

        if (string.IsNullOrWhiteSpace(output))
        {
            return [];
        }

        return ParsePackages(output);
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