using ProjectTreeCli.Models;
using System.Xml.Serialization;

namespace ProjectTreeCli.Services.Exporters;

public sealed class XmlExporter : IExporter
{
    public string Export(DirectoryNode root)
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