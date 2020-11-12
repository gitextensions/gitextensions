using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using DeleteUnusedBranches.Properties;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;

namespace DeleteUnusedBranches
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginForRepository,
        IGitPluginConfigurable,
        IGitPluginExecutable
    {
        private readonly StringSetting _mergedInBranch = new StringSetting("Branch where all branches should be merged in", Strings.MergedInBranch, "HEAD");
        private readonly NumberSetting<int> _daysOlderThan = new NumberSetting<int>("Delete obsolete branches older than (days)", Strings.DaysOlderThan, 30);
        private readonly BoolSetting _deleteRemoteBranchesFromFlag = new BoolSetting("Delete obsolete branches from remote", Strings.DeleteRemoteBranchesFromFlag, false);
        private readonly StringSetting _remoteName = new StringSetting("Remote name obsoleted branches should be deleted from", Strings.RemoteName, "origin");
        private readonly BoolSetting _useRegexToFilterBranchesFlag = new BoolSetting("Use regex to filter branches to delete", Strings.UseRegexToFilterBranchesFlag, false);
        private readonly StringSetting _regexFilter = new StringSetting("Regex to filter branches to delete", Strings.RegexFilter, "/(feature|develop)/");
        private readonly BoolSetting _regexCaseInsensitiveFlag = new BoolSetting("Is regex filter case insensitive?", Strings.RegexCaseInsensitiveFlag, false);
        private readonly BoolSetting _regexInvertedFlag = new BoolSetting("Search branches that does not match regex", Strings.RegexInvertedFlag, false);
        private readonly BoolSetting _includeUnmergedBranchesFlag = new BoolSetting("Delete unmerged branches", Strings.IncludeUnmergedBranchesFlag, false);

        public string Name => "Delete obsolete branches";

        public string Description => Strings.Description;

        public Image Icon => Images.IconDeleteUnusedBranches;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public IEnumerable<ISetting> GetSettings()
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

        public bool Execute(GitUIEventArgs args)
        {
            var settings = new DeleteUnusedBranchesFormSettings(
                _daysOlderThan.ValueOrDefault(SettingsContainer.GetSettingsSource()),
                _mergedInBranch.ValueOrDefault(SettingsContainer.GetSettingsSource()),
                _deleteRemoteBranchesFromFlag.ValueOrDefault(SettingsContainer.GetSettingsSource()),
                _remoteName.ValueOrDefault(SettingsContainer.GetSettingsSource()),
                _useRegexToFilterBranchesFlag.ValueOrDefault(SettingsContainer.GetSettingsSource()),
                _regexFilter.ValueOrDefault(SettingsContainer.GetSettingsSource()),
                _regexCaseInsensitiveFlag.ValueOrDefault(SettingsContainer.GetSettingsSource()),
                _regexInvertedFlag.ValueOrDefault(SettingsContainer.GetSettingsSource()),
                _includeUnmergedBranchesFlag.ValueOrDefault(SettingsContainer.GetSettingsSource()));

            using var frm = new DeleteUnusedBranchesForm(settings, args.GitModule, args.GitUICommands, this);

            frm.ShowDialog(args.OwnerForm);

            return true;
        }
    }
}
