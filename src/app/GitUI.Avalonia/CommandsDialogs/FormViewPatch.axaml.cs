using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using GitCommands;
using GitCommands.Patches;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public partial class FormViewPatch : GitModuleForm
{
    private readonly TranslationString _patchFileFilterString = new("Patch file (*.Patch)");
    private readonly TranslationString _patchFileFilterTitle = new("Select patch file");
    private IReadOnlyList<Patch> _patches = [];
    private string? _sortColumn;
    private bool _sortAscending = true;

    public FormViewPatch()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormViewPatch(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        WireControls();
        ChangesList.UICommandsSource = this;
        InitializeComplete();
    }

    private void WireControls()
    {
        GridChangedFiles.ItemTemplate = new FuncDataTemplate<Patch>(
            (_, _) => new PatchRow(),
            supportsRecycling: true);
        GridChangedFiles.SelectionChanged += GridChangedFiles_SelectionChanged;
        ChangesList.ExtraDiffArgumentsChanged += GridChangedFiles_SelectionChanged;
        BrowsePatch.Click += BrowsePatch_Click;
        FileNameA.PointerReleased += Header_PointerReleased;
        typeDataGridViewTextBoxColumn.PointerReleased += Header_PointerReleased;
        File.PointerReleased += Header_PointerReleased;
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddHeaderTranslationItem(translation, nameof(FileNameA), "Filename");
        AddHeaderTranslationItem(translation, nameof(typeDataGridViewTextBoxColumn), "Change");
        AddHeaderTranslationItem(translation, nameof(File), "Type");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, nameof(FileNameA), FileNameA, "Filename");
        TranslateHeader(translation, nameof(typeDataGridViewTextBoxColumn), typeDataGridViewTextBoxColumn, "Change");
        TranslateHeader(translation, nameof(File), File, "Type");
    }

    public void LoadPatch(string patch)
    {
        PatchFileNameEdit.Text = patch;
        LoadPatchFile();
    }

    private void GridChangedFiles_SelectionChanged(object? sender, EventArgs e)
    {
        if (GridChangedFiles.SelectedItem is not Patch patch)
        {
            return;
        }

        ChangesList.ViewFixedPatch(patch.FileNameB, patch.Text ?? string.Empty);
    }

    private void BrowsePatch_Click(object? sender, EventArgs e)
    {
        this.InvokeAndForget(BrowsePatchAsync);
    }

    private async Task BrowsePatchAsync()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return;
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = _patchFileFilterTitle.Text,
                FileTypeFilter =
                [
                    new FilePickerFileType(_patchFileFilterString.Text)
                    {
                        Patterns = ["*.patch"],
                    },
                ],
            });
        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrEmpty(path))
        {
            PatchFileNameEdit.Text = path;
        }

        LoadPatchFile();
    }

    private void LoadPatchFile()
    {
        try
        {
            string text = System.IO.File.ReadAllText(PatchFileNameEdit.Text ?? string.Empty, GitModule.LosslessEncoding);
            LoadPatchText(text);
        }
        catch
        {
        }
    }

    private void LoadPatchText(string text)
    {
        List<Patch> patches = [.. PatchProcessor.CreatePatchesFromString(text, new Lazy<Encoding>(() => Module.FilesEncoding))];
        _patches = patches;
        _sortColumn = null;
        _sortAscending = true;
        SetPatchItems(patches, selectedPatch: null);
    }

    private void Header_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        string column = ReferenceEquals(sender, FileNameA)
            ? nameof(Patch.FileNameA)
            : ReferenceEquals(sender, typeDataGridViewTextBoxColumn)
                ? nameof(Patch.ChangeType)
                : nameof(Patch.FileType);
        _sortAscending = _sortColumn != column || !_sortAscending;
        _sortColumn = column;

        Patch? selectedPatch = GridChangedFiles.SelectedItem as Patch;
        IEnumerable<Patch> ordered = column switch
        {
            nameof(Patch.ChangeType) => OrderBy(_patches, patch => patch.ChangeType.ToString()),
            nameof(Patch.FileType) => OrderBy(_patches, patch => patch.FileType.ToString()),
            _ => OrderBy(_patches, patch => patch.FileNameA),
        };
        SetPatchItems(ordered.ToArray(), selectedPatch);
        e.Handled = true;
    }

    private IEnumerable<Patch> OrderBy(IEnumerable<Patch> patches, Func<Patch, string?> selector)
        => _sortAscending
            ? patches.OrderBy(selector, StringComparer.Ordinal)
            : patches.OrderByDescending(selector, StringComparer.Ordinal);

    private void SetPatchItems(IReadOnlyList<Patch> patches, Patch? selectedPatch)
    {
        GridChangedFiles.ItemsSource = patches;
        GridChangedFiles.SelectedItem = selectedPatch ?? patches.FirstOrDefault();
    }

    private static void AddHeaderTranslationItem(ITranslation translation, string fieldName, string text)
    {
        translation.AddTranslationItem(nameof(FormViewPatch), fieldName, "HeaderText", text);
    }

    private static void TranslateHeader(ITranslation translation, string fieldName, Border header, string defaultText)
    {
        string? text = translation.TranslateItem(nameof(FormViewPatch), fieldName, "HeaderText", () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FormViewPatch _form;

        public TestAccessor(FormViewPatch form)
        {
            _form = form;
        }

        public IReadOnlyList<Patch> Patches => _form._patches;

        public void LoadPatchText(string text) => _form.LoadPatchText(text);
    }

    private sealed class PatchRow : Grid
    {
        public PatchRow()
        {
            ColumnDefinitions = new ColumnDefinitions("*,70,50");
            MinWidth = 300;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            Children.Clear();
            if (DataContext is not Patch patch)
            {
                return;
            }

            AddCell(patch.FileNameA, 0);
            AddCell(patch.ChangeType.ToString(), 1);
            AddCell(patch.FileType.ToString(), 2);
        }

        private void AddCell(string? text, int column)
        {
            TextBlock textBlock = new()
            {
                Text = text ?? string.Empty,
                Margin = new Avalonia.Thickness(6, 3),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            SetColumn(textBlock, column);
            Children.Add(textBlock);
        }
    }
}
