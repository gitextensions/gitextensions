using System.ComponentModel;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;
using GitUI.Properties;
using GitUI.ScriptsEngine;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class ScriptsSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _scriptSettingsPageHelpDisplayArgumentsHelp = new("Arguments help");
    private readonly TranslationString _scriptSettingsPageHelpDisplayContent = new(@"Use {option} for normal replacement.
Use {{option}} for quoted replacement.

User inputs:
{UserInput}
{UserInput:a popup label}
{UserInput:a popup label=a default value}
{UserInput:a popup label=a default value using {sLocalBranch}}
{UserFiles}

Working directory:
{WorkingDir}

Repository:
{RepoName}

Selected commits:
{sHashes}

Selected revision:
{sTag}
{sBranch}
{sLocalBranch}
{sRemoteBranch}
{sRemoteBranchName}   (without the remote's name)
{sRemote}
{sRemoteUrl}
{sRemotePathFromUrl}
{sHash}
{sMessage}
{sSubject}
{sAuthor}
{sCommitter}
{sAuthorDate}
{sCommitDate}

Currently checked out revision:
{HEAD}   (checked out branch name or checked out commit hash)
{cTag}
{cBranch}
{cLocalBranch}
{cRemoteBranch}
{cRemoteBranchName}   (without the remote's name)
{cHash}
{cMessage}
{cSubject}
{cAuthor}
{cCommitter}
{cAuthorDate}
{cCommitDate}
{cDefaultRemote}
{cDefaultRemoteUrl}
{cDefaultRemotePathFromUrl}

Diff selection:
{SelectedRelativePaths}   (relative paths as they were in the selected commit)
{LineNumber}
{ColumnNumber}");

    private readonly BindingList<ScriptInfoProxy> _scripts = [];
    private readonly IReadOnlyList<IconChoice> _iconChoices;
    private readonly IScriptsManager _scriptsManager;
    private SimpleHelpDisplayDialog? _argumentsCheatSheet;
    private bool _updatingEditor;

    public ScriptsSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public ScriptsSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _scriptsManager = serviceProvider.GetService(typeof(IScriptsManager)) as IScriptsManager
            ?? new ScriptsManager();
        _iconChoices = CreateIconChoices();

        InitializeComponent();
        ConfigureSelectors();
        WireEvents();
        InitializeComplete();
    }

    private ScriptInfoProxy? SelectedScript { get; set; }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(ScriptsSettingsPage));

    protected override void SettingsToPage()
    {
        _scripts.Clear();
        foreach (ScriptInfo script in _scriptsManager.GetScripts())
        {
            _scripts.Add((ScriptInfoProxy)script);
        }

        BindScripts(_scripts, selectedScript: null);
        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        ApplyEditorToSelectedScript();

        System.ComponentModel.BindingList<ScriptInfo> scripts = _scriptsManager.GetScripts();
        scripts.Clear();
        foreach (ScriptInfoProxy proxy in _scripts)
        {
            scripts.Add(proxy);
        }

        AppSettings.OwnScripts = _scriptsManager.SerializeIntoXml();
        base.PageToSettings();
    }

    private void ConfigureSelectors()
    {
        cbxOnEvent.ItemsSource = Enum.GetValues<ScriptEvent>();
        cbxIcon.ItemsSource = _iconChoices;
        cbxIcon.ItemTemplate = new FuncDataTemplate<IconChoice>(
            (choice, _) => new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                Children =
                {
                    new Image
                    {
                        Width = 16,
                        Height = 16,
                        Source = choice?.Image,
                        IsVisible = choice?.Image is not null,
                    },
                    new TextBlock
                    {
                        Text = choice?.DisplayName ?? string.Empty,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                },
            },
            supportsRecycling: true);
    }

    private void WireEvents()
    {
        lvScripts.SelectionChanged += lvScripts_SelectedIndexChanged;
        btnAdd.Click += btnAdd_Click;
        btnDelete.Click += btnDelete_Click;
        btnMoveUp.Click += btnMoveUp_Click;
        btnMoveDown.Click += btnMoveDown_Click;
        btnArgumentsHelp.Click += btnArgumentsHelp_Click;
        btnBrowseCommand.Click += (_, _) => this.InvokeAndForget(() => BrowseFileAsync(txtCommand, "Select script command", [FilePickerFileTypes.All]));
        btnBrowseIcon.Click += (_, _) => this.InvokeAndForget(() => BrowseFileAsync(
            txtIconFilePath,
            "Select script icon or associated file",
            [new FilePickerFileType("Images") { Patterns = ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif", "*.ico"] }, FilePickerFileTypes.All]));

        txtName.TextChanged += EditorValueChanged;
        chkEnabled.IsCheckedChanged += EditorValueChanged;
        txtCommand.TextChanged += EditorValueChanged;
        txtArguments.TextChanged += EditorValueChanged;
        cbxOnEvent.SelectionChanged += EditorValueChanged;
        cbxIcon.SelectionChanged += EditorValueChanged;
        txtIconFilePath.TextChanged += EditorValueChanged;
        chkAskConfirmation.IsCheckedChanged += EditorValueChanged;
        chkRunInBackground.IsCheckedChanged += EditorValueChanged;
        chkIsPowerShell.IsCheckedChanged += EditorValueChanged;
        chkAddToRevisionGridContextMenu.IsCheckedChanged += EditorValueChanged;
        DetachedFromVisualTree += (_, _) => _argumentsCheatSheet?.Close();
    }

    private void BindScripts(IEnumerable<ScriptInfoProxy> scripts, ScriptInfoProxy? selectedScript)
    {
        lvScripts.SelectionChanged -= lvScripts_SelectedIndexChanged;
        try
        {
            lvScripts.Items.Clear();
            foreach (ScriptInfoProxy script in scripts)
            {
                ListBoxItem item = new()
                {
                    Content = CreateScriptRow(script),
                    Tag = script,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                };
                Avalonia.Controls.ToolTip.SetTip(item, string.Concat(script.Command, " ", script.Arguments));
                lvScripts.Items.Add(item);
            }

            ListBoxItem? selectedItem = lvScripts.Items
                .OfType<ListBoxItem>()
                .FirstOrDefault(item => ReferenceEquals(item.Tag, selectedScript))
                ?? lvScripts.Items.OfType<ListBoxItem>().FirstOrDefault();
            lvScripts.SelectedItem = selectedItem;
            SetSelectedScript(selectedItem?.Tag as ScriptInfoProxy);
        }
        finally
        {
            lvScripts.SelectionChanged += lvScripts_SelectedIndexChanged;
        }
    }

    private Grid CreateScriptRow(ScriptInfoProxy script)
    {
        Grid row = new()
        {
            Height = 24,
            ColumnDefinitions = new ColumnDefinitions("28,24,200,130,*"),
        };
        CheckBox enabled = new()
        {
            IsChecked = script.Enabled,
            IsHitTestVisible = false,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Image icon = new()
        {
            Width = 16,
            Height = 16,
            Source = GetScriptIcon(script),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        TextBlock name = CreateCell(script.Name);
        TextBlock scriptEvent = CreateCell(script.OnEvent.ToString());
        TextBlock command = CreateCell(string.Concat(script.Command, " ", script.Arguments));
        if (!script.Enabled)
        {
            name.Classes.Add("script-disabled");
            scriptEvent.Classes.Add("script-disabled");
            command.Classes.Add("script-disabled");
        }

        Grid.SetColumn(icon, 1);
        Grid.SetColumn(name, 2);
        Grid.SetColumn(scriptEvent, 3);
        Grid.SetColumn(command, 4);
        row.Children.Add(enabled);
        row.Children.Add(icon);
        row.Children.Add(name);
        row.Children.Add(scriptEvent);
        row.Children.Add(command);
        return row;

        static TextBlock CreateCell(string? text)
            => new()
            {
                Margin = new Avalonia.Thickness(5, 0),
                Text = text ?? string.Empty,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
            };
    }

    private static IImage? GetScriptIcon(ScriptInfoProxy script)
    {
        if (File.Exists(script.IconFilePath))
        {
            try
            {
                return new Bitmap(script.IconFilePath);
            }
            catch
            {
            }
        }

        return GetEmbeddedIcon(script.Icon);
    }

    private static IImage? GetEmbeddedIcon(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        PropertyInfo? property = typeof(Images).GetProperty(key, BindingFlags.Public | BindingFlags.Static);
        return property?.GetValue(obj: null) as IImage;
    }

    private static IReadOnlyList<IconChoice> CreateIconChoices()
    {
        List<IconChoice> choices = [new(null, Image: null)];
        choices.AddRange(Images.ResourcePaths.Keys
            .Order(StringComparer.Ordinal)
            .Select(key => new IconChoice(key, GetEmbeddedIcon(key))));
        return choices;
    }

    private void SetSelectedScript(ScriptInfoProxy? script)
    {
        SelectedScript = script;
        bool hasSelection = script is not null;
        propertyGrid1.IsEnabled = hasSelection;
        btnDelete.IsEnabled = hasSelection;

        int index = script is null ? -1 : _scripts.IndexOf(script);
        btnMoveUp.IsEnabled = index > 0;
        btnMoveDown.IsEnabled = index >= 0 && index < _scripts.Count - 1;

        _updatingEditor = true;
        try
        {
            txtName.Text = script?.Name;
            chkEnabled.IsChecked = script?.Enabled ?? false;
            txtCommand.Text = script?.Command;
            txtArguments.Text = script?.Arguments;
            cbxOnEvent.SelectedItem = script?.OnEvent ?? ScriptEvent.None;
            cbxIcon.SelectedItem = _iconChoices.FirstOrDefault(choice => string.Equals(choice.Key, script?.Icon, StringComparison.Ordinal))
                ?? _iconChoices[0];
            txtIconFilePath.Text = script?.IconFilePath;
            chkAskConfirmation.IsChecked = script?.AskConfirmation ?? false;
            chkRunInBackground.IsChecked = script?.RunInBackground ?? false;
            chkIsPowerShell.IsChecked = script?.IsPowerShell ?? false;
            chkAddToRevisionGridContextMenu.IsChecked = script?.AddToRevisionGridContextMenu ?? false;
        }
        finally
        {
            _updatingEditor = false;
        }
    }

    private void ApplyEditorToSelectedScript()
    {
        if (_updatingEditor || SelectedScript is not ScriptInfoProxy script)
        {
            return;
        }

        script.Name = txtName.Text;
        script.Enabled = chkEnabled.IsChecked == true;
        script.Command = txtCommand.Text;
        script.Arguments = txtArguments.Text;
        script.OnEvent = cbxOnEvent.SelectedItem is ScriptEvent scriptEvent ? scriptEvent : ScriptEvent.None;
        script.Icon = (cbxIcon.SelectedItem as IconChoice)?.Key;
        script.IconFilePath = txtIconFilePath.Text;
        script.AskConfirmation = chkAskConfirmation.IsChecked == true;
        script.RunInBackground = chkRunInBackground.IsChecked == true;
        script.IsPowerShell = chkIsPowerShell.IsChecked == true;
        script.AddToRevisionGridContextMenu = chkAddToRevisionGridContextMenu.IsChecked == true;
    }

    private void EditorValueChanged(object? sender, EventArgs e)
    {
        if (_updatingEditor)
        {
            return;
        }

        ApplyEditorToSelectedScript();
        BindScripts(_scripts, SelectedScript);
    }

    private void btnAdd_Click(object? sender, RoutedEventArgs e)
    {
        ScriptInfoProxy script = _scripts.AddNew();
        script.HotkeyCommandIdentifier = Math.Max(
            ScriptsManager.MinimumUserScriptID,
            _scripts.Select(existing => existing.HotkeyCommandIdentifier).DefaultIfEmpty(0).Max()) + 1;
        script.Name = "<New Script>";
        script.Enabled = true;
        BindScripts(_scripts, script);
        txtName.Focus();
        txtName.SelectAll();
    }

    private void btnDelete_Click(object? sender, RoutedEventArgs e)
    {
        if (SelectedScript is not ScriptInfoProxy script)
        {
            return;
        }

        _scripts.Remove(script);
        BindScripts(_scripts, selectedScript: null);
    }

    private void btnMoveDown_Click(object? sender, RoutedEventArgs e)
    {
        if (SelectedScript is not ScriptInfoProxy script)
        {
            return;
        }

        int index = _scripts.IndexOf(script);
        _scripts.Remove(script);
        _scripts.Insert(Math.Min(index + 1, _scripts.Count), script);
        BindScripts(_scripts, script);
    }

    private void btnMoveUp_Click(object? sender, RoutedEventArgs e)
    {
        if (SelectedScript is not ScriptInfoProxy script)
        {
            return;
        }

        int index = _scripts.IndexOf(script);
        _scripts.Remove(script);
        _scripts.Insert(Math.Max(index - 1, 0), script);
        BindScripts(_scripts, script);
    }

    private void lvScripts_SelectedIndexChanged(object? sender, SelectionChangedEventArgs e)
    {
        ApplyEditorToSelectedScript();
        SetSelectedScript((lvScripts.SelectedItem as ListBoxItem)?.Tag as ScriptInfoProxy);
    }

    private void btnArgumentsHelp_Click(object? sender, RoutedEventArgs e)
    {
        if (_argumentsCheatSheet?.IsVisible == true)
        {
            _argumentsCheatSheet.Activate();
            return;
        }

        _argumentsCheatSheet = new SimpleHelpDisplayDialog
        {
            DialogTitle = _scriptSettingsPageHelpDisplayArgumentsHelp.Text,
            ContentText = _scriptSettingsPageHelpDisplayContent.Text.Replace("\n", Environment.NewLine),
        };
        if (TopLevel.GetTopLevel(this) is Window owner)
        {
            _argumentsCheatSheet.Show(owner);
        }
        else
        {
            _argumentsCheatSheet.Show();
        }
    }

    private async Task BrowseFileAsync(TextBox target, string title, IReadOnlyList<FilePickerFileType> fileTypes)
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is null)
        {
            return;
        }

        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            Title = title,
            FileTypeFilter = fileTypes,
        };
        string? currentDirectory = Path.GetDirectoryName(target.Text);
        if (!string.IsNullOrEmpty(currentDirectory))
        {
            options.SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(currentDirectory);
        }

        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        string? path = files.FirstOrDefault()?.TryGetLocalPath();
        if (!string.IsNullOrWhiteSpace(path))
        {
            target.Text = path;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    private sealed record IconChoice(string? Key, IImage? Image)
    {
        public string DisplayName => Key ?? "(none)";
    }

    internal readonly struct TestAccessor(ScriptsSettingsPage page)
    {
        public IReadOnlyList<ScriptInfo> Scripts => page._scripts.Select(script => (ScriptInfo)script).ToArray();

        public ListBox List => page.lvScripts;

        public TextBox Name => page.txtName;

        public CheckBox Enabled => page.chkEnabled;

        public TextBox Command => page.txtCommand;

        public TextBox Arguments => page.txtArguments;

        public ComboBox OnEvent => page.cbxOnEvent;

        public ComboBox Icon => page.cbxIcon;

        public TextBox IconFilePath => page.txtIconFilePath;

        public CheckBox AskConfirmation => page.chkAskConfirmation;

        public CheckBox RunInBackground => page.chkRunInBackground;

        public CheckBox IsPowerShell => page.chkIsPowerShell;

        public CheckBox AddToRevisionGridContextMenu => page.chkAddToRevisionGridContextMenu;

        public Button Add => page.btnAdd;

        public Button Delete => page.btnDelete;

        public Button MoveUp => page.btnMoveUp;

        public Button MoveDown => page.btnMoveDown;

        public Button ArgumentsHelp => page.btnArgumentsHelp;

        public void Select(int index) => page.lvScripts.SelectedIndex = index;

        public void SelectIcon(string? key)
            => page.cbxIcon.SelectedItem = page._iconChoices.Single(choice => string.Equals(choice.Key, key, StringComparison.Ordinal));
    }
}
