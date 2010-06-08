    using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace GitUI
{
    public partial class EditNetSpell : GitExtensionsControl
    {
        private NetSpell.SpellChecker.Spelling spelling;
        private NetSpell.SpellChecker.Dictionary.WordDictionary wordDictionary;
        //private System.ComponentModel.IContainer components;
        private CustomPaintTextBox customUnderlines;

        public EditNetSpell()
        {
            InitializeComponent();

            customUnderlines = new CustomPaintTextBox(TextBox);
            
            SpellCheckTimer.Enabled = false;
        }

        public void SetEmptyMessage(string message)
        {
            EmptyLabel.Text = message;
        }

        public override string Text
        {
            get
            {
                return TextBox.Text;
            }
            set
            {
                TextBox.Text = value;

                UpdateEmptyLabel();
            }
        }

        public Font MistakeFont { get; set; }

        private void EditNetSpell_Load(object sender, EventArgs e)
        {
            this.MistakeFont = new Font(TextBox.Font, FontStyle.Underline);

            //this.TextBox.Document.TextEditorProperties.LineViewerStyle = ICSharpCode.TextEditor.Document.LineViewerStyle.None;

            this.components = new System.ComponentModel.Container();
            this.spelling = new NetSpell.SpellChecker.Spelling(components);
            this.spelling.ShowDialog = false;
            this.spelling.IgnoreAllCapsWords = true;
            this.spelling.IgnoreWordsWithDigits = true;

            // 
            // spelling
            //             
            this.spelling.ReplacedWord += new NetSpell.SpellChecker.Spelling.ReplacedWordEventHandler(this.spelling_ReplacedWord);
            this.spelling.EndOfText += new NetSpell.SpellChecker.Spelling.EndOfTextEventHandler(this.spelling_EndOfText);
            this.spelling.DeletedWord += new NetSpell.SpellChecker.Spelling.DeletedWordEventHandler(this.spelling_DeletedWord);
            this.spelling.MisspelledWord += new NetSpell.SpellChecker.Spelling.MisspelledWordEventHandler(spelling_MisspelledWord);

            // 
            // wordDictionary
            // 
            LoadDictionary();

            SpellCheckContextMenu.ItemClicked += new ToolStripItemClickedEventHandler(SpellCheckContextMenu_ItemClicked);
            TextBox.MouseDown += new MouseEventHandler(TextBox_MouseDown);

        }

        private void LoadDictionary()
        {
            this.wordDictionary = new NetSpell.SpellChecker.Dictionary.WordDictionary(components);

            this.wordDictionary.DictionaryFile = GitCommands.Settings.GetDictionaryDir() + GitCommands.Settings.Dictionary + ".dic";

            this.spelling.Dictionary = this.wordDictionary;
        }

        void spelling_MisspelledWord(object sender, NetSpell.SpellChecker.SpellingEventArgs e)
        {
            customUnderlines.Lines.Add(new TextPos(e.TextIndex, e.TextIndex + e.Word.Length));
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            customUnderlines.Lines.Clear();
            customUnderlines.IllFormedLines.Clear();

            if (GitCommands.Settings.Dictionary == "None" || TextBox.Text.Length < 4)
                return;

            SpellCheckTimer.Enabled = false;
            SpellCheckTimer.Interval = 250;
            SpellCheckTimer.Enabled = true;
        }

        public void CheckSpelling()
        {
            customUnderlines.IllFormedLines.Clear();
            customUnderlines.Lines.Clear();
            try
            {
                if (this.spelling != null)
                {
                    this.spelling.Text = this.TextBox.Text;
                    this.spelling.ShowDialog = false;
                    this.spelling.SpellCheck();
                }
            }
            catch
            {
            }
            markLines();
            TextBox.Refresh();
        }
        private void markLines()
        {
            if(GitCommands.Settings.MarkIllFormedLinesInCommitMsg)
            {
                int numLines = TextBox.Lines.Length;
                int chars = 0;
                for (int curLine = 0; curLine < numLines; ++curLine)
                {
                    int curLength = TextBox.Lines[curLine].Length;
                    int curMaxLength = 72;
                    if (curLine == 0)
                        curMaxLength = 50;
                    if (curLine == 1)
                        curMaxLength = 0;
                    if (curLength > curMaxLength)
                    {
                        customUnderlines.IllFormedLines.Add(new TextPos(chars + curMaxLength, chars + curLength));
                    }
                    //System.Diagnostics.Trace.WriteLine("len: " + curLength + " curMax: " + curMaxLength + " " + (chars + curMaxLength) + "->" + (chars + curLength) + "| " + TextBox.Lines[curLine]);
                    chars += curLength + 1;
                }
            }
        }
        private void spelling_DeletedWord(object sender, NetSpell.SpellChecker.SpellingEventArgs e)
        {
            int start = this.TextBox.SelectionStart;
            int length = this.TextBox.SelectionLength;

            this.TextBox.Select(e.TextIndex, e.Word.Length);
            this.TextBox.SelectedText = "";

            if (start > this.TextBox.Text.Length)
                start = this.TextBox.Text.Length;

            if ((start + length) > this.TextBox.Text.Length)
                length = 0;

            this.TextBox.Select(start, length);
        }

        private void spelling_ReplacedWord(object sender, NetSpell.SpellChecker.ReplaceWordEventArgs e)
        {
            int start = this.TextBox.SelectionStart;
            int length = this.TextBox.SelectionLength;

            this.TextBox.Select(e.TextIndex, e.Word.Length);
            this.TextBox.SelectedText = e.ReplacementWord;

            if (start > this.TextBox.Text.Length)
                start = this.TextBox.Text.Length;

            if ((start + length) > this.TextBox.Text.Length)
                length = 0;

            this.TextBox.Select(start, length);
        }

        private void spelling_EndOfText(object sender, System.EventArgs e)
        {
            //Console.WriteLine("EndOfText");
        }

        private void SpellCheckContextMenu_Click(object sender, EventArgs e)
        {

        }

        private void SpellCheckContextMenu_Opening(object sender, CancelEventArgs e)
        {
            SpellCheckContextMenu.Items.Clear();

            try
            {



                //int pos = this.TextBox.Document.PositionToOffset(this.TextBox.ActiveTextAreaControl.Caret.Position);//= TextBox.GetCharIndexFromPosition(TextBox.PointToClient(MousePosition));
                int pos = TextBox.GetCharIndexFromPosition(TextBox.PointToClient(MousePosition));

                if (pos <= 0)
                {
                    e.Cancel = true;
                    return;
                }

                this.spelling.Text = TextBox.Text;

                this.spelling.WordIndex = this.spelling.GetWordIndexFromTextIndex(pos);
                this.spelling.ShowDialog = false;
                this.spelling.MaxSuggestions = 10;

                //generate suggestions
                this.spelling.Suggest();

                ToolStripItem addToDictionary = SpellCheckContextMenu.Items.Add("Add to dictionary");
                addToDictionary.Click += new EventHandler(addToDictionary_Click);
                ToolStripItem ignoreWord = SpellCheckContextMenu.Items.Add("Ignore word");
                ignoreWord.Click += new EventHandler(ignoreWord_Click);
                ToolStripItem removeWord = SpellCheckContextMenu.Items.Add("Remove word");
                removeWord.Click += new EventHandler(removeWord_Click);

                SpellCheckContextMenu.Items.Add(new ToolStripSeparator());

                foreach (string suggestion in (string[])this.spelling.Suggestions.ToArray(typeof(string)))
                {
                    ToolStripItem suggestionToolStripItem = SpellCheckContextMenu.Items.Add(suggestion);
                    suggestionToolStripItem.Click += new EventHandler(suggestionToolStripItem_Click);
                }
            }
            catch
            {
            }

            try
            {
                SpellCheckContextMenu.Items.Add(new ToolStripSeparator());
                ToolStripMenuItem dictionaryToolStripMenuItem = new ToolStripMenuItem("Dictionary");
                SpellCheckContextMenu.Items.Add(dictionaryToolStripMenuItem);

                ContextMenuStrip toolStripDropDown = new ContextMenuStrip();

                ToolStripMenuItem noDicToolStripMenuItem = new ToolStripMenuItem("None");
                noDicToolStripMenuItem.Click += new EventHandler(dicToolStripMenuItem_Click);
                if (GitCommands.Settings.Dictionary == "None")
                    noDicToolStripMenuItem.Checked = true;


                toolStripDropDown.Items.Add(noDicToolStripMenuItem);

                foreach (string fileName in Directory.GetFiles(GitCommands.Settings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    FileInfo file = new FileInfo(fileName);

                    string dic = file.Name.Replace(".dic", "");

                    ToolStripMenuItem dicToolStripMenuItem = new ToolStripMenuItem(dic);
                    dicToolStripMenuItem.Click += new EventHandler(dicToolStripMenuItem_Click);

                    if (GitCommands.Settings.Dictionary == dic)
                        dicToolStripMenuItem.Checked = true;

                    toolStripDropDown.Items.Add(dicToolStripMenuItem);
                }

                dictionaryToolStripMenuItem.DropDown = toolStripDropDown;
            }
            catch
            {
            }
            ToolStripMenuItem mi = new ToolStripMenuItem("Mark ill formed lines");
            mi.Checked = GitCommands.Settings.MarkIllFormedLinesInCommitMsg;
            mi.Click += new EventHandler(mi_Click);
            SpellCheckContextMenu.Items.Add(mi);
        }

        void removeWord_Click(object sender, EventArgs e)
        {
            this.spelling.DeleteWord();
            CheckSpelling();
        }

        void ignoreWord_Click(object sender, EventArgs e)
        {
            this.spelling.IgnoreWord();
            CheckSpelling();
        }

        void addToDictionary_Click(object sender, EventArgs e)
        {
            this.spelling.Dictionary.Add(this.spelling.CurrentWord);
            CheckSpelling();
        }

        void mi_Click(object sender, EventArgs e)
        {
            GitCommands.Settings.MarkIllFormedLinesInCommitMsg = !GitCommands.Settings.MarkIllFormedLinesInCommitMsg;
            CheckSpelling();
        }

        void suggestionToolStripItem_Click(object sender, EventArgs e)
        {
            this.spelling.ReplaceWord(((ToolStripItem)sender).Text);
            CheckSpelling();
        }

        void dicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitCommands.Settings.Dictionary = ((ToolStripItem)sender).Text;
            LoadDictionary();
            CheckSpelling();

        }

        void TextBox_MouseDown(object sender, MouseEventArgs e)
        {
        }

        void SpellCheckContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
        }

        private void TextBox_Click(object sender, EventArgs e)
        {

        }

        private void SpellCheckTimer_Tick(object sender, EventArgs e)
        {
            CheckSpelling();

            SpellCheckTimer.Enabled = false;
            SpellCheckTimer.Interval = 250;
        }

        private void TextBox_Load(object sender, EventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, EventArgs e)
        {

            if (GitCommands.Settings.Dictionary == "None" || TextBox.Text.Length < 4)
                return;

            SpellCheckTimer.Enabled = true;
        }

        private void TextBox_TextChanged_2(object sender, EventArgs e)
        {
            
        }

        private void TextBox_TextChanged_3(object sender, EventArgs e)
        {
            TextBox_TextChanged(sender, e);
        }

        private void TextBox_SizeChanged(object sender, EventArgs e)
        {
            EmptyLabel.Location = new Point(3, 3);
            EmptyLabel.Size = new Size(Size.Width - 6, Size.Height - 6);
            SpellCheckTimer.Enabled = true;
        }

        private void UpdateEmptyLabel()
        {
            if (TextBox.TextLength == 0)
                EmptyLabel.Visible = true;
            else
                EmptyLabel.Visible = false;
        }

        private void EmptyLabel_Click(object sender, EventArgs e)
        {
            EmptyLabel.Visible = false;
            TextBox.Focus();
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            UpdateEmptyLabel();
        }
    }
}
