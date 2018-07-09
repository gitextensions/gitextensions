﻿using System;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    public partial class FormBlame : GitModuleForm
    {
        private FormBlame()
            : this(null)
        {
        }

        private FormBlame(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            Translate();

            this.AdjustForDpiScaling();
        }

        public FormBlame(GitUICommands commands, string fileName, GitRevision revision, int? initialLine = null) : this(commands)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            FileName = fileName;

            blameControl1.LoadBlame(revision ?? Module.GetRevision("Head"), null, fileName, null, null, Module.FilesEncoding, initialLine);
        }

        public string FileName { get; set; }

        private void FormBlameLoad(object sender, EventArgs e)
        {
            Text = $"Blame ({FileName})";
        }
    }
}