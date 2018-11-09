using System;
using System.Collections.Generic;
using System.Linq;
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
            "{sHashes}",
            "{sTag}",
            "{sBranch}",
            "{sLocalBranch}",
            "{sRemoteBranch}",
            "{sRemote}",
            "{sRemoteUrl}",
            "{sRemotePathFromUrl}",
            "{sHash}",
            "{sMessage}",
            "{sAuthor}",
            "{sCommitter}",
            "{sAuthorDate}",
            "{sCommitDate}",
            "{cTag}",
            "{cBranch}",
            "{cLocalBranch}",
            "{cRemoteBranch}",
            "{cHash}",
            "{cMessage}",
            "{cAuthor}",
            "{cCommitter}",
            "{cAuthorDate}",
            "{cCommitDate}",
            "{cDefaultRemote}",
            "{cDefaultRemoteUrl}",
            "{cDefaultRemotePathFromUrl}",
            "{UserInput}",
            "{UserFiles}",
            "{WorkingDir}"
        };

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
                if (string.IsNullOrEmpty(argument) || !argument.Contains(option))
                {
                    continue;
                }

                if (option.StartsWith("{c") && currentRevision == null)
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
                else if (option.StartsWith("{s") && selectedRevision == null && revisionGrid != null)
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
            string remote;
            string url;
            switch (option)
            {
                case "{sHashes}":
                    argument = argument.Replace(option,
                        string.Join(" ", allSelectedRevisions.Select(revision => revision.Guid).ToArray()));
                    break;

                case "{sTag}":
                    if (selectedTags.Count == 1)
                    {
                        argument = argument.Replace(option, selectedTags[0].Name);
                    }
                    else if (selectedTags.Count != 0)
                    {
                        argument = argument.Replace(option, AskToSpecify(selectedTags, revisionGrid));
                    }
                    else
                    {
                        argument = argument.Replace(option, "");
                    }

                    break;
                case "{sBranch}":
                    if (selectedBranches.Count == 1)
                    {
                        argument = argument.Replace(option, selectedBranches[0].Name);
                    }
                    else if (selectedBranches.Count != 0)
                    {
                        argument = argument.Replace(option,
                            AskToSpecify(selectedBranches, revisionGrid));
                    }
                    else
                    {
                        argument = argument.Replace(option, "");
                    }

                    break;
                case "{sLocalBranch}":
                    if (selectedLocalBranches.Count == 1)
                    {
                        argument = argument.Replace(option, selectedLocalBranches[0].Name);
                    }
                    else if (selectedLocalBranches.Count != 0)
                    {
                        argument = argument.Replace(option, AskToSpecify(selectedLocalBranches, revisionGrid));
                    }
                    else
                    {
                        argument = argument.Replace(option, "");
                    }

                    break;
                case "{sRemoteBranch}":
                    if (selectedRemoteBranches.Count == 1)
                    {
                        argument = argument.Replace(option, selectedRemoteBranches[0].Name);
                    }
                    else if (selectedRemoteBranches.Count != 0)
                    {
                        argument = argument.Replace(option, AskToSpecify(selectedRemoteBranches, revisionGrid));
                    }
                    else
                    {
                        argument = argument.Replace(option, "");
                    }

                    break;
                case "{sRemote}":
                    if (selectedRemotes.Count == 0)
                    {
                        argument = argument.Replace(option, "");
                        break;
                    }

                    remote = selectedRemotes.Count == 1
                        ? selectedRemotes[0]
                        : AskToSpecify(selectedRemotes, revisionGrid);

                    argument = argument.Replace(option, remote);
                    break;
                case "{sRemoteUrl}":
                    if (selectedRemotes.Count == 0)
                    {
                        argument = argument.Replace(option, "");
                        break;
                    }

                    remote = selectedRemotes.Count == 1
                        ? selectedRemotes[0]
                        : AskToSpecify(selectedRemotes, revisionGrid);

                    url = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
                    argument = argument.Replace(option, url);
                    break;
                case "{sRemotePathFromUrl}":
                    if (selectedRemotes.Count == 0)
                    {
                        argument = argument.Replace(option, "");
                        break;
                    }

                    remote = selectedRemotes.Count == 1
                        ? selectedRemotes[0]
                        : AskToSpecify(selectedRemotes, revisionGrid);

                    url = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, remote));
                    argument = argument.Replace(option, GetRemotePath(url));
                    break;
                case "{sHash}":
                    argument = argument.Replace(option, selectedRevision.Guid);
                    break;
                case "{sMessage}":
                    argument = argument.Replace(option, selectedRevision.Subject);
                    break;
                case "{sAuthor}":
                    argument = argument.Replace(option, selectedRevision.Author);
                    break;
                case "{sCommitter}":
                    argument = argument.Replace(option, selectedRevision.Committer);
                    break;
                case "{sAuthorDate}":
                    argument = argument.Replace(option, selectedRevision.AuthorDate.ToString());
                    break;
                case "{sCommitDate}":
                    argument = argument.Replace(option, selectedRevision.CommitDate.ToString());
                    break;
                case "{cTag}":
                    if (currentTags.Count == 1)
                    {
                        argument = argument.Replace(option, currentTags[0].Name);
                    }
                    else if (currentTags.Count != 0)
                    {
                        argument = argument.Replace(option, AskToSpecify(currentTags, revisionGrid));
                    }
                    else
                    {
                        argument = argument.Replace(option, "");
                    }

                    break;
                case "{cBranch}":
                    if (currentBranches.Count == 1)
                    {
                        argument = argument.Replace(option, currentBranches[0].Name);
                    }
                    else if (currentBranches.Count != 0)
                    {
                        argument = argument.Replace(option, AskToSpecify(currentBranches, revisionGrid));
                    }
                    else
                    {
                        argument = argument.Replace(option, "");
                    }

                    break;
                case "{cLocalBranch}":
                    if (currentLocalBranches.Count == 1)
                    {
                        argument = argument.Replace(option, currentLocalBranches[0].Name);
                    }
                    else if (currentLocalBranches.Count != 0)
                    {
                        argument = argument.Replace(option, AskToSpecify(currentLocalBranches, revisionGrid));
                    }
                    else
                    {
                        argument = argument.Replace(option, "");
                    }

                    break;
                case "{cRemoteBranch}":
                    if (currentRemoteBranches.Count == 1)
                    {
                        argument = argument.Replace(option, currentRemoteBranches[0].Name);
                    }
                    else if (currentRemoteBranches.Count != 0)
                    {
                        argument = argument.Replace(option, AskToSpecify(currentRemoteBranches, revisionGrid));
                    }
                    else
                    {
                        argument = argument.Replace(option, "");
                    }

                    break;
                case "{cHash}":
                    argument = argument.Replace(option, currentRevision.Guid);
                    break;
                case "{cMessage}":
                    argument = argument.Replace(option, currentRevision.Subject);
                    break;
                case "{cAuthor}":
                    argument = argument.Replace(option, currentRevision.Author);
                    break;
                case "{cCommitter}":
                    argument = argument.Replace(option, currentRevision.Committer);
                    break;
                case "{cAuthorDate}":
                    argument = argument.Replace(option, currentRevision.AuthorDate.ToString());
                    break;
                case "{cCommitDate}":
                    argument = argument.Replace(option, currentRevision.CommitDate.ToString());
                    break;
                case "{cDefaultRemote}":
                    if (string.IsNullOrEmpty(currentRemote))
                    {
                        argument = argument.Replace(option, "");
                        break;
                    }

                    argument = argument.Replace(option, currentRemote);
                    break;
                case "{cDefaultRemoteUrl}":
                    if (string.IsNullOrEmpty(currentRemote))
                    {
                        argument = argument.Replace(option, "");
                        break;
                    }

                    url = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote));
                    argument = argument.Replace(option, url);
                    break;
                case "{cDefaultRemotePathFromUrl}":
                    if (string.IsNullOrEmpty(currentRemote))
                    {
                        argument = argument.Replace(option, "");
                        break;
                    }

                    url = module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentRemote));
                    argument = argument.Replace(option, GetRemotePath(url));
                    break;
                case "{UserInput}":
                    using (var prompt = new SimplePrompt())
                    {
                        prompt.ShowDialog();
                        argument = argument.Replace(option, prompt.UserInput);
                    }

                    break;
                case "{UserFiles}":
                    using (FormFilePrompt prompt = new FormFilePrompt())
                    {
                        if (prompt.ShowDialog(owner) != DialogResult.OK)
                        {
                            // abort parsing as the user chose to abort
                            return null;
                        }

                        argument = argument.Replace(option, prompt.FileInput);
                        break;
                    }

                case "{WorkingDir}":
                    argument = argument.Replace(option, module.WorkingDir);
                    break;
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