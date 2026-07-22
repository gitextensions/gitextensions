using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog;

/// <summary>Base for same-named Avalonia settings pages.</summary>
public abstract class SettingsPageBase : TranslatedControl, ISettingsPage
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<SettingsPageBase, string?>(nameof(Text));

    private readonly List<ISettingControlBinding> _controlBindings = [];
    private readonly List<ISettingsPageBinding> _avaloniaBindings = [];
    private IReadOnlyList<string>? _childrenText;
    private ISettingsPageHost? _pageHost;

    protected SettingsPageBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected ISettingsPageHost PageHost => _pageHost
        ?? throw new InvalidOperationException("PageHost instance was not passed to page: " + GetType().FullName);

    protected CheckSettingsLogic CheckSettingsLogic => PageHost.CheckSettingsLogic;

    protected CommonLogic CommonLogic => CheckSettingsLogic.CommonLogic;

    public virtual Control GuiControl => this;

    public virtual bool IsInstantSavePage => false;

    public virtual bool ReadOnly => false;

    protected IGitModule? Module => CommonLogic.Module;

    public virtual SettingsPageReference PageReference => new SettingsPageReferenceByType(GetType());

    protected internal IServiceProvider ServiceProvider { get; }

    protected SettingsToolTip ToolTip { get; } = new();

    protected virtual void Init(ISettingsPageHost pageHost)
    {
        _pageHost = pageHost;
    }

    public static T Create<T>(ISettingsPageHost pageHost, IServiceProvider serviceProvider) where T : SettingsPageBase
    {
        T result = (T?)Activator.CreateInstance(typeof(T), serviceProvider)
            ?? throw new InvalidOperationException($"Could not create settings page {typeof(T).FullName}.");
        result.Init(pageHost);
        return result;
    }

    public virtual string GetTitle() => Text ?? string.Empty;

    public virtual void OnPageShown()
    {
    }

    protected bool IsLoadingSettings { get; private set; }

    protected bool IsSettingsLoaded { get; private set; }

    public void LoadSettings()
    {
        IsLoadingSettings = true;
        try
        {
            SettingsToPage();
            IsSettingsLoaded = true;
        }
        finally
        {
            IsLoadingSettings = false;
        }
    }

    public void SaveSettings()
    {
        if (!ReadOnly)
        {
            PageToSettings();
        }
    }

    protected virtual void SettingsToPage()
    {
        SettingsSource settings = GetCurrentSettings();
        foreach (ISettingControlBinding binding in _controlBindings)
        {
            binding.LoadSetting(settings);
        }

        foreach (ISettingsPageBinding binding in _avaloniaBindings)
        {
            binding.Load();
        }
    }

    protected virtual void PageToSettings()
    {
        SettingsSource settings = GetCurrentSettings();
        foreach (ISettingControlBinding binding in _controlBindings)
        {
            binding.SaveSetting(settings);
        }

        foreach (ISettingsPageBinding binding in _avaloniaBindings)
        {
            binding.Save();
        }
    }

    protected abstract SettingsSource GetCurrentSettings();

    public void AddControlBinding(ISettingControlBinding binding)
    {
        _controlBindings.Add(binding);
    }

    protected void AddSettingBinding(ISetting<string> setting, ComboBox comboBox)
    {
        _avaloniaBindings.Add(new StringComboBoxBinding(setting, comboBox));
    }

    public virtual IEnumerable<string> GetSearchKeywords()
        => _childrenText ??= GetChildrenText(this);

    private static IReadOnlyList<string> GetChildrenText(Control control)
    {
        List<string> texts = [];
        foreach (Control child in new[] { control }.Concat(control.GetLogicalDescendants().OfType<Control>()))
        {
            if (!child.IsVisible || !child.IsEnabled || child is NumericUpDown)
            {
                continue;
            }

            string? text = child switch
            {
                TextBlock textBlock => textBlock.Text,
                TextBox textBox => textBox.Text,
                ContentControl { Content: string content } => content,
                HeaderedContentControl { Header: string header } => header,
                _ => null,
            };

            if (!string.IsNullOrWhiteSpace(text))
            {
                texts.Add(text.Trim());
            }
        }

        return texts;
    }

    private interface ISettingsPageBinding
    {
        void Load();

        void Save();
    }

    private sealed class StringComboBoxBinding(ISetting<string> setting, ComboBox comboBox) : ISettingsPageBinding
    {
        public void Load()
        {
            comboBox.SelectedItem = comboBox.Items
                .OfType<string>()
                .FirstOrDefault(item => string.Equals(item, setting.Value, StringComparison.Ordinal))
                ?? setting.Value;
        }

        public void Save()
        {
            if (comboBox.SelectedItem is string value)
            {
                setting.Value = value;
            }
        }
    }
}

public sealed class SettingsToolTip
{
    public void SetToolTip(Control control, object? value)
        => Avalonia.Controls.ToolTip.SetTip(control, value);
}
