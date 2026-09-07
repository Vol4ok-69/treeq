using ProjectTreeCli.Models;

namespace ProjectTreeCli.Exporters;

public interface IExporter
{
    string Export(DirectoryNode root, AppOptions options);
}