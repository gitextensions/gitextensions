using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitCommands.Git
{
    public interface IGitTreeParser
    {
        [NotNull]
        [ItemNotNull]
        IEnumerable<GitItem> Parse([CanBeNull] string tree);

        [CanBeNull]
        GitItem ParseSingle([CanBeNull] string rawItem);
    }

    public sealed class GitTreeParser : IGitTreeParser
    {
        private static readonly Regex _treeLineRegex = new Regex(
            @"^(?<mode>\d{6}) (?<type>(blob|tree|commit)+) (?<objectid>[0-9a-f]{40})\s+(?<name>.+)$",
            RegexOptions.Compiled);

        public IEnumerable<GitItem> Parse(string tree)
        {
            if (string.IsNullOrWhiteSpace(tree))
            {
                return Enumerable.Empty<GitItem>();
            }

            // $ git ls-tree HEAD
            // 100644 blob ff17eaee6b8952e5736637424bc4348a69c40227    .editorconfig
            // 100644 blob bf29d31ff93be092ce746849e8db0984d4a83231    .gitattributes
            // 040000 tree 7469e02057cf5e5eb86f1edc6061d6f816e20ff7    .github
            // 100644 blob 770e60816f1d6338c15af91e3ce5b7ccd38b0e3f    .gitignore
            // 040000 tree c8f09c5438fca1477e79814c99ffcec58e06a83c    GitExtensions
            // 160000 commit 6868f2b4a39fc894c44711c8903407da596acbf5  GitExtensionsDoc
            // 100644 blob 7e4eb9dc6a1531a6ee37d8efa6bf570e4bf61146    README.md
            // 100644 blob 5b0965cd097b8c48b66dd456337852640fa429c8    stylecop.json

            // Split on \0 too, as GitModule.GetTree uses `ls-tree -z` which uses null terminators
            var items = tree.Split('\0', '\n');

            return items.Select(ParseSingle).Where(item => item != null);
        }

        public GitItem ParseSingle(string rawItem)
        {
            if (rawItem == null)
            {
                return null;
            }

            var match = _treeLineRegex.Match(rawItem);

            if (!match.Success)
            {
                return null;
            }

            var mode = int.Parse(match.Groups["mode"].Value);
            var typeName = match.Groups["type"].Value;
            var objectId = ObjectId.Parse(rawItem, match.Groups["objectid"]);
            var name = match.Groups["name"].Value;

            Enum.TryParse(typeName, ignoreCase: true, out GitObjectType type);

            return new GitItem(mode, type, objectId, name);
        }
    }
}