using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services.Exporters;

public interface IExporter
{
    string Export(DirectoryNode root);
}