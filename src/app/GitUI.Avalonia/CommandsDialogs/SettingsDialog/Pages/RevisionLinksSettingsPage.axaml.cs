using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitCommands;
using GitCommands.ExternalLinks;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI.CommandsDialogs.SettingsDialog.RevisionLinks;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

// Twin of GitUI/CommandsDialogs/SettingsDialog/Pages/RevisionLinksSettingsPage.cs.
// The editable DataGridView is represented by a typed ListBox whose row text boxes update
// the original ExternalLinkFormat objects directly.
public sealed partial class RevisionLinksSettingsPage : DistributedSettingsPage
{
    private readonly TranslationString _addTemplate = new("Add {0} templates");
    private ExternalLinksManager? _externalLinksManager;

    // Avalonia's designer constructs views before the application initializes ThreadHelper.
    private readonly TaskManager _templateOperations = GitUI.Compat.DesignTimeTaskManager.Create();
    private readonly MenuFlyout _templateFlyout;
    private CancellationTokenSource _lifetimeCancellationTokenSource = new();

    public RevisionLinksSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public RevisionLinksSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        _templateFlyout = (MenuFlyout?)Add.Flyout
            ?? throw new InvalidOperationException("The template flyout was not created.");
        ConfigureLists();
        WireEvents();
        LoadTemplatesInMenu();
        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        LoadDefinitions(CurrentSettings ?? AppSettings.SettingsContainer);
        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        ApplyTextFields();
        Validates.NotNull(_externalLinksManager);
        _externalLinksManager.Save();

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(RevisionLinksSettingsPage));

    private void LoadDefinitions(DistributedSettings settings)
    {
        _externalLinksManager = new ExternalLinksManager(settings);

        ReloadCategories();
        if (_NO_TRANSLATE_Categories.ItemCount > 0)
        {
            _NO_TRANSLATE_Categories.SelectedIndex = 0;
        }

        CategoryChanged();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_lifetimeCancellationTokenSource.IsCancellationRequested)
        {
            _lifetimeCancellationTokenSource.Dispose();
            _lifetimeCancellationTokenSource = new CancellationTokenSource();
        }

        Add.IsEnabled = true;
        base.OnAttachedToVisualTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _lifetimeCancellationTokenSource.Cancel();
        _templateOperations.JoinPendingOperations();
        base.OnDetachedFromVisualTree(e);
    }

    private void ConfigureLists()
    {
        _NO_TRANSLATE_Categories.ItemTemplate = new FuncDataTemplate<ExternalLinkDefinition>(
            (definition, _) => new TextBlock
            {
                Text = definition?.Name ?? string.Empty,
                TextTrimming = TextTrimming.CharacterEllipsis,
            },
            supportsRecycling: true);
        LinksGrid.ItemTemplate = new FuncDataTemplate<ExternalLinkFormat>(
            CreateLinkFormatRow,
            supportsRecycling: false);
    }

    private void WireEvents()
    {
        _NO_TRANSLATE_Categories.SelectionChanged += _NO_TRANSLATE_Categories_SelectedIndexChanged;
        Add.Click += Add_Click;
        Remove.Click += Remove_Click;
        EnabledChx.IsCheckedChanged += EnabledChx_CheckedChanged;
        MessageChx.IsCheckedChanged += MessageChx_CheckedChanged;
        LocalBranchChx.IsCheckedChanged += LocalBranchChx_CheckedChanged;
        RemoteBranchChx.IsCheckedChanged += RemoteBranchChx_CheckedChanged;
        chxURL.IsCheckedChanged += chxURL_CheckedChanged;
        chxPushURL.IsCheckedChanged += chxPushURL_CheckedChanged;
        chkOnlyFirstRemote.IsCheckedChanged += chkOnlyFirstRemote_CheckedChanged;
        _NO_TRANSLATE_Name.LostFocus += _NO_TRANSLATE_Name_Leave;
        _NO_TRANSLATE_SearchPatternEdit.LostFocus += _NO_TRANSLATE_SearchPatternEdit_Leave;
        _NO_TRANSLATE_NestedPatternEdit.LostFocus += _NO_TRANSLATE_NestedPatternEdit_Leave;
        _NO_TRANSLATE_RemotePatern.LostFocus += _NO_TRANSLATE_RemotePatern_Leave;
        _NO_TRANSLATE_UseRemotes.LostFocus += _NO_TRANSLATE_UseRemotes_Leave;
        _NO_TRANSLATE_AddLink.Click += AddLink_Click;
        _NO_TRANSLATE_RemoveLink.Click += RemoveLink_Click;
        LinksGrid.KeyDown += LinksGrid_KeyDown;
    }

    private Control CreateLinkFormatRow(ExternalLinkFormat? format, INameScope? nameScope)
    {
        Grid row = new()
        {
            ColumnDefinitions = new ColumnDefinitions("150,*"),
            ColumnSpacing = 4,
        };
        if (format is null)
        {
            return row;
        }

        TextBox caption = new()
        {
            Text = format.Caption ?? string.Empty,
            MinWidth = 100,
        };
        TextBox uri = new()
        {
            Text = format.Format ?? string.Empty,
            MinWidth = 160,
        };
        Grid.SetColumn(uri, 1);
        caption.TextChanged += (_, _) => format.Caption = caption.Text;
        uri.TextChanged += (_, _) => format.Format = uri.Text;
        caption.GotFocus += (_, _) => LinksGrid.SelectedItem = format;
        uri.GotFocus += (_, _) => LinksGrid.SelectedItem = format;
        row.Children.Add(caption);
        row.Children.Add(uri);
        return row;
    }

    private void _NO_TRANSLATE_Categories_SelectedIndexChanged(object? sender, EventArgs e)
    {
        CategoryChanged();
    }

    private void ReloadCategories(ExternalLinkDefinition? selected = null)
    {
        Validates.NotNull(_externalLinksManager);
        IReadOnlyList<ExternalLinkDefinition> effectiveDefinitions =
            _externalLinksManager.GetEffectiveSettings();
        _NO_TRANSLATE_Categories.ItemsSource = effectiveDefinitions;
        _NO_TRANSLATE_Categories.SelectedItem = selected;
    }

    private ExternalLinkDefinition? SelectedLinkDefinition
        => _NO_TRANSLATE_Categories.SelectedItem as ExternalLinkDefinition;

    private void CategoryChanged()
    {
        ExternalLinkDefinition? selected = SelectedLinkDefinition;
        _NO_TRANSLATE_DetailPanel.IsEnabled = selected is not null;
        if (selected is null)
        {
            _NO_TRANSLATE_Name.Text = string.Empty;
            EnabledChx.IsChecked = false;
            MessageChx.IsChecked = false;
            LocalBranchChx.IsChecked = false;
            RemoteBranchChx.IsChecked = false;
            _NO_TRANSLATE_SearchPatternEdit.Text = string.Empty;
            _NO_TRANSLATE_NestedPatternEdit.Text = string.Empty;
            _NO_TRANSLATE_RemotePatern.Text = string.Empty;
            _NO_TRANSLATE_UseRemotes.Text = string.Empty;
            chkOnlyFirstRemote.IsChecked = false;
            chxURL.IsChecked = false;
            chxPushURL.IsChecked = false;
            LinksGrid.ItemsSource = null;
            return;
        }

        _NO_TRANSLATE_Name.Text = selected.Name ?? string.Empty;
        EnabledChx.IsChecked = selected.Enabled;
        MessageChx.IsChecked = selected.SearchInParts.Contains(
            ExternalLinkDefinition.RevisionPart.Message);
        LocalBranchChx.IsChecked = selected.SearchInParts.Contains(
            ExternalLinkDefinition.RevisionPart.LocalBranches);
        RemoteBranchChx.IsChecked = selected.SearchInParts.Contains(
            ExternalLinkDefinition.RevisionPart.RemoteBranches);
        _NO_TRANSLATE_SearchPatternEdit.Text = selected.SearchPattern ?? string.Empty;
        _NO_TRANSLATE_NestedPatternEdit.Text = selected.NestedSearchPattern ?? string.Empty;
        _NO_TRANSLATE_RemotePatern.Text = selected.RemoteSearchPattern ?? string.Empty;
        chxURL.IsChecked = selected.RemoteSearchInParts.Contains(
            ExternalLinkDefinition.RemotePart.URL);
        chxPushURL.IsChecked = selected.RemoteSearchInParts.Contains(
            ExternalLinkDefinition.RemotePart.PushURL);
        _NO_TRANSLATE_UseRemotes.Text = selected.UseRemotesPattern ?? string.Empty;
        chkOnlyFirstRemote.IsChecked = selected.UseOnlyFirstRemote;
        RefreshLinks();
    }

    private void Add_Click(object? sender, EventArgs e)
    {
        ExternalLinkDefinition definition = new()
        {
            Name = "<new>",
            Enabled = true,
            UseRemotesPattern = "upstream|origin",
            UseOnlyFirstRemote = true,
            SearchInParts = { ExternalLinkDefinition.RevisionPart.Message },
            RemoteSearchInParts = { ExternalLinkDefinition.RemotePart.URL },
        };
        Validates.NotNull(_externalLinksManager);
        _externalLinksManager.Add(definition);

        ReloadCategories(definition);
        CategoryChanged();
    }

    private void LoadTemplatesInMenu()
    {
        foreach (ICloudProviderExternalLinkDefinitionExtractor extractor
                 in new CloudProviderExternalLinkDefinitionExtractorFactory().GetAllExtractor())
        {
            MenuItem item = new()
            {
                Header = string.Format(_addTemplate.Text, extractor.ServiceName),
                Icon = new Image
                {
                    Width = 16,
                    Height = 16,
                    Source = extractor.Icon,
                },
                Tag = extractor,
            };
            item.Click += (_, _) => ExtractExternalLinkDefinitions(extractor);
            _templateFlyout.Items.Add(item);
        }
    }

    private static Remote FindRemoteByPreference(IList<Remote> remotes)
    {
        if (remotes?.Count is not > 0)
        {
            return default;
        }

        string[] remoteNames = ["upstream", "fork", "origin"];
        foreach (string remoteName in remoteNames)
        {
            Remote remoteFound = remotes.FirstOrDefault(remote => remote.Name == remoteName);
            if (remoteFound.Name is not null)
            {
                return remoteFound;
            }
        }

        return remotes[0];
    }

    private void ExtractExternalLinkDefinitions(
        ICloudProviderExternalLinkDefinitionExtractor extractor)
    {
        IGitModule module = Module
            ?? throw new InvalidOperationException("A repository is required to add revision-link templates.");
        ExtractExternalLinkDefinitions(extractor, module);
    }

    private void ExtractExternalLinkDefinitions(
        ICloudProviderExternalLinkDefinitionExtractor extractor,
        IGitModule module)
    {
        Validates.NotNull(_externalLinksManager);
        ExternalLinksManager manager = _externalLinksManager;
        CancellationToken cancellationToken = _lifetimeCancellationTokenSource.Token;
        Add.IsEnabled = false;
        _templateOperations.FileAndForget(async () =>
        {
            try
            {
                IReadOnlyList<Remote> remotes = await module.GetRemotesAsync()
                    .WaitAsync(cancellationToken);
                Remote selectedRemote = FindRemoteByPreference(
                    [.. remotes.Where(remote => extractor.IsValidRemoteUrl(remote.FetchUrl))]);
                IList<ExternalLinkDefinition> definitions =
                    extractor.GetDefinitions(selectedRemote.FetchUrl);
                cancellationToken.ThrowIfCancellationRequested();

                await _templateOperations.JoinableTaskFactory.SwitchToMainThreadAsync(
                    cancellationToken);
                manager.AddRange(definitions);
                ExternalLinkDefinition? selected = definitions.FirstOrDefault();
                ReloadCategories(selected);
                CategoryChanged();
            }
            finally
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await _templateOperations.JoinableTaskFactory.SwitchToMainThreadAsync();
                    Add.IsEnabled = true;
                }
            }
        });
    }

    private void Remove_Click(object? sender, EventArgs e)
    {
        ExternalLinkDefinition? selected = SelectedLinkDefinition;
        if (selected is null)
        {
            return;
        }

        Validates.NotNull(_externalLinksManager);
        int index = _NO_TRANSLATE_Categories.SelectedIndex;
        _externalLinksManager.Remove(selected);
        ReloadCategories();

        if (_NO_TRANSLATE_Categories.ItemCount > 0)
        {
            _NO_TRANSLATE_Categories.SelectedIndex = Math.Min(
                Math.Max(index, 0),
                _NO_TRANSLATE_Categories.ItemCount - 1);
        }

        CategoryChanged();
    }

    private void AddLink_Click(object? sender, EventArgs e)
    {
        if (SelectedLinkDefinition is not { } selected)
        {
            return;
        }

        ExternalLinkFormat format = new();
        selected.LinkFormats.Add(format);
        RefreshLinks(format);
    }

    private void RemoveLink_Click(object? sender, EventArgs e)
    {
        RemoveSelectedLink();
    }

    private void LinksGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            RemoveSelectedLink();
            e.Handled = true;
        }
    }

    private void RemoveSelectedLink()
    {
        if (SelectedLinkDefinition is not { } selected
            || LinksGrid.SelectedItem is not ExternalLinkFormat format)
        {
            return;
        }

        int index = LinksGrid.SelectedIndex;
        selected.LinkFormats.Remove(format);
        ExternalLinkFormat? next = selected.LinkFormats.Count == 0
            ? null
            : selected.LinkFormats[Math.Min(Math.Max(index, 0), selected.LinkFormats.Count - 1)];
        RefreshLinks(next);
    }

    private void RefreshLinks(ExternalLinkFormat? selected = null)
    {
        LinksGrid.ItemsSource = null;
        LinksGrid.ItemsSource = SelectedLinkDefinition?.LinkFormats;
        LinksGrid.SelectedItem = selected;
    }

    private void ApplyTextFields()
    {
        ExternalLinkDefinition? selected = SelectedLinkDefinition;
        if (selected is null)
        {
            return;
        }

        selected.Name = _NO_TRANSLATE_Name.Text;
        selected.SearchPattern = _NO_TRANSLATE_SearchPatternEdit.Text?.Trim();
        selected.NestedSearchPattern = _NO_TRANSLATE_NestedPatternEdit.Text?.Trim();
        selected.RemoteSearchPattern = _NO_TRANSLATE_RemotePatern.Text?.Trim();
        selected.UseRemotesPattern = _NO_TRANSLATE_UseRemotes.Text?.Trim();
    }

    private void _NO_TRANSLATE_Name_Leave(object? sender, EventArgs e)
    {
        if (SelectedLinkDefinition is not { } selected)
        {
            return;
        }

        selected.Name = _NO_TRANSLATE_Name.Text;
        ReloadCategories(selected);
    }

    private void EnabledChx_CheckedChanged(object? sender, EventArgs e)
    {
        if (SelectedLinkDefinition is { } selected)
        {
            selected.Enabled = EnabledChx.IsChecked == true;
        }
    }

    private void MessageChx_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateSet(
            SelectedLinkDefinition?.SearchInParts,
            ExternalLinkDefinition.RevisionPart.Message,
            MessageChx.IsChecked == true);
    }

    private void LocalBranchChx_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateSet(
            SelectedLinkDefinition?.SearchInParts,
            ExternalLinkDefinition.RevisionPart.LocalBranches,
            LocalBranchChx.IsChecked == true);
    }

    private void RemoteBranchChx_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateSet(
            SelectedLinkDefinition?.SearchInParts,
            ExternalLinkDefinition.RevisionPart.RemoteBranches,
            RemoteBranchChx.IsChecked == true);
    }

    private void _NO_TRANSLATE_SearchPatternEdit_Leave(object? sender, EventArgs e)
    {
        if (SelectedLinkDefinition is { } selected)
        {
            selected.SearchPattern = _NO_TRANSLATE_SearchPatternEdit.Text?.Trim();
        }
    }

    private void _NO_TRANSLATE_NestedPatternEdit_Leave(object? sender, EventArgs e)
    {
        if (SelectedLinkDefinition is { } selected)
        {
            selected.NestedSearchPattern = _NO_TRANSLATE_NestedPatternEdit.Text?.Trim();
        }
    }

    private void _NO_TRANSLATE_RemotePatern_Leave(object? sender, EventArgs e)
    {
        if (SelectedLinkDefinition is { } selected)
        {
            selected.RemoteSearchPattern = _NO_TRANSLATE_RemotePatern.Text?.Trim();
        }
    }

    private void chxURL_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateSet(
            SelectedLinkDefinition?.RemoteSearchInParts,
            ExternalLinkDefinition.RemotePart.URL,
            chxURL.IsChecked == true);
    }

    private void chxPushURL_CheckedChanged(object? sender, EventArgs e)
    {
        UpdateSet(
            SelectedLinkDefinition?.RemoteSearchInParts,
            ExternalLinkDefinition.RemotePart.PushURL,
            chxPushURL.IsChecked == true);
    }

    private void _NO_TRANSLATE_UseRemotes_Leave(object? sender, EventArgs e)
    {
        if (SelectedLinkDefinition is { } selected)
        {
            selected.UseRemotesPattern = _NO_TRANSLATE_UseRemotes.Text?.Trim();
        }
    }

    private void chkOnlyFirstRemote_CheckedChanged(object? sender, EventArgs e)
    {
        if (SelectedLinkDefinition is { } selected)
        {
            selected.UseOnlyFirstRemote = chkOnlyFirstRemote.IsChecked == true;
        }
    }

    private static void UpdateSet<T>(ISet<T>? values, T value, bool include)
    {
        if (values is null)
        {
            return;
        }

        if (include)
        {
            values.Add(value);
        }
        else
        {
            values.Remove(value);
        }
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(
            nameof(RevisionLinksSettingsPage),
            "$this",
            "Text",
            Text ?? "Revision links");
        translation.AddTranslationItem(
            nameof(RevisionLinksSettingsPage),
            nameof(CaptionCol),
            "HeaderText",
            "Caption");
        translation.AddTranslationItem(
            nameof(RevisionLinksSettingsPage),
            nameof(URICol),
            "HeaderText",
            "URI");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string neutralText = Text ?? "Revision links";
        Text = translation.TranslateItem(
            nameof(RevisionLinksSettingsPage),
            "$this",
            "Text",
            () => neutralText) ?? neutralText;
        TranslateHeader(translation, CaptionCol, nameof(CaptionCol), "Caption");
        TranslateHeader(translation, URICol, nameof(URICol), "URI");
        foreach (MenuItem item in _templateFlyout.Items.OfType<MenuItem>())
        {
            if (item.Tag is ICloudProviderExternalLinkDefinitionExtractor extractor)
            {
                item.Header = string.Format(_addTemplate.Text, extractor.ServiceName);
            }
        }
    }

    private static void TranslateHeader(
        ITranslation translation,
        Border header,
        string fieldName,
        string neutralText)
    {
        string translated = translation.TranslateItem(
            nameof(RevisionLinksSettingsPage),
            fieldName,
            "HeaderText",
            () => neutralText) ?? neutralText;
        if (header.Child is TextBlock textBlock)
        {
            textBlock.Text = translated;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(RevisionLinksSettingsPage page)
    {
        public ListBox Categories => page._NO_TRANSLATE_Categories;

        public ListBox Links => page.LinksGrid;

        public TextBox Name => page._NO_TRANSLATE_Name;

        public TextBox SearchPattern => page._NO_TRANSLATE_SearchPatternEdit;

        public TextBox NestedPattern => page._NO_TRANSLATE_NestedPatternEdit;

        public TextBox RemotePattern => page._NO_TRANSLATE_RemotePatern;

        public TextBox UseRemotes => page._NO_TRANSLATE_UseRemotes;

        public CheckBox Enabled => page.EnabledChx;

        public CheckBox Message => page.MessageChx;

        public CheckBox LocalBranch => page.LocalBranchChx;

        public CheckBox RemoteBranch => page.RemoteBranchChx;

        public CheckBox Url => page.chxURL;

        public CheckBox PushUrl => page.chxPushURL;

        public CheckBox OnlyFirstRemote => page.chkOnlyFirstRemote;

        public IReadOnlyList<MenuItem> TemplateItems
            => [.. page._templateFlyout.Items.OfType<MenuItem>()];

        public ExternalLinkDefinition? SelectedDefinition => page.SelectedLinkDefinition;

        public ExternalLinkFormat? SelectedFormat => page.LinksGrid.SelectedItem as ExternalLinkFormat;

        public void AddCategory() => page.Add_Click(page.Add, EventArgs.Empty);

        public void RemoveCategory() => page.Remove_Click(page.Remove, EventArgs.Empty);

        public void AddLink() => page.AddLink_Click(page._NO_TRANSLATE_AddLink, EventArgs.Empty);

        public void RemoveLink() => page.RemoveLink_Click(page._NO_TRANSLATE_RemoveLink, EventArgs.Empty);

        public void ApplyTextFields() => page.ApplyTextFields();

        public void LoadFromSettings(DistributedSettings settings) => page.LoadDefinitions(settings);

        public void SaveToSettings()
        {
            page.ApplyTextFields();
            Validates.NotNull(page._externalLinksManager);
            page._externalLinksManager.Save();
        }

        public void ExtractTemplate(
            ICloudProviderExternalLinkDefinitionExtractor extractor,
            IGitModule module)
            => page.ExtractExternalLinkDefinitions(extractor, module);

        public Task JoinTemplateOperationsAsync(CancellationToken cancellationToken = default)
            => page._templateOperations.JoinPendingOperationsAsync(cancellationToken);

        public static Remote FindRemoteByPreferenceForTesting(IList<Remote> remotes)
            => RevisionLinksSettingsPage.FindRemoteByPreference(remotes);
    }
}
