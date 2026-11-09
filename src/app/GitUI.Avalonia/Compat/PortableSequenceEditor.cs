namespace GitUI.Compat;

/// <summary>
///  Keeps the original first-pick edit/reword operation portable to the macOS system tools.
/// </summary>
internal static class PortableSequenceEditor
{
    private static Func<bool> _isMacOS = OperatingSystem.IsMacOS;

    internal static string GetCommand(string command)
        => GetCommand(command, _isMacOS());

    internal static string GetCommand(string command, bool isMacOS)
    {
        if (command is not ("e" or "r"))
        {
            throw new ArgumentException("The sequence-editor action must be edit or reword.", nameof(command));
        }

        if (!isMacOS)
        {
            return string.Format("sed -i -re '0,/pick/s//{0}/'", command);
        }

        string script = "todo=$1; temporary=$(mktemp \"${todo}.XXXXXX\") || exit; "
            + "trap 'rm -f \"$temporary\"' 0; "
            + "awk '!changed && /pick/ { sub(/pick/, \"" + command + "\"); changed=1 } { print }' \"$todo\" "
            + "> \"$temporary\" && cat \"$temporary\" > \"$todo\"";
        return "sh -c " + PlatformToolLocator.QuoteShell(script) + " gitextensions-sequence-editor";
    }

    // parity-scaffolding: substitutes only the operating-system answer for actual revision-grid command tests.
    internal static class TestAccessor
    {
        internal static Func<bool> IsMacOS
        {
            get => _isMacOS;
            set => _isMacOS = value;
        }
    }
}
