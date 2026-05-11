using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services;

public sealed class FileScanner(ExcludeMatcher excludeMatcher)
{
    private readonly ExcludeMatcher _excludeMatcher = excludeMatcher;

    public DirectoryNode Scan(AppOptions options)
    {
        return ScanDirectory(
            options.RootPath,
            options,
            0);
    }

    private DirectoryNode ScanDirectory(
        string path,
        AppOptions options,
        int depth)
    {
        var directoryNode = new DirectoryNode
        {
            Name = Path.GetFileName(path),
            FullPath = path
        };

        if (depth > options.Depth)
        {
            return directoryNode;
        }

        foreach (var directory in Directory.EnumerateDirectories(path))
        {
            if (_excludeMatcher.IsExcluded(directory, options.Excludes))
            {
                continue;
            }

            var childDirectory = ScanDirectory(
                directory,
                options,
                depth + 1);

            directoryNode.Directories.Add(childDirectory);
        }

        foreach (var file in Directory.EnumerateFiles(path))
        {
            if (_excludeMatcher.IsExcluded(file, options.Excludes))
            {
                continue;
            }

            var extension = Path.GetExtension(file);

            if (options.OnlyExtensions.Count > 0 &&
                !options.OnlyExtensions.Contains(extension))
            {
                continue;
            }

            var fileInfo = new FileInfo(file);

            directoryNode.Files.Add(new FileNode
            {
                Name = Path.GetFileName(file),
                FullPath = file,
                Size = fileInfo.Length
            });
        }

        return directoryNode;
    }
}