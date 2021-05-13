using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Git.Commands;
using GitCommands.Git.Tag;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormVerify : GitModuleForm
    {
        private const string RestoredObjectsTagPrefix = "LOST_FOUND_";

        private readonly TranslationString _removeDanglingObjectsCaption = new("Remove");
        private readonly TranslationString _removeDanglingObjectsQuestion = new("Are you sure you want to delete all dangling objects?");
        private readonly TranslationString _xTagsCreated = new("{0} Tags created." + Environment.NewLine + Environment.NewLine + "Do not forget to delete these tags when finished.");
        private readonly TranslationString _selectLostObjectsToRestoreMessage = new("Select objects to restore.");
        private readonly TranslationString _selectLostObjectsToRestoreCaption = new("Restore lost objects");

        private readonly List<LostObject> _lostObjects = new();
        private readonly SortableLostObjectsList _filteredLostObjects = new();
        private readonly DataGridViewCheckBoxHeaderCell _selectedItemsHeader = new();
        private readonly IGitTagController _gitTagController;

        private LostObject? _previewedItem;

        private static readonly Dictionary<string, string> LanguagesStartOfFile = new()
        {
            { "{", "recovery.json" },
            { "#include", "recovery.cpp" },
            { "import", "recovery.java" },
            { "from", "recovery.py" },
            { "package", "recovery.go" },
            { "#!", "recovery.sh" },
            { "[", "recovery.ini" },
            { "using", "recovery.cs" },
        };

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormVerify()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormVerify(GitUICommands commands)
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
            fileViewer.ExtraDiffArgumentsChanged += Warnings_SelectionChanged;

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

        private LostObject? CurrentItem => Warnings.SelectedRows.Count == 0 ? null : _filteredLostObjects[Warnings.SelectedRows[0].Index];

        #region Event Handlers

        private void FormVerifyShown(object sender, EventArgs e)
        {
            UpdateLostObjects();
            Warnings.DataSource = _filteredLostObjects;
        }

        private void SaveObjectsClick(object sender, EventArgs e)
        {
            var options = GetOptions();
            FormProcess.ShowDialog(this, process: null, arguments: $"fsck-objects --lost-found{options}", Module.WorkingDir, input: null, useDialogSettings: true);
            UpdateLostObjects();
        }

        private void RemoveClick(object sender, EventArgs e)
        {
            if (MessageBox.Show(this,
                _removeDanglingObjectsQuestion.Text,
                _removeDanglingObjectsCaption.Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            FormProcess.ShowDialog(this, process: null, arguments: "prune", Module.WorkingDir, input: null, useDialogSettings: true);
            UpdateLostObjects();
        }

        private void mnuLostObjectView_Click(object sender, EventArgs e)
        {
            ViewCurrentItem();
        }

        private void mnuLostObjectsCreateTag_Click(object sender, EventArgs e)
        {
            using FormCreateTag frm = new(UICommands, GetCurrentGitRevision());
            var dialogResult = frm.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                UpdateLostObjects();
            }
        }

        private void mnuLostObjectsCreateBranch_Click(object sender, EventArgs e)
        {
            using FormCreateBranch frm = new(UICommands, GetCurrentGitRevision());
            var dialogResult = frm.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
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

        private void Warnings_SelectionChanged(object sender, EventArgs e)
        {
            if (CurrentItem is null || _previewedItem == CurrentItem)
            {
                return;
            }

            _previewedItem = CurrentItem;

            var content = Module.ShowObject(_previewedItem.ObjectId) ?? "";
            if (_previewedItem.ObjectType == LostObjectType.Commit)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(() =>
                    fileViewer.ViewFixedPatchAsync("commit.patch", content, null))
                .FileAndForget();
            }
            else
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    var filename = GuessFileTypeWithContent(content);
                    await fileViewer.ViewTextAsync(filename, content, null);
                }).FileAndForget();
            }
        }

        private static string GuessFileTypeWithContent(string content)
        {
            if (content.StartsWith("<"))
            {
                return content.Contains("<html", StringComparison.InvariantCultureIgnoreCase) ? "recovery.html" : "recovery.xml";
            }

            foreach (var pair in LanguagesStartOfFile)
            {
                if (content.StartsWith(pair.Key))
                {
                    return pair.Value;
                }
            }

            if (content.Contains("function"))
            {
                return "recovery.ts";
            }

            return "recovery.txt";
        }

        #endregion

        private void UpdateLostObjects()
        {
            using (WaitCursorScope.Enter())
            {
                string cmdOutput = FormProcess.ReadDialog(this, process: null, arguments: $"fsck-objects{GetOptions()}", Module.WorkingDir, input: null, useDialogSettings: true);
                if (FormProcess.IsOperationAborted(cmdOutput))
                {
                    DialogResult = DialogResult.Abort;
                    return;
                }

                _lostObjects.Clear();
                _lostObjects.AddRange(
                    cmdOutput
                        .Split('\r', '\n')
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => LostObject.TryParse(Module, s))
                        .WhereNotNull()
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
            if (currentItem is null)
            {
                return;
            }

            string? obj = Module.ShowObject(currentItem.ObjectId);

            if (obj is not null)
            {
                using FormEdit frm = new(UICommands, obj);
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
                GitCreateTagArgs createTagArgs = new($"{RestoredObjectsTagPrefix}{tagName}", lostObject.ObjectId);
                _gitTagController.CreateTag(createTagArgs, this);
            }

            return currentTag;
        }

        private void DeleteLostFoundTags()
        {
            foreach (var head in Module.GetRefs(RefsFilter.Tags))
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
            if (Warnings is not null && Warnings.SelectedRows.Count != 0 && Warnings.SelectedRows[0].DataBoundItem is not null)
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
            if (Warnings is not null && Warnings.SelectedRows.Count != 0 && Warnings.SelectedRows[0].DataBoundItem is not null)
            {
                var lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;
                ClipboardUtil.TrySetText(lostObject.ObjectId.ToString());
            }
        }

        private void copyParentHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Warnings is not null && Warnings.SelectedRows.Count != 0 && Warnings.SelectedRows[0].DataBoundItem is not null)
            {
                var lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;
                ObjectId? parent = lostObject.Parent;

                if (parent is not null)
                {
                    ClipboardUtil.TrySetText(parent.ToString());
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Warnings is null || Warnings.SelectedRows.Count == 0 || Warnings.SelectedRows[0].DataBoundItem is null)
            {
                return;
            }

            var lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;

            if (lostObject.ObjectType == LostObjectType.Blob)
            {
                using SaveFileDialog fileDialog =
                    new()
                    {
                        InitialDirectory = Module.WorkingDir,
                        FileName = "LOST_FOUND.txt",
                        Filter = "(*.*)|*.*",
                        DefaultExt = "txt",
                        AddExtension = true
                    };
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Module.SaveBlobAs(fileDialog.FileName, lostObject.ObjectId.ToString());
                }
            }
        }
    }
}
