using ProjectTreeCli.Helpers;
using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services;

public sealed class SummaryRenderer
{
    public void Render(
        ProjectSummary summary,
        OutputWriter writer)
    {
        writer.WriteLine();
        writer.WriteLine("Project Summary");
        writer.WriteLine("────────────────");
        writer.WriteLine();

        writer.WriteLine(
            $"Directories: {summary.DirectoryCount}");

        writer.WriteLine(
            $"Files: {summary.FileCount}");

        writer.WriteLine(
            $"Total Size: {SizeFormatter.Format(summary.TotalSize)}");

        writer.WriteLine();

        writer.WriteLine("Top Extensions");
        writer.WriteLine("──────────────");

        foreach (var extension in summary.Extensions
                     .OrderByDescending(x => x.Value)
                     .Take(10))
        {
            writer.WriteLine(
                $"{extension.Key}    {extension.Value}");
        }

        writer.WriteLine();

        writer.WriteLine("Largest Files");
        writer.WriteLine("─────────────");

        foreach (var file in summary.LargestFiles)
        {
            writer.WriteLine(
                $"{file.Name}    {SizeFormatter.Format(file.Size)}");
        }
    }
}