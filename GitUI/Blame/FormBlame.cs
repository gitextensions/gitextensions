using System;
using GitCommands;

namespace GitUI.Blame
{
    public partial class FormBlame : GitExtensionsForm
    {
        public FormBlame(string fileName, GitRevision revision) : base(true)
        {
            InitializeComponent();
            Translate();

            if (string.IsNullOrEmpty(fileName))
                return;
            if (revision == null)
                revision = new GitRevision("Head");


            blameControl1.LoadBlame(revision.Guid, fileName, null, null, Settings.FilesEncoding);
        }

        public string FileName { get; set; }


        private void FormBlameLoad(object sender, EventArgs e)
        {
            Text = string.Format("Blame ({0})", FileName);
        }
    }
}