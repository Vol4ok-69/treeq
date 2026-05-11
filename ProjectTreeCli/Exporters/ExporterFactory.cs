namespace ProjectTreeCli.Services.Exporters;

public static class ExporterFactory
{
    public static IExporter Create(string format)
    {
        return format.ToLower() switch
        {
            "json" => new JsonExporter(),
            "xml" => new XmlExporter(),
            "yaml" => new YamlExporter(),
            "yml" => new YamlExporter(),

            _ => throw new ArgumentException(
                $"Unsupported format: {format}")
        };
    }
}