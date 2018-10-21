using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.CommandsDialogs.FormatPatchDialog;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormFormatPatch : GitModuleForm
    {
        private readonly TranslationString _currentBranchText = new TranslationString("Current branch:");
        private readonly TranslationString _noOutputPathEnteredText =
            new TranslationString("You need to enter an output path.");
        private readonly TranslationString _noEmailEnteredText =
            new TranslationString("You need to enter an email address.");
        private readonly TranslationString _noSubjectEnteredText =
            new TranslationString("You need to enter a mail subject.");
        private readonly TranslationString _wrongSmtpSettingsText =
            new TranslationString("You need to enter a valid smtp in the settings dialog.");
        private readonly TranslationString _revisionsNeededText =
            new TranslationString("You need to select at least one revision");
        private readonly TranslationString _revisionsNeededCaption =
            new TranslationString("Patch error");
        private readonly TranslationString _sendMailResult =
            new TranslationString("Send to:");
        private readonly TranslationString _sendMailResultFailed =
            new TranslationString("Failed to send mail.");
        private readonly TranslationString _patchResultCaption =
            new TranslationString("Patch result");
        private readonly TranslationString _noGitMailConfigured =
            new TranslationString("There is no email address configured in the settings dialog.");

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormFormatPatch()
        {
            InitializeComponent();
        }

        public FormFormatPatch(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            RevisionGrid.ShowUncommittedChangesIfPossible = false;
            InitializeComplete();

            MailFrom.Text = Module.GetEffectiveSetting(SettingKeyString.UserEmail);
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            var userSelectedPath = OsShellUtil.PickFolder(this);

            if (userSelectedPath != null)
            {
                OutputPath.Text = userSelectedPath;
            }
        }

        private void FormFormatPath_Load(object sender, EventArgs e)
        {
            OutputPath.Text = AppSettings.LastFormatPatchDir;
            string selectedHead = Module.GetSelectedBranch();
            SelectedBranch.Text = _currentBranchText.Text + " " + selectedHead;

            SaveToDir_CheckedChanged(null, null);
            OutputPath.TextChanged += OutputPath_TextChanged;
            RevisionGrid.Load();
        }

        private void OutputPath_TextChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(OutputPath.Text))
            {
                AppSettings.LastFormatPatchDir = OutputPath.Text;
            }
        }

        private void FormatPatch_Click(object sender, EventArgs e)
        {
            if (SaveToDir.Checked && string.IsNullOrEmpty(OutputPath.Text))
            {
                MessageBox.Show(this, _noOutputPathEnteredText.Text);
                return;
            }

            if (!SaveToDir.Checked && string.IsNullOrEmpty(MailTo.Text))
            {
                MessageBox.Show(this, _noEmailEnteredText.Text);
                return;
            }

            if (!SaveToDir.Checked && string.IsNullOrEmpty(MailSubject.Text))
            {
                MessageBox.Show(this, _noSubjectEnteredText.Text);
                return;
            }

            if (!SaveToDir.Checked && string.IsNullOrEmpty(AppSettings.SmtpServer))
            {
                MessageBox.Show(this, _wrongSmtpSettingsText.Text);
                return;
            }

            string savePatchesToDir = OutputPath.Text;

            if (!SaveToDir.Checked)
            {
                savePatchesToDir = Path.Combine(Module.WorkingDirGitDir, "PatchesToMail");
                if (Directory.Exists(savePatchesToDir))
                {
                    foreach (string file in Directory.GetFiles(savePatchesToDir, "*.patch"))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    Directory.CreateDirectory(savePatchesToDir);
                }
            }

            string rev1 = "";
            string rev2 = "";
            string result = "";

            var revisions = RevisionGrid.GetSelectedRevisions();
            if (revisions.Count > 0)
            {
                if (revisions.Count == 1)
                {
                    var parents = revisions[0].ParentIds;
                    rev1 = parents?.Count > 0 ? parents[0].ToString() : "";
                    rev2 = revisions[0].Guid;
                    result = Module.FormatPatch(rev1, rev2, savePatchesToDir);
                }
                else if (revisions.Count == 2)
                {
                    var parents = revisions[0].ParentIds;
                    rev1 = parents?.Count > 0 ? parents[0].ToString() : "";
                    rev2 = revisions[1].Guid;
                    result = Module.FormatPatch(rev1, rev2, savePatchesToDir);
                }
                else
                {
                    int n = 0;
                    foreach (GitRevision revision in revisions)
                    {
                        n++;
                        var parents = revision.ParentIds;
                        rev1 = parents?.Count > 0 ? parents[0].ToString() : "";
                        rev2 = revision.Guid;
                        result += Module.FormatPatch(rev1, rev2, savePatchesToDir, n);
                    }
                }
            }
            else
            {
                MessageBox.Show(this, _revisionsNeededText.Text, _revisionsNeededCaption.Text);
                return;
            }

            if (!SaveToDir.Checked)
            {
                result += Environment.NewLine + Environment.NewLine;
                if (SendMail(savePatchesToDir))
                {
                    result += _sendMailResult.Text + " " + MailTo.Text;
                }
                else
                {
                    result += _sendMailResultFailed.Text;
                }

                // Clean up
                if (Directory.Exists(savePatchesToDir))
                {
                    foreach (string file in Directory.GetFiles(savePatchesToDir, "*.patch"))
                    {
                        File.Delete(file);
                    }
                }
            }

            MessageBox.Show(this, result, _patchResultCaption.Text);
            Close();
        }

        private bool SendMail(string dir)
        {
            try
            {
                string from = MailFrom.Text;

                if (string.IsNullOrEmpty(from))
                {
                    MessageBox.Show(this, _noGitMailConfigured.Text);
                }

                string to = MailTo.Text;

                using (var mail = new MailMessage(from, to, MailSubject.Text, MailBody.Text))
                {
                    foreach (string file in Directory.GetFiles(dir, "*.patch"))
                    {
                        var attachment = new Attachment(file);
                        mail.Attachments.Add(attachment);
                    }

                    var smtpClient = new SmtpClient(AppSettings.SmtpServer)
                    {
                        Port = AppSettings.SmtpPort,
                        EnableSsl = AppSettings.SmtpUseSsl
                    };

                    using (var credentials = new SmtpCredentials())
                    {
                        credentials.login.Text = from;

                        smtpClient.Credentials = credentials.ShowDialog(this) == DialogResult.OK
                            ? new NetworkCredential(credentials.login.Text, credentials.password.Text)
                            : CredentialCache.DefaultNetworkCredentials;
                    }

                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    smtpClient.Send(mail);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return false;
            }

            return true;
        }

        private void SaveToDir_CheckedChanged(object sender, EventArgs e)
        {
            OutputPath.Enabled = SaveToDir.Checked;
            MailFrom.Enabled = !SaveToDir.Checked;
            MailTo.Enabled = !SaveToDir.Checked;
            MailSubject.Enabled = !SaveToDir.Checked;
            MailBody.Enabled = !SaveToDir.Checked;
        }
    }
}
