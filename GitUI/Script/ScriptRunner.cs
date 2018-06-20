using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;

namespace GitUI.Script
{
    /// <summary>Runs scripts.</summary>
    public static class ScriptRunner
    {
        /// <summary>Tries to run scripts identified by a <paramref name="command"/>
        /// and returns true if any executed.</summary>
        public static bool ExecuteScriptCommand(IWin32Window owner, GitModule module, int command, RevisionGrid revisionGrid = null)
        {
            var curScripts = ScriptManager.GetScripts();
            bool anyScriptExecuted = false;

            foreach (ScriptInfo s in curScripts)
            {
                if (s.HotkeyCommandIdentifier == command)
                {
                    RunScript(owner, module, s.Name, revisionGrid);
                    anyScriptExecuted = true;
                }
            }

            return anyScriptExecuted;
        }

        public static bool RunScript(IWin32Window owner, GitModule module, string script, RevisionGrid revisionGrid)
        {
            if (string.IsNullOrEmpty(script))
            {
                return false;
            }

            ScriptInfo scriptInfo = ScriptManager.GetScript(script);

            if (scriptInfo == null)
            {
                MessageBox.Show(owner, "Cannot find script: " + script, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrEmpty(scriptInfo.Command))
            {
                return false;
            }

            string argument = scriptInfo.Arguments;
            foreach (string option in Options)
            {
                if (string.IsNullOrEmpty(argument) || !argument.Contains(option))
                {
                    continue;
                }

                if (!option.StartsWith("{s"))
                {
                    continue;
                }

                if (revisionGrid != null)
                {
                    continue;
                }

                MessageBox.Show(owner,
                    string.Format("Option {0} is only supported when started from revision grid.", option),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return RunScript(owner, module, scriptInfo, revisionGrid);
        }

        private static string GetRemotePath(string url)
        {
            Uri uri;
            string path = "";
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                path = uri.LocalPath;
            }
            else if (Uri.TryCreate("ssh://" + url.Replace(":", "/"), UriKind.Absolute, out uri))
            {
                path = uri.LocalPath;
            }

            int pos = path.LastIndexOf(".");
            if (pos >= 0)
            {
                path = path.Substring(0, pos);
            }

            return path;
        }

        private static bool RunScript(IWin32Window owner, GitModule module, ScriptInfo scriptInfo, RevisionGrid revisionGrid)
        {
            if (scriptInfo.AskConfirmation && MessageBox.Show(owner, string.Format("Do you want to execute '{0}'?", scriptInfo.Name), "Script", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return false;
            }

            string originalCommand = scriptInfo.Command;
            string argument = scriptInfo.Arguments;

            string command = OverrideCommandWhenNecessary(originalCommand);
            IReadOnlyList<GitRevision> allSelectedRevisions = Array.Empty<GitRevision>();

            GitRevision selectedRevision = null;
            GitRevision currentRevision = null;

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
                                AskToSpecify(currentLocalBranches, "Current Revision Branch")));
                        }
                    }
                }
                else if (option.StartsWith("{s") && selectedRevision == null && revisionGrid != null)
                {
                    allSelectedRevisions = revisionGrid.GetSelectedRevisions();
                    allSelectedRevisions.Reverse(); // Put first clicked revisions first
                    selectedRevision = CalculateSelectedRevision(revisionGrid, selectedRemoteBranches, selectedRemotes, selectedLocalBranches, selectedBranches, selectedTags);
                }

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
                            argument = argument.Replace(option, AskToSpecify(selectedTags, "Selected Revision Tag"));
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
                                                        AskToSpecify(selectedBranches, "Selected Revision Branch"));
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
                            argument = argument.Replace(option,
                                                        AskToSpecify(selectedLocalBranches,
                                                                     "Selected Revision Local Branch"));
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
                            argument = argument.Replace(option,
                                                        AskToSpecify(selectedRemoteBranches,
                                                                     "Selected Revision Remote Branch"));
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
                            : AskToSpecify(selectedRemotes, "Selected Revision Remote");

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
                            : AskToSpecify(selectedRemotes, "Selected Revision Remote");

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
                            : AskToSpecify(selectedRemotes, "Selected Revision Remote");

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
                            argument = argument.Replace(option, AskToSpecify(currentTags, "Current Revision Tag"));
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
                            argument = argument.Replace(option,
                                                        AskToSpecify(currentBranches, "Current Revision Branch"));
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
                            argument = argument.Replace(option,
                                                        AskToSpecify(currentLocalBranches,
                                                                     "Current Revision Local Branch"));
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
                            argument = argument.Replace(option,
                                                        AskToSpecify(currentRemoteBranches,
                                                                     "Current Revision Remote Branch"));
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
                        using (SimplePrompt prompt = new SimplePrompt())
                        {
                            prompt.ShowDialog();
                            argument = argument.Replace(option, prompt.UserInput);
                        }

                        break;
                    case "{WorkingDir}":
                        argument = argument.Replace(option, module.WorkingDir);
                        break;
                }
            }

            command = ExpandCommandVariables(command, module);

            if (scriptInfo.IsPowerShell)
            {
                PowerShellHelper.RunPowerShell(command, argument, module.WorkingDir, scriptInfo.RunInBackground);
                return false;
            }

            if (command.StartsWith(PluginPrefix))
            {
                command = command.Replace(PluginPrefix, "");
                foreach (var plugin in PluginRegistry.Plugins)
                {
                    if (plugin.Description.ToLower().Equals(command, StringComparison.CurrentCultureIgnoreCase))
                    {
                        var eventArgs = new GitUIEventArgs(owner, revisionGrid.UICommands);
                        return plugin.Execute(eventArgs);
                    }
                }

                return false;
            }

            if (!scriptInfo.RunInBackground)
            {
                FormProcess.ShowStandardProcessDialog(owner, command, argument, module.WorkingDir, null, true);
            }
            else
            {
                if (originalCommand.Equals("{openurl}", StringComparison.CurrentCultureIgnoreCase))
                {
                    Process.Start(argument);
                }
                else
                {
                    module.RunExternalCmdDetached(command, argument);
                }
            }

            return !scriptInfo.RunInBackground;
        }

        private static string ExpandCommandVariables(string originalCommand, GitModule module)
        {
            return originalCommand.Replace("{WorkingDir}", module.WorkingDir);
        }

        private static GitRevision CalculateSelectedRevision(RevisionGrid revisionGrid, List<IGitRef> selectedRemoteBranches,
                                                             List<string> selectedRemotes, List<IGitRef> selectedLocalBranches,
                                                             List<IGitRef> selectedBranches, List<IGitRef> selectedTags)
        {
            GitRevision selectedRevision = revisionGrid.LatestSelectedRevision;
            foreach (GitRef head in selectedRevision.Refs)
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

        private static GitRevision GetCurrentRevision(GitModule module, RevisionGrid revisionGrid, List<IGitRef> currentTags, List<IGitRef> currentLocalBranches,
                                                      List<IGitRef> currentRemoteBranches, List<IGitRef> currentBranches,
                                                      GitRevision currentRevision)
        {
            if (currentRevision == null)
            {
                IEnumerable<IGitRef> refs;

                if (revisionGrid == null)
                {
                    string currentRevisionGuid = module.GetCurrentCheckout();
                    refs = module.GetRefs(true, true).Where(gitRef => gitRef.Guid == currentRevisionGuid).ToList();
                }
                else
                {
                    currentRevision = revisionGrid.GetCurrentRevision();
                    refs = currentRevision.Refs;
                }

                foreach (IGitRef gitRef in refs)
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

        private static string[] Options
        {
            get
            {
                string[] options =
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
                        "{WorkingDir}"
                    };
                return options;
            }
        }

        private const string PluginPrefix = "plugin:";

        private static string OverrideCommandWhenNecessary(string originalCommand)
        {
            // Make sure we are able to run git, even if git is not in the path
            if (originalCommand.Equals("git", StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("{git}", StringComparison.CurrentCultureIgnoreCase))
            {
                return AppSettings.GitCommand;
            }

            if (originalCommand.Equals("gitextensions", StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("{gitextensions}", StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("gitex", StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("{gitex}", StringComparison.CurrentCultureIgnoreCase))
            {
                return AppSettings.GetGitExtensionsFullPath();
            }

            if (originalCommand.Equals("{openurl}", StringComparison.CurrentCultureIgnoreCase))
            {
                return "explorer";
            }

            // Prefix should be {plugin:pluginname},{plugin=pluginname}
            var match = System.Text.RegularExpressions.Regex.Match(originalCommand, @"\{plugin.(.+)\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && match.Groups.Count > 1)
            {
                originalCommand = string.Format("{0}{1}", PluginPrefix, match.Groups[1].Value.ToLower());
            }

            return originalCommand;
        }

        private static string AskToSpecify(IEnumerable<IGitRef> options, string title)
        {
            using (var f = new FormRunScriptSpecify(options, title))
            {
                f.ShowDialog();
                return f.Ret;
            }
        }

        private static string AskToSpecify(IEnumerable<string> options, string title)
        {
            using (var f = new FormRunScriptSpecify(options, title))
            {
                f.ShowDialog();
                return f.Ret;
            }
        }
    }
}