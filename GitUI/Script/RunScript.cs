using System.Collections.Generic;
using GitCommands;
using System.Windows.Forms;

namespace GitUI.Script
{
    public static class ScriptRunner
    {
        public static void RunScript(string script, RevisionGrid RevisionGrid)
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

            RunScript(RevisionGrid, scriptInfo);
        }

        internal static void RunScript(RevisionGrid RevisionGrid, ScriptInfo scriptInfo)
        {
            string command = scriptInfo.Command;
            string argument = scriptInfo.Arguments;

            command = OverrideCommandWhenNecessary(command);

            GitRevision selectedRevision = null;
            GitRevision currentRevision = null;

            var selectedLocalBranches = new List<GitHead>();
            var selectedRemoteBranches = new List<GitHead>();
            var selectedRemotes = new List<string>();
            var selectedBranches = new List<GitHead>();
            var selectedTags = new List<GitHead>();
            var currentLocalBranches = new List<GitHead>();
            var currentRemoteBranches = new List<GitHead>();
            var currentBranches = new List<GitHead>();
            var currentTags = new List<GitHead>();

            foreach (string option in Options)
            {
                if (string.IsNullOrEmpty(argument) || !argument.Contains(option))
                    continue;
                if (!option.StartsWith("{s") || selectedRevision != null)
                {
                    if (option.StartsWith("{c") && currentRevision == null)
                    {
                        IList<GitHead> heads;

                        if (RevisionGrid == null)
                        {
                            heads = new List<GitHead>();
                            string currentRevisionGuid = Settings.Module.GetCurrentCheckout();
                            foreach (GitHead head in Settings.Module.GetHeads(true, true))
                            {
                                if (head.Guid == currentRevisionGuid)
                                    heads.Add(head);
                            }
                        }
                        else
                        {
                            currentRevision = RevisionGrid.GetCurrentRevision();
                            heads = currentRevision.Heads;
                        }

                        foreach (GitHead head in heads)
                        {
                            if (head.IsTag)
                                currentTags.Add(head);
                            else if (head.IsHead || head.IsRemote)
                            {
                                currentBranches.Add(head);
                                if (head.IsRemote)
                                    currentRemoteBranches.Add(head);
                                else
                                    currentLocalBranches.Add(head);
                            }
                        }
                    }
                }
                else
                {
                    if (RevisionGrid == null)
                    {
                        MessageBox.Show(
                            string.Format("Option {0} is only supported when started from revision grid.", option),
                            "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    selectedRevision = RevisionGrid.GetRevision(RevisionGrid.LastRow);
                    foreach (GitHead head in selectedRevision.Heads)
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
                                                        Settings.Module.GetSetting(string.Format("branch.{0}.remote",
                                                                                                 currentBranches[0].Name)));
                        else if (currentBranches.Count != 0)
                            argument = argument.Replace(option,
                                                        Settings.Module.GetSetting(string.Format("branch.{0}.remote",
                                                                                                 askToSpecify(
                                                                                                     currentBranches,
                                                                                                     "Current Revision Branch"))));
                        else
                            argument = argument.Replace(option, "");
                        break;
                    case "{UserInput}":
                        SimplePrompt Prompt = new SimplePrompt();
                        Prompt.ShowDialog();
                        argument = argument.Replace(option, Prompt.UserInput);
                        break;
                    default:
                        break;
                }
            }

            new FormProcess(command, argument).ShowDialog();
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

        private static string OverrideCommandWhenNecessary(string command)
        {
            //Make sure we are able to run git, even if git is not in the path
            if (command.Equals("git", System.StringComparison.CurrentCultureIgnoreCase) ||
                command.Equals("{git}", System.StringComparison.CurrentCultureIgnoreCase))
                return Settings.GitCommand;

            if (command.Equals("gitextensions", System.StringComparison.CurrentCultureIgnoreCase) ||
                command.Equals("{gitextensions}", System.StringComparison.CurrentCultureIgnoreCase) ||
                command.Equals("gitex", System.StringComparison.CurrentCultureIgnoreCase) ||
                command.Equals("{gitex}", System.StringComparison.CurrentCultureIgnoreCase))
                return Settings.GetGitExtensionsFullPath();
            return command;
        }

        private static string askToSpecify(IEnumerable<GitHead> options, string title)
        {
            var f = new FormRunScriptSpecify(options, title);
            f.ShowDialog();
            return f.ret;
        }

        private static string askToSpecify(IEnumerable<string> options, string title)
        {
            var f = new FormRunScriptSpecify(options, title);
            f.ShowDialog();
            return f.ret;
        }
    }
}