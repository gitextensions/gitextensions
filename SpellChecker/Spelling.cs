// Copyright (c) 2003, Paul Welter
// All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NHunspell;

namespace SpellChecker
{
    /// <summary>
    ///		The Spelling class encapsulates the functions necessary to check
    ///		the spelling of inputted text.
    /// </summary>
    [ToolboxBitmap(typeof(Spelling), "Spelling.bmp")]
    public class Spelling : Component
    {
        #region Global Regex
        // Regex are class scope and compiled to improve performance on reuse
        private readonly Regex _digitRegex = new Regex(@"^\d", RegexOptions.Compiled);
        private readonly Regex _htmlRegex = new Regex(@"</[c-g\d]+>|</[i-o\d]+>|</[a\d]+>|</[q-z\d]+>|<[cg]+[^>]*>|<[i-o]+[^>]*>|<[q-z]+[^>]*>|<[a]+[^>]*>|<(\[^\]*\|'[^']*'|[^'\>])*>", RegexOptions.IgnoreCase & RegexOptions.Compiled);
        private MatchCollection _htmlTags;
        private readonly Regex _letterRegex = new Regex(@"\D", RegexOptions.Compiled);
        private readonly Regex _upperRegex = new Regex(@"[^\p{Lu}]", RegexOptions.Compiled); // @"[^A-Z]
        private readonly Regex _wordEx = new Regex(@"\b[\w']+\b", RegexOptions.Compiled); // @"\b[A-Za-z0-9_'À-ÿ]+\b"
        private MatchCollection _words;
        #endregion

        #region private variables
        private System.ComponentModel.Container components;
        #endregion

        #region Events

        /// <summary>
        ///     This event is fired when a word is deleted
        /// </summary>
        /// <remarks>
        ///		Use this event to update the parent text
        /// </remarks>
        public event DeletedWordEventHandler DeletedWord;

        /// <summary>
        ///     This event is fired when word is detected two times in a row
        /// </summary>
        public event DoubledWordEventHandler DoubledWord;

        /// <summary>
        ///     This event is fired when the spell checker reaches the end of
        ///     the text in the Text property
        /// </summary>
        public event EndOfTextEventHandler EndOfText;

        /// <summary>
        ///     This event is fired when a word is skipped
        /// </summary>
        public event IgnoredWordEventHandler IgnoredWord;

        /// <summary>
        ///     This event is fired when the spell checker finds a word that 
        ///     is not in the dictionaries
        /// </summary>
        public event MisspelledWordEventHandler MisspelledWord;

        /// <summary>
        ///     This event is fired when a word is replace
        /// </summary>
        /// <remarks>
        ///		Use this event to update the parent text
        /// </remarks>
        public event ReplacedWordEventHandler ReplacedWord;


        /// <summary>
        ///     This represents the delegate method prototype that
        ///     event receivers must implement
        /// </summary>
        public delegate void DeletedWordEventHandler(object sender, SpellingEventArgs e);

        /// <summary>
        ///     This represents the delegate method prototype that
        ///     event receivers must implement
        /// </summary>
        public delegate void DoubledWordEventHandler(object sender, SpellingEventArgs e);

        /// <summary>
        ///     This represents the delegate method prototype that
        ///     event receivers must implement
        /// </summary>
        public delegate void EndOfTextEventHandler(object sender, System.EventArgs e);

        /// <summary>
        ///     This represents the delegate method prototype that
        ///     event receivers must implement
        /// </summary>
        public delegate void IgnoredWordEventHandler(object sender, SpellingEventArgs e);

        /// <summary>
        ///     This represents the delegate method prototype that
        ///     event receivers must implement
        /// </summary>
        public delegate void MisspelledWordEventHandler(object sender, SpellingEventArgs e);

        /// <summary>
        ///     This represents the delegate method prototype that
        ///     event receivers must implement
        /// </summary>
        public delegate void ReplacedWordEventHandler(object sender, ReplaceWordEventArgs e);

        /// <summary>
        ///     This is the method that is responsible for notifying
        ///     receivers that the event occurred
        /// </summary>
        protected virtual void OnDeletedWord(SpellingEventArgs e)
        {
            if (DeletedWord != null)
            {
                DeletedWord(this, e);
            }
        }

        /// <summary>
        ///     This is the method that is responsible for notifying
        ///     receivers that the event occurred
        /// </summary>
        protected virtual void OnDoubledWord(SpellingEventArgs e)
        {
            if (DoubledWord != null)
            {
                DoubledWord(this, e);
            }
        }

        /// <summary>
        ///     This is the method that is responsible for notifying
        ///     receivers that the event occurred
        /// </summary>
        protected virtual void OnEndOfText(System.EventArgs e)
        {
            if (EndOfText != null)
            {
                EndOfText(this, e);
            }
        }

        /// <summary>
        ///     This is the method that is responsible for notifying
        ///     receivers that the event occurred
        /// </summary>
        protected virtual void OnIgnoredWord(SpellingEventArgs e)
        {
            if (IgnoredWord != null)
            {
                IgnoredWord(this, e);
            }
        }

        /// <summary>
        ///     This is the method that is responsible for notifying
        ///     receivers that the event occurred
        /// </summary>
        protected virtual void OnMisspelledWord(SpellingEventArgs e)
        {
            if (MisspelledWord != null)
            {
                MisspelledWord(this, e);
            }
        }

        /// <summary>
        ///     This is the method that is responsible for notifying
        ///     receivers that the event occurred
        /// </summary>
        protected virtual void OnReplacedWord(ReplaceWordEventArgs e)
        {
            if (ReplacedWord != null)
            {
                ReplacedWord(this, e);
            }
        }

        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the SpellCheck class
        /// </summary>
        public Spelling()
        {
            InitializeComponent();
        }


        /// <summary>
        ///     Required for Windows.Forms Class Composition Designer support
        /// </summary>
        public Spelling(System.ComponentModel.IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        #endregion

        #region private methods

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if(components != null)
                    components.Dispose();
            }

            base.Dispose( disposing );
        }

        /// <summary>
        ///     Calculates the words from the Text property
        /// </summary>
        private void CalculateWords()
        {
            // splits the text into words
            _words = _wordEx.Matches(_text.ToString());
            
            // remark html
            MarkHtml();
        }

        /// <summary>
        ///     Determines if the string should be spell checked
        /// </summary>
        /// <param name="characters" type="string">
        ///     <para>
        ///         The Characters to check
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns true if the string should be spell checked
        /// </returns>
        private bool CheckString(string characters)
        {
            if(_ignoreAllCapsWords && !_upperRegex.IsMatch(characters))
            {
                return false;
            }
            if(_ignoreWordsWithDigits && _digitRegex.IsMatch(characters))
            {
                return false;
            }
            if(!_letterRegex.IsMatch(characters))
            {
                return false;
            }
            if(_ignoreHtml)
            {
                int startIndex = GetWordIndex();

                return _htmlTags.Cast<Match>().All(item => startIndex < item.Index || startIndex > item.Index + item.Length - 1);
            }
            return true;
        }

        private int GetWordIndex()
        {
            return _words[WordIndex].Index;
        }

        private void Initialize()
        {
            if(_dictionary == null)
                _dictionary = new Hunspell();
        }

        /// <summary>
        ///     Calculates the position of html tags in the Text property
        /// </summary>
        private void MarkHtml()
        {
            // splits the text into words
            _htmlTags = _htmlRegex.Matches(_text.ToString());
        }

        /// <summary>
        ///     Resets the public properties
        /// </summary>
        private void Reset()
        {
            WordIndex = 0; // reset word index
            _replacementWord = "";
            _suggestions.Clear();
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Deletes the CurrentWord from the Text Property
        /// </summary>
        /// <remarks>
        ///		Note, calling ReplaceWord with the ReplacementWord property set to 
        ///		an empty string has the same behavior as DeleteWord.
        /// </remarks>
        public void DeleteWord()
        {
            if (_words == null || _words.Count == 0)
            {
                return;
            }
            int replacedIndex = WordIndex;

            int index = _words[replacedIndex].Index;
            int length = _words[replacedIndex].Length;
            
            // adjust length to remove extra white space after first word
            if (index == 0 
                && index + length < _text.Length 
                && _text[index+length] == ' ')
            {
                length++; //removing trailing space
            }
            // adjust length to remove double white space
            else if (index > 0 
                && index + length < _text.Length 
                && _text[index-1] == ' ' 
                && _text[index+length] == ' ')
            {					
                length++; //removing trailing space
            }
            // adjust index to remove extra white space before punctuation
            else if (index > 0 
                && index + length < _text.Length 
                && _text[index-1] == ' ' 
                && char.IsPunctuation(_text[index+length]))
            {					
                index--;
                length++;
            }
            // adjust index to remove extra white space before last word
            else if (index > 0 
                && index + length == _text.Length
                && _text[index-1] == ' ')	
            {				
                index--;
                length++;
            }

            string deletedWord = _text.ToString(index, length);
            _text.Remove(index, length);
            
            CalculateWords();
            OnDeletedWord(new SpellingEventArgs(deletedWord, replacedIndex, index));
        }

        /// <summary>
        ///		Gets the word index from the text index.  Use this method to 
        ///		find a word based on the text position.
        /// </summary>
        /// <param name="textIndex">
        ///		<para>
        ///         The index to search for
        ///     </para>
        /// </param>
        /// <returns>
        ///		The word index that the text index falls on
        /// </returns>
        public int GetWordIndexFromTextIndex(int textIndex)
        {
            if (_words == null || _words.Count == 0 || textIndex < 1)
            {
                return 0;
            }

            if(_words.Count == 1)
                return 0;

            int low=0; 
            int high=_words.Count-1; 

            // binary search
            while(low<=high) 
            { 
                int mid=(low+high)/2; 
                int wordStartIndex = _words[mid].Index;
                int wordEndIndex = _words[mid].Index + _words[mid].Length - 1;
            
                // add white space to end of word by finding the start of the next word
                if ((mid+1) < _words.Count)
                    wordEndIndex = _words[mid+1].Index - 1;

                if(textIndex < wordStartIndex) 
                    high=mid-1; 
                else if(textIndex > wordEndIndex) 
                    low=mid+1; 
                else if(wordStartIndex <= textIndex && textIndex <= wordEndIndex) 
                    return mid; 
            } 

            // return last word if not found
            return _words.Count-1;
        }

        /// <summary>
        ///     Ignores all instances of the CurrentWord in the Text Property
        /// </summary>
        public void IgnoreAllWord()
        {
            if (CurrentWord.Length == 0)
            {
                return;
            }

            // Add current word to ignore list
            _ignoreList.Add(CurrentWord);
            IgnoreWord();
        }

        /// <summary>
        ///     Ignores the instances of the CurrentWord in the Text Property
        /// </summary>
        /// <remarks>
        ///		Must call SpellCheck after call this method to resume
        ///		spell checking
        /// </remarks>
        public void IgnoreWord()
        {
            if (_words == null || _words.Count == 0 || CurrentWord.Length == 0)
            {
                return;
            }

            OnIgnoredWord(new SpellingEventArgs(
                CurrentWord, 
                WordIndex,
                GetWordIndex()));

            // increment Word Index to skip over this word
            WordIndex = _wordIndex + 1;
        }

        /// <summary>
        ///     Replaces all instances of the CurrentWord in the Text Property
        /// </summary>
        public void ReplaceAllWord()
        {
            if (CurrentWord.Length == 0)
            {
                return;
            }

            // if not in list and replacement word has length
            if(!_replaceList.ContainsKey(CurrentWord) && _replacementWord.Length > 0) 
            {
                _replaceList.Add(CurrentWord, _replacementWord);
            }
            
            ReplaceWord();
        }

        /// <summary>
        ///     Replaces all instances of the CurrentWord in the Text Property
        /// </summary>
        /// <param name="replacementWord" type="string">
        ///     <para>
        ///         The word to replace the CurrentWord with
        ///     </para>
        /// </param>
        public void ReplaceAllWord(string replacementWord)
        {
            ReplacementWord = replacementWord;
            ReplaceAllWord();
        }


        /// <summary>
        ///     Replaces the instances of the CurrentWord in the Text Property
        /// </summary>
        public void ReplaceWord()
        {
            if (_words == null || _words.Count == 0 || CurrentWord.Length == 0)
            {
                return;
            }

            if (_replacementWord.Length == 0) 
            {
                DeleteWord();
                return;
            }
            string replacedWord = CurrentWord;
            int replacedIndex = WordIndex;

            int index = _words[replacedIndex].Index;
            int length = _words[replacedIndex].Length;
            
            _text.Remove(index, length);
            // if first letter upper case, match case for replacement word
            if (char.IsUpper(_words[replacedIndex].ToString(), 0))
            {
                _replacementWord = _replacementWord.Substring(0,1).ToUpper(CultureInfo.CurrentUICulture) 
                    + _replacementWord.Substring(1);
            }
            _text.Insert(index, _replacementWord);
            
            CalculateWords();

            OnReplacedWord(new ReplaceWordEventArgs(
                _replacementWord, 
                replacedWord, 
                replacedIndex, 
                index));
        }

        /// <summary>
        ///     Replaces the instances of the CurrentWord in the Text Property
        /// </summary>
        /// <param name="replacementWord" type="string">
        ///     <para>
        ///         The word to replace the CurrentWord with
        ///     </para>
        /// </param>
        public void ReplaceWord(string replacementWord)
        {
            ReplacementWord = replacementWord;
            ReplaceWord();
        }

        /// <summary>
        ///     Spell checks the words in the <see cref="Text"/> property starting
        ///     at the <see cref="WordIndex"/> position.
        /// </summary>
        /// <returns>
        ///     Returns true if there is a word found in the text 
        ///     that is not in the dictionaries
        /// </returns>
        /// <seealso cref="CurrentWord"/>
        /// <seealso cref="WordIndex"/>
        public bool SpellCheck()
        {
            return SpellCheck(_wordIndex, WordCount-1);
        }

        /// <summary>
        ///     Spell checks the words in the <see cref="Text"/> property starting
        ///     at the <see cref="WordIndex"/> position. This overload takes in the
        ///     WordIndex to start checking from.
        /// </summary>
        /// <param name="startWordIndex" type="int">
        ///     <para>
        ///         The index of the word to start checking from. 
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns true if there is a word found in the text 
        ///     that is not in the dictionaries
        /// </returns>
        /// <seealso cref="CurrentWord"/>
        /// <seealso cref="WordIndex"/>
        public bool SpellCheck(int startWordIndex)
        {
            WordIndex = startWordIndex;
            return SpellCheck();
        }

        /// <summary>
        ///     Spell checks a range of words in the <see cref="Text"/> property starting
        ///     at the <see cref="WordIndex"/> position and ending at endWordIndex. 
        /// </summary>
        /// <param name="startWordIndex" type="int">
        ///     <para>
        ///         The index of the word to start checking from. 
        ///     </para>
        /// </param>
        /// <param name="endWordIndex" type="int">
        ///     <para>
        ///         The index of the word to end checking with. 
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns true if there is a word found in the text 
        ///     that is not in the dictionaries
        /// </returns>
        /// <seealso cref="CurrentWord"/>
        /// <seealso cref="WordIndex"/>
        public bool SpellCheck(int startWordIndex, int endWordIndex)
        {
            if(startWordIndex > endWordIndex || _words == null || _words.Count == 0) 
            {
                // make sure end index is not greater then word count
                OnEndOfText(EventArgs.Empty);	//raise event
                return false;
            }

            Initialize();

            bool misspelledWord = false;

            for (int i = startWordIndex; i <= endWordIndex; i++) 
            {
                WordIndex = i; // saving the current word index
                string currentWord = CurrentWord;

                if(CheckString(currentWord)) 
                {
                    if(!TestWord()) 
                    {
                        if(_replaceList.ContainsKey(currentWord)) 
                        {
                            ReplacementWord = _replaceList[currentWord];
                            ReplaceWord();
                        }
                        else if(!_ignoreList.Contains(currentWord))
                        {
                            misspelledWord = true;
                            OnMisspelledWord(new SpellingEventArgs(currentWord, i, _words[i].Index));		//raise event
                            //break;
                        }
                    }
                    else if(i > 0 && _words[i-1].Value == currentWord 
                        && (_words[i-1].Index + _words[i-1].Length + 1) == _words[i].Index)
                    {
                        misspelledWord = true;
                        OnDoubledWord(new SpellingEventArgs(currentWord, i, _words[i].Index));		//raise event
                        //break;
                    }
                }
            }

            if(_wordIndex >= _words.Count-1 && !misspelledWord) 
            {
                OnEndOfText(EventArgs.Empty);	//raise event
            }
        
            return misspelledWord;

        }
        
        /// <summary>
        ///     Spell checks the words in the <see cref="Text"/> property starting
        ///     at the <see cref="WordIndex"/> position. This overload takes in the 
        ///     text to spell check
        /// </summary>
        /// <param name="text" type="string">
        ///     <para>
        ///         The text to spell check
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns true if there is a word found in the text 
        ///     that is not in the dictionaries
        /// </returns>
        /// <seealso cref="CurrentWord"/>
        /// <seealso cref="WordIndex"/>
        public bool SpellCheck(string text)
        {
            Text = text;
            return SpellCheck();
        }

        /// <summary>
        ///     Spell checks the words in the <see cref="Text"/> property starting
        ///     at the <see cref="WordIndex"/> position. This overload takes in 
        ///     the text to check and the WordIndex to start checking from.
        /// </summary>
        /// <param name="text" type="string">
        ///     <para>
        ///         The text to spell check
        ///     </para>
        /// </param>
        /// <param name="startWordIndex" type="int">
        ///     <para>
        ///         The index of the word to start checking from
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns true if there is a word found in the text 
        ///     that is not in the dictionaries
        /// </returns>
        /// <seealso cref="CurrentWord"/>
        /// <seealso cref="WordIndex"/>
        public bool SpellCheck(string text, int startWordIndex)
        {
            WordIndex = startWordIndex;
            Text = text;
            return SpellCheck();
        }

        /// <summary>
        ///     Populates the <see cref="Suggestions"/> property with word suggestions
        ///     for the word
        /// </summary>
        /// <param name="word" type="string">
        ///     <para>
        ///         The word to generate suggestions on
        ///     </para>
        /// </param>
        /// <remarks>
        ///		This method sets the <see cref="Text"/> property to the word. 
        ///		Then calls <see cref="TestWord"/> on the word to generate the need
        ///		information for suggestions. Note that the Text, CurrentWord and WordIndex 
        ///		properties are set when calling this method.
        /// </remarks>
        /// <seealso cref="CurrentWord"/>
        /// <seealso cref="Suggestions"/>
        /// <seealso cref="TestWord"/>
        public void Suggest(string word)
        {
            Text = word;
            if(!TestWord(word))
                Suggest();
        }

        /// <summary>
        ///     Populates the <see cref="Suggestions"/> property with word suggestions
        ///     for the <see cref="CurrentWord"/>
        /// </summary>
        /// <remarks>
        ///		<see cref="TestWord"/> must have been called before calling this method
        /// </remarks>
        /// <seealso cref="CurrentWord"/>
        /// <seealso cref="Suggestions"/>
        /// <seealso cref="TestWord"/>
        public void Suggest()
        {
            // can't generate suggestions with out current word
            if (CurrentWord.Length == 0)
            {
                return;
            }

            Initialize();
            _suggestions = _dictionary.Suggest(CurrentWord).Take(_maxSuggestions).ToList();
        }

        /// <summary>
        ///     Checks to see if the word is in the dictionary
        /// </summary>
        /// <returns>
        ///     Returns true if word is found in dictionary
        /// </returns>
        public bool TestWord()
        {
            return TestWord(CurrentWord);
        }

        /// <summary>
        ///     Checks to see if the word is in the dictionary
        /// </summary>
        /// <param name="word" type="string">
        ///     <para>
        ///         The word to check
        ///     </para>
        /// </param>
        /// <returns>
        ///     Returns true if word is found in dictionary
        /// </returns>
        private bool TestWord(string word)
        {
            Initialize();

            return Dictionary.Spell(word);
        }
        #endregion

        #region public properties

        private Hunspell _dictionary;
        private bool _ignoreAllCapsWords = true;
        private bool _ignoreHtml = true;
        private readonly List<string> _ignoreList = new List<string>();
        private bool _ignoreWordsWithDigits;
        private int _maxSuggestions = 5;
        private readonly Dictionary<string, string> _replaceList = new Dictionary<string, string>();
        private string _replacementWord = "";
        private List<string> _suggestions = new List<string>();
        private StringBuilder _text = new StringBuilder();
        private int _wordIndex;
        private string _currentWord = string.Empty;

        /// <summary>
        ///     The current word being spell checked from the text property
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CurrentWord
        {
            get { return _currentWord; }
        }

        /// <summary>
        ///     The WordDictionary object to use when spell checking
        /// </summary>
        [Browsable(true)]
        [Category("Dictionary")]
        [Description("The WordDictionary object to use when spell checking")]
        public Hunspell Dictionary
        {
            get 
            {
                if(!DesignMode && _dictionary == null)
                    _dictionary = new Hunspell();

                return _dictionary;
            }
            set 
            {
                if (value != null)
                    _dictionary = value;
            }
        }


        /// <summary>
        ///     Ignore words with all capital letters when spell checking
        /// </summary>
        [DefaultValue(true)]
        [Category("Options")]
        [Description("Ignore words with all capital letters when spell checking")]
        public bool IgnoreAllCapsWords
        {
            get {return _ignoreAllCapsWords;}
            set {_ignoreAllCapsWords = value;}
        }

        /// <summary>
        ///     Ignore html tags when spell checking
        /// </summary>
        [DefaultValue(true)]
        [Category("Options")]
        [Description("Ignore html tags when spell checking")]
        public bool IgnoreHtml
        {
            get {return _ignoreHtml;}
            set {_ignoreHtml = value;}
        }

        /// <summary>
        ///     List of words to automatically ignore
        /// </summary>
        /// <remarks>
        ///		When <see cref="IgnoreAllWord"/> is clicked, the <see cref="CurrentWord"/> is added to this list
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> IgnoreList
        {
            get {return _ignoreList;}
        }

        /// <summary>
        ///     Ignore words with digits when spell checking
        /// </summary>
        [DefaultValue(false)]
        [Category("Options")]
        [Description("Ignore words with digits when spell checking")]
        public bool IgnoreWordsWithDigits
        {
            get {return _ignoreWordsWithDigits;}
            set {_ignoreWordsWithDigits = value;}
        }

        /// <summary>
        ///     The maximum number of suggestions to generate
        /// </summary>
        [DefaultValue(5)]
        [Category("Options")]
        [Description("The maximum number of suggestions to generate")]
        public int MaxSuggestions
        {
            get {return _maxSuggestions;}
            set {_maxSuggestions = value;}
        }

        /// <summary>
        ///     List of words and replacement values to automatically replace
        /// </summary>
        /// <remarks>
        ///		When <see cref="ReplaceAllWord()"/> is clicked, the <see cref="CurrentWord"/> is added to this list
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<string, string> ReplaceList
        {
            get {return _replaceList;}
        }

        /// <summary>
        ///     The word to used when replacing the misspelled word
        /// </summary>
        /// <seealso cref="ReplaceAllWord()"/>
        /// <seealso cref="ReplaceWord()"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ReplacementWord
        {
            get {return _replacementWord;}
            set {_replacementWord = value.Trim();}
        }

        /// <summary>
        ///     An array of word suggestions for the correct spelling of the misspelled word
        /// </summary>
        /// <seealso cref="Suggest()"/>
        /// <seealso cref="SpellCheck()"/>
        /// <seealso cref="MaxSuggestions"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> Suggestions
        {
            get {return _suggestions;}
        }

        /// <summary>
        ///     The text to spell check
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Text
        {
            get {return _text.ToString();}
            set 
            {
                _text = new StringBuilder(value);
                CalculateWords();
                Reset();
            }
        }

        /// <summary>
        ///     TextIndex is the index of the current text being spell checked
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TextIndex
        {
            get 
            {
                if (_words == null || _words.Count == 0)
                    return 0;

                return GetWordIndex();			
            }
        }

        /// <summary>
        ///     The number of words being spell checked
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int WordCount
        {
            get 
            {
                if(_words == null)
                    return 0;

                return _words.Count;
            }
        }

        /// <summary>
        ///     WordIndex is the index of the current word being spell checked
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int WordIndex
        {
            get 
            {
                if(_words == null)
                    return 0;
                
                // make sure word index can't be higher then word count
                return Math.Max(0, Math.Min(_wordIndex, (WordCount-1)));	
            }
            set 
            {
                _wordIndex = value;

                if (_words == null || _words.Count == 0)
                    _currentWord = string.Empty;
                else
                    _currentWord = _words[WordIndex].Value;
            }
        }
        #endregion

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

    } 
}
