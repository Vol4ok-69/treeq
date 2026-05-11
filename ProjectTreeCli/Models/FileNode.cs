namespace ProjectTreeCli.Models;

public sealed class FileNode
{
    public string Name { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public long Size { get; set; }
}