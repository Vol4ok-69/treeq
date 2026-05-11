using ProjectTreeCli.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProjectTreeCli.Services.Exporters;

public sealed class YamlExporter : IExporter
{
    public string Export(DirectoryNode root)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(
                CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(root);
    }
}