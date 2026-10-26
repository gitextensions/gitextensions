using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.SettingControlBindings;

// parity-scaffolding: Hosts every native setting binding in the same deterministic paired surface as WinForms.
internal class SettingControlBindingsCaptureSurface : Grid
{
    private readonly Grid _layout = new();

    public SettingControlBindingsCaptureSurface()
        : this(useNullAndInvalidValues: false)
    {
    }

    protected SettingControlBindingsCaptureSurface(bool useNullAndInvalidValues)
    {
        _layout.Margin = new Thickness(12);
        _layout.ColumnDefinitions = new ColumnDefinitions("170,*");
        _layout.RowSpacing = 0;
        Children.Add(_layout);

        CaptureSettingsSource settings = new(useNullAndInvalidValues);
        AddBinding("Boolean", new BoolSetting("Enabled", defaultValue: false), "boolControl", settings);
        AddBinding("Choice", new ChoiceSetting("Mode", ["one", "two", "three"], "one"), "choiceControl", settings);
        AddBinding("String", new StringSetting("Command", "fetch --all"), "stringControl", settings);
        AddBinding("Password", new PasswordSetting("Token", "secret"), "passwordControl", settings);
        AddBinding("Integer", new NumberSetting<int>("Interval", 42), "numberControl", settings);

        NumberSetting<double> ratio = new("Ratio", 1.5)
        {
            CustomControl = new WinFormsShims.TextBox(),
        };
        Control numberTextControl = AddBinding("Decimal", ratio, "numberTextControl", settings);
        if (useNullAndInvalidValues)
        {
            ((TextBox)numberTextControl).Text = "invalid";
        }

        CredentialsSetting credentials = new("Credentials", "Credentials", () => null);
        PluginSettingBinding credentialsBinding = SettingControlBindingsProvider.CreateControlBinding(credentials);
        Control credentialsControl = credentialsBinding.GetControl();
        credentialsControl.Name = "credentialsControl";
        if (useNullAndInvalidValues)
        {
            credentialsBinding.LoadSetting(new CaptureSettingsSource(useNullAndInvalidValues: true)
            {
                SettingLevel = SettingLevel.Distributed,
            });
        }
        else
        {
            TextBox[] fields = ((Grid)credentialsControl).GetLogicalDescendants().OfType<TextBox>().ToArray();
            fields[0].Text = "capture-user";
            fields[1].Text = "capture-secret";
        }

        AddControl("Credentials", credentialsControl);

        PseudoSetting pseudo = new("Read-only plugin information", height: 40);
        AddBinding("Information", pseudo, "pseudoControl", settings);
    }

    private Control AddBinding(string caption, ISetting setting, string name, SettingsSource settings)
    {
        PluginSettingBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        Control control = binding.GetControl();
        control.Name = name;
        binding.LoadSetting(settings);
        AddControl(caption, control);
        return control;
    }

    private void AddControl(string caption, Control control)
    {
        int row = _layout.RowDefinitions.Count;
        double rowHeight = control.Name switch
        {
            "boolControl" => 29,
            "credentialsControl" => 51,
            "pseudoControl" => 45,
            _ => 28,
        };
        // WinForms TableLayoutPanel's AutoSize algorithm uses each source control's
        // preferred height. Avalonia's arranged track contributes the shared final boundary
        // DIP, so these integer definitions are one less than the effective WinForms rows.
        // CredentialsControl contributes its inherited 46-DIP preferred height while its
        // contradictory min/max contract arranges the control at 24 DIPs.
        _layout.RowDefinitions.Add(new RowDefinition(new GridLength(rowHeight)));
        TextBlock label = new()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = control.Name switch
            {
                "credentialsControl" => new Thickness(3, 9, 3, 3),
                "pseudoControl" => new Thickness(3, 17, 3, 3),
                "boolControl" => new Thickness(3, 9, 3, 3),
                _ => new Thickness(3, 8, 3, 3),
            },
            Text = caption,
            VerticalAlignment = VerticalAlignment.Top,
        };
        WinFormsAutoSizeTextBlock.Attach(label, includePadding: true);
        control.HorizontalAlignment = HorizontalAlignment.Stretch;
        control.Margin = new Thickness(3);
        control.VerticalAlignment = VerticalAlignment.Top;
        SetRow(label, row);
        SetColumn(control, 1);
        SetRow(control, row);
        _layout.Children.Add(label);
        _layout.Children.Add(control);
    }

    private sealed class CaptureSettingsSource(bool useNullAndInvalidValues) : SettingsSource
    {
        private readonly Dictionary<string, string?> _values = useNullAndInvalidValues
            ? []
            : new(StringComparer.Ordinal)
            {
                ["Enabled"] = "true",
                ["Mode"] = "two",
                ["Command"] = "fetch --all",
                ["Token"] = "capture-secret",
                ["Interval"] = "42",
                ["Ratio"] = "1.5",
            };

        public override string? GetValue(string name) => _values.GetValueOrDefault(name);

        public override void SetValue(string name, string? value) => _values[name] = value;
    }
}

// parity-scaffolding: Exercises unset, invalid, indeterminate, and unsupported-level rendering.
internal sealed class SettingControlBindingsNullCaptureSurface : SettingControlBindingsCaptureSurface
{
    public SettingControlBindingsNullCaptureSurface()
        : base(useNullAndInvalidValues: true)
    {
    }
}
