using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;
using System.Windows.Forms;
using GitUI.Editor;
using ICSharpCode.TextEditor.Util;

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
                if (revisions.Count == 1)   // single item selected
                {
                    if (revisions[0].Guid == GitRevision.UncommittedWorkingDirGuid) //working dir changes
                        output = Settings.Module.OpenWithDifftool(fileName);
                    else if (revisions[0].Guid == GitRevision.IndexGuid) //staged changes
                        output = Settings.Module.OpenWithDifftool(fileName, null, null, "--cached");
                    else
                        output = Settings.Module.OpenWithDifftool(fileName, revisions[0].Guid,
                                                                      revisions[0].ParentGuids[0]);
                }
                else                        // multiple items selected
                    output = Settings.Module.OpenWithDifftool(fileName, revisions[0].Guid,
                                                                  revisions[revisions.Count - 1].Guid);

            if (!string.IsNullOrEmpty(output))
                MessageBox.Show(grid, output);
        }


        public static string GetSelectedPatch(this FileViewer diffViewer, RevisionGrid grid, GitItemStatus file)
        {
            IList<GitRevision> revisions = grid.GetSelectedRevisions();

            if (revisions.Count == 0)
                return null;

            if (revisions[0].Guid == GitRevision.UncommittedWorkingDirGuid) //working dir changes
            {
                if (file.IsTracked)
                    return Settings.Module.GetCurrentChanges(file.Name, file.OldName, false, diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);
                return FileReader.ReadFileContent(Settings.WorkingDir + file.Name, diffViewer.Encoding);
            }
            if (revisions[0].Guid == GitRevision.IndexGuid) //index
            {
                return Settings.Module.GetCurrentChanges(file.Name, file.OldName, true, diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);
            }
            var secondRevision = revisions.Count == 2 ? revisions[1].Guid : revisions[0].Guid + "^";

            PatchApply.Patch patch = Settings.Module.GetSingleDiff(revisions[0].Guid, secondRevision, file.Name, file.OldName,
                                                    diffViewer.GetExtraDiffArguments(), diffViewer.Encoding);

            if (patch == null)
                return null;

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
