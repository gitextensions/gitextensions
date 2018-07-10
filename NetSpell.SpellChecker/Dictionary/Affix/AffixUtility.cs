namespace NetSpell.SpellChecker.Dictionary.Affix
{
    /// <summary>
    /// Summary description for AffixUtility.
    /// </summary>
    public static class AffixUtility
    {
        /// <summary>
        ///     Adds a prefix to a word
        /// </summary>
        /// <param name="word" type="string">
        ///     <para>
        ///         The word to add the prefix to
        ///     </para>
        /// </param>
        /// <param name="rule" type="NetSpell.SpellChecker.Dictionary.Affix.AffixRule">
        ///     <para>
        ///         The AffixRule to use when adding the prefix
        ///     </para>
        /// </param>
        /// <returns>
        ///     The word with the prefix added
        /// </returns>
        public static string AddPrefix(string word, AffixRule rule)
        {
            foreach (AffixEntry entry in rule.AffixEntries)
            {
                // check that this entry is valid
                if (word.Length >= entry.ConditionCount)
                {
                    int passCount = 0;
                    for (int i = 0; i < entry.ConditionCount; i++)
                    {
                        int charCode = word[i];
                        if ((entry.Condition[charCode] & (1 << i)) == (1 << i))
                        {
                            passCount++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (passCount == entry.ConditionCount)
                    {
                        string tempWord = word.Substring(entry.StripCharacters.Length);
                        tempWord = entry.AddCharacters + tempWord;
                        return tempWord;
                    }
                }
            }

            return word;
        }

        /// <summary>
        ///     Adds a suffix to a word
        /// </summary>
        /// <param name="word" type="string">
        ///     <para>
        ///         The word to get the suffix added to
        ///     </para>
        /// </param>
        /// <param name="rule" type="NetSpell.SpellChecker.Dictionary.Affix.AffixRule">
        ///     <para>
        ///         The AffixRule to use when adding the suffix
        ///     </para>
        /// </param>
        /// <returns>
        ///     The word with the suffix added
        /// </returns>
        public static string AddSuffix(string word, AffixRule rule)
        {
            foreach (AffixEntry entry in rule.AffixEntries)
            {
                // check that this entry is valid
                if (word.Length >= entry.ConditionCount)
                {
                    int passCount = 0;
                    for (int i = 0; i < entry.ConditionCount; i++)
                    {
                        int charCode = word[word.Length - (entry.ConditionCount - i)];
                        if ((entry.Condition[charCode] & (1 << i)) == (1 << i))
                        {
                            passCount++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (passCount == entry.ConditionCount)
                    {
                        int tempLen = word.Length - entry.StripCharacters.Length;
                        string tempWord = word.Substring(0, tempLen);
                        tempWord += entry.AddCharacters;
                        return tempWord;
                    }
                }
            }

            return word;
        }

        /// <summary>
        ///     Generates the condition character array
        /// </summary>
        /// <param name="conditionText" type="string">
        ///     <para>
        ///         the text form of the conditions
        ///     </para>
        /// </param>
        /// <param name="entry" type="NetSpell.SpellChecker.Dictionary.Affix.AffixEntry">
        ///     <para>
        ///         The AffixEntry to add the condition array to
        ///     </para>
        /// </param>
        public static void EncodeConditions(string conditionText, AffixEntry entry)
        {
            // clear the conditions array
            for (int i = 0; i < entry.Condition.Length; i++)
            {
                entry.Condition[i] = 0;
            }

            // if no condition just return
            if (conditionText == ".")
            {
                entry.ConditionCount = 0;
                return;
            }

            bool neg = false;  /* complement indicator */
            bool group = false;  /* group indicator */
            bool end = false;   /* end condition indicator */
            int num = 0;    /* number of conditions */

            char[] memberChars = new char[200];
            int numMember = 0;   /* number of member in group */

            foreach (char cond in conditionText)
            {
                // parse member group
                if (cond == '[')
                {
                    group = true;  // start a group
                }
                else if (cond == '^' && group)
                {
                    neg = true; // negative group
                }
                else if (cond == ']')
                {
                    end = true; // end of a group
                }
                else if (group)
                {
                    // add chars to group
                    memberChars[numMember] = cond;
                    numMember++;
                }
                else
                {
                    end = true;  // no group
                }

                // set condition
                if (end)
                {
                    if (group)
                    {
                        if (neg)
                        {
                            // turn all chars on
                            for (int j = 0; j < entry.Condition.Length; j++)
                            {
                                entry.Condition[j] = entry.Condition[j] | (1 << num);
                            }

                            // turn off chars in member group
                            for (int j = 0; j < numMember; j++)
                            {
                                int charCode = memberChars[j];
                                entry.Condition[charCode] = entry.Condition[charCode] & ~(1 << num);
                            }
                        }
                        else
                        {
                            // turn on chars in member group
                            for (int j = 0; j < numMember; j++)
                            {
                                int charCode = memberChars[j];
                                entry.Condition[charCode] = entry.Condition[charCode] | (1 << num);
                            }
                        }

                        group = false;
                        neg = false;
                        numMember = 0;
                    }
                    else
                    {
                        if (cond == '.')
                        {
                            // wild card character, turn all chars on
                            for (int j = 0; j < entry.Condition.Length; j++)
                            {
                                entry.Condition[j] = entry.Condition[j] | (1 << num);
                            }
                        }
                        else
                        {
                            // turn on char
                            int charCode = cond;
                            entry.Condition[charCode] = entry.Condition[charCode] | (1 << num);
                        }
                    }

                    end = false;
                    num++;
                }
            }

            entry.ConditionCount = num;
        }

        /// <summary>
        ///     Removes the affix prefix rule entry for the word if valid
        /// </summary>
        /// <param name="word" type="string">
        ///     <para>
        ///         The word to be modified
        ///     </para>
        /// </param>
        /// <param name="entry" type="NetSpell.SpellChecker.Dictionary.Affix.AffixEntry">
        ///     <para>
        ///         The affix rule entry to use
        ///     </para>
        /// </param>
        /// <returns>
        ///     The word after affix removed.  Will be the same word if affix could not be removed.
        /// </returns>
        /// <remarks>
        ///     This method does not verify that the returned word is a valid word, only that the affix can be removed
        /// </remarks>
        public static string RemovePrefix(string word, AffixEntry entry)
        {
            int tempLength = word.Length - entry.AddCharacters.Length;
            if ((tempLength > 0)
                && (tempLength + entry.StripCharacters.Length >= entry.ConditionCount)
                && word.StartsWith(entry.AddCharacters))
            {
                // word with out affix
                string tempWord = word.Substring(entry.AddCharacters.Length);

                // add back strip chars
                tempWord = entry.StripCharacters + tempWord;

                // check that this is valid
                int passCount = 0;
                for (int i = 0; i < entry.ConditionCount; i++)
                {
                    int charCode = tempWord[i];
                    if ((entry.Condition[charCode] & (1 << i)) == (1 << i))
                    {
                        passCount++;
                    }
                }

                if (passCount == entry.ConditionCount)
                {
                    return tempWord;
                }
            }

            return word;
        }

        /// <summary>
        ///     Removes the affix suffix rule entry for the word if valid
        /// </summary>
        /// <param name="word" type="string">
        ///     <para>
        ///         The word to be modified
        ///     </para>
        /// </param>
        /// <param name="entry" type="NetSpell.SpellChecker.Dictionary.Affix.AffixEntry">
        ///     <para>
        ///         The affix rule entry to use
        ///     </para>
        /// </param>
        /// <returns>
        ///     The word after affix removed.  Will be the same word if affix could not be removed.
        /// </returns>
        /// <remarks>
        ///     This method does not verify that the returned word is a valid word, only that the affix can be removed
        /// </remarks>
        public static string RemoveSuffix(string word, AffixEntry entry)
        {
            int tempLength = word.Length - entry.AddCharacters.Length;
            if ((tempLength > 0)
                && (tempLength + entry.StripCharacters.Length >= entry.ConditionCount)
                && word.EndsWith(entry.AddCharacters))
            {
                // word with out affix
                string tempWord = word.Substring(0, tempLength);

                // add back strip chars
                tempWord += entry.StripCharacters;

                // check that this is valid
                int passCount = 0;
                for (int i = 0; i < entry.ConditionCount; i++)
                {
                    int charCode = tempWord[tempWord.Length - (entry.ConditionCount - i)];
                    if ((entry.Condition[charCode] & (1 << i)) == (1 << i))
                    {
                        passCount++;
                    }
                }

                if (passCount == entry.ConditionCount)
                {
                    return tempWord;
                }
            }

            return word;
        }
    }
}
