using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary;
using System.Globalization;
using ResourceManager.Translation;

namespace GitUI.SpellChecker
{
    [DefaultEvent("TextChanged")]
    public partial class EditNetSpell : GitExtensionsControl
    {
        private readonly TranslationString undoMenuItemText = new TranslationString("Undo");
        private readonly TranslationString redoMenuItemText = new TranslationString("Redo");
        private readonly TranslationString cutMenuItemText = new TranslationString("Cut");
        private readonly TranslationString copyMenuItemText = new TranslationString("Copy");
        private readonly TranslationString pasteMenuItemText = new TranslationString("Paste");
        private readonly TranslationString deleteMenuItemText = new TranslationString("Delete");
        private readonly TranslationString selectAllMenuItemText = new TranslationString("Select all");
        
        private readonly TranslationString translateEntireText = new TranslationString("Translate entire text to {0}");
        private readonly TranslationString translateCurrentWord = new TranslationString("Translate '{0}' to {1}");
        private readonly TranslationString addToDictionaryText = new TranslationString("Add to dictionary");
        private readonly TranslationString ignoreWordText = new TranslationString("Ignore word");
        private readonly TranslationString removeWordText = new TranslationString("Remove word");
        private readonly TranslationString dictionaryText = new TranslationString("Dictionary");
        private readonly TranslationString markIllFormedLinesText = new TranslationString("Mark ill formed lines");
       
        private readonly SpellCheckEditControl _customUnderlines;
        private Spelling _spelling;
        private static WordDictionary _wordDictionary;
        private Font TextBoxFont;

        public EditNetSpell()
        {
            InitializeComponent();            
            Translate();

            _customUnderlines = new SpellCheckEditControl(TextBox);

            SpellCheckTimer.Enabled = false;

            EnabledChanged += EditNetSpellEnabledChanged;
        }

        public override string Text
        {
            get 
            {
                return IsWatermarkShowing ? string.Empty : TextBox.Text; 
            }
            set
            {
                HideWatermark();
                TextBox.Text = value;
                ShowWatermark();
            }
        }

        [Description("The font for spelling errors.")]
        [Category("Appearance")]
        public Font MistakeFont { get; set; }

        private void EditNetSpellEnabledChanged(object sender, EventArgs e)
        {
            if (Enabled)
            {
                TextBox.ReadOnly = false;
                ShowWatermark();
            }
            else
            {
                TextBox.ReadOnly = true;
                HideWatermark();
            }
        }

        private bool IsWatermarkShowing;
        private string _WatermarkText = "";
        public string WatermarkText
        {
            get { return _WatermarkText; }

            set 
            {
                HideWatermark();
                _WatermarkText = value;
                ShowWatermark();
            }      
        }

        private void EditNetSpellLoad(object sender, EventArgs e)
        {
            MistakeFont = new Font(TextBox.Font, FontStyle.Underline);
            TextBoxFont = TextBox.Font;
            ShowWatermark();

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
            string dictionaryFile = string.Concat(Settings.GetDictionaryDir(), Settings.Dictionary, ".dic");

            if (_wordDictionary == null || _wordDictionary.DictionaryFile != dictionaryFile)
            {
                _wordDictionary =
                    new WordDictionary(components)
                        {
                            DictionaryFile = dictionaryFile
                        };
            }

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

            //Do not check spelling of watermark text
            if (!IsWatermarkShowing)
            {
                try
                {
                    if (_spelling != null && TextBox.Text.Length < 5000)
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
            }

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
            TextBox.Focus();
            
            SpellCheckContextMenu.Items.Clear();
            
            var undoMenuItem = (ToolStripMenuItem)SpellCheckContextMenu.Items.Add(undoMenuItemText.Text);
            undoMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            undoMenuItem.Click += UndoMenuItemClick;
            undoMenuItem.Enabled = TextBox.CanUndo;
            var redoMenuItem = (ToolStripMenuItem)SpellCheckContextMenu.Items.Add(redoMenuItemText.Text);
            redoMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            redoMenuItem.Click += RedoMenuItemClick;
            redoMenuItem.Enabled = TextBox.CanRedo;
            
            SpellCheckContextMenu.Items.Add(new ToolStripSeparator());

            var cutMenuItem = (ToolStripMenuItem)SpellCheckContextMenu.Items.Add(cutMenuItemText.Text);
            cutMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            cutMenuItem.Click += CutMenuItemClick;
            cutMenuItem.Enabled = (TextBox.SelectedText.Length > 0);
            var copyMenuItem = (ToolStripMenuItem)SpellCheckContextMenu.Items.Add(copyMenuItemText.Text);
            copyMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            copyMenuItem.Click += CopyMenuItemdClick;
            copyMenuItem.Enabled = (TextBox.SelectedText.Length > 0);
            var pasteMenuItem = (ToolStripMenuItem)SpellCheckContextMenu.Items.Add(pasteMenuItemText.Text);
            pasteMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            pasteMenuItem.Click += PasteMenuItemClick;
            pasteMenuItem.Enabled = Clipboard.ContainsText();
            var deleteMenuItem = (ToolStripMenuItem)SpellCheckContextMenu.Items.Add(deleteMenuItemText.Text);
            deleteMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            deleteMenuItem.Click += DeleteMenuItemClick;
            deleteMenuItem.Enabled = (TextBox.SelectedText.Length > 0);
            
            SpellCheckContextMenu.Items.Add(new ToolStripSeparator());

            var selectAllMenuItem = (ToolStripMenuItem)SpellCheckContextMenu.Items.Add(selectAllMenuItemText.Text);
            selectAllMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            selectAllMenuItem.Click += SelectAllMenuItemClick;

            SpellCheckContextMenu.Items.Add(new ToolStripSeparator());

            try
            {
                var pos = TextBox.GetCharIndexFromPosition(TextBox.PointToClient(MousePosition));

                if (pos < 0)
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

                var addToDictionary = SpellCheckContextMenu.Items.Add(addToDictionaryText.Text);
                addToDictionary.Click += AddToDictionaryClick;
                addToDictionary.Enabled = (_spelling.CurrentWord.Length > 0);
                var ignoreWord = SpellCheckContextMenu.Items.Add(ignoreWordText.Text);
                ignoreWord.Click += IgnoreWordClick;
                ignoreWord.Enabled = (_spelling.CurrentWord.Length > 0);
                var removeWord = SpellCheckContextMenu.Items.Add(removeWordText.Text);
                removeWord.Click += RemoveWordClick;
                removeWord.Enabled = (_spelling.CurrentWord.Length > 0);

                SpellCheckContextMenu.Items.Add(new ToolStripSeparator());

                bool suggestionsFound = false;
                foreach (var suggestion in (string[])_spelling.Suggestions.ToArray(typeof(string)))
                {
                    var suggestionToolStripItem = SpellCheckContextMenu.Items.Add(suggestion);
                    suggestionToolStripItem.Click += SuggestionToolStripItemClick;

                    suggestionsFound = true;
                }

                if (suggestionsFound)
                    SpellCheckContextMenu.Items.Add(new ToolStripSeparator());
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

            if (!string.IsNullOrEmpty(_spelling.CurrentWord))
            {
                var translate = new ToolStripMenuItem(string.Format(translateCurrentWord.Text, _spelling.CurrentWord, CultureCodeToString(Settings.Dictionary)));
                translate.Click += translate_Click;
                SpellCheckContextMenu.Items.Add(translate);
            }

            var translateText = new ToolStripMenuItem(string.Format(translateEntireText.Text, CultureCodeToString(Settings.Dictionary)));
            translateText.Click += translateText_Click;
            SpellCheckContextMenu.Items.Add(translateText);

            SpellCheckContextMenu.Items.Add(new ToolStripSeparator());

            try
            {
                var dictionaryToolStripMenuItem = new ToolStripMenuItem(dictionaryText.Text);
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

            SpellCheckContextMenu.Items.Add(new ToolStripSeparator());

            var mi =
                new ToolStripMenuItem(markIllFormedLinesText.Text)
                    {
                        Checked = Settings.MarkIllFormedLinesInCommitMsg
                    };
            mi.Click += MarkIllFormedLinesInCommitMsgClick;
            SpellCheckContextMenu.Items.Add(mi);
        }

        private static string CultureCodeToString(string cultureCode)
        {
            try
            {
                return new CultureInfo(new CultureInfo(cultureCode.Replace('_', '-')).TwoLetterISOLanguageName).NativeName;
            }
            catch
            {
                return cultureCode;
            }
        }

        void translateText_Click(object sender, EventArgs e)
        {
            try
            {
                TextBox.Text = Google.TranslateText(TextBox.Text, "", new CultureInfo(Settings.Dictionary.Replace('_', '-')).TwoLetterISOLanguageName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString());
            }
        }

        void translate_Click(object sender, EventArgs e)
        {
            try
            {
                _spelling.ReplaceWord(Google.TranslateText(_spelling.CurrentWord, "", new CultureInfo(Settings.Dictionary.Replace('_', '-')).TwoLetterISOLanguageName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString());
            }
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
            _spelling.ReplaceWord(((ToolStripItem)sender).Text);
            CheckSpelling();
        }

        private void DicToolStripMenuItemClick(object sender, EventArgs e)
        {
            Settings.Dictionary = ((ToolStripItem)sender).Text;
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
            SpellCheckTimer.Enabled = true;
        }

        private void TextBoxLeave(object sender, EventArgs e)
        {
            ShowWatermark();
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                if (!Clipboard.ContainsText())
                {
                    e.Handled = true;
                    return;
                }
                // remove image data from clipboard
                string text = Clipboard.GetText();
                Clipboard.SetText(text);
            }
            OnKeyDown(e);
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            HideWatermark();
        }

        private void ShowWatermark()
        {
            if (!Focused && string.IsNullOrEmpty(TextBox.Text) && TextBoxFont!= null)
            {
                TextBox.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Italic);
                TextBox.ForeColor = SystemColors.InactiveCaption;
                TextBox.Text = WatermarkText;
                IsWatermarkShowing = true;
            }
        }

        private void HideWatermark()
        {
            if (IsWatermarkShowing && TextBoxFont != null)
            {
                TextBox.Font = TextBoxFont;
                TextBox.Text = string.Empty;
                TextBox.ForeColor = SystemColors.WindowText;
            }
            IsWatermarkShowing = false;
        }
        
        public new bool Focus()
        {
            HideWatermark();
            return base.Focus();
        }
        
        private void UndoMenuItemClick(object sender, EventArgs e)
        {
            TextBox.Undo();
            CheckSpelling();
        }

        private void RedoMenuItemClick(object sender, EventArgs e)
        {
            TextBox.Redo();
            CheckSpelling();
        }

        private void CutMenuItemClick(object sender, EventArgs e)
        {
            TextBox.Cut();
            CheckSpelling();
        }

        private void CopyMenuItemdClick(object sender, EventArgs e)
        {
            TextBox.Copy();
        }

        private void PasteMenuItemClick(object sender, EventArgs e)
        {
            if (!Clipboard.ContainsText()) return;
            // remove image data from clipboard
            string text = Clipboard.GetText();
            Clipboard.SetText(text);

            TextBox.Paste();
            CheckSpelling();
        }

        private void DeleteMenuItemClick(object sender, EventArgs e)
        {
            TextBox.SelectedText = string.Empty;
            CheckSpelling();
        }

        private void SelectAllMenuItemClick(object sender, EventArgs e)
        {
            TextBox.SelectAll();
        }
    }
}
