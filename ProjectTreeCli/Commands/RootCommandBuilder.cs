using ProjectTreeCli.Models;
using ProjectTreeCli.Services;
using ProjectTreeCli.Services.Exporters;
using System.CommandLine;

namespace ProjectTreeCli.Commands;

public static class RootCommandBuilder
{
    public static RootCommand Build()
    {
        var depthOption = new Option<int>("--depth", "-d")
        {
            Description = "Tree depth",
            DefaultValueFactory = _ => 10
        };

        var excludeOption = new Option<string[]>("--exclude", "-e")
        {
            Description = "Excluded folders/files",
            AllowMultipleArgumentsPerToken = true
        };

        var onlyOption = new Option<string[]>("--only", "-o")
        {
            Description = "Only extensions",
            AllowMultipleArgumentsPerToken = true
        };

        var contentOption = new Option<bool>("--content", "-c")
        {
            Description = "Show file content"
        };

        var outputOption = new Option<string?>("--output", "-out")
        {
            Description = "Output file path"
        };

        var formatOption = new Option<string>("--format", "-f")
        {
            Description = "Export format",
            DefaultValueFactory = _ => "tree"
        };

        var rootCommand = new RootCommand("Project tree viewer")
        {
            depthOption,
            excludeOption,
            onlyOption,
            contentOption,
            outputOption,
            formatOption,
        };

        rootCommand.SetAction(parseResult =>
        {
            var options = new AppOptions
            {
                Depth = parseResult.GetValue(depthOption),
                ShowContent = parseResult.GetValue(contentOption),
                Excludes = parseResult.GetValue(excludeOption)?.ToList() ?? [],
                OnlyExtensions = parseResult.GetValue(onlyOption)?.ToList() ?? [],
                OutputPath = parseResult.GetValue(outputOption),
                Format = parseResult.GetValue(formatOption) ?? "tree",
            };

            using var writer = new OutputWriter(options.OutputPath);

            if (!string.IsNullOrWhiteSpace(options.OutputPath))
            {
                options.Excludes.Add(
                    Path.GetFileName(options.OutputPath));
            }

            var gitIgnoreService = new GitIgnoreService();

            var gitIgnoreExcludes = gitIgnoreService
                .Load(options.RootPath);

            options.Excludes.AddRange(gitIgnoreExcludes);

            var scanner = new FileScanner(
                new ExcludeMatcher());

            var renderer = new TreeRenderer();

            var result = scanner.Scan(options);

            if (options.Format == "tree")
            {
                renderer.Render(result, writer);
            }
            else
            {
                var exporter = ExporterFactory.Create(
                    options.Format);

                var exported = exporter.Export(result);

                writer.WriteLine(exported);
            }

            if (options.ShowContent)
            {
                var contentReader = new FileContentReader();

                contentReader.PrintContent(result, writer);
            }
        });

        return rootCommand;
    }
}