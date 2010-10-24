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
        public FileViewerMono()
        {
            InitializeComponent();
            Translate();

            TextEditor.TextChanged += new EventHandler(TextEditor_TextChanged);
            TextEditor.VScroll += new EventHandler(TextEditor_VScroll);
            TextEditor.WordWrap = false;
            TextEditor.DoubleClick += new EventHandler(TextEditor_DoubleClick);
            TextEditor.MouseDown += new MouseEventHandler(TextEditor_MouseDown);
            TextEditor.BackColor = Color.White;
            TextEditor.MouseLeave += new EventHandler(TextArea_MouseLeave);
            TextEditor.MouseMove += new MouseEventHandler(TextArea_MouseLeave);
        }

        public new event EventHandler MouseLeave;

        public void Find()
        {
        }

        void TextArea_MouseLeave(object sender, EventArgs e)
        {
            if (MouseLeave != null)
                MouseLeave(sender, e);
        }

        public event SelectedLineChangedEventHandler SelectedLineChanged;

        void OnSelectedLineChanged(int selectedLine)
        {
            if (SelectedLineChanged != null)
                SelectedLineChanged(this, selectedLine);
        }

        void TextEditor_MouseDown(object sender, MouseEventArgs e)
        {
            OnSelectedLineChanged(TextEditor.GetLineFromCharIndex(TextEditor.GetCharIndexFromPosition(new Point(e.X, e.Y))));
        }

        public new event EventHandler DoubleClick;

        void TextEditor_DoubleClick(object sender, EventArgs e)
        {
            if (DoubleClick != null)
                DoubleClick(sender, e);
        }

        void TextEditor_VScroll(object sender, EventArgs e)
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

        public void EnableScrollBars(bool enable)
        {
            TextEditor.ScrollBars = RichTextBoxScrollBars.None;
        }

        public bool ShowLineNumbers
        {
            get { return false; }
            set {  }
        }

        public string GetText()
        {
            return TextEditor.Text;
        }

        public void SetText(string text)
        {
            TextEditor.Clear();

            TextEditor.Text = text;
        }

        public string GetSelectedText()
        {
            return TextEditor.SelectedText;
        }

        public int ScrollPos
        {
            get
            {
                return 0;
            }
            set
            {
                //TextEditor.ScrollBars   
            }
        }

        public bool ShowEOLMarkers
        {
            get
            {
                return false;
            }
            set
            {
                
            }
        }

        public bool ShowSpaces
        {
            get
            {
                return false;
            }
            set
            {
                
            }
        }

        public bool ShowTabs
        {
            get
            {
                return false;
            }
            set
            {
                
            }
        }

        public int FirstVisibleLine
        {
            get
            {
                int index = TextEditor.GetCharIndexFromPosition(new Point(0, 0));
                return TextEditor.GetLineFromCharIndex(index);
            }
            set
            {
                
            }
        }

        public string GetLineText(int line)
        {
            return "";// TextEditor.GetFirstCharIndexFromLine(line);                
        }

        public int TotalNumberOfLines
        {
            get { return 0; }
        }

        public bool IsReadOnly
        {
            get
            {
                return TextEditor.ReadOnly;
            }
            set
            {
                TextEditor.ReadOnly = value;
            }
        }


        public void AddPatchHighlighting()
        {
            //I modified the code below to add syntax highlighting to the RTFEditor. This 
            //does work on Windows, but it doesn't work on Linux...:(
            /*
            TextEditor.SuspendLayout();
            int lineNumber = 0;
            foreach(string line in TextEditor.Lines)
            {
                if (line.Length == 0)
                    continue;

                if (line[0] == '+')
                {
                    var color = Settings.DiffAddedColor;
                    TextEditor.Select(TextEditor.GetFirstCharIndexFromLine(lineNumber), line.Length);
                    TextEditor.SelectionColor = ColorHelper.GetForeColorForBackColor(color);
                    TextEditor.SelectionBackColor = color;

                }
                if (line[0] == '-')
                {
                    var color = Settings.DiffRemovedColor;
                    TextEditor.Select(TextEditor.GetFirstCharIndexFromLine(lineNumber), line.Length);
                    TextEditor.SelectionColor = ColorHelper.GetForeColorForBackColor(color);
                    TextEditor.SelectionBackColor = color;
                }
                if (line[0] == '@')
                {
                    var color = Settings.DiffSectionColor;
                    TextEditor.Select(TextEditor.GetFirstCharIndexFromLine(lineNumber), line.Length);
                    TextEditor.SelectionColor = ColorHelper.GetForeColorForBackColor(color);
                    TextEditor.SelectionBackColor = color;
                }
                lineNumber++;
            }
            AddExtraPatchHighlighting();
            TextEditor.ResumeLayout();
            TextEditor.Refresh();
        }

        private void AddExtraPatchHighlighting()
        {
            for (var line = 0; line + 3 < TextEditor.Lines.GetLength(0); line++)
            {
                var lineSegment1 = TextEditor.Lines[line];
                var lineSegment2 = TextEditor.Lines[line+1];
                var lineSegment3 = TextEditor.Lines[line+2];
                var lineSegment4 = TextEditor.Lines[line+3];

                if (lineSegment1[0] != ' ' ||
                    lineSegment2[0] != '-' ||
                    lineSegment3[0] != '+' ||
                    lineSegment4[0] != ' ')
                    continue;

                var beginOffset = 0;
                var endOffset = lineSegment3.Length;
                var reverseOffset = 0;

                for (; beginOffset < endOffset; beginOffset++)
                {
                    if (lineSegment3.Length > beginOffset &&
                        lineSegment2.Length > beginOffset &&
                        !lineSegment3[beginOffset].Equals('+') &&
                        !lineSegment2[beginOffset].Equals('-') &&
                        !lineSegment3[beginOffset].Equals(lineSegment2[beginOffset]))
                        break;
                }

                for (; endOffset > beginOffset; endOffset--)
                {
                    reverseOffset = lineSegment3.Length - endOffset;

                    if (!lineSegment3[lineSegment3.Length - 1 - reverseOffset].Equals('+') &&
                        !lineSegment2[lineSegment2.Length - 1 - reverseOffset].Equals('-') &&
                        !lineSegment3[lineSegment3.Length - 1 - reverseOffset].Equals(lineSegment2[lineSegment2.Length - 1 - reverseOffset]))
                        break;
                }

                Color color;
                if (lineSegment3.Length - beginOffset - reverseOffset > 0)
                {
                    color = Settings.DiffAddedExtraColor;
                    TextEditor.Select(TextEditor.GetFirstCharIndexFromLine(line + 2) + beginOffset, lineSegment3.Length - beginOffset - reverseOffset);
                    TextEditor.SelectionColor = ColorHelper.GetForeColorForBackColor(color);
                    TextEditor.SelectionBackColor = color;
                }

                if (lineSegment2.Length - beginOffset - reverseOffset > 0)
                {
                    color = Settings.DiffRemovedExtraColor;
                    TextEditor.Select(TextEditor.GetFirstCharIndexFromLine(line + 1) + beginOffset, lineSegment2.Length - beginOffset - reverseOffset);
                    TextEditor.SelectionColor = ColorHelper.GetForeColorForBackColor(color);
                    TextEditor.SelectionBackColor = color;
                }
                TextEditor.Select(0, 0);
            }*/
        }

        public void SetHighlighting(string  s)
        {
        }

        #endregion
    }
}
