using GitExtensions.Extensibility.Git;
using GitUIPluginInterfaces;

namespace GitUI;

partial class FileStatusList
{
    /// <summary>
    ///  The expanded state of the folder and group nodes of the last unfiltered tree, keyed by their path in the tree.
    ///  It is reapplied when the list is refreshed, so that folders collapsed by the user stay collapsed.
    ///  Not persisted.
    /// </summary>
    private Dictionary<string, bool> _rememberedExpansionStates = [];
    private string? _rememberedExpansionStatesWorkingDir;
    private bool _isExpansionStateOfDisplayedNodesRemembered;

    private bool IsExpansionStateRemembered => _filter is null && !FindInCommitFilesGitGrepActive;

    /// <summary>
    ///  Stores the expanded state of the currently displayed nodes before they are replaced.
    /// </summary>
    /// <remarks>
    ///  The state of a filtered tree is not stored, so that the state before filtering is restored when the filter is cleared.
    ///  An empty tree does not replace the stored state.
    /// </remarks>
    private void RememberExpansionStates()
    {
        if (!_isExpansionStateOfDisplayedNodesRemembered || FileStatusListView.Nodes.Count == 0)
        {
            return;
        }

        Dictionary<string, bool> expansionStates = [];
        AddExpansionStates(FileStatusListView.Nodes, parentKey: "");
        _rememberedExpansionStates = expansionStates;
        _rememberedExpansionStatesWorkingDir = GetWorkingDir();

        return;

        void AddExpansionStates(TreeNodeCollection nodes, string parentKey)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Nodes.Count > 0 && GetExpansionKey(node, parentKey) is string key)
                {
                    expansionStates[key] = node.IsExpanded;
                    AddExpansionStates(node.Nodes, key);
                }
            }
        }
    }

    /// <summary>
    ///  Reapplies the stored expanded state to the nodes which were displayed before, too.
    ///  Other nodes keep their default state.
    /// </summary>
    private void RestoreExpansionStates()
    {
        _isExpansionStateOfDisplayedNodesRemembered = IsExpansionStateRemembered;
        if (!_isExpansionStateOfDisplayedNodesRemembered || _rememberedExpansionStates.Count == 0)
        {
            return;
        }

        if (_rememberedExpansionStatesWorkingDir != GetWorkingDir())
        {
            _rememberedExpansionStates = [];
            return;
        }

        RestoreExpansionStates(FileStatusListView.Nodes, parentKey: "");

        return;

        void RestoreExpansionStates(TreeNodeCollection nodes, string parentKey)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Nodes.Count == 0 || GetExpansionKey(node, parentKey) is not string key)
                {
                    continue;
                }

                if (_rememberedExpansionStates.TryGetValue(key, out bool isExpanded) && isExpanded != node.IsExpanded)
                {
                    if (isExpanded)
                    {
                        // Replaces a possible placeholder with the actual children via BeforeExpand
                        node.Expand();
                    }
                    else
                    {
                        node.Collapse(ignoreChildren: true);
                    }
                }

                RestoreExpansionStates(node.Nodes, key);
            }
        }
    }

    private string? GetWorkingDir() => TryGetUICommandsDirect(out IGitUICommands? commands) ? commands.Module.WorkingDir : null;

    private static string? GetExpansionKey(TreeNode node, string parentKey)
    {
        string? key = node.Tag switch
        {
            RelativePath path => $"/{path.Value}",
            GroupKey groupKey => $":{groupKey.Value}",
            GitRevision revision => $"@{revision.ObjectId}",
            null => "@",
            _ => null
        };

        return key is null ? null : $"{parentKey}|{key}";
    }
}
