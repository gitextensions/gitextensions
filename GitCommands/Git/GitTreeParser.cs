using System.Collections.Generic;
using System.Linq;
using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public interface IGitTreeParser
    {
        IEnumerable<IGitItem> Parse(string tree);
        GitItem ParseSingle(string rawItem);
    }

    public sealed class GitTreeParser : IGitTreeParser
    {
        internal static readonly int MinimumStringLength = 53;


        public IEnumerable<IGitItem> Parse(string tree)
        {
            if (string.IsNullOrWhiteSpace(tree))
            {
                return Enumerable.Empty<IGitItem>();
            }
            var items = tree.Split('\0', '\n');
            return items.Select(ParseSingle).Where(item => item != null);
        }

        public GitItem ParseSingle(string rawItem)
        {
            if (rawItem == null || rawItem.Length <= MinimumStringLength)
            {
                return null;
            }

            var guidStart = rawItem.IndexOf(' ', 7);
            var mode = rawItem.Substring(0, 6);
            var itemType = rawItem.Substring(7, guidStart - 7);
            var guid = rawItem.Substring(guidStart + 1, 40);
            var name = rawItem.Substring(guidStart + 42).Trim();

            var item = new GitItem(mode, itemType, guid, name);
            return item;
        }
    }
}