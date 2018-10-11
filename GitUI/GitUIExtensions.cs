using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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

        public static Task ViewChangesAsync(this FileViewer diffViewer, IReadOnlyList<GitRevision> revisions, GitItemStatus file, string defaultText)
        {
            if (revisions.Count == 0)
            {
                return Task.CompletedTask;
            }

            var selectedRevision = revisions[0];
            var secondRevision = selectedRevision?.ObjectId;
            var firstRevision = revisions.Count >= 2 ? revisions[1].ObjectId : null;
            if (firstRevision == null && selectedRevision != null)
            {
                firstRevision = selectedRevision.FirstParentGuid;
            }

            return ViewChangesAsync(diffViewer, firstRevision, secondRevision, file, defaultText, openWithDifftool: null /* use default */);
        }

        public static Task ViewChangesAsync(
            this FileViewer diffViewer,
            [CanBeNull] ObjectId firstRevision,
            ObjectId secondRevision,
            [NotNull] GitItemStatus file,
            [NotNull] string defaultText,
            [CanBeNull] Action openWithDifftool)
        {
            if (firstRevision == null)
            {
                // The previous commit does not exist, nothing to compare with
                if (file.TreeGuid == null)
                {
                    return diffViewer.ViewGitItemAsync(file.Name, file.TreeGuid);
                }

                if (secondRevision == null)
                {
                    throw new ArgumentNullException(nameof(secondRevision));
                }

                return diffViewer.ViewGitItemRevisionAsync(file.Name, secondRevision);
            }

            return diffViewer.ViewPatchAsync(() =>
            {
                string selectedPatch = diffViewer.GetSelectedPatch(firstRevision, secondRevision, file);
                if (selectedPatch == null)
                {
                    return (text: defaultText, openWithDifftool: null /* not applicable */);
                }

                return (text: selectedPatch,
                    openWithDifftool: openWithDifftool ?? OpenWithDifftool);

                void OpenWithDifftool()
                {
                    diffViewer.Module.OpenWithDifftool(
                        file.Name,
                        null,
                        firstRevision.ToString(),
                        firstRevision.ToString(),
                        "",
                        file.IsTracked);
                }
            });
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

        public static async Task InvokeAsync(this Control control, Action action)
        {
            await control.SwitchToMainThreadAsync();
            action();
        }

        public static async Task InvokeAsync<T>(this Control control, Action<T> action, T state)
        {
            await control.SwitchToMainThreadAsync();
            action(state);
        }

#pragma warning disable VSTHRD100 // Avoid async void methods
        /// <summary>
        /// Use <see cref="InvokeAsync(Control, Action)"/> instead. If the result of
        /// <see cref="InvokeAsync(Control, Action)"/> is not awaited, use
        /// <see cref="ThreadHelper.FileAndForget(Task, Func{Exception, bool})"/> to ignore it.
        /// </summary>
        public static async void InvokeAsyncDoNotUseInNewCode(this Control control, Action action)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            if (ThreadHelper.JoinableTaskContext.IsOnMainThread)
            {
                await Task.Yield();
            }
            else
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }

            if (control.IsDisposed)
            {
                return;
            }

            action();
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
