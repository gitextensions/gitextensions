using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor;
using ICSharpCode.TextEditor.Util;
using System.Threading;

namespace GitUI
{
    public static class GitUIExtensions
    {

        public static SynchronizationContext UISynchronizationContext;

        /// <summary>
        /// One row selected:
        /// B - Selected row
        /// A - B's parent
        /// 
        /// Two rows selected:
        /// A - first selected row
        /// B - second selected row
        /// </summary>
        public enum DiffWithRevisionKind
        {
            DiffAB,
            DiffALocal,
            DiffBLocal,
            DiffAParentLocal,
            DiffBParentLocal
        }

        public static void OpenWithDifftool(this RevisionGrid grid, string fileName, string oldFileName, DiffWithRevisionKind diffKind)
        {
            IList<GitRevision> revisions = grid.GetSelectedRevisions();

            if (revisions.Count == 0 || revisions.Count > 2)
                return;

            string output;
            if (diffKind == DiffWithRevisionKind.DiffAB)
            {
                string firstRevision = revisions[0].Guid;
                var secondRevision = revisions.Count == 2 ? revisions[1].Guid : null;

                //to simplify if-ology
                if (GitRevision.IsArtificial(secondRevision) && firstRevision != GitRevision.UnstagedGuid)
                {
                    firstRevision = secondRevision;
                    secondRevision = revisions[0].Guid;
                }

                string extraDiffArgs = "-M -C";

                if (GitRevision.IsArtificial(firstRevision))
                {
                    bool staged = firstRevision == GitRevision.IndexGuid;
                    if (secondRevision == null || secondRevision == GitRevision.IndexGuid)
                        firstRevision = string.Empty;
                    else
                        firstRevision = secondRevision;
                    secondRevision = string.Empty;
                    if (staged) //rev1 vs index
                        extraDiffArgs = string.Join(" ", extraDiffArgs, "--cached");
                }
                else if (secondRevision == null)
                    secondRevision = firstRevision + "^";

                output = grid.Module.OpenWithDifftool(fileName, oldFileName, firstRevision, secondRevision, extraDiffArgs);
            }
            else
            {
                string revisionToCmp;
                if (revisions.Count == 1)
                {
                    GitRevision revision = revisions[0];
                    if (diffKind == DiffWithRevisionKind.DiffALocal)
                        revisionToCmp = revision.ParentGuids.Length == 0 ? null : revision.ParentGuids[0];
                    else if (diffKind == DiffWithRevisionKind.DiffBLocal)
                        revisionToCmp = revision.Guid;
                    else
                        revisionToCmp = null;
                }
                else
                {
                    if (diffKind == DiffWithRevisionKind.DiffALocal)
                        revisionToCmp = revisions[0].Guid;
                    else if (diffKind == DiffWithRevisionKind.DiffBLocal)
                        revisionToCmp = revisions[1].Guid;
                    else if (diffKind == DiffWithRevisionKind.DiffAParentLocal)
                        revisionToCmp = revisions[0].ParentGuids.Length == 0 ? null : revisions[0].ParentGuids[0];
                    else if (diffKind == DiffWithRevisionKind.DiffBLocal)
                        revisionToCmp = revisions[1].ParentGuids.Length == 0 ? null : revisions[1].ParentGuids[0];
                    else
                        revisionToCmp = null;
                }

                if (revisionToCmp == null)
                    return;

                output = grid.Module.OpenWithDifftool(fileName, revisionToCmp);            
            }

            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(grid, output);
        }

        public static bool IsItemUntracked(GitItemStatus file,
            string firstRevision, string secondRevision)
        {
            if (firstRevision == GitRevision.UnstagedGuid) //working dir changes
            {
                if (secondRevision == null || secondRevision == GitRevision.IndexGuid)
                    return !file.IsTracked;
            }
            return false;
        }

        private static PatchApply.Patch GetItemPatch(GitModule module, GitItemStatus file,
            string firstRevision, string secondRevision, string diffArgs, Encoding encoding)
        {
            if (GitRevision.IsArtificial(firstRevision))
            {
                bool staged = firstRevision == GitRevision.IndexGuid;
                if (secondRevision == null || secondRevision == GitRevision.IndexGuid)
                {
                    return module.GetCurrentChanges(file.Name, file.OldName, staged,
                            diffArgs, encoding);
                }

                firstRevision = secondRevision;
                secondRevision = string.Empty;
                if (staged)
                    diffArgs = string.Join(" ", diffArgs, "--cached");
            }
            else if (secondRevision == null)
                secondRevision = firstRevision + "^";

            return module.GetSingleDiff(firstRevision, secondRevision, file.Name, file.OldName,
                    diffArgs, encoding);
        }

        public static string GetSelectedPatch(this FileViewer diffViewer, RevisionGrid grid, GitItemStatus file)
        {
            IList<GitRevision> revisions = grid.GetSelectedRevisions();
            return GetSelectedPatch(diffViewer, revisions, file);
        }

        public static string GetSelectedPatch(this FileViewer diffViewer, IList<GitRevision> revisions, GitItemStatus file)
        {
            if (revisions.Count == 0)
                return null;

            string firstRevision = revisions[0].Guid;
            var secondRevision = revisions.Count == 2 ? revisions[1].Guid : null;

            //to simplify if-ology
            if (GitRevision.IsArtificial(secondRevision) && firstRevision != GitRevision.UnstagedGuid)
            {
                firstRevision = secondRevision;
                secondRevision = revisions[0].Guid;
            }

            if (IsItemUntracked(file, firstRevision, secondRevision))
            {
                var fullPath = Path.Combine(diffViewer.Module.WorkingDir, file.Name);
                if (Directory.Exists(fullPath) && GitModule.IsValidGitWorkingDir(fullPath))
                    return GitCommandHelpers.GetSubmoduleText(diffViewer.Module, file.Name.TrimEnd('/'), "");
                return FileReader.ReadFileContent(fullPath, diffViewer.Encoding);
            }

            if (file.IsSubmodule && file.SubmoduleStatus != null)
                return GitCommandHelpers.ProcessSubmoduleStatus(diffViewer.Module, file.SubmoduleStatus.Result);

            PatchApply.Patch patch = GetItemPatch(diffViewer.Module, file, firstRevision, secondRevision,
                diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);

            if (patch == null)
                return string.Empty;

            if (file.IsSubmodule)
                return GitCommandHelpers.ProcessSubmodulePatch(diffViewer.Module, patch);
            return patch.Text;
        }

        public static void ViewPatch(this FileViewer diffViewer, RevisionGrid grid, GitItemStatus file, string defaultText)
        {
            IList<GitRevision> revisions = grid.GetSelectedRevisions();
            ViewPatch(diffViewer, revisions, file, defaultText);
        }

        public static void ViewPatch(this FileViewer diffViewer, IList<GitRevision> revisions, GitItemStatus file, string defaultText)
        {
            if (revisions.Count == 1 && (revisions[0].ParentGuids == null || revisions[0].ParentGuids.Length == 0))
            {
                if (file.TreeGuid.IsNullOrEmpty())
                    diffViewer.ViewGitItemRevision(file.Name, revisions[0].Guid);
                else if (!file.IsSubmodule)
                    diffViewer.ViewGitItem(file.Name, file.TreeGuid);
                else
                    diffViewer.ViewText(file.Name, 
                        GitCommandHelpers.GetSubmoduleText(diffViewer.Module, file.Name, file.TreeGuid));
            }
            else
            {
                diffViewer.ViewPatch(() =>
                    {
                        string selectedPatch = diffViewer.GetSelectedPatch(revisions, file);
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
                    action(s);
            };

            if (!control.IsDisposed)
                UISynchronizationContext.Send(checkDisposedAndInvoke, state);
        }
    }
}
