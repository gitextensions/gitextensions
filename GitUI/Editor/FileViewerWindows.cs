using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    public partial class FileViewerWindows : GitExtensionsControl, IFileViewer
    {
        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();

        public FileViewerWindows()
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

        public event SelectedLineChangedEventHandler SelectedLineChanged;

        void OnSelectedLineChanged(int selectedLine)
        {
            if (SelectedLineChanged != null)
                SelectedLineChanged(this, selectedLine);
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

        public void SetText(string text)
        {
            TextEditor.Text = text;
            TextEditor.Refresh();
        }

        public void SetHighlighting(string syntax)
        {
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

        private List<LineSegment> GetLinesStartingWith(IDocument document, ref int beginIndex, char startingChar, ref bool found)
        {
            List<LineSegment> result = new List<LineSegment>();

            while (beginIndex < document.TotalNumberOfLines)
            {
                var lineSegment = document.GetLineSegment(beginIndex);

                if (lineSegment.Length > 0 && document.GetCharAt(lineSegment.Offset) == startingChar)
                {
                    found = true;
                    result.Add(lineSegment);
                    beginIndex++;
                }
                else
                {
                    if (found)
                        break;
                    else
                        beginIndex++;
                }
            }

            return result;
        }

        private void MarkDifference(IDocument document, List<LineSegment> linesRemoved, List<LineSegment> linesAdded, int beginOffset)
        {            
            int count = Math.Min(linesRemoved.Count, linesAdded.Count);

            for (int i = 0; i < count; i++)
                MarkDifference(document, linesRemoved[i], linesAdded[i], beginOffset);        
        }

        private void MarkDifference(IDocument document, LineSegment lineRemoved, LineSegment lineAdded, int beginOffset)
        {
            var lineRemovedEndOffset = lineRemoved.Length;
            var lineAddedEndOffset = lineAdded.Length;
            var endOffsetMin = Math.Min(lineRemovedEndOffset, lineAddedEndOffset);
            var reverseOffset = 0;

            while (beginOffset < endOffsetMin)
            {
                if (!document.GetCharAt(lineAdded.Offset + beginOffset).Equals(
                        document.GetCharAt(lineRemoved.Offset + beginOffset)))
                    break;

                beginOffset++;
            }

            while (lineAddedEndOffset > beginOffset && lineRemovedEndOffset > beginOffset)
            {
                reverseOffset = lineAdded.Length - lineAddedEndOffset;

                if (!document.GetCharAt(lineAdded.Offset + lineAdded.Length - 1 - reverseOffset).
                         Equals(document.GetCharAt(lineRemoved.Offset + lineRemoved.Length - 1 -
                                                   reverseOffset)))
                    break;

                lineRemovedEndOffset--;
                lineAddedEndOffset--;
            }

            Color color;
            var markerStrategy = document.MarkerStrategy;

            if (lineAdded.Length - beginOffset - reverseOffset > 0)
            {
                color = AppSettings.DiffAddedExtraColor;
                markerStrategy.AddMarker(new TextMarker(lineAdded.Offset + beginOffset,
                                                        lineAdded.Length - beginOffset - reverseOffset,
                                                        TextMarkerType.SolidBlock, color,
                                                        ColorHelper.GetForeColorForBackColor(color)));
            }

            if (lineRemoved.Length - beginOffset - reverseOffset > 0)
            {
                color = AppSettings.DiffRemovedExtraColor;
                markerStrategy.AddMarker(new TextMarker(lineRemoved.Offset + beginOffset,
                                                        lineRemoved.Length - beginOffset - reverseOffset,
                                                        TextMarkerType.SolidBlock, color,
                                                        ColorHelper.GetForeColorForBackColor(color)));
            }

        }


        private void AddExtraPatchHighlighting()
        {
            var document = TextEditor.Document;      

            var line = 0;

            bool found = false;
            int numberOfParents;
            var linesRemoved = GetLinesStartingWith(document, ref line, '-', ref found);
            var linesAdded = GetLinesStartingWith(document, ref line, '+', ref found);
            if (linesAdded.Count == 1 && linesRemoved.Count == 1)
            {
                var lineA = linesRemoved[0];
                var lineB = linesAdded[0];
                if (lineA.Length > 4 && lineB.Length > 4 &&
                    document.GetCharAt(lineA.Offset + 4) == 'a' &&
                    document.GetCharAt(lineB.Offset + 4) == 'b')
                    numberOfParents = 5;
                else
                    numberOfParents = 4;

                MarkDifference(document, linesRemoved, linesAdded, numberOfParents);
            }
            
            numberOfParents = 1;
            while (line < document.TotalNumberOfLines)
            {
                found = false;
                linesRemoved = GetLinesStartingWith(document, ref line, '-', ref found);
                linesAdded = GetLinesStartingWith(document, ref line, '+', ref found);

                MarkDifference(document, linesRemoved, linesAdded, numberOfParents);                
            }
        }

        public void EnableScrollBars(bool enable)
        {
            TextEditor.ActiveTextAreaControl.VScrollBar.Width = 0;
            TextEditor.ActiveTextAreaControl.VScrollBar.Visible = enable;
            TextEditor.ActiveTextAreaControl.TextArea.Dock = DockStyle.Fill;
        }

        private void ProcessLineSegment(ref int line, LineSegment lineSegment, char signChar, Color color)
        {
            var document = TextEditor.Document;
            if (document.GetCharAt(lineSegment.Offset) == signChar)
            {
                var endLine = document.GetLineSegment(line);

                for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == signChar; line++)
                {
                    endLine = document.GetLineSegment(line);
                }
                line--;
                line--;
                endLine = document.GetLineSegment(line);

                document.MarkerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                        (endLine.Offset + endLine.TotalLength) -
                                                        lineSegment.Offset, TextMarkerType.SolidBlock, color,
                                                        ColorHelper.GetForeColorForBackColor(color)));
            }
        }

        public void AddPatchHighlighting()
        {
            var document = TextEditor.Document;
            var markerStrategy = document.MarkerStrategy;
            markerStrategy.RemoveAll(m => true);
            bool forceAbort = false;

            AddExtraPatchHighlighting();

            for (var line = 0; line < document.TotalNumberOfLines && !forceAbort; line++)
            {
                var lineSegment = document.GetLineSegment(line);

                if (lineSegment.TotalLength == 0)
                    continue;

                if (line == document.TotalNumberOfLines - 1)
                    forceAbort = true;

                ProcessLineSegment(ref line, lineSegment, '+', AppSettings.DiffAddedColor);
                ProcessLineSegment(ref line, lineSegment, '-', AppSettings.DiffRemovedColor);
                ProcessLineSegment(ref line, lineSegment, '@', AppSettings.DiffSectionColor);
                ProcessLineSegment(ref line, lineSegment, '\\', AppSettings.DiffSectionColor);
            }
        }

        public int ScrollPos
        {
            get { return TextEditor.ActiveTextAreaControl.VScrollBar.Value; }
            set
            {
                var scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;
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

        public int LineAtCaret
        {
            get
            {
                return TextEditor.ActiveTextAreaControl.Caret.Position.Line;
            }
        }

        public void HighlightLine(int line, Color color)
        {
            if (line >= TextEditor.Document.TotalNumberOfLines)
                return; 

            var document = TextEditor.Document;
            var markerStrategy = document.MarkerStrategy;
            var lineSegment = document.GetLineSegment(line);
            markerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                    lineSegment.Length, TextMarkerType.SolidBlock, color
                                                    ));
        }

        public void HighlightLines(int startLine, int endLine, Color color)
        {
            if (startLine > endLine || endLine >= TextEditor.Document.TotalNumberOfLines)
                return;

            var document = TextEditor.Document;
            var markerStrategy = document.MarkerStrategy;
            var startLineSegment = document.GetLineSegment(startLine);
            var endLineSegment = document.GetLineSegment(endLine);
            markerStrategy.AddMarker(new TextMarker(startLineSegment.Offset,
                                                    endLineSegment.Offset - startLineSegment.Offset + endLineSegment.Length,
                                                    TextMarkerType.SolidBlock, color
                                                    ));
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