using ProjectTreeCli.Exporters;
using ProjectTreeCli.Models;
using ProjectTreeCli.Services;
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

        var maxFileSizeOption = new Option<long>("--max-file-size", "-mfs")
        {
            Description = "Maximum file size in KB",
            DefaultValueFactory = _ => 512
        };

        var sizeOption = new Option<bool>("--size", "-s")
        {
            Description = "Show file and directory sizes"
        };

        var summaryOption = new Option<bool>("--summary")
        {
            Description = "Show project summary"
        };

        var clipboardOption = new Option<bool>("--clipboard")
        {
            Description = "Copy output to clipboard"
        };

        // var parallelOption = new Option<bool>("--parallel")
        // {
        //     Description = "Enable parallel scanning"
        // };

        var ignoreHiddenOption = new Option<bool>("--ignore-hidden")
        {
            Description = "Ignore hidden files"
        };

        // var dockerIgnoreOption = new Option<bool>("--docker-ignore")
        // {
        //     Description = "Respect .dockerignore"
        // };

        // var languageStatsOption = new Option<bool>("--language-stats")
        // {
        //     Description = "Show language statistics"
        // };

        // var dotNetOption = new Option<bool>("--dotnet")
        // {
        //     Description = ".NET project mode"
        // };

        // var androidOption = new Option<bool>("--android")
        // {
        //     Description = "Android project mode"
        // };

        // var diffOption = new Option<string>("--diff")
        // {
        //     Description = "Compare with another project"
        // };

        var rootCommand = new RootCommand("Project tree viewer")
        {
            depthOption,
            excludeOption,
            onlyOption,
            contentOption,
            outputOption,
            formatOption,
            maxFileSizeOption,
            sizeOption,
            summaryOption,
             clipboardOption,
            // parallelOption,
             ignoreHiddenOption,
            // dockerIgnoreOption,
            // languageStatsOption,
            // dotNetOption,
            // androidOption,
            // diffOption,
        };

        rootCommand.SetAction(async parseResult =>
        {
            var options = new AppOptions
            {
                Depth = parseResult.GetValue(depthOption),
                ShowContent = parseResult.GetValue(contentOption),
                Excludes = parseResult.GetValue(excludeOption)?.ToList() ?? [],
                OnlyExtensions = parseResult.GetValue(onlyOption)?.ToList() ?? [],
                OutputPath = parseResult.GetValue(outputOption),
                Format = parseResult.GetValue(formatOption) ?? "tree",
                MaxFileSizeKb = parseResult.GetValue(maxFileSizeOption),
                ShowSize = parseResult.GetValue(sizeOption),
                ShowSummary = parseResult.GetValue(summaryOption),
                Clipboard = parseResult.GetValue(clipboardOption),
                // Parallel = parseResult.GetValue(parallelOption),
                IgnoreHidden = parseResult.GetValue(ignoreHiddenOption),
                // DockerIgnore = parseResult.GetValue(dockerIgnoreOption),
                // LanguageStats = parseResult.GetValue(languageStatsOption),
                // DotNetMode = parseResult.GetValue(dotNetOption),
                // AndroidMode = parseResult.GetValue(androidOption),
                // DiffPath = parseResult.GetValue(diffOption),
            };

            if (options.ShowContent && options.Format == "tree")
            {
                options.Format = "markdown";
            }

            using var writer = new OutputWriter(options.OutputPath);

            if (!string.IsNullOrWhiteSpace(options.OutputPath))
            {
                options.Excludes.Add(Path.GetFileName(options.OutputPath));
            }

            var gitIgnoreService = new GitIgnoreService();

            var gitIgnoreExcludes = gitIgnoreService.Load(options.RootPath);

            options.Excludes.AddRange(gitIgnoreExcludes);

            var scanner = new FileScanner(new ExcludeMatcher());

            var result = scanner.Scan(options);

            var exporter = ExporterFactory.Create(options.Format);

            var exported = exporter.Export(result, options);

            writer.WriteLine(exported);

            if (options.Clipboard)
            {
                var clipboard = new ClipboardManager();

                await clipboard.CopyAsync(exported);

                writer.WriteLine("\n[Copied to clipboard]");
            }

            if (options.ShowSummary)
            {
                var summaryService = new ProjectSummaryService();

                var summary = summaryService.Build(result);

                var summaryRenderer = new SummaryRenderer();

                summaryRenderer.Render(summary, writer);
            }
        });

        return rootCommand;
    }
}