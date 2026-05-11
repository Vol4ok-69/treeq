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
}