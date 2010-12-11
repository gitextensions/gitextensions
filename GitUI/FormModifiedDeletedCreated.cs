using System;
using System.Drawing;

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

            questionImage.BackgroundImage = SystemIcons.Warning.ToBitmap(); 
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

        private void FormModifiedDeletedCreated_Load(object sender, EventArgs e)
        {
            //save position of this dialog, since the teksts could be to large when larger font is used.
            RestorePosition("formmodifieddeletedcreated");
            CenterToParent();
        }

        private void FormModifiedDeletedCreated_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            SavePosition("formmodifieddeletedcreated");
        }
    }
}