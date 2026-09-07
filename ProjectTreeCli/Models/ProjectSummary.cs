namespace ProjectTreeCli.Models;

public sealed class ProjectSummary
{
    public int DirectoryCount { get; set; }

    public int FileCount { get; set; }

    public long TotalSize { get; set; }

    public Dictionary<string, int> Extensions { get; set; } = [];

    public List<FileNode> LargestFiles { get; set; } = [];
}