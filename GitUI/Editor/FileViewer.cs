﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ICSharpCode.TextEditor.Util;
using PatchApply;
using GitUI.Hotkey;

namespace GitUI.Editor
{

    public partial class FileViewer : GitExtensionsControl
    {
        private readonly AsyncLoader _async;
        private int _currentScrollPos = -1;
        private bool _currentViewIsPatch;
        private readonly IFileViewer _internalFileViewer;

        public FileViewer()
        {
            TreatAllFilesAsText = false;
            ShowEntireFile = false;
            NumberOfVisibleLines = 3;
            InitializeComponent();
            Translate();

            if (GitCommands.Settings.RunningOnWindows())
                _internalFileViewer = new FileViewerWindows();
            else
                _internalFileViewer = new FileViewerMono();

            _internalFileViewer.MouseMove += _internalFileViewer_MouseMove;

            var internalFileViewerControl = (Control)_internalFileViewer;
            internalFileViewerControl.Dock = DockStyle.Fill;
            Controls.Add(internalFileViewerControl);

            _async = new AsyncLoader();
            _async.LoadingError +=
                (sender, args) =>
                {
                    ResetForText(null);
                    _internalFileViewer.SetText("Unsupported file");
                    if (TextLoaded != null)
                        TextLoaded(this, null);
                };

            IgnoreWhitespaceChanges = false;

            IsReadOnly = true;

            _internalFileViewer.MouseMove += TextAreaMouseMove;
            _internalFileViewer.MouseLeave += TextAreaMouseLeave;
            _internalFileViewer.TextChanged += TextEditor_TextChanged;
            _internalFileViewer.ScrollPosChanged += new EventHandler(_internalFileViewer_ScrollPosChanged);
            _internalFileViewer.SelectedLineChanged += new SelectedLineChangedEventHandler(_internalFileViewer_SelectedLineChanged);
            _internalFileViewer.DoubleClick += (sender, args) => OnRequestDiffView(EventArgs.Empty);

            this.HotkeysEnabled = true;
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);

            ContextMenu.Opening += ContextMenu_Opening; 
        }

        protected override void OnLoad(EventArgs e)
        {
            Font = Settings.DiffFont;
        }

        public new Font Font
        {
            set { _internalFileViewer.Font = value; }
        }

        void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ContextMenuOpening != null)
                ContextMenuOpening(sender, e);
        }

        void _internalFileViewer_MouseMove(object sender, MouseEventArgs e)
        {
            this.OnMouseMove(e);
            _internalFileViewer.FocusTextArea();
        }

        void _internalFileViewer_SelectedLineChanged(object sender, int selectedLine)
        {
            if (SelectedLineChanged != null)
                SelectedLineChanged(sender, selectedLine);
        }

        public event SelectedLineChangedEventHandler SelectedLineChanged;

        public event EventHandler ScrollPosChanged;
        public event EventHandler RequestDiffView;
        public new event EventHandler TextChanged;
        public event EventHandler TextLoaded;
        public event CancelEventHandler ContextMenuOpening;

        public ToolStripItem AddContextMenuEntry(string text, EventHandler toolStripItem_Click)
        {
            if (string.IsNullOrEmpty(text))
            {
                ToolStripSeparator separator =new ToolStripSeparator();
                ContextMenu.Items.Add(separator);
                return separator;
            }

            ToolStripItem toolStripItem = ContextMenu.Items.Add(text);
            toolStripItem.Click += toolStripItem_Click;
            return toolStripItem;
        }

        protected virtual void OnRequestDiffView(EventArgs args)
        {
            var handler = RequestDiffView;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        void _internalFileViewer_ScrollPosChanged(object sender, EventArgs e)
        {
            if (ScrollPosChanged != null)
                ScrollPosChanged(sender, e);
        }

        public void EnableScrollBars(bool enable)
        {
            _internalFileViewer.EnableScrollBars(enable);
        }

        public bool ShowLineNumbers
        {
            get { return _internalFileViewer.ShowLineNumbers; }
            set { _internalFileViewer.ShowLineNumbers = value; }
        }


        void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (patchHighlighting)
                _internalFileViewer.AddPatchHighlighting();

            if (TextChanged != null)
                TextChanged(sender, e);
        }

        public bool IsReadOnly
        {
            get { return _internalFileViewer.IsReadOnly; }
            set { _internalFileViewer.IsReadOnly = value; }
        }

        public bool IgnoreWhitespaceChanges { get; set; }
        public int NumberOfVisibleLines { get; set; }
        public bool ShowEntireFile { get; set; }
        public bool TreatAllFilesAsText { get; set; }

        public int ScrollPos
        {
            get { return _internalFileViewer.ScrollPos; }
            set { _internalFileViewer.ScrollPos = value; }
        }

        public event EventHandler<EventArgs> ExtraDiffArgumentsChanged;

        public void EnableDiffContextMenu(bool enable)
        {
            _currentViewIsPatch = enable;
            ignoreWhitespaceChangesToolStripMenuItem.Enabled = enable;
            increaseNumberOfLinesToolStripMenuItem.Enabled = enable;
            descreaseNumberOfLinesToolStripMenuItem.Enabled = enable;
            showEntireFileToolStripMenuItem.Enabled = enable;
            treatAllFilesAsTextToolStripMenuItem.Enabled = enable;
        }


        private void OnExtraDiffArgumentsChanged()
        {
            if (ExtraDiffArgumentsChanged != null)
                ExtraDiffArgumentsChanged(this, new EventArgs());
        }

        public string GetExtraDiffArguments()
        {
            var diffArguments = new StringBuilder();
            if (IgnoreWhitespaceChanges)
                diffArguments.Append(" --ignore-space-change");

            if (ShowEntireFile)
                diffArguments.AppendFormat(" --inter-hunk-context=9000 --unified=9000");
            else
                diffArguments.AppendFormat(" --unified={0}", NumberOfVisibleLines);

            if (TreatAllFilesAsText)
                diffArguments.Append(" --text");

            return diffArguments.ToString();
        }

        private void TextAreaMouseMove(object sender, MouseEventArgs e)
        {
            if (_currentViewIsPatch && !fileviewerToolbar.Visible)
            {
                fileviewerToolbar.Visible = true;
                fileviewerToolbar.Location = new Point(this.Width - fileviewerToolbar.Width - 40, 0);
                fileviewerToolbar.BringToFront();
            }
        }

        private void TextAreaMouseLeave(object sender, EventArgs e)
        {
            if (GetChildAtPoint(PointToClient(MousePosition)) != fileviewerToolbar &&
                fileviewerToolbar != null)
                fileviewerToolbar.Visible = false;
        }


        public void SaveCurrentScrollPos()
        {
            _currentScrollPos = ScrollPos;
        }

        private void RestoreCurrentScrollPos()
        {
            if (_currentScrollPos < 0)
                return;
            ScrollPos = _currentScrollPos;
            _currentScrollPos = 0;
        }


        public void ViewFile(string fileName)
        {
            ViewItem(fileName, () => GetImage(fileName), () => GetFileText(fileName));
        }

        public string GetText()
        {
            return _internalFileViewer.GetText();
        }



        public void ViewCurrentChanges(string fileName, string oldFileName, bool staged)
        {
            _async.Load(() => GitCommandHelpers.GetCurrentChanges(fileName, oldFileName, staged, GetExtraDiffArguments()), ViewStagingPatch);
        }

        public void ViewStagingPatch(string text)
        {
            ViewPatch(text);
            Reset(true, true, true);
        }

        public void ViewPatch(string text)
        {
            ResetForDiff();
            _internalFileViewer.SetText(text);
            if (TextLoaded != null)
                TextLoaded(this, null);
            RestoreCurrentScrollPos();
        }

        public void ViewStagingPatch(Func<string> loadPatchText)
        {
            ViewPatch(loadPatchText);
            Reset(true, true, true);
        }

        public void ViewPatch(Func<string> loadPatchText)
        {
            _async.Load(loadPatchText, ViewPatch);
        }

        public void ViewText(string fileName, string text)
        {
            ResetForText(fileName);

            //Check for binary file.
            if (FileHelper.IsBinaryFileAccordingToContent(text))
            {
                _internalFileViewer.SetText("Binary file: " + fileName + " (Detected)");
                if (TextLoaded != null)
                    TextLoaded(this, null);
                return;
            }

            _internalFileViewer.SetText(text);
            if (TextLoaded != null)
                TextLoaded(this, null);

            RestoreCurrentScrollPos();
        }

        public void ViewGitItemRevision(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => GitCommandHelpers.GetFileRevisionText(fileName, guid));
        }

        public void ViewGitItem(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => GitCommandHelpers.GetFileText(guid));
        }

        private void ViewItem(string fileName, Func<Image> getImage, Func<string> getFileText)
        {
            if (IsImage(fileName))
            {
                _async.Load(getImage,
                            image =>
                            {
                                ResetForImage();
                                if (image != null)
                                {
                                    if (image.Size.Height > PictureBox.Size.Height ||
                                        image.Size.Width > PictureBox.Size.Width)
                                    {
                                        PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                                    }
                                    else
                                    {
                                        PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                                    }

                                }
                                PictureBox.Image = image;
                            });
            }
            else if (IsBinaryFile(fileName))
            {
                ViewText(null, "Binary file: " + fileName);
            }
            else
            {
                _async.Load(getFileText, text => ViewText(fileName, text));
            }
        }

        private static bool IsBinaryFile(string fileName)
        {
            return FileHelper.IsBinaryFile(fileName);
        }

        private static bool IsImage(string fileName)
        {
            return FileHelper.IsImage(fileName);
        }

        private static bool IsIcon(string fileName)
        {
            return fileName.EndsWith(".ico", StringComparison.CurrentCultureIgnoreCase);
        }

        private static Image GetImage(string fileName, string guid)
        {
            try
            {
                using (var stream = GitCommandHelpers.GetFileStream(guid))
                {
                    return CreateImage(fileName, stream);
                }
            }
            catch
            {
                return null;
            }
        }

        private static Image GetImage(string fileName)
        {
            try
            {
                using (Stream stream = File.OpenRead(GitCommands.Settings.WorkingDir + fileName))
                {
                    return CreateImage(fileName, stream);
                }
            }
            catch
            {
                return null;
            }
        }

        private static Image CreateImage(string fileName, Stream stream)
        {
            if (IsIcon(fileName))
            {
                using (var icon = new Icon(stream))
                {
                    return icon.ToBitmap();
                }
            }

            return new Bitmap(CreateCopy(stream));
        }

        private static MemoryStream CreateCopy(Stream stream)
        {
            return new MemoryStream(new BinaryReader(stream).ReadBytes((int)stream.Length));
        }

        private static string GetFileText(string fileName)
        {
            string path;
            if (File.Exists(fileName))
                path = fileName;
            else
                path = GitCommands.Settings.WorkingDir + fileName;

            return !File.Exists(path) ? null : FileReader.ReadFileContent(path, GitCommands.Settings.Encoding);
        }

        private void ResetForImage()
        {
            Reset(false, false);
            _internalFileViewer.SetHighlighting("Default");
        }

        public void SetSyntax(string fileName)
        {
            EditorOptions.SetSyntax(_internalFileViewer, fileName);
        }

        private void ResetForText(string fileName)
        {
            Reset(false, true);

            if (fileName == null)
                _internalFileViewer.SetHighlighting("Default");
            else
                EditorOptions.SetSyntax(_internalFileViewer, fileName);

            if (!string.IsNullOrEmpty(fileName) &&
                (fileName.EndsWith(".diff", StringComparison.OrdinalIgnoreCase) ||
                 fileName.EndsWith(".patch", StringComparison.OrdinalIgnoreCase)))
            {
                ResetForDiff();
            }
        }
        private bool patchHighlighting = false;
        private void ResetForDiff()
        {
            Reset(true, true);
            _internalFileViewer.SetHighlighting("Patch");
            patchHighlighting = true;
        }

        private void Reset(bool diff, bool text)
        {
            Reset(diff, text, false);
        }

        private void Reset(bool diff, bool text, bool staging_diff)
        {
            patchHighlighting = diff;
            EnableDiffContextMenu(diff);
            ClearImage();
            PictureBox.Visible = !text;
            _internalFileViewer.Visible = text;
        }

        private void ClearImage()
        {
            PictureBox.ImageLocation = "";

            if (PictureBox.Image == null) return;
            PictureBox.Image.Dispose();
            PictureBox.Image = null;
        }

        private void IgnoreWhitespaceChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            IgnoreWhitespaceChanges = !IgnoreWhitespaceChanges;
            ignoreWhiteSpaces.Checked = IgnoreWhitespaceChanges;
            ignoreWhitespaceChangesToolStripMenuItem.Checked = IgnoreWhitespaceChanges;
            OnExtraDiffArgumentsChanged();
        }

        private void IncreaseNumberOfLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            NumberOfVisibleLines++;
            OnExtraDiffArgumentsChanged();
        }

        private void DescreaseNumberOfLinesToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (NumberOfVisibleLines > 0)
                NumberOfVisibleLines--;
            else
                NumberOfVisibleLines = 0;
            OnExtraDiffArgumentsChanged();
        }

        private void ShowEntireFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            showEntireFileToolStripMenuItem.Checked = !showEntireFileToolStripMenuItem.Checked;
            showEntireFileButton.Checked = showEntireFileToolStripMenuItem.Checked;

            ShowEntireFile = showEntireFileToolStripMenuItem.Checked;
            OnExtraDiffArgumentsChanged();
        }

        private void TreatAllFilesAsTextToolStripMenuItemClick(object sender, EventArgs e)
        {
            treatAllFilesAsTextToolStripMenuItem.Checked = !treatAllFilesAsTextToolStripMenuItem.Checked;
            TreatAllFilesAsText = treatAllFilesAsTextToolStripMenuItem.Checked;
            OnExtraDiffArgumentsChanged();
        }

        private void CopyToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_internalFileViewer.GetSelectedText()))
                return;

            string code;
            if (_currentViewIsPatch)
            {
                code = _internalFileViewer.GetSelectedText();

                if (code.Contains("\n") && (code[0].Equals(' ') || code[0].Equals('+') || code[0].Equals('-')))
                    code = code.Substring(1);

                code = code.Replace("\n+", "\n").Replace("\n-", "\n").Replace("\n ", "\n");
            }
            else
                code = _internalFileViewer.GetSelectedText();

            Clipboard.SetText(code);
        }

        private void CopyPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_internalFileViewer.GetSelectedText()))
            {
                Clipboard.SetText(_internalFileViewer.GetSelectedText());
            }
            else
            {
                Clipboard.SetText(_internalFileViewer.GetText());
            }
        }

        public int GetSelectionPosition()
        {
            return _internalFileViewer.GetSelectionPosition();
        }

        public int GetSelectionLength()
        {
            return _internalFileViewer.GetSelectionLength();
        }

        private void NextChangeButtonClick(object sender, EventArgs e)
        {
            var firstVisibleLine = _internalFileViewer.FirstVisibleLine;
            var totalNumberOfLines = _internalFileViewer.TotalNumberOfLines;
            var emptyLineCheck = false;

            for (var line = firstVisibleLine + 1; line < totalNumberOfLines; line++)
            {
                var lineContent = _internalFileViewer.GetLineText(line);
                if (lineContent.StartsWith("+") || lineContent.StartsWith("-"))
                {
                    if (emptyLineCheck)
                    {
                        _internalFileViewer.FirstVisibleLine = Math.Max(line - 1, 0);
                        return;
                    }
                }
                else
                {
                    emptyLineCheck = true;
                }
            }

            //Do not go to the end of the file if no change is found
            //TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = totalNumberOfLines - TextEditor.ActiveTextAreaControl.TextArea.TextView.VisibleLineCount;
        }

        public int GetLineFromVisualPosY(int visualPosY)
        {
            return _internalFileViewer.GetLineFromVisualPosY(visualPosY);
        }

        public string GetLineText(int line)
        {
            return _internalFileViewer.GetLineText(line);
        }

        public void HighlightLine(int line, Color color)
        {
            _internalFileViewer.HighlightLine(line, color);
        }

        public void ClearHighlighting()
        {
            _internalFileViewer.ClearHighlighting();
        }

        private void PreviousChangeButtonClick(object sender, EventArgs e)
        {
            var firstVisibleLine = _internalFileViewer.FirstVisibleLine;
            var emptyLineCheck = false;

            for (var line = firstVisibleLine - 1; line > 0; line--)
            {
                var lineContent = _internalFileViewer.GetLineText(line);
                if (lineContent.StartsWith("+") || lineContent.StartsWith("-"))
                {
                    emptyLineCheck = true;
                }
                else
                {
                    if (emptyLineCheck)
                    {
                        _internalFileViewer.FirstVisibleLine = line;
                        return;
                    }
                }
            }

            //Do not go to the start of the file if no change is found
            //TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = 0;
        }

        private void IncreaseNumberOfLinesClick(object sender, EventArgs e)
        {
            IncreaseNumberOfLinesToolStripMenuItemClick(null, null);
        }

        private void DecreaseNumberOfLinesClick(object sender, EventArgs e)
        {
            DescreaseNumberOfLinesToolStripMenuItemClick(null, null);
        }

        private void ShowEntireFileButtonClick(object sender, EventArgs e)
        {
            ShowEntireFileToolStripMenuItemClick(null, null);
        }

        private void ShowNonPrintCharsClick(object sender, EventArgs e)
        {
            ShowNonprintableCharactersToolStripMenuItemClick(null, null);
        }

        private void ShowNonprintableCharactersToolStripMenuItemClick(object sender, EventArgs e)
        {
            showNonprintableCharactersToolStripMenuItem.Checked = !showNonprintableCharactersToolStripMenuItem.Checked;
            showNonPrintChars.Checked = showNonprintableCharactersToolStripMenuItem.Checked;

            _internalFileViewer.ShowEOLMarkers = showNonprintableCharactersToolStripMenuItem.Checked;
            _internalFileViewer.ShowSpaces = showNonprintableCharactersToolStripMenuItem.Checked;
            _internalFileViewer.ShowTabs = showNonprintableCharactersToolStripMenuItem.Checked;
        }

        private void FindToolStripMenuItemClick(object sender, EventArgs e)
        {
            _internalFileViewer.Find();
        }

        private void ignoreWhiteSpaces_Click(object sender, EventArgs e)
        {
            IgnoreWhitespaceChangesToolStripMenuItemClick(null, null);
        }

        #region Hotkey commands

        public const string HotkeySettingsName = "FileViewer";

        internal enum Commands : int
        {
            Find,
            IncreaseNumberOfVisibleLines,
            DecreaseNumberOfVisibleLines,
            ShowEntireFile,
            TreatFileAsText

        }

        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.Find: this.FindToolStripMenuItemClick(null, null); break;
                case Commands.IncreaseNumberOfVisibleLines: this.IncreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
                case Commands.DecreaseNumberOfVisibleLines: this.DescreaseNumberOfLinesToolStripMenuItemClick(null, null); break;
                case Commands.ShowEntireFile: this.ShowEntireFileToolStripMenuItemClick(null, null); break;
                case Commands.TreatFileAsText: this.TreatAllFilesAsTextToolStripMenuItemClick(null, null); break;
                default: ExecuteScriptCommand(cmd, Keys.None); break;
            }

            return true;
        }

        #endregion

        public void Clear()
        {
            ViewText("", "");
        }

        public bool HasAnyPatches()
        {
            return (_internalFileViewer.GetText() != null && _internalFileViewer.GetText().Contains("@@"));
        }

        public void SetFileLoader(Func<Tuple<int, string>> fileLoader){
            _internalFileViewer.SetFileLoader(fileLoader);
        }

    }
}