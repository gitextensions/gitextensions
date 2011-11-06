﻿using System;
using System.Windows.Forms;
using GitCommands;
using PatchApply;

namespace GitUI
{
    public partial class FormDiffSmall : GitExtensionsForm
    {
        public FormDiffSmall()
        {
            InitializeComponent(); Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();
        }

        private void FormDiffSmall_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("diff-small");
        }

        private void FormDiffSmall_Load(object sender, EventArgs e)
        {
            RestorePosition("diff-small");
        }

        private GitRevision Revision;

        public void SetRevision(GitRevision revision)
        {
            Revision = revision;
            DiffFiles.GitItemStatuses = null;
            DiffFiles.GitItemStatuses = Settings.Module.GetDiffFiles(revision.Guid, revision.Guid + "^");

            commitInfo.SetRevision(revision.Guid);
        }

        public void SetRevision(string revision)
        {
            Revision = new GitRevision(revision) { ParentGuids = new[] { revision + "^" } };
            SetRevision(Revision);
        }


        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

        private void ViewSelectedDiff()
        {
            Cursor.Current = Cursors.WaitCursor;

            if (DiffFiles.SelectedItem != null)
            {
                Patch selectedPatch = Settings.Module.GetSingleDiff(Revision.Guid, Revision.Guid + "^", DiffFiles.SelectedItem.Name, DiffFiles.SelectedItem.OldName, DiffText.GetExtraDiffArguments());
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
