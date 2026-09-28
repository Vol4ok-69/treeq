namespace ProjectTreeCli.Models;

public sealed class ProjectReferenceInfo
{
    public string Project { get; set; } = string.Empty;

    public List<string> References { get; set; } = [];
}