namespace ProjectTreeCli.Models;

public sealed class DirectoryNode
{
    public string Name { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public List<DirectoryNode> Directories { get; set; } = [];

    public List<FileNode> Files { get; set; } = [];
}