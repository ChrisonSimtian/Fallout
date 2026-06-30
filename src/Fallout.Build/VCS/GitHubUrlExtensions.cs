using System.IO;
using Fallout.Common.Utilities;
using static Fallout.Common.IO.PathConstruction;

namespace Fallout.Common.Git;

public enum GitHubItemType
{
    Automatic,
    File,
    Directory
}

/// <summary>
/// Pure URL/identity helpers over a <see cref="GitRepository"/> for GitHub-hosted repositories.
/// These are Git-domain string helpers (no GitHub API access) and live next to
/// <see cref="GitRepository"/>; the Octokit-backed client lives in the GitHub tool.
/// </summary>
public static class GitHubUrlExtensions
{
    public static bool IsGitHubRepository(this GitRepository repository)
    {
        return repository?.Endpoint?.EqualsOrdinalIgnoreCase("github.com") ?? false;
    }

    public static string GetGitHubOwner(this GitRepository repository)
    {
        Assert.True(repository.IsGitHubRepository());
        return repository.Identifier.Split('/')[0];
    }

    public static string GetGitHubName(this GitRepository repository)
    {
        Assert.True(repository.IsGitHubRepository());
        return repository.Identifier.Split('/')[1];
    }

    public static string GetGitHubCompareCommitsUrl(this GitRepository repository, string startCommitSha, string endCommitSha)
    {
        Assert.True(repository.IsGitHubRepository());
        return $"https://github.com/{repository.Identifier}/compare/{endCommitSha}^...{startCommitSha}";
    }

    public static string GetGitHubCompareTagToHeadUrl(this GitRepository repository, string tag)
    {
        Assert.True(repository.IsGitHubRepository());
        return $"https://github.com/{repository.Identifier}/compare/{tag}...HEAD";
    }

    public static string GetGitHubCompareTagsUrl(this GitRepository repository, string startTag, string endTag)
    {
        Assert.True(repository.IsGitHubRepository());
        return $"https://github.com/{repository.Identifier}/compare/{endTag}...{startTag}";
    }

    public static string GetGitHubCommitUrl(this GitRepository repository, string commitSha)
    {
        Assert.True(repository.IsGitHubRepository());
        return $"https://github.com/{repository.Identifier}/commit/{commitSha}";
    }

    /// <summary>Url in the form of <c>https://raw.githubusercontent.com/{identifier}/{branch}/{file}</c>.</summary>
    public static string GetGitHubDownloadUrl(this GitRepository repository, string file, string branch = null)
    {
        Assert.True(repository.IsGitHubRepository());

        branch ??= repository.Branch.NotNull("repository.Branch != null");
        var relativePath = GetRepositoryRelativePath(file, repository);
        return $"https://raw.githubusercontent.com/{repository.Identifier}/{branch}/{relativePath}";
    }

    /// <summary>
    /// Url in the form of <c>https://github.com/{identifier}/tree/{branch}/directory</c> or
    /// <c>https://github.com/{identifier}/blob/{branch}/file</c> depending on the item type.
    /// </summary>
    public static string GetGitHubBrowseUrl(
        this GitRepository repository,
        string path = null,
        string branch = null,
        GitHubItemType itemType = GitHubItemType.Automatic)
    {
        Assert.True(repository.IsGitHubRepository());

        branch ??= repository.Branch.NotNull();
        var relativePath = GetRepositoryRelativePath(path, repository);
        var method = GetMethod(relativePath, itemType, repository);
        Assert.True(path == null || method != null, "Could not determine item type");

        return $"https://github.com/{repository.Identifier}/{method}/{branch}/{relativePath}".TrimEnd("/");
    }

    private static string GetMethod(string relativePath, GitHubItemType itemType, GitRepository repository)
    {
        var absolutePath = repository.LocalDirectory != null && relativePath != null
            ? NormalizePath(Path.Combine(repository.LocalDirectory, relativePath))
            : null;

        if (itemType == GitHubItemType.Directory || Directory.Exists(absolutePath) || relativePath == null)
            return "tree";

        if (itemType == GitHubItemType.File || File.Exists(absolutePath))
            return "blob";

        return null;
    }

    private static string GetRepositoryRelativePath(string path, GitRepository repository)
    {
        if (path == null)
            return null;

        if (!Path.IsPathRooted(path))
            return path;

        var localDirectory = repository.LocalDirectory.NotNull();
        Assert.True(IsDescendantPath(localDirectory, path), $"Path {path.SingleQuote()} must be descendant of {localDirectory:s}");
        return GetRelativePath(localDirectory, path).Replace(oldChar: '\\', newChar: '/');
    }
}
