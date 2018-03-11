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

        private StringSetting _mergedInBranch = new StringSetting("Branch where all branches should be merged in", "HEAD");
        private NumberSetting<int> _daysOlderThan = new NumberSetting<int>("Delete obsolete branches older than (days)", 30);
        private BoolSetting _deleteRemoteBranchesFromFlag = new BoolSetting("Delete obsolete branches from remote", false);
        private StringSetting _remoteName = new StringSetting("Remote name obsoleted branches should be deleted from", "origin");
        private BoolSetting _useRegexToFilterBranchesFlag = new BoolSetting("Use regex to filter branches to delete", false);
        private StringSetting _regexFilter = new StringSetting("Regex to filter branches to delete", "/(feature|develop)/");
        private BoolSetting _regexCaseInsensitiveFlag = new BoolSetting("Is regex filter case insensitive?", false);
        private BoolSetting _regexInvertedFlag = new BoolSetting("Search branches that does not match regex", false);
        private BoolSetting _includeUnmergedBranchesFlag = new BoolSetting("Delete unmerged branches", false);

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return _daysOlderThan;
            yield return _mergedInBranch;
            yield return _deleteRemoteBranchesFromFlag;
            yield return _remoteName;
            yield return _useRegexToFilterBranchesFlag;
            yield return _regexFilter;
            yield return _regexCaseInsensitiveFlag;
            yield return _regexInvertedFlag;
            yield return _includeUnmergedBranchesFlag;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiArgs)
        {
            var settings = new DeleteUnusedBranchesFormSettings(
                _daysOlderThan.ValueOrDefault(Settings),
                _mergedInBranch.ValueOrDefault(Settings),
                _deleteRemoteBranchesFromFlag.ValueOrDefault(Settings),
                _remoteName.ValueOrDefault(Settings),
                _useRegexToFilterBranchesFlag.ValueOrDefault(Settings),
                _regexFilter.ValueOrDefault(Settings),
                _regexCaseInsensitiveFlag.ValueOrDefault(Settings),
                _regexInvertedFlag.ValueOrDefault(Settings),
                _includeUnmergedBranchesFlag.ValueOrDefault(Settings));

            using (var frm = new DeleteUnusedBranchesForm(settings, gitUiArgs.GitModule, gitUiArgs.GitUICommands, this))
            {
                frm.ShowDialog(gitUiArgs.OwnerForm);
            }

            return true;
        }
    }
}
