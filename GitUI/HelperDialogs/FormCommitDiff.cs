﻿using System;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        private readonly GitRevision _revision;

        private FormCommitDiff()
            : this(null, null)
        { 
        
        }

        public FormCommitDiff(GitUICommands aCommands, GitRevision revision)
            : base(aCommands)
        {
            InitializeComponent(); 
            Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();

            _revision = revision;

            DiffFiles.GitItemStatuses = null;
            if (_revision != null)
            {
                DiffFiles.SetDiff(revision);

                commitInfo.Revision = _revision;
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

        private void ViewSelectedDiff()
        {
            Cursor.Current = Cursors.WaitCursor;

            if (DiffFiles.SelectedItem != null && _revision != null)
            {
                DiffText.ViewChanges(_revision.Guid, DiffFiles.SelectedItemParent, DiffFiles.SelectedItem, String.Empty);
            }
            Cursor.Current = Cursors.Default;
        }

        void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

    }
}
