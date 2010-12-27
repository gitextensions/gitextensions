using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;

using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary.Affix;
using NetSpell.SpellChecker.Dictionary.Phonetic;

namespace NetSpell.SpellChecker.Dictionary
{

	/// <summary>
	/// The WordDictionary class contains all the logic for managing the word list.
	/// </summary>
	[ToolboxBitmap(typeof(NetSpell.SpellChecker.Dictionary.WordDictionary), "Dictionary.bmp")]
	public class WordDictionary : System.ComponentModel.Component
	{
		private Hashtable _baseWords = new Hashtable();
		private string _copyright = "";
		private string _dictionaryFile = Thread.CurrentThread.CurrentCulture.Name + ".dic";
		private string _dictionaryFolder = "";
		private bool _enableUserFile = true;
		private bool _initialized = false;
		private PhoneticRuleCollection _phoneticRules = new PhoneticRuleCollection();
		private ArrayList _possibleBaseWords = new ArrayList();
		private AffixRuleCollection _prefixRules = new AffixRuleCollection();
		private ArrayList _replaceCharacters = new ArrayList();
		private AffixRuleCollection _suffixRules = new AffixRuleCollection();
		private string _tryCharacters = "";
		private string _userFile = "user.dic";
		private Hashtable _userWords = new Hashtable();
		private System.ComponentModel.Container components = null;

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		public WordDictionary()
		{
			InitializeComponent();
		}

		/// <summary>
		///     Initializes a new instance of the class
		/// </summary>
		public WordDictionary(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
		}

		/// <summary>
		///     Loads the user dictionary file
		/// </summary>
		private void LoadUserFile()
		{
			// load user words
			_userWords.Clear();

			// quit if user file is disabled
			if(!this.EnableUserFile) return;

			string userPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "NetSpell");
			string filePath = Path.Combine(userPath, _userFile);

			if (File.Exists(filePath)) 
			{
				TraceWriter.TraceInfo("Loading User Dictionary:{0}", filePath);

				//IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
				//fs = new IsolatedStorageFileStream(_UserFile, FileMode.OpenOrCreate, FileAccess.ReadWrite, isf);
				FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
				StreamReader sr = new StreamReader(fs, Encoding.UTF8);

				// read line by line
				while (sr.Peek() >= 0) 
				{
					string tempLine = sr.ReadLine().Trim();
					if (tempLine.Length > 0)
					{
						_userWords.Add(tempLine, tempLine);
					}
				}

				fs.Close();
				sr.Close();
				//isf.Close();

				TraceWriter.TraceInfo("Loaded User Dictionary; Words:{0}", _userWords.Count);
			}
		}

		/// <summary>
		///     Saves the user dictionary file
		/// </summary>
		/// <remarks>
		///		If the file does not exist, it will be created
		/// </remarks>
		private void SaveUserFile()
		{

			// quit if user file is disabled
			if(!this.EnableUserFile) return;

			string userPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "NetSpell");
			if (!Directory.Exists(userPath)) Directory.CreateDirectory(userPath);

			string filePath = Path.Combine(userPath, _userFile);

			TraceWriter.TraceInfo("Saving User Dictionary:{0}", filePath);

			//IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForAssembly();
			//FileStream fs = new IsolatedStorageFileStream(_UserFile, FileMode.Create, FileAccess.Write, isf);
			FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
			StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
			sw.NewLine = "\n";

			foreach (string tempWord in _userWords.Keys)
			{
				sw.WriteLine(tempWord);
			}
		
			sw.Close();
			fs.Close();
			//isf.Close();

			TraceWriter.TraceInfo("Saved User Dictionary; Words:{0}", _userWords.Count);
		}

		/// <summary>
		///     Verifies the base word has the affix key
		/// </summary>
		/// <param name="word" type="string">
		///     <para>
		///         Base word to check
		///     </para>
		/// </param>
		/// <param name="affixKey" type="string">
		///     <para>
		///         Affix key to check 
		///     </para>
		/// </param>
		/// <returns>
		///     True if word contains affix key
		/// </returns>
		private bool VerifyAffixKey(string word, char affixKey)
		{
			// make sure base word has this affix key
			Word baseWord = (Word)this.BaseWords[word];
			ArrayList keys = new ArrayList(baseWord.AffixKeys.ToCharArray());
			return keys.Contains(affixKey);			
		}

		/// <summary>
		///     Adds a word to the user list
		/// </summary>
		/// <param name="word" type="string">
		///     <para>
		///         The word to add
		///     </para>
		/// </param>
		/// <remarks>
		///		This method is only affects the user word list
		/// </remarks>
		public void Add(string word)
		{
			_userWords.Add(word, word);
			this.SaveUserFile();
		}

		/// <summary>
		///     Clears the user list of words
		/// </summary>
		/// <remarks>
		///		This method is only affects the user word list
		/// </remarks>
		public void Clear()
		{
			_userWords.Clear();
			this.SaveUserFile();
		}

		/// <summary>
		///     Searches all contained word lists for word
		/// </summary>
		/// <param name="word" type="string">
		///     <para>
		///         The word to search for
		///     </para>
		/// </param>
		/// <returns>
		///     Returns true if word is found
		/// </returns>
		public bool Contains(string word)
		{
			// clean up possible base word list
			_possibleBaseWords.Clear();

			// Step 1 Search UserWords
			if (_userWords.Contains(word)) 
			{
				TraceWriter.TraceVerbose("Word Found in User Dictionary: {0}", word);
				return true;  // word found
			}

			// Step 2 Search BaseWords
			if (_baseWords.Contains(word)) 
			{
				TraceWriter.TraceVerbose("Word Found in Base Words: {0}", word);
				return true; // word found
			}

			// Step 3 Remove suffix, Search BaseWords

			// save suffixed words for use when removing prefix
			ArrayList suffixWords = new ArrayList();
			// Add word to suffix word list
			suffixWords.Add(word);

			foreach(AffixRule rule in SuffixRules.Values)
			{	
				foreach(AffixEntry entry in rule.AffixEntries)
				{
					string tempWord = AffixUtility.RemoveSuffix(word, entry);
					if(tempWord != word)
					{
						if (_baseWords.Contains(tempWord))
						{
							if(this.VerifyAffixKey(tempWord, rule.Name[0]))
							{
								TraceWriter.TraceVerbose("Word Found With Base Words: {0}; Suffix Key: {1}", tempWord, rule.Name[0]);
								return true; // word found
							}
						}
						
						if(rule.AllowCombine)
						{
							// saving word to check if it is a word after prefix is removed
							suffixWords.Add(tempWord);
						}
						else 
						{
							// saving possible base words for use in generating suggestions
							_possibleBaseWords.Add(tempWord);
						}
					}
				}
			}
			// saving possible base words for use in generating suggestions
			_possibleBaseWords.AddRange(suffixWords);

			// Step 4 Remove Prefix, Search BaseWords
			foreach(AffixRule rule in PrefixRules.Values)
			{
				foreach(AffixEntry entry in rule.AffixEntries)
				{
					foreach(string suffixWord in suffixWords)
					{
						string tempWord = AffixUtility.RemovePrefix(suffixWord, entry);
						if (tempWord != suffixWord)
						{
							if (_baseWords.Contains(tempWord))
							{
								if(this.VerifyAffixKey(tempWord, rule.Name[0]))
								{
									TraceWriter.TraceVerbose("Word Found With Base Words: {0}; Prefix Key: {1}", tempWord, rule.Name[0]);
									return true; // word found
								}
							}
							
							// saving possible base words for use in generating suggestions
							_possibleBaseWords.Add(tempWord);
						}
					} // suffix word
				} // prefix rule entry
			} // prefix rule
			// word not found 
			TraceWriter.TraceVerbose("Possible Base Words: {0}", _possibleBaseWords.Count);
			return false;
		}

		/// <summary>
		///     Expands an affix compressed base word
		/// </summary>
		/// <param name="word" type="NetSpell.SpellChecker.Dictionary.Word">
		///     <para>
		///         The word to expand
		///     </para>
		/// </param>
		/// <returns>
		///     A System.Collections.ArrayList of words expanded from base word
		/// </returns>
		public ArrayList ExpandWord(Word word)
		{
			ArrayList suffixWords = new ArrayList();
			ArrayList words = new ArrayList();

			suffixWords.Add(word.Text);
			string prefixKeys = "";


			// check suffix keys first
			foreach(char key in word.AffixKeys)
			{
				if (_suffixRules.ContainsKey(key.ToString(CultureInfo.CurrentUICulture)))
				{
					AffixRule rule = _suffixRules[key.ToString(CultureInfo.CurrentUICulture)];
					string tempWord = AffixUtility.AddSuffix(word.Text, rule);
					if (tempWord != word.Text)
					{
						if (rule.AllowCombine) 
						{
							suffixWords.Add(tempWord);
						}
						else 
						{
							words.Add(tempWord);
						}
					}
				}
				else if (_prefixRules.ContainsKey(key.ToString(CultureInfo.CurrentUICulture)))
				{
					prefixKeys += key.ToString(CultureInfo.CurrentUICulture);
				}
			}

			// apply prefixes
			foreach(char key in prefixKeys)
			{
				AffixRule rule = _prefixRules[key.ToString(CultureInfo.CurrentUICulture)];
				// apply prefix to all suffix words
				foreach (string suffixWord in suffixWords)
				{
					string tempWord = AffixUtility.AddPrefix(suffixWord, rule);
					if (tempWord != suffixWord)
					{
						words.Add(tempWord);
					}
				}
			}

			words.AddRange(suffixWords);

			TraceWriter.TraceVerbose("Word Expanded: {0}; Child Words: {1}", word.Text, words.Count);
			return words;
		}

		/// <summary>
		///     Initializes the dictionary by loading and parsing the
		///     dictionary file and the user file.
		/// </summary>
		public void Initialize()
		{
			// clean up data first
			_baseWords.Clear();
			_replaceCharacters.Clear();
			_prefixRules.Clear();
			_suffixRules.Clear();
			_phoneticRules.Clear();
			_tryCharacters = "";
			

			// the following is used to split a line by white space
			Regex _spaceRegx = new Regex(@"[^\s]+", RegexOptions.Compiled);
			MatchCollection partMatches;

			string currentSection = "";
			AffixRule currentRule = null;
			string dictionaryPath = Path.Combine(_dictionaryFolder, _dictionaryFile);

			TraceWriter.TraceInfo("Loading Dictionary:{0}", dictionaryPath);

			// open dictionary file
			FileStream fs = new FileStream(dictionaryPath, FileMode.Open, FileAccess.Read, FileShare.Read);
			StreamReader sr = new StreamReader(fs, Encoding.UTF8);
			
			// read line by line
			while (sr.Peek() >= 0) 
			{
				string tempLine = sr.ReadLine().Trim();
				if (tempLine.Length > 0)
				{
					// check for section flag
					switch (tempLine)
					{
						case "[Copyright]" :
						case "[Try]" : 
						case "[Replace]" : 
						case "[Prefix]" :
						case "[Suffix]" :
						case "[Phonetic]" :
						case "[Words]" :
							// set current section that is being parsed
							currentSection = tempLine;
							break;
						default :		
							// parse line and place in correct object
						switch (currentSection)
						{
							case "[Copyright]" :
								this.Copyright += tempLine + "\r\n";
								break;
							case "[Try]" : // ISpell try chars
								this.TryCharacters += tempLine;
								break;
							case "[Replace]" : // ISpell replace chars
								this.ReplaceCharacters.Add(tempLine);
								break;
							case "[Prefix]" : // MySpell prefix rules
							case "[Suffix]" : // MySpell suffix rules

								// split line by white space
								partMatches = _spaceRegx.Matches(tempLine);
									
								// if 3 parts, then new rule  
								if (partMatches.Count == 3)
								{
									currentRule = new AffixRule();
									
									// part 1 = affix key
									currentRule.Name = partMatches[0].Value;
									// part 2 = combine flag
									if (partMatches[1].Value == "Y") currentRule.AllowCombine = true;
									// part 3 = entry count, not used

									if (currentSection == "[Prefix]")
									{
										// add to prefix collection
										this.PrefixRules.Add(currentRule.Name, currentRule);
									}
									else 
									{
										// add to suffix collection
										this.SuffixRules.Add(currentRule.Name, currentRule);
									}
								}
									//if 4 parts, then entry for current rule
								else if (partMatches.Count == 4)
								{
									// part 1 = affix key
									if (currentRule.Name == partMatches[0].Value)
									{
										AffixEntry entry = new AffixEntry();

										// part 2 = strip char
										if (partMatches[1].Value != "0") entry.StripCharacters = partMatches[1].Value;
										// part 3 = add chars
										entry.AddCharacters = partMatches[2].Value;
										// part 4 = conditions
										AffixUtility.EncodeConditions(partMatches[3].Value, entry);

										currentRule.AffixEntries.Add(entry);
									}
								}	
								break;
							case "[Phonetic]" : // ASpell phonetic rules
								// split line by white space
								partMatches = _spaceRegx.Matches(tempLine);
								if (partMatches.Count >= 2)
								{
									PhoneticRule rule = new PhoneticRule();
									PhoneticUtility.EncodeRule(partMatches[0].Value, ref rule);
									rule.ReplaceString = partMatches[1].Value;
									_phoneticRules.Add(rule);
								}
								break;
							case "[Words]" : // dictionary word list
								// splits word into its parts
								string[] parts = tempLine.Split('/');
								Word tempWord = new Word();
								// part 1 = base word
								tempWord.Text = parts[0];
								// part 2 = affix keys
								if (parts.Length >= 2) tempWord.AffixKeys = parts[1];
								// part 3 = phonetic code
								if (parts.Length >= 3) tempWord.PhoneticCode = parts[2];
								
								this.BaseWords.Add(tempWord.Text, tempWord);
								break;
						} // currentSection swith
							break;
					} //tempLine switch
				} // if templine
			} // read line
			// close files
			sr.Close();
			fs.Close();

			TraceWriter.TraceInfo("Dictionary Loaded BaseWords:{0}; PrefixRules:{1}; SuffixRules:{2}; PhoneticRules:{3}",
				this.BaseWords.Count, this.PrefixRules.Count, this.SuffixRules.Count, this.PhoneticRules.Count);

			this.LoadUserFile();

			_initialized = true;
		}


		/// <summary>
		///     Generates a phonetic code of how the word sounds
		/// </summary>
		/// <param name="word" type="string">
		///     <para>
		///         The word to generated the sound code from
		///     </para>
		/// </param>
		/// <returns>
		///     A code of how the word sounds
		/// </returns>
		public string PhoneticCode(string word)
		{
			string tempWord = word.ToUpper();
			string prevWord = "";
			StringBuilder code = new StringBuilder();

			while (tempWord.Length > 0)
			{
				// save previous word
				prevWord = tempWord;
				foreach (PhoneticRule rule in _phoneticRules)
				{
					bool begining = tempWord.Length == word.Length ? true : false;
					bool ending = rule.ConditionCount == tempWord.Length ? true : false;

					if ((rule.BeginningOnly == begining || !rule.BeginningOnly)
						&& (rule.EndOnly == ending || !rule.EndOnly)
						&& rule.ConditionCount <= tempWord.Length)
					{
						int passCount = 0;
						// loop through conditions
						for (int i = 0;  i < rule.ConditionCount; i++) 
						{
							int charCode = (int)tempWord[i];
							if ((rule.Condition[charCode] & (1 << i)) == (1 << i))
							{
								passCount++; // condition passed
							}
							else 
							{
								break; // rule fails if one condition fails
							}
						}
						// if all condtions passed
						if (passCount == rule.ConditionCount)
						{
							if (rule.ReplaceMode)
							{
								tempWord = rule.ReplaceString + tempWord.Substring(rule.ConditionCount - rule.ConsumeCount);
							}
							else
							{
								if (rule.ReplaceString != "_")
								{
									code.Append(rule.ReplaceString);
								}
								tempWord = tempWord.Substring(rule.ConditionCount - rule.ConsumeCount);
							}
							break;
						}
					} 
				} // for each

				// if no match consume one char
				if (prevWord.Length == tempWord.Length) 
				{
					
					tempWord = tempWord.Substring(1);
				}
			}// while

			return code.ToString();
		}

		/// <summary>
		///     Removes a word from the user list
		/// </summary>
		/// <param name="word" type="string">
		///     <para>
		///         The word to remove
		///     </para>
		/// </param>
		/// <remarks>
		///		This method is only affects the user word list
		/// </remarks>
		public void Remove(string word)
		{
			_userWords.Remove(word);
			this.SaveUserFile();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		///     The collection of base words for the dictionary
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Hashtable BaseWords
		{
			get {return _baseWords;}
		}

		/// <summary>
		///     Copyright text for the dictionary
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Copyright
		{
			get {return _copyright;}
			set {_copyright = value;}
		}

		/// <summary>
		///     The file name for the main dictionary
		/// </summary>
		[DefaultValue("en-US.dic")]
		[CategoryAttribute("Dictionary")]
		[Description("The file name for the main dictionary")]
		[NotifyParentProperty(true)]
		public string DictionaryFile
		{
			get {return _dictionaryFile;}
			set 
			{
				_dictionaryFile = value;
				if (_dictionaryFile.Length == 0)
				{
					_dictionaryFile = Thread.CurrentThread.CurrentCulture.Name + ".dic";
				}
			}
		}


		/// <summary>
		///     Folder containing the dictionaries
		/// </summary>
		[DefaultValue("")]
		[CategoryAttribute("Dictionary")]
		[Description("The folder containing dictionaries")]
		[Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
		[NotifyParentProperty(true)]
		public string DictionaryFolder
		{
			get {return _dictionaryFolder;}
			set {_dictionaryFolder = value;}
		}

		/// <summary>
		///     Set this to true to automaticly create a user dictionary when
		///     a word is added.
		/// </summary>
		/// <remarks>
		///		This should be set to false in a web environment
		/// </remarks>
		[DefaultValue(true)]
		[CategoryAttribute("Options")]
		[Description("Set this to true to automaticly create a user dictionary")]
		[NotifyParentProperty(true)]
		public bool EnableUserFile
		{
			get {return _enableUserFile;}
			set {_enableUserFile = value;}
		}


		/// <summary>
		///     True if the dictionary has been initialized
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Initialized
		{
			get {return _initialized;}
		}


		/// <summary>
		///     Collection of phonetic rules for this dictionary
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PhoneticRuleCollection PhoneticRules
		{
			get {return _phoneticRules;}
		}


		/// <summary>
		///     Collection of affix prefixes for the base words in this dictionary
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AffixRuleCollection PrefixRules
		{
			get {return _prefixRules;}
		}

		/// <summary>
		///     List of characters to use when generating suggestions using the near miss stratigy
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ArrayList ReplaceCharacters
		{
			get {return _replaceCharacters;}
		}


		/// <summary>
		///     Collection of affix suffixes for the base words in this dictionary
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AffixRuleCollection SuffixRules
		{
			get {return _suffixRules;}
		}

		/// <summary>
		///     List of characters to try when generating suggestions using the near miss stratigy
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string TryCharacters
		{
			get {return _tryCharacters;}
			set {_tryCharacters = value;}
		}

		/// <summary>
		///     The file name for the user word list for this dictionary
		/// </summary>
		[DefaultValue("user.dic")]
		[CategoryAttribute("Dictionary")]
		[Description("The file name for the user word list for this dictionary")]
		[NotifyParentProperty(true)]
		public string UserFile
		{
			get {return _userFile;}
			set {_userFile = value;}
		}

		/// <summary>
		///     List of user entered words in this dictionary
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Hashtable UserWords
		{
			get {return _userWords;}
		}

		/// <summary>
		///     List of text saved from when 'Contains' is called. 
		///     This list is used to generate suggestions from if Contains
		///     doesn't find a word.
		/// </summary>
		/// <remarks>
		///		These are not actual words.
		/// </remarks>
		internal ArrayList PossibleBaseWords
		{
			get {return _possibleBaseWords;}
		}


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
