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