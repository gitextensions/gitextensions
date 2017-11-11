using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor;
using ICSharpCode.TextEditor.Util;
using ResourceManager;

namespace GitUI
{
    public static class GitUIExtensions
    {

        public static SynchronizationContext UISynchronizationContext;

        public enum DiffWithRevisionKind
        {
            DiffAB,
            DiffALocal,
            DiffBLocal,
            DiffAParentLocal,
            DiffBParentLocal
        }

        public class DiffKindRevision
        {
            /// <summary>
            /// One row selected:
            /// B - Selected row
            /// A - B's parent
            ///
            /// Two rows selected:
            /// A - first selected row
            /// B - second selected row
            /// </summary>
            public static string Get(IList<GitRevision> revisions, DiffWithRevisionKind diffKind,
                out string extraDiffArgs, out string firstRevision, out string secondRevision)
            {
                //Note: Order in revisions is that first clicked is last in array
                string error = "";
                //Detect rename and copy
                extraDiffArgs = "-M -C";

                if (revisions == null)
                {
                    error = "Unexpected null revision argument to difftool";
                    firstRevision = null;
                    secondRevision = null;
                }
                else if (revisions.Count == 0 || revisions.Count > 2)
                {
                    error = "Unexpected number of arguments to difftool: " + revisions.Count;
                    firstRevision = null;
                    secondRevision = null;
                }
                else if (revisions[0] == null || revisions.Count > 1 && revisions[1] == null)
                {
                    error = "Unexpected single null argument to difftool";
                    firstRevision = null;
                    secondRevision = null;
                }
                else if (diffKind == DiffWithRevisionKind.DiffAB)
                {
                    if (revisions.Count == 1)
                    {
                        firstRevision = revisions[0].FirstParentGuid ?? revisions[0].Guid + '^';
                    }
                    else
                    {
                        firstRevision = revisions[1].Guid;
                    }
                    secondRevision = revisions[0].Guid;
                }
                else
                {
                    //Second revision is always local 
                    secondRevision = null;

                    if (diffKind == DiffWithRevisionKind.DiffBLocal)
                    {
                        firstRevision = revisions[0].Guid;
                    }
                    else if (diffKind == DiffWithRevisionKind.DiffBParentLocal)
                    {
                        firstRevision = revisions[0].FirstParentGuid ?? revisions[0].Guid + '^';
                    }
                    else
                    {
                        firstRevision = revisions[0].Guid;
                        if (revisions.Count == 1)
                        {
                            if (diffKind == DiffWithRevisionKind.DiffALocal)
                            {
                                firstRevision = revisions[0].FirstParentGuid ?? revisions[0].Guid + '^';
                            }
                            else if (diffKind == DiffWithRevisionKind.DiffAParentLocal)
                            {
                                firstRevision = (revisions[0].FirstParentGuid ?? revisions[0].Guid + '^') + "^";
                            }
                            else
                            {
                                error = "Unexpected arg to difftool with one revision: " + diffKind;
                            }
                        }
                        else
                        {
                            if (diffKind == DiffWithRevisionKind.DiffALocal)
                            {
                                firstRevision = revisions[1].Guid;
                            }
                            else if (diffKind == DiffWithRevisionKind.DiffAParentLocal)
                            {
                                firstRevision = revisions[1].FirstParentGuid ?? revisions[1].Guid + '^';
                            }
                            else
                            {
                                error = "Unexpected arg to difftool with two revisions: " + diffKind;
                            }
                        }
                    }
                }
                return error;
            }
        }

        public static void OpenWithDifftool(this RevisionGrid grid, string fileName, string oldFileName, DiffWithRevisionKind diffKind)
        {
            //Note: Order in revisions is that first clicked is last in array
            string extraDiffArgs;
            string firstRevision;
            string secondRevision;

            string error = DiffKindRevision.Get(grid.GetSelectedRevisions(), diffKind, out extraDiffArgs, out firstRevision, out secondRevision);
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(grid, error);
            }
            else
            {
                string output = grid.Module.OpenWithDifftool(fileName, oldFileName, firstRevision, secondRevision, extraDiffArgs);
                if (!string.IsNullOrEmpty(output))
                    MessageBox.Show(grid, output);
            }
        }

        public static bool IsItemUntracked(GitItemStatus file,
            string firstRevision, string secondRevision)
        {
            if (firstRevision == GitRevision.UnstagedGuid && file.IsDeleted ||
                secondRevision == GitRevision.UnstagedGuid && file.IsNew)
            {
                return true;
            }
            else if (firstRevision == GitRevision.UnstagedGuid) //working directory changes
            {
                if (secondRevision == null || secondRevision == GitRevision.IndexGuid)
                    return !file.IsTracked;
            }
            return false;
        }

        private static PatchApply.Patch GetItemPatch(GitModule module, GitItemStatus file,
            string firstRevision, string secondRevision, string diffArgs, Encoding encoding)
        {
            return module.GetSingleDiff(firstRevision, secondRevision, file.Name, file.OldName,
                    diffArgs, encoding, true);
        }

        public static string GetSelectedPatch(this FileViewer diffViewer, string firstRevision, string secondRevision, GitItemStatus file)
        {
            if (firstRevision == null)
                return null;

            if (IsItemUntracked(file, firstRevision, secondRevision))
            {
                var fullPath = Path.Combine(diffViewer.Module.WorkingDir, file.Name);
                if (Directory.Exists(fullPath) && GitModule.IsValidGitWorkingDir(fullPath))
                    return LocalizationHelpers.GetSubmoduleText(diffViewer.Module, file.Name.TrimEnd('/'), "");
                return FileReader.ReadFileContent(fullPath, diffViewer.Encoding);
            }

            if (file.IsSubmodule && file.SubmoduleStatus != null)
                return LocalizationHelpers.ProcessSubmoduleStatus(diffViewer.Module, file.SubmoduleStatus.Result);

            PatchApply.Patch patch = GetItemPatch(diffViewer.Module, file, firstRevision, secondRevision,
                diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);

            if (patch == null)
                return string.Empty;

            if (file.IsSubmodule)
                return LocalizationHelpers.ProcessSubmodulePatch(diffViewer.Module, file.Name, patch);
            return patch.Text;
        }

        public static void ViewChanges(this FileViewer diffViewer, IList<GitRevision> revisions, GitItemStatus file, string defaultText)
        {
            var firstRevision = revisions.Count > 0 ? revisions[0] : null;
            string firstRevisionGuid = firstRevision == null ? null : firstRevision.Guid;
            string parentRevisionGuid = revisions.Count == 2 ? revisions[1].Guid : null;
            if (parentRevisionGuid == null && firstRevision != null)
                parentRevisionGuid = firstRevision.FirstParentGuid;
            ViewChanges(diffViewer, firstRevisionGuid, parentRevisionGuid, file, defaultText);
        }

        public static void ViewChanges(this FileViewer diffViewer, string revision, string parentRevision, GitItemStatus file, string defaultText)
        {
            if (parentRevision == null)
            {
                if (file.TreeGuid.IsNullOrEmpty())
                    diffViewer.ViewGitItemRevision(file.Name, revision);
                else if (!file.IsSubmodule)
                    diffViewer.ViewGitItem(file.Name, file.TreeGuid);
                else
                    diffViewer.ViewText(file.Name,
                        LocalizationHelpers.GetSubmoduleText(diffViewer.Module, file.Name, file.TreeGuid));
            }
            else
            {
                diffViewer.ViewPatch(() =>
                    {
                        string selectedPatch = diffViewer.GetSelectedPatch(parentRevision, revision, file);
                        return selectedPatch ?? defaultText;
                    });
            }
        }

        public static void RemoveIfExists(this TabControl tabControl, TabPage page)
        {
            if (tabControl.TabPages.Contains(page))
                tabControl.TabPages.Remove(page);
        }

        public static void InsertIfNotExists(this TabControl tabControl, int index, TabPage page)
        {
            if (!tabControl.TabPages.Contains(page))
                tabControl.TabPages.Insert(index, page);
        }

        public static void Mask(this Control control)
        {
            if (control.FindMaskPanel() == null)
            {
                MaskPanel panel = new MaskPanel();
                control.Controls.Add(panel);
                panel.Dock = DockStyle.Fill;
                panel.BringToFront();
            }
        }

        public static void UnMask(this Control control)
        {
            MaskPanel panel = control.FindMaskPanel();
            if (panel != null)
            {
                control.Controls.Remove(panel);
                panel.Dispose();
            }
        }

        private static MaskPanel FindMaskPanel(this Control control)
        {
            foreach (var c in control.Controls)
                if (c is MaskPanel)
                    return c as MaskPanel;

            return null;
        }

        public class MaskPanel : PictureBox
        {
            public MaskPanel()
            {
                Image = Properties.Resources.loadingpanel;
                SizeMode = PictureBoxSizeMode.CenterImage;
                BackColor = SystemColors.AppWorkspace;
            }
        }

        public static IEnumerable<TreeNode> AllNodes(this TreeView tree)
        {
            return tree.Nodes.AllNodes();
        }

        public static IEnumerable<TreeNode> AllNodes(this TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;

                foreach(TreeNode subNode in node.Nodes.AllNodes())
                    yield return subNode;
            }
        }

        public static void InvokeAsync(this Control control, Action action)
        {
            InvokeAsync(control, _ => action(), null);
        }

        public static void InvokeAsync(this Control control, SendOrPostCallback action, object state)
        {
            SendOrPostCallback checkDisposedAndInvoke = (s) =>
            {
                if (!control.IsDisposed)
                    action(s);
            };

            if (!control.IsDisposed)
                UISynchronizationContext.Post(checkDisposedAndInvoke, state);
        }

        public static void InvokeSync(this Control control, Action action)
        {
            InvokeSync(control, _ => action(), null);
        }

        public static void InvokeSync(this Control control, SendOrPostCallback action, object state)
        {
            SendOrPostCallback checkDisposedAndInvoke = (s) =>
            {
                if (!control.IsDisposed)
                {
                    try
                    {
                        action(s);
                    }
                    catch (Exception e)
                    {
                        e.Data["StackTrace" + e.Data.Count] = e.StackTrace;
                        throw;
                    }
                }
            };

            if (!control.IsDisposed)
                UISynchronizationContext.Send(checkDisposedAndInvoke, state);
        }

        public static Control FindFocusedControl(this ContainerControl container)
        {
            var control = container.ActiveControl;
            container = control as ContainerControl;

            if (container == null)
                return control;
            else
                return container.FindFocusedControl();
        }

    }
}
