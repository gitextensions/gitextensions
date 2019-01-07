using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.Script
{
    public sealed class ScriptOptionsParser
    {
        public static readonly IReadOnlyList<string> Options = new[]
        {
            "sHashes",
            "sTag",
            "sBranch",
            "sLocalBranch",
            "sRemoteBranch",
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
            "cHash",
            "cMessage",
            "cAuthor",
            "cCommitter",
            "cAuthorDate",
            "cCommitDate",
            "cDefaultRemote",
            "cDefaultRemoteUrl",
            "cDefaultRemotePathFromUrl",
            "UserInput",
            "UserFiles",
            "WorkingDir"
        };

        private static string CreateOption(string option, bool quoted)
        {
            var result = "{" + option + "}";

            if (quoted)
            {
                result = "{" + result + "}";
            }

            return result;
        }

        public static (string argument, bool abort) Parse(string argument, IGitModule module, IWin32Window owner, RevisionGridControl revisionGrid)
        {
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
                if (string.IsNullOrEmpty(argument))
                {
                    continue;
                }

                string regularOption = CreateOption(option, false);
                string quotedOption = CreateOption(option, true);

                if (!argument.Contains(regularOption) && (!argument.Contains(quotedOption)))
                {
                    continue;
                }

                if (option.StartsWith("c") && currentRevision == null)
                {
                    currentRevision = GetCurrentRevision(module, revisionGrid, currentTags, currentLocalBranches, currentRemoteBranches, currentBranches, currentRevision);

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
                                AskToSpecify(currentLocalBranches, revisionGrid)));
                        }
                    }
                }
                else if (option.StartsWith("s") && selectedRevision == null && revisionGrid != null)
                {
                    allSelectedRevisions = revisionGrid.GetSelectedRevisions();
                    selectedRevision = CalculateSelectedRevision(revisionGrid, selectedRemoteBranches, selectedRemotes, selectedLocalBranches, selectedBranches, selectedTags);
                }

                argument = ParseScriptArguments(argument, option, owner, revisionGrid, module, allSelectedRevisions, selectedTags, selectedBranches, selectedLocalBranches, selectedRemoteBranches, selectedRemotes, selectedRevision, currentTags, currentBranches, currentLocalBranches, currentRemoteBranches, currentRevision, currentRemote);
                if (argument == null)
                {
                    return (argument: null, abort: true);
                }
            }

            return (argument, abort: false);
        }

        private static string AskToSpecify(IEnumerable<IGitRef> options, RevisionGridControl revisionGrid)
        {
            using (var f = new FormQuickGitRefSelector())
            {
                f.Location = revisionGrid?.GetQuickItemSelectorLocation() ?? new System.Drawing.Point();
                f.Init(FormQuickGitRefSelector.Action.Select, options.ToList());
                f.ShowDialog();
                return f.SelectedRef.Name;
            }
        }

        private static string AskToSpecify(IEnumerable<string> options, RevisionGridControl revisionGrid)
        {
            using (var f = new FormQuickStringSelector())
            {
                f.Location = revisionGrid?.GetQuickItemSelectorLocation() ?? new System.Drawing.Point();
                f.Init(options.ToList());
                f.ShowDialog();
                return f.SelectedString;
            }
        }

        private static GitRevision CalculateSelectedRevision(RevisionGridControl revisionGrid, List<IGitRef> selectedRemoteBranches,
            List<string> selectedRemotes, List<IGitRef> selectedLocalBranches,
            List<IGitRef> selectedBranches, List<IGitRef> selectedTags)
        {
            GitRevision selectedRevision = revisionGrid.LatestSelectedRevision;
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

        [CanBeNull]
        private static GitRevision GetCurrentRevision(
            IGitModule module, [CanBeNull] RevisionGridControl revisionGrid, List<IGitRef> currentTags, List<IGitRef> currentLocalBranches,
            List<IGitRef> currentRemoteBranches, List<IGitRef> currentBranches, [CanBeNull] GitRevision currentRevision)
        {
            if (currentRevision == null)
            {
                IEnumerable<IGitRef> refs;

                if (revisionGrid == null)
                {
                    var currentRevisionGuid = module.GetCurrentCheckout();
                    refs = module.GetRefs(true, true).Where(gitRef => gitRef.ObjectId == currentRevisionGuid).ToList();
                }
                else
                {
                    currentRevision = revisionGrid.GetCurrentRevision();
                    refs = currentRevision.Refs;
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

        private static string ParseScriptArguments(string argument, string option, IWin32Window owner, RevisionGridControl revisionGrid, IGitModule module, IReadOnlyList<GitRevision> allSelectedRevisions, in IList<IGitRef> selectedTags, in IList<IGitRef> selectedBranches, in IList<IGitRef> selectedLocalBranches, in IList<IGitRef> selectedRemoteBranches, in IList<string> selectedRemotes, GitRevision selectedRevision, in IList<IGitRef> currentTags, in IList<IGitRef> currentBranches, in IList<IGitRef> currentLocalBranches, in IList<IGitRef> currentRemoteBranches, GitRevision currentRevision, string currentRemote)
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
                    if (selectedTags.Count == 1)
                    {
                        newString = selectedTags[0].Name;
                    }
                    else if (selectedTags.Count != 0)
                    {
                        newString = AskToSpecify(selectedTags, revisionGrid);
                    }
                    else
                    {
                        newString = "";
                    }

                    break;

                case "sBranch":
                    if (selectedBranches.Count == 1)
                    {
                        newString = selectedBranches[0].Name;
                    }
                    else if (selectedBranches.Count != 0)
                    {
                        newString = AskToSpecify(selectedBranches, revisionGrid);
                    }
                    else
                    {
                        newString = "";
                    }

                    break;

                case "sLocalBranch":
                    if (selectedLocalBranches.Count == 1)
                    {
                        newString = selectedLocalBranches[0].Name;
                    }
                    else if (selectedLocalBranches.Count != 0)
                    {
                        newString = AskToSpecify(selectedLocalBranches, revisionGrid);
                    }
                    else
                    {
                        newString = "";
                    }

                    break;

                case "sRemoteBranch":
                    if (selectedRemoteBranches.Count == 1)
                    {
                        newString = selectedRemoteBranches[0].Name;
                    }
                    else if (selectedRemoteBranches.Count != 0)
                    {
                        newString = AskToSpecify(selectedRemoteBranches, revisionGrid);
                    }
                    else
                    {
                        newString = "";
                    }

                    break;

                case "sRemote":
                    if (selectedRemotes.Count == 0)
                    {
                        newString = "";
                    }
                    else
                    {
                        newString = selectedRemotes.Count == 1
                            ? selectedRemotes[0]
                            : AskToSpecify(selectedRemotes, revisionGrid);
                    }

                    break;

                case "sRemoteUrl":
                    if (selectedRemotes.Count == 0)
                    {
                        newString = "";
                    }
                    else
                    {
                        remote = selectedRemotes.Count == 1
                            ? selectedRemotes[0]
                            : AskToSpecify(selectedRemotes, revisionGrid);
                        newString = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
                    }

                    break;

                case "sRemotePathFromUrl":
                    if (selectedRemotes.Count == 0)
                    {
                        newString = "";
                    }
                    else
                    {
                        remote = selectedRemotes.Count == 1
                            ? selectedRemotes[0]
                            : AskToSpecify(selectedRemotes, revisionGrid);
                        newString = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
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
                    if (currentTags.Count == 1)
                    {
                        newString = currentTags[0].Name;
                    }
                    else if (currentTags.Count != 0)
                    {
                        newString = AskToSpecify(currentTags, revisionGrid);
                    }
                    else
                    {
                        newString = "";
                    }

                    break;

                case "cBranch":
                    if (currentBranches.Count == 1)
                    {
                        newString = currentBranches[0].Name;
                    }
                    else if (currentBranches.Count != 0)
                    {
                        newString = AskToSpecify(currentBranches, revisionGrid);
                    }
                    else
                    {
                        newString = "";
                    }

                    break;

                case "cLocalBranch":
                    if (currentLocalBranches.Count == 1)
                    {
                        newString = currentLocalBranches[0].Name;
                    }
                    else if (currentLocalBranches.Count != 0)
                    {
                        newString = AskToSpecify(currentLocalBranches, revisionGrid);
                    }
                    else
                    {
                        newString = "";
                    }

                    break;

                case "cRemoteBranch":
                    if (currentRemoteBranches.Count == 1)
                    {
                        newString = currentRemoteBranches[0].Name;
                    }
                    else if (currentRemoteBranches.Count != 0)
                    {
                        newString = AskToSpecify(currentRemoteBranches, revisionGrid);
                    }
                    else
                    {
                        newString = "";
                    }

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
                    newString = module.WorkingDir;
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

                argument = argument.Replace(CreateOption(option, true), newStringQuoted);
                argument = argument.Replace(CreateOption(option, false), newString);
            }

            return argument;
        }

        internal static TestAccessor GetTestAccessor() => new TestAccessor();

        public readonly struct TestAccessor
        {
            public string ParseScriptArguments(string argument, string option, IWin32Window owner, RevisionGridControl revisionGrid, IGitModule module, IReadOnlyList<GitRevision> allSelectedRevisions, List<IGitRef> selectedTags, List<IGitRef> selectedBranches, List<IGitRef> selectedLocalBranches, List<IGitRef> selectedRemoteBranches, List<string> selectedRemotes, GitRevision selectedRevision, List<IGitRef> currentTags, List<IGitRef> currentBranches, List<IGitRef> currentLocalBranches, List<IGitRef> currentRemoteBranches, GitRevision currentRevision, string currentRemote) =>
                ScriptOptionsParser.ParseScriptArguments(argument, option, owner, revisionGrid, module, allSelectedRevisions, selectedTags, selectedBranches, selectedLocalBranches, selectedRemoteBranches, selectedRemotes, selectedRevision, currentTags, currentBranches, currentLocalBranches, currentRemoteBranches, currentRevision, currentRemote);
        }
    }
}