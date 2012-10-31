using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor;
using ICSharpCode.TextEditor.Util;

namespace GitUI
{
    public static class GitUIExtensions
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
                if (GitRevision.IsArtificial(secondRevision) && firstRevision != GitRevision.UncommittedWorkingDirGuid)
                {
                    firstRevision = secondRevision;
                    secondRevision = revisions[0].Guid;
                }

                string extraDiffArgs = "-M -C";

                if (firstRevision == GitRevision.UncommittedWorkingDirGuid) //working dir changes
                {
                    if (secondRevision == null || secondRevision == GitRevision.IndexGuid)
                    {
                        firstRevision = string.Empty;
                        secondRevision = string.Empty;
                    }
                    else
                    {
                        // rev2 vs working dir changes
                        firstRevision = secondRevision;
                        secondRevision = string.Empty;
                    }
                }
                if (firstRevision == GitRevision.IndexGuid) //index
                {
                    if (secondRevision == null)
                    {
                        firstRevision = string.Empty;
                        secondRevision = string.Empty;
                        extraDiffArgs = string.Join(" ", extraDiffArgs, "--cached");
                    }
                    else //rev1 vs index
                    {
                        firstRevision = secondRevision;
                        secondRevision = string.Empty;
                        extraDiffArgs = string.Join(" ", extraDiffArgs, "--cached");
                    }
                }

                Debug.Assert(!GitRevision.IsArtificial(firstRevision), string.Join(" ", firstRevision, secondRevision));

                if (secondRevision == null)
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

        public static string GetSelectedPatch(this FileViewer diffViewer, RevisionGrid grid, GitItemStatus file)
        {
            IList<GitRevision> revisions = grid.GetSelectedRevisions();

            if (revisions.Count == 0)
                return null;

            string firstRevision = revisions[0].Guid;
            var secondRevision = revisions.Count == 2 ? revisions[1].Guid : null;

            //to simplify if-ology
            if (GitRevision.IsArtificial(secondRevision) && firstRevision != GitRevision.UncommittedWorkingDirGuid)
            {
                firstRevision = secondRevision;
                secondRevision = revisions[0].Guid;
            }

            string extraDiffArgs = null;

            if (firstRevision == GitRevision.UncommittedWorkingDirGuid) //working dir changes
            {
                if (secondRevision == null || secondRevision == GitRevision.IndexGuid)
                {
                    if (file.IsTracked)
                    {
                        return ProcessDiffText(grid.Module, grid.Module.GetCurrentChanges(file.Name, file.OldName, false,
                            diffViewer.GetExtraDiffArguments(), diffViewer.Encoding), file.IsSubmodule);
                    }

                    return FileReader.ReadFileContent(grid.Module.WorkingDir + file.Name, diffViewer.Encoding);
                }
                else
                {
                    firstRevision = secondRevision;
                    secondRevision = string.Empty;
                }
            }
            if (firstRevision == GitRevision.IndexGuid) //index
            {
                if (secondRevision == null)
                {
                    return ProcessDiffText(grid.Module, grid.Module.GetCurrentChanges(file.Name, file.OldName, true,
                        diffViewer.GetExtraDiffArguments(), diffViewer.Encoding), file.IsSubmodule);
                }

                //rev1 vs index
                firstRevision = secondRevision;
                secondRevision = string.Empty;
                extraDiffArgs = string.Join(" ", extraDiffArgs, "--cached");
            }

            Debug.Assert(!GitRevision.IsArtificial(firstRevision), string.Join(" ", firstRevision,secondRevision));                

            if (secondRevision == null)
                secondRevision = firstRevision + "^";

            PatchApply.Patch patch = grid.Module.GetSingleDiff(firstRevision, secondRevision, file.Name, file.OldName,
                                                    string.Join(" ", diffViewer.GetExtraDiffArguments(), extraDiffArgs), diffViewer.Encoding);

            if (patch == null)
                return string.Empty;

            return ProcessDiffText(grid.Module, patch.Text, file.IsSubmodule);
        }

        private static string ProcessDiffText(GitModule module, string diff, bool isSubmodule)
        {
            if (isSubmodule)
                return GitCommandHelpers.ProcessSubmodulePatch(module, diff);

            return diff;
        }

        public static void ViewPatch(this FileViewer diffViewer, RevisionGrid grid, GitItemStatus file, string defaultText)
        {
            IList<GitRevision> revisions = grid.GetSelectedRevisions();

            if (revisions.Count == 1 && (revisions[0].ParentGuids == null || revisions[0].ParentGuids.Length == 0))
            {
                if (file.TreeGuid.IsNullOrEmpty())
                    diffViewer.ViewGitItemRevision(file.Name, revisions[0].Guid);
                else if (!file.IsSubmodule)
                    diffViewer.ViewGitItem(file.Name, file.TreeGuid);
                else
                    diffViewer.ViewText(file.Name, 
                        GitCommandHelpers.GetSubmoduleText(grid.Module, file.Name, file.TreeGuid));
            }
            else
            {
                diffViewer.ViewPatch(() =>
                                       {
                                           string selectedPatch = diffViewer.GetSelectedPatch(grid, file);
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
    }
}
