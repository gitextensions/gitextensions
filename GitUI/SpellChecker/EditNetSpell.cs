using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary;

namespace GitUI.SpellChecker
{
    public partial class EditNetSpell : GitExtensionsControl
    {
        private readonly SpellCheckEditControl _customUnderlines;
        private Spelling _spelling;
        private WordDictionary _wordDictionary;

        public EditNetSpell()
        {
            InitializeComponent();
            Translate();

            EmptyLabel.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Italic);

            _customUnderlines = new SpellCheckEditControl(TextBox);

            SpellCheckTimer.Enabled = false;

            EnabledChanged += EditNetSpellEnabledChanged;
        }

        public override string Text
        {
            get { return TextBox.Text; }
            set
            {
                TextBox.Text = value;

                UpdateEmptyLabel();
            }
        }

        public Font MistakeFont { get; set; }

        private void EditNetSpellEnabledChanged(object sender, EventArgs e)
        {
            if (Enabled)
            {
                TextBox.ReadOnly = false;
                UpdateEmptyLabel();
            }
            else
            {
                TextBox.ReadOnly = true;
                EmptyLabel.Visible = false;
            }
        }

        public void SetEmptyMessage(string message)
        {
            EmptyLabel.Text = message;
        }

        private void EditNetSpellLoad(object sender, EventArgs e)
        {
            MistakeFont = new Font(TextBox.Font, FontStyle.Underline);

            components = new Container();
            _spelling =
                new Spelling(components)
                    {
                        ShowDialog = false,
                        IgnoreAllCapsWords = true,
                        IgnoreWordsWithDigits = true
                    };

            // 
            // spelling
            //             
            _spelling.ReplacedWord += SpellingReplacedWord;
            _spelling.DeletedWord += SpellingDeletedWord;
            _spelling.MisspelledWord += SpellingMisspelledWord;

            // 
            // wordDictionary
            // 
            LoadDictionary();
        }

        private void LoadDictionary()
        {
            _wordDictionary =
                new WordDictionary(components)
                    {
                        DictionaryFile = Settings.GetDictionaryDir() + Settings.Dictionary + ".dic"
                    };

            _spelling.Dictionary = _wordDictionary;
        }

        private void SpellingMisspelledWord(object sender, SpellingEventArgs e)
        {
            _customUnderlines.Lines.Add(new TextPos(e.TextIndex, e.TextIndex + e.Word.Length));
        }

        public void CheckSpelling()
        {
            _customUnderlines.IllFormedLines.Clear();
            _customUnderlines.Lines.Clear();
            try
            {
                if (_spelling != null)
                {
                    _spelling.Text = TextBox.Text;
                    _spelling.ShowDialog = false;

                    if (File.Exists(_spelling.Dictionary.DictionaryFile))
                        _spelling.SpellCheck();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            MarkLines();
            TextBox.Refresh();
        }

        private void MarkLines()
        {
            if (!Settings.MarkIllFormedLinesInCommitMsg)
                return;
            var numLines = TextBox.Lines.Length;
            var chars = 0;
            for (var curLine = 0; curLine < numLines; ++curLine)
            {
                var curLength = TextBox.Lines[curLine].Length;
                var curMaxLength = 72;
                if (curLine == 0)
                    curMaxLength = 50;
                if (curLine == 1)
                    curMaxLength = 0;
                if (curLength > curMaxLength)
                {
                    _customUnderlines.IllFormedLines.Add(new TextPos(chars + curMaxLength, chars + curLength));
                }
                chars += curLength + 1;
            }
        }

        private void SpellingDeletedWord(object sender, SpellingEventArgs e)
        {
            var start = TextBox.SelectionStart;
            var length = TextBox.SelectionLength;

            TextBox.Select(e.TextIndex, e.Word.Length);
            TextBox.SelectedText = "";

            if (start > TextBox.Text.Length)
                start = TextBox.Text.Length;

            if ((start + length) > TextBox.Text.Length)
                length = 0;

            TextBox.Select(start, length);
        }

        private void SpellingReplacedWord(object sender, ReplaceWordEventArgs e)
        {
            var start = TextBox.SelectionStart;
            var length = TextBox.SelectionLength;

            TextBox.Select(e.TextIndex, e.Word.Length);
            TextBox.SelectedText = e.ReplacementWord;

            if (start > TextBox.Text.Length)
                start = TextBox.Text.Length;

            if ((start + length) > TextBox.Text.Length)
                length = 0;

            TextBox.Select(start, length);
        }

        private void SpellCheckContextMenuOpening(object sender, CancelEventArgs e)
        {
            SpellCheckContextMenu.Items.Clear();

            try
            {
                var pos = TextBox.GetCharIndexFromPosition(TextBox.PointToClient(MousePosition));

                if (pos <= 0)
                {
                    e.Cancel = true;
                    return;
                }

                _spelling.Text = TextBox.Text;

                _spelling.WordIndex = _spelling.GetWordIndexFromTextIndex(pos);
                _spelling.ShowDialog = false;
                _spelling.MaxSuggestions = 10;

                //generate suggestions
                _spelling.Suggest();

                var addToDictionary = SpellCheckContextMenu.Items.Add("Add to dictionary");
                addToDictionary.Click += AddToDictionaryClick;
                var ignoreWord = SpellCheckContextMenu.Items.Add("Ignore word");
                ignoreWord.Click += IgnoreWordClick;
                var removeWord = SpellCheckContextMenu.Items.Add("Remove word");
                removeWord.Click += RemoveWordClick;

                SpellCheckContextMenu.Items.Add(new ToolStripSeparator());

                foreach (var suggestion in (string[]) _spelling.Suggestions.ToArray(typeof (string)))
                {
                    var suggestionToolStripItem = SpellCheckContextMenu.Items.Add(suggestion);
                    suggestionToolStripItem.Click += SuggestionToolStripItemClick;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            try
            {
                SpellCheckContextMenu.Items.Add(new ToolStripSeparator());
                var dictionaryToolStripMenuItem = new ToolStripMenuItem("Dictionary");
                SpellCheckContextMenu.Items.Add(dictionaryToolStripMenuItem);

                var toolStripDropDown = new ContextMenuStrip();

                var noDicToolStripMenuItem = new ToolStripMenuItem("None");
                noDicToolStripMenuItem.Click += DicToolStripMenuItemClick;
                if (Settings.Dictionary == "None")
                    noDicToolStripMenuItem.Checked = true;


                toolStripDropDown.Items.Add(noDicToolStripMenuItem);

                foreach (
                    var fileName in
                        Directory.GetFiles(Settings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    var file = new FileInfo(fileName);

                    var dic = file.Name.Replace(".dic", "");

                    var dicToolStripMenuItem = new ToolStripMenuItem(dic);
                    dicToolStripMenuItem.Click += DicToolStripMenuItemClick;

                    if (Settings.Dictionary == dic)
                        dicToolStripMenuItem.Checked = true;

                    toolStripDropDown.Items.Add(dicToolStripMenuItem);
                }

                dictionaryToolStripMenuItem.DropDown = toolStripDropDown;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            var mi =
                new ToolStripMenuItem("Mark ill formed lines")
                    {
                        Checked = Settings.MarkIllFormedLinesInCommitMsg
                    };
            mi.Click += MarkIllFormedLinesInCommitMsgClick;
            SpellCheckContextMenu.Items.Add(mi);
        }

        private void RemoveWordClick(object sender, EventArgs e)
        {
            _spelling.DeleteWord();
            CheckSpelling();
        }

        private void IgnoreWordClick(object sender, EventArgs e)
        {
            _spelling.IgnoreWord();
            CheckSpelling();
        }

        private void AddToDictionaryClick(object sender, EventArgs e)
        {
            _spelling.Dictionary.Add(_spelling.CurrentWord);
            CheckSpelling();
        }

        private void MarkIllFormedLinesInCommitMsgClick(object sender, EventArgs e)
        {
            Settings.MarkIllFormedLinesInCommitMsg = !Settings.MarkIllFormedLinesInCommitMsg;
            CheckSpelling();
        }

        private void SuggestionToolStripItemClick(object sender, EventArgs e)
        {
            _spelling.ReplaceWord(((ToolStripItem) sender).Text);
            CheckSpelling();
        }

        private void DicToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.Dictionary = ((ToolStripItem) sender).Text;
            LoadDictionary();
            CheckSpelling();
        }

        private void SpellCheckTimerTick(object sender, EventArgs e)
        {
            CheckSpelling();

            SpellCheckTimer.Enabled = false;
            SpellCheckTimer.Interval = 250;
        }

        private void TextBoxTextChanged(object sender, EventArgs e)
        {
            _customUnderlines.Lines.Clear();
            _customUnderlines.IllFormedLines.Clear();

            if (Settings.Dictionary == "None" || TextBox.Text.Length < 4)
                return;

            SpellCheckTimer.Enabled = false;
            SpellCheckTimer.Interval = 250;
            SpellCheckTimer.Enabled = true;
        }

        private void TextBoxSizeChanged(object sender, EventArgs e)
        {
            EmptyLabel.Location = new Point(3, 3);
            EmptyLabel.Size = new Size(Size.Width - 6, Size.Height - 6);
            SpellCheckTimer.Enabled = true;
        }

        private void UpdateEmptyLabel()
        {
            EmptyLabel.Visible = TextBox.TextLength == 0;
        }

        private void EmptyLabelClick(object sender, EventArgs e)
        {
            EmptyLabel.Visible = false;
            TextBox.Focus();
        }

        private void TextBoxLeave(object sender, EventArgs e)
        {
            UpdateEmptyLabel();
        }
    }
}