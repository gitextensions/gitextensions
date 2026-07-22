using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using GitCommands.Git;
using GitCommands.Git.Tag;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.Compat;
using GitUI.HelperDialogs;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormVerify.cs. Avalonia's header-plus-ListBox grid keeps
// the original sortable columns and selection checkboxes without introducing a view model.
public sealed partial class FormVerify : GitModuleForm
{
    private const string _restoredObjectsTagPrefix = "LOST_FOUND_";
    private const string _commitColumns = "28,80,100,*,150,92,92";
    private const string _objectColumns = "28,80,*,0,0,150,0";

    private readonly TranslationString _removeDanglingObjectsCaption = new("Remove");
    private readonly TranslationString _removeDanglingObjectsQuestion = new("Are you sure you want to delete all dangling objects?");
    private readonly TranslationString _xTagsCreated = new("{0} Tags created." + Environment.NewLine + Environment.NewLine + "Do not forget to delete these tags when finished.");
    private readonly TranslationString _selectLostObjectsToRestoreMessage = new("Select objects to restore.");
    private readonly TranslationString _selectLostObjectsToRestoreCaption = new("Restore lost objects");
    private readonly TranslationString _seemingly = new("seemingly");

    private readonly List<LostObject> _lostObjects = [];
    private readonly List<LostObject> _filteredLostObjects = [];
    private readonly HashSet<ObjectId> _selectedObjectIds = [];
    private readonly CancellationTokenSequence _typeDetectionSequence = new();
    private readonly TaskManager _loadOperations = ThreadHelper.CreateTaskManager();
    private readonly IGitTagController? _gitTagController;

    private LostObject? _previewedItem;
    private string? _defaultFilename;
    private string? _sortColumn;
    private bool _sortAscending;
    private bool _typeDetected;
    private bool _updatingSelectionHeader;

    // https://en.wikipedia.org/wiki/List_of_file_signatures
    private static readonly Dictionary<string, string> _languagesStartOfFile = new()
    {
        { @"{\rtf", "rtf" },
        { "{", "json" },
        { "#include", "cpp" },
        { "import {", "js" },
        { "import * as", "js" },
        { "import \"", "js" },
        { "export ", "js" },
        { "import ", "java" },
        { "from", "py" },
        { "package", "go" },
        { "namespace ", "fs" },
        { "#!", "sh" },
        { "[", "ini" },
        { "using ", "cs" },
        { "# ", "md" },
        { "##", "md" },
        { "<!doctype html", "html" },
        { "<html", "html" },
        { "<?xml", "xml" },
        { "use ", "rs" },
        { "%PDF", "pdf" },
        { "PK", "zip" },
        { "MZ", "exe" },
        { @"\document", "tex" },
        { "\u0089PNG", "png" },
        { "ÿØÿQ", "jp2" },
        { "ÿØÿ", "jpg" },
        { "ÿ\x0A", "jxl" },
        { "RIFF", "webp" },
        { "<svg", "svg" },
        { "BM", "bmp" },
        { "7z", "7z" },
        { "GIF", "gif" },
        { "ÐÏ\x11à¡±\x1Aá", "doc" },
        { "qoif", "qoi" },
        { "Rar!", "rar" },
        { "%!PS", "ps" },
        { "OggS", "ogg" },
        { "8BPS", "psf" },
        { "ID3", "mp3" },
        { "CD001", "iso" },
        { "fLaC", "flac" },
        { "FLIF", "flif" },
        { "␚Eß£", "mkv" },
        { "<", "xml" },
    };

    private static readonly Dictionary<string, string[]> _fileTypesEquivalences = new()
    {
        { "js", ["ts", "jsx", "tsx"] },
        { "html", ["php", "cshtml"] },
        { "cpp", ["c"] },
        { "xml", ["config", "settings", "csproj", "xlf", "props"] },
        { "zip", ["docx", "xlsx", "pptx", "odt", "ods", "odp", "epub", "jar", "msix"] },
        { "exe", ["dll"] },
        { "doc", ["xls", "ppt", "msi"] },
        { "md", ["sh", "yml"] },
        { "txt", ["csv", "css", "md", "yml"] },
    };

    public FormVerify()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormVerify(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        WireControls();
        fileViewer.UICommandsSource = this;
        _gitTagController = new GitTagController(commands);
        InitializeComplete();
    }

    private LostObject? CurrentItem => Warnings.SelectedItem as LostObject;

    private void WireControls()
    {
        Warnings.ItemTemplate = new FuncDataTemplate<LostObject>(CreateLostObjectRow, supportsRecycling: false);
        Warnings.SelectionChanged += Warnings_SelectionChanged;
        Warnings.DoubleTapped += (_, _) => ViewCurrentItem();
        Warnings.KeyDown += Warnings_KeyDown;
        Warnings.AddHandler(PointerPressedEvent, Warnings_PointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        mnuLostObjects.Opening += (_, _) => UpdateContextMenuItems();
        mnuLostObjectView.Click += (_, _) => ViewCurrentItem();
        mnuLostObjectsCreateTag.Click += mnuLostObjectsCreateTag_Click;
        mnuLostObjectsCreateBranch.Click += mnuLostObjectsCreateBranch_Click;
        copyHashToolStripMenuItem.Click += copyHashToolStripMenuItem_Click;
        copyParentHashToolStripMenuItem.Click += copyParentHashToolStripMenuItem_Click;
        saveAsToolStripMenuItem.Click += (_, _) => SaveCurrentBlob();
        ShowCommitsAndTags.IsCheckedChanged += ShowCommitsCheckedChanged;
        ShowOtherObjects.IsCheckedChanged += ShowOtherObjects_CheckedChanged;
        NoReflogs.IsCheckedChanged += (_, _) => ReloadForOptionChange();
        FullCheck.IsCheckedChanged += (_, _) => ReloadForOptionChange();
        Unreachable.IsCheckedChanged += (_, _) => ReloadForOptionChange();
        columnIsLostObjectSelected.IsCheckedChanged += SelectionHeader_CheckedChanged;
        columnDate.PointerReleased += (_, e) => SortBy(nameof(LostObject.Date), e);
        columnType.PointerReleased += (_, e) => SortBy(nameof(LostObject.RawType), e);
        columnSubject.PointerReleased += (_, e) => SortBy(nameof(LostObject.Subject), e);
        columnAuthor.PointerReleased += (_, e) => SortBy(nameof(LostObject.Author), e);
        columnHash.PointerReleased += (_, e) => SortBy(nameof(LostObject.ObjectId), e);
        columnParent.PointerReleased += (_, e) => SortBy(nameof(LostObject.Parent), e);
        SaveObjects.Click += SaveObjectsClick;
        Remove.Click += RemoveClick;
        DeleteAllLostAndFoundTags.Click += DeleteAllLostAndFoundTagsClick;
        btnRestoreSelectedObjects.Click += btnRestoreSelectedObjects_Click;
        btnCloseDialog.Click += (_, _) => DialogResult = WinFormsShims.DialogResult.Cancel;
        UpdateContextMenuItems();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        if (TryGetUICommands(out _))
        {
            UpdateLostObjects();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _typeDetectionSequence.CancelCurrent();
        _loadOperations.JoinPendingOperations();
        _typeDetectionSequence.Dispose();
        base.OnClosed(e);
    }

    private Control CreateLostObjectRow(LostObject? lostObject, INameScope? nameScope)
    {
        Grid row = new() { ColumnDefinitions = new ColumnDefinitions(GetColumnLayout()) };
        if (lostObject is null)
        {
            return row;
        }

        CheckBox selected = new()
        {
            IsChecked = _selectedObjectIds.Contains(lostObject.ObjectId),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        selected.IsCheckedChanged += (_, _) => SetObjectSelected(lostObject, selected.IsChecked == true);
        row.Children.Add(selected);
        row.Children.Add(CreateCell(lostObject.Date?.ToString("g") ?? string.Empty, 1));
        row.Children.Add(CreateCell(lostObject.RawType, 2));
        row.Children.Add(CreateCell(lostObject.Subject ?? string.Empty, 3));
        row.Children.Add(CreateCell(lostObject.Author ?? string.Empty, 4));
        row.Children.Add(CreateCell(lostObject.ObjectId.ToString(), 5, monospace: true, wrap: true));
        row.Children.Add(CreateCell(lostObject.Parent.IsZero ? string.Empty : lostObject.Parent.ToString(), 6, monospace: true, wrap: true));
        return row;

        static TextBlock CreateCell(string text, int column, bool monospace = false, bool wrap = false)
        {
            TextBlock cell = new()
            {
                Text = text,
                Margin = new Thickness(6, 2),
                TextTrimming = wrap ? TextTrimming.None : TextTrimming.CharacterEllipsis,
                TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
                VerticalAlignment = VerticalAlignment.Center,
                FontFamily = monospace ? new FontFamily("monospace") : FontFamily.Default,
            };
            Grid.SetColumn(cell, column);
            return cell;
        }
    }

    private void SetObjectSelected(LostObject lostObject, bool selected)
    {
        if (selected)
        {
            _selectedObjectIds.Add(lostObject.ObjectId);
        }
        else
        {
            _selectedObjectIds.Remove(lostObject.ObjectId);
        }

        UpdateSelectionHeader();
    }

    private void SelectionHeader_CheckedChanged(object? sender, EventArgs e)
    {
        if (_updatingSelectionHeader || columnIsLostObjectSelected.IsChecked is not bool selected)
        {
            return;
        }

        if (selected)
        {
            _selectedObjectIds.UnionWith(_filteredLostObjects.Select(item => item.ObjectId));
        }
        else
        {
            _selectedObjectIds.ExceptWith(_filteredLostObjects.Select(item => item.ObjectId));
        }

        RefreshRows(CurrentItem);
    }

    private void UpdateSelectionHeader()
    {
        _updatingSelectionHeader = true;
        int selectedCount = _filteredLostObjects.Count(item => _selectedObjectIds.Contains(item.ObjectId));
        columnIsLostObjectSelected.IsChecked = selectedCount == 0
            ? false
            : selectedCount == _filteredLostObjects.Count
                ? true
                : null;
        _updatingSelectionHeader = false;
    }

    private void SaveObjectsClick(object? sender, EventArgs e)
    {
        FormProcess.ShowDialog(this, UICommands, $"fsck-objects --lost-found{GetOptions()}", Module.WorkingDir, input: null, useDialogSettings: true);
        UpdateLostObjects();
    }

    private void RemoveClick(object? sender, EventArgs e)
    {
        if (MessageBoxes.Show(
                this,
                _removeDanglingObjectsQuestion.Text,
                _removeDanglingObjectsCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Question) != WinFormsShims.DialogResult.Yes)
        {
            return;
        }

        FormProcess.ShowDialog(this, UICommands, "prune", Module.WorkingDir, input: null, useDialogSettings: true);
        UpdateLostObjects();
    }

    private void mnuLostObjectsCreateTag_Click(object? sender, EventArgs e)
    {
        if (CurrentItem is not { ObjectType: LostObjectType.Commit } currentItem)
        {
            return;
        }

        using FormCreateTag form = new(UICommands, currentItem.ObjectId);
        if (form.ShowDialog(this) == WinFormsShims.DialogResult.OK)
        {
            UpdateLostObjects();
        }
    }

    private void mnuLostObjectsCreateBranch_Click(object? sender, EventArgs e)
    {
        if (CurrentItem is not { ObjectType: LostObjectType.Commit } currentItem)
        {
            return;
        }

        using FormCreateBranch form = new(UICommands, currentItem.ObjectId);
        if (form.ShowDialog(this) == WinFormsShims.DialogResult.OK)
        {
            UpdateLostObjects();
        }
    }

    private void DeleteAllLostAndFoundTagsClick(object? sender, EventArgs e)
    {
        DeleteLostFoundTags();
        UpdateLostObjects();
    }

    private void btnRestoreSelectedObjects_Click(object? sender, EventArgs e)
    {
        DeleteLostFoundTags();
        int restoredObjectsCount = CreateLostFoundTags();
        if (restoredObjectsCount == 0)
        {
            return;
        }

        MessageBoxes.Show(
            this,
            string.Format(_xTagsCreated.Text, restoredObjectsCount),
            "Tags created",
            WinFormsShims.MessageBoxButtons.OK,
            WinFormsShims.MessageBoxIcon.Information);

        if (restoredObjectsCount == _filteredLostObjects.Count)
        {
            DialogResult = WinFormsShims.DialogResult.OK;
            return;
        }

        UpdateLostObjects();
    }

    private void ReloadForOptionChange()
    {
        if (TryGetUICommands(out _))
        {
            UpdateLostObjects();
        }
    }

    private void ShowCommitsCheckedChanged(object? sender, EventArgs e)
    {
        if (ShowCommitsAndTags.IsChecked != true && ShowOtherObjects.IsChecked != true)
        {
            ShowOtherObjects.IsChecked = true;
        }
        else
        {
            UpdateFilteredLostObjects();
        }
    }

    private void ShowOtherObjects_CheckedChanged(object? sender, EventArgs e)
    {
        if (ShowCommitsAndTags.IsChecked != true && ShowOtherObjects.IsChecked != true)
        {
            ShowCommitsAndTags.IsChecked = true;
        }
        else
        {
            UpdateFilteredLostObjects();
        }
    }

    private void Warnings_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(Warnings);
        if (!point.Properties.IsRightButtonPressed)
        {
            return;
        }

        ListBoxItem? item = (e.Source as Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true);
        if (item?.DataContext is LostObject lostObject)
        {
            Warnings.SelectedItem = lostObject;
        }
    }

    private void Warnings_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ViewCurrentItem();
            e.Handled = true;
        }
    }

    private void Warnings_SelectionChanged(object? sender, EventArgs e)
    {
        _defaultFilename = null;
        if (CurrentItem is null || ReferenceEquals(_previewedItem, CurrentItem) || !TryGetUICommands(out _))
        {
            UpdateContextMenuItems();
            return;
        }

        _previewedItem = CurrentItem;
        string content = GetLostObjectContent(_previewedItem);
        if (_previewedItem.ObjectType is LostObjectType.Commit or LostObjectType.Tag)
        {
            _defaultFilename = "commit.patch";
            fileViewer.ViewFixedPatch(_defaultFilename, content);
        }
        else if (_previewedItem.ObjectType == LostObjectType.Blob)
        {
            _defaultFilename = GuessFileNameWithContent(content, _previewedItem.ObjectId.ToString());
            fileViewer.ViewText(_defaultFilename, content);
        }
        else
        {
            _defaultFilename = "file.txt";
            fileViewer.ViewText(_defaultFilename, content);
        }

        UpdateContextMenuItems();
    }

    private static string GuessFileTypeWithContent(string content)
    {
        foreach (KeyValuePair<string, string> pair in _languagesStartOfFile)
        {
            if (content.StartsWith(pair.Key, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Value;
            }
        }

        return "txt";
    }

    private static string GuessFileNameWithContent(string content, string hash)
        => $"LOST_FOUND_{hash}.{GuessFileTypeWithContent(content)}";

    private void UpdateLostObjects()
    {
        using (WaitCursorScope.Enter())
        {
            string cmdOutput = FormProcess.ReadDialog(this, UICommands, $"fsck-objects{GetOptions()}", Module.WorkingDir, input: null, useDialogSettings: true);
            if (FormProcess.IsOperationAborted(cmdOutput))
            {
                DialogResult = WinFormsShims.DialogResult.Abort;
                return;
            }

            _typeDetectionSequence.CancelCurrent();
            _lostObjects.Clear();
            _lostObjects.AddRange(ParseLostObjects(Module, cmdOutput));
            _selectedObjectIds.IntersectWith(_lostObjects.Select(item => item.ObjectId));
            _previewedItem = null;
            _typeDetected = false;
            _sortColumn = nameof(LostObject.Date);
            _sortAscending = false;
            UpdateFilteredLostObjects();
        }
    }

    private static IReadOnlyList<LostObject> ParseLostObjects(IGitModule module, string cmdOutput)
    {
        List<LostObject> lostObjects =
        [
            .. cmdOutput
                .Split(Delimiters.LineFeedAndCarriageReturn)
                .Where(line => !string.IsNullOrEmpty(line))
                .Select(line => LostObject.TryParse(module, line))
                .WhereNotNull(),
        ];

        LostObject[] commits = [.. lostObjects.Where(item => item.ObjectType == LostObjectType.Commit)];
        List<string> metadata = new(commits.Length);
        int batchSize = 30_000 / (ObjectId.Sha1CharCount + 1);
        for (int currentBatch = 0; currentBatch * batchSize < commits.Length; currentBatch++)
        {
            LostObject[] nextBatch = [.. commits.Skip(currentBatch * batchSize).Take(batchSize)];
            metadata.AddRange(LostObject.GetCommitsMetadata(module, nextBatch.Select(item => item.ObjectId.ToString())));
        }

        for (int i = 0; i < commits.Length && i < metadata.Count; i++)
        {
            commits[i].FillCommitData(module, metadata[i]);
        }

        return [.. lostObjects.OrderByDescending(item => item.Date)];
    }

    private void UpdateFilteredLostObjects()
    {
        LostObject? selectedItem = CurrentItem;
        _filteredLostObjects.Clear();
        _filteredLostObjects.AddRange(_lostObjects.Where(IsMatchToFilter));
        ApplySort();
        UpdateColumnLayout();
        RefreshRows(selectedItem);
        DetectBlobTypes();
    }

    private void DetectBlobTypes()
    {
        if (_typeDetected || ShowOtherObjects.IsChecked != true || !TryGetUICommands(out _))
        {
            return;
        }

        _typeDetected = true;
        CancellationToken cancellationToken = _typeDetectionSequence.Next();
        LostObject[] blobs = [.. _lostObjects.Where(item => item.ObjectType == LostObjectType.Blob)];
        _loadOperations.FileAndForget(async () =>
        {
            await Task.Run(() =>
            {
                foreach (LostObject item in blobs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    string content = GetLostObjectContent(item);
                    item.RawType += $" ({_seemingly.Text}: {GuessFileTypeWithContent(content)})";
                }
            }, cancellationToken);

            await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            RefreshRows(CurrentItem);
        });
    }

    private bool IsMatchToFilter(LostObject lostObject)
        => (ShowCommitsAndTags.IsChecked == true && lostObject.ObjectType is LostObjectType.Commit or LostObjectType.Tag)
            || (ShowOtherObjects.IsChecked == true && lostObject.ObjectType is not LostObjectType.Commit and not LostObjectType.Tag);

    private string GetLostObjectContent(LostObject item)
        => Module.ShowObject(item.ObjectId, returnRaw: item.ObjectType == LostObjectType.Blob) ?? string.Empty;

    private string GetOptions()
    {
        string options = string.Empty;
        if (Unreachable.IsChecked == true)
        {
            options += " --unreachable";
        }

        if (FullCheck.IsChecked == true)
        {
            options += " --full";
        }

        if (NoReflogs.IsChecked == true)
        {
            options += " --no-reflogs";
        }

        return options;
    }

    private void ViewCurrentItem()
    {
        if (CurrentItem is null)
        {
            return;
        }

        if (!ReferenceEquals(_previewedItem, CurrentItem))
        {
            Warnings_SelectionChanged(this, EventArgs.Empty);
        }

        fileViewer.Focus();
    }

    private int CreateLostFoundTags()
    {
        List<LostObject> selectedLostObjects =
        [
            .. _filteredLostObjects.Where(item => _selectedObjectIds.Contains(item.ObjectId)),
        ];
        if (selectedLostObjects.Count == 0)
        {
            MessageBoxes.Show(
                this,
                _selectLostObjectsToRestoreMessage.Text,
                _selectLostObjectsToRestoreCaption.Text,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Warning);
            return 0;
        }

        int currentTag = 0;
        foreach (LostObject lostObject in selectedLostObjects)
        {
            currentTag++;
            string tagName = lostObject.ObjectType == LostObjectType.Tag ? lostObject.TagName! : currentTag.ToString();
            _gitTagController!.CreateTag(new GitCreateTagArgs($"{_restoredObjectsTagPrefix}{tagName}", lostObject.ObjectId), this);
        }

        return currentTag;
    }

    private void DeleteLostFoundTags()
    {
        foreach (IGitRef head in Module.GetRefs(RefsFilter.Tags))
        {
            if (head.Name.StartsWith(_restoredObjectsTagPrefix, StringComparison.Ordinal))
            {
                Module.DeleteTag(head.Name);
            }
        }
    }

    private void UpdateContextMenuItems()
    {
        LostObject? lostObject = CurrentItem;
        bool hasSelection = lostObject is not null;
        bool isCommit = lostObject?.ObjectType == LostObjectType.Commit;
        bool isBlob = lostObject?.ObjectType == LostObjectType.Blob;
        mnuLostObjectView.IsEnabled = hasSelection;
        mnuLostObjectsCreateTag.IsEnabled = isCommit;
        mnuLostObjectsCreateBranch.IsEnabled = isCommit;
        copyHashToolStripMenuItem.IsEnabled = hasSelection;
        copyParentHashToolStripMenuItem.IsEnabled = isCommit && lostObject is not null && !lostObject.Parent.IsZero;
        saveAsToolStripMenuItem.IsEnabled = isBlob;
    }

    private void copyHashToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (CurrentItem is LostObject lostObject)
        {
            ClipboardUtil.TrySetText(lostObject.ObjectId.ToString());
        }
    }

    private void copyParentHashToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (CurrentItem is LostObject { Parent.IsZero: false } lostObject)
        {
            ClipboardUtil.TrySetText(lostObject.Parent.ToString());
        }
    }

    private void SaveCurrentBlob()
    {
        if (CurrentItem is not { ObjectType: LostObjectType.Blob } lostObject)
        {
            return;
        }

        string fileName = _defaultFilename ?? lostObject.ObjectId + "_LOST_FOUND.txt";
        string extension = Path.GetExtension(fileName).TrimStart('.');
        List<FilePickerFileType> fileTypes = [CreateFileType(extension)];
        if (_fileTypesEquivalences.TryGetValue(extension, out string[]? equivalentTypes))
        {
            fileTypes.AddRange(equivalentTypes.Select(CreateFileType));
        }

        fileTypes.Add(FilePickerFileTypes.All);
        IStorageFile? file = DispatcherPump.Wait(() => StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            SuggestedFileName = fileName,
            DefaultExtension = extension,
            FileTypeChoices = fileTypes,
        }));
        string? targetPath = file?.TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(targetPath))
        {
            ThreadHelper.JoinableTaskFactory.Run(
                () => Module.SaveBlobAsAsync(targetPath, lostObject.ObjectId.ToString(), CancellationToken.None));
        }

        return;

        static FilePickerFileType CreateFileType(string type)
            => new($"{type} Files") { Patterns = [$"*.{type}"] };
    }

    private void SortBy(string column, PointerReleasedEventArgs e)
    {
        _sortAscending = _sortColumn != column || !_sortAscending;
        _sortColumn = column;
        LostObject? selectedItem = CurrentItem;
        ApplySort();
        RefreshRows(selectedItem);
        e.Handled = true;
    }

    private void ApplySort()
    {
        if (_sortColumn is null)
        {
            return;
        }

        Comparison<LostObject> comparison = _sortColumn switch
        {
            nameof(LostObject.Date) => (left, right) => Nullable.Compare(left.Date, right.Date),
            nameof(LostObject.RawType) => (left, right) => string.Compare(left.RawType, right.RawType, StringComparison.Ordinal),
            nameof(LostObject.Subject) => (left, right) => string.Compare(left.Subject, right.Subject, StringComparison.CurrentCulture),
            nameof(LostObject.Author) => (left, right) => string.Compare(left.Author, right.Author, StringComparison.CurrentCulture),
            nameof(LostObject.ObjectId) => (left, right) => left.ObjectId.CompareTo(right.ObjectId),
            _ => (left, right) => left.Parent.CompareTo(right.Parent),
        };
        _filteredLostObjects.Sort((left, right) => _sortAscending ? comparison(left, right) : comparison(right, left));
    }

    private void RefreshRows(LostObject? selectedItem)
    {
        Warnings.ItemsSource = null;
        Warnings.ItemsSource = _filteredLostObjects.ToArray();
        Warnings.SelectedItem = selectedItem is not null && _filteredLostObjects.Contains(selectedItem)
            ? selectedItem
            : _filteredLostObjects.FirstOrDefault();
        UpdateSelectionHeader();
        UpdateContextMenuItems();
    }

    private void UpdateColumnLayout()
    {
        bool showCommitColumns = ShowCommitsAndTags.IsChecked == true;
        ((Grid)columnDate.Parent!).ColumnDefinitions = new ColumnDefinitions(GetColumnLayout());
        columnSubject.IsVisible = showCommitColumns;
        columnAuthor.IsVisible = showCommitColumns;
        columnParent.IsVisible = showCommitColumns;
    }

    private string GetColumnLayout() => ShowCommitsAndTags.IsChecked == true ? _commitColumns : _objectColumns;

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddHeaderTranslationItem(translation, nameof(columnDate), "Date");
        AddHeaderTranslationItem(translation, nameof(columnType), "Type");
        AddHeaderTranslationItem(translation, nameof(columnSubject), "Subject");
        AddHeaderTranslationItem(translation, nameof(columnAuthor), "Author");
        AddHeaderTranslationItem(translation, nameof(columnHash), "Hash");
        AddHeaderTranslationItem(translation, nameof(columnParent), "Parent(s) hashs");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, columnDate, nameof(columnDate), "Date");
        TranslateHeader(translation, columnType, nameof(columnType), "Type");
        TranslateHeader(translation, columnSubject, nameof(columnSubject), "Subject");
        TranslateHeader(translation, columnAuthor, nameof(columnAuthor), "Author");
        TranslateHeader(translation, columnHash, nameof(columnHash), "Hash");
        TranslateHeader(translation, columnParent, nameof(columnParent), "Parent(s) hashs");
    }

    private static void AddHeaderTranslationItem(ITranslation translation, string fieldName, string text)
        => translation.AddTranslationItem(nameof(FormVerify), fieldName, "HeaderText", text);

    private static void TranslateHeader(ITranslation translation, Border header, string fieldName, string defaultText)
    {
        string? text = translation.TranslateItem(nameof(FormVerify), fieldName, "HeaderText", () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    private sealed partial class LostObject
    {
        public static LostObject CreatePreview(
            LostObjectType objectType,
            string rawType,
            ObjectId objectId,
            DateTime? date,
            string? subject,
            string? author,
            ObjectId parent = default)
        {
            LostObject item = new(objectType, rawType, objectId)
            {
                Date = date,
                Subject = subject,
                Author = author,
                Parent = parent,
            };
            return item;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormVerify form)
    {
        public Button Close => form.btnCloseDialog;
        public CheckBox FullCheck => form.FullCheck;
        public CheckBox NoReflogs => form.NoReflogs;
        public CheckBox ShowCommits => form.ShowCommitsAndTags;
        public CheckBox ShowOther => form.ShowOtherObjects;
        public CheckBox Unreachable => form.Unreachable;
        public ListBox Warnings => form.Warnings;
        public MenuItem CreateBranch => form.mnuLostObjectsCreateBranch;
        public MenuItem CreateTag => form.mnuLostObjectsCreateTag;
        public MenuItem SaveAs => form.saveAsToolStripMenuItem;
        public IReadOnlyList<string> ObjectIds => form._filteredLostObjects.Select(item => item.ObjectId.ToString()).ToArray();
        public IReadOnlyList<string> RawTypes => form._filteredLostObjects.Select(item => item.RawType).ToArray();
        public string Options => form.GetOptions();

        public static IReadOnlyList<(string RawType, string ObjectId, string? Subject)> Parse(IGitModule module, string output)
            => ParseLostObjects(module, output)
                .Select(item => (item.RawType, item.ObjectId.ToString(), item.Subject))
                .ToArray();

        public void SelectAll(bool selected) => form.columnIsLostObjectSelected.IsChecked = selected;

        public int SelectedCount => form._selectedObjectIds.Count;

        public void SetPreviewRows()
        {
            LostObject first = LostObject.CreatePreview(
                LostObjectType.Commit,
                "dangling commit",
                ObjectId.Parse("1111111111111111111111111111111111111111"),
                new DateTime(2026, 7, 21, 10, 30, 0),
                "Recover the lost feature work",
                "Ada Developer",
                ObjectId.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
            LostObject second = LostObject.CreatePreview(
                LostObjectType.Commit,
                "unreachable commit",
                ObjectId.Parse("2222222222222222222222222222222222222222"),
                new DateTime(2026, 7, 19, 9, 15, 0),
                "Temporary experiment",
                "Grace Contributor");
            LostObject blob = LostObject.CreatePreview(
                LostObjectType.Blob,
                "dangling blob (seemingly: cs)",
                ObjectId.Parse("3333333333333333333333333333333333333333"),
                new DateTime(2026, 7, 18, 8, 0, 0),
                null,
                null);
            form._lostObjects.Clear();
            form._lostObjects.AddRange([first, second, blob]);
            form._sortColumn = nameof(LostObject.Date);
            form._sortAscending = false;
            form.UpdateFilteredLostObjects();
            form.fileViewer.ViewFixedPatch(
                "commit.patch",
                "commit 1111111111111111111111111111111111111111\nAuthor: Ada Developer\n\n    Recover the lost feature work\n\ndiff --git a/recovered.cs b/recovered.cs\n--- a/recovered.cs\n+++ b/recovered.cs\n@@ -1 +1 @@\n-old content\n+recovered content\n");
        }

        public void SortByType()
        {
            form._sortColumn = nameof(LostObject.RawType);
            form._sortAscending = true;
            form.ApplySort();
            form.RefreshRows(form.CurrentItem);
        }
    }
}
