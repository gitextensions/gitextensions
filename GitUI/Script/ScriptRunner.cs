using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.HelperDialogs;

namespace GitUI.Script
{
    /// <summary>Runs scripts.</summary>
    public static class ScriptRunner
    {
        /// <summary>Tries to run scripts identified by a <paramref name="command"/> 
        /// and returns true if any executed.</summary>
        public static bool ExecuteScriptCommand(GitModule aModule, int command)
        {
            var curScripts = ScriptManager.GetScripts();
            bool anyScriptExecuted = false;

            foreach (ScriptInfo s in curScripts)
            {
                if (s.HotkeyCommandIdentifier == command)
                {
                    RunScript(aModule, s.Name, null);
                    anyScriptExecuted = true;
                }
            }
            return anyScriptExecuted;
        }


        public static void RunScript(GitModule aModule, string script, RevisionGrid RevisionGrid)
        {
            if (string.IsNullOrEmpty(script))
                return;

            ScriptInfo scriptInfo = ScriptManager.GetScript(script);

            if (scriptInfo == null)
            {
                MessageBox.Show("Cannot find script: " + script, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(scriptInfo.Command))
                return;

            string argument = scriptInfo.Arguments;
            foreach (string option in Options)
            {
                if (string.IsNullOrEmpty(argument) || !argument.Contains(option))
                    continue;
                if (!option.StartsWith("{s"))
                    continue;
                if (RevisionGrid != null)
                    continue;
                MessageBox.Show(
                    string.Format("Option {0} is only supported when started from revision grid.", option),
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            RunScript(aModule, scriptInfo, RevisionGrid);
        }

        internal static void RunScript(GitModule aModule, ScriptInfo scriptInfo, RevisionGrid RevisionGrid)
        {
            string command = scriptInfo.Command;
            string argument = scriptInfo.Arguments;

            command = OverrideCommandWhenNecessary(command);

            GitRevision selectedRevision = null;
            GitRevision currentRevision = null;

            var selectedLocalBranches = new List<GitRef>();
            var selectedRemoteBranches = new List<GitRef>();
            var selectedRemotes = new List<string>();
            var selectedBranches = new List<GitRef>();
            var selectedTags = new List<GitRef>();
            var currentLocalBranches = new List<GitRef>();
            var currentRemoteBranches = new List<GitRef>();
            var currentBranches = new List<GitRef>();
            var currentTags = new List<GitRef>();

            foreach (string option in Options)
            {
                if (string.IsNullOrEmpty(argument) || !argument.Contains(option))
                    continue;
                if (!option.StartsWith("{s") || selectedRevision != null)
                {
                    currentRevision = GetCurrentRevision(aModule, RevisionGrid, currentTags, currentLocalBranches, currentRemoteBranches, currentBranches, currentRevision, option);
                }
                else
                {
                    selectedRevision = CalculateSelectedRevision(RevisionGrid, selectedRemoteBranches, selectedRemotes, selectedLocalBranches, selectedBranches, selectedTags);
                }

                switch (option)
                {
                    case "{sTag}":
                        if (selectedTags.Count == 1)
                            argument = argument.Replace(option, selectedTags[0].Name);
                        else if (selectedTags.Count != 0)
                            argument = argument.Replace(option, askToSpecify(selectedTags, "Selected Revision Tag"));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{sBranch}":
                        if (selectedBranches.Count == 1)
                            argument = argument.Replace(option, selectedBranches[0].Name);
                        else if (selectedBranches.Count != 0)
                            argument = argument.Replace(option,
                                                        askToSpecify(selectedBranches, "Selected Revision Branch"));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{sLocalBranch}":
                        if (selectedLocalBranches.Count == 1)
                            argument = argument.Replace(option, selectedLocalBranches[0].Name);
                        else if (selectedLocalBranches.Count != 0)
                            argument = argument.Replace(option,
                                                        askToSpecify(selectedLocalBranches,
                                                                     "Selected Revision Local Branch"));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{sRemoteBranch}":
                        if (selectedRemoteBranches.Count == 1)
                            argument = argument.Replace(option, selectedRemoteBranches[0].Name);
                        else if (selectedRemoteBranches.Count != 0)
                            argument = argument.Replace(option,
                                                        askToSpecify(selectedRemoteBranches,
                                                                     "Selected Revision Remote Branch"));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{sRemote}":
                        if (selectedRemotes.Count == 1)
                            argument = argument.Replace(option, selectedRemotes[0]);
                        else if (selectedRemotes.Count != 0)
                        {
                            argument = argument.Replace(option,
                                                        askToSpecify(selectedRemotes, "Selected Revision Remote"));
                        }
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{sHash}":
                        argument = argument.Replace(option, selectedRevision.Guid);
                        break;
                    case "{sMessage}":
                        argument = argument.Replace(option, selectedRevision.Message);
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
                            argument = argument.Replace(option, currentTags[0].Name);
                        else if (currentTags.Count != 0)
                            argument = argument.Replace(option, askToSpecify(currentTags, "Current Revision Tag"));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{cBranch}":
                        if (currentBranches.Count == 1)
                            argument = argument.Replace(option, currentBranches[0].Name);
                        else if (currentBranches.Count != 0)
                            argument = argument.Replace(option,
                                                        askToSpecify(currentBranches, "Current Revision Branch"));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{cLocalBranch}":
                        if (currentLocalBranches.Count == 1)
                            argument = argument.Replace(option, currentLocalBranches[0].Name);
                        else if (currentLocalBranches.Count != 0)
                            argument = argument.Replace(option,
                                                        askToSpecify(currentLocalBranches,
                                                                     "Current Revision Local Branch"));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{cRemoteBranch}":
                        if (currentRemoteBranches.Count == 1)
                            argument = argument.Replace(option, currentRemoteBranches[0].Name);
                        else if (currentRemoteBranches.Count != 0)
                            argument = argument.Replace(option,
                                                        askToSpecify(currentRemoteBranches,
                                                                     "Current Revision Remote Branch"));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{cHash}":
                        argument = argument.Replace(option, currentRevision.Guid);
                        break;
                    case "{cMessage}":
                        argument = argument.Replace(option, currentRevision.Message);
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
                        if (currentBranches.Count == 1)
                            argument = argument.Replace(option,
                                                        aModule.GetSetting(string.Format("branch.{0}.remote",
                                                                                                 currentBranches[0].Name)));
                        else if (currentBranches.Count != 0)
                            argument = argument.Replace(option,
                                                        aModule.GetSetting(string.Format("branch.{0}.remote",
                                                                                                 askToSpecify(
                                                                                                     currentBranches,
                                                                                                     "Current Revision Branch"))));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{UserInput}":
                        using (SimplePrompt Prompt = new SimplePrompt())
                        {
                            Prompt.ShowDialog();
                            argument = argument.Replace(option, Prompt.UserInput);
                        }
                        break;
                }
            }

            FormProcess.ShowDialog(null, command, argument, aModule.WorkingDir, null, true);
        }

        private static GitRevision CalculateSelectedRevision(RevisionGrid RevisionGrid, List<GitRef> selectedRemoteBranches,
                                                             List<string> selectedRemotes, List<GitRef> selectedLocalBranches,
                                                             List<GitRef> selectedBranches, List<GitRef> selectedTags)
        {
            GitRevision selectedRevision = RevisionGrid.GetRevision(RevisionGrid.LastRow);
            foreach (GitRef head in selectedRevision.Refs)
            {
                if (head.IsTag)
                    selectedTags.Add(head);

                else if (head.IsHead || head.IsRemote)
                {
                    selectedBranches.Add(head);
                    if (head.IsRemote)
                    {
                        selectedRemoteBranches.Add(head);
                        if (!selectedRemotes.Contains(head.Remote))
                            selectedRemotes.Add(head.Remote);
                    }
                    else
                        selectedLocalBranches.Add(head);
                }
            }
            return selectedRevision;
        }

        private static GitRevision GetCurrentRevision(GitModule aModule, RevisionGrid RevisionGrid, List<GitRef> currentTags, List<GitRef> currentLocalBranches,
                                                      List<GitRef> currentRemoteBranches, List<GitRef> currentBranches,
                                                      GitRevision currentRevision, string option)
        {
            if (option.StartsWith("{c") && currentRevision == null)
            {
                IList<GitRef> refs;

                if (RevisionGrid == null)
                {
                    string currentRevisionGuid = aModule.GetCurrentCheckout();
                    refs = aModule.GetRefs(true, true).Where(gitRef => gitRef.Guid == currentRevisionGuid).ToList();
                    }
                else
                {
                    currentRevision = RevisionGrid.GetCurrentRevision();
                    refs = currentRevision.Refs;
                }

                foreach (GitRef gitRef in refs)
                {
                    if (gitRef.IsTag)
                        currentTags.Add(gitRef);
                    else if (gitRef.IsHead || gitRef.IsRemote)
                    {
                        currentBranches.Add(gitRef);
                        if (gitRef.IsRemote)
                            currentRemoteBranches.Add(gitRef);
                        else
                            currentLocalBranches.Add(gitRef);
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
                        "{sTag}",
                        "{sBranch}",
                        "{sLocalBranch}",
                        "{sRemoteBranch}",
                        "{sRemote}",
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
                        "{UserInput}"
                    };
                return options;
            }
        }

        private static string OverrideCommandWhenNecessary(string originalCommand)
        {
            //Make sure we are able to run git, even if git is not in the path
            if (originalCommand.Equals("git", System.StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("{git}", System.StringComparison.CurrentCultureIgnoreCase))
                return Settings.GitCommand;

            if (originalCommand.Equals("gitextensions", System.StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("{gitextensions}", System.StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("gitex", System.StringComparison.CurrentCultureIgnoreCase) ||
                originalCommand.Equals("{gitex}", System.StringComparison.CurrentCultureIgnoreCase))
                return Settings.GetGitExtensionsFullPath();
            return originalCommand;
        }

        private static string askToSpecify(IEnumerable<GitRef> options, string title)
        {
            using (var f = new FormRunScriptSpecify(options, title))
            {
                f.ShowDialog();
                return f.ret;
            }
        }

        private static string askToSpecify(IEnumerable<string> options, string title)
        {
            using (var f = new FormRunScriptSpecify(options, title))
            {
                f.ShowDialog();
                return f.ret;
            }
        }
    }
}