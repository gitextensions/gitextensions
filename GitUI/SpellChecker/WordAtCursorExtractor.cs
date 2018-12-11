using System;
using System.Text;

namespace GitUI.SpellChecker
{
    internal class WordAtCursorExtractor : IWordAtCursorExtractor
    {
        public string Extract(string text, int index)
        {
            int start = FindStartOfWord(text, index);
            return start < 0 ? string.Empty : text.Substring(start, index - start + 1);
        }

        public (int start, int length) GetWordBounds(string text, int index)
        {
            int start = Math.Min(FindStartOfWord(text, index), index);
            if (start < 0)
            {
                return (0, 0);
            }

            int end = FindEndOfWord(text, index);
            return (start, Math.Max(1, end - start));
        }

        internal int FindStartOfWord(string text, int index)
        {
            index = Math.Min(index, text.Length - 1);
            if (index < 0)
            {
                return -1;
            }

            // do not avoid that the result can be right from the initial index index
            #if false
            if (!BelongsToStartOfWord(text, index))
            {
                return index;
            }
            #endif

            while (index >= 0 && BelongsToStartOfWord(text, index))
            {
                --index;
            }

            return index + 1;
        }

        internal int FindEndOfWord(string text, int index)
        {
            index = Math.Min(index, text.Length);
            if (index < 0)
            {
                return -1;
            }

            while (index < text.Length && BelongsToWord(text[index]))
            {
                ++index;
            }

            return index;
        }

        private static bool BelongsToStartOfWord(string text, int index)
        {
            return IsDot(text[index]) ? IsLeadingChar(text, index) : BelongsToWord(text[index]);
        }

        private static bool BelongsToWord(char c)
        {
            return !c.IsSeparator();
        }

        private static bool IsDot(char c)
        {
            return c == '.';
        }

        private static bool IsLeadingChar(string text, int index)
        {
            return index == 0 || IsSeparatorExceptClosingBrackets(text[index - 1]);
        }

        private static bool IsSeparatorExceptClosingBrackets(char c)
        {
            return c != ')' && c != ']' && c.IsSeparator();
        }
    }

    internal interface IWordAtCursorExtractor
    {
        string Extract(string text, int index);
        (int start, int length) GetWordBounds(string text, int index);
    }
}