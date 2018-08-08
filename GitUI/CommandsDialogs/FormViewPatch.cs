using System;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Patches;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormViewPatch : GitModuleForm
    {
        private readonly TranslationString _patchFileFilterString = new TranslationString("Patch file (*.Patch)");
        private readonly TranslationString _patchFileFilterTitle = new TranslationString("Select patch file");

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormViewPatch()
        {
            InitializeComponent();
        }

        public FormViewPatch(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();

            typeDataGridViewTextBoxColumn.Width = DpiUtil.Scale(70);
            File.Width = DpiUtil.Scale(50);

            InitializeComplete();

            typeDataGridViewTextBoxColumn.DataPropertyName = nameof(Patch.ChangeType);
            File.DataPropertyName = nameof(Patch.FileType);
            FileNameA.DataPropertyName = nameof(Patch.FileNameA);
        }

        public void LoadPatch(string patch)
        {
            PatchFileNameEdit.Text = patch;
            LoadPatchFile();
        }

        private void GridChangedFiles_SelectionChanged(object sender, EventArgs e)
        {
            if (GridChangedFiles.SelectedRows.Count == 0)
            {
                return;
            }

            var patch = (Patch)GridChangedFiles.SelectedRows[0].DataBoundItem;

            if (patch == null)
            {
                return;
            }

            ChangesList.ViewPatch(patch);
        }

        private void BrowsePatch_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = _patchFileFilterString.Text + "|*.patch",
                InitialDirectory = @".",
                Title = _patchFileFilterTitle.Text
            };

            using (dialog)
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    PatchFileNameEdit.Text = dialog.FileName;
                }
            }

            LoadPatchFile();
        }

        private void LoadPatchFile()
        {
            try
            {
                var text = System.IO.File.ReadAllText(PatchFileNameEdit.Text, GitModule.LosslessEncoding);
                var patches = PatchProcessor.CreatePatchesFromString(text, Module.FilesEncoding).ToList();

                GridChangedFiles.DataSource = patches;
            }
            catch
            {
            }
        }
    }
}
