using System.ComponentModel.Composition;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Plugins.DeleteUnusedBranches.Properties;

namespace GitExtensions.Plugins.DeleteUnusedBranches
{
    [Export(typeof(IGitPlugin))]
    public class DeleteUnusedBranchesPlugin : GitPluginBase, IGitPluginForRepository
    {
        public DeleteUnusedBranchesPlugin() : base(true)
        {
            Id = new Guid("DC3CA904-B9A5-4FE8-BF63-5B8EE9C2DDAC");
            Name = "Delete obsolete branches";
            Translate(AppSettings.CurrentTranslation);
            Icon = Resources.IconDeleteUnusedBranches;
        }

        private readonly StringSetting _mergedInBranch = new("Branch where all branches should be merged in", "HEAD");
        private readonly NumberSetting<int> _daysOlderThan = new("Delete obsolete branches older than (days)", 30);
        private readonly BoolSetting _deleteRemoteBranchesFromFlag = new("Delete obsolete branches from remote", false);
        private readonly StringSetting _remoteName = new("Remote name obsoleted branches should be deleted from", "origin");
        private readonly BoolSetting _useRegexToFilterBranchesFlag = new("Use regex to filter branches to delete", false);
        private readonly StringSetting _regexFilter = new("Regex to filter branches to delete", "/(feature|develop)/");
        private readonly BoolSetting _regexCaseInsensitiveFlag = new("Is regex filter case insensitive?", false);
        private readonly BoolSetting _regexInvertedFlag = new("Search branches that does not match regex", false);
        private readonly BoolSetting _includeUnmergedBranchesFlag = new("Delete unmerged branches", false);

        public override IEnumerable<ISetting> GetSettings()
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

        public override bool Execute(GitUIEventArgs args)
        {
            DeleteUnusedBranchesFormSettings settings = new(
                _daysOlderThan.ValueOrDefault(Settings),
                _mergedInBranch.ValueOrDefault(Settings),
                _deleteRemoteBranchesFromFlag.ValueOrDefault(Settings),
                _remoteName.ValueOrDefault(Settings),
                _useRegexToFilterBranchesFlag.ValueOrDefault(Settings),
                _regexFilter.ValueOrDefault(Settings),
                _regexCaseInsensitiveFlag.ValueOrDefault(Settings),
                _regexInvertedFlag.ValueOrDefault(Settings),
                _includeUnmergedBranchesFlag.ValueOrDefault(Settings));

            using DeleteUnusedBranchesForm frm = new(settings, args.GitModule, args.GitUICommands, this);
            frm.ShowDialog(args.OwnerForm);

            return frm.HasDeletedBranch;
        }
    }
}
