using System.Text;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI.UserControls;

internal static class DiffProvider
{
    /// <summary>
    ///  Append unified diffs for the items, one git diff call per unique revision pair.
    /// </summary>
    /// <param name="sb">The builder to append the diffs to.</param>
    /// <param name="items">The file status items to diff.</param>
    /// <param name="module">The module to run git in.</param>
    /// <param name="contextLines">The number of context lines for the diff.</param>
    public static async Task AppendDiffsAsync(StringBuilder sb, IReadOnlyList<FileStatusItem> items, IGitModule module, int contextLines)
    {
        // Group by revision pair so each unique context is one git diff call.
        foreach (IGrouping<(GitRevision? First, GitRevision Second), FileStatusItem> group in items.GroupBy(item => (item.FirstRevision, item.SecondRevision)))
        {
            // CombinedDiff artificial commits cannot be diffed explicitly.
            if (group.Key.First?.Guid == GitRevision.CombinedDiffGuid || group.Key.Second.Guid == GitRevision.CombinedDiffGuid)
            {
                continue;
            }

            // Untracked files are read directly from the filesystem and formatted as a
            // synthetic unified diff so no git process is needed per file.
            foreach (GitItemStatus item in group.Select(item => item.Item).Where(item => !item.IsTracked))
            {
                FileInfo fileInfo = new(Path.Combine(module.WorkingDir, item.Name));

                if (fileInfo.LinkTarget is { } linkTarget)
                {
                    sb.AppendLine($"{item.Name} -> {linkTarget}");
                    continue;
                }

                if (!fileInfo.Exists)
                {
                    continue;
                }

                string[] lines = await File.ReadAllLinesAsync(fileInfo.FullName);
                sb.AppendLine("--- /dev/null");
                sb.AppendLine($"+++ b/{item.Name}");
                sb.AppendLine($"@@ -0,0 +1,{lines.Length} @@");
                foreach (string line in lines)
                {
                    sb.Append('+').AppendLine(line);
                }
            }

            List<GitItemStatus> trackedItems = [.. group.Select(item => item.Item).Where(item => item.IsTracked)];
            if (trackedItems.Count == 0)
            {
                continue;
            }

            ArgumentString args = Commands.GetDiffs(group.Key.First, group.Key.Second, trackedItems, contextLines);
            string diff = await module.GitExecutable.GetOutputAsync(args);
            sb.Append(diff);
        }
    }
}
