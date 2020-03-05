﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Patches;
using GitUI.Editor;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI
{
    public static class GitUIExtensions
    {
        [CanBeNull]
        private static Patch GetItemPatch(
            [NotNull] GitModule module,
            [NotNull] GitItemStatus file,
            [CanBeNull] ObjectId firstRevision,
            [CanBeNull] ObjectId secondRevision,
            [NotNull] string diffArgs,
            [NotNull] Encoding encoding)
        {
            // Files with tree guid should be presented with normal diff
            var isTracked = file.IsTracked || (file.TreeGuid != null && secondRevision != null);

            return module.GetSingleDiff(firstRevision?.ToString(), secondRevision?.ToString(), file.Name, file.OldName, diffArgs, encoding, true, isTracked);
        }

        [CanBeNull]
        private static string GetSelectedPatch(
            [NotNull] this FileViewer diffViewer,
            [CanBeNull] ObjectId firstRevision,
            [CanBeNull] ObjectId secondRevision,
            [NotNull] GitItemStatus file)
        {
            if (!file.IsTracked)
            {
                var fullPath = Path.Combine(diffViewer.Module.WorkingDir, file.Name);
                if (Directory.Exists(fullPath) && GitModule.IsValidGitWorkingDir(fullPath))
                {
                    // git-status does not detect details for untracked and git-diff --no-index will not give info
                    return LocalizationHelpers.GetSubmoduleText(diffViewer.Module, file.Name.TrimEnd('/'), "");
                }
            }

            if (file.IsSubmodule && file.GetSubmoduleStatusAsync() != null)
            {
                return LocalizationHelpers.ProcessSubmoduleStatus(diffViewer.Module, ThreadHelper.JoinableTaskFactory.Run(() => file.GetSubmoduleStatusAsync()));
            }

            Patch patch = GetItemPatch(diffViewer.Module, file, firstRevision, secondRevision,
                diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);

            if (patch == null)
            {
                return string.Empty;
            }

            if (file.IsSubmodule)
            {
                return LocalizationHelpers.ProcessSubmodulePatch(diffViewer.Module, file.Name, patch);
            }

            return patch.Text;
        }

        public static Task ViewChangesAsync(this FileViewer diffViewer,
            [CanBeNull] ObjectId firstId,
            [CanBeNull] GitRevision selectedRevision,
            [NotNull] GitItemStatus file,
            [NotNull] string defaultText,
            [CanBeNull] Action openWithDifftool = null)
        {
            if (firstId == null && selectedRevision != null)
            {
                firstId = selectedRevision.FirstParentGuid;
            }

            openWithDifftool = openWithDifftool ?? OpenWithDifftool;
            if (file.IsNew && selectedRevision?.ObjectId == ObjectId.WorkTreeId)
            {
                return diffViewer.ViewFileAsync(file.Name, openWithDifftool: openWithDifftool);
            }

            if (firstId == null || FileHelper.IsImage(file.Name))
            {
                // The previous commit does not exist, nothing to compare with
                if (file.TreeGuid != null)
                {
                    // blob guid exists
                    return diffViewer.ViewGitItemAsync(file, openWithDifftool);
                }

                if (selectedRevision == null)
                {
                    throw new ArgumentNullException(nameof(selectedRevision));
                }

                // Get blob guid from revision
                return diffViewer.ViewGitItemRevisionAsync(file, selectedRevision.ObjectId, openWithDifftool);
            }

            string selectedPatch = diffViewer.GetSelectedPatch(firstId, selectedRevision.ObjectId, file);
            if (selectedPatch == null)
            {
                return diffViewer.ViewPatchAsync(file.Name, text: defaultText,
                    openWithDifftool: null /* not applicable */, isText: true);
            }

            return diffViewer.ViewPatchAsync(file.Name, text: selectedPatch,
                openWithDifftool: openWithDifftool, isText: file.IsSubmodule);

            void OpenWithDifftool()
            {
                diffViewer.Module.OpenWithDifftool(
                    file.Name,
                    file.OldName,
                    firstId?.ToString(),
                    selectedRevision?.ToString(),
                    "",
                    file.IsTracked);
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
            if (FindMaskPanel(control) == null)
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
            if (panel != null)
            {
                control.Controls.Remove(panel);
                panel.Dispose();
            }
        }

        [CanBeNull]
        private static LoadingControl FindMaskPanel(Control control)
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
