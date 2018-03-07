using GitUIPluginInterfaces;
using ResourceManager;

namespace DeleteUnusedBranches
{
    public class DeleteUnusedBranchesPlugin : GitPluginBase, IGitPluginForRepository
    {
        public DeleteUnusedBranchesPlugin()
        {
            SetNameAndDescription("Delete obsolete branches");
            Translate();
        }

        private StringSetting _MergedInBranch = new StringSetting("Branch where all branches should be merged in", "HEAD");
        private NumberSetting<int> _DaysOlderThan = new NumberSetting<int>("Delete obsolete branches older than (days)", 30);
        private BoolSetting _DeleteRemoteBranchesFromFlag = new BoolSetting("Delete obsolete branches from remote", false);
        private StringSetting _RemoteName = new StringSetting("Remote name obsoleted branches should be deleted from", "origin");
        private BoolSetting _UseRegexToFilterBranchesFlag = new BoolSetting("Use regex to filter branches to delete", false);
        private StringSetting _RegexFilter = new StringSetting("Regex to filter branches to delete", "/(feature|develop)/");
        private BoolSetting _RegexCaseInsensitiveFlag = new BoolSetting("Is regex filter case insensitive?", false);
        private BoolSetting _RegexInvertedFlag = new BoolSetting("Search branches that does not match regex", false);
        private BoolSetting _IncludeUnmergedBranchesFlag = new BoolSetting("Delete unmerged branches", false);

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return _DaysOlderThan;
            yield return _MergedInBranch;
            yield return _DeleteRemoteBranchesFromFlag;
            yield return _RemoteName;
            yield return _UseRegexToFilterBranchesFlag;
            yield return _RegexFilter;
            yield return _RegexCaseInsensitiveFlag;
            yield return _RegexInvertedFlag;
            yield return _IncludeUnmergedBranchesFlag;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiArgs)
        {
            var settings = new DeleteUnusedBranchesFormSettings(
                _DaysOlderThan.ValueOrDefault(Settings),
                _MergedInBranch.ValueOrDefault(Settings),
                _DeleteRemoteBranchesFromFlag.ValueOrDefault(Settings),
                _RemoteName.ValueOrDefault(Settings),
                _UseRegexToFilterBranchesFlag.ValueOrDefault(Settings),
                _RegexFilter.ValueOrDefault(Settings),
                _RegexCaseInsensitiveFlag.ValueOrDefault(Settings),
                _RegexInvertedFlag.ValueOrDefault(Settings),
                _IncludeUnmergedBranchesFlag.ValueOrDefault(Settings));

            using (var frm = new DeleteUnusedBranchesForm(settings, gitUiArgs.GitModule, gitUiArgs.GitUICommands, this))
            {
                frm.ShowDialog(gitUiArgs.OwnerForm);
            }

            return true;
        }
    }
}
