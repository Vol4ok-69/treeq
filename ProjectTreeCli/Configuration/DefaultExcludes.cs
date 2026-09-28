namespace ProjectTreeCli.Configuration;

public static class DefaultExcludes
{
    public static readonly HashSet<string> Items =
    [
        ".git",
        ".idea",
        ".vs",
        ".vscode",
        ".gradle",
        "bin",
        "obj",
        "packages",
        "node_modules",
        "dist",
        "build",
        "coverage",
        ".next",
        ".nuxt",
        "target",
        "out",
        ".gradle-user",
        ".ps1",
        ".md",
        "docs",
        ".docx",
        ".drawio",

        "LICENSE",
        ".license"
    ];
}