using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI
{
    public delegate bool GetNextFileFnc(bool seekBackward, bool loop, out int fileIndex, out Task loadFileContent);

    public partial class FindAndReplaceForm : GitExtensionsForm
    {
        private readonly TranslationString _findAndReplaceString =
            new TranslationString("Find & replace");
        private readonly TranslationString _findString =
            new TranslationString("Find");
        private readonly TranslationString _selectionOnlyString =
            new TranslationString("selection only");
        private readonly TranslationString _textNotFoundString =
            new TranslationString("Text not found");
        private readonly TranslationString _noSearchString =
            new TranslationString("No string specified to look for!");
        private readonly TranslationString _textNotFoundString2 =
            new TranslationString("Search text not found.");
        private readonly TranslationString _notFoundString =
            new TranslationString("Not found");
        private readonly TranslationString _noOccurrencesFoundString =
            new TranslationString("No occurrences found.");
        private readonly TranslationString _replacedOccurrencesString =
            new TranslationString("Replaced {0} occurrences.");

        private readonly Dictionary<TextEditorControl, HighlightGroup> _highlightGroups =
            new Dictionary<TextEditorControl, HighlightGroup>();

        private readonly TextEditorSearcher _search;
        private TextEditorControl _editor;
        private bool _lastSearchLoopedAround;
        private bool _lastSearchWasBackward;
        private GetNextFileFnc _fileLoader;

        public FindAndReplaceForm()
        {
            InitializeComponent();
            InitializeComplete();
            _search = new TextEditorSearcher();

            ShowInTaskbar = false;
        }

        private TextEditorControl Editor
        {
            set
            {
                _editor = value;
                _search.Document = _editor.Document;
                UpdateTitleBar();
            }
        }

        public bool ReplaceMode
        {
            get { return txtReplaceWith.Visible; }
            set
            {
                btnReplace.Visible = btnReplaceAll.Visible = value;
                lblReplaceWith.Visible = txtReplaceWith.Visible = value;
                btnHighlightAll.Visible = !value;
                AcceptButton = value ? btnReplace : btnFindNext;
                UpdateTitleBar();
            }
        }

        public string LookFor => txtLookFor.Text;

        private void UpdateTitleBar()
        {
            string text = ReplaceMode ? _findAndReplaceString.Text : _findString.Text;

            if (_editor?.FileName != null)
            {
                text += " - " + Path.GetFileName(_editor.FileName);
            }

            if (_search.HasScanRegion)
            {
                text += " (" + _selectionOnlyString.Text + ")";
            }

            Text = text;
        }

        public void ShowFor(TextEditorControl editor, bool replaceMode)
        {
            Editor = editor;

            _search.ClearScanRegion();
            SelectionManager sm = editor.ActiveTextAreaControl.SelectionManager;
            if (sm.HasSomethingSelected && sm.SelectionCollection.Count == 1)
            {
                ISelection sel = sm.SelectionCollection[0];
                if (sel.StartPosition.Line == sel.EndPosition.Line)
                {
                    txtLookFor.Text = sm.SelectedText;
                }
                else
                {
                    _search.SetScanRegion(sel);
                }
            }
            else
            {
                // Get the current word that the caret is on
                Caret caret = editor.ActiveTextAreaControl.Caret;
                int start = TextUtilities.FindWordStart(editor.Document, caret.Offset);
                int endAt = TextUtilities.FindWordEnd(editor.Document, caret.Offset);
                txtLookFor.Text = editor.Document.GetText(start, endAt - start);
            }

            ReplaceMode = replaceMode;

            Owner = (Form)editor.TopLevelControl;
            Location = new Point(Owner.Location.X + 100, Owner.Location.Y + 100);
            Show();

            txtLookFor.SelectAll();
            txtLookFor.Focus();
        }

        private async void btnFindPrevious_Click(object sender, EventArgs e)
        {
            await FindNextAsync(false, true, _textNotFoundString.Text);
        }

        private async void btnFindNext_Click(object sender, EventArgs e)
        {
            await FindNextAsync(false, false, _textNotFoundString.Text);
        }

        [ItemCanBeNull]
        public async Task<TextRange> FindNextAsync(bool viaF3, bool searchBackward, string messageIfNotFound)
        {
            if (string.IsNullOrEmpty(txtLookFor.Text))
            {
                MessageBox.Show(this, _noSearchString.Text, Text, MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return null;
            }

            _lastSearchWasBackward = searchBackward;
            _search.LookFor = txtLookFor.Text;
            _search.MatchCase = chkMatchCase.Checked;
            _search.MatchWholeWordOnly = chkMatchWholeWord.Checked;

            int startIdx = -1;
            int currentIdx = -1;
            TextRange range;
            do
            {
                Caret caret = _editor.ActiveTextAreaControl.Caret;
                if (viaF3 && _search.HasScanRegion &&
                    !Globals.IsInRange(caret.Offset, _search.BeginOffset, _search.EndOffset))
                {
                    // user moved outside of the originally selected region
                    _search.ClearScanRegion();
                    UpdateTitleBar();
                }

                int startFrom = caret.Offset - (searchBackward ? 1 : 0);
                if (startFrom == -1)
                {
                    startFrom = _search.EndOffset;
                }

                range = _search.FindNext(startFrom, searchBackward, out _lastSearchLoopedAround);
                if (range != null && (!_lastSearchLoopedAround || _fileLoader == null))
                {
                    SelectResult(range);
                }
                else if (_fileLoader != null)
                {
                    range = null;
                    if (currentIdx != -1 && startIdx == -1)
                    {
                        startIdx = currentIdx;
                    }

                    if (_fileLoader(searchBackward, true, out var fileIndex, out var loadFileContent))
                    {
                        currentIdx = fileIndex;
                        try
                        {
                            await loadFileContent;
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            while (range == null && startIdx != currentIdx && currentIdx != -1);
            if (range == null && messageIfNotFound != null)
            {
                MessageBox.Show(this, messageIfNotFound, " ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            return range;
        }

        private void SelectResult(TextRange range)
        {
            TextLocation p1 = _editor.Document.OffsetToPosition(range.Offset);
            TextLocation p2 = _editor.Document.OffsetToPosition(range.Offset + range.Length);
            _editor.ActiveTextAreaControl.SelectionManager.SetSelection(p1, p2);
            _editor.ActiveTextAreaControl.ScrollTo(p1.Line, p1.Column);

            // Also move the caret to the end of the selection, because when the user
            // presses F3, the caret is where we start searching next time.
            _editor.ActiveTextAreaControl.Caret.Position = p2;
        }

        private void btnHighlightAll_Click(object sender, EventArgs e)
        {
            if (!_highlightGroups.ContainsKey(_editor))
            {
                _highlightGroups[_editor] = new HighlightGroup(_editor);
            }

            HighlightGroup group = _highlightGroups[_editor];

            if (string.IsNullOrEmpty(LookFor))
            {
                // Clear highlights
                group.ClearMarkers();
            }
            else
            {
                group.ClearMarkers();
                _search.LookFor = txtLookFor.Text;
                _search.MatchCase = chkMatchCase.Checked;
                _search.MatchWholeWordOnly = chkMatchWholeWord.Checked;

                int offset = 0, count = 0;
                for (; ;)
                {
                    TextRange range = _search.FindNext(offset, false, out var looped);
                    if (range == null || looped)
                    {
                        break;
                    }

                    offset = range.Offset + range.Length;
                    count++;

                    var m = new TextMarker(range.Offset, range.Length,
                                           TextMarkerType.SolidBlock, Color.Yellow, Color.Black);
                    group.AddMarker(m);
                }

                if (count == 0)
                {
                    MessageBox.Show(this, _textNotFoundString2.Text, _notFoundString.Text, MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }
                else
                {
                    Close();
                }
            }
        }

        private void FindAndReplaceForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Prevent dispose, as this form can be re-used
            if (e.CloseReason != CloseReason.FormOwnerClosing)
            {
                Owner?.Select(); // prevent another app from being activated instead

                e.Cancel = true;
                Hide();

                // Discard search region
                _search.ClearScanRegion();
                _editor.Refresh(); // must repaint manually
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void btnReplace_Click(object sender, EventArgs e)
        {
            SelectionManager sm = _editor.ActiveTextAreaControl.SelectionManager;
            if (string.Equals(sm.SelectedText, txtLookFor.Text, StringComparison.OrdinalIgnoreCase))
            {
                InsertText(txtReplaceWith.Text);
            }

            await FindNextAsync(false, _lastSearchWasBackward, _textNotFoundString.Text);
        }

        private void btnReplaceAll_Click(object sender, EventArgs e)
        {
            int count = 0;

            // BUG FIX: if the replacement string contains the original search string
            // (e.g. replace "red" with "very red") we must avoid looping around and
            // replacing forever! To fix, start replacing at beginning of region (by
            // moving the caret) and stop as soon as we loop around.
            _editor.ActiveTextAreaControl.Caret.Position =
                _editor.Document.OffsetToPosition(_search.BeginOffset);

            _editor.Document.UndoStack.StartUndoGroup();
            try
            {
                while (FindNextAsync(false, false, null) != null)
                {
                    if (_lastSearchLoopedAround)
                    {
                        break;
                    }

                    // Replace
                    count++;
                    InsertText(txtReplaceWith.Text);
                }
            }
            finally
            {
                _editor.Document.UndoStack.EndUndoGroup();
            }

            if (count == 0)
            {
                MessageBox.Show(this, _noOccurrencesFoundString.Text);
            }
            else
            {
                MessageBox.Show(this, string.Format(_replacedOccurrencesString.Text, count));
                Close();
            }
        }

        private void InsertText(string text)
        {
            TextArea textArea = _editor.ActiveTextAreaControl.TextArea;
            textArea.Document.UndoStack.StartUndoGroup();
            try
            {
                if (textArea.SelectionManager.HasSomethingSelected)
                {
                    textArea.Caret.Position = textArea.SelectionManager.SelectionCollection[0].StartPosition;
                    textArea.SelectionManager.RemoveSelectedText();
                }

                textArea.InsertString(text);
            }
            finally
            {
                textArea.Document.UndoStack.EndUndoGroup();
            }
        }

        internal void SetFileLoader(GetNextFileFnc fileLoader)
        {
            _fileLoader = fileLoader;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _search.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    public class TextRange : AbstractSegment
    {
        public TextRange(IDocument document, int offset, int length)
        {
            this.offset = offset;
            this.length = length;
        }
    }

    /// <summary>This class finds occurrences of a search string in a text
    /// editor's IDocument... it's like Find box without a GUI.</summary>
    public sealed class TextEditorSearcher : IDisposable
    {
        public bool MatchCase;

        public bool MatchWholeWordOnly;
        private IDocument _document;
        private string _lookFor2; // uppercase in case-insensitive mode

        // I would have used the TextAnchor class to represent the beginning and
        // end of the region to scan while automatically adjusting to changes in
        // the document--but for some reason it is sealed and its constructor is
        // internal. Instead I use a TextMarker, which is perhaps even better as
        // it gives me the opportunity to highlight the region. Note that all the
        // markers and coloring information is associated with the text document,
        // not the editor control, so TextEditorSearcher doesn't need a reference
        // to the TextEditorControl. After adding the marker to the document, we
        // must remember to remove it when it is no longer needed.
        private TextMarker _region;

        public IDocument Document
        {
            get { return _document; }
            set
            {
                if (_document != value)
                {
                    ClearScanRegion();
                    _document = value;
                }
            }
        }

        public bool HasScanRegion => _region != null;

        /// <summary>Begins the start offset for searching</summary>
        public int BeginOffset
        {
            get
            {
                if (_region != null)
                {
                    return _region.Offset;
                }

                return 0;
            }
        }

        /// <summary>Begins the end offset for searching</summary>
        public int EndOffset
        {
            get
            {
                if (_region != null)
                {
                    return _region.EndOffset;
                }

                return _document.TextLength;
            }
        }

        public string LookFor { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            ClearScanRegion();
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>Sets the region to search. The region is updated
        /// automatically as the document changes.</summary>
        public void SetScanRegion(ISelection sel)
        {
            SetScanRegion(sel.Offset, sel.Length);
        }

        /// <summary>Sets the region to search. The region is updated
        /// automatically as the document changes.</summary>
        public void SetScanRegion(int offset, int length)
        {
            Color bkgColor = _document.HighlightingStrategy.GetColorFor("Default").BackgroundColor;
            _region = new TextMarker(offset, length, TextMarkerType.SolidBlock,
                                     Globals.HalfMix(bkgColor, Color.FromArgb(160, 160, 160)));
            _document.MarkerStrategy.AddMarker(_region);
        }

        public void ClearScanRegion()
        {
            if (_region != null)
            {
                _document.MarkerStrategy.RemoveMarker(_region);
                _region = null;
            }
        }

        ~TextEditorSearcher()
        {
            Dispose();
        }

        /// <summary>Finds next instance of LookFor, according to the search rules
        /// (MatchCase, MatchWholeWordOnly).</summary>
        /// <param name="beginAtOffset">Offset in Document at which to begin the search</param>
        /// <remarks>If there is a match at beginAtOffset precisely, it will be returned.</remarks>
        /// <returns>Region of document that matches the search string</returns>
        public TextRange FindNext(int beginAtOffset, bool searchBackward, out bool loopedAround)
        {
            Debug.Assert(!string.IsNullOrEmpty(LookFor), "!string.IsNullOrEmpty(LookFor)");
            loopedAround = false;

            int startAt = BeginOffset, endAt = EndOffset;
            int curOffs = Globals.InRange(beginAtOffset, startAt, endAt);

            _lookFor2 = MatchCase ? LookFor : LookFor.ToUpperInvariant();

            TextRange result;
            if (searchBackward)
            {
                result = FindNextIn(startAt, curOffs, true);
                if (result == null)
                {
                    loopedAround = true;
                    result = FindNextIn(curOffs, endAt, true);
                }
            }
            else
            {
                result = FindNextIn(curOffs, endAt, false);
                if (result == null)
                {
                    loopedAround = true;
                    result = FindNextIn(startAt, curOffs, false);
                }
            }

            return result;
        }

        private static bool MatchFirstCh(char a, char b, bool matchCase)
        {
            if (a == b)
            {
                return true;
            }

            if (!matchCase && a == char.ToUpperInvariant(b))
            {
                return true;
            }

            return false;
        }

        [CanBeNull]
        private TextRange FindNextIn(int offset1, int offset2, bool searchBackward)
        {
            Debug.Assert(offset2 >= offset1, "offset2 >= offset1");
            offset2 -= LookFor.Length;

            // Search
            char lookForCh = _lookFor2[0];
            if (searchBackward)
            {
                for (int offset = offset2; offset >= offset1; offset--)
                {
                    if (MatchFirstCh(lookForCh, _document.GetCharAt(offset), MatchCase)
                        &&
                        (IsWholeWordMatch(offset) ||
                         (!MatchWholeWordOnly && IsPartWordMatch(offset))))
                    {
                        return new TextRange(_document, offset, LookFor.Length);
                    }
                }
            }
            else
            {
                for (int offset = offset1; offset <= offset2; offset++)
                {
                    if (MatchFirstCh(lookForCh, _document.GetCharAt(offset), MatchCase)
                        &&
                        (IsWholeWordMatch(offset) ||
                         (!MatchWholeWordOnly && IsPartWordMatch(offset))))
                    {
                        return new TextRange(_document, offset, LookFor.Length);
                    }
                }
            }

            return null;
        }

        private bool IsWholeWordMatch(int offset)
        {
            if (IsWordBoundary(offset) && IsWordBoundary(offset + LookFor.Length))
            {
                return IsPartWordMatch(offset);
            }

            return false;
        }

        private bool IsWordBoundary(int offset)
        {
            return offset <= 0 || offset >= _document.TextLength ||
                   !IsAlphaNumeric(offset - 1) || !IsAlphaNumeric(offset);
        }

        private bool IsAlphaNumeric(int offset)
        {
            char c = _document.GetCharAt(offset);
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private bool IsPartWordMatch(int offset)
        {
            string substr = _document.GetText(offset, LookFor.Length);
            if (!MatchCase)
            {
                substr = substr.ToUpperInvariant();
            }

            return substr == _lookFor2;
        }
    }

    /// <summary>Bundles a group of markers together so that they can be cleared
    /// together.</summary>
    public sealed class HighlightGroup : IDisposable
    {
        private readonly IDocument _document;
        private readonly TextEditorControl _editor;
        private readonly List<TextMarker> _markers = new List<TextMarker>();

        public HighlightGroup(TextEditorControl editor)
        {
            _editor = editor;
            _document = editor.Document;
        }

        #region IDisposable Members

        public void Dispose()
        {
            ClearMarkers();
            GC.SuppressFinalize(this);
        }

        #endregion

        public void AddMarker(TextMarker marker)
        {
            _markers.Add(marker);
            _document.MarkerStrategy.AddMarker(marker);
        }

        public void ClearMarkers()
        {
            foreach (TextMarker m in _markers)
            {
                _document.MarkerStrategy.RemoveMarker(m);
            }

            _markers.Clear();
            _editor.Refresh();
        }

        ~HighlightGroup()
        {
            Dispose();
        }
    }

    public static class Globals
    {
        public static int InRange(int x, int lo, int hi)
        {
            Debug.Assert(lo <= hi, "lo <= hi");
            return x < lo ? lo : (x > hi ? hi : x);
        }

        public static bool IsInRange(int x, int lo, int hi)
        {
            return x >= lo && x <= hi;
        }

        public static Color HalfMix(Color one, Color two)
        {
            return Color.FromArgb(
                (one.A + two.A) >> 1,
                (one.R + two.R) >> 1,
                (one.G + two.G) >> 1,
                (one.B + two.B) >> 1);
        }
    }
}