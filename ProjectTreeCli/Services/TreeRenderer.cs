using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services;

public sealed class TreeRenderer
{
    public void Render(DirectoryNode root, OutputWriter writer)
    {
        writer.WriteLine(root.Name);

        RenderDirectory(root, string.Empty, writer);
    }

    private void RenderDirectory(DirectoryNode directory, string indent, OutputWriter writer)
    {
        var items = new List<object>();

        items.AddRange
        (
            directory.Directories.OrderBy(x => x.Name)
        );

        items.AddRange
        (
            directory.Files.OrderBy(x => x.Name)
        );

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            var isLast = i == items.Count - 1;

            var prefix = isLast ? "└── " : "├── ";

            if (item is DirectoryNode childDirectory)
            {
                writer.WriteLine($"{indent}{prefix}{childDirectory.Name}");

                var nextIndent = indent + (isLast ? "    " : "│   ");

                RenderDirectory(childDirectory, nextIndent, writer);
            }

            if (item is FileNode file)
            {
                writer.WriteLine($"{indent}{prefix}{file.Name}");
            }
        }
    }
}