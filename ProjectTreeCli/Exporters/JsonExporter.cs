using ProjectTreeCli.Models;
using System.Text.Json;

namespace ProjectTreeCli.Exporters;

public sealed class JsonExporter : IExporter
{
    public string Export(DirectoryNode root, AppOptions options)
    {
        return JsonSerializer.Serialize(
            root,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }
}