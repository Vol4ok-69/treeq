using ProjectTreeCli.Configuration;
using ProjectTreeCli.Models;
using ProjectTreeCli.Services;
using System.Text;

namespace ProjectTreeCli.Exporters;

public sealed class MarkdownExporter : IExporter
{
    public string Export(DirectoryNode root, AppOptions options)
    {
        var builder = new StringBuilder();

        // 1. Project Structure
        builder.AppendLine("# Project Structure");
        builder.AppendLine();

        builder.AppendLine("```text");

        var treeBuilder = new TreeBuilder();

        builder.AppendLine(treeBuilder.Build(root));

        builder.AppendLine("```");

        // 2. Project References
        if (options.ShowReferences)
        {
            var referenceService = new ProjectReferenceService();
            var references = referenceService.GetReferences(root.FullPath);

            RenderReferences(references, builder);
        }

        // 3. NuGet Packages
        if (options.ShowPackages)
        {
            var packageService = new NuGetPackageService();
            var packages = packageService.GetPackages(root.FullPath);

            RenderPackages(packages, builder);
        }

        // 4. File Contents
        if (options.ShowContent)
        {
            builder.AppendLine();
            builder.AppendLine("# File Contents");

            var binaryDetector = new BinaryDetector();
            var spreadsheetReader = new SpreadsheetReader();

            RenderContent(
                root,
                root.FullPath,
                builder,
                binaryDetector,
                spreadsheetReader);
        }

        return builder.ToString();
    }

    private void RenderContent(
        DirectoryNode directory,
        string rootPath,
        StringBuilder builder,
        BinaryDetector binaryDetector,
        SpreadsheetReader spreadsheetReader)
    {
        foreach (var childDirectory in directory.Directories)
        {
            RenderContent(
                childDirectory,
                rootPath,
                builder,
                binaryDetector,
                spreadsheetReader);
        }

        foreach (var file in directory.Files)
        {
            var extension = Path.GetExtension(file.Name)
                .ToLowerInvariant();

            var relativePath = Path.GetRelativePath(
                rootPath,
                file.FullPath);

            // CSV
            if (extension == ".csv")
            {
                builder.AppendLine();
                builder.AppendLine($"## {relativePath}");
                builder.AppendLine();

                try
                {
                    var content = spreadsheetReader.ReadCsv(file.FullPath);

                    builder.AppendLine(content);
                }
                catch (Exception ex)
                {
                    builder.AppendLine(
                        $"[ERROR] {ex.Message}");
                }

                continue;
            }

            // XLSX
            if (extension == ".xlsx")
            {
                builder.AppendLine();
                builder.AppendLine($"## {relativePath}");
                builder.AppendLine();

                try
                {
                    var content = spreadsheetReader.ReadXlsx(file.FullPath);

                    builder.AppendLine(content);
                }
                catch (Exception ex)
                {
                    builder.AppendLine(
                        $"[ERROR] {ex.Message}");
                }

                continue;
            }

            // Other binary files
            if (binaryDetector.IsBinary(file.FullPath))
            {
                continue;
            }

            builder.AppendLine();
            builder.AppendLine($"## {relativePath}");
            builder.AppendLine();

            builder.AppendLine(
                $"```{GetLanguage(extension)}");

            try
            {
                var content = File.ReadAllText(file.FullPath);

                builder.AppendLine(content);
            }
            catch (Exception ex)
            {
                builder.AppendLine(
                    $"[ERROR] {ex.Message}");
            }

            builder.AppendLine("```");
        }
    }

    private void RenderReferences(
    List<ProjectReferenceInfo> references,
    StringBuilder builder)
    {
        builder.AppendLine();
        builder.AppendLine("# Project References");
        builder.AppendLine();

        if (references.Count == 0)
        {
            builder.AppendLine("No project-to-project references found.");
            return;
        }

        foreach (var project in references)
        {
            builder.AppendLine($"## {project.Project}");
            builder.AppendLine();

            foreach (var reference in project.References)
            {
                builder.AppendLine($"- {reference}");
            }

            builder.AppendLine();
        }
    }

    private void RenderPackages(
        List<ProjectPackageInfo> projects,
        StringBuilder builder)
    {
        builder.AppendLine();
        builder.AppendLine("# NuGet Packages");
        builder.AppendLine();

        if (projects.Count == 0)
        {
            builder.AppendLine("No NuGet packages found.");
            return;
        }

        foreach (var project in projects)
        {
            builder.AppendLine($"## {project.Project}");
            builder.AppendLine();

            builder.AppendLine("| Package | Requested | Resolved |");
            builder.AppendLine("| --- | --- | --- |");

            foreach (var package in project.Packages)
            {
                builder.AppendLine(
                    $"| {package.Name} | {package.Requested} | {package.Resolved} |");
            }

            builder.AppendLine();
        }
    }
    private string GetLanguage(string extension)
    {
        return extension.ToLower() switch
        {
            ".cs" => "csharp",
            ".json" => "json",
            ".xml" => "xml",
            ".yml" => "yaml",
            ".yaml" => "yaml",
            ".md" => "markdown",
            ".sql" => "sql",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".kt" => "kotlin",
            ".java" => "java",
            ".py" => "python",
            ".rb" => "ruby",
            ".go" => "go",
            ".cpp" => "cpp",
            ".c" => "c",
            ".h" => "c",
            ".html" => "html",
            ".css" => "css",
            ".scss" => "scss",
            ".less" => "less",
            ".cshtml" => "razor",
            ".vb" => "vbnet",
            ".fs" => "fsharp",
            ".swift" => "swift",
            ".rs" => "rust",
            ".sh" => "bash",
            ".ps1" => "powershell",
            ".dockerfile" => "dockerfile",
            "dockerfile" => "dockerfile",

            _ => string.Empty
        };
    }
}