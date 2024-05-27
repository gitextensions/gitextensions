using GitCommands.Git;
using GitCommands.Git.Tag;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormVerify : GitModuleForm
    {
        private const string _restoredObjectsTagPrefix = "LOST_FOUND_";

        private readonly TranslationString _removeDanglingObjectsCaption = new("Remove");
        private readonly TranslationString _removeDanglingObjectsQuestion = new("Are you sure you want to delete all dangling objects?");
        private readonly TranslationString _xTagsCreated = new("{0} Tags created." + Environment.NewLine + Environment.NewLine + "Do not forget to delete these tags when finished.");
        private readonly TranslationString _selectLostObjectsToRestoreMessage = new("Select objects to restore.");
        private readonly TranslationString _selectLostObjectsToRestoreCaption = new("Restore lost objects");
        private readonly TranslationString _seemingly = new("seemingly");

        private readonly List<LostObject> _lostObjects = [];
        private readonly SortableLostObjectsList _filteredLostObjects = new();
        private readonly DataGridViewCheckBoxHeaderCell _selectedItemsHeader = new();
        private readonly IGitTagController _gitTagController;

        private LostObject? _previewedItem;
        private string? _defaultFilename;
        private bool _typeDetected;

        // https://en.wikipedia.org/wiki/List_of_file_signatures
        private static readonly Dictionary<string, string> _languagesStartOfFile = new()
        {
            { @"{\rtf", "rtf" },
            { "{", "json" },
            { "#include", "cpp" },
            { "import {", "js" },
            { "import * as", "js" },
            { "import \"", "js" },
            { "export ", "js" },
            { "import ", "java" },
            { "from", "py" },
            { "package", "go" },
            { "namespace ", "fs" },
            { "#!", "sh" },
            { "[", "ini" },
            { "using ", "cs" },
            { "# ", "md" },
            { "##", "md" },
            { "<!doctype html", "html" },
            { "<html", "html" },
            { "<?xml", "xml" },
            { "use ", "rs" },
            { "%PDF", "pdf" },
            { "PK", "zip" },
            { "MZ", "exe" },
            { @"\document", "tex" },
            { "\u0089PNG", "png" },
            { "ÿØÿQ", "jp2" },
            { "ÿØÿ", "jpg" },
            { "ÿ\x0A", "jxl" },
            { "RIFF", "webp" },
            { "<svg", "svg" },
            { "BM", "bmp" },
            { "7z", "7z" },
            { "GIF", "gif" },
            { "ÐÏ\x11à¡±\x1Aá", "doc" },
            { "qoif", "qoi" },
            { "Rar!", "rar" },
            { "%!PS", "ps" },
            { "OggS", "ogg" },
            { "8BPS", "psf" },
            { "ID3", "mp3" },
            { "CD001", "iso" },
            { "fLaC", "flac" },
            { "FLIF", "flif" },
            { "␚Eß£", "mkv" },
            { "<", "xml" },
        };

        private static readonly Dictionary<string, string[]> _fileTypesEquivalences = new()
        {
            { "js", ["ts", "jsx", "tsx"] },
            { "html", ["php", "cshtml"] },
            { "cpp", ["c"] },
            { "xml", ["config", "settings", "csproj", "xlf", "props"] },
            { "zip", ["docx", "xlsx", "pptx", "odt", "ods", "odp", "epub", "jar", "msix"] },
            { "exe", ["dll"] },
            { "doc", ["xls", "ppt", "msi"] },
            { "md", ["sh", "yml"] },
            { "txt", ["csv", "css", "md", "yml"] },
        };

        public FormVerify(IGitUICommands commands)
            : base(commands)
        {
            InitializeComponent();

            columnIsLostObjectSelected.Width = DpiUtil.Scale(20);
            columnDate.Width = DpiUtil.Scale(56);
            columnType.Width = DpiUtil.Scale(58);
            columnAuthor.Width = DpiUtil.Scale(150);
            columnHash.Width = DpiUtil.Scale(60);
            columnHash.MinimumWidth = DpiUtil.Scale(25);
            columnHash.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            columnParent.Width = DpiUtil.Scale(60);
            columnParent.MinimumWidth = DpiUtil.Scale(25);
            columnParent.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

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
            Warnings.Sort(columnDate, System.ComponentModel.ListSortDirection.Descending);
        }

        private void SaveObjectsClick(object sender, EventArgs e)
        {
            string options = GetOptions();
            FormProcess.ShowDialog(this, UICommands, arguments: $"fsck-objects --lost-found{options}", Module.WorkingDir, input: null, useDialogSettings: true);
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

            FormProcess.ShowDialog(this, UICommands, arguments: "prune", Module.WorkingDir, input: null, useDialogSettings: true);
            UpdateLostObjects();
        }

        private void mnuLostObjectView_Click(object sender, EventArgs e)
        {
            ViewCurrentItem();
        }

        private void mnuLostObjectsCreateTag_Click(object sender, EventArgs e)
        {
            using FormCreateTag frm = new(UICommands, GetCurrentGitRevision());
            DialogResult dialogResult = frm.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                UpdateLostObjects();
            }
        }

        private void mnuLostObjectsCreateBranch_Click(object sender, EventArgs e)
        {
            using FormCreateBranch frm = new(UICommands, GetCurrentGitRevision());
            DialogResult dialogResult = frm.ShowDialog(this);
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
            int restoredObjectsCount = CreateLostFoundTags();

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
            else
            {
                UpdateFilteredLostObjects();
            }
        }

        private void ShowOtherObjects_CheckedChanged(object sender, EventArgs e)
        {
            if (!ShowCommitsAndTags.Checked && !ShowOtherObjects.Checked)
            {
                ShowCommitsAndTags.Checked = true;
            }
            else
            {
                UpdateFilteredLostObjects();
            }
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
            _defaultFilename = null;
            if (CurrentItem is null || _previewedItem == CurrentItem)
            {
                return;
            }

            _previewedItem = CurrentItem;

            string content = GetLostObjectContent(_previewedItem);
            if (_previewedItem.ObjectType == LostObjectType.Commit || _previewedItem.ObjectType == LostObjectType.Tag)
            {
                _defaultFilename = "commit.patch";
                fileViewer.InvokeAndForget(() => fileViewer.ViewFixedPatchAsync(_defaultFilename, content, openWithDifftool: null));
            }
            else if (_previewedItem.ObjectType == LostObjectType.Blob)
            {
                _defaultFilename = GuessFileNameWithContent(content, _previewedItem.ObjectId.ToString());
                fileViewer.InvokeAndForget(() => fileViewer.ViewTextAsync(_defaultFilename, content, openWithDifftool: null));
            }
            else
            {
                _defaultFilename = "file.txt";
                fileViewer.InvokeAndForget(() => fileViewer.ViewTextAsync(_defaultFilename, content, openWithDifftool: null));
            }
        }

        private static string GuessFileTypeWithContent(string content)
        {
            foreach (KeyValuePair<string, string> pair in _languagesStartOfFile)
            {
                if (content.StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value;
                }
            }

            return "txt";
        }

        private static string GuessFileNameWithContent(string content, string hash)
            => $"LOST_FOUND_{hash}.{GuessFileTypeWithContent(content)}";

        #endregion

        private void UpdateLostObjects()
        {
            using (WaitCursorScope.Enter())
            {
                string cmdOutput = FormProcess.ReadDialog(this, UICommands, arguments: $"fsck-objects{GetOptions()}", Module.WorkingDir, input: null, useDialogSettings: true);
                if (FormProcess.IsOperationAborted(cmdOutput))
                {
                    DialogResult = DialogResult.Abort;
                    return;
                }

                _lostObjects.Clear();
                _lostObjects.AddRange(
                    cmdOutput
                        .Split(Delimiters.LineFeedAndCarriageReturn)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(s => LostObject.TryParse(Module, s))
                        .WhereNotNull()
                        .OrderByDescending(l => l.Date));

                LostObject[] commits = _lostObjects.Where(o => o.ObjectType == LostObjectType.Commit).ToArray();
                List<string> metadata = new(commits.Length);
                int batchSize = 30_000 / (ObjectId.Sha1CharCount + 1); // Based on process max command line length and hash length (with a margin)

                for (int currentBatch = 0; currentBatch * batchSize < commits.Length; ++currentBatch)
                {
                    LostObject[] nextBatch = commits.Skip(currentBatch * batchSize)
                        .Take(batchSize).ToArray();
                    string[] metadataBatch = LostObject.GetCommitsMetadata(Module, nextBatch.Select(c => c.ObjectId.ToString()));
                    metadata.AddRange(metadataBatch);
                }

                for (int i = 0; i < commits.Length; i++)
                {
                    commits[i].FillCommitData(Module, metadata[i]);
                }

                UpdateFilteredLostObjects();
            }
        }

        private void UpdateFilteredLostObjects()
        {
            if (ShowOtherObjects.Checked & !_typeDetected)
            {
                ThreadHelper.FileAndForget(async () =>
                {
                    _typeDetected = true;
                    foreach (LostObject item in _lostObjects.Where(o => o.ObjectType == LostObjectType.Blob))
                    {
                        string content = GetLostObjectContent(item);
                        item.RawType += $" ({_seemingly.Text}: {GuessFileTypeWithContent(content)})";
                    }

                    await this.SwitchToMainThreadAsync();
                    Warnings.AutoResizeColumn(columnType.Index);
                    Warnings.Refresh();
                });
            }

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

        private string GetLostObjectContent(LostObject item)
            => Module.ShowObject(item.ObjectId, returnRaw: item.ObjectType == LostObjectType.Blob) ?? "";

        private string GetOptions()
        {
            string options = string.Empty;

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
            LostObject currentItem = CurrentItem;
            if (currentItem is null)
            {
                return;
            }

            string obj = GetLostObjectContent(currentItem);

            if (!string.IsNullOrEmpty(obj))
            {
                using FormEdit frm = new(UICommands, obj, _defaultFilename);
                frm.IsReadOnly = true;
                frm.ShowDialog(this);
            }
        }

        private int CreateLostFoundTags()
        {
            List<LostObject> selectedLostObjects = Warnings.Rows
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

            int currentTag = 0;
            foreach (LostObject lostObject in selectedLostObjects)
            {
                currentTag++;
                string tagName = lostObject.ObjectType == LostObjectType.Tag ? lostObject.TagName : currentTag.ToString();
                GitCreateTagArgs createTagArgs = new($"{_restoredObjectsTagPrefix}{tagName}", lostObject.ObjectId);
                _gitTagController.CreateTag(createTagArgs, this);
            }

            return currentTag;
        }

        private void DeleteLostFoundTags()
        {
            foreach (IGitRef head in Module.GetRefs(RefsFilter.Tags))
            {
                if (head.Name.StartsWith(_restoredObjectsTagPrefix))
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
            if (Warnings?.SelectedRows.Count is > 0 && Warnings.SelectedRows[0].DataBoundItem is not null)
            {
                LostObject lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;
                bool isCommit = lostObject.ObjectType == LostObjectType.Commit;
                bool isBlob = lostObject.ObjectType == LostObjectType.Blob;
                ContextMenuStrip contextMenu = Warnings.SelectedRows[0].ContextMenuStrip;
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
            if (Warnings?.SelectedRows.Count is > 0 && Warnings.SelectedRows[0].DataBoundItem is not null)
            {
                LostObject lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;
                ClipboardUtil.TrySetText(lostObject.ObjectId.ToString());
            }
        }

        private void copyParentHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Warnings?.SelectedRows.Count is > 0 && Warnings.SelectedRows[0].DataBoundItem is not null)
            {
                LostObject lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;
                ObjectId? parent = lostObject.Parent;

                if (parent is not null)
                {
                    ClipboardUtil.TrySetText(parent.ToString());
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Warnings?.SelectedRows.Count is not > 0 || Warnings.SelectedRows[0].DataBoundItem is null)
            {
                return;
            }

            LostObject lostObject = (LostObject)Warnings.SelectedRows[0].DataBoundItem;

            if (lostObject.ObjectType == LostObjectType.Blob)
            {
                string filename = _defaultFilename ?? lostObject.ObjectId.ToString() + "_LOST_FOUND.txt";
                string extension = Path.GetExtension(filename).TrimStart('.');
                string filter = $"{extension} Files (*.{extension})|*.{extension}";
                if (_fileTypesEquivalences.TryGetValue(extension, out string[] types))
                {
                    filter += "|" + string.Join("|", types.Select(t => $"{t} Files (*.{t})|*.{t}"));
                }

                using SaveFileDialog fileDialog =
                    new()
                    {
                        InitialDirectory = Module.WorkingDir,
                        FileName = filename,
                        Filter = $"{filter}| All Files (*.*)|*.*",
                        DefaultExt = extension,
                        AddExtension = true
                    };
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Module.SaveBlobAs(fileDialog.FileName, lostObject.ObjectId.ToString());
                }
            }
        }

        protected override bool ProcessKeyPreview(ref Message msg)
        {
            // Check if keyboard shortcut is one provided by the contextual menu
            return mnuLostObjects.PreProcessMessage(ref msg) || base.ProcessKeyPreview(ref msg);
        }
    }
}
