namespace ProjectTreeCli.Exporters;

public static class ExporterFactory
{
    public static IExporter Create(string format)
    {
        return format.ToLower() switch
        {
            "tree" => new TreeExporter(),

            "json" => new JsonExporter(),
            "xml" => new XmlExporter(),
            "yaml" => new YamlExporter(),
            "yml" => new YamlExporter(),
            "md" => new MarkdownExporter(),
            "markdown" => new MarkdownExporter(),

            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }
}