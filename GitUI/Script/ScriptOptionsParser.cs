using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.Script
{
    public sealed class ScriptOptionsParser
    {
        /// <summary>
        /// Gets the list of available script options.
        /// </summary>
        public static readonly IReadOnlyList<string> Options = new[]
        {
            "sHashes",
            "sTag",
            "sBranch",
            "sLocalBranch",
            "sRemoteBranch",
            "sRemoteBranchName",
            "sRemote",
            "sRemoteUrl",
            "sRemotePathFromUrl",
            "sHash",
            "sMessage",
            "sAuthor",
            "sCommitter",
            "sAuthorDate",
            "sCommitDate",
            "cTag",
            "cBranch",
            "cLocalBranch",
            "cRemoteBranch",
            "cRemoteBranchName",
            "cHash",
            "cMessage",
            "cAuthor",
            "cCommitter",
            "cAuthorDate",
            "cCommitDate",
            "cDefaultRemote",
            "cDefaultRemoteUrl",
            "cDefaultRemotePathFromUrl",
            "RepoName",
            "UserInput",
            "UserFiles",
            "WorkingDir"
        };

        /// <summary>
        ///  Checks whether <paramref name="arguments"/> contains the supplied <paramref name="option"/>.
        /// </summary>
        /// <param name="arguments">The script argument.</param>
        /// <param name="option">The option.</param>
        /// <returns><see langword="true"/> if the argument contains the option; otherwise <see langword="false"/>.</returns>
        public static bool Contains([NotNull] string arguments, [NotNull] string option)
        {
            arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
            option = option ?? throw new ArgumentNullException(nameof(option));

            return arguments.Contains(CreateOption(option, quoted: false));
        }

        /// <summary>
        ///  Checks whether <paramref name="option"/> starts with "s".
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns><see langword="true"/> if the options starts with "s"; otherwise <see langword="false"/>.</returns>
        public static bool DependsOnSelectedRevision([NotNull] string option)
        {
            option = option ?? throw new ArgumentNullException(nameof(option));

            return option.StartsWith("s");
        }

        public static (string arguments, bool abort) Parse([CanBeNull] string arguments, [NotNull] IGitModule module, IWin32Window owner, IScriptHostControl scriptHostControl)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                return (arguments, abort: false);
            }

            module = module ?? throw new ArgumentNullException(nameof(module));

            GitRevision selectedRevision = null;
            GitRevision currentRevision = null;

            IReadOnlyList<GitRevision> allSelectedRevisions = Array.Empty<GitRevision>();
            var selectedLocalBranches = new List<IGitRef>();
            var selectedRemoteBranches = new List<IGitRef>();
            var selectedRemotes = new List<string>();
            var selectedBranches = new List<IGitRef>();
            var selectedTags = new List<IGitRef>();
            var currentLocalBranches = new List<IGitRef>();
            var currentRemoteBranches = new List<IGitRef>();
            var currentRemote = "";
            var currentBranches = new List<IGitRef>();
            var currentTags = new List<IGitRef>();

            foreach (string option in Options)
            {
                if (!Contains(arguments, option))
                {
                    continue;
                }

                if (currentRevision == null && option.StartsWith("c"))
                {
                    currentRevision = GetCurrentRevision(module, scriptHostControl, currentTags, currentLocalBranches, currentRemoteBranches, currentBranches);
                    if (currentRevision == null)
                    {
                        return (arguments: null, abort: true);
                    }

                    if (currentLocalBranches.Count == 1)
                    {
                        currentRemote = module.GetSetting(string.Format(SettingKeyString.BranchRemote, currentLocalBranches[0].Name));
                    }
                    else
                    {
                        currentRemote = module.GetCurrentRemote();
                        if (string.IsNullOrEmpty(currentRemote))
                        {
                            currentRemote = module.GetSetting(string.Format(SettingKeyString.BranchRemote,
                                AskToSpecify(currentLocalBranches, scriptHostControl)));
                        }
                    }
                }
                else if (selectedRevision == null && scriptHostControl != null && DependsOnSelectedRevision(option))
                {
                    allSelectedRevisions = scriptHostControl.GetSelectedRevisions() ?? Array.Empty<GitRevision>();
                    selectedRevision = CalculateSelectedRevision(scriptHostControl, selectedRemoteBranches, selectedRemotes, selectedLocalBranches, selectedBranches, selectedTags);
                    if (selectedRevision == null)
                    {
                        return (arguments: null, abort: true);
                    }
                }

                arguments = ParseScriptArguments(arguments, option, owner, scriptHostControl, module, allSelectedRevisions, selectedTags, selectedBranches, selectedLocalBranches, selectedRemoteBranches, selectedRemotes, selectedRevision, currentTags, currentBranches, currentLocalBranches, currentRemoteBranches, currentRevision, currentRemote);
                if (arguments == null)
                {
                    return (arguments: null, abort: true);
                }
            }

            return (arguments, abort: false);
        }

        private static string AskToSpecify(IEnumerable<IGitRef> options, IScriptHostControl scriptHostControl)
        {
            var items = options.ToList();
            if (items.Count == 0)
            {
                return string.Empty;
            }

            using (var f = new FormQuickGitRefSelector())
            {
                f.Location = scriptHostControl?.GetQuickItemSelectorLocation() ?? new System.Drawing.Point();
                f.Init(FormQuickGitRefSelector.Action.Select, items);
                f.ShowDialog();
                return f.SelectedRef.Name;
            }
        }

        private static string AskToSpecify(IEnumerable<string> options, IScriptHostControl scriptHostControl)
        {
            using (var f = new FormQuickStringSelector())
            {
                f.Location = scriptHostControl?.GetQuickItemSelectorLocation() ?? new System.Drawing.Point();
                f.Init(options.ToList());
                f.ShowDialog();
                return f.SelectedString;
            }
        }

        private static GitRevision CalculateSelectedRevision(IScriptHostControl scriptHostControl, List<IGitRef> selectedRemoteBranches,
            List<string> selectedRemotes, List<IGitRef> selectedLocalBranches,
            List<IGitRef> selectedBranches, List<IGitRef> selectedTags)
        {
            GitRevision selectedRevision = scriptHostControl?.GetLatestSelectedRevision();
            if (selectedRevision == null)
            {
                return null;
            }

            foreach (var head in selectedRevision.Refs)
            {
                if (head.IsTag)
                {
                    selectedTags.Add(head);
                }
                else if (head.IsHead || head.IsRemote)
                {
                    selectedBranches.Add(head);
                    if (head.IsRemote)
                    {
                        selectedRemoteBranches.Add(head);
                        if (!selectedRemotes.Contains(head.Remote))
                        {
                            selectedRemotes.Add(head.Remote);
                        }
                    }
                    else
                    {
                        selectedLocalBranches.Add(head);
                    }
                }
            }

            return selectedRevision;
        }

        [NotNull]
        private static string CreateOption([NotNull] string option, bool quoted)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                return string.Empty;
            }

            var result = "{" + option.Trim() + "}";

            if (quoted)
            {
                result = "{" + result + "}";
            }

            return result;
        }

        [CanBeNull]
        private static GitRevision GetCurrentRevision(
            [NotNull] IGitModule module, [CanBeNull] IScriptHostControl scriptHostControl, List<IGitRef> currentTags, List<IGitRef> currentLocalBranches,
            List<IGitRef> currentRemoteBranches, List<IGitRef> currentBranches)
        {
            GitRevision currentRevision;
            IEnumerable<IGitRef> refs;
            if (scriptHostControl == null)
            {
                var currentRevisionGuid = module.GetCurrentCheckout();
                currentRevision = currentRevisionGuid == null ? null : new GitRevision(currentRevisionGuid);
                refs = module.GetRefs(true, true).Where(gitRef => gitRef.ObjectId == currentRevisionGuid);
            }
            else
            {
                currentRevision = scriptHostControl.GetCurrentRevision();
                refs = currentRevision?.Refs ?? Array.Empty<IGitRef>();
            }

            foreach (var gitRef in refs)
            {
                if (gitRef.IsTag)
                {
                    currentTags.Add(gitRef);
                }
                else if (gitRef.IsHead || gitRef.IsRemote)
                {
                    currentBranches.Add(gitRef);
                    if (gitRef.IsRemote)
                    {
                        currentRemoteBranches.Add(gitRef);
                    }
                    else
                    {
                        currentLocalBranches.Add(gitRef);
                    }
                }
            }

            return currentRevision;
        }

        private static string GetRemotePath(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                Uri.TryCreate("ssh://" + url.Replace(":", "/"), UriKind.Absolute, out uri))
            {
                return uri.LocalPath.SubstringUntilLast('.');
            }

            return "";
        }

        private static string ParseScriptArguments([NotNull] string arguments, [NotNull] string option, IWin32Window owner,
            IScriptHostControl scriptHostControl, IGitModule module, IReadOnlyList<GitRevision> allSelectedRevisions,
            in IList<IGitRef> selectedTags, in IList<IGitRef> selectedBranches, in IList<IGitRef> selectedLocalBranches,
            in IList<IGitRef> selectedRemoteBranches, in IList<string> selectedRemotes, GitRevision selectedRevision,
            in IList<IGitRef> currentTags, in IList<IGitRef> currentBranches, in IList<IGitRef> currentLocalBranches,
            in IList<IGitRef> currentRemoteBranches, GitRevision currentRevision, string currentRemote)
        {
            string newString = null;
            string remote;
            string url;
            switch (option)
            {
                case "sHashes":
                    newString = string.Join(" ", allSelectedRevisions.Select(revision => revision.Guid).ToArray());
                    break;

                case "sTag":
                    newString = SelectOneRef(selectedTags);
                    break;

                case "sBranch":
                    newString = SelectOneRef(selectedBranches);
                    break;

                case "sLocalBranch":
                    newString = SelectOneRef(selectedLocalBranches);
                    break;

                case "sRemoteBranch":
                    newString = SelectOneRef(selectedRemoteBranches);
                    break;

                case "sRemoteBranchName":
                    newString = StripRemoteName(SelectOneRef(selectedRemoteBranches));
                    break;

                case "sRemote":
                    newString = SelectOneString(selectedRemotes);
                    break;

                case "sRemoteUrl":
                    newString = SelectOneString(selectedRemotes);
                    if (!string.IsNullOrEmpty(newString))
                    {
                        remote = newString;
                        newString = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
                    }

                    break;

                case "sRemotePathFromUrl":
                    newString = SelectOneString(selectedRemotes);
                    if (!string.IsNullOrEmpty(newString))
                    {
                        remote = newString;
                        url = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
                        newString = GetRemotePath(url);
                    }

                    break;

                case "sHash":
                    newString = selectedRevision.Guid;
                    break;

                case "sMessage":
                    newString = selectedRevision.Subject;
                    break;

                case "sAuthor":
                    newString = selectedRevision.Author;
                    break;

                case "sCommitter":
                    newString = selectedRevision.Committer;
                    break;

                case "sAuthorDate":
                    newString = selectedRevision.AuthorDate.ToString();
                    break;

                case "sCommitDate":
                    newString = selectedRevision.CommitDate.ToString();
                    break;

                case "cTag":
                    newString = SelectOneRef(currentTags);
                    break;

                case "cBranch":
                    newString = SelectOneRef(currentBranches);
                    break;

                case "cLocalBranch":
                    newString = SelectOneRef(currentLocalBranches);
                    break;

                case "cRemoteBranch":
                    newString = SelectOneRef(currentRemoteBranches);
                    break;

                case "cRemoteBranchName":
                    newString = StripRemoteName(SelectOneRef(currentRemoteBranches));
                    break;

                case "cHash":
                    newString = currentRevision.Guid;
                    break;

                case "cMessage":
                    newString = currentRevision.Subject;
                    break;

                case "cAuthor":
                    newString = currentRevision.Author;
                    break;

                case "cCommitter":
                    newString = currentRevision.Committer;
                    break;

                case "cAuthorDate":
                    newString = currentRevision.AuthorDate.ToString();
                    break;

                case "cCommitDate":
                    newString = currentRevision.CommitDate.ToString();
                    break;

                case "cDefaultRemote":
                    if (string.IsNullOrEmpty(currentRemote))
                    {
                        newString = "";
                    }
                    else
                    {
                        newString = currentRemote;
                    }

                    break;

                case "cDefaultRemoteUrl":
                    if (string.IsNullOrEmpty(currentRemote))
                    {
                        newString = "";
                    }
                    else
                    {
                        newString = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote));
                    }

                    break;

                case "cDefaultRemotePathFromUrl":
                    if (string.IsNullOrEmpty(currentRemote))
                    {
                        newString = "";
                    }
                    else
                    {
                        url = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote));
                        newString = GetRemotePath(url);
                    }

                    break;

                case "RepoName":
                    newString = module == null ? string.Empty : new RepositoryDescriptionProvider(new GitDirectoryResolver()).Get(module.WorkingDir);
                    break;

                case "UserInput":
                    using (var prompt = new SimplePrompt())
                    {
                        prompt.ShowDialog();
                        newString = prompt.UserInput;
                    }

                    break;

                case "UserFiles":
                    using (FormFilePrompt prompt = new FormFilePrompt())
                    {
                        if (prompt.ShowDialog(owner) != DialogResult.OK)
                        {
                            // abort parsing as the user chose to abort
                            return null;
                        }

                        newString = prompt.FileInput;
                        break;
                    }

                case "WorkingDir":
                    newString = module == null ? string.Empty : module.WorkingDir;
                    break;
            }

            if (newString != null)
            {
                string newStringQuoted = Regex.Replace(newString, @"(?<!\\)""", "\\\"");
                newStringQuoted = "\"" + newStringQuoted;
                if (newStringQuoted.EndsWith("\\"))
                {
                    newStringQuoted = newStringQuoted + "\\";
                }

                newStringQuoted = newStringQuoted + "\"";

                arguments = arguments.Replace(CreateOption(option, quoted: true), newStringQuoted);
                arguments = arguments.Replace(CreateOption(option, quoted: false), newString);
            }

            return arguments;

            string SelectOneRef(IList<IGitRef> refs) => ScriptOptionsParser.SelectOne(refs, scriptHostControl);
            string SelectOneString(IList<string> strings) => ScriptOptionsParser.SelectOne(strings, scriptHostControl);
        }

        private static string SelectOne(IList<IGitRef> refs, IScriptHostControl scriptHostControl)
        {
            switch (refs.Count)
            {
                case 0: return string.Empty;
                case 1: return refs[0].Name;
                default: return AskToSpecify(refs, scriptHostControl);
            }
        }

        private static string SelectOne(IList<string> strings, IScriptHostControl scriptHostControl)
        {
            switch (strings.Count)
            {
                case 0: return string.Empty;
                case 1: return strings[0];
                default: return AskToSpecify(strings, scriptHostControl);
            }
        }

        private static string StripRemoteName(string remoteBranchName)
        {
            if (string.IsNullOrEmpty(remoteBranchName))
            {
                return string.Empty;
            }

            int firstSlashIndex = remoteBranchName.IndexOf('/');
            if (firstSlashIndex >= 0)
            {
                return remoteBranchName.Substring(firstSlashIndex + 1);
            }

            return remoteBranchName;
        }

        internal static TestAccessor GetTestAccessor() => new TestAccessor();

        internal readonly struct TestAccessor
        {
            public string CreateOption([NotNull] string option, bool quoted)
                => ScriptOptionsParser.CreateOption(option, quoted);

            public GitRevision GetCurrentRevision(IGitModule module, [CanBeNull] IScriptHostControl scriptHostControl, List<IGitRef> currentTags,
                List<IGitRef> currentLocalBranches, List<IGitRef> currentRemoteBranches, List<IGitRef> currentBranches)
                => ScriptOptionsParser.GetCurrentRevision(module, scriptHostControl, currentTags, currentLocalBranches, currentRemoteBranches, currentBranches);

            public string ParseScriptArguments(string arguments, string option, IWin32Window owner, IScriptHostControl scriptHostControl,
                IGitModule module, IReadOnlyList<GitRevision> allSelectedRevisions, List<IGitRef> selectedTags, List<IGitRef> selectedBranches,
                List<IGitRef> selectedLocalBranches, List<IGitRef> selectedRemoteBranches, List<string> selectedRemotes, GitRevision selectedRevision,
                List<IGitRef> currentTags, List<IGitRef> currentBranches, List<IGitRef> currentLocalBranches, List<IGitRef> currentRemoteBranches,
                GitRevision currentRevision, string currentRemote)
                => ScriptOptionsParser.ParseScriptArguments(arguments, option, owner, scriptHostControl, module, allSelectedRevisions,
                    selectedTags, selectedBranches, selectedLocalBranches, selectedRemoteBranches, selectedRemotes, selectedRevision,
                    currentTags, currentBranches, currentLocalBranches, currentRemoteBranches, currentRevision, currentRemote);
        }
    }
}
