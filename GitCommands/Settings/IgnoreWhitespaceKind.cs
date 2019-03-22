namespace GitCommands.Settings
{
    public enum IgnoreWhitespaceKind
    {
        // DO NOT RENAME THESE -- doing so will break user preferences

        /// <summary>
        /// Do not ignore whitespace.
        /// </summary>
        None = 0,

        /// <summary>
        /// Ignore changes in whitespace at EOL.
        /// </summary>
        /// <see href="https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---ignore-space-at-eol" />
        Eol,

        /// <summary>
        /// Ignore changes in amount of whitespace. This ignores whitespace at line end, and considers all other sequences of one or more whitespace characters to be equivalent.
        /// </summary>
        /// <see href="https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---ignore-space-change" />
        Change,

        /// <summary>
        /// Ignore whitespace when comparing lines. This ignores differences even if one line has whitespace where the other line has none.
        /// </summary>
        /// <see href="https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---ignore-all-space" />
        AllSpace
    }
}
