using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PatchApply;
using GitCommands;
using System.Text.RegularExpressions;

namespace GitUI
{
    public partial class FormDiffSmall : GitExtensionsForm
    {
        public FormDiffSmall()
        {
            InitializeComponent();
            DiffText.ExtraDiffArgumentsChanged += new EventHandler<EventArgs>(DiffText_ExtraDiffArgumentsChanged);
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

        private GitRevision Revision = null;

        public void SetRevision(GitRevision revision)
        {
            Revision = revision;
            DiffFiles.GitItemStatusses = null;

            if (revision.ParentGuids.Count > 0)
                DiffFiles.GitItemStatusses = GitCommands.GitCommands.GetDiffFiles(revision.Guid, revision.ParentGuids[0]);

            commitInfo.SetRevision(revision.Guid);

        }

        public void SetRevision(string revision)
        {
            Revision = new GitRevision();
            Revision.Guid = revision;
            Revision.ParentGuids.Add(revision + "^");
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
                Patch selectedPatch = GitCommands.GitCommands.GetSingleDiff(Revision.Guid, Revision.ParentGuids[0], DiffFiles.SelectedItem.Name, DiffText.GetExtraDiffArguments());
                if (selectedPatch != null)
                {
                    DiffText.ViewPatch(selectedPatch.Text);
                }
                else
                {
                    DiffText.ViewPatch("");
                }
            }
        }

        void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ViewSelectedDiff();
        }

    }
}
