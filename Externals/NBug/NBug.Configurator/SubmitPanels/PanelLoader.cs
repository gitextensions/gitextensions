// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PanelLoader.cs" company="NBug Project">
//   Copyright (c) 2011 - 2013 Teoman Soygul. Licensed under MIT license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using NBug.Configurator.SubmitPanels.Tracker;

namespace NBug.Configurator.SubmitPanels
{
    using System;
    using System.Windows.Forms;

    using NBug.Core.Submission.Tracker;
    using NBug.Core.Submission.Web;
    using NBug.Core.Util;

    public partial class PanelLoader : UserControl
    {
        public string connString;

        private string settingsLoadedProtocolType;

        public PanelLoader()
        {
            this.InitializeComponent();
            this.submitComboBox.SelectedIndex = 0;
        }

        public event EventHandler RemoveDestination;

        public void LoadPanel(string connectionString)
        {
            this.connString = connectionString;
            var protocol = ConnectionStringParser.Parse(connectionString)["Type"];

            if (protocol == typeof(Mail).Name || protocol.ToLower() == "email" || protocol.ToLower() == "e-mail")
            {
                this.submitComboBox.SelectedItem = "E-Mail";
            }
            else if (protocol == typeof(Redmine).Name)
            {
                this.submitComboBox.SelectedItem = "Redmine Issue Tracker";
            }
            else if (protocol == typeof(Ftp).Name)
            {
                this.submitComboBox.SelectedItem = "FTP";
            }
            else if (protocol == typeof(Http).Name)
            {
                this.submitComboBox.SelectedItem = "HTTP";
            }
            else if (protocol == typeof(Custom.Custom).Name)
            {
                this.submitComboBox.SelectedItem = "Custom";
            }
            else if (protocol == typeof(Core.Submission.Tracker.Mantis.Mantis).Name)
            {
                this.submitComboBox.SelectedItem = "Mantis Bug Tracker";
            }
            else
            {
                MessageBox.Show("Undefined protocol type was selected. This is an internal error, please notify the developers.");
            }

            this.settingsLoadedProtocolType = this.submitComboBox.Text;

            if (this.Controls.Count == 2)
            {
                ((ISubmitPanel)this.Controls[0]).ConnectionString = connectionString;
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

                case "Custom":
                    this.Controls.Add(new Custom.Custom());
                    break;

                case "Mantis Bug Tracker":
                    this.Controls.Add(new Mantis());
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
            this.RemoveDestination.DynamicInvoke(this, new EventArgs());
        }

    }
}