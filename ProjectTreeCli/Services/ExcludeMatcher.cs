using ProjectTreeCli.Configuration;

namespace ProjectTreeCli.Services;

public sealed class ExcludeMatcher
{
    public bool IsExcluded(
        string path,
        IEnumerable<string> customExcludes)
    {
        var name = Path.GetFileName(path);

        if (DefaultExcludes.Items.Contains(name))
        {
            return true;
        }

        foreach (var exclude in customExcludes)
        {
            if (Matches(name, exclude))
            {
                return true;
            }
        }

        return false;
    }

    private bool Matches(
        string name,
        string pattern)
    {
        pattern = pattern.Trim();

        if (pattern.StartsWith("*."))
        {
            return Path.GetExtension(name)
                .Equals(
                    pattern[1..],
                    StringComparison.OrdinalIgnoreCase);
        }

        if (pattern.EndsWith('/'))
        {
            pattern = pattern.TrimEnd('/');
        }

        return name.Equals(
            pattern,
            StringComparison.OrdinalIgnoreCase);
    }
}