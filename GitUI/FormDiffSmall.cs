﻿using System;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI
{
    public sealed partial class FormDiffSmall : GitExtensionsForm
    {
        private readonly GitRevision revision;

        public FormDiffSmall(GitRevision revision)
            : base(true)
        {
            InitializeComponent(); Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();

            this.revision = revision;

            DiffFiles.GitItemStatuses = null;
            if (this.revision != null)
            {
                DiffFiles.GitItemStatuses = GitModule.Current.GetDiffFiles(revision.Guid, revision.Guid + "^");

                commitInfo.SetRevision(revision.Guid);
            }
        }

        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

        private void ViewSelectedDiff()
        {
            Cursor.Current = Cursors.WaitCursor;

            if (DiffFiles.SelectedItem != null && revision != null)
            {
                Patch selectedPatch = GitModule.Current.GetSingleDiff(revision.Guid, revision.Guid + "^", DiffFiles.SelectedItem.Name, DiffFiles.SelectedItem.OldName, DiffText.GetExtraDiffArguments(), DiffText.Encoding);
                DiffText.ViewPatch(selectedPatch != null ? selectedPatch.Text : "");
            }
            Cursor.Current = Cursors.Default;
        }

        void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

    }
}
