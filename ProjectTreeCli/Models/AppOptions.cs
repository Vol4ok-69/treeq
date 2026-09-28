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
    public bool ShowReferences { get; set; }

    public bool ShowPackages { get; set; }
}