using ProjectTreeCli.Models;
using System.Xml.Linq;

namespace ProjectTreeCli.Services;

public sealed class ProjectReferenceService
{
    public List<ProjectReferenceInfo> GetReferences(string rootPath)
    {
        var projectFiles = Directory
            .EnumerateFiles(
                rootPath,
                "*.*proj",
                SearchOption.AllDirectories)
            .Where(IsSupportedProject)
            .ToList();

        var result = new List<ProjectReferenceInfo>();

        foreach (var projectFile in projectFiles)
        {
            var references = GetProjectReferences(projectFile);

            if (references.Count == 0)
            {
                continue;
            }

            result.Add(new ProjectReferenceInfo
            {
                Project = Path.GetFileNameWithoutExtension(projectFile),
                References = references
            });
        }

        return result;
    }

    private List<string> GetProjectReferences(string projectFile)
    {
        var document = XDocument.Load(projectFile);

        return document
            .Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => ResolveProjectName(projectFile, path!))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToList();
    }

    private string ResolveProjectName(
        string projectFile,
        string referencePath)
    {
        var fullPath = Path.GetFullPath(
            Path.Combine(
                Path.GetDirectoryName(projectFile)!,
                referencePath));

        return Path.GetFileNameWithoutExtension(fullPath);
    }

    private bool IsSupportedProject(string path)
    {
        var extension = Path.GetExtension(path);

        return extension.Equals(
                   ".csproj",
                   StringComparison.OrdinalIgnoreCase)
               || extension.Equals(
                   ".fsproj",
                   StringComparison.OrdinalIgnoreCase)
               || extension.Equals(
                   ".vbproj",
                   StringComparison.OrdinalIgnoreCase);
    }
}