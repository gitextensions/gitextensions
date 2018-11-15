namespace GitUI.SpellChecker
{
    internal static class SpellCheckerHelper
    {
        public static bool IsSeparator(this char c)
        {
            return !char.IsLetterOrDigit(c) && c != '_';
        }
    }
}