using System;

namespace NetSpell.SpellChecker
{
    /// <summary>
    ///     Class sent to the event handler when the DoubleWord or
    ///     MisspelledWord event occurs
    /// </summary>
    public class SpellingEventArgs : EventArgs
    {
        /// <summary>
        ///     Constructor used to pass in properties
        /// </summary>
        public SpellingEventArgs(string word, int wordIndex, int textIndex)
        {
            Word = word;
            WordIndex = wordIndex;
            TextIndex = textIndex;
        }

        /// <summary>
        ///     Text index of the WordEvent
        /// </summary>
        public int TextIndex { get; }

        /// <summary>
        ///     Word that caused the WordEvent
        /// </summary>
        public string Word { get; }

        /// <summary>
        ///     Word index of the WordEvent
        /// </summary>
        public int WordIndex { get; }
    }
}
