﻿using GitCommands;
using GitCommands.Git;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormRevertCommit : GitModuleForm
    {
        private readonly TranslationString _noneParentSelectedText = new("None parent is selected!");

        private bool _isMerge;

        public FormRevertCommit(GitUICommands commands, GitRevision revision)
            : base(commands)
        {
            Revision = revision;

            InitializeComponent();
            InitializeComplete();
        }

        public GitRevision Revision { get; }

        private void FormRevertCommit_Load(object sender, EventArgs e)
        {
            commitSummaryUserControl1.Revision = Revision;

            ParentsList.Items.Clear(); // TODO: search this line and the ones below to find code duplication

            _isMerge = Module.IsMerge(Revision.ObjectId);
            parentListPanel.Visible = _isMerge;
            if (_isMerge)
            {
                IReadOnlyList<GitRevision> parents = Module.GetParentRevisions(Revision.ObjectId);

                for (int i = 0; i < parents.Count; i++)
                {
                    ParentsList.Items.Add(new ListViewItem((i + 1).ToString())
                    {
                        SubItems =
                        {
                            parents[i].Subject,
                            parents[i].Author,
                            parents[i].CommitDate.ToShortDateString()
                        }
                    });
                }

                ParentsList.TopItem.Selected = true;
                Size size = MinimumSize;
                size.Height += 100;
                MinimumSize = size;
            }
        }

        private void Revert_Click(object sender, EventArgs e)
        {
            int parentIndex = 0;
            if (_isMerge)
            {
                if (ParentsList.SelectedItems.Count != 1)
                {
                    MessageBox.Show(this, _noneParentSelectedText.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    parentIndex = ParentsList.SelectedItems[0].Index + 1;
                }
            }

            CommitMessageManager commitMessageManager = new(this, Module.WorkingDirGitDir, Module.CommitEncoding);

            string existingCommitMessage = ThreadHelper.JoinableTaskFactory.Run(() => commitMessageManager.GetMergeOrCommitMessageAsync());

            ArgumentString command = Commands.Revert(Revision.ObjectId, AutoCommit.Checked, parentIndex);

            // Don't verify whether the command is successful.
            // If it fails, likely there is a conflict that needs to be resolved.
            FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);

            if (!string.IsNullOrWhiteSpace(existingCommitMessage))
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    try
                    {
                        await TaskScheduler.Default;

                        string message = await commitMessageManager.GetMergeOrCommitMessageAsync();
                        string newCommitMessageContent = $"{existingCommitMessage}\n\n{message}";
                        await commitMessageManager.WriteCommitMessageToFileAsync(newCommitMessageContent, CommitMessageType.Merge,
                                                                                 usingCommitTemplate: false, ensureCommitMessageSecondLineEmpty: false);
                    }
                    catch (Exception)
                    {
                    }
                });
            }

            MergeConflictHandler.HandleMergeConflicts(UICommands, this, AutoCommit.Checked);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
