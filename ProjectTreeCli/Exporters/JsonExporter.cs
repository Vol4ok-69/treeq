using ProjectTreeCli.Models;
using System.Text.Json;

namespace ProjectTreeCli.Services.Exporters;

public sealed class JsonExporter : IExporter
{
    public string Export(DirectoryNode root)
    {
        return JsonSerializer.Serialize(
            root,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }
}