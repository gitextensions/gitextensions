// Copyright (c) 2003, Paul Welter
// All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NetSpell.SpellChecker.Dictionary;

namespace NetSpell.SpellChecker;

/// <summary>
///     The Spelling class encapsulates the functions necessary to check
///     the spelling of inputted text.
/// </summary>
[ToolboxBitmap(typeof(Spelling), "Spelling.bmp")]
public partial class Spelling : Component
{
    #region Global Regex
    [GeneratedRegex(@"^\d")]
    private static partial Regex DigitRegex { get; }
    [GeneratedRegex(@"</[c-g\d]+>|</[i-o\d]+>|</[a\d]+>|</[q-z\d]+>|<[cg]+[^>]*>|<[i-o]+[^>]*>|<[q-z]+[^>]*>|<[a]+[^>]*>|<(\[^\]*\|'[^']*'|[^'\>])*>", RegexOptions.IgnoreCase)]
    private static partial Regex HtmlRegex { get; }
    [GeneratedRegex(@"\D")]
    private static partial Regex LetterRegex { get; }
    [GeneratedRegex(@"[^\p{Lu}]")]
    private static partial Regex UpperRegex { get; } // @"[^A-Z]
    [GeneratedRegex(@"\b[\w']+\b")]
    private static partial Regex WordRegex { get; } // @"\b[A-Za-z0-9_'À-ÿ]+\b"

    private MatchCollection _htmlTags;
    private MatchCollection _words;
    #endregion

    #region private variables
    private Container _components;
    #endregion

    #region Events

    /// <summary>
    ///     This event is fired when a word is deleted
    /// </summary>
    /// <remarks>
    ///     Use this event to update the parent text
    /// </remarks>
    public event EventHandler<SpellingEventArgs> DeletedWord;

    /// <summary>
    ///     This event is fired when word is detected two times in a row
    /// </summary>
    public event EventHandler<SpellingEventArgs> DoubledWord;

    /// <summary>
    ///     This event is fired when the spell checker reaches the end of
    ///     the text in the Text property
    /// </summary>
    public event EventHandler EndOfText;

    /// <summary>
    ///     This event is fired when a word is skipped
    /// </summary>
    public event EventHandler<SpellingEventArgs> IgnoredWord;

    /// <summary>
    ///     This event is fired when the spell checker finds a word that
    ///     is not in the dictionaries
    /// </summary>
    public event EventHandler<SpellingEventArgs> MisspelledWord;

    /// <summary>
    ///     This event is fired when a word is replace
    /// </summary>
    /// <remarks>
    ///     Use this event to update the parent text
    /// </remarks>
    public event EventHandler<ReplaceWordEventArgs> ReplacedWord;

    /// <summary>
    ///     This is the method that is responsible for notifying
    ///     receivers that the event occurred
    /// </summary>
    protected virtual void OnDeletedWord(SpellingEventArgs e)
    {
        DeletedWord?.Invoke(this, e);
    }

    /// <summary>
    ///     This is the method that is responsible for notifying
    ///     receivers that the event occurred
    /// </summary>
    protected virtual void OnDoubledWord(SpellingEventArgs e)
    {
        DoubledWord?.Invoke(this, e);
    }

    /// <summary>
    ///     This is the method that is responsible for notifying
    ///     receivers that the event occurred
    /// </summary>
    protected virtual void OnEndOfText(EventArgs e)
    {
        EndOfText?.Invoke(this, e);
    }

    /// <summary>
    ///     This is the method that is responsible for notifying
    ///     receivers that the event occurred
    /// </summary>
    protected virtual void OnIgnoredWord(SpellingEventArgs e)
    {
        IgnoredWord?.Invoke(this, e);
    }

    /// <summary>
    ///     This is the method that is responsible for notifying
    ///     receivers that the event occurred
    /// </summary>
    protected virtual void OnMisspelledWord(SpellingEventArgs e)
    {
        MisspelledWord?.Invoke(this, e);
    }

    /// <summary>
    ///     This is the method that is responsible for notifying
    ///     receivers that the event occurred
    /// </summary>
    protected virtual void OnReplacedWord(ReplaceWordEventArgs e)
    {
        ReplacedWord?.Invoke(this, e);
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
    public Spelling(IContainer container)
    {
        container.Add(this);
        InitializeComponent();
    }

    #endregion

    #region private methods

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _components?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    ///     Calculates the words from the Text property
    /// </summary>
    private void CalculateWords()
    {
        // splits the text into words
        _words = WordRegex.Matches(_text.ToString());

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
        if (_autoCompleteWords.Contains(characters))
        {
            return false;
        }

        if (IgnoreAllCapsWords && !UpperRegex.IsMatch(characters))
        {
            return false;
        }

        if (IgnoreWordsWithDigits && DigitRegex.IsMatch(characters))
        {
            return false;
        }

        if (!LetterRegex.IsMatch(characters))
        {
            return false;
        }

        if (IgnoreHtml)
        {
            int startIndex = GetWordIndex();

            foreach (Match item in _htmlTags)
            {
                if (startIndex >= item.Index && startIndex <= item.Index + item.Length - 1)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private int GetWordIndex()
    {
        return _words[WordIndex].Index;
    }

    private void Initialize()
    {
        _dictionary ??= new WordDictionary();

        if (!_dictionary.Initialized)
        {
            _dictionary.Initialize();
        }
    }

    /// <summary>
    ///     Calculates the position of html tags in the Text property
    /// </summary>
    private void MarkHtml()
    {
        // splits the text into words
        _htmlTags = HtmlRegex.Matches(_text.ToString());
    }

    /// <summary>
    ///     Resets the public properties
    /// </summary>
    private void Reset()
    {
        WordIndex = 0; // reset word index
        _replacementWord = "";
        Suggestions.Clear();
    }

    #endregion

    #region ISpell Near Miss Suggestion methods

    private void SuggestWord(string word, List<Word> tempSuggestion)
    {
        Word ws = new()
        {
            Text = word,
            EditDistance = EditDistance(CurrentWord, word)
        };
        tempSuggestion.Add(ws);
    }

    /// <summary>
    ///     suggestions for a typical fault of spelling, that
    ///     differs with more, than 1 letter from the right form.
    /// </summary>
    private void ReplaceChars(List<Word> tempSuggestion)
    {
        List<string> replacementChars = Dictionary.ReplaceCharacters;

        // a replacement never grows the word by more than the length of the replacement entry
        int maxEntryLength = 0;
        for (int i = 0; i < replacementChars.Count; i++)
        {
            maxEntryLength = Math.Max(maxEntryLength, replacementChars[i].Length);
        }

        int maxLength = CurrentWord.Length + maxEntryLength;
        Span<char> tempWord = maxLength <= 256 ? stackalloc char[maxLength] : new char[maxLength];

        for (int i = 0; i < replacementChars.Count; i++)
        {
            int split = replacementChars[i].IndexOf(' ');
            ReadOnlySpan<char> key = replacementChars[i].AsSpan(..split);
            ReadOnlySpan<char> replacement = replacementChars[i].AsSpan((split + 1)..);

            int pos = CurrentWord.IndexOf(key, StringComparison.InvariantCulture);
            while (pos > -1)
            {
                CurrentWord.AsSpan(..pos).CopyTo(tempWord);
                replacement.CopyTo(tempWord[pos..]);
                ReadOnlySpan<char> rest = CurrentWord.AsSpan((pos + key.Length)..);
                rest.CopyTo(tempWord[(pos + replacement.Length)..]);

                if (FindWord(tempWord[..(pos + replacement.Length + rest.Length)], out string word))
                {
                    SuggestWord(word, tempSuggestion);
                }

                int next = CurrentWord.AsSpan((pos + 1)..).IndexOf(key, StringComparison.InvariantCulture);
                pos = next < 0 ? -1 : pos + 1 + next;
            }
        }
    }

    /// <summary>
    ///     swap out each char one by one and try all the tryme
    ///     chars in its place to see if that makes a good word
    /// </summary>
    private void BadChar(List<Word> tempSuggestion)
    {
        string tryme = Dictionary.TryCharacters;
        Span<char> tempWord = CurrentWord.Length <= 256 ? stackalloc char[CurrentWord.Length] : new char[CurrentWord.Length];

        for (int i = 0; i < CurrentWord.Length; i++)
        {
            CurrentWord.CopyTo(tempWord);
            for (int x = 0; x < tryme.Length; x++)
            {
                tempWord[i] = tryme[x];
                if (FindWord(tempWord, out string word))
                {
                    SuggestWord(word, tempSuggestion);
                }
            }
        }
    }

    /// <summary>
    ///     try omitting one char of word at a time
    /// </summary>
    private void ExtraChar(List<Word> tempSuggestion)
    {
        if (CurrentWord.Length > 1)
        {
            Span<char> tempWord = CurrentWord.Length <= 257 ? stackalloc char[CurrentWord.Length - 1] : new char[CurrentWord.Length - 1];

            for (int i = 0; i < CurrentWord.Length; i++)
            {
                CurrentWord.AsSpan(0, i).CopyTo(tempWord);
                CurrentWord.AsSpan(i + 1).CopyTo(tempWord[i..]);
                if (FindWord(tempWord, out string word))
                {
                    SuggestWord(word, tempSuggestion);
                }
            }
        }
    }

    /// <summary>
    ///     try inserting a tryme character before every letter
    /// </summary>
    private void ForgotChar(List<Word> tempSuggestion)
    {
        string tryme = Dictionary.TryCharacters;
        Span<char> tempWord = CurrentWord.Length <= 255 ? stackalloc char[CurrentWord.Length + 1] : new char[CurrentWord.Length + 1];

        for (int i = 0; i <= CurrentWord.Length; i++)
        {
            CurrentWord.AsSpan(0, i).CopyTo(tempWord);
            CurrentWord.AsSpan(i).CopyTo(tempWord[(i + 1)..]);
            for (int x = 0; x < tryme.Length; x++)
            {
                tempWord[i] = tryme[x];
                if (FindWord(tempWord, out string word))
                {
                    SuggestWord(word, tempSuggestion);
                }
            }
        }
    }

    /// <summary>
    ///     try swapping adjacent chars one by one
    /// </summary>
    private void SwapChar(List<Word> tempSuggestion)
    {
        Span<char> tempWord = CurrentWord.Length <= 256 ? stackalloc char[CurrentWord.Length] : new char[CurrentWord.Length];

        for (int i = 0; i < CurrentWord.Length - 1; i++)
        {
            CurrentWord.CopyTo(tempWord);
            (tempWord[i + 1], tempWord[i]) = (tempWord[i], tempWord[i + 1]);
            if (FindWord(tempWord, out string word))
            {
                SuggestWord(word, tempSuggestion);
            }
        }
    }

    /// <summary>
    ///     split the string into two pieces after every char
    ///     if both pieces are good words make them a suggestion
    /// </summary>
    private void TwoWords(List<Word> tempSuggestion)
    {
        for (int i = 1; i < CurrentWord.Length - 1; i++)
        {
            if (FindWord(CurrentWord.AsSpan(..i), out string firstWord)
                && FindWord(CurrentWord.AsSpan(i..), out string secondWord))
            {
                SuggestWord(string.Concat(firstWord, " ", secondWord), tempSuggestion);
            }
        }
    }

    #endregion

    #region public methods

    /// <summary>
    ///     Deletes the CurrentWord from the Text Property
    /// </summary>
    /// <remarks>
    ///     Note, calling ReplaceWord with the ReplacementWord property set to
    ///     an empty string has the same behavior as DeleteWord.
    /// </remarks>
    public void DeleteWord()
    {
        if (_words is null || _words.Count == 0)
        {
            TraceWriter.TraceWarning("No Words to Delete");
            return;
        }

        int replacedIndex = WordIndex;

        int index = _words[replacedIndex].Index;
        int length = _words[replacedIndex].Length;

        // adjust length to remove extra white space after first word
        if (index == 0
            && length < _text.Length
            && _text[length] == ' ')
        {
            length++; // removing trailing space
        }

        // adjust length to remove double white space
        else if (index > 0
            && index + length < _text.Length
            && _text[index - 1] == ' '
            && _text[index + length] == ' ')
        {
            length++; // removing trailing space
        }

        // adjust index to remove extra white space before punctuation
        else if (index > 0
            && index + length < _text.Length
            && _text[index - 1] == ' '
            && char.IsPunctuation(_text[index + length]))
        {
            index--;
            length++;
        }

        // adjust index to remove extra white space before last word
        else if (index > 0
            && index + length == _text.Length
            && _text[index - 1] == ' ')
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
    ///     Calculates the minimum number of change, inserts or deletes
    ///     required to change firstWord into secondWord
    /// </summary>
    /// <param name="source" type="string">
    ///     <para>
    ///         The first word to calculate
    ///     </para>
    /// </param>
    /// <param name="target" type="string">
    ///     <para>
    ///         The second word to calculate
    ///     </para>
    /// </param>
    /// <param name="positionPriority" type="bool">
    ///     <para>
    ///         set to true if the first and last char should have priority
    ///     </para>
    /// </param>
    /// <returns>
    ///     The number of edits to make firstWord equal secondWord
    /// </returns>
    public int EditDistance(string source, string target, bool positionPriority)
    {
        // Only the previous and current row of the distance matrix are needed
        int width = target.Length + 1;
        Span<int> buffer = width <= 128 ? stackalloc int[width * 2] : new int[width * 2];
        Span<int> prev = buffer[..width];
        Span<int> curr = buffer[width..];

        for (int j = 0; j < width; j++)
        {
            prev[j] = j;
        }

        for (int i = 1; i <= source.Length; i++)
        {
            curr[0] = i;

            for (int j = 1; j < width; j++)
            {
                int diag = prev[j - 1] + (source[i - 1] == target[j - 1] ? 0 : 1);
                curr[j] = Math.Min(diag, Math.Min(prev[j], curr[j - 1]) + 1);
            }

            Span<int> swap = prev;
            prev = curr;
            curr = swap;
        }

        int dist = prev[^1];

        // extra edit on first and last chars
        if (positionPriority)
        {
            if (char.ToLowerInvariant(source[0]) != char.ToLowerInvariant(target[0]))
            {
                dist++;
            }

            if (char.ToLowerInvariant(source[^1]) != char.ToLowerInvariant(target[^1]))
            {
                dist++;
            }
        }

        return dist;
    }

    /// <summary>
    ///     Calculates the minimum number of change, inserts or deletes
    ///     required to change firstWord into secondWord
    /// </summary>
    /// <param name="source" type="string">
    ///     <para>
    ///         The first word to calculate
    ///     </para>
    /// </param>
    /// <param name="target" type="string">
    ///     <para>
    ///         The second word to calculate
    ///     </para>
    /// </param>
    /// <returns>
    ///     The number of edits to make firstWord equal secondWord
    /// </returns>
    /// <remarks>
    ///     This method automatically gives priority to matching the first and last char
    /// </remarks>
    public int EditDistance(string source, string target)
    {
        return EditDistance(source, target, positionPriority: true);
    }

    /// <summary>
    ///     Gets the word index from the text index.  Use this method to
    ///     find a word based on the text position.
    /// </summary>
    /// <param name="textIndex">
    ///     <para>
    ///         The index to search for
    ///     </para>
    /// </param>
    /// <returns>
    ///     The word index that the text index falls on
    /// </returns>
    public int GetWordIndexFromTextIndex(int textIndex)
    {
        if (_words is null || _words.Count == 0 || textIndex < 1)
        {
            TraceWriter.TraceWarning("No words to get text index from.");
            return 0;
        }

        if (_words.Count == 1)
        {
            return 0;
        }

        int low = 0;
        int high = _words.Count - 1;

        // binary search
        while (low <= high)
        {
            int mid = (low + high) / 2;
            int wordStartIndex = _words[mid].Index;
            int wordEndIndex = _words[mid].Index + _words[mid].Length - 1;

            // add white space to end of word by finding the start of the next word
            if ((mid + 1) < _words.Count)
            {
                wordEndIndex = _words[mid + 1].Index - 1;
            }

            if (textIndex < wordStartIndex)
            {
                high = mid - 1;
            }
            else if (textIndex > wordEndIndex)
            {
                low = mid + 1;
            }
            else if (wordStartIndex <= textIndex && textIndex <= wordEndIndex)
            {
                return mid;
            }
        }

        // return last word if not found
        return _words.Count - 1;
    }

    /// <summary>
    ///     Ignores all instances of the CurrentWord in the Text Property
    /// </summary>
    public void IgnoreAllWord()
    {
        if (CurrentWord.Length == 0)
        {
            TraceWriter.TraceWarning("No current word");
            return;
        }

        // Add current word to ignore list
        IgnoreList.Add(CurrentWord);
        IgnoreWord();
    }

    /// <summary>
    ///     Ignores the instances of the CurrentWord in the Text Property
    /// </summary>
    /// <remarks>
    ///     Must call SpellCheck after call this method to resume
    ///     spell checking
    /// </remarks>
    public void IgnoreWord()
    {
        if (_words is null || _words.Count == 0 || CurrentWord.Length == 0)
        {
            TraceWriter.TraceWarning("No text or current word");
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
            TraceWriter.TraceWarning("No current word");
            return;
        }

        // if not in list and replacement word has length
        if (_replacementWord.Length > 0)
        {
            ReplaceList.TryAdd(CurrentWord, _replacementWord);
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
        if (_words is null || _words.Count == 0 || CurrentWord.Length == 0)
        {
            TraceWriter.TraceWarning("No text or current word");
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
        if (char.IsUpper(_words[replacedIndex].ValueSpan[0]))
        {
            _replacementWord = string.Concat([char.ToUpper(_replacementWord[0], CultureInfo.CurrentUICulture)], _replacementWord.AsSpan(1..));
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
        return SpellCheck(_wordIndex, WordCount - 1);
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
        if (startWordIndex > endWordIndex || _words is null || _words.Count == 0)
        {
            // make sure end index is not greater then word count
            OnEndOfText(EventArgs.Empty);
            return false;
        }

        Initialize();

        bool misspelledWord = false;

        for (int i = startWordIndex; i <= endWordIndex; i++)
        {
            // save the current word index
            WordIndex = i;
            string currentWord = CurrentWord;

            if (CheckString(currentWord))
            {
                if (!TestWord())
                {
                    if (ReplaceList.TryGetValue(currentWord, out string word))
                    {
                        ReplacementWord = word;
                        ReplaceWord();
                    }
                    else if (!IgnoreList.Contains(currentWord))
                    {
                        misspelledWord = true;
                        OnMisspelledWord(new SpellingEventArgs(currentWord, i, _words[i].Index));       // raise event
                    }
                }
                else if (i > 0 && _words[i - 1].Value == currentWord
                    && (_words[i - 1].Index + _words[i - 1].Length + 1) == _words[i].Index)
                {
                    misspelledWord = true;
                    OnDoubledWord(new SpellingEventArgs(currentWord, i, _words[i].Index));      // raise event
                }
            }
        }

        if (_wordIndex >= _words.Count - 1 && !misspelledWord)
        {
            OnEndOfText(EventArgs.Empty);
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
    ///     This method sets the <see cref="Text"/> property to the word.
    ///     Then calls <see cref="TestWord(string)"/> on the word to generate the need
    ///     information for suggestions. Note that the Text, CurrentWord and WordIndex
    ///     properties are set when calling this method.
    /// </remarks>
    /// <seealso cref="CurrentWord"/>
    /// <seealso cref="Suggestions"/>
    /// <seealso cref="TestWord(string)"/>
    public void Suggest(string word)
    {
        Text = word;
        if (!TestWord(word))
        {
            Suggest();
        }
    }

    /// <summary>
    ///     Populates the <see cref="Suggestions"/> property with word suggestions
    ///     for the <see cref="CurrentWord"/>
    /// </summary>
    /// <remarks>
    ///     <see cref="TestWord()"/> or <see cref="TestWord(string)"/> must have been called before calling this method
    /// </remarks>
    /// <seealso cref="CurrentWord"/>
    /// <seealso cref="Suggestions"/>
    /// <seealso cref="TestWord()"/>
    /// <seealso cref="TestWord(string)"/>
    public void Suggest()
    {
        // can't generate suggestions with out current word
        if (CurrentWord.Length == 0)
        {
            TraceWriter.TraceWarning("No current word");
            return;
        }

        Initialize();

        List<Word> tempSuggestion = [];

        if ((SuggestionMode == SuggestionEnum.PhoneticNearMiss
            || SuggestionMode == SuggestionEnum.Phonetic)
            && _dictionary.PhoneticRules.Count > 0)
        {
            // generate phonetic code for possible root word
            Dictionary<string, string> codes = [];
            foreach (string tempWord in _dictionary.PossibleBaseWords)
            {
                string tempCode = _dictionary.PhoneticCode(tempWord);
                if (tempCode.Length > 0)
                {
                    codes.TryAdd(tempCode, tempCode);
                }
            }

            if (codes.Count > 0)
            {
                // search root words for phonetic codes
                foreach (Word word in _dictionary.BaseWords.Values)
                {
                    if (codes.ContainsKey(word.PhoneticCode))
                    {
                        List<string> words = _dictionary.ExpandWord(word);

                        // add expanded words
                        foreach (string expandedWord in words)
                        {
                            SuggestWord(expandedWord, tempSuggestion);
                        }
                    }
                }
            }

            TraceWriter.TraceVerbose("Suggestions Found with Phonetic Strategy: {0}", tempSuggestion.Count);
        }

        if (SuggestionMode == SuggestionEnum.PhoneticNearMiss
            || SuggestionMode == SuggestionEnum.NearMiss)
        {
            // suggestions for a typical fault of spelling, that
            // differs with more, than 1 letter from the right form.
            ReplaceChars(tempSuggestion);

            // swap out each char one by one and try all the tryme
            // chars in its place to see if that makes a good word
            BadChar(tempSuggestion);

            // try omitting one char of word at a time
            ExtraChar(tempSuggestion);

            // try inserting a tryme character before every letter
            ForgotChar(tempSuggestion);

            // split the string into two pieces after every char
            // if both pieces are good words make them a suggestion
            TwoWords(tempSuggestion);

            // try swapping adjacent chars one by one
            SwapChar(tempSuggestion);
        }

        TraceWriter.TraceVerbose("Total Suggestions Found: {0}", tempSuggestion.Count);

        tempSuggestion.Sort();  // sorts by edit score
        Suggestions.Clear();

        foreach (Word suggestion in tempSuggestion)
        {
            string word = suggestion.Text;

            // looking for duplicates
            if (!Suggestions.Contains(word))
            {
                // populating the suggestion list
                Suggestions.Add(word);
            }

            if (Suggestions.Count >= MaxSuggestions && MaxSuggestions > 0)
            {
                break;
            }
        }
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

        TraceWriter.TraceVerbose("Testing Word: {0}", word);

        if (Dictionary.Contains(word))
        {
            return true;
        }

        string lowerWord = word.ToLower();
        if (word != lowerWord)
        {
            return Dictionary.Contains(lowerWord);
        }

        return false;
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
    public bool FindWord(ref string word)
    {
        Initialize();

        TraceWriter.TraceVerbose("Find Word: {0}", word);

        if (Dictionary.Contains(word))
        {
            return true;
        }

        string wordLower = word.ToLowerInvariant();
        if (word != wordLower && Dictionary.Contains(wordLower))
        {
            word = wordLower;
            return true;
        }

        return false;
    }

#nullable enable
    /// <summary>
    ///     Checks to see if the word is in the dictionary without allocating for words that are not found
    /// </summary>
    /// <param name="word">The word to check</param>
    /// <param name="foundWord">
    ///     The word as found in the dictionary, which is the lower case word
    ///     if only that is in the dictionary. Null if the word was not found.
    /// </param>
    /// <returns>
    ///     Returns true if word is found in dictionary
    /// </returns>
    /// <remarks>
    ///     Unlike <see cref="FindWord(ref string)"/> this does not update the possible base words
    ///     used when generating phonetic suggestions
    /// </remarks>
    public bool FindWord(ReadOnlySpan<char> word, [NotNullWhen(true)] out string? foundWord)
    {
        Initialize();

        if (TraceWriter.Switch.TraceVerbose)
        {
            TraceWriter.TraceVerbose("Find Word: {0}", word.ToString());
        }

        if (Dictionary.ContainsWord(word))
        {
            foundWord = word.ToString();
            return true;
        }

        Span<char> wordLower = word.Length <= 256 ? stackalloc char[word.Length] : new char[word.Length];
        word.ToLowerInvariant(wordLower);
        if (!word.SequenceEqual(wordLower) && Dictionary.ContainsWord(wordLower))
        {
            foundWord = wordLower.ToString();
            return true;
        }
    
        foundWord = null;
        return false;
    }
#nullable restore

    #endregion

    #region public properties

    private WordDictionary _dictionary;
    private readonly HashSet<string> _autoCompleteWords = [];
    private string _replacementWord = "";
    private StringBuilder _text = new();
    private int _wordIndex;

    /// <summary>
    ///     The suggestion strategy to use when generating suggestions
    /// </summary>
    public enum SuggestionEnum
    {
        /// <summary>
        ///     Combines the phonetic and near miss strategies
        /// </summary>
        PhoneticNearMiss,

        /// <summary>
        ///     The phonetic strategy generates suggestions by word sound
        /// </summary>
        /// <remarks>
        ///     This technique was developed by the open source project ASpell.net
        /// </remarks>
        Phonetic,

        /// <summary>
        ///     The near miss strategy generates suggestion by replacing,
        ///     removing, adding chars to make words
        /// </summary>
        /// <remarks>
        ///     This technique was developed by the open source spell checker ISpell
        /// </remarks>
        NearMiss
    }

    /// <summary>
    ///     Display the 'Spell Check Complete' alert.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(true)]
    [Category("Options")]
    [Description("Display the 'Spell Check Complete' alert.")]
    public bool AlertComplete { get; set; } = true;

    /// <summary>
    ///     The current word being spell checked from the text property
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CurrentWord { get; private set; } = string.Empty;

    /// <summary>
    ///     The WordDictionary object to use when spell checking
    /// </summary>
    [Browsable(true)]
    [Category("Dictionary")]
    [Description("The WordDictionary object to use when spell checking")]
    public WordDictionary Dictionary
    {
        get
        {
            if (!DesignMode && _dictionary is null)
            {
                _dictionary = new WordDictionary();
            }

            return _dictionary;
        }
        set
        {
            if (value is not null)
            {
                _dictionary = value;
            }
        }
    }

    /// <summary>
    ///     Ignore words with all capital letters when spell checking
    /// </summary>
    [DefaultValue(true)]
    [Category("Options")]
    [Description("Ignore words with all capital letters when spell checking")]
    public bool IgnoreAllCapsWords { get; set; } = true;

    /// <summary>
    ///     Ignore html tags when spell checking
    /// </summary>
    [DefaultValue(true)]
    [Category("Options")]
    [Description("Ignore html tags when spell checking")]
    public bool IgnoreHtml { get; set; } = true;

    /// <summary>
    ///     List of words to automatically ignore
    /// </summary>
    /// <remarks>
    ///     When <see cref="IgnoreAllWord"/> is clicked, the <see cref="CurrentWord"/> is added to this list
    /// </remarks>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<string> IgnoreList { get; } = [];

    /// <summary>
    ///     Ignore words with digits when spell checking
    /// </summary>
    [DefaultValue(false)]
    [Category("Options")]
    [Description("Ignore words with digits when spell checking")]
    public bool IgnoreWordsWithDigits { get; set; }

    /// <summary>
    ///     The maximum number of suggestions to generate
    /// </summary>
    [DefaultValue(25)]
    [Category("Options")]
    [Description("The maximum number of suggestions to generate")]
    public int MaxSuggestions { get; set; } = 25;

    /// <summary>
    ///     List of words and replacement values to automatically replace
    /// </summary>
    /// <remarks>
    ///     When <see cref="ReplaceAllWord()"/> is clicked, the <see cref="CurrentWord"/> is added to this list
    /// </remarks>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Dictionary<string, string> ReplaceList { get; } = [];

    /// <summary>
    ///     The word to used when replacing the misspelled word
    /// </summary>
    /// <seealso cref="ReplaceAllWord()"/>
    /// <seealso cref="ReplaceWord()"/>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ReplacementWord
    {
        get => _replacementWord;
        set => _replacementWord = value.Trim();
    }

    /// <summary>
    ///     Determines if the spell checker should use its internal suggestions
    ///     and options dialogs.
    /// </summary>
    [DefaultValue(true)]
    [Category("Options")]
    [Description("Determines if the spell checker should use its internal dialogs")]
    public bool ShowDialog { get; set; } = true;

    /// <summary>
    ///     The suggestion strategy to use when generating suggestions
    /// </summary>
    [DefaultValue(SuggestionEnum.PhoneticNearMiss)]
    [Category("Options")]
    [Description("The suggestion strategy to use when generating suggestions")]
    public SuggestionEnum SuggestionMode { get; set; } = SuggestionEnum.PhoneticNearMiss;

    /// <summary>
    ///     An array of word suggestions for the correct spelling of the misspelled word
    /// </summary>
    /// <seealso cref="Suggest()"/>
    /// <seealso cref="SpellCheck()"/>
    /// <seealso cref="MaxSuggestions"/>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<string> Suggestions { get; } = [];

    /// <summary>
    ///     The text to spell check
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Text
    {
        get => _text.ToString();
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
            if (_words is null || _words.Count == 0)
            {
                return 0;
            }

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
            if (_words is null)
            {
                return 0;
            }

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
            if (_words is null)
            {
                return 0;
            }

            // make sure word index can't be higher then word count
            return Math.Max(0, Math.Min(_wordIndex, WordCount - 1));
        }
        set
        {
            _wordIndex = value;

            if (_words is null || _words.Count == 0)
            {
                CurrentWord = string.Empty;
            }
            else
            {
                CurrentWord = _words[WordIndex].Value;
            }
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
        _components = new Container();
    }

    #endregion

    public void AddAutoCompleteWords(IEnumerable<string> words)
    {
        foreach (string word in words)
        {
            _autoCompleteWords.Add(word);
        }
    }
}
