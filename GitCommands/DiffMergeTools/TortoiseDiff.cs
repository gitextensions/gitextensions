namespace GitCommands.DiffMergeTools
{
    /// <summary>
    /// Alias for name 'tortoisemerge' as Git disables difftools with that name:
    /// https://github.com/git/git/pull/471#issuecomment-660205096
    /// </summary>
    internal class TortoiseDiff : TortoiseGitMerge
    {
        /// <inheritdoc />
        public override bool IsDiffTool => true;

        /// <inheritdoc />
        public override string Name => "tortoisediff";
    }
}
