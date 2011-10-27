using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.Tag;
using ResourceManager.Translation;

namespace GitUI
{
    public sealed partial class FormVerify : GitExtensionsForm
    {
        private const string RestoredObjectsTagPrefix = "LOST_FOUND_";

        private readonly TranslationString _removeDanglingObjectsCaption = new TranslationString("Remove");

        private readonly TranslationString _removeDanglingObjectsQuestion =
            new TranslationString("Are you sure you want to delete all dangling objects?");

        private readonly TranslationString _xTagsCreated =
            new TranslationString("{0} Tags created." + Environment.NewLine + Environment.NewLine +
                                  "Do not forget to delete these tags when finished.");

        private readonly TranslationString selectLostObjectsToRestoreMessage = new TranslationString("Select objects to restore.");
        private readonly TranslationString selectLostObjectsToRestoreCaption = new TranslationString("Restore lost objects");

        private readonly List<LostObject> lostObjects = new List<LostObject>();
        private readonly SortableLostObjectsList filteredLostObjects = new SortableLostObjectsList();
        private readonly DataGridViewCheckBoxHeaderCell selectedItemsHeader = new DataGridViewCheckBoxHeaderCell();

        public FormVerify()
        {
            InitializeComponent();
            selectedItemsHeader.AttachTo(columnIsLostObjectSelected);

            Translate();
            Warnings.AutoGenerateColumns = false;
        }

        private LostObject CurrentItem
        {
            get { return Warnings.SelectedRows.Count == 0 ? null : filteredLostObjects[Warnings.SelectedRows[0].Index]; }
        }

        #region Event Handlers

        private void FormVerifyShown(object sender, EventArgs e)
        {
            UpdateLostObjects();
            Warnings.DataSource = filteredLostObjects;
        }

        private void SaveObjectsClick(object sender, EventArgs e)
        {
            var options = GetOptions();

            var process = new FormProcess("fsck-objects --lost-found" + options);
            process.ShowDialog();
            UpdateLostObjects();
        }

        private void RemoveClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                _removeDanglingObjectsQuestion.Text,
                _removeDanglingObjectsCaption.Text,
                MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            new FormProcess("prune").ShowDialog();
            UpdateLostObjects();
        }

        private void mnuLostObjectView_Click(object sender, EventArgs e)
        {
            ViewCurrentItem();
        }

        private void mnuLostObjectsCreateTag_Click(object sender, EventArgs e)
        {
            using (var frm = new FormTagSmall { Revision = GetCurrentGitRevision() })
            {
                var dialogResult = frm.ShowDialog();
                if (dialogResult == DialogResult.OK)
                    UpdateLostObjects();
            }
        }

        private void mnuLostObjectsCreateBranch_Click(object sender, EventArgs e)
        {
            using (var frm = new FormBranchSmall { Revision = GetCurrentGitRevision() })
            {
                var dialogResult = frm.ShowDialog();
                if (dialogResult == DialogResult.OK)
                    UpdateLostObjects();
            }
        }

        private void DeleteAllLostAndFoundTagsClick(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            UpdateLostObjects();
        }

        private void btnRestoreSelectedObjects_Click(object sender, EventArgs e)
        {
            DeleteLostFoundTags();
            var restoredObjectsCount = CreateLostFoundTags();

            if (restoredObjectsCount == 0)
                return;

            MessageBox.Show(string.Format(_xTagsCreated.Text, restoredObjectsCount), "Tags created", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // if user restored all items, nothing else to do in this form. 
            // User wants to see restored commits, so close this dialog and return to the main window.
            if (restoredObjectsCount == Warnings.Rows.Count)
            {
                DialogResult = DialogResult.OK;
                return;
            }

            UpdateLostObjects();
        }

        private void UnreachableCheckedChanged(object sender, EventArgs e)
        {
            UpdateLostObjects();
        }

        private void FullCheckCheckedChanged(object sender, EventArgs e)
        {
            UpdateLostObjects();
        }

        private void NoReflogsCheckedChanged(object sender, EventArgs e)
        {
            UpdateLostObjects();
        }

        private void ShowOnlyCommitsCheckedChanged(object sender, EventArgs e)
        {
            UpdateFilteredLostObjects();
        }

        // NOTE: hack to select row under cursor on right click and context menu open
        private void Warnings_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex != -1)
            {
                Warnings.Rows[e.RowIndex].Selected = true;
            }
        }

        private void Warnings_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // ignore double click by header, user just wants to change sorting order
            if (e.RowIndex == -1)
                return;
            // ignore double click by checkbox, user probably wanted to change checked state
            if (e.ColumnIndex == 0)
                return;

            ViewCurrentItem();
        }

        #endregion

        private void UpdateLostObjects()
        {
            Cursor.Current = Cursors.WaitCursor;

            string dialogResult;
            using (var process = new FormProcess("fsck-objects" + GetOptions()))
            {
                process.ShowDialog();
                dialogResult = process.OutputString.ToString();
            }

            if (FormProcess.IsOperationAborted(dialogResult))
            {
                DialogResult = DialogResult.Abort;
                return;
            }

            lostObjects.Clear();
            lostObjects.AddRange(dialogResult
                .Split('\r', '\n')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select<string, LostObject>(LostObject.TryParse)
                .Where(parsedLostObject => parsedLostObject != null));

            UpdateFilteredLostObjects();
            Cursor.Current = Cursors.Default;
        }

        private void UpdateFilteredLostObjects()
        {
            SuspendLayout();
            filteredLostObjects.Clear();
            filteredLostObjects.AddRange(lostObjects.Where(IsMatchToFilter));
            //Warnings.DataSource = filteredLostObjects;
            ResumeLayout();
        }

        // TODO: add textbox for simple fulltext search/filtering (useful for large repos)
        private bool IsMatchToFilter(LostObject lostObject)
        {
            if (ShowOnlyCommits.Checked)
                return lostObject.ObjectType == LostObjectType.Commit;
            return true;
        }

        private string GetOptions()
        {
            var options = string.Empty;

            if (Unreachable.Checked)
                options += " --unreachable";

            if (FullCheck.Checked)
                options += " --full";

            if (NoReflogs.Checked)
                options += " --no-reflogs";
            return options;
        }

        private void ViewCurrentItem()
        {
            var currenItem = CurrentItem;
            if (currenItem == null)
                return;
            new FormEdit(Settings.Module.ShowSha1(currenItem.Hash)).ShowDialog();
        }

        private int CreateLostFoundTags()
        {
            var selectedLostObjects = Warnings.Rows
                .Cast<DataGridViewRow>()
                .Select(row => row.Cells[columnIsLostObjectSelected.Index])
                .Where(cell => (bool?)cell.Value == true)
                .Select(cell => filteredLostObjects[cell.RowIndex])
                .ToList();

            if (selectedLostObjects.Count == 0)
            {
                MessageBox.Show(selectLostObjectsToRestoreMessage.Text, selectLostObjectsToRestoreCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }
            var currentTag = 0;
            foreach (var lostObject in selectedLostObjects)
            {
                currentTag++;
                Settings.Module.Tag(RestoredObjectsTagPrefix + currentTag, lostObject.Hash, false);
            }

            return currentTag;
        }

        private static void DeleteLostFoundTags()
        {
            foreach (var head in Settings.Module.GetHeads(true, false))
            {
                if (head.Name.StartsWith(RestoredObjectsTagPrefix))
                    Settings.Module.DeleteTag(head.Name);
            }
        }

        private GitRevision GetCurrentGitRevision()
        {
            var currentItem = CurrentItem;
            if (currentItem == null)
                throw new InvalidOperationException("There are no current selected item.");

            return new GitRevision(currentItem.Hash);
        }
    }
}