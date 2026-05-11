namespace ProjectTreeCli.Services;

public sealed class GitIgnoreService
{
    public List<string> Load(string rootPath)
    {
        var gitIgnorePath = Path.Combine(
            rootPath,
            ".gitignore");

        if (!File.Exists(gitIgnorePath))
        {
            return [];
        }

        return File.ReadAllLines(gitIgnorePath)
            .Select(line => line.Trim())
            .Where(line =>
                !string.IsNullOrWhiteSpace(line) &&
                !line.StartsWith('#'))
            .ToList();
    }
}