using System;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    public partial class FormBlame : GitModuleForm
    {
        private FormBlame()
            : this(null)
        {
        }

        private FormBlame(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
        }

        public FormBlame(GitUICommands aCommands, string fileName, GitRevision revision, int? initialLine = null) : this(aCommands)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            FileName = fileName;
            if (revision == null)
                revision = Module.GetRevision("Head");

            blameControl1.LoadBlame(revision, null, fileName, null, null, Module.FilesEncoding, initialLine);
        }

        public string FileName { get; set; }


        private void FormBlameLoad(object sender, EventArgs e)
        {
            Text = string.Format("Blame ({0})", FileName);
        }
    }
}