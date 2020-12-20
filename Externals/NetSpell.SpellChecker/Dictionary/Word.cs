using System;

namespace NetSpell.SpellChecker.Dictionary
{
    /// <summary>
    /// The Word class represents a base word in the dictionary
    /// </summary>
    public sealed class Word : IComparable<Word>
    {
        /// <summary>
        ///     Initializes a new instance of the class
        /// </summary>
        public Word()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the class
        /// </summary>
        /// <param name="text" type="string">
        ///     <para>
        ///         The string for the base word
        ///     </para>
        /// </param>
        /// <param name="affixKeys" type="string">
        ///     <para>
        ///         The affix keys that can be applied to this base word
        ///     </para>
        /// </param>
        /// <param name="phoneticCode" type="string">
        ///     <para>
        ///         The phonetic code for this word
        ///     </para>
        /// </param>
        public Word(string text, string affixKeys, string phoneticCode)
        {
            Text = text;
            AffixKeys = affixKeys;
            PhoneticCode = phoneticCode;
        }

        /// <summary>
        ///     Initializes a new instance of the class
        /// </summary>
        /// <param name="text" type="string">
        ///     <para>
        ///         The string for the base word
        ///     </para>
        /// </param>
        /// <param name="affixKeys" type="string">
        ///     <para>
        ///         The affix keys that can be applied to this base word
        ///     </para>
        /// </param>
        public Word(string text, string affixKeys)
        {
            Text = text;
            AffixKeys = affixKeys;
        }

        /// <summary>
        ///     Initializes a new instance of the class
        /// </summary>
        /// <param name="text" type="string">
        ///     <para>
        ///         The string for the base word
        ///     </para>
        /// </param>
        public Word(string text)
        {
            Text = text;
        }

        /// <summary>
        ///     Initializes a new instance of the class
        /// </summary>
        /// <param name="text" type="string">
        ///     <para>
        ///         The string for the word
        ///     </para>
        /// </param>
        /// <param name="index" type="int">
        ///     <para>
        ///         The position index of this word
        ///     </para>
        /// </param>
        /// <param name="height" type="int">
        ///     <para>
        ///         The line height of this word
        ///     </para>
        /// </param>
        /// <returns>
        ///     A void value...
        /// </returns>
        internal Word(string text, int index, int height)
        {
            Text = text;
            Index = index;
            Height = height;
        }

        /// <summary>
        ///     Initializes a new instance of the class
        /// </summary>
        /// <param name="text" type="string">
        ///     <para>
        ///         The string for the base word
        ///     </para>
        /// </param>
        /// <param name="editDistance" type="int">
        ///     <para>
        ///         The edit distance from the misspelled word
        ///     </para>
        /// </param>
        internal Word(string text, int editDistance)
        {
            Text = text;
            EditDistance = editDistance;
        }

        /// <summary>
        ///     Sorts a collection of words by EditDistance
        /// </summary>
        /// <remarks>
        ///     The compare sorts in desc order, largest EditDistance first
        /// </remarks>
        public int CompareTo(Word word)
        {
            int result = EditDistance.CompareTo(word.EditDistance);
            return result; // * -1; // sorts desc order
        }

        /// <summary>
        ///     The affix keys that can be applied to this base word
        /// </summary>
        public string AffixKeys { get; set; } = "";

        /// <summary>
        ///     The index position of where this word appears
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     The phonetic code for this word
        /// </summary>
        public string PhoneticCode { get; set; } = "";

        /// <summary>
        ///     The string for the base word
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        ///     Used for sorting suggestions by its edit distance for
        ///     the misspelled word
        /// </summary>
        internal int EditDistance { get; set; }

        /// <summary>
        ///     The line height of this word
        /// </summary>
        internal int Height { get; set; }

        /// <summary>
        ///     Converts the word object to a string
        /// </summary>
        /// <returns>
        ///     Returns the Text Property contents
        /// </returns>
        public override string ToString()
        {
            return Text;
        }
    }
}
