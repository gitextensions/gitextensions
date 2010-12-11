using System;

namespace GitUI
{
    public partial class FormModifiedDeletedCreated : GitExtensionsForm
    {
        public FormModifiedDeletedCreated(string localText, string remoteText, string baseText, string description)
        {
            InitializeComponent();
            Translate();
            Aborted = true;
            KeepLocal = false;
            KeepRemote = false;
            KeepBase = false;

            Local.Text = localText;
            Remote.Text = remoteText;
            Base.Text = baseText;
            Label.Text = description;
        }

        public bool Aborted { get; set; }
        public bool KeepLocal { get; set; }
        public bool KeepRemote { get; set; }
        public bool KeepBase { get; set; }

        private void AbortClick(object sender, EventArgs e)
        {
            Aborted = true;
            Close();
        }

        private void Local_Click(object sender, EventArgs e)
        {
            Aborted = false;
            KeepLocal = true;
            KeepRemote = false;
            KeepBase = false;
            Close();
        }

        private void Remote_Click(object sender, EventArgs e)
        {
            Aborted = false;
            KeepLocal = false;
            KeepRemote = true;
            KeepBase = false;
            Close();
        }

        private void Base_Click(object sender, EventArgs e)
        {
            Aborted = false;
            KeepLocal = false;
            KeepRemote = false;
            KeepBase = true;
            Close();
        }
    }
}