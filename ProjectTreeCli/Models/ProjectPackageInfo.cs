namespace ProjectTreeCli.Models;

public sealed class ProjectPackageInfo
{
    public string Project { get; set; } = string.Empty;

    public List<PackageInfo> Packages { get; set; } = [];
}

