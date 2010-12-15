using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;

namespace GitUI
{
    class RunScript
    {
        public RunScript(string script, RevisionGrid RevisionGrid)
        {
            string[] scriptInfo = Settings.GetScript(script);
            string command;
            string argument;

            if (scriptInfo == null)
            {
                command = "cmd";
                argument = "echo \'Cannot find script: " + script + "\'";
            }
            else
            {
                command = scriptInfo[1];
                argument = scriptInfo[2];
            }

            string[] options = 
           {
               "{sTag}",
               "{sBranch}",
               "{sLocalBranch}",
               "{sRemoteBranch}",
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
               "{cCommitDate}"
           };

            GitRevision selectedRevision = null;
            GitRevision currentRevision = null;

            List<GitHead> selectedLocalBranches = new List<GitHead>();
            List<GitHead> selectedRemoteBranches = new List<GitHead>();
            List<GitHead> selectedBranches = new List<GitHead>();
            List<GitHead> selectedTags = new List<GitHead>();
            List<GitHead> currentLocalBranches = new List<GitHead>();
            List<GitHead> currentRemoteBranches = new List<GitHead>();
            List<GitHead> currentBranches = new List<GitHead>();
            List<GitHead> currentTags = new List<GitHead>();

            foreach (string option in options)
            {
                if (argument.Contains(option))
                {
                    if (option.StartsWith("{s") && selectedRevision == null)
                    {
                        selectedRevision = RevisionGrid.GetRevision(RevisionGrid.LastRow);
                        foreach (var head in selectedRevision.Heads)
                        {
                            if (head.IsTag)
                                selectedTags.Add(head);

                            else if (head.IsHead || head.IsRemote)
                            {
                                selectedBranches.Add(head);
                                if (head.IsRemote)
                                    selectedRemoteBranches.Add(head);

                                else
                                    selectedLocalBranches.Add(head);

                            }
                        }
                    }
                    else if (option.StartsWith("{c") && currentRevision == null)
                    {
                        currentRevision = RevisionGrid.GetCurrentRevision();

                        foreach (var head in currentRevision.Heads)
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
                                argument = argument.Replace(option, askToSpecify(selectedBranches, "Selected Revision Branch"));
                            else
                                argument = argument.Replace(option, "");
                            break;
                        case "{sLocalBranch}":
                            if (selectedLocalBranches.Count == 1)
                                argument = argument.Replace(option, selectedLocalBranches[0].Name);
                            else if (selectedLocalBranches.Count != 0)
                                argument = argument.Replace(option, askToSpecify(selectedLocalBranches, "Selected Revision Local Branch"));
                            else
                                argument = argument.Replace(option, "");
                            break;
                        case "{sRemoteBranch}":
                            if (selectedRemoteBranches.Count == 1)
                                argument = argument.Replace(option, selectedRemoteBranches[0].Name);
                            else if (selectedRemoteBranches.Count != 0)
                                argument = argument.Replace(option, askToSpecify(selectedRemoteBranches, "Selected Revision Remote Branch"));
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
                                argument = argument.Replace(option, askToSpecify(currentBranches, "Current Revision Branch"));
                            else
                                argument = argument.Replace(option, "");
                            break;
                        case "{cLocalBranch}":
                            if (currentLocalBranches.Count == 1)
                                argument = argument.Replace(option, currentLocalBranches[0].Name);
                            else if (currentLocalBranches.Count != 0)
                                argument = argument.Replace(option, askToSpecify(currentLocalBranches, "Current Revision Local Branch"));
                            else
                                argument = argument.Replace(option, "");
                            break;
                        case "{cRemoteBranch}":
                            if (currentRemoteBranches.Count == 1)
                                argument = argument.Replace(option, currentRemoteBranches[0].Name);
                            else if (currentRemoteBranches.Count != 0)
                                argument = argument.Replace(option, askToSpecify(currentRemoteBranches, "Current Revision Remote Branch"));
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
                        default:
                            break;
                    }
                }
            }

            new FormProcess(command, argument).ShowDialog();
        }

        private string askToSpecify(List<GitHead> options, string title)
        {
            FormRunScriptSpecify f = new FormRunScriptSpecify(options, title);
            f.ShowDialog();
            return f.ret;
        }
    }
}
