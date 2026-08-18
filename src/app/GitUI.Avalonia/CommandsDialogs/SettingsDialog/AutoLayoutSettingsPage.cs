using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;

namespace GitUI.CommandsDialogs.SettingsDialog;

public abstract partial class AutoLayoutSettingsPage : DistributedSettingsPage, ISettingsLayout
{
    private readonly List<PluginSettingBinding> _settingBindings = [];
    private ISettingsLayout? _settingsLayout;

    public AutoLayoutSettingsPage(IServiceProvider serviceProvider)
       : base(serviceProvider)
    {
    }

    protected virtual ISettingsLayout GetSettingsLayout()
    {
        if (_settingsLayout is null)
        {
            _settingsLayout = CreateSettingsLayout();
            if (_settingsLayout.GetControl().Parent is null)
            {
                Content = _settingsLayout.GetControl();
            }
        }

        return _settingsLayout;
    }

    protected virtual ISettingsLayout CreateSettingsLayout()
    {
        return new TableSettingsLayout(this, CreateDefaultTableLayoutPanel());
    }

    public static Grid CreateDefaultTableLayoutPanel()
    {
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 10,
            RowSpacing = 6,
        };
    }

    public void AddSettingControl(ISettingControlBinding controlBinding)
    {
        GetSettingsLayout().AddSettingControl(controlBinding);
    }

    public Control GetControl()
    {
        return this;
    }

    public void AddSettingsLayout(ISettingsLayout layout)
    {
        GetSettingsLayout().AddSettingsLayout(layout);
    }

    public new void AddControlBinding(ISettingControlBinding controlBinding)
    {
        // Avalonia cannot host the WinForms-shaped control returned by the shared binding.
        // TableSettingsLayout registers the native control it materializes for this binding.
    }

    internal void AddAvaloniaBinding(PluginSettingBinding binding) => _settingBindings.Add(binding);

    protected override void SettingsToPage()
    {
        SettingsSource settings = GetCurrentSettings();
        foreach (PluginSettingBinding binding in _settingBindings)
        {
            binding.Load(settings);
        }

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        SettingsSource settings = GetCurrentSettings();
        foreach (PluginSettingBinding binding in _settingBindings)
        {
            binding.Save(settings);
        }

        base.PageToSettings();
    }
}

public interface ISettingsLayout
{
    void AddSettingControl(ISettingControlBinding controlBinding);
    void AddSettingsLayout(ISettingsLayout layout);
    Control GetControl();
    void AddControlBinding(ISettingControlBinding controlBinding);
}

public abstract class BaseSettingsLayout : ISettingsLayout
{
    public readonly ISettingsLayout ParentLayout;

    protected BaseSettingsLayout(ISettingsLayout parentLayout)
    {
        ParentLayout = parentLayout;
    }

    public void AddControlBinding(ISettingControlBinding aControlBinding)
    {
        ParentLayout.AddControlBinding(aControlBinding);
    }

    public void AddSettingControl(ISettingControlBinding aControlBinding)
    {
        AddControlBinding(aControlBinding);
        AddSettingControlImpl(aControlBinding);
    }

    public abstract void AddSettingControlImpl(ISettingControlBinding controlBinding);
    public abstract void AddSettingsLayout(ISettingsLayout layout);
    public abstract Control GetControl();
}

public class TableSettingsLayout : BaseSettingsLayout
{
    protected Grid Panel { get; }
    private int _currentRow = -1;

    public TableSettingsLayout(ISettingsLayout parentLayout, Grid panel)
        : base(parentLayout)
    {
        Panel = panel;
    }

    public override void AddSettingControlImpl(ISettingControlBinding controlBinding)
    {
        _currentRow++;
        PluginSettingBinding binding = PluginSettingControlFactory.Create(controlBinding);
        GetRootLayout().AddAvaloniaBinding(binding);
        Panel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        string? caption = binding.Caption();
        if (caption is not null)
        {
            TextBlock label = new()
            {
                Text = caption,
                Margin = new Avalonia.Thickness(0, 2, 0, 0),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            };
            Grid.SetRow(label, _currentRow);
            Panel.Children.Add(label);
        }

        Grid.SetRow(binding.Control, _currentRow);
        Grid.SetColumn(binding.Control, 1);
        Panel.Children.Add(binding.Control);
    }

    public override void AddSettingsLayout(ISettingsLayout layout)
    {
        _currentRow++;
        Control control = layout.GetControl();
        Panel.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        Grid.SetRow(control, _currentRow);
        Grid.SetColumn(control, 1);
        Panel.Children.Add(control);
    }

    public override Control GetControl()
    {
        return Panel;
    }

    private AutoLayoutSettingsPage GetRootLayout()
    {
        ISettingsLayout layout = ParentLayout;
        while (layout is BaseSettingsLayout nested)
        {
            layout = nested.ParentLayout;
        }

        return (AutoLayoutSettingsPage)layout;
    }
}
