namespace GitExtUtils;

public static class DisplayWithSuffixUpdater
{
    public static string UpdateSuffix(this string currentText, string suffix)
    {
        // NO-BREAK SPACE
        const string specialSpaceSeparator = "\u00A0";
        int indexSuffix = currentText.LastIndexOf(specialSpaceSeparator);
        string textWithoutSuffix = indexSuffix < 0 ? currentText : currentText[..indexSuffix];
        return string.IsNullOrWhiteSpace(suffix) ? textWithoutSuffix : $"{textWithoutSuffix}{specialSpaceSeparator}{suffix}";
    }

    public static string UpdateSuffixWithinParenthesis(this string currentText, string suffix)
        => currentText.UpdateSuffix($"({suffix})");
}
