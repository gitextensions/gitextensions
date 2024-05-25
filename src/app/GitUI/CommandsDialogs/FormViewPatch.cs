using System.Text;
using GitCommands;
using GitCommands.Patches;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormViewPatch : GitModuleForm
    {
        private sealed class SortablePatchesList : SortableBindingList<Patch>
        {
            static SortablePatchesList()
            {
                AddSortableProperty(patch => patch.FileNameA, (x, y) => string.Compare(x.FileNameA, y.FileNameA, StringComparison.Ordinal));
                AddSortableProperty(patch => patch.ChangeType, (x, y) => string.Compare(x.ChangeType.ToString(), y.ChangeType.ToString(), StringComparison.Ordinal));
                AddSortableProperty(patch => patch.FileType, (x, y) => string.Compare(x.FileType.ToString(), y.FileType.ToString(), StringComparison.Ordinal));
            }
        }

        private readonly TranslationString _patchFileFilterString = new("Patch file (*.Patch)");
        private readonly TranslationString _patchFileFilterTitle = new("Select patch file");

        public FormViewPatch(IGitUICommands commands)
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

            Patch patch = (Patch)GridChangedFiles.SelectedRows[0].DataBoundItem;

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
                string text = System.IO.File.ReadAllText(PatchFileNameEdit.Text, GitModule.LosslessEncoding);
                List<Patch> patches = PatchProcessor.CreatePatchesFromString(text, new Lazy<Encoding>(() => Module.FilesEncoding)).ToList();
                SortablePatchesList patchesList = new();
                patchesList.AddRange(patches);
                GridChangedFiles.DataSource = patchesList;
            }
            catch
            {
            }
        }
    }
}
