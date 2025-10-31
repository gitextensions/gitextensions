#nullable enable

using System.Text.RegularExpressions;
using GitCommands.Git;
using GitExtensions.Extensibility;

namespace GitCommands.UserRepositoryHistory;

public interface IRepositoryDescriptionProvider
{
    /// <summary>
    /// Returns a short name for repository.
    /// If the repository contains a description it is returned,
    /// otherwise the last part of path is returned.
    /// </summary>
    /// <param name="repositoryDir">Path to repository.</param>
    /// <returns>Short name for repository.</returns>
    string Get(string repositoryDir, Func<string, bool>? isValidGitWorkingDir = default);

    /// <summary>
    ///  Returns a short name for the repository followed by a unique name.
    /// </summary>
    /// <param name="repositoryDir">Path to repository.</param>
    string GetDescriptiveUnique(string repositoryDir);
}

/// <summary>
///  Provides the ability to read .git/description file.
/// </summary>
/// <remarks>
///  https://git-scm.com/book/en/v2/Git-Internals-Plumbing-and-Porcelain:
///  ...The description file is used only by the GitWeb program, so don’t worry about it...
/// </remarks>
public sealed class RepositoryDescriptionProvider : IRepositoryDescriptionProvider
{
    private const string _repositoryDescriptionFileName = "description";
    private const string _defaultDescription = "Unnamed repository; edit this file 'description' to name the repository.";

    private readonly Regex _uninformativeNameRegex = new($"^({AppSettings.UninformativeRepoNameRegex.Value})$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

    private readonly IGitDirectoryResolver _gitDirectoryResolver;

    public RepositoryDescriptionProvider(IGitDirectoryResolver gitDirectoryResolver)
    {
        _gitDirectoryResolver = gitDirectoryResolver;
    }

    /// <summary>
    /// Returns a short name for repository.
    /// If the repository contains a description it is returned,
    /// otherwise the last part of path is returned.
    /// </summary>
    /// <param name="repositoryDir">Path to repository.</param>
    /// <returns>Short name for repository.</returns>
    public string Get(string repositoryDir, Func<string, bool>? isValidGitWorkingDir)
    {
        isValidGitWorkingDir ??= GitModule.IsValidGitWorkingDir;

        DirectoryInfo repositoryDirInfo = new(repositoryDir);
        DirectoryInfo rootRepositoryDirInfo = GetRootRepoDirInfo(repositoryDirInfo);
        string repositoryDescription = GetShortName(repositoryDirInfo);
        return repositoryDirInfo != rootRepositoryDirInfo
            ? $"{repositoryDescription} < {GetShortName(rootRepositoryDirInfo)}"
            : repositoryDescription;

        DirectoryInfo GetRootRepoDirInfo(DirectoryInfo repositoryDirInfo)
        {
            DirectoryInfo rootRepoDirInfo = repositoryDirInfo;
            for (DirectoryInfo? parentDirInfo = repositoryDirInfo.Parent; parentDirInfo?.Exists is true; parentDirInfo = parentDirInfo?.Parent)
            {
                if (parentDirInfo is not null && isValidGitWorkingDir(parentDirInfo.FullName))
                {
                    rootRepoDirInfo = parentDirInfo;
                }
            }

            return rootRepoDirInfo;
        }

        string GetShortName(DirectoryInfo dirInfo)
        {
            if (!dirInfo.Exists)
            {
                return dirInfo.Name;
            }

            string? desc = ReadRepositoryDescription(dirInfo.FullName);
            if (!string.IsNullOrWhiteSpace(desc))
            {
                return desc;
            }

            while (dirInfo != rootRepositoryDirInfo && dirInfo.Parent is not null && _uninformativeNameRegex.IsMatch(dirInfo.Name) && !isValidGitWorkingDir(dirInfo.Parent.FullName))
            {
                dirInfo = dirInfo.Parent;
            }

            return dirInfo.Name;
        }
    }

    public string GetDescriptiveUnique(string repositoryDir)
    {
        const int maxDescriptiveLength = 25;
        string descriptive = Get(repositoryDir, isValidGitWorkingDir: default);
        int endOfLineIndex = descriptive.IndexOfAny(Delimiters.LineFeedAndCarriageReturnAndNull);
        int descriptiveEnd = endOfLineIndex >= 0 ? endOfLineIndex : descriptive.Length;
        descriptiveEnd = Math.Min(descriptiveEnd, maxDescriptiveLength);
        ReadOnlySpan<char> shortName = descriptive.AsSpan(0, descriptiveEnd).Trim();

        string unique = repositoryDir.TrimEnd(Path.DirectorySeparatorChar);
        return shortName.Length == 0 ? unique : $"{shortName} ({unique})";
    }

    /// <summary>
    /// Reads repository description's first line from ".git\description" file.
    /// </summary>
    /// <param name="workingDir">Path to repository.</param>
    /// <returns>If the repository has description, returns that description, else returns <c>null</c>.</returns>
    private string? ReadRepositoryDescription(string workingDir)
    {
        string gitDir = _gitDirectoryResolver.Resolve(workingDir);
        string descriptionFilePath = Path.Combine(gitDir, _repositoryDescriptionFileName);

        if (!File.Exists(descriptionFilePath))
        {
            return null;
        }

        try
        {
            string? repositoryDescription = File.ReadLines(descriptionFilePath).FirstOrDefault();
            return string.Equals(repositoryDescription, _defaultDescription, StringComparison.CurrentCulture)
                ? null
                : repositoryDescription;
        }
        catch (IOException)
        {
            return null;
        }
    }
}
