using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Git.Tag;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormVerify : GitModuleForm
    {
        private const string RestoredObjectsTagPrefix = "LOST_FOUND_";

        private readonly TranslationString _removeDanglingObjectsCaption = new TranslationString("Remove");
        private readonly TranslationString _removeDanglingObjectsQuestion = new TranslationString("Are you sure you want to delete all dangling objects?");
        private readonly TranslationString _xTagsCreated = new TranslationString("{0} Tags created." + Environment.NewLine + Environment.NewLine + "Do not forget to delete these tags when finished.");
        private readonly TranslationString _selectLostObjectsToRestoreMessage = new TranslationString("Select objects to restore.");
        private readonly TranslationString _selectLostObjectsToRestoreCaption = new TranslationString("Restore lost objects");

        private readonly List<LostObject> _lostObjects = new List<LostObject>();
        private readonly SortableLostObjectsList _filteredLostObjects = new SortableLostObjectsList();
        private readonly DataGridViewCheckBoxHeaderCell _selectedItemsHeader = new DataGridViewCheckBoxHeaderCell();
        private readonly IGitTagController _gitTagController;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormVerify()
        {
            InitializeComponent();
        }

        public FormVerify([NotNull] GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();

            columnIsLostObjectSelected.Width = DpiUtil.Scale(20);
            columnDate.Width = DpiUtil.Scale(56);
            columnType.Width = DpiUtil.Scale(58);
            columnAuthor.Width = DpiUtil.Scale(150);
            columnHash.Width = DpiUtil.Scale(280);
            columnHash.MinimumWidth = DpiUtil.Scale(75);
            columnParent.Width = DpiUtil.Scale(280);
            columnParent.MinimumWidth = DpiUtil.Scale(75);

            _selectedItemsHeader.AttachTo(columnIsLostObjectSelected);

            InitializeComplete();
            Warnings.AutoGenerateColumns = false;

            columnIsLostObjectSelected.DataPropertyName = "IsSelected"; // TODO this property is not on the bound type
            columnDate.DataPropertyName = nameof(LostObject.Date);
            columnType.DataPropertyName = nameof(LostObject.RawType);
            columnSubject.DataPropertyName = nameof(LostObject.Subject);
            columnAuthor.DataPropertyName = nameof(LostObject.Author);
            columnHash.DataPropertyName = nameof(LostObject.ObjectId);
            columnParent.DataPropertyName = nameof(LostObject.Parent);

            _gitTagController = new GitTagController(commands);
        }

        [CanBeNull]
        private LostObject CurrentItem => Warnings.SelectedRows.Count == 0 ? null : _filteredLostObjects[Warnings.SelectedRows[0].Index];

        #region Event Handlers

        private void FormVerifyShown(object sender, EventArgs e)
        {
            UpdateLostObjects();
            Warnings.DataSource = _filteredLostObjects;
        }

        private void SaveObjectsClick(object sender, EventArgs e)
        {
            var options = GetOptions();

            FormProcess.ShowDialog(this, "fsck-objects --lost-found" + options);
            UpdateLostObjects();
        }

        private void RemoveClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,
                _removeDanglingObjectsQuestion.Text,
                _removeDanglingObjectsCaption.Text,
                MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }

            FormProcess.ShowDialog(this, "prune");
            UpdateLostObjects();
        }

        private void mnuLostObjectView_Click(object sender, EventArgs e)
        {
            ViewCurrentItem();
        }

        private void mnuLostObjectsCreateTag_Click(object sender, EventArgs e)
        {
            using (var frm = new FormCreateTag(UICommands, GetCurrentGitRevision()))
            {
                var dialogResult = frm.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    UpdateLostObjects();
                }
            }
        }

        private void mnuLostObjectsCreateBranch_Click(object sender, EventArgs e)
        {
            using (var frm = new FormCreateBranch(UICommands, GetCurrentGitRevision()))
            {
                var dialogResult = frm.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    UpdateLostObjects();
                }
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
            {
                return;
            }

            MessageBox.Show(this, string.Format(_xTagsCreated.Text, restoredObjectsCount), "Tags created", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        private void ShowCommitsCheckedChanged(object sender, EventArgs e)
        {
            if (!ShowCommitsAndTags.Checked && !ShowOtherObjects.Checked)
            {
                ShowOtherObjects.Checked = true;
            }

            UpdateFilteredLostObjects();
        }

        private void ShowOtherObjects_CheckedChanged(object sender, EventArgs e)
        {
            if (!ShowCommitsAndTags.Checked && !ShowOtherObjects.Checked)
            {
                ShowCommitsAndTags.Checked = true;
            }

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
            {
                return;
            }

            // ignore double click by checkbox, user probably wanted to change checked state
            if (e.ColumnIndex == 0)
            {
                return;
            }

            ViewCurrentItem();
        }

        #endregion

        private void UpdateLostObjects()
        {
            using (WaitCursorScope.Enter())
            {
                var dialogResult = FormProcess.ReadDialog(this, "fsck-objects" + GetOptions());

                if (FormProcess.IsOperationAborted(dialogResult))
                {
                    DialogResult = DialogResult.Abort;
                    return;
                }

                _lostObjects.Clear();
                _lostObjects.AddRange(
                    dialogResult
                        .Split('\r', '\n')
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select((s) => LostObject.TryParse(Module, s))
                        .Where(parsedLostObject => parsedLostObject != null)
                        .OrderByDescending(l => l.Date));

                UpdateFilteredLostObjects();
            }
        }

        private void UpdateFilteredLostObjects()
        {
            SuspendLayout();
            _filteredLostObjects.Clear();
            _filteredLostObjects.AddRange(_lostObjects.Where(IsMatchToFilter));

            columnAuthor.Visible = ShowCommitsAndTags.Checked;
            columnSubject.Visible = ShowCommitsAndTags.Checked;
            columnParent.Visible = ShowCommitsAndTags.Checked;
            ////Warnings.DataSource = filteredLostObjects;
            ResumeLayout();
        }

        // TODO: add textbox for simple fulltext search/filtering (useful for large repos)
        private bool IsMatchToFilter(LostObject lostObject)
        {
            return (ShowCommitsAndTags.Checked && (lostObject.ObjectType == LostObjectType.Commit || lostObject.ObjectType == LostObjectType.Tag))
                || (ShowOtherObjects.Checked && (lostObject.ObjectType != LostObjectType.Commit && lostObject.ObjectType != LostObjectType.Tag));
        }

        private string GetOptions()
        {
            var options = string.Empty;

            if (Unreachable.Checked)
            {
                options += " --unreachable";
            }

            if (FullCheck.Checked)
            {
                options += " --full";
            }

            if (NoReflogs.Checked)
            {
                options += " --no-reflogs";
            }

            return options;
        }

        private void ViewCurrentItem()
        {
            var currentItem = CurrentItem;
            if (currentItem == null)
            {
                return;
            }

            using (var frm = new FormEdit(UICommands, Module.ShowObject(currentItem.ObjectId)))
            {
                frm.IsReadOnly = true;
                frm.ShowDialog(this);
            }
        }

        private int CreateLostFoundTags()
        {
            var selectedLostObjects = Warnings.Rows
                .Cast<DataGridViewRow>()
                .Select(row => row.Cells[columnIsLostObjectSelected.Index])
                .Where(cell => (bool?)cell.Value == true)
                .Select(cell => _filteredLostObjects[cell.RowIndex])
                .ToList();

            if (selectedLostObjects.Count == 0)
            {
                MessageBox.Show(this, _selectLostObjectsToRestoreMessage.Text, _selectLostObjectsToRestoreCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }

            var currentTag = 0;
            foreach (var lostObject in selectedLostObjects)
            {
                currentTag++;
                var tagName = lostObject.ObjectType == LostObjectType.Tag ? lostObject.TagName : currentTag.ToString();
                var createTagArgs = new GitCreateTagArgs($"{RestoredObjectsTagPrefix}{tagName}", lostObject.ObjectId);
                _gitTagController.CreateTag(createTagArgs, this);
            }

            return currentTag;
        }

        private void DeleteLostFoundTags()
        {
            foreach (var head in Module.GetRefs(true, false))
            {
                if (head.Name.StartsWith(RestoredObjectsTagPrefix))
                {
                    Module.DeleteTag(head.Name);
                }
            }
        }

        private ObjectId GetCurrentGitRevision()
        {
            return CurrentItem?.ObjectId ?? throw new InvalidOperationException("There are no current selected item.");
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _selectedItemsHeader.Detach();
                _selectedItemsHeader.Dispose();

                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void mnuLostObjects_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Warnings != null && Warnings.SelectedRows.Count != 0 && Warnings.SelectedRows[0].DataBoundItem != null)
            {
                var lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;
                var isCommit = lostObject.ObjectType == LostObjectType.Commit;
                var isBlob = lostObject.ObjectType == LostObjectType.Blob;
                var contextMenu = Warnings.SelectedRows[0].ContextMenuStrip;
                contextMenu.Items[1].Enabled = isCommit;
                contextMenu.Items[2].Enabled = isCommit;
                contextMenu.Items[4].Enabled = isCommit;
                contextMenu.Items[5].Enabled = isBlob;
            }
        }

        private void Warnings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ViewCurrentItem();
            }
        }

        private void copyHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Warnings != null && Warnings.SelectedRows.Count != 0 && Warnings.SelectedRows[0].DataBoundItem != null)
            {
                var lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;
                ClipboardUtil.TrySetText(lostObject.ObjectId.ToString());
            }
        }

        private void copyParentHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Warnings != null && Warnings.SelectedRows.Count != 0 && Warnings.SelectedRows[0].DataBoundItem != null)
            {
                var lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;
                ClipboardUtil.TrySetText(lostObject.Parent?.ToString());
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Warnings == null || Warnings.SelectedRows.Count == 0 || Warnings.SelectedRows[0].DataBoundItem == null)
            {
                return;
            }

            var lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;

            if (lostObject.ObjectType == LostObjectType.Blob)
            {
                using (var fileDialog =
                    new SaveFileDialog
                    {
                        InitialDirectory = Module.WorkingDir,
                        FileName = "LOST_FOUND.txt",
                        Filter = "(*.*)|*.*",
                        DefaultExt = "txt",
                        AddExtension = true
                    })
                {
                    if (fileDialog.ShowDialog(this) == DialogResult.OK)
                    {
                        Module.SaveBlobAs(fileDialog.FileName, lostObject.ObjectId.ToString());
                    }
                }
            }
        }
    }
}