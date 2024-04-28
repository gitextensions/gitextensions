namespace GitCommands.Settings
{
    public enum DiffDisplayAppearance
    {
        // DO NOT RENAME THESE -- doing so will break user preferences

        /// <summary>
        /// Show as patches.
        /// </summary>
        Patch = 0,

        /// <summary>
        /// Git word diff coloring.
        /// </summary>
        /// <see href="https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---word-diffltmodegt" />
        GitWordDiff = 1,

        /// <summary>
        /// Structural diff display, not patch.
        /// </summary>
        /// <see href="https://difftastic.wilfred.me.uk/" />
        Difftastic = 2,
    }
}
