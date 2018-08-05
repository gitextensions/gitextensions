using System;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public partial class FormBlame : GitModuleForm
    {
        /// <summary>
        /// For VS designer and translation test.
        /// </summary>
        private FormBlame()
        {
            InitializeComponent();
        }

        private FormBlame(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
        }

        public FormBlame(GitUICommands commands, string fileName, [CanBeNull] GitRevision revision, int? initialLine = null)
            : this(commands)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            FileName = fileName;

            blameControl1.LoadBlame(revision ?? Module.GetRevision(), null, fileName, null, null, Module.FilesEncoding, initialLine);
        }

        public string FileName { get; set; }

        private void FormBlameLoad(object sender, EventArgs e)
        {
            Text = $"Blame ({FileName})";
        }
    }
}