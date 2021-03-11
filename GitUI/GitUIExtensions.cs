﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Patches;
using GitExtUtils;
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
        /// View the changes between the revisions, if possible as a diff
        /// </summary>
        /// <param name="fileViewer">Current FileViewer</param>
        /// <param name="item">The FileStatusItem to present changes for</param>
        /// <param name="defaultText">default text if no diff is possible</param>
        /// <param name="openWithDiffTool">The difftool command to open with</param>
        /// <returns>Task to view</returns>
        public static Task ViewChangesAsync(this FileViewer fileViewer,
            FileStatusItem? item,
            string defaultText = "",
            Action? openWithDiffTool = null)
        {
            if (item?.Item.IsStatusOnly ?? false)
            {
                // Present error (e.g. parsing Git)
                return fileViewer.ViewTextAsync(item.Item.Name, item.Item.ErrorMessage ?? "");
            }

            if (item?.Item is null || item.SecondRevision?.ObjectId is null)
            {
                if (!string.IsNullOrWhiteSpace(defaultText))
                {
                    return fileViewer.ViewTextAsync(item?.Item?.Name, defaultText);
                }

                fileViewer.Clear();
                return Task.CompletedTask;
            }

            var firstId = item.FirstRevision?.ObjectId ?? item.SecondRevision.FirstParentId;

            openWithDiffTool ??= OpenWithDiffTool;

            if (item.Item.IsNew || firstId is null || FileHelper.IsImage(item.Item.Name))
            {
                // View blob guid from revision, or file for worktree
                return fileViewer.ViewGitItemRevisionAsync(item.Item, item.SecondRevision.ObjectId, openWithDiffTool);
            }

            if (item.Item.IsRangeDiff)
            {
                // This command may take time, give an indication of what is going on
                // The sha are incorrect if baseA/baseB is set, to simplify the presentation
                fileViewer.ViewText("range-diff.sh", $"git range-diff {firstId}...{item.SecondRevision.ObjectId}");

                string output = fileViewer.Module.GetRangeDiff(
                        firstId,
                        item.SecondRevision.ObjectId,
                        item.BaseA,
                        item.BaseB,
                        fileViewer.GetExtraDiffArguments(isRangeDiff: true));

                // Try set highlighting from first found filename
                var match = new Regex(@"\n\s*(@@|##)\s+(?<file>[^#:\n]+)").Match(output ?? "");
                var filename = match.Groups["file"].Success ? match.Groups["file"].Value : item.Item.Name;

                return fileViewer.ViewRangeDiffAsync(filename, output ?? defaultText);
            }

            string selectedPatch = GetSelectedPatch(fileViewer, firstId, item.SecondRevision.ObjectId, item.Item)
                ?? defaultText;

            return item.Item.IsSubmodule
                ? fileViewer.ViewTextAsync(item.Item.Name, text: selectedPatch, openWithDifftool: openWithDiffTool)
                : fileViewer.ViewPatchAsync(item, text: selectedPatch, openWithDifftool: openWithDiffTool);

            void OpenWithDiffTool()
            {
                fileViewer.Module.OpenWithDifftool(
                    item.Item.Name,
                    item.Item.OldName,
                    firstId?.ToString(),
                    item.SecondRevision.ToString(),
                    isTracked: item.Item.IsTracked);
            }

            static string? GetSelectedPatch(
                FileViewer fileViewer,
                ObjectId firstId,
                ObjectId selectedId,
                GitItemStatus file)
            {
                if (firstId == ObjectId.CombinedDiffId)
                {
                    var diffOfConflict = fileViewer.Module.GetCombinedDiffContent(selectedId, file.Name,
                        fileViewer.GetExtraDiffArguments(), fileViewer.Encoding);

                    return Strings.IsNullOrWhiteSpace(diffOfConflict)
                        ? TranslatedStrings.UninterestingDiffOmitted
                        : diffOfConflict;
                }

                if (file.IsSubmodule
                    && file.GetSubmoduleStatusAsync() is Task<GitSubmoduleStatus> task)
                {
                    // Patch already evaluated
                    var status = ThreadHelper.JoinableTaskFactory.Run(() => task);
                    return status is not null
                        ? LocalizationHelpers.ProcessSubmoduleStatus(fileViewer.Module, status)
                        : $"Failed to get status for submodule \"{file.Name}\"";
                }

                var patch = GetItemPatch(fileViewer.Module, file, firstId, selectedId,
                    fileViewer.GetExtraDiffArguments(), fileViewer.Encoding);

                return file.IsSubmodule
                    ? LocalizationHelpers.ProcessSubmodulePatch(fileViewer.Module, file.Name, patch)
                    : patch?.Text;

                static Patch? GetItemPatch(
                    GitModule module,
                    GitItemStatus file,
                    ObjectId? firstId,
                    ObjectId? secondId,
                    string diffArgs,
                    Encoding encoding)
                {
                    // Files with tree guid should be presented with normal diff
                    var isTracked = file.IsTracked || (file.TreeGuid is not null && secondId is not null);

                    return module.GetSingleDiff(firstId, secondId, file.Name, file.OldName, diffArgs, encoding, true, isTracked);
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
                var panel = new LoadingControl
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
