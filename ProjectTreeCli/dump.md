# Project Structure

```text
ProjectTreeCli
├── Commands
│   └── RootCommandBuilder.cs
├── Configuration
│   ├── DefaultBinaryExtensions.cs
│   └── DefaultExcludes.cs
├── Exporters
│   ├── ExporterFactory.cs
│   ├── IExporter.cs
│   ├── JsonExporter.cs
│   ├── MarkdownExporter.cs
│   ├── TreeExporter.cs
│   ├── XmlExporter.cs
│   └── YamlExporter.cs
├── Helpers
│   └── SizeFormatter.cs
├── Models
│   ├── AppOptions.cs
│   ├── DirectoryNode.cs
│   ├── FileNode.cs
│   └── ProjectSummary.cs
├── Services
│   ├── BinaryDetector.cs
│   ├── ClipboardService.cs
│   ├── ExcludeMatcher.cs
│   ├── FileScanner.cs
│   ├── GitIgnoreService.cs
│   ├── OutputWriter.cs
│   ├── ProjectSummaryService.cs
│   ├── SummaryRenderer.cs
│   └── TreeBuilder.cs
├── Program.cs
└── ProjectTreeCli.csproj

```


# File Contents

## Commands\RootCommandBuilder.cs

```csharp
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
```

## Configuration\DefaultBinaryExtensions.cs

```csharp
namespace ProjectTreeCli.Configuration;

public static class DefaultBinaryExtensions
{
    public static readonly HashSet<string> Items =
    [
        ".png",
        ".jpg",
        ".jpeg",
        ".gif",
        ".bmp",
        ".webp",
        ".mp4",
        ".mp3",
        ".wav",
        ".dll",
        ".exe",
        ".so",
        ".zip",
        ".rar",
        ".7z",
        ".apk",
        ".jar"
    ];
}
```

## Configuration\DefaultExcludes.cs

```csharp
namespace ProjectTreeCli.Configuration;

public static class DefaultExcludes
{
    public static readonly HashSet<string> Items =
    [
        ".git",
        ".idea",
        ".vs",
        ".vscode",
        ".gradle",
        "bin",
        "obj",
        "packages",
        "node_modules",
        "dist",
        "build",
        "coverage",
        ".next",
        ".nuxt",
        "target",
        "out"
    ];
}
```

## Exporters\ExporterFactory.cs

```csharp
namespace ProjectTreeCli.Exporters;

public static class ExporterFactory
{
    public static IExporter Create(string format)
    {
        return format.ToLower() switch
        {
            "tree" => new TreeExporter(),

            "json" => new JsonExporter(),
            "xml" => new XmlExporter(),
            "yaml" => new YamlExporter(),
            "yml" => new YamlExporter(),
            "md" => new MarkdownExporter(),
            "markdown" => new MarkdownExporter(),

            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }
}
```

## Exporters\IExporter.cs

```csharp
using ProjectTreeCli.Models;

namespace ProjectTreeCli.Exporters;

public interface IExporter
{
    string Export(DirectoryNode root, AppOptions options);
}
```

## Exporters\JsonExporter.cs

```csharp
using ProjectTreeCli.Models;
using System.Text.Json;

namespace ProjectTreeCli.Exporters;

public sealed class JsonExporter : IExporter
{
    public string Export(DirectoryNode root, AppOptions options)
    {
        return JsonSerializer.Serialize(
            root,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }
}
```

## Exporters\MarkdownExporter.cs

```csharp
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

        builder.AppendLine("# Project Structure");
        builder.AppendLine();

        builder.AppendLine("```text");

        var treeBuilder = new TreeBuilder();

        builder.AppendLine(treeBuilder.Build(root));

        builder.AppendLine("```");

        builder.AppendLine();

        if (options.ShowContent)
        {
            builder.AppendLine();

            builder.AppendLine("# File Contents");

            RenderContent(root, root.FullPath, builder, new BinaryDetector());
        }

        return builder.ToString();
    }

    private void RenderContent(DirectoryNode directory, string rootPath, StringBuilder builder, BinaryDetector binaryDetector)
    {
        foreach (var childDirectory in directory.Directories)
        {
            RenderContent(childDirectory, rootPath, builder, binaryDetector);
        }

        foreach (var file in directory.Files)
        {
            var extension = Path.GetExtension(file.Name);

            if (DefaultBinaryExtensions.Items.Contains(extension))
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(rootPath, file.FullPath);

            builder.AppendLine();
            builder.AppendLine($"## {relativePath}");

            builder.AppendLine();

            builder.AppendLine($"```{GetLanguage(extension)}");

            try
            {
                var content = File.ReadAllText(file.FullPath);

                builder.AppendLine(content);
            }
            catch (Exception ex)
            {
                builder.AppendLine($"[ERROR] {ex.Message}");
            }

            builder.AppendLine("```");
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
            "Dockerfile" => "dockerfile",

            _ => string.Empty
        };
    }
}
```

## Exporters\TreeExporter.cs

```csharp
using ProjectTreeCli.Models;
using ProjectTreeCli.Services;

namespace ProjectTreeCli.Exporters;

public sealed class TreeExporter : IExporter
{
    public string Export(DirectoryNode root, AppOptions options)
    {
        var treeBuilder = new TreeBuilder();

        return treeBuilder.Build(root);
    }
}
```

## Exporters\XmlExporter.cs

```csharp
using ProjectTreeCli.Models;
using System.Xml.Serialization;

namespace ProjectTreeCli.Exporters;

public sealed class XmlExporter : IExporter
{
    public string Export(DirectoryNode root, AppOptions options)
    {
        var serializer = new XmlSerializer(
            typeof(DirectoryNode));

        using var stringWriter = new StringWriter();

        serializer.Serialize(
            stringWriter,
            root);

        return stringWriter.ToString();
    }
}
```

## Exporters\YamlExporter.cs

```csharp
using ProjectTreeCli.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProjectTreeCli.Exporters;

public sealed class YamlExporter : IExporter
{
    public string Export(DirectoryNode root, AppOptions options)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(
                CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(root);
    }
}
```

## Helpers\SizeFormatter.cs

```csharp
namespace ProjectTreeCli.Helpers;

public static class SizeFormatter
{
    public static string Format(long bytes)
    {
        const double kb = 1024;
        const double mb = kb * 1024;
        const double gb = mb * 1024;

        if (bytes >= gb)
        {
            return $"{bytes / gb:F2} GB";
        }

        if (bytes >= mb)
        {
            return $"{bytes / mb:F2} MB";
        }

        if (bytes >= kb)
        {
            return $"{bytes / kb:F2} KB";
        }

        return $"{bytes} B";
    }
}
```

## Models\AppOptions.cs

```csharp
namespace ProjectTreeCli.Models;

public sealed class AppOptions
{
    public string RootPath { get; set; } = Directory.GetCurrentDirectory();

    public int Depth { get; set; } = 10;

    public bool ShowContent { get; set; }

    public List<string> Excludes { get; set; } = [];

    public List<string> OnlyExtensions { get; set; } = [];

    public string? OutputPath { get; set; }

    public string Format { get; set; } = "tree";

    public long MaxFileSizeKb { get; set; } = 512;

    public bool ShowSize { get; set; }

    public bool ShowSummary { get; set; }

    public bool Clipboard { get; set; }

    // public bool Parallel { get; set; }

    public bool IgnoreHidden { get; set; }

    // public bool DockerIgnore { get; set; }

    // public bool LanguageStats { get; set; }

    // public bool DotNetMode { get; set; }

    // public bool AndroidMode { get; set; }

    // public string? DiffPath { get; set; }
}
```

## Models\DirectoryNode.cs

```csharp
namespace ProjectTreeCli.Models;

public sealed class DirectoryNode
{
    public string Name { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public long Size { get; set; }

    public List<DirectoryNode> Directories { get; set; } = [];

    public List<FileNode> Files { get; set; } = [];
}
```

## Models\FileNode.cs

```csharp
namespace ProjectTreeCli.Models;

public sealed class FileNode
{
    public string Name { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public long Size { get; set; }
}
```

## Models\ProjectSummary.cs

```csharp
namespace ProjectTreeCli.Models;

public sealed class ProjectSummary
{
    public int DirectoryCount { get; set; }

    public int FileCount { get; set; }

    public long TotalSize { get; set; }

    public Dictionary<string, int> Extensions { get; set; } = [];

    public List<FileNode> LargestFiles { get; set; } = [];
}
```

## Services\BinaryDetector.cs

```csharp
using ProjectTreeCli.Configuration;

namespace ProjectTreeCli.Services;

public sealed class BinaryDetector
{
    public bool IsBinary(string path)
    {
        var extension = Path.GetExtension(path);

        if (DefaultBinaryExtensions.Items.Contains(extension))
        {
            return true;
        }

        try
        {
            const int sampleSize = 8000;

            using var stream = File.OpenRead(path);

            var buffer = new byte[sampleSize];

            var bytesRead = stream.Read(
                buffer,
                0,
                buffer.Length);

            for (var i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == 0)
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return true;
        }
    }
}
```

## Services\ClipboardService.cs

```csharp
using TextCopy;

namespace ProjectTreeCli.Services;

public sealed class ClipboardManager
{
    public async Task CopyAsync(string text)
    {
        await ClipboardService.SetTextAsync(text);
    }
}
```

## Services\ExcludeMatcher.cs

```csharp
using DotNet.Globbing;
using ProjectTreeCli.Configuration;

namespace ProjectTreeCli.Services;

public sealed class ExcludeMatcher
{
    public bool IsExcluded(string path, IEnumerable<string> patterns)
    {
        var normalizedPath = path.Replace('\\', '/');

        var fileName = Path.GetFileName(path);

        if (DefaultExcludes.Items.Contains(fileName))
        {
            return true;
        }

        foreach (var pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                continue;
            }

            var glob = Glob.Parse(NormalizePattern(pattern));

            if (glob.IsMatch(normalizedPath) || glob.IsMatch(fileName))
            {
                return true;
            }
        }

        return false;
    }

    private string NormalizePattern(string pattern)
    {
        return pattern.Replace('\\', '/').Trim();
    }
}
```

## Services\FileScanner.cs

```csharp
using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services;

public sealed class FileScanner(ExcludeMatcher excludeMatcher)
{
    private readonly ExcludeMatcher _excludeMatcher = excludeMatcher;

    public DirectoryNode Scan(AppOptions options)
    {
        return ScanDirectory(options.RootPath, options, 0);
    }

    private DirectoryNode ScanDirectory(string path, AppOptions options, int depth)
    {
        var directoryNode = new DirectoryNode
        {
            Name = Path.GetFileName(path),
            FullPath = path
        };

        if (depth > options.Depth)
        {
            return directoryNode;
        }

        foreach (var directory in Directory.EnumerateDirectories(path))
        {
            var relativeDirectory = Path.GetRelativePath(options.RootPath, directory);

            if (_excludeMatcher.IsExcluded(relativeDirectory, options.Excludes))
            {
                continue;
            }
            if (options.IgnoreHidden && IsHidden(directory))
            {
                continue;
            }
            var childDirectory = ScanDirectory(directory, options, depth + 1);

            directoryNode.Directories.Add(childDirectory);
            directoryNode.Size += childDirectory.Size;
        }

        foreach (var file in Directory.EnumerateFiles(path))
        {
            if (_excludeMatcher.IsExcluded(file, options.Excludes))
            {
                continue;
            }

            var extension = Path.GetExtension(file);

            if (options.OnlyExtensions.Count > 0 && !options.OnlyExtensions.Contains(extension))
            {
                continue;
            }

            var fileInfo = new FileInfo(file);

            var maxBytes = options.MaxFileSizeKb * 1024;

            if (fileInfo.Length > maxBytes)
            {
                continue;
            }

            directoryNode.Files.Add(new FileNode
            {
                Name = Path.GetFileName(file),
                FullPath = file,
                Size = fileInfo.Length
            });
            directoryNode.Size += fileInfo.Length;
        }

        return directoryNode;
    }

    private bool IsHidden(string path)
    {
        var name = Path.GetFileName(path);

        if (name.StartsWith('.'))
        {
            return true;
        }

        try
        {
            var attributes = File.GetAttributes(path);

            return attributes.HasFlag(
                FileAttributes.Hidden);
        }
        catch
        {
            return false;
        }
    }
}
```

## Services\GitIgnoreService.cs

```csharp
namespace ProjectTreeCli.Services;

public sealed class GitIgnoreService
{
    public List<string> Load(string rootPath)
    {
        var gitIgnorePath = Path.Combine(
            rootPath,
            ".gitignore");

        if (!File.Exists(gitIgnorePath))
        {
            return [];
        }

        return File.ReadAllLines(gitIgnorePath)
            .Select(line => line.Trim())
            .Where(line =>
                !string.IsNullOrWhiteSpace(line) &&
                !line.StartsWith('#'))
            .ToList();
    }
}
```

## Services\OutputWriter.cs

```csharp
namespace ProjectTreeCli.Services;

public sealed class OutputWriter : IDisposable
{
    private readonly TextWriter _writer;

    public OutputWriter(string? outputPath = null)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            _writer = Console.Out;
        }
        else
        {
            _writer = new StreamWriter(
                outputPath,
                false,
                System.Text.Encoding.UTF8);
        }
    }

    public void WriteLine(string text = "")
    {
        _writer.WriteLine(text);
    }

    public void Dispose()
    {
        _writer.Flush();

        if (_writer != Console.Out)
        {
            _writer.Dispose();
        }
    }
}
```

## Services\ProjectSummaryService.cs

```csharp
using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services;

public sealed class ProjectSummaryService
{
    public ProjectSummary Build(DirectoryNode root)
    {
        var summary = new ProjectSummary();

        ProcessDirectory(root, summary);

        summary.LargestFiles = [.. summary
            .LargestFiles
            .OrderByDescending(x => x.Size)
            .Take(10)];

        return summary;
    }

    private void ProcessDirectory(
        DirectoryNode directory,
        ProjectSummary summary)
    {
        summary.DirectoryCount++;

        summary.TotalSize += directory.Size;

        foreach (var file in directory.Files)
        {
            summary.FileCount++;

            summary.LargestFiles.Add(file);

            var extension = Path.GetExtension(file.Name);

            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = "[no extension]";
            }

            if (!summary.Extensions.TryGetValue(extension, out int value))
            {
                value = 0;
                summary.Extensions[extension] = value;
            }

            summary.Extensions[extension] = ++value;
        }

        foreach (var childDirectory in directory.Directories)
        {
            ProcessDirectory(
                childDirectory,
                summary);
        }
    }
}
```

## Services\SummaryRenderer.cs

```csharp
using ProjectTreeCli.Helpers;
using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services;

public sealed class SummaryRenderer
{
    public void Render(
        ProjectSummary summary,
        OutputWriter writer)
    {
        writer.WriteLine();
        writer.WriteLine("Project Summary");
        writer.WriteLine("────────────────");
        writer.WriteLine();

        writer.WriteLine(
            $"Directories: {summary.DirectoryCount}");

        writer.WriteLine(
            $"Files: {summary.FileCount}");

        writer.WriteLine(
            $"Total Size: {SizeFormatter.Format(summary.TotalSize)}");

        writer.WriteLine();

        writer.WriteLine("Top Extensions");
        writer.WriteLine("──────────────");

        foreach (var extension in summary.Extensions
                     .OrderByDescending(x => x.Value)
                     .Take(10))
        {
            writer.WriteLine(
                $"{extension.Key}    {extension.Value}");
        }

        writer.WriteLine();

        writer.WriteLine("Largest Files");
        writer.WriteLine("─────────────");

        foreach (var file in summary.LargestFiles)
        {
            writer.WriteLine(
                $"{file.Name}    {SizeFormatter.Format(file.Size)}");
        }
    }
}
```

## Services\TreeBuilder.cs

```csharp
using ProjectTreeCli.Models;
using System.Text;

namespace ProjectTreeCli.Services;

public sealed class TreeBuilder
{
    public string Build(DirectoryNode root)
    {
        var builder = new StringBuilder();

        builder.AppendLine(root.Name);

        RenderDirectory(
            root,
            string.Empty,
            builder);

        return builder.ToString();
    }

    private void RenderDirectory(
        DirectoryNode directory,
        string indent,
        StringBuilder builder)
    {
        var items = new List<object>();

        items.AddRange(
            directory.Directories
                .OrderBy(x => x.Name));

        items.AddRange(
            directory.Files
                .OrderBy(x => x.Name));

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            var isLast = i == items.Count - 1;

            var prefix = isLast
                ? "└── "
                : "├── ";

            if (item is DirectoryNode childDirectory)
            {
                builder.AppendLine(
                    $"{indent}{prefix}{childDirectory.Name}");

                var nextIndent = indent +
                    (isLast ? "    " : "│   ");

                RenderDirectory(
                    childDirectory,
                    nextIndent,
                    builder);
            }

            if (item is FileNode file)
            {
                builder.AppendLine(
                    $"{indent}{prefix}{file.Name}");
            }
        }
    }
}
```

## Program.cs

```csharp
using ProjectTreeCli.Commands;

var command = RootCommandBuilder.Build();

return await command.Parse(args).InvokeAsync();
```

## ProjectTreeCli.csproj

```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="DotNet.Glob" Version="3.1.3" />
    <PackageReference Include="System.CommandLine" Version="3.0.0-preview.3.26207.106" />
    <PackageReference Include="TextCopy" Version="6.2.1" />
    <PackageReference Include="YamlDotNet" Version="17.1.0" />
  </ItemGroup>

</Project>

```

