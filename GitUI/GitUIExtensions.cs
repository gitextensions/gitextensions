using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Patches;
using GitUI.Editor;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI
{
    public static class GitUIExtensions
    {
        /// <summary>
        /// View the changes between the revisions, if possible as a diff.
        /// </summary>
        /// <param name="fileViewer">Current FileViewer.</param>
        /// <param name="item">The FileStatusItem to present changes for.</param>
        /// <param name="line">The line to display.</param>
        /// <param name="defaultText">default text if no diff is possible.</param>
        /// <param name="openWithDiffTool">The difftool command to open with.</param>
        /// <param name="additionalCommandInfo">If the diff is range-diff, this contains the current path filter.</param>
        /// <returns>Task to view.</returns>
        public static async Task ViewChangesAsync(this FileViewer fileViewer,
            FileStatusItem? item,
            CancellationToken cancellationToken,
            int? line = null,
            string defaultText = "",
            Action? openWithDiffTool = null,
            string additionalCommandInfo = null)
        {
            if (item?.Item.IsStatusOnly ?? false)
            {
                // Present error (e.g. parsing Git)
                await fileViewer.ViewTextAsync(item.Item.Name, item.Item.ErrorMessage ?? "");
                return;
            }

            if (item?.Item is null || item.SecondRevision?.ObjectId is null)
            {
                if (!string.IsNullOrWhiteSpace(defaultText))
                {
                    await fileViewer.ViewTextAsync(item?.Item?.Name, defaultText);
                    return;
                }

                await fileViewer.ClearAsync();
                return;
            }

            var firstId = item.FirstRevision?.ObjectId ?? item.SecondRevision.FirstParentId;

            openWithDiffTool ??= OpenWithDiffTool;

            if (item.Item.IsNew || firstId is null || (!item.Item.IsDeleted && FileHelper.IsImage(item.Item.Name)))
            {
                // View blob guid from revision, or file for worktree
                await fileViewer.ViewGitItemAsync(item, line, openWithDiffTool);
                return;
            }

            if (item.Item.IsRangeDiff)
            {
                // Git range-diff has cubic runtime complexity and can be slow and memory consuming,
                // give an indication of what is going on
                string range = item.BaseA is null || item.BaseB is null
                    ? $"{firstId}...{item.SecondRevision.ObjectId}"
                    : $"{item.BaseA}..{firstId} {item.BaseB}..{item.SecondRevision.ObjectId}";
                await fileViewer.ViewTextAsync("git-range-diff.sh", $"git range-diff {range} -- {additionalCommandInfo}");

                string output = await fileViewer.Module.GetRangeDiffAsync(
                        firstId,
                        item.SecondRevision.ObjectId,
                        item.BaseA,
                        item.BaseB,
                        fileViewer.GetExtraDiffArguments(isRangeDiff: true),
                        additionalCommandInfo,
                        cancellationToken);

                // Try set highlighting from first found filename
                Match match = new Regex(@"\n\s*(@@|##)\s+(?<file>[^#:\n]+)").Match(output ?? "");
                string filename = match.Groups["file"].Success ? match.Groups["file"].Value : item.Item.Name;

                cancellationToken.ThrowIfCancellationRequested();

                await fileViewer.ViewRangeDiffAsync(filename, output ?? defaultText);
                return;
            }

            string selectedPatch = (await GetSelectedPatchAsync(fileViewer, firstId, item.SecondRevision.ObjectId, item.Item, cancellationToken))
                ?? defaultText;

            cancellationToken.ThrowIfCancellationRequested();

            if (item.Item.IsSubmodule)
            {
                await fileViewer.ViewTextAsync(item.Item.Name, text: selectedPatch, openWithDifftool: openWithDiffTool);
            }
            else
            {
                await fileViewer.ViewPatchAsync(item, text: selectedPatch, line: line, openWithDifftool: openWithDiffTool);
            }

            return;

            void OpenWithDiffTool()
            {
                fileViewer.Module.OpenWithDifftool(
                    item.Item.Name,
                    item.Item.OldName,
                    firstId?.ToString(),
                    item.SecondRevision.ToString(),
                    isTracked: item.Item.IsTracked);
            }

            static async Task<string?> GetSelectedPatchAsync(
                FileViewer fileViewer,
                ObjectId firstId,
                ObjectId selectedId,
                GitItemStatus file,
                CancellationToken cancellationToken)
            {
                if (firstId == ObjectId.CombinedDiffId)
                {
                    var diffOfConflict = fileViewer.Module.GetCombinedDiffContent(selectedId, file.Name,
                        fileViewer.GetExtraDiffArguments(), fileViewer.Encoding);

                    cancellationToken.ThrowIfCancellationRequested();
                    return string.IsNullOrWhiteSpace(diffOfConflict)
                        ? TranslatedStrings.UninterestingDiffOmitted
                        : diffOfConflict;
                }

                var task = file.GetSubmoduleStatusAsync();

                if (file.IsSubmodule && task is not null)
                {
                    // Patch already evaluated
                    var status = await task;

                    cancellationToken.ThrowIfCancellationRequested();
                    return status is not null
                        ? LocalizationHelpers.ProcessSubmoduleStatus(fileViewer.Module, status)
                        : $"Failed to get status for submodule \"{file.Name}\"";
                }

                var patch = await GetItemPatchAsync(fileViewer.Module, file, firstId, selectedId,
                    fileViewer.GetExtraDiffArguments(), fileViewer.Encoding);

                cancellationToken.ThrowIfCancellationRequested();
                return file.IsSubmodule
                    ? LocalizationHelpers.ProcessSubmodulePatch(fileViewer.Module, file.Name, patch)
                    : patch?.Text;

                static async Task<Patch?> GetItemPatchAsync(
                    GitModule module,
                    GitItemStatus file,
                    ObjectId? firstId,
                    ObjectId? secondId,
                    string diffArgs,
                    Encoding encoding)
                {
                    // Files with tree guid should be presented with normal diff
                    var isTracked = file.IsTracked || (file.TreeGuid is not null && secondId is not null);

                    return await module.GetSingleDiffAsync(firstId, secondId, file.Name, file.OldName, diffArgs, encoding, true, isTracked);
                }
            }
        }

        public static void RemoveIfExists(this TabControl tabControl, TabPage page)
        {
            if (tabControl.TabPages.Contains(page))
            {
                tabControl.TabPages.Remove(page);
            }
        }

        public static void InsertIfNotExists(this TabControl tabControl, int index, TabPage page)
        {
            if (!tabControl.TabPages.Contains(page))
            {
                tabControl.TabPages.Insert(index, page);
            }
        }

        public static void Mask(this Control control)
        {
            if (FindMaskPanel(control) is null)
            {
                LoadingControl panel = new()
                {
                    Dock = DockStyle.Fill,
                    IsAnimating = true,
                    BackColor = SystemColors.AppWorkspace
                };
                control.Controls.Add(panel);
                panel.BringToFront();
            }
        }

        public static void UnMask(this Control control)
        {
            var panel = FindMaskPanel(control);
            if (panel is not null)
            {
                control.Controls.Remove(panel);
                panel.Dispose();
            }
        }

        private static LoadingControl? FindMaskPanel(Control control)
        {
            return control.Controls.Cast<Control>().OfType<LoadingControl>().FirstOrDefault();
        }

        public static IEnumerable<TreeNode> AllNodes(this TreeView tree)
        {
            return tree.Nodes.AllNodes();
        }

        private static IEnumerable<TreeNode> AllNodes(this TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;

                foreach (TreeNode subNode in node.Nodes.AllNodes())
                {
                    yield return subNode;
                }
            }
        }

        public static async Task InvokeAsync(this Control control, Action action, CancellationToken token = default)
        {
            await control.SwitchToMainThreadAsync(token);
            action();
        }

        public static async Task InvokeAsync<T>(this Control control, Action<T> action, T state, CancellationToken token = default)
        {
            await control.SwitchToMainThreadAsync(token);
            action(state);
        }

        public static void InvokeSync(this Control control, Action action)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                async () =>
                {
                    try
                    {
                        await InvokeAsync(control, action);
                    }
                    catch (Exception e)
                    {
                        e.Data["StackTrace" + e.Data.Count] = e.StackTrace;
                        throw;
                    }
                });
        }

        public static Control FindFocusedControl(this ContainerControl container)
        {
            while (true)
            {
                if (container.ActiveControl is ContainerControl activeContainer)
                {
                    container = activeContainer;
                }
                else
                {
                    return container.ActiveControl;
                }
            }
        }
    }
}
