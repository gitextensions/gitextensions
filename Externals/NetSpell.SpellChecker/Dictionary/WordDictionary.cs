using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
#if FULL_PROFILE
using System.Windows.Forms.Design;
#endif
using NetSpell.SpellChecker.Dictionary.Affix;
using NetSpell.SpellChecker.Dictionary.Phonetic;

namespace NetSpell.SpellChecker.Dictionary
{
    using AffixRuleCollection = Dictionary<string, AffixRule>;
    using PhoneticRuleCollection = List<PhoneticRule>;

    /// <summary>
    /// The WordDictionary class contains all the logic for managing the word list.
    /// </summary>
    [ToolboxBitmap(typeof(WordDictionary), "Dictionary.bmp")]
    public class WordDictionary : Component
    {
        private string _dictionaryFile = Thread.CurrentThread.CurrentCulture.Name + ".dic";
        private Container _components;

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
        public WordDictionary(IContainer container)
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
            UserWords.Clear();

            // quit if user file is disabled
            if (!EnableUserFile)
            {
                return;
            }

            string userPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "NetSpell");
            string filePath = Path.Combine(userPath, UserFile);

            if (File.Exists(filePath))
            {
                TraceWriter.TraceInfo("Loading User Dictionary:{0}", filePath);

                FileStream fs = null;
                try
                {
                    fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        fs = null;

                        // read line by line
                        while (sr.Peek() >= 0)
                        {
                            string tempLine = sr.ReadLine().Trim();
                            if (tempLine.Length > 0)
                            {
                                UserWords.Add(tempLine, tempLine);
                            }
                        }
                    }
                }
                finally
                {
                    fs?.Dispose();
                }

                TraceWriter.TraceInfo("Loaded User Dictionary; Words:{0}", UserWords.Count);
            }
        }

        /// <summary>
        ///     Saves the user dictionary file
        /// </summary>
        /// <remarks>
        ///     If the file does not exist, it will be created
        /// </remarks>
        private void SaveUserFile()
        {
            // quit if user file is disabled
            if (!EnableUserFile)
            {
                return;
            }

            string userPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "NetSpell");
            if (!Directory.Exists(userPath))
            {
                Directory.CreateDirectory(userPath);
            }

            string filePath = Path.Combine(userPath, UserFile);

            TraceWriter.TraceInfo("Saving User Dictionary:{0}", filePath);

            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    fs = null;
                    sw.NewLine = "\n";

                    foreach (string tempWord in UserWords.Keys)
                    {
                        sw.WriteLine(tempWord);
                    }
                }
            }
            finally
            {
                fs?.Dispose();
            }

            TraceWriter.TraceInfo("Saved User Dictionary; Words:{0}", UserWords.Count);
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
            Word baseWord = BaseWords[word];
            return baseWord.AffixKeys.IndexOf(affixKey) != -1;
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
        ///     This method is only affects the user word list
        /// </remarks>
        public void Add(string word)
        {
            UserWords.Add(word, word);
            SaveUserFile();
        }

        /// <summary>
        ///     Clears the user list of words
        /// </summary>
        /// <remarks>
        ///     This method is only affects the user word list
        /// </remarks>
        public void Clear()
        {
            UserWords.Clear();
            SaveUserFile();
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
            PossibleBaseWords.Clear();

            // Step 1 Search UserWords
            if (UserWords.ContainsKey(word))
            {
                TraceWriter.TraceVerbose("Word Found in User Dictionary: {0}", word);
                return true;  // word found
            }

            // Step 2 Search BaseWords
            if (BaseWords.ContainsKey(word))
            {
                TraceWriter.TraceVerbose("Word Found in Base Words: {0}", word);
                return true; // word found
            }

            // Step 3 Remove suffix, Search BaseWords

            // save suffixed words for use when removing prefix
            var suffixWords = new List<string>();

            // Add word to suffix word list
            suffixWords.Add(word);

            foreach (AffixRule rule in SuffixRules.Values)
            {
                foreach (AffixEntry entry in rule.AffixEntries)
                {
                    string tempWord = AffixUtility.RemoveSuffix(word, entry);
                    if (tempWord != word)
                    {
                        if (BaseWords.ContainsKey(tempWord))
                        {
                            if (VerifyAffixKey(tempWord, rule.Name[0]))
                            {
                                TraceWriter.TraceVerbose("Word Found With Base Words: {0}; Suffix Key: {1}", tempWord, rule.Name[0]);
                                return true; // word found
                            }
                        }

                        if (rule.AllowCombine)
                        {
                            // saving word to check if it is a word after prefix is removed
                            suffixWords.Add(tempWord);
                        }
                        else
                        {
                            // saving possible base words for use in generating suggestions
                            PossibleBaseWords.Add(tempWord);
                        }
                    }
                }
            }

            // saving possible base words for use in generating suggestions
            PossibleBaseWords.AddRange(suffixWords);

            // Step 4 Remove Prefix, Search BaseWords
            foreach (AffixRule rule in PrefixRules.Values)
            {
                foreach (AffixEntry entry in rule.AffixEntries)
                {
                    foreach (string suffixWord in suffixWords)
                    {
                        string tempWord = AffixUtility.RemovePrefix(suffixWord, entry);
                        if (tempWord != suffixWord)
                        {
                            if (BaseWords.ContainsKey(tempWord))
                            {
                                if (VerifyAffixKey(tempWord, rule.Name[0]))
                                {
                                    TraceWriter.TraceVerbose("Word Found With Base Words: {0}; Prefix Key: {1}", tempWord, rule.Name[0]);
                                    return true; // word found
                                }
                            }

                            // saving possible base words for use in generating suggestions
                            PossibleBaseWords.Add(tempWord);
                        }
                    }
                }
            }

            // word not found
            TraceWriter.TraceVerbose("Possible Base Words: {0}", PossibleBaseWords.Count);
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
        ///     A System.Collections.Generic.List(of String) of words expanded from base word
        /// </returns>
        public List<string> ExpandWord(Word word)
        {
            var suffixWords = new List<string>();
            var words = new List<string>();

            suffixWords.Add(word.Text);
            string prefixKeys = "";

            // check suffix keys first
            foreach (char key in word.AffixKeys)
            {
                if (SuffixRules.ContainsKey(key.ToString(CultureInfo.CurrentUICulture)))
                {
                    AffixRule rule = SuffixRules[key.ToString(CultureInfo.CurrentUICulture)];
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
                else if (PrefixRules.ContainsKey(key.ToString(CultureInfo.CurrentUICulture)))
                {
                    prefixKeys += key.ToString(CultureInfo.CurrentUICulture);
                }
            }

            // apply prefixes
            foreach (char key in prefixKeys)
            {
                AffixRule rule = PrefixRules[key.ToString(CultureInfo.CurrentUICulture)];

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
            BaseWords.Clear();
            ReplaceCharacters.Clear();
            PrefixRules.Clear();
            SuffixRules.Clear();
            PhoneticRules.Clear();
            TryCharacters = "";

            // the following is used to split a line by white space
            var spaceRegex = new Regex(@"[^\s]+", RegexOptions.Compiled);

            string currentSection = "";
            AffixRule currentRule = null;
            string dictionaryPath = Path.Combine(DictionaryFolder, _dictionaryFile);

            TraceWriter.TraceInfo("Loading Dictionary:{0}", dictionaryPath);

            // open dictionary file
            FileStream fs = null;
            try
            {
                fs = new FileStream(dictionaryPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using (var sr = new StreamReader(fs, Encoding.UTF8))
                {
                    fs = null;

                    // read line by line
                    while (sr.Peek() >= 0)
                    {
                        string tempLine = sr.ReadLine().Trim();
                        if (tempLine.Length <= 0)
                        {
                            continue;
                        }

                        // check for section flag
                        if (tempLine.StartsWith("[") && tempLine.EndsWith("]"))
                        {
                            // set current section that is being parsed
                            currentSection = tempLine;
                            continue;
                        }

                        // parse line and place in correct object
                        MatchCollection partMatches;
                        switch (currentSection)
                        {
                            case "[Copyright]":
                                Copyright += tempLine + "\r\n";
                                break;
                            case "[Try]": // ISpell try chars
                                TryCharacters += tempLine;
                                break;
                            case "[Replace]": // ISpell replace chars
                                ReplaceCharacters.Add(tempLine);
                                break;
                            case "[Prefix]": // MySpell prefix rules
                            case "[Suffix]": // MySpell suffix rules

                                // split line by white space
                                partMatches = spaceRegex.Matches(tempLine);

                                // if 3 parts, then new rule
                                if (partMatches.Count == 3)
                                {
                                    currentRule = new AffixRule();

                                    // part 1 = affix key
                                    currentRule.Name = partMatches[0].Value;

                                    // part 2 = combine flag
                                    if (partMatches[1].Value == "Y")
                                    {
                                        currentRule.AllowCombine = true;
                                    }

                                    // part 3 = entry count, not used
                                    if (currentSection == "[Prefix]")
                                    {
                                        // add to prefix collection
                                        PrefixRules.Add(currentRule.Name, currentRule);
                                    }
                                    else
                                    {
                                        // add to suffix collection
                                        SuffixRules.Add(currentRule.Name, currentRule);
                                    }
                                }

                                // if 4 parts, then entry for current rule
                                else if (partMatches.Count == 4)
                                {
                                    // part 1 = affix key
                                    if (currentRule.Name == partMatches[0].Value)
                                    {
                                        var entry = new AffixEntry();

                                        // part 2 = strip char
                                        if (partMatches[1].Value != "0")
                                        {
                                            entry.StripCharacters = partMatches[1].Value;
                                        }

                                        // part 3 = add chars
                                        entry.AddCharacters = partMatches[2].Value;

                                        // part 4 = conditions
                                        AffixUtility.EncodeConditions(partMatches[3].Value, entry);

                                        currentRule.AffixEntries.Add(entry);
                                    }
                                }

                                break;
                            case "[Phonetic]": // ASpell phonetic rules
                                // split line by white space
                                partMatches = spaceRegex.Matches(tempLine);
                                if (partMatches.Count >= 2)
                                {
                                    var rule = new PhoneticRule();
                                    PhoneticUtility.EncodeRule(partMatches[0].Value, ref rule);
                                    rule.ReplaceString = partMatches[1].Value;
                                    PhoneticRules.Add(rule);
                                }

                                break;
                            case "[Words]": // dictionary word list
                                // splits word into its parts
                                string[] parts = tempLine.Split('/');
                                var tempWord = new Word();

                                // part 1 = base word
                                tempWord.Text = parts[0];

                                // part 2 = affix keys
                                if (parts.Length >= 2)
                                {
                                    tempWord.AffixKeys = parts[1];
                                }

                                // part 3 = phonetic code
                                if (parts.Length >= 3)
                                {
                                    tempWord.PhoneticCode = parts[2];
                                }

                                BaseWords.Add(tempWord.Text, tempWord);
                                break;
                        }
                    }

                    // close files
                }
            }
            finally
            {
                fs?.Dispose();
            }

            TraceWriter.TraceInfo("Dictionary Loaded BaseWords:{0}; PrefixRules:{1}; SuffixRules:{2}; PhoneticRules:{3}",
                BaseWords.Count, PrefixRules.Count, SuffixRules.Count, PhoneticRules.Count);

            LoadUserFile();

            Initialized = true;
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
            var code = new StringBuilder();

            while (tempWord.Length > 0)
            {
                // save previous word
                var prevWord = tempWord;
                foreach (PhoneticRule rule in PhoneticRules)
                {
                    bool beginning = tempWord.Length == word.Length;
                    bool ending = rule.ConditionCount == tempWord.Length;

                    if ((rule.BeginningOnly == beginning || !rule.BeginningOnly)
                        && (rule.EndOnly == ending || !rule.EndOnly)
                        && rule.ConditionCount <= tempWord.Length)
                    {
                        int passCount = 0;

                        // loop through conditions
                        for (int i = 0; i < rule.ConditionCount; i++)
                        {
                            int charCode = tempWord[i];
                            if ((rule.Condition[charCode] & (1 << i)) == (1 << i))
                            {
                                passCount++; // condition passed
                            }
                            else
                            {
                                break; // rule fails if one condition fails
                            }
                        }

                        // if all conditions passed
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
                }

                // if no match consume one char
                if (prevWord.Length == tempWord.Length)
                {
                    tempWord = tempWord.Substring(1);
                }
            }

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
        ///     This method is only affects the user word list
        /// </remarks>
        public void Remove(string word)
        {
            UserWords.Remove(word);
            SaveUserFile();
        }

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
        ///     The collection of base words for the dictionary
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<string, Word> BaseWords { get; } = new Dictionary<string, Word>();

        /// <summary>
        ///     Copyright text for the dictionary
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Copyright { get; set; } = "";

        /// <summary>
        ///     The file name for the main dictionary
        /// </summary>
        [DefaultValue("en-US.dic")]
        [Category("Dictionary")]
        [Description("The file name for the main dictionary")]
        [NotifyParentProperty(true)]
        public string DictionaryFile
        {
            get => _dictionaryFile;
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
        [Category("Dictionary")]
        [Description("The folder containing dictionaries")]
#if FULL_PROFILE
        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
#endif
        [NotifyParentProperty(true)]
        public string DictionaryFolder { get; set; } = "";

        /// <summary>
        ///     Set this to true to automatically create a user dictionary when
        ///     a word is added.
        /// </summary>
        /// <remarks>
        ///     This should be set to false in a web environment
        /// </remarks>
        [DefaultValue(true)]
        [Category("Options")]
        [Description("Set this to true to automatically create a user dictionary")]
        [NotifyParentProperty(true)]
        public bool EnableUserFile { get; set; } = true;

        /// <summary>
        ///     True if the dictionary has been initialized
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Initialized { get; private set; }

        /// <summary>
        ///     Collection of phonetic rules for this dictionary
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PhoneticRuleCollection PhoneticRules { get; } = new PhoneticRuleCollection();

        /// <summary>
        ///     Collection of affix prefixes for the base words in this dictionary
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AffixRuleCollection PrefixRules { get; } = new AffixRuleCollection();

        /// <summary>
        ///     List of characters to use when generating suggestions using the near miss strategy
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<string> ReplaceCharacters { get; } = new List<string>();

        /// <summary>
        ///     Collection of affix suffixes for the base words in this dictionary
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AffixRuleCollection SuffixRules { get; } = new AffixRuleCollection();

        /// <summary>
        ///     List of characters to try when generating suggestions using the near miss strategy
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TryCharacters { get; set; } = "";

        /// <summary>
        ///     The file name for the user word list for this dictionary
        /// </summary>
        [DefaultValue("user.dic")]
        [Category("Dictionary")]
        [Description("The file name for the user word list for this dictionary")]
        [NotifyParentProperty(true)]
        public string UserFile { get; set; } = "user.dic";

        /// <summary>
        ///     List of user entered words in this dictionary
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Dictionary<string, string> UserWords { get; } = new Dictionary<string, string>();

        /// <summary>
        ///     List of text saved from when 'Contains' is called.
        ///     This list is used to generate suggestions from if Contains
        ///     doesn't find a word.
        /// </summary>
        /// <remarks>
        ///     These are not actual words.
        /// </remarks>
        internal List<string> PossibleBaseWords { get; } = new List<string>();

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _components = new System.ComponentModel.Container();
        }
        #endregion

    }
}
