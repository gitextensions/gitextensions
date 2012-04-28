using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using System.Collections.Generic;

namespace GitUI
{
    public partial class FormCheckoutBranch : GitExtensionsForm
    {
        public FormCheckoutBranch()
        {
            InitializeComponent();
            Translate();

            Initialize();
        }

        public FormCheckoutBranch(string branch, bool remote)
        {
            InitializeComponent();
            Translate();

            Initialize();

            LocalBranch.Checked = !remote;
            Remotebranch.Checked = remote;

            Branches.Text = branch;
        }

        private void Initialize()
        {
            Branches.DisplayMember = "Name";

            if (LocalBranch.Checked)
            {
                Branches.DataSource = Settings.Module.GetHeads(false);
            }
            else
            {
                var heads = Settings.Module.GetHeads(true, true);

                var remoteHeads = new List<GitHead>();

                foreach (var head in heads)
                {
                    if (head.IsRemote && !head.IsTag)
                        remoteHeads.Add(head);
                }

                Branches.DataSource = remoteHeads;
            }

            Branches.Text = null;
        }

        private void OkClick(object sender, EventArgs e)
        {
            if (Remotebranch.Checked)
            {
                var checkoutRemote = new FormCheckoutRemoteBranch(Branches.Text, Force.Checked);
                checkoutRemote.ShowDialog(this);
            }
            else
            {
                try
                {
                    var command = "checkout";
                    if (Force.Checked)
                        command += " --force";
                    command += " \"" + Branches.Text + "\"";
                    var form = new FormProcess(command);
                    form.ShowDialog(this);
                    if (!form.ErrorOccurred())
                        Close();
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
        }

        private void BranchTypeChanged()
        {
            Initialize();
        }

        private void LocalBranchCheckedChanged(object sender, EventArgs e)
        {
            BranchTypeChanged();
        }

        private void RemoteBranchCheckedChanged(object sender, EventArgs e)
        {
            BranchTypeChanged();
        }
    }
}