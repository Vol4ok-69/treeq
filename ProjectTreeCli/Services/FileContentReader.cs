using ProjectTreeCli.Configuration;
using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services;

public sealed class FileContentReader
{
    public void PrintContent(
        DirectoryNode root,
        OutputWriter writer)
    {
        PrintDirectory(
            root,
            root.FullPath,
            writer);
    }

    private void PrintDirectory(
        DirectoryNode directory,
        string rootPath,
        OutputWriter writer)
    {
        foreach (var childDirectory in directory.Directories)
        {
            PrintDirectory(
                childDirectory,
                rootPath,
                writer);
        }

        foreach (var file in directory.Files)
        {
            var extension = Path.GetExtension(file.FullPath);

            if (DefaultBinaryExtensions.Items.Contains(extension))
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(
                rootPath,
                file.FullPath);

            writer.WriteLine();
            writer.WriteLine(
                $"===== {relativePath} =====");

            writer.WriteLine();

            try
            {
                var content = File.ReadAllText(file.FullPath);

                writer.WriteLine(content);
            }
            catch (Exception ex)
            {
                writer.WriteLine(
                    $"[ERROR] {ex.Message}");
            }
        }
    }
}