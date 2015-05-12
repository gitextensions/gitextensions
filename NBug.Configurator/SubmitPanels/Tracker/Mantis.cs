using System.Globalization;

namespace NBug.Configurator.SubmitPanels.Tracker
{
    using System;
    using System.Windows.Forms;

    public partial class Mantis : UserControl, ISubmitPanel
    {
        public Mantis()
        {
            InitializeComponent();
        }

        private void Warning(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public string ConnectionString
        {
            get
            {
                // Check Mandatory Fields
                if (string.IsNullOrEmpty(trackerURLTextBox.Text.Trim()))
                {
                    Warning(String.Format("Mandatory field {0} cannot be left blank.", trackerURLLabel.Name));
                    return null;
                }

                if (string.IsNullOrEmpty(usernameTextBox.Text.Trim()))
                {
                    Warning(String.Format("Mandatory field {0} cannot be left blank.", usernameTextBox.Name));
                    return null;
                }

                if (string.IsNullOrEmpty(passwordTextBox.Text.Trim()))
                {
                    Warning(String.Format("Mandatory field {0} cannot be left blank.", passwordTextBox.Name));
                    return null;
                }

                if (string.IsNullOrEmpty(categoryTextBox.Text.Trim()))
                {
                    Warning(String.Format("Mandatory field {0} cannot be left blank.", categoryTextBox.Name));
                    return null;
                }

                if (string.IsNullOrEmpty(projectIDTextBox.Text.Trim()) || projectIDTextBox.Text.Trim() == "0")
                {
                    Warning(String.Format("Mandatory field {0} cannot be left blank.", projectIDTextBox.Name));
                    return null;
                }

                int projectId;
                Int32.TryParse(projectIDTextBox.Text.Trim(), out projectId);
                if (projectId <= 0)
                {
                    Warning("Project ID has to be positive numeric value.");
                }

                var mantis = new Core.Submission.Tracker.Mantis.Mantis
                {
                    Url = trackerURLTextBox.Text,
                    Username = usernameTextBox.Text,
                    Password = passwordTextBox.Text,
                    ProjectId = Int32.Parse(projectIDTextBox.Text),
                    Category =  categoryTextBox.Text,
                    SendAttachment = uploadCheckbox.Checked,
                    SuccessIfAttachmentFails = uploadFailCheckBox.Checked,
                    AddVersionIfNotExists = versionAddCheckBox.Checked
                };

                if (! string.IsNullOrEmpty(summaryTextBox.Text))
                {
                    mantis.Summary = summaryTextBox.Text;
                }

                return mantis.ConnectionString;
            }

            set
            {
                var mantis = new Core.Submission.Tracker.Mantis.Mantis(value);

                trackerURLTextBox.Text = mantis.Url;
                usernameTextBox.Text = mantis.Username;
                passwordTextBox.Text = mantis.Password;
                categoryTextBox.Text = mantis.Category;
                projectIDTextBox.Text = mantis.ProjectId.ToString(CultureInfo.InvariantCulture);
                uploadCheckbox.Checked = mantis.SendAttachment;
                uploadFailCheckBox.Enabled = mantis.SendAttachment;
                uploadFailCheckBox.Checked = mantis.SuccessIfAttachmentFails;
                summaryTextBox.Text = mantis.Summary;
                versionAddCheckBox.Checked = mantis.AddVersionIfNotExists;
            }
        }

        private void uploadCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (uploadCheckbox.Checked)
            {
                uploadFailCheckBox.Enabled = true;
            }
            else
            {
                uploadFailCheckBox.Checked = false;
                uploadFailCheckBox.Enabled = false;
            }
        }
    }
}