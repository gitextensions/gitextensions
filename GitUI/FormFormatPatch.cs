using System;
using System.IO;
using System.Net.Mail;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormFormatPatch : GitExtensionsForm
    {
        public FormFormatPatch()
        {
            InitializeComponent(); Translate();
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                OutputPath.Text = dialog.SelectedPath;
        }

        private void FormFormatPath_Load(object sender, EventArgs e)
        {
            OutputPath.Text = Settings.LastFormatPatchDir;
            string selectedHead = GitCommandHelpers.GetSelectedBranch();
            SelectedBranch.Text = "Current branch: " + selectedHead;

            SaveToDir_CheckedChanged(null, null);
            OutputPath.TextChanged += OutputPath_TextChanged;
            RevisionGrid.Load();
        }

        private void OutputPath_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(OutputPath.Text))
               Settings.LastFormatPatchDir = OutputPath.Text;
        }

        private void FormatPatch_Click(object sender, EventArgs e)
        {
            if (SaveToDir.Checked && string.IsNullOrEmpty(OutputPath.Text))
            {
                MessageBox.Show("You need to enter an output path.");
                return;
            }

            if (!SaveToDir.Checked && string.IsNullOrEmpty(MailAddress.Text))
            {
                MessageBox.Show("You need to enter an email address.");
                return;
            }

            if (!SaveToDir.Checked && string.IsNullOrEmpty(MailSubject.Text))
            {
                MessageBox.Show("You need to enter a mail subject.");
                return;
            }

            if (!SaveToDir.Checked && string.IsNullOrEmpty(Settings.Smtp))
            {
                MessageBox.Show("You need to enter a valid smtp in the settings dialog.");
                return;
            }

            string savePatchesToDir = OutputPath.Text;

            if (!SaveToDir.Checked)
            {
                savePatchesToDir = Settings.WorkingDirGitDir() + "\\PatchesToMail";
                if (Directory.Exists(savePatchesToDir))
                {
                    foreach (string file in Directory.GetFiles(savePatchesToDir, "*.patch"))
                        File.Delete(file);
                }
                else
                {
                    Directory.CreateDirectory(savePatchesToDir);
                }
            }

            string rev1 = "";
            string rev2 = "";
            string result = "";

            if (RevisionGrid.GetRevisions().Count > 0)
            {
                if (RevisionGrid.GetRevisions().Count == 1)
                {
                    rev1 = RevisionGrid.GetRevisions()[0].ParentGuids[0];
                    rev2 = RevisionGrid.GetRevisions()[0].Guid;
                    result = GitCommandHelpers.FormatPatch(rev1, rev2, savePatchesToDir);
                }

                if (RevisionGrid.GetRevisions().Count == 2)
                {
                    rev1 = RevisionGrid.GetRevisions()[0].ParentGuids[0];
                    rev2 = RevisionGrid.GetRevisions()[1].Guid;
                    result = GitCommandHelpers.FormatPatch(rev1, rev2, savePatchesToDir);
                }

                if (RevisionGrid.GetRevisions().Count > 2)
                {
                    int n = 0;
                    foreach (GitRevision revision in RevisionGrid.GetRevisions())
                    {
                        n++;
                        rev1 = revision.ParentGuids[0];
                        rev2 = revision.Guid;
                        result += GitCommandHelpers.FormatPatch(rev1, rev2, savePatchesToDir, n);
                    }
                }
            }
            else
                if (string.IsNullOrEmpty(rev1) || string.IsNullOrEmpty(rev2))
                {
                    MessageBox.Show("You need to select 2 revisions", "Patch error");
                    return;
                }

            if (!SaveToDir.Checked)
            {
                if (SendMail(savePatchesToDir))
                    result += "\n\nSend to: " + MailAddress.Text;
                else
                    result += "\n\nFailed to send mail.";


                //Clean up
                if (Directory.Exists(savePatchesToDir))
                {
                    foreach (string file in Directory.GetFiles(savePatchesToDir, "*.patch"))
                        File.Delete(file);
                }
            }

            MessageBox.Show(result, "Patch result");
            Close();
        }

        private bool SendMail(string dir)
        {
            try
            {
                string from = GitCommandHelpers.GetSetting("user.email");

                if (string.IsNullOrEmpty(from))
                    from = GitCommandHelpers.GetGlobalSetting("user.email");

                if (string.IsNullOrEmpty(from))
                    MessageBox.Show("There is no email address configured in the settings dialog.");

                string to = MailAddress.Text;

                using (var mail = new MailMessage(from, to, MailSubject.Text, MailBody.Text))
                {
                    foreach (string file in Directory.GetFiles(dir, "*.patch"))
                    {
                        var attacheMent = new Attachment(file);
                        mail.Attachments.Add(attacheMent);
                    }

                    var smtpClient = new SmtpClient(Settings.Smtp);
                    smtpClient.Send(mail);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        private void SaveToDir_CheckedChanged(object sender, EventArgs e)
        {
            OutputPath.Enabled = SaveToDir.Checked;
            MailAddress.Enabled = !SaveToDir.Checked;
            MailSubject.Enabled = !SaveToDir.Checked;
            MailBody.Enabled = !SaveToDir.Checked;
        }
    }
}
