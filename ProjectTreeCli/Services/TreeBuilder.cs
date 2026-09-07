using ProjectTreeCli.Models;
using System.Text;

namespace ProjectTreeCli.Services;

public sealed class TreeBuilder
{
    public string Build(DirectoryNode root)
    {
        var builder = new StringBuilder();

        builder.AppendLine(root.Name);

        RenderDirectory(
            root,
            string.Empty,
            builder);

        return builder.ToString();
    }

    private void RenderDirectory(
        DirectoryNode directory,
        string indent,
        StringBuilder builder)
    {
        var items = new List<object>();

        items.AddRange(
            directory.Directories
                .OrderBy(x => x.Name));

        items.AddRange(
            directory.Files
                .OrderBy(x => x.Name));

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            var isLast = i == items.Count - 1;

            var prefix = isLast
                ? "└── "
                : "├── ";

            if (item is DirectoryNode childDirectory)
            {
                builder.AppendLine(
                    $"{indent}{prefix}{childDirectory.Name}");

                var nextIndent = indent +
                    (isLast ? "    " : "│   ");

                RenderDirectory(
                    childDirectory,
                    nextIndent,
                    builder);
            }

            if (item is FileNode file)
            {
                builder.AppendLine(
                    $"{indent}{prefix}{file.Name}");
            }
        }
    }
}