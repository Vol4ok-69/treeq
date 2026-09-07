using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services;

public sealed class ProjectSummaryService
{
    public ProjectSummary Build(DirectoryNode root)
    {
        var summary = new ProjectSummary();

        ProcessDirectory(root, summary);

        summary.LargestFiles = [.. summary
            .LargestFiles
            .OrderByDescending(x => x.Size)
            .Take(10)];

        return summary;
    }

    private void ProcessDirectory(
        DirectoryNode directory,
        ProjectSummary summary)
    {
        summary.DirectoryCount++;

        summary.TotalSize += directory.Size;

        foreach (var file in directory.Files)
        {
            summary.FileCount++;

            summary.LargestFiles.Add(file);

            var extension = Path.GetExtension(file.Name);

            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = "[no extension]";
            }

            if (!summary.Extensions.TryGetValue(extension, out int value))
            {
                value = 0;
                summary.Extensions[extension] = value;
            }

            summary.Extensions[extension] = ++value;
        }

        foreach (var childDirectory in directory.Directories)
        {
            ProcessDirectory(
                childDirectory,
                summary);
        }
    }
}