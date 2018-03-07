using System.Text;

namespace GitUI.SpellChecker
{
    internal class WordAtCursorExtractor : IWordAtCursorExtractor
    {
        public string Extract(string text, int cursor)
        {
            if (cursor < 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            while (cursor >= 0)
            {
                if (text[cursor].IsSeparator() && !IsDot(text[cursor]))
                {
                    break;
                }

                if (IsDot(text[cursor]) && !IsLeadingChar(text, cursor))
                {
                    break;
                }

                sb.Insert(0, text[cursor--]);
            }

            return sb.ToString();
        }

        private static bool IsDot(char c)
        {
            return c == '.';
        }

        private static bool IsLeadingChar(string text, int cursor)
        {
            return cursor == 0 || IsSeparatorExceptClosingBrackets(text[cursor - 1]);
        }

        private static bool IsSeparatorExceptClosingBrackets(char c)
        {
            return c != ')' && c != ']' && c.IsSeparator();
        }
    }

    internal interface IWordAtCursorExtractor
    {
        string Extract(string text, int cursor);
    }
}