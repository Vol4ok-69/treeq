using DotNet.Globbing;
using ProjectTreeCli.Configuration;

namespace ProjectTreeCli.Services;

public sealed class ExcludeMatcher
{
    public bool IsExcluded(string path, IEnumerable<string> patterns)
    {
        var normalizedPath = path.Replace('\\', '/');

        var fileName = Path.GetFileName(path);

        if (DefaultExcludes.Items.Contains(fileName))
        {
            return true;
        }

        foreach (var pattern in patterns)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                continue;
            }

            var glob = Glob.Parse(NormalizePattern(pattern));

            if (glob.IsMatch(normalizedPath) || glob.IsMatch(fileName))
            {
                return true;
            }
        }

        return false;
    }

    private string NormalizePattern(string pattern)
    {
        return pattern.Replace('\\', '/').Trim();
    }
}