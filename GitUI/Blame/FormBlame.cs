using System;
using GitCommands;

namespace GitUI.Blame
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

        public FormBlame(GitUICommands aCommands, string fileName, GitRevision revision) : this(aCommands)
        {
            if (string.IsNullOrEmpty(fileName))
                return;

            if (revision == null)
                revision = new GitRevision(Module, "Head");


            blameControl1.LoadBlame(revision.Guid, fileName, null, null, Module.FilesEncoding);
        }

        public string FileName { get; set; }


        private void FormBlameLoad(object sender, EventArgs e)
        {
            Text = string.Format("Blame ({0})", FileName);
        }
    }
}