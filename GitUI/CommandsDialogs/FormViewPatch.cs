using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
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
        private sealed class SortablePatchesList : BindingList<Patch>
        {
            public void AddRange(IEnumerable<Patch> patches)
            {
                Patches.AddRange(patches);

                // NOTE: adding items via wrapper's AddRange doesn't generate ListChanged event, so DataGridView doesn't update itself
                // There are two solutions:
                //  0. Add items one by one using direct this.Add method (without IList<T> wrapper).
                //     Too many ListChanged events will be generated (one per item), too many updates for gridview. Bad performance.
                //  1. Batch add items through Items wrapper's AddRange method.
                //     One reset event will be generated, one batch update for gridview. Ugly but fast code.
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }

            protected override bool SupportsSortingCore => true;

            protected override void ApplySortCore(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
            {
                Patches.Sort(PatchesComparer.Create(propertyDescriptor, direction == ListSortDirection.Descending));
            }

            private List<Patch> Patches => (List<Patch>)Items;

            private static class PatchesComparer
            {
                private static readonly Dictionary<string, Comparison<Patch>> PropertyComparers = new Dictionary<string, Comparison<Patch>>();

                static PatchesComparer()
                {
                    AddSortableProperty(patch => patch.FileNameA, (x, y) => string.Compare(x.FileNameA, y.FileNameA, StringComparison.Ordinal));
                    AddSortableProperty(patch => patch.ChangeType, (x, y) => string.Compare(x.ChangeType.ToString(), y.ChangeType.ToString(), StringComparison.Ordinal));
                    AddSortableProperty(patch => patch.FileType, (x, y) => string.Compare(x.FileType.ToString(), y.FileType.ToString(), StringComparison.Ordinal));
                }

                /// <summary>
                /// Creates a comparer to sort lostObjects by specified property.
                /// </summary>
                /// <param name="propertyDescriptor">Property to sort by.</param>
                /// <param name="isReversedComparing">Use reversed sorting order.</param>
                public static Comparison<Patch> Create(PropertyDescriptor propertyDescriptor, bool isReversedComparing)
                {
                    if (PropertyComparers.TryGetValue(propertyDescriptor.Name, out var comparer))
                    {
                        return isReversedComparing ? (x, y) => comparer(y, x) : comparer;
                    }

                    throw new NotSupportedException(string.Format("Custom sort by {0} property is not supported.", propertyDescriptor.Name));
                }

                /// <summary>
                /// Adds custom property comparer.
                /// </summary>
                /// <typeparam name="T">Property type.</typeparam>
                /// <param name="expr">Property to sort by.</param>
                /// <param name="propertyComparer">Property values comparer.</param>
                private static void AddSortableProperty<T>(Expression<Func<Patch, T>> expr, Comparison<Patch> propertyComparer)
                {
                    PropertyComparers[((MemberExpression)expr.Body).Member.Name] = propertyComparer;
                }
            }
        }

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
                var patchesList = new SortablePatchesList();
                patchesList.AddRange(patches);
                GridChangedFiles.DataSource = patchesList;
            }
            catch
            {
            }
        }
    }
}
