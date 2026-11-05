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
    private string? _displayedNodesWorkingDir;
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

        // Not the current working dir, which already is the new one if the repo has just been switched
        _rememberedExpansionStatesWorkingDir = _displayedNodesWorkingDir;

        return;

        void AddExpansionStates(TreeNodeCollection nodes, string parentKey)
        {
            foreach ((TreeNode node, string key, string childrenParentKey) in GetExpandableNodes(nodes, parentKey))
            {
                expansionStates[key] = node.IsExpanded;
                AddExpansionStates(node.Nodes, childrenParentKey);
            }
        }
    }

    /// <summary>
    ///  Reapplies the stored expanded state to the nodes which were displayed before, too.
    ///  Other nodes keep their default state.
    /// </summary>
    private void RestoreExpansionStates()
    {
        _displayedNodesWorkingDir = GetWorkingDir();
        _isExpansionStateOfDisplayedNodesRemembered = IsExpansionStateRemembered;
        if (!_isExpansionStateOfDisplayedNodesRemembered || _rememberedExpansionStates.Count == 0)
        {
            return;
        }

        if (_rememberedExpansionStatesWorkingDir != _displayedNodesWorkingDir)
        {
            _rememberedExpansionStates = [];
            return;
        }

        RestoreExpansionStates(FileStatusListView.Nodes, parentKey: "");

        return;

        void RestoreExpansionStates(TreeNodeCollection nodes, string parentKey)
        {
            foreach ((TreeNode node, string key, string childrenParentKey) in GetExpandableNodes(nodes, parentKey))
            {
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

                RestoreExpansionStates(node.Nodes, childrenParentKey);
            }
        }
    }

    private string? GetWorkingDir() => TryGetUICommandsDirect(out IGitUICommands? commands) ? commands.Module.WorkingDir : null;

    /// <summary>
    ///  Gets the nodes with children, their keys and the parent key for their children.
    ///  Siblings with the same tag, e.g. the diff groups "BASE to A" and "BASE to B", are distinguished by their order.
    ///  Folders are keyed by their full path within their group, i.e. independently of their parent folders,
    ///  because the compacted parent folders change if files are added or removed in sibling folders.
    /// </summary>
    private static IEnumerable<(TreeNode Node, string Key, string ChildrenParentKey)> GetExpandableNodes(TreeNodeCollection nodes, string parentKey)
    {
        Dictionary<string, int> occurrences = [];
        foreach (TreeNode node in nodes)
        {
            string? key = node.Nodes.Count == 0
                ? null
                : node.Tag switch
                {
                    RelativePath path => $"/{path.Value}",
                    GroupKey groupKey => $":{groupKey.Value}",
                    GitRevision revision => $"@{revision.ObjectId}",
                    null => "@",
                    _ => null
                };
            if (key is null)
            {
                continue;
            }

            int occurrence = occurrences.GetValueOrDefault(key);
            occurrences[key] = occurrence + 1;
            string nodeKey = occurrence == 0 ? $"{parentKey}|{key}" : $"{parentKey}|{key}#{occurrence}";
            yield return (node, nodeKey, node.Tag is RelativePath ? parentKey : nodeKey);
        }
    }
}
