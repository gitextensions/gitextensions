using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;
using System.Windows.Forms;
using GitUI.Editor;
using ICSharpCode.TextEditor.Util;
using System.Diagnostics;

namespace GitUI
{
    public static class GitUIExtensions
    {

        public enum DiffWithRevisionKind
        {
            DiffBaseLocal,
            DiffRemoteLocal,
            DiffAsSelected
        }

        public static void OpenWithDifftool(this RevisionGrid grid, string fileName, DiffWithRevisionKind diffKind)
        {
            IList<GitRevision> revisions = grid.GetSelectedRevisions();

            if (revisions.Count == 0)
                return;

            string output;
            if (diffKind == DiffWithRevisionKind.DiffBaseLocal)
            {
                if (revisions[0].ParentGuids.Length == 0)
                    return;
                output = Settings.Module.OpenWithDifftool(fileName, revisions[0].ParentGuids[0]);

            }
            else if (diffKind == DiffWithRevisionKind.DiffRemoteLocal)
                output = Settings.Module.OpenWithDifftool(fileName, revisions[0].Guid);
            else
            {
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
                        extraDiffArgs = extraDiffArgs.Join(" ", "--cached");
                    }
                    else //rev1 vs index
                    {
                        firstRevision = secondRevision;
                        secondRevision = string.Empty;
                        extraDiffArgs = extraDiffArgs.Join(" ", "--cached");
                    }
                }

                Debug.Assert(!GitRevision.IsArtificial(firstRevision), firstRevision.Join(" ", secondRevision));

                if (secondRevision == null)
                    secondRevision = firstRevision + "^";

                output = Settings.Module.OpenWithDifftool(fileName, firstRevision, secondRevision, extraDiffArgs);
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
                        return Settings.Module.GetCurrentChanges(file.Name, file.OldName, false, diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);
                    return FileReader.ReadFileContent(Settings.WorkingDir + file.Name, diffViewer.Encoding);
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
                    return Settings.Module.GetCurrentChanges(file.Name, file.OldName, true, diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);
                else //rev1 vs index
                {
                    firstRevision = secondRevision;
                    secondRevision = string.Empty;
                    extraDiffArgs = extraDiffArgs.Join(" ", "--cached");                
                }
            }

            Debug.Assert(!GitRevision.IsArtificial(firstRevision), firstRevision.Join(" ", secondRevision));                

            if (secondRevision == null)
                secondRevision = firstRevision + "^";            

            PatchApply.Patch patch = Settings.Module.GetSingleDiff(firstRevision, secondRevision, file.Name, file.OldName,
                                                    diffViewer.GetExtraDiffArguments().Join(" ", extraDiffArgs), diffViewer.Encoding);

            if (patch == null)
                return string.Empty;

            if (file.IsSubmodule)
                return GitCommandHelpers.ProcessSubmodulePatch(patch.Text);

            return patch.Text;
        }


        public static void ViewPatch(this FileViewer diffViewer, RevisionGrid grid, GitItemStatus file, string defaultText)
        {
            diffViewer.ViewPatch(() =>
                                   {
                                       string selectedPatch = diffViewer.GetSelectedPatch(grid, file);

                                       return selectedPatch ?? defaultText;
                                   });
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


    }
}
