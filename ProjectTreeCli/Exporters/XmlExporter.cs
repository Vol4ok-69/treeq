using ProjectTreeCli.Models;
using System.Xml.Serialization;

namespace ProjectTreeCli.Exporters;

public sealed class XmlExporter : IExporter
{
    public string Export(DirectoryNode root, AppOptions options)
    {
        var serializer = new XmlSerializer(
            typeof(DirectoryNode));

        using var stringWriter = new StringWriter();

        serializer.Serialize(
            stringWriter,
            root);

        return stringWriter.ToString();
    }
}