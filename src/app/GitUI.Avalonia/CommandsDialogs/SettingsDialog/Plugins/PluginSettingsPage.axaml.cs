using System.Text;
using Avalonia.Controls;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using Microsoft;

namespace GitUI.CommandsDialogs.SettingsDialog.Plugins;

public partial class PluginSettingsPage : DistributedSettingsPage
{
    private readonly List<PluginSettingBinding> _settingBindings = [];
    private IGitPlugin? _gitPlugin;
    private GitPluginSettingsContainer? _settingsContainer;

    public PluginSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public PluginSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
    }

    private void CreateSettingsControls()
    {
        StringBuilder state = new();
        try
        {
            state.Append(_gitPlugin is null ? "null" : $"{(_gitPlugin.HasSettings ? "" : "not ")}having settings");
            ISetting[] settings = [.. GetSettings()];
            state.Append($", enumerable with {settings.Length} setting(s)");

            foreach (ISetting setting in settings)
            {
                PluginSettingBinding binding = PluginSettingControlFactory.Create(setting);
                _settingBindings.Add(binding);
                settingsPanel.Children.Add(CreateSettingRow(binding));
                state.Append('.');
            }

            labelNoSettings.IsVisible = settings.Length == 0;
            settingsPanel.IsVisible = settings.Length != 0;
        }
        catch (Exception exception)
        {
            throw new ExternalOperationException(
                command: $"Cannot load settings for plugin {_gitPlugin?.Name ?? "unknown"}: {state}",
                innerException: exception);
        }
    }

    private void Init(IGitPlugin gitPlugin)
    {
        Validates.NotNull(gitPlugin.Description);
        _gitPlugin = gitPlugin;
        _settingsContainer = new GitPluginSettingsContainer(gitPlugin.Id, gitPlugin.Description!);
        CreateSettingsControls();
        InitializeComplete();
    }

    public static PluginSettingsPage CreateSettingsPageFromPlugin(
        ISettingsPageHost pageHost,
        IGitPlugin gitPlugin,
        IServiceProvider serviceProvider)
    {
        PluginSettingsPage result = Create<PluginSettingsPage>(pageHost, serviceProvider);
        result.Init(gitPlugin);
        return result;
    }

    protected override SettingsSource GetCurrentSettings()
    {
        if (_settingsContainer is null)
        {
            return base.GetCurrentSettings();
        }

        _settingsContainer.SetSettingsSource(base.GetCurrentSettings());
        return _settingsContainer;
    }

    public override string GetTitle()
        => _gitPlugin?.Name ?? string.Empty;

    private IEnumerable<ISetting> GetSettings()
    {
        if (_gitPlugin is null)
        {
            throw new InvalidOperationException("Plugin settings page has not been initialized.");
        }

        return _gitPlugin.HasSettings ? _gitPlugin.GetSettings() : [];
    }

    public override SettingsPageReference PageReference
        => new SettingsPageReferenceByType(
            _gitPlugin?.GetType() ?? throw new InvalidOperationException("Plugin settings page has not been initialized."));

    protected override void SettingsToPage()
    {
        SettingsSource settings = GetCurrentSettings();
        foreach (PluginSettingBinding binding in _settingBindings)
        {
            binding.Load(settings);
        }
    }

    protected override void PageToSettings()
    {
        SettingsSource settings = GetCurrentSettings();
        foreach (PluginSettingBinding binding in _settingBindings)
        {
            binding.Save(settings);
        }
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string numberPlaceholder = translation.TranslateItem(
            nameof(SettingsPageBase),
            "_numberSettingPlaceholder",
            "Text",
            () => PluginSettingControlFactory.NumberPlaceholder) ?? PluginSettingControlFactory.NumberPlaceholder;
        string stringPlaceholder = translation.TranslateItem(
            nameof(SettingsPageBase),
            "_stringSettingPlaceholder",
            "Text",
            () => PluginSettingControlFactory.StringPlaceholder) ?? PluginSettingControlFactory.StringPlaceholder;
        foreach (PluginSettingBinding binding in _settingBindings)
        {
            binding.SetPlaceholder(numberPlaceholder, stringPlaceholder);
        }
    }

    private static Grid CreateSettingRow(PluginSettingBinding binding)
    {
        Grid row = new()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
            ColumnSpacing = 10,
        };
        if (binding.Caption is not null)
        {
            TextBlock label = new()
            {
                Text = binding.Caption,
                Margin = new Avalonia.Thickness(0, 2, 0, 0),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            };
            Grid.SetColumn(label, 0);
            row.Children.Add(label);
        }

        Grid.SetColumn(binding.Control, 1);
        row.Children.Add(binding.Control);
        return row;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(PluginSettingsPage page)
    {
        internal IReadOnlyList<PluginSettingBinding> Bindings => page._settingBindings;
    }
}
