namespace GitUI.SpellChecker
{
    internal static class SpellCheckerHelper
    {
        public static bool IsSeparator(this char c)
            => !"_+-".Contains(c) && !char.IsLetterOrDigit(c);
    }
}
