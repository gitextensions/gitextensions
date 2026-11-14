namespace GitUI.SpellChecker;

internal static class SpellCheckerHelper
{
    public static bool IsSeparator(this char c)
        => c switch
        {
            '_' or '+' or '-' => false,
            _ => !char.IsLetterOrDigit(c)
        };
}
