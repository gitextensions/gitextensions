using System.Text.RegularExpressions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitCommands.Git;

public interface IGitTreeParser
{
    IEnumerable<GitItem> Parse(string? tree);

    /// <summary>
    /// Parse ls-files --stage
    /// Should only be used with Git lessthan 2.38.0 without --format support
    /// </summary>
    /// <param name="tree">String to parse</param>
    /// <returns>GitItems</returns>
    IEnumerable<GitItem> ParseLsFiles(string? tree);

    GitItem? ParseSingle(string? rawItem);

    /// <summary>
    /// "git ls-tree --format" default, optimized codepath if empty
    /// Set explicitly for "git ls-tree"
    /// -z required too
    /// </summary>
    string GitTreeFormat { get; }
}

public sealed partial class GitTreeParser : IGitTreeParser
{
    // $ git ls-tree HEAD
    // 100644 blob ff17eaee6b8952e5736637424bc4348a69c40227    .editorconfig
    // 100644 blob bf29d31ff93be092ce746849e8db0984d4a83231    .gitattributes
    // 040000 tree 7469e02057cf5e5eb86f1edc6061d6f816e20ff7    .github
    // 100644 blob 770e60816f1d6338c15af91e3ce5b7ccd38b0e3f    .gitignore
    // 040000 tree c8f09c5438fca1477e79814c99ffcec58e06a83c    GitExtensions
    // 160000 commit 6868f2b4a39fc894c44711c8903407da596acbf5  GitExtensionsDoc
    // 100644 blob 7e4eb9dc6a1531a6ee37d8efa6bf570e4bf61146    README.md
    // 100644 blob 5b0965cd097b8c48b66dd456337852640fa429c8    stylecop.json
    [GeneratedRegex(@"^(?<mode>\d{6}) (?<type>(blob|tree|commit)+) (?<objectid>[0-9a-f]{40})\t(?<name>.+)$", RegexOptions.ExplicitCapture)]
    private static partial Regex TreeLineRegex();

    // $ git ls-files --stage
    // 100644 07c4d877fa885b9ef1ea2c343fe237beaf7a087c 0       externals/Directory.Build.props
    // 100644 532e4f49ecac926e5ff3881ec9cd46a9d48b5ddd 0       externals/Directory.Build.targets
    // 160000 1b0386aea1acdd2ba258977bd79e40a0a7b95665 0       externals/Git.hub
    // 160000 be6183dc8f29079ce677b6834c56b05752828f23 0       externals/ICSharpCode.TextEditor
    // ignore the stage part
    [GeneratedRegex(@"^(?<mode>\d{6}) (?<objectid>[0-9a-f]{40}) (?:[0-9])\t(?<name>.+)$", RegexOptions.ExplicitCapture)]
    private static partial Regex LsFilesLineRegex();

    public string GitTreeFormat { get; } = "%(objectmode) %(objecttype) %(objectname)%x09%(path)";

    public IEnumerable<GitItem> Parse(string? tree)
    {
        if (string.IsNullOrWhiteSpace(tree))
        {
            return [];
        }

        return tree.LazySplit('\0').Select(ParseSingle).WhereNotNull();
    }

    public GitItem? ParseSingle(string? rawItem)
    {
        if (rawItem is null)
        {
            return null;
        }

        Match match = TreeLineRegex().Match(rawItem);

        if (!match.Success)
        {
            return null;
        }

        int mode = int.Parse(match.Groups["mode"].Value);
        string typeName = match.Groups["type"].Value;
        ObjectId objectId = ObjectId.Parse(rawItem, match.Groups["objectid"]);
        string name = match.Groups["name"].Value;

        Enum.TryParse(typeName, ignoreCase: true, out GitObjectType type);

        return new GitItem(mode, type, objectId, name);
    }

    public IEnumerable<GitItem> ParseLsFiles(string? tree)
    {
        if (string.IsNullOrWhiteSpace(tree))
        {
            return [];
        }

        return tree.LazySplit('\0').Select(ParseSingleLsFiles).WhereNotNull();
    }

    private GitItem? ParseSingleLsFiles(string? rawItem)
    {
        if (rawItem is null)
        {
            return null;
        }

        Match match = LsFilesLineRegex().Match(rawItem);

        if (!match.Success)
        {
            return null;
        }

        int mode = int.Parse(match.Groups["mode"].Value);
        ObjectId objectId = ObjectId.Parse(rawItem, match.Groups["objectid"]);
        string name = match.Groups["name"].Value;

        GitObjectType type = mode == 160000
            ? GitObjectType.Commit
            : mode == 040000
                ? GitObjectType.Tree
                : GitObjectType.Blob;

        return new GitItem(mode, type, objectId, name);
    }
}
