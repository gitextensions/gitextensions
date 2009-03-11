using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class EditNetSpell : UserControl
    {
        private NetSpell.SpellChecker.Spelling spelling;
        private NetSpell.SpellChecker.Dictionary.WordDictionary wordDictionary;
        //private System.ComponentModel.IContainer components;

        public EditNetSpell()
        {
            InitializeComponent();
        }

        private void EditNetSpell_Load(object sender, EventArgs e)
        {
            this.components = new System.ComponentModel.Container();
            this.wordDictionary = new NetSpell.SpellChecker.Dictionary.WordDictionary(components);
            this.spelling = new NetSpell.SpellChecker.Spelling(components);
            this.spelling.ShowDialog = false;
            this.spelling.IgnoreAllCapsWords = true;

            // 
            // spelling
            // 
            this.spelling.Dictionary = this.wordDictionary;
            this.spelling.ReplacedWord += new NetSpell.SpellChecker.Spelling.ReplacedWordEventHandler(this.spelling_ReplacedWord);
            this.spelling.EndOfText += new NetSpell.SpellChecker.Spelling.EndOfTextEventHandler(this.spelling_EndOfText);
            this.spelling.DeletedWord += new NetSpell.SpellChecker.Spelling.DeletedWordEventHandler(this.spelling_DeletedWord);
            this.spelling.MisspelledWord += new NetSpell.SpellChecker.Spelling.MisspelledWordEventHandler(spelling_MisspelledWord);

            // 
            // wordDictionary
            // 
            this.wordDictionary.DictionaryFile = @"D:\NetSpell\dic\en-US.dic";

            SpellCheckContextMenu.ItemClicked += new ToolStripItemClickedEventHandler(SpellCheckContextMenu_ItemClicked);
            TextBox.MouseDown += new MouseEventHandler(TextBox_MouseDown);

        }

        void spelling_MisspelledWord(object sender, NetSpell.SpellChecker.SpellingEventArgs e)
        {
            this.TextBox.Select(e.TextIndex, e.Word.Length);
            this.TextBox.SelectionBackColor = Color.OrangeRed;
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            CheckSpelling();
        }

        public void CheckSpelling()
        {
            int start = this.TextBox.SelectionStart;
            int length = this.TextBox.SelectionLength;

            this.TextBox.Select(0, this.TextBox.Text.Length);
            this.TextBox.SelectionBackColor = Color.White;

            this.spelling.Text = this.TextBox.Text;
            this.spelling.ShowDialog = false;
            this.spelling.SpellCheck();

            if (start > this.TextBox.Text.Length)
                start = this.TextBox.Text.Length;

            if ((start + length) > this.TextBox.Text.Length)
                length = 0;

            this.TextBox.Select(start, length);            
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
            char[] seperators = new char[]{' ', '\\', '/', '-', '=', '!', '.', ',', '|', '"', '\'', '&', '*', '@', '#', '(', ')', '[', ']', '{', '}', '+', '^', ':', ';', '~'};

            int pos = TextBox.GetCharIndexFromPosition(TextBox.PointToClient(MousePosition));

            if (pos <= 0)
            {
                e.Cancel = true;
                return;
            }

            this.spelling.Text = TextBox.Text.Substring(TextBox.Text.Substring(0, pos).LastIndexOfAny(seperators));
            this.spelling.WordIndex = 0;
            this.spelling.ShowDialog = false;
            this.spelling.SpellCheck();

            //generate suggestions
            this.spelling.Suggest();

            SpellCheckContextMenu.Items.Clear();

            SpellCheckContextMenu.Items.Add("Add to dictionary");
            SpellCheckContextMenu.Items.Add("Ignore word");
            SpellCheckContextMenu.Items.Add("Remove word");

            SpellCheckContextMenu.Items.Add(new ToolStripSeparator());

            foreach (string suggestion in (string[])this.spelling.Suggestions.ToArray(typeof(string)))
            {
                SpellCheckContextMenu.Items.Add(suggestion);
            }
        }

        void TextBox_MouseDown(object sender, MouseEventArgs e)
        {
        
        }

        void SpellCheckContextMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string selection = e.ClickedItem.Text;

            if (selection == "Add to dictionary")
                this.spelling.Dictionary.Add(this.spelling.CurrentWord);
            else
                if (selection == "Ignore word")
                    this.spelling.IgnoreWord();
                else
                    if (selection == "Remove word")
                        this.spelling.DeleteWord();
                    else
                        this.spelling.ReplaceWord(selection);

            CheckSpelling();
        }

        private void TextBox_Click(object sender, EventArgs e)
        {

        }
    }
}
