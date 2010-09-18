using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using GitCommands;
using ICSharpCode.TextEditor.Util;

namespace GitUI.Editor
{
    public partial class FileViewerMono : GitExtensionsControl, IFileViewer
    {
        private readonly AsyncLoader _async;
        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();
        private int _currentScrollPos = -1;
        private bool _currentViewIsPatch;

        public FileViewerMono()
        {
            TreatAllFilesAsText = false;
            ShowEntireFile = false;
            NumberOfVisibleLines = 3;
            InitializeComponent();
            Translate();
            TextEditor.KeyDown += TextAreaKeyUp;
            IgnoreWhitespaceChanges = false;

            _async = new AsyncLoader();
            _async.LoadingError +=
                (sender, args) =>
                    {
                        ResetForText(null);
                        TextEditor.Text = "Unsupported file";
                        TextEditor.Refresh();
                    };


            TextEditor.MouseMove += TextAreaMouseMove;
            TextEditor.MouseLeave += TextAreaMouseLeave;

            IsReadOnly = true;
            TextEditor.TextChanged += new EventHandler(TextEditor_TextChanged);
        }

        void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (patchHighlighting)
                AddPatchHighlighting();
        }

        public bool IsReadOnly
        {
            get { return TextEditor.ReadOnly;  }
            set { TextEditor.ReadOnly = value; }
        }

        public bool IgnoreWhitespaceChanges { get; set; }
        public int NumberOfVisibleLines { get; set; }
        public bool ShowEntireFile { get; set; }
        public bool TreatAllFilesAsText { get; set; }

        public int ScrollPos
        {
            get { return 0; }
            set
            {
            }
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
            }
        }

        private void TextAreaMouseLeave(object sender, EventArgs e)
        {
            if (GetChildAtPoint(PointToClient(MousePosition)) != fileviewerToolbar &&
                fileviewerToolbar != null)
                fileviewerToolbar.Visible = false;
        }

        private void TextAreaKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
                Find();

            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.F3)
                _findAndReplaceForm.FindNext(true, true, "Text not found");
            else if (e.KeyCode == Keys.F3)
                _findAndReplaceForm.FindNext(true, false, "Text not found");
        }

        public void SaveCurrentScrollPos()
        {
            _currentScrollPos = 0;
        }

        private void RestoreCurrentScrollPos()
        {
            if (_currentScrollPos < 0)
                return;
            ScrollPos = _currentScrollPos;
            _currentScrollPos = 0;
        }

        public void Find()
        {
        }

        public void ViewFile(string fileName)
        {
            ViewItem(fileName, () => GetImage(fileName), () => GetFileText(fileName));
        }

        public string GetText()
        {
            return TextEditor.Text;
        }

        public void ViewCurrentChanges(string fileName, bool staged)
        {
            _async.Load(() => GitCommands.GitCommands.GetCurrentChanges(fileName, staged, GetExtraDiffArguments()), ViewPatch);
        }

        public void ViewPatch(string text)
        {
            ResetForDiff();
            TextEditor.Text = text;
            TextEditor.Refresh();
            RestoreCurrentScrollPos();
        }

        public void ViewPatch(Func<string> loadPatchText)
        {
            _async.Load(loadPatchText, ViewPatch);
        }

        private void AddExtraPatchHighlighting()
        {

        }

        private void AddPatchHighlighting()
        {
            AddExtraPatchHighlighting();
        }

        public void ViewText(string fileName, string text)
        {
            ResetForText(fileName);
            TextEditor.Text = text;
            TextEditor.Refresh();
            RestoreCurrentScrollPos();
        }

        public void ViewGitItemRevision(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => GitCommands.GitCommands.GetFileRevisionText(fileName, guid));
        }

        public void ViewGitItem(string fileName, string guid)
        {
            ViewItem(fileName, () => GetImage(fileName, guid), () => GitCommands.GitCommands.GetFileText(guid));
        }

        private void ViewItem(string fileName, Func<Image> getImage, Func<string> getFileText)
        {
            if (IsImage(fileName))
            {
                _async.Load(getImage,
                            image =>
                                {
                                    ResetForImage();

                                    if (image.Size.Height > PictureBox.Size.Height ||
                                        image.Size.Width > PictureBox.Size.Width)
                                    {
                                        PictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                                    }
                                    else
                                    {
                                        PictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
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
            using (var stream = GitCommands.GitCommands.GetFileStream(guid))
            {
                return CreateImage(fileName, stream);
            }
        }

        private static Image GetImage(string fileName)
        {
            using (Stream stream = File.OpenRead(Settings.WorkingDir + fileName))
            {
                return CreateImage(fileName, stream);
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
            return new MemoryStream(new BinaryReader(stream).ReadBytes((int) stream.Length));
        }

        private static string GetFileText(string fileName)
        {
            string path; 
            if (File.Exists(fileName))
                path = fileName;
            else
                path = Settings.WorkingDir + fileName;

            return !File.Exists(path) ? null : FileReader.ReadFileContent(path, Settings.Encoding);
        }

        private void ResetForImage()
        {
            Reset(false, false);
            //TextEditor.SetHighlighting("Default");
        }

        private void ResetForText(string fileName)
        {
            Reset(false, true);

            if (fileName.EndsWith(".diff", StringComparison.OrdinalIgnoreCase) ||
                fileName.EndsWith(".patch", StringComparison.OrdinalIgnoreCase))
            {
                ResetForDiff();
            }
        }
        private bool patchHighlighting = false;
        private void ResetForDiff()
        {
            Reset(true, true);
            patchHighlighting = true;
        }

        private void Reset(bool diff, bool text)
        {
            patchHighlighting = diff;
            EnableDiffContextMenu(diff);
            ClearImage();
            PictureBox.Visible = !text;
            TextEditor.Visible = text;
        }

        private void ClearImage()
        {
            PictureBox.ImageLocation = "";

            if (PictureBox.Image == null) return;
            PictureBox.Image.Dispose();
            PictureBox.Image = null;
        }
    }
}
