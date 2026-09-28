using ProjectTreeCli.Models;

namespace ProjectTreeCli.Services;

public sealed class FileScanner(ExcludeMatcher excludeMatcher)
{
    private readonly ExcludeMatcher _excludeMatcher = excludeMatcher;

    public DirectoryNode Scan(AppOptions options)
    {
        return ScanDirectory(options.RootPath, options, 0);
    }

    private DirectoryNode ScanDirectory(string path, AppOptions options, int depth)
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
            var relativeDirectory = Path.GetRelativePath(options.RootPath, directory);

            if (_excludeMatcher.IsExcluded(relativeDirectory, options.Excludes))
            {
                continue;
            }
            if (options.IgnoreHidden && IsHidden(directory))
            {
                continue;
            }
            var childDirectory = ScanDirectory(directory, options, depth + 1);

            directoryNode.Directories.Add(childDirectory);
            directoryNode.Size += childDirectory.Size;
        }

        foreach (var file in Directory.EnumerateFiles(path))
        {
            if (_excludeMatcher.IsExcluded(file, options.Excludes))
            {
                continue;
            }

            if (options.IgnoreHidden && IsHidden(file))
            {
                continue;
            }

            var extension = Path.GetExtension(file);

            if (options.OnlyExtensions.Count > 0 && !options.OnlyExtensions.Contains(extension))
            {
                continue;
            }

            var fileInfo = new FileInfo(file);

            var maxBytes = options.MaxFileSizeKb * 1024;

            if (fileInfo.Length > maxBytes)
            {
                continue;
            }



            directoryNode.Files.Add(new FileNode
            {
                Name = Path.GetFileName(file),
                FullPath = file,
                Size = fileInfo.Length
            });
            directoryNode.Size += fileInfo.Length;
        }

        return directoryNode;
    }

    private bool IsHidden(string path)
    {
        var name = Path.GetFileName(path);

        if (name.StartsWith('.'))
        {
            return true;
        }

        try
        {
            var attributes = File.GetAttributes(path);

            return attributes.HasFlag(
                FileAttributes.Hidden);
        }
        catch
        {
            return false;
        }
    }
}