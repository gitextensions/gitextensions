﻿using System;

namespace GitUI
{
    public sealed partial class FormAddFiles : GitModuleForm
    {

        /// <summary>
        /// For VS designer
        /// </summary>
        private FormAddFiles()
            : this(null)
        {
        }

        public FormAddFiles(GitUICommands aCommands, string addFile)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            Filter.Text = addFile ?? ".";
        }

        public FormAddFiles(GitUICommands aCommands)
            : this(aCommands, null)
        {
        }

        private void AddFilesClick(object sender, EventArgs e)
        {
            var argumentFormat = force.Checked ? "add -f \"{0}\"" : "add \"{0}\"";
            FormProcess.ShowDialog(this, string.Format(argumentFormat, Filter.Text), false);
        }

        private void ShowFilesClick(object sender, EventArgs e)
        {
            FormProcess.ShowDialog(this, string.Format("add --dry-run \"{0}\"", Filter.Text), false);
        }
    }
}