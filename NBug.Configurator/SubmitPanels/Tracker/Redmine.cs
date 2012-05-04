// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Redmine.cs" company="NBusy Project">
//   Copyright © 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace NBug.Configurator.SubmitPanels.Tracker
{
	using System;
	using System.Windows.Forms;

	public partial class Redmine : UserControl, ISubmitPanel
	{
		public Redmine()
		{
			InitializeComponent();
		}

		public string ConnectionString
		{
			get
			{
				// Check the mendatory fields
				if (string.IsNullOrEmpty(this.trackerURLTextBox.Text))
				{
					MessageBox.Show("Mandatory field \"" + trackerURLLabel.Name + "\" cannot be left blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return null;
				}
				else if (string.IsNullOrEmpty(this.projectIDTextBox.Text))
				{
					MessageBox.Show("Mandatory field \"" + projectIDLabel.Name + "\" cannot be left blank.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return null;
				}

				// Do fixes
				if (!this.trackerURLTextBox.Text.EndsWith("/"))
				{
					this.trackerURLTextBox.Text += "/";
				}

				var redmine = new Core.Submission.Tracker.Redmine
					{
						Url = this.trackerURLTextBox.Text,
						ProjectId = this.projectIDTextBox.Text,
						TrackerId = this.trackerIDTextBox.Text,
						PriorityId = this.priorityIDTextBox.Text,
						CategoryId = this.categoryIDTextBox.Text,
						CustomSubject = this.customSubjectTextBox.Text,
						FixedVersionId = this.fixedVersionIDTextBox.Text,
						AssignedToId = this.assignedToIDTextBox.Text,
						ParentId = this.parentIDTextBox.Text,
						StatusId = this.statusIDTextBox.Text,
						AuthorId = this.authorIDTextBox.Text
					};

				if (this.apiKeyRadioButton.Checked)
				{
					redmine.ApiKey = this.apiKeyTextBox.Text;
				}

				return redmine.ConnectionString;
			}

			set
			{
				var redmine = new Core.Submission.Tracker.Redmine(value);

				this.trackerURLTextBox.Text = redmine.Url;
				this.projectIDTextBox.Text = redmine.ProjectId;
				this.trackerIDTextBox.Text = redmine.TrackerId;
				this.priorityIDTextBox.Text = redmine.PriorityId;
				this.categoryIDTextBox.Text = redmine.CategoryId;
				this.customSubjectTextBox.Text = redmine.CustomSubject;
				this.fixedVersionIDTextBox.Text = redmine.FixedVersionId;
				this.assignedToIDTextBox.Text = redmine.AssignedToId;
				this.parentIDTextBox.Text = redmine.ParentId;
				this.statusIDTextBox.Text = redmine.StatusId;
				this.authorIDTextBox.Text = redmine.AuthorId;

				if (!string.IsNullOrEmpty(redmine.ApiKey))
				{
					this.apiKeyRadioButton.Checked = true;
					this.apiKeyTextBox.Text = redmine.ApiKey;
				}
				else
				{
					this.anonymousRadioButton.Checked = true;
				}
			}
		}

		private void AnonymousRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (this.anonymousRadioButton.Checked)
			{
				this.apiKeyTextBox.Enabled = false;
			}
		}

		private void ApiKeyRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			if (this.apiKeyRadioButton.Checked)
			{
				this.apiKeyTextBox.Enabled = true;
			}
		}
	}
}
