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
        public event EventHandler<SelectedLineEventArgs> SelectedLineChanged;
        public new event MouseEventHandler MouseMove;
        public new event EventHandler MouseEnter;
        public new event EventHandler MouseLeave;
        public new event System.Windows.Forms.KeyEventHandler KeyUp;
        public new event EventHandler DoubleClick;

        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();
        private readonly DiffViewerLineNumberControl _lineNumbersControl;
        private readonly Func<GitModule> _moduleProvider;

        private DiffHighlightService _diffHighlightService = DiffHighlightService.Instance;
        private bool _isGotoLineUIApplicable = true;

        public Action OpenWithDifftool { get; private set; }

        public FileViewerInternal(Func<GitModule> moduleProvider)
        {
            _moduleProvider = moduleProvider;

            InitializeComponent();
            InitializeComplete();

            TextEditor.TextChanged += (s, e) => TextChanged?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.VScrollBar.ValueChanged += (s, e) => ScrollPosChanged?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.KeyUp += (s, e) => KeyUp?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.DoubleClick += (s, e) => DoubleClick?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.MouseMove += (s, e) => MouseMove?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.MouseEnter += (s, e) => MouseEnter?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.MouseLeave += (s, e) => MouseLeave?.Invoke(s, e);
            TextEditor.ActiveTextAreaControl.TextArea.MouseDown += (s, e) =>
            {
                SelectedLineChanged?.Invoke(
                    this,
                    new SelectedLineEventArgs(
                        TextEditor.ActiveTextAreaControl.TextArea.TextView.GetLogicalLine(e.Y)));
            };

            HighlightingManager.Manager.DefaultHighlighting.SetColorFor("LineNumbers", new HighlightColor(Color.FromArgb(80, 0, 0, 0), Color.White, false, false));
            TextEditor.ActiveTextAreaControl.TextEditorProperties.EnableFolding = false;

            _lineNumbersControl = new DiffViewerLineNumberControl(TextEditor.ActiveTextAreaControl.TextArea);

            VRulerPosition = AppSettings.DiffVerticalRulerPosition;
        }

        public new Font Font
        {
            get => TextEditor.Font;
            set => TextEditor.Font = value;
        }

        public void Find()
        {
            _findAndReplaceForm.ShowFor(TextEditor, false);
            ScrollPosChanged?.Invoke(this, null);
        }

        public async Task FindNextAsync(bool searchForwardOrOpenWithDifftool)
        {
            if (searchForwardOrOpenWithDifftool && OpenWithDifftool != null && string.IsNullOrEmpty(_findAndReplaceForm.LookFor))
            {
                OpenWithDifftool.Invoke();
                return;
            }

            await _findAndReplaceForm.FindNextAsync(viaF3: true, !searchForwardOrOpenWithDifftool, "Text not found");
            ScrollPosChanged?.Invoke(this, null);
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