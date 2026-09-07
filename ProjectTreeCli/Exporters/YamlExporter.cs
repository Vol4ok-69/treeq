using ProjectTreeCli.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ProjectTreeCli.Exporters;

public sealed class YamlExporter : IExporter
{
    public string Export(DirectoryNode root, AppOptions options)
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(
                CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(root);
    }
}