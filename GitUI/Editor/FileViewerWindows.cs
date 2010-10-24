using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Util;

namespace GitUI.Editor
{
    public partial class FileViewerWindows : GitExtensionsControl, IFileViewer
    {
        private readonly FindAndReplaceForm _findAndReplaceForm = new FindAndReplaceForm();

        public FileViewerWindows()
        {
            InitializeComponent();
            Translate();

            TextEditor.TextChanged += new EventHandler(TextEditor_TextChanged);
            TextEditor.ActiveTextAreaControl.VScrollBar.ValueChanged += new EventHandler(VScrollBar_ValueChanged);

            TextEditor.ActiveTextAreaControl.TextArea.MouseMove += new MouseEventHandler(TextArea_MouseMove);
            TextEditor.ActiveTextAreaControl.TextArea.MouseLeave += new EventHandler(TextArea_MouseLeave);
            TextEditor.ActiveTextAreaControl.TextArea.MouseDown += TextAreaMouseDown;
            TextEditor.KeyDown += BlameFileKeyUp;
            TextEditor.ActiveTextAreaControl.TextArea.KeyDown += BlameFileKeyUp;
            //TextEditor.ActiveTextAreaControl.KeyDown += BlameFileKeyUp;
            TextEditor.ActiveTextAreaControl.TextArea.DoubleClick += ActiveTextAreaControlDoubleClick;
        }

        public new event MouseEventHandler MouseMove;
        public new event EventHandler MouseLeave;

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

        public string GetSelectedText()
        {
            return TextEditor.ActiveTextAreaControl.SelectionManager.SelectedText;
        }

        private void AddExtraPatchHighlighting()
        {
            var document = TextEditor.Document;
            var markerStrategy = document.MarkerStrategy;

            for (var line = 0; line + 3 < document.TotalNumberOfLines; line++)
            {
                var lineSegment1 = document.GetLineSegment(line);
                var lineSegment2 = document.GetLineSegment(line + 1);
                var lineSegment3 = document.GetLineSegment(line + 2);
                var lineSegment4 = document.GetLineSegment(line + 3);

                if (document.GetCharAt(lineSegment1.Offset) != ' ' ||
                    document.GetCharAt(lineSegment2.Offset) != '-' ||
                    document.GetCharAt(lineSegment3.Offset) != '+' ||
                    (lineSegment4.Length > 0 && document.GetCharAt(lineSegment4.Offset) != ' ')) //fix for issue 173
                    continue;

                var beginOffset = 0;
                var endOffset = lineSegment3.Length;
                var reverseOffset = 0;

                for (; beginOffset < endOffset; beginOffset++)
                {
                    if (!document.GetCharAt(lineSegment3.Offset + beginOffset).Equals('+') &&
                        !document.GetCharAt(lineSegment2.Offset + beginOffset).Equals('-') &&
                        !document.GetCharAt(lineSegment3.Offset + beginOffset).Equals(
                            document.GetCharAt(lineSegment2.Offset + beginOffset)))
                        break;
                }

                for (; endOffset > beginOffset; endOffset--)
                {
                    reverseOffset = lineSegment3.Length - endOffset;

                    if (!document.GetCharAt(lineSegment3.Offset + lineSegment3.Length - 1 - reverseOffset)
                             .Equals('+') &&
                        !document.GetCharAt(lineSegment2.Offset + lineSegment2.Length - 1 - reverseOffset)
                             .Equals('-') &&
                        !document.GetCharAt(lineSegment3.Offset + lineSegment3.Length - 1 - reverseOffset).
                             Equals(document.GetCharAt(lineSegment2.Offset + lineSegment2.Length - 1 -
                                                       reverseOffset)))
                        break;
                }

                Color color;
                if (lineSegment3.Length - beginOffset - reverseOffset > 0)
                {
                    color = Settings.DiffAddedExtraColor;
                    markerStrategy.AddMarker(new TextMarker(lineSegment3.Offset + beginOffset,
                                                            lineSegment3.Length - beginOffset - reverseOffset,
                                                            TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
                }

                if (lineSegment2.Length - beginOffset - reverseOffset > 0)
                {
                    color = Settings.DiffRemovedExtraColor;
                    markerStrategy.AddMarker(new TextMarker(lineSegment2.Offset + beginOffset,
                                                            lineSegment2.Length - beginOffset - reverseOffset,
                                                            TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
                }
            }
        }

        public void EnableScrollBars(bool enable)
        {
            TextEditor.ActiveTextAreaControl.VScrollBar.Width = 0;
            TextEditor.ActiveTextAreaControl.VScrollBar.Visible = enable;
            TextEditor.ActiveTextAreaControl.TextArea.Dock = DockStyle.Fill;
            TextEditor.Enabled = enable;
        }

        public void AddPatchHighlighting()
        {
            var document = TextEditor.Document;
            var markerStrategy = document.MarkerStrategy;
            markerStrategy.RemoveAll(m => true);

            AddExtraPatchHighlighting();

            for (var line = 0; line < document.TotalNumberOfLines; line++)
            {
                var lineSegment = document.GetLineSegment(line);

                if (lineSegment.TotalLength == 0)
                    continue;

                if (document.GetCharAt(lineSegment.Offset) == '+')
                {
                    var color = Settings.DiffAddedColor;
                    var endLine = document.GetLineSegment(line);

                    for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == '+'; line++)
                    {
                        endLine = document.GetLineSegment(line);
                    }
                    line--;
                    line--;
                    endLine = document.GetLineSegment(line);

                    markerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                            (endLine.Offset + endLine.TotalLength) -
                                                            lineSegment.Offset, TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
                }
                if (document.GetCharAt(lineSegment.Offset) == '-')
                {
                    var color = Settings.DiffRemovedColor;
                    var endLine = document.GetLineSegment(line);

                    for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == '-'; line++)
                    {
                        endLine = document.GetLineSegment(line);
                    }
                    line--;
                    line--;
                    endLine = document.GetLineSegment(line);

                    markerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                            (endLine.Offset + endLine.TotalLength) -
                                                            lineSegment.Offset, TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
                }
                if (document.GetCharAt(lineSegment.Offset) == '@')
                {
                    var color = Settings.DiffSectionColor;
                    var endLine = document.GetLineSegment(line);

                    for (; line < document.TotalNumberOfLines && document.GetCharAt(endLine.Offset) == '@'; line++)
                    {
                        endLine = document.GetLineSegment(line);
                    }
                    line--;
                    line--;
                    endLine = document.GetLineSegment(line);

                    markerStrategy.AddMarker(new TextMarker(lineSegment.Offset,
                                                            (endLine.Offset + endLine.TotalLength) -
                                                            lineSegment.Offset, TextMarkerType.SolidBlock, color,
                                                            ColorHelper.GetForeColorForBackColor(color)));
                }
            }
        }

        public int ScrollPos
        {
            get { return TextEditor.ActiveTextAreaControl.VScrollBar.Value; }
            set
            {
                var scrollBar = TextEditor.ActiveTextAreaControl.VScrollBar;
                scrollBar.Value = scrollBar.Maximum > value ? value : scrollBar.Maximum;
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

        public string GetLineText(int line)
        {
            return TextEditor.Document.GetText(TextEditor.Document.GetLineSegment(line));
        }

        public int TotalNumberOfLines
        {
            get { return TextEditor.Document.TotalNumberOfLines; }
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

        #endregion
    }
}