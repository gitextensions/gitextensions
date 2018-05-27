using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ResourceManager;

namespace GitUI.Editor
{
    public partial class FileViewerInternal : GitExtensionsControl, IFileViewer
    {
        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();
        private readonly DiffViewerLineNumberCtrl _lineNumbersControl;
        private DiffHighlightService _diffHighlightService = DiffHighlightService.Instance;
        private readonly Func<GitModule> _moduleProvider;
        private bool _isGotoLineUIApplicable = true;

        public Action OpenWithDifftool { get; private set; }

        public FileViewerInternal(Func<GitModule> moduleProvider)
        {
            _moduleProvider = moduleProvider;

            InitializeComponent();
            Translate();

            TextEditor.TextChanged += TextEditor_TextChanged;
            TextEditor.ActiveTextAreaControl.VScrollBar.ValueChanged += VScrollBar_ValueChanged;

            TextEditor.ActiveTextAreaControl.TextArea.MouseMove += TextArea_MouseMove;
            TextEditor.ActiveTextAreaControl.TextArea.MouseEnter += TextArea_MouseEnter;
            TextEditor.ActiveTextAreaControl.TextArea.MouseLeave += TextArea_MouseLeave;
            TextEditor.ActiveTextAreaControl.TextArea.MouseDown += TextAreaMouseDown;
            TextEditor.ActiveTextAreaControl.TextArea.KeyUp += TextArea_KeyUp;
            TextEditor.ActiveTextAreaControl.TextArea.DoubleClick += ActiveTextAreaControlDoubleClick;

            _lineNumbersControl = new DiffViewerLineNumberCtrl(TextEditor.ActiveTextAreaControl.TextArea);

            VRulerPosition = AppSettings.DiffVerticalRulerPosition;
        }

        private void TextArea_KeyUp(object sender, KeyEventArgs e)
        {
            KeyUp?.Invoke(sender, e);
        }

        public new Font Font
        {
            get => TextEditor.Font;
            set => TextEditor.Font = value;
        }

        public new event MouseEventHandler MouseMove;
        public new event EventHandler MouseEnter;
        public new event EventHandler MouseLeave;
        public new event System.Windows.Forms.KeyEventHandler KeyUp;

        private void TextArea_MouseEnter(object sender, EventArgs e)
        {
            MouseEnter?.Invoke(sender, e);
        }

        private void TextArea_MouseLeave(object sender, EventArgs e)
        {
            MouseLeave?.Invoke(sender, e);
        }

        private void TextArea_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(sender, e);
        }

        public new event EventHandler DoubleClick;

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            DoubleClick?.Invoke(sender, e);
        }

        public void Find()
        {
            _findAndReplaceForm.ShowFor(TextEditor, false);
            VScrollBar_ValueChanged(this, null);
        }

        public async Task FindNextAsync(bool searchForwardOrOpenWithDifftool)
        {
            if (searchForwardOrOpenWithDifftool && OpenWithDifftool != null && string.IsNullOrEmpty(_findAndReplaceForm.LookFor))
            {
                OpenWithDifftool.Invoke();
                return;
            }

            await _findAndReplaceForm.FindNextAsync(viaF3: true, !searchForwardOrOpenWithDifftool, "Text not found");
            VScrollBar_ValueChanged(this, null);
        }

        private void TextAreaMouseDown(object sender, MouseEventArgs e)
        {
            OnSelectedLineChanged(TextEditor.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y));
        }

        public event EventHandler<SelectedLineEventArgs> SelectedLineChanged;

        private void OnSelectedLineChanged(int selectedLine)
        {
            SelectedLineChanged?.Invoke(this, new SelectedLineEventArgs(selectedLine));
        }

        private void VScrollBar_ValueChanged(object sender, EventArgs e)
        {
            ScrollPosChanged?.Invoke(sender, e);
        }

        private void TextEditor_TextChanged(object sender, EventArgs e)
        {
            TextChanged?.Invoke(sender, e);
        }

        #region IFileViewer Members

        public event EventHandler ScrollPosChanged;
        public new event EventHandler TextChanged;

        public string GetText()
        {
            return TextEditor.Text;
        }

        public bool ShowLineNumbers
        {
            get => TextEditor.ShowLineNumbers;
            set => TextEditor.ShowLineNumbers = value;
        }

        public void SetText(string text, Action openWithDifftool, bool isDiff)
        {
            OpenWithDifftool = openWithDifftool;
            _lineNumbersControl.Clear(isDiff);

            if (isDiff)
            {
                TextEditor.ShowLineNumbers = false;
                _lineNumbersControl.SetVisibility(true);
                var index = TextEditor.ActiveTextAreaControl.TextArea.LeftMargins.IndexOf(_lineNumbersControl);
                if (index == -1)
                {
                    TextEditor.ActiveTextAreaControl.TextArea.InsertLeftMargin(0, _lineNumbersControl);
                }

                _diffHighlightService = DiffHighlightService.IsCombinedDiff(text)
                    ? CombinedDiffHighlightService.Instance
                    : DiffHighlightService.Instance;
            }
            else
            {
                TextEditor.ShowLineNumbers = true;
                _lineNumbersControl.SetVisibility(false);
            }

            TextEditor.Text = text;
            _isGotoLineUIApplicable = !isDiff;

            if (isDiff)
            {
                _lineNumbersControl.DisplayLineNumFor(text);
            }

            TextEditor.Refresh();
        }

        public void SetHighlighting(string syntax)
        {
            TextEditor.SetHighlighting(syntax);
            TextEditor.Refresh();
        }

        public void SetHighlightingForFile(string filename)
        {
            IHighlightingStrategy highlightingStrategy;

            if (filename.EndsWith("git-rebase-todo"))
            {
                highlightingStrategy = new RebaseTodoHighlightingStrategy(_moduleProvider());
            }
            else if (filename.EndsWith("COMMIT_EDITMSG"))
            {
                highlightingStrategy = new CommitMessageHighlightingStrategy(_moduleProvider());
            }
            else
            {
                highlightingStrategy = HighlightingManager.Manager.FindHighlighterForFile(filename);
            }

            if (highlightingStrategy != null)
            {
                TextEditor.Document.HighlightingStrategy = highlightingStrategy;
            }
            else
            {
                TextEditor.SetHighlighting("XML");
            }

            TextEditor.Refresh();
        }

        public string GetSelectedText()
        {
            return TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText;
        }

        public int GetSelectionPosition()
        {
            if (TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection.Count > 0)
            {
                return TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Offset;
            }

            return TextEditor.ActiveTextAreaControl.Caret.Offset;
        }

        public int GetSelectionLength()
        {
            if (TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection.Count > 0)
            {
                return TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Length;
            }

            return 0;
        }

        public void EnableScrollBars(bool enable)
        {
            TextEditor.ActiveTextAreaControl.VScrollBar.Width = 0;
            TextEditor.ActiveTextAreaControl.VScrollBar.Visible = enable;
            TextEditor.ActiveTextAreaControl.TextArea.Dock = DockStyle.Fill;
        }

        public void AddPatchHighlighting()
        {
            _diffHighlightService.AddPatchHighlighting(TextEditor.Document);
        }

        public int ScrollPos
        {
            get { return TextEditor.ActiveTextAreaControl.VScrollBar?.Value ?? 0; }
            set
            {
                var scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;
                if (scrollBar == null)
                {
                    return;
                }

                int max = scrollBar.Maximum - scrollBar.LargeChange;
                max = Math.Max(max, scrollBar.Minimum);
                scrollBar.Value = max > value ? value : max;
            }
        }

        public bool ShowEOLMarkers
        {
            get => TextEditor.ShowEOLMarkers;
            set => TextEditor.ShowEOLMarkers = value;
        }

        public bool ShowSpaces
        {
            get => TextEditor.ShowSpaces;
            set => TextEditor.ShowSpaces = value;
        }

        public bool ShowTabs
        {
            get => TextEditor.ShowTabs;
            set => TextEditor.ShowTabs = value;
        }

        public int VRulerPosition
        {
            get => TextEditor.VRulerRow;
            set => TextEditor.VRulerRow = value;
        }

        public int FirstVisibleLine
        {
            get => TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine;
            set => TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = value;
        }

        public int GetLineFromVisualPosY(int visualPosY)
        {
            return TextEditor.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(visualPosY);
        }

        public string GetLineText(int line)
        {
            if (line >= TextEditor.Document.TotalNumberOfLines)
            {
                return string.Empty;
            }

            return TextEditor.Document.GetText(TextEditor.Document.GetLineSegment(line));
        }

        public void GoToLine(int lineNumber)
        {
            TextEditor.ActiveTextAreaControl.Caret.Position = new TextLocation(0, lineNumber);
        }

        public bool IsGotoLineUIApplicable()
        {
            return _isGotoLineUIApplicable;
        }

        public int LineAtCaret => TextEditor.ActiveTextAreaControl.Caret.Position.Line;

        public void HighlightLine(int line, Color color)
        {
            _diffHighlightService.HighlightLine(TextEditor.Document, line, color);
        }

        public void HighlightLines(int startLine, int endLine, Color color)
        {
            _diffHighlightService.HighlightLines(TextEditor.Document, startLine, endLine, color);
        }

        public void ClearHighlighting()
        {
            var document = TextEditor.Document;
            document.MarkerStrategy.RemoveAll(t => true);
        }

        public int TotalNumberOfLines => TextEditor.Document.TotalNumberOfLines;

        public bool IsReadOnly
        {
            get => TextEditor.IsReadOnly;
            set => TextEditor.IsReadOnly = value;
        }

        public void SetFileLoader(GetNextFileFnc fileLoader)
        {
            _findAndReplaceForm.SetFileLoader(fileLoader);
        }

        #endregion
    }
}