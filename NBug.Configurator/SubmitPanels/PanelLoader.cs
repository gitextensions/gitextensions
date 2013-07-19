// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanelLoader.cs" company="NBusy Project">
//   Copyright © 2010 - 2011 Teoman Soygul. Licensed under LGPLv3 (http://www.gnu.org/licenses/lgpl.html).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using NBug.Core.Util;

namespace NBug.Configurator.SubmitPanels
{
	using System;
	using System.Windows.Forms;

	public partial class PanelLoader : UserControl
	{		
		private string settingsLoadedProtocolType;
        public string connString;
        public event EventHandler RemoveDestination;

		public PanelLoader()
		{
			InitializeComponent();
			this.submitComboBox.SelectedIndex = 0;
		}

		public void LoadPanel(string connectionString)
		{
			connString = connectionString;
			var protocol = ConnectionStringParser.Parse(connectionString)["Type"];

			if (protocol == typeof(Core.Submission.Web.Mail).Name || protocol.ToLower() == "email" || protocol.ToLower() == "e-mail")
			{
				submitComboBox.SelectedItem = "E-Mail";
			}
			else if (protocol == typeof(Core.Submission.Tracker.Redmine).Name)
			{
				submitComboBox.SelectedItem = "Redmine Issue Tracker";
			}
			else if (protocol == typeof(Core.Submission.Web.Ftp).Name)
			{
				submitComboBox.SelectedItem = "FTP";
			}
			else if (protocol == typeof(Core.Submission.Web.Http).Name)
			{
				submitComboBox.SelectedItem = "HTTP";
			}
			else
			{
				MessageBox.Show("Undefined protocol type was selected. This is an internal error, please notify the developers.");
			}

			settingsLoadedProtocolType = submitComboBox.Text;

			if (Controls.Count == 2)
			{
				((ISubmitPanel)Controls[0]).ConnectionString = connectionString;
			}
		}

		public void UnloadPanel()
		{
			this.submitComboBox.SelectedItem = "None";
		}

		private void SubmitComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.Controls.Count == 2)
			{
				this.Controls.RemoveAt(0);
			}

			switch (this.submitComboBox.SelectedItem.ToString())
			{
				case "E-Mail":
					this.Controls.Add(new Web.Mail());
					break;

				case "Redmine Issue Tracker":
					this.Controls.Add(new Tracker.Redmine());
					break;

				case "FTP":
					this.Controls.Add(new Web.Ftp());
					break;

				case "HTTP":
					this.Controls.Add(new Web.Http());
					break;
			}

			if (this.Controls.Count == 2)
			{
				this.Controls[1].Dock = DockStyle.Top;
				this.Controls[1].BringToFront(); // Note that this swaps Controls[1] -> Controls[0] so submit panel is 0 now!

				if (this.submitComboBox.SelectedItem.ToString() == this.settingsLoadedProtocolType)
				{
					((ISubmitPanel)this.Controls[0]).ConnectionString = this.connString;
				}
			}
		}

        private void btnRemove_Click(object sender, EventArgs e)
        {
            RemoveDestination.DynamicInvoke(this, new EventArgs());
        }
	}
}