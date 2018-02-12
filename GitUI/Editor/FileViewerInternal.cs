using System;
using System.Drawing;
using System.Windows.Forms;
using GitUI.Editor.Diff;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ResourceManager;

namespace GitUI.Editor
{
    public partial class FileViewerInternal : GitExtensionsControl, IFileViewer
    {
        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();
        private DiffHighlightService _diffHighlightService = DiffHighlightService.Instance;
        private readonly DiffViewerLineNumberCtrl _lineNumbersControl;
        private bool _isGotoLineUIApplicable = true;

        public FileViewerInternal()
        {
            InitializeComponent();
            Translate();

            TextEditor.TextChanged += TextEditor_TextChanged;
            TextEditor.ActiveTextAreaControl.VScrollBar.ValueChanged += VScrollBar_ValueChanged;

            TextEditor.ActiveTextAreaControl.TextArea.MouseMove += TextArea_MouseMove;
            TextEditor.ActiveTextAreaControl.TextArea.MouseEnter += TextArea_MouseEnter;
            TextEditor.ActiveTextAreaControl.TextArea.MouseLeave += TextArea_MouseLeave;
            TextEditor.ActiveTextAreaControl.TextArea.MouseDown += TextAreaMouseDown;
            TextEditor.KeyDown += BlameFileKeyUp;
            TextEditor.ActiveTextAreaControl.TextArea.KeyDown += BlameFileKeyUp;
            TextEditor.ActiveTextAreaControl.TextArea.DoubleClick += ActiveTextAreaControlDoubleClick;

            _lineNumbersControl = new DiffViewerLineNumberCtrl(TextEditor.ActiveTextAreaControl.TextArea);

            VRulerPosition = GitCommands.AppSettings.DiffVerticalRulerPosition;
        }

        public new Font Font
        {
            get { return TextEditor.Font; }
            set { TextEditor.Font = value; }
        }

        public new event MouseEventHandler MouseMove;
        public new event EventHandler MouseEnter;
        public new event EventHandler MouseLeave;

        void TextArea_MouseEnter(object sender, EventArgs e)
        {
            if (MouseEnter != null)
                MouseEnter(sender, e);
        }

        void TextArea_MouseLeave(object sender, EventArgs e)
        {
            if (MouseLeave != null)
                MouseLeave(sender, e);
        }

        void TextArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseMove != null)
                MouseMove(sender, e);
        }

        public new event EventHandler DoubleClick;

        private void ActiveTextAreaControlDoubleClick(object sender, EventArgs e)
        {
            if (DoubleClick != null)
                DoubleClick(sender, e);
        }

        private void BlameFileKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.F)
                Find();

            if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.F3)
                _findAndReplaceForm.FindNext(true, true, "Text not found");
            else if (e.KeyCode == Keys.F3)
                _findAndReplaceForm.FindNext(true, false, "Text not found");

            VScrollBar_ValueChanged(this, e);
        }

        public void Find()
        {
            _findAndReplaceForm.ShowFor(TextEditor, false);
        }

        private void TextAreaMouseDown(object sender, MouseEventArgs e)
        {
            OnSelectedLineChanged(TextEditor.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y));
        }

        public event EventHandler<SelectedLineEventArgs> SelectedLineChanged;

        void OnSelectedLineChanged(int selectedLine)
        {
            if (SelectedLineChanged != null)
                SelectedLineChanged(this, new SelectedLineEventArgs(selectedLine));
        }

        void VScrollBar_ValueChanged(object sender, EventArgs e)
        {
            if (ScrollPosChanged != null)
                ScrollPosChanged(sender, e);
        }

        void TextEditor_TextChanged(object sender, EventArgs e)
        {
            if (TextChanged != null)
                TextChanged(sender, e);
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
            get { return TextEditor.ShowLineNumbers; }
            set { TextEditor.ShowLineNumbers = value; }
        }

        public void SetText(string text, bool isDiff = false)
        {
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
            var strategy = CustomSyntaxStrategyManager.TryFindCustomSyntaxStrategy(syntax);
            if (strategy != null)
                TextEditor.Document.HighlightingStrategy = strategy;
            else
                TextEditor.SetHighlighting(syntax);
            TextEditor.Refresh();
        }

        public void SetHighlightingForFile(string filename)
        {
            IHighlightingStrategy highlightingStrategy = HighlightingManager.Manager.FindHighlighterForFile(filename);
            if (highlightingStrategy != null)
                TextEditor.Document.HighlightingStrategy = highlightingStrategy;
            else
                TextEditor.SetHighlighting("XML");
            TextEditor.Refresh();
        }

        public string GetSelectedText()
        {
            return TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText;
        }

        public int GetSelectionPosition()
        {
            if (TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection.Count > 0)
                return TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Offset;

            return TextEditor.ActiveTextAreaControl.Caret.Offset;
        }

        public int GetSelectionLength()
        {
            if (TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection.Count > 0)
                return TextEditor.ActiveTextAreaControl.SelectionManager.SelectionCollection[0].Length;

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
            get
            {
                return TextEditor.ShowEOLMarkers;
            }
            set
            {
                TextEditor.ShowEOLMarkers = value;
            }
        }

        public bool ShowSpaces
        {
            get
            {
                return TextEditor.ShowSpaces;
            }
            set
            {
                TextEditor.ShowSpaces = value;
            }
        }

        public bool ShowTabs
        {
            get
            {
                return TextEditor.ShowTabs;
            }
            set
            {
                TextEditor.ShowTabs = value;
            }
        }

        public int VRulerPosition
        {
            get
            {
                return TextEditor.VRulerRow;
            }
            set
            {
                TextEditor.VRulerRow = value;
            }
        }

        public int FirstVisibleLine
        {
            get
            {
                return TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine;
            }
            set
            {
                TextEditor.ActiveTextAreaControl.TextArea.TextView.FirstVisibleLine = value;
            }
        }

        public int GetLineFromVisualPosY(int visualPosY)
        {
            return TextEditor.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(visualPosY);
        }

        public string GetLineText(int line)
        {
            if (line >= TextEditor.Document.TotalNumberOfLines)
                return string.Empty;

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

        public int LineAtCaret
        {
            get
            {
                return TextEditor.ActiveTextAreaControl.Caret.Position.Line;
            }
        }

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

        public int TotalNumberOfLines
        {
            get { return TextEditor.Document.TotalNumberOfLines; }
        }

        public void FocusTextArea()
        {
            TextEditor.ActiveTextAreaControl.TextArea.Select();
        }

        public bool IsReadOnly
        {
            get
            {
                return TextEditor.IsReadOnly;
            }
            set
            {
                TextEditor.IsReadOnly = value;
            }
        }

        public void SetFileLoader(Func<bool, Tuple<int, string>> fileLoader)
        {
            _findAndReplaceForm.SetFileLoader(fileLoader);
        }

        #endregion
    }
}