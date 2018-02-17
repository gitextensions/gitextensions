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

        private readonly StringSetting MergedInBranch = new StringSetting("Branch where all branches should be merged in", "HEAD");
        private readonly NumberSetting<int> DaysOlderThan = new NumberSetting<int>("Delete obsolete branches older than (days)", 30);
        private readonly BoolSetting DeleteRemoteBranchesFromFlag = new BoolSetting("Delete obsolete branches from remote", false);
        private readonly StringSetting RemoteName = new StringSetting("Remote name obsoleted branches should be deleted from", "origin");
        private readonly BoolSetting UseRegexToFilterBranchesFlag = new BoolSetting("Use regex to filter branches to delete", false);
        private readonly StringSetting RegexFilter = new StringSetting("Regex to filter branches to delete", "/(feature|develop)/");
        private readonly BoolSetting RegexCaseInsensitiveFlag = new BoolSetting("Is regex filter case insensitive?", false);
        private readonly BoolSetting RegexInvertedFlag = new BoolSetting("Search branches that does not match regex", false);
        private readonly BoolSetting IncludeUnmergedBranchesFlag = new BoolSetting("Delete unmerged branches", false);

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return DaysOlderThan;
            yield return MergedInBranch;
            yield return DeleteRemoteBranchesFromFlag;
            yield return RemoteName;
            yield return UseRegexToFilterBranchesFlag;
            yield return RegexFilter;
            yield return RegexCaseInsensitiveFlag;
            yield return RegexInvertedFlag;
            yield return IncludeUnmergedBranchesFlag;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiArgs)
        {
            var settings = new DeleteUnusedBranchesFormSettings(
                DaysOlderThan.ValueOrDefault(Settings),
                MergedInBranch.ValueOrDefault(Settings),
                DeleteRemoteBranchesFromFlag.ValueOrDefault(Settings),
                RemoteName.ValueOrDefault(Settings),
                UseRegexToFilterBranchesFlag.ValueOrDefault(Settings),
                RegexFilter.ValueOrDefault(Settings),
                RegexCaseInsensitiveFlag.ValueOrDefault(Settings),
                RegexInvertedFlag.ValueOrDefault(Settings),
                IncludeUnmergedBranchesFlag.ValueOrDefault(Settings));

            using (var frm = new DeleteUnusedBranchesForm(settings, gitUiArgs.GitModule, gitUiArgs.GitUICommands, this))
            {
                frm.ShowDialog(gitUiArgs.OwnerForm);
            }

            return true;
        }
    }
}
