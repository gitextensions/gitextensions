using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Patches;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormViewPatch : GitModuleForm
    {
        private readonly TranslationString _patchFileFilterString = new("Patch file (*.Patch)");
        private readonly TranslationString _patchFileFilterTitle = new("Select patch file");

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
            ChangesList.ExtraDiffArgumentsChanged += GridChangedFiles_SelectionChanged;

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

            if (patch is null)
            {
                return;
            }

            ChangesList.ViewFixedPatch(patch.FileNameB, patch.Text ?? "");
        }

        private void BrowsePatch_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new()
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
                var patches = PatchProcessor.CreatePatchesFromString(text, new Lazy<Encoding>(() => Module.FilesEncoding)).ToList();

                GridChangedFiles.DataSource = patches;
            }
            catch
            {
            }
        }
    }
}
