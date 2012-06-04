// Copyright (c) 2003, Paul Welter
// All rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using NetSpell.SpellChecker.Dictionary;


namespace NetSpell.SpellChecker
{
	/// <summary>
	///		The Spelling class encapsulates the functions necessary to check
	///		the spelling of inputted text.
	/// </summary>
	[ToolboxBitmap(typeof(NetSpell.SpellChecker.Spelling), "Spelling.bmp")]
	public class Spelling : System.ComponentModel.Component
	{

		#region Global Regex
		// Regex are class scope and compiled to improve performance on reuse
		private Regex _digitRegex = new Regex(@"^\d", RegexOptions.Compiled);
		private Regex _htmlRegex = new Regex(@"</[c-g\d]+>|</[i-o\d]+>|</[a\d]+>|</[q-z\d]+>|<[cg]+[^>]*>|<[i-o]+[^>]*>|<[q-z]+[^>]*>|<[a]+[^>]*>|<(\[^\]*\|'[^']*'|[^'\>])*>", RegexOptions.IgnoreCase & RegexOptions.Compiled);
		private MatchCollection _htmlTags;
		private Regex _letterRegex = new Regex(@"\D", RegexOptions.Compiled);
		private Regex _upperRegex = new Regex(@"[^A-Z]", RegexOptions.Compiled);
		private Regex _wordEx = new Regex(@"\b[A-Za-z0-9_'À-ÿ]+\b", RegexOptions.Compiled);
		private MatchCollection _words;
        private SuggestionEnum _suggestionMode = SuggestionEnum.PhoneticNearMiss;
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
			this.MarkHtml();
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
				int startIndex = _words[this.WordIndex].Index;
				
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

		private void Initialize()
		{
			if(_dictionary == null)
				_dictionary = new WordDictionary();

			if(!_dictionary.Initialized)
				_dictionary.Initialize();
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
			_wordIndex = 0; // reset word index
			_replacementWord = "";
			_suggestions.Clear();
		}

		#endregion

		#region ISpell Near Miss Suggetion methods

		/// <summary>
		///		swap out each char one by one and try all the tryme
		///		chars in its place to see if that makes a good word
		/// </summary>
        private void BadChar(ref List<Word> tempSuggestion)
		{
			for (int i = 0; i < this.CurrentWord.Length; i++)
			{
				StringBuilder tempWord = new StringBuilder(this.CurrentWord);
				char[] tryme = this.Dictionary.TryCharacters.ToCharArray();
				for (int x = 0; x < tryme.Length; x++)
				{
					tempWord[i] = tryme[x];
					if (this.TestWord(tempWord.ToString())) 
					{
						Word ws = new Word();
						ws.Text = tempWord.ToString().ToLower();
						ws.EditDistance = this.EditDistance(this.CurrentWord, tempWord.ToString());
				
						tempSuggestion.Add(ws);
					}
				}			 
			}
		}

		/// <summary>
		///     try omitting one char of word at a time
		/// </summary>
        private void ExtraChar(ref List<Word> tempSuggestion)
		{
			if (this.CurrentWord.Length > 1) 
			{
				for (int i = 0; i < this.CurrentWord.Length; i++)
				{
					StringBuilder tempWord = new StringBuilder(this.CurrentWord);
					tempWord.Remove(i, 1);

					if (this.TestWord(tempWord.ToString())) 
					{
						Word ws = new Word();
						ws.Text = tempWord.ToString().ToLower(CultureInfo.CurrentUICulture);
						ws.EditDistance = this.EditDistance(this.CurrentWord, tempWord.ToString());
				
						tempSuggestion.Add(ws);
					}
								 
				}
			}
		}

		/// <summary>
		///     try inserting a tryme character before every letter
		/// </summary>
        private void ForgotChar(ref List<Word> tempSuggestion)
		{
			char[] tryme = this.Dictionary.TryCharacters.ToCharArray();
				
			for (int i = 0; i <= this.CurrentWord.Length; i++)
			{
				for (int x = 0; x < tryme.Length; x++)
				{
					StringBuilder tempWord = new StringBuilder(this.CurrentWord);
				
					tempWord.Insert(i, tryme[x]);
					if (this.TestWord(tempWord.ToString())) 
					{
						Word ws = new Word();
						ws.Text = tempWord.ToString().ToLower();
						ws.EditDistance = this.EditDistance(this.CurrentWord, tempWord.ToString());
				
						tempSuggestion.Add(ws);
					}
				}			 
			}
		}

		/// <summary>
		///     suggestions for a typical fault of spelling, that
		///		differs with more, than 1 letter from the right form.
		/// </summary>
        private void ReplaceChars(ref List<Word> tempSuggestion)
		{
			List<string> replacementChars = this.Dictionary.ReplaceCharacters;
			for (int i = 0; i < replacementChars.Count; i++)
			{
				int split = ((string)replacementChars[i]).IndexOf(' ');
				string key = ((string)replacementChars[i]).Substring(0, split);
				string replacement = ((string)replacementChars[i]).Substring(split+1);

				int pos = this.CurrentWord.IndexOf(key);
				while (pos > -1)
				{
					string tempWord = this.CurrentWord.Substring(0, pos);
					tempWord += replacement;
					tempWord += this.CurrentWord.Substring(pos + key.Length);

					if (this.TestWord(tempWord))
					{
						Word ws = new Word();
						ws.Text = tempWord.ToLower();
						ws.EditDistance = this.EditDistance(this.CurrentWord, tempWord);
				
						tempSuggestion.Add(ws);
					}
					pos = this.CurrentWord.IndexOf(key, pos+1);
				}
			}
		}

		/// <summary>
		///     try swapping adjacent chars one by one
		/// </summary>
        private void SwapChar(ref List<Word> tempSuggestion)
		{
			for (int i = 0; i < this.CurrentWord.Length - 1; i++)
			{
				StringBuilder tempWord = new StringBuilder(this.CurrentWord);
				
				char swap = tempWord[i];
				tempWord[i] = tempWord[i+1];
				tempWord[i+1] = swap;

				if (this.TestWord(tempWord.ToString())) 
				{
					
					Word ws = new Word();
					ws.Text = tempWord.ToString().ToLower();
					ws.EditDistance = this.EditDistance(this.CurrentWord, tempWord.ToString());
				
					tempSuggestion.Add(ws);
				}	 
			}
		}
		
		/// <summary>
		///     split the string into two pieces after every char
		///		if both pieces are good words make them a suggestion
		/// </summary>
        private void TwoWords(ref List<Word> tempSuggestion)
		{
			for (int i = 1; i < this.CurrentWord.Length - 1; i++)
			{
				string firstWord = this.CurrentWord.Substring(0,i);
				string secondWord = this.CurrentWord.Substring(i);
				
				if (this.TestWord(firstWord) && this.TestWord(secondWord)) 
				{
					string tempWord = firstWord + " " + secondWord;
					
					Word ws = new Word();
					ws.Text = tempWord.ToLower();
					ws.EditDistance = this.EditDistance(this.CurrentWord, tempWord);
				
					tempSuggestion.Add(ws);
				}	 
			}
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
				TraceWriter.TraceWarning("No Words to Delete");
				return;
			}
		    int replacedIndex = this.WordIndex;

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
			
			this.CalculateWords();
			this.OnDeletedWord(new SpellingEventArgs(deletedWord, replacedIndex, index));
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
		
			// i.e. 2-D array
			Array matrix = new int[source.Length+1, target.Length+1];

			// boundary conditions
			matrix.SetValue(0, 0, 0); 

			for(int j=1; j <= target.Length; j++)
			{
				// boundary conditions
				int val = (int)matrix.GetValue(0,j-1);
				matrix.SetValue(val+1, 0, j);
			}

			// outer loop
			for(int i=1; i <= source.Length; i++)                            
			{ 
				// boundary conditions
				int val = (int)matrix.GetValue(i-1, 0);
				matrix.SetValue(val+1, i, 0); 

				// inner loop
				for(int j=1; j <= target.Length; j++)                         
				{ 
					int diag = (int)matrix.GetValue(i-1, j-1);

					if(source.Substring(i-1, 1) != target.Substring(j-1, 1)) 
					{
						diag++;
					}

					int deletion = (int)matrix.GetValue(i-1, j);
					int insertion = (int)matrix.GetValue(i, j-1);
					int match = Math.Min(deletion+1, insertion+1);		
					matrix.SetValue(Math.Min(diag, match), i, j);
				}//for j
			}//for i

			int dist = (int)matrix.GetValue(source.Length, target.Length);

			// extra edit on first and last chars
			if (positionPriority)
			{
				if (source[0] != target[0]) dist++;
				if (source[source.Length-1] != target[target.Length-1]) dist++;
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
		///		This method automatically gives priority to matching the first and last char
		/// </remarks>
		public int EditDistance(string source, string target)
		{
			return this.EditDistance(source, target, true);
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
				TraceWriter.TraceWarning("No words to get text index from.");
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
			if (this.CurrentWord.Length == 0)
			{
				TraceWriter.TraceWarning("No current word");
				return;
			}

			// Add current word to ignore list
			_ignoreList.Add(this.CurrentWord);
			this.IgnoreWord();
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
			
			if (_words == null || _words.Count == 0 || this.CurrentWord.Length == 0)
			{
				TraceWriter.TraceWarning("No text or current word");
				return;
			}

			this.OnIgnoredWord(new SpellingEventArgs(
				this.CurrentWord, 
				this.WordIndex, 
				_words[this.WordIndex].Index));

			// increment Word Index to skip over this word
			_wordIndex++;
		}

		/// <summary>
		///     Replaces all instances of the CurrentWord in the Text Property
		/// </summary>
		public void ReplaceAllWord()
		{
			if (this.CurrentWord.Length == 0)
			{
				TraceWriter.TraceWarning("No current word");
				return;
			}

			// if not in list and replacement word has length
			if(!_replaceList.ContainsKey(this.CurrentWord) && _replacementWord.Length > 0) 
			{
				_replaceList.Add(this.CurrentWord, _replacementWord);
			}
			
			this.ReplaceWord();
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
			this.ReplacementWord = replacementWord;
			this.ReplaceAllWord();
		}


		/// <summary>
		///     Replaces the instances of the CurrentWord in the Text Property
		/// </summary>
		public void ReplaceWord()
		{
			if (_words == null || _words.Count == 0 || this.CurrentWord.Length == 0)
			{
				TraceWriter.TraceWarning("No text or current word");
				return;
			}

			if (_replacementWord.Length == 0) 
			{
				this.DeleteWord();
				return;
			}
			string replacedWord = this.CurrentWord;
			int replacedIndex = this.WordIndex;

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
			
			this.CalculateWords();

			this.OnReplacedWord(new ReplaceWordEventArgs(
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
			this.ReplacementWord = replacementWord;
			this.ReplaceWord();
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
			return SpellCheck(_wordIndex, this.WordCount-1);
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
			_wordIndex = startWordIndex;
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
				this.OnEndOfText(System.EventArgs.Empty);	//raise event
				return false;
			}

			this.Initialize();

			string currentWord = "";
			bool misspelledWord = false;

			for (int i = startWordIndex; i <= endWordIndex; i++) 
			{
				_wordIndex = i;		// saving the current word index 
				currentWord = this.CurrentWord;

				if(CheckString(currentWord)) 
				{
					if(!TestWord(currentWord)) 
					{
						if(_replaceList.ContainsKey(currentWord)) 
						{
							this.ReplacementWord = _replaceList[currentWord].ToString();
							this.ReplaceWord();
						}
						else if(!_ignoreList.Contains(currentWord))
						{
							misspelledWord = true;
							this.OnMisspelledWord(new SpellingEventArgs(currentWord, i, _words[i].Index));		//raise event
							//break;
						}
					}
					else if(i > 0 && _words[i-1].Value == currentWord 
						&& (_words[i-1].Index + _words[i-1].Length + 1) == _words[i].Index)
					{
						misspelledWord = true;
						this.OnDoubledWord(new SpellingEventArgs(currentWord, i, _words[i].Index));		//raise event
						//break;
					}
				}
			} // for

			if(_wordIndex >= _words.Count-1 && !misspelledWord) 
			{
				this.OnEndOfText(System.EventArgs.Empty);	//raise event
			}
		
			return misspelledWord;

		} // SpellCheck
		
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
			this.Text = text;
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
			this.WordIndex = startWordIndex;
			this.Text = text;
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
			this.Text = word;
			if(!this.TestWord(word))
				this.Suggest();
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
			if (this.CurrentWord.Length == 0)
			{
				TraceWriter.TraceWarning("No current word");
				return;
			}

			this.Initialize();

            List<Word> tempSuggestion = new List<Word>();

			if ((_suggestionMode == SuggestionEnum.PhoneticNearMiss 
				|| _suggestionMode == SuggestionEnum.Phonetic)
				&& _dictionary.PhoneticRules.Count > 0)
			{
				// generate phonetic code for possible root word
                Dictionary<string, string> codes = new Dictionary<string, string>();
				foreach (string tempWord in _dictionary.PossibleBaseWords)
				{
					string tempCode = _dictionary.PhoneticCode(tempWord);
					if (tempCode.Length > 0 && !codes.ContainsKey(tempCode)) 
					{
						codes.Add(tempCode, tempCode);
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
								Word newWord = new Word();
								newWord.Text = expandedWord;
								newWord.EditDistance = this.EditDistance(this.CurrentWord, expandedWord);
								tempSuggestion.Add(newWord);
							}
						}
					}
				}
				TraceWriter.TraceVerbose("Suggestiongs Found with Phonetic Stratagy: {0}" , tempSuggestion.Count);
			}

			if (_suggestionMode == SuggestionEnum.PhoneticNearMiss 
				|| _suggestionMode == SuggestionEnum.NearMiss)
			{
				// suggestions for a typical fault of spelling, that
				// differs with more, than 1 letter from the right form.
				this.ReplaceChars(ref tempSuggestion);

				// swap out each char one by one and try all the tryme
				// chars in its place to see if that makes a good word
				this.BadChar(ref tempSuggestion);

				// try omitting one char of word at a time
				this.ExtraChar(ref tempSuggestion);

				// try inserting a tryme character before every letter
				this.ForgotChar(ref tempSuggestion);

				// split the string into two pieces after every char
				// if both pieces are good words make them a suggestion
				this.TwoWords(ref tempSuggestion);

				// try swapping adjacent chars one by one
				this.SwapChar(ref tempSuggestion);
			}

			TraceWriter.TraceVerbose("Total Suggestiongs Found: {0}" , tempSuggestion.Count);

			tempSuggestion.Sort();  // sorts by edit score
			_suggestions.Clear(); 

			for (int i = 0; i < tempSuggestion.Count; i++)
			{
				string word = ((Word)tempSuggestion[i]).Text;
				// looking for duplicates
				if (!_suggestions.Contains(word))
				{
					// populating the suggestion list
					_suggestions.Add(word);
				}

				if (_suggestions.Count >= _maxSuggestions && _maxSuggestions > 0)
				{
					break;
				}
			}

		} // suggest

        /// <summary>
        ///     Checks to see if the word is in the dictionary
        /// </summary>
        /// <returns>
        ///     Returns true if word is found in dictionary
        /// </returns>
        public bool TestWord()
        {
            Initialize();

            TraceWriter.TraceVerbose("Testing Word: {0}", CurrentWord);

            return Dictionary.Contains(CurrentWord) || Dictionary.Contains(CurrentWord.ToLower());
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
		public bool TestWord(string word)
		{
			Initialize();

			TraceWriter.TraceVerbose("Testing Word: {0}" , word);

		    return Dictionary.Contains(word) || Dictionary.Contains(word.ToLower());
		}

		#endregion

		#region public properties

		private bool _alertComplete = true;
		private WordDictionary _dictionary;
		private bool _ignoreAllCapsWords = true;
		private bool _ignoreHtml = true;
		private List<string> _ignoreList = new List<string>();
		private bool _ignoreWordsWithDigits;
		private int _maxSuggestions = 25;
        private Dictionary<string, string> _replaceList = new Dictionary<string, string>();
		private string _replacementWord = "";
		private bool _showDialog = true;
        private List<string> _suggestions = new List<string>();
		private StringBuilder _text = new StringBuilder();
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
			///		This technique was developed by the open source project ASpell.net
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
		[CategoryAttribute("Options")]
		[Description("Display the 'Spell Check Complete' alert.")]
		public bool AlertComplete
		{
			get { return _alertComplete; }
			set { _alertComplete = value; }
		}

		/// <summary>
		///     The current word being spell checked from the text property
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string CurrentWord
		{
			get
			{
			    if(_words == null || _words.Count == 0)
					return string.Empty;
				else
			    return _words[this.WordIndex].Value;
			}
		}

		/// <summary>
		///     The WordDictionary object to use when spell checking
		/// </summary>
		[Browsable(true)]
		[CategoryAttribute("Dictionary")]
		[Description("The WordDictionary object to use when spell checking")]
		public WordDictionary Dictionary
		{
			get 
			{
				if(!DesignMode && _dictionary == null)
					_dictionary = new WordDictionary();

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
		[CategoryAttribute("Options")]
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
		[CategoryAttribute("Options")]
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
		[CategoryAttribute("Options")]
		[Description("Ignore words with digits when spell checking")]
		public bool IgnoreWordsWithDigits
		{
			get {return _ignoreWordsWithDigits;}
			set {_ignoreWordsWithDigits = value;}
		}

		/// <summary>
		///     The maximum number of suggestions to generate
		/// </summary>
		[DefaultValue(25)]
		[CategoryAttribute("Options")]
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
		///     Determines if the spell checker should use its internal suggestions
		///     and options dialogs.
		/// </summary>
		[DefaultValue(true)]
		[CategoryAttribute("Options")]
		[Description("Determines if the spell checker should use its internal dialogs")]
		public bool ShowDialog
		{
			get {return _showDialog;}
			set 
			{
				_showDialog = value;
			}
		}


		/// <summary>
		///     The suggestion strategy to use when generating suggestions
		/// </summary>
		[DefaultValue(SuggestionEnum.PhoneticNearMiss)]
		[CategoryAttribute("Options")]
		[Description("The suggestion strategy to use when generating suggestions")]
		public SuggestionEnum SuggestionMode
		{
			get {return _suggestionMode;}
			set {_suggestionMode = value;}
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
				this.CalculateWords();
				this.Reset();
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
	
				return _words[this.WordIndex].Index;			
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
				return Math.Max(0, Math.Min(_wordIndex, (this.WordCount-1)));	
			}
			set 
			{	
				_wordIndex = value;	
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
