using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.Blame
{
    public partial class FormBlame : GitExtensionsForm
    {
        public FormBlame(string fileName, GitRevision revision)
        {
            InitializeComponent();
            Translate();

            if (string.IsNullOrEmpty(fileName))
                return;
            if (revision == null)
                revision = new GitRevision {Guid = "Head"};


            blameControl1.LoadBlame(revision.Guid, fileName, null);
        }

        public string FileName { get; set; }


        private void FormBlameFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("file-blame");
        }

        private void FormBlameLoad(object sender, EventArgs e)
        {
            RestorePosition("file-blame");
            Text = string.Format("Blame ({0})", FileName);
        }
    }
}