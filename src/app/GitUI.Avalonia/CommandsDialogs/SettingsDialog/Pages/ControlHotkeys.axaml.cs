using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using GitExtUtils;
using GitUI.Hotkey;
using Microsoft;
using ResourceManager;
using ResourceManager.Hotkey;
using Keys = GitExtensions.Shims.WinForms.Keys;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

/// <summary>
/// ControlHotkeys enables editing of HotkeySettings.
/// </summary>
internal partial class ControlHotkeys : GitExtensionsControl
{
    private IReadOnlyList<HotkeySettings>? _settings;

    private IReadOnlyList<HotkeySettings>? Settings
    {
        get { return _settings; }
        set
        {
            _settings = value;
            UpdateCombobox(value);
        }
    }

    #region SelectedHotkeySettings
    private HotkeySettings? _selectedHotkeySettings;
    private HotkeySettings? SelectedHotkeySettings
    {
        get { return _selectedHotkeySettings; }
        set
        {
            _selectedHotkeySettings = value;
            UpdateListViewItems(value);
        }
    }

    #endregion

    #region SelectedHotkeyCommand
    private HotkeyCommand? _selectedHotkeyCommand;
    private HotkeyCommand? SelectedHotkeyCommand
    {
        get { return _selectedHotkeyCommand; }
        set
        {
            _selectedHotkeyCommand = value;
            UpdateTextBox(value);
        }
    }
    #endregion

    public ControlHotkeys()
    {
        InitializeComponent();
        ConfigureLists();
        WireEvents();
        InitializeComplete();
    }

    private IHotkeySettingsManager HotkeySettingsManager
    {
        get
        {
            if (this.GetLogicalAncestors().OfType<SettingsPageBase>().FirstOrDefault() is not SettingsPageBase settingsPage)
            {
                throw new InvalidOperationException($"{GetType().Name} must be sited on a {typeof(SettingsPageBase)} control");
            }

            return settingsPage.ServiceProvider.GetRequiredService<IHotkeySettingsManager>();
        }
    }

    public void SaveSettings()
    {
        Validates.NotNull(Settings);

        HotkeySettingsManager.SaveSettings(Settings);
    }

    public void ReloadSettings()
    {
        Settings = HotkeySettingsManager.LoadSettings();
    }

    private void UpdateCombobox(IReadOnlyList<HotkeySettings>? settings)
    {
        SelectedHotkeySettings = null;

        cmbSettings.ItemsSource = settings;
        cmbSettings.SelectedIndex = -1;
    }

    private void UpdateListViewItems(HotkeySettings? setting)
    {
        SelectedHotkeyCommand = null;

        listMappings.ItemsSource = null;
        listMappings.ItemsSource = setting?.Commands;
        listMappings.SelectedIndex = -1;
    }

    private void UpdateTextBox(HotkeyCommand? command)
    {
        txtHotkey.KeyData = command?.KeyData ?? Keys.None;
    }

    private void cmbSettings_SelectedIndexChanged(object? sender, EventArgs e)
    {
        SelectedHotkeySettings = cmbSettings.SelectedItem as HotkeySettings;
    }

    private void listMappings_SelectedIndexChanged(object? sender, EventArgs e)
    {
        SelectedHotkeyCommand = listMappings.SelectedItem as HotkeyCommand;
    }

    private void bApply_Click(object? sender, EventArgs e)
    {
        HotkeyCommand? hotkey = SelectedHotkeyCommand;
        if (hotkey is not null)
        {
            // Update the KeyData with the chosen one
            hotkey.KeyData = txtHotkey.KeyData;

            // Refresh the ListView
            UpdateListViewItems(SelectedHotkeySettings);
        }
    }

    private void bClear_Click(object? sender, EventArgs e)
    {
        HotkeyCommand? hotkey = SelectedHotkeyCommand;
        if (hotkey is not null)
        {
            // Update the KeyData with the chosen one
            hotkey.KeyData = Keys.None;

            // Refresh the ListView
            UpdateListViewItems(SelectedHotkeySettings);
            txtHotkey.KeyData = hotkey.KeyData;
        }
    }

    private void bResetToDefaults_Click(object? sender, EventArgs e)
    {
        Settings = GitUI.Hotkey.HotkeySettingsManager.CreateDefaultSettingsCore(scriptsManager: null);
    }

    private void ConfigureLists()
    {
        cmbSettings.ItemTemplate = new FuncDataTemplate<HotkeySettings>(
            (setting, _) => new TextBlock
            {
                Margin = new Thickness(6, 2),
                Text = setting?.Name ?? string.Empty,
                VerticalAlignment = VerticalAlignment.Center,
            },
            supportsRecycling: true);
        listMappings.ItemTemplate = new FuncDataTemplate<HotkeyCommand>(
            CreateMappingRow,
            supportsRecycling: true);
    }

    private Control CreateMappingRow(HotkeyCommand? command, INameScope? nameScope)
    {
        Grid row = new()
        {
            ColumnDefinitions = new ColumnDefinitions("222,*"),
        };
        TextBlock commandText = new()
        {
            Margin = new Thickness(6, 2),
            Text = command?.Name ?? string.Empty,
            TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        };
        TextBlock keyText = new()
        {
            Margin = new Thickness(6, 2),
            Text = command?.KeyData.ToText() ?? string.Empty,
            TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        };
        Grid.SetColumn(keyText, 1);
        row.Children.Add(commandText);
        row.Children.Add(keyText);
        return row;
    }

    private void WireEvents()
    {
        cmbSettings.SelectionChanged += cmbSettings_SelectedIndexChanged;
        listMappings.SelectionChanged += listMappings_SelectedIndexChanged;
        bApply.Click += bApply_Click;
        bClear.Click += bClear_Click;
        bResetToDefaults.Click += bResetToDefaults_Click;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ControlHotkeys control)
    {
        public ListBox Settings => control.cmbSettings;

        public ListBox Mappings => control.listMappings;

        public TextboxHotkey Hotkey => control.txtHotkey;

        public Button Apply => control.bApply;

        public Button Clear => control.bClear;

        public Button Reset => control.bResetToDefaults;

        public IReadOnlyList<HotkeySettings>? Values => control.Settings;
    }
}
