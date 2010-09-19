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
            TextEditor.Text = text;
        }

        public void SetHighlighting(string syntax)
        {
            
        }

        public string GetSelectedText()
        {
            return TextEditor.SelectedText;
        }

        public void AddPatchHighlighting()
        {
            
        }

        public int ScrollPos
        {
            get
            {
                return 0;
            }
            set
            {
                
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


        #endregion
    }
}
