using GitExtensions.Extensibility.Settings;

namespace GitUI.SettingControlBindings;

// parity-scaffolding: Hosts every original setting binding in one deterministic paired surface.
internal class SettingControlBindingsCaptureSurface : UserControl
{
    private readonly TableLayoutPanel _layout = new();

    public SettingControlBindingsCaptureSurface()
        : this(useNullAndInvalidValues: false)
    {
    }

    protected SettingControlBindingsCaptureSurface(bool useNullAndInvalidValues)
    {
        Size = new Size(800, 320);
        Padding = new Padding(12);
        _layout.ColumnCount = 2;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _layout.Dock = DockStyle.Fill;
        _layout.RowCount = 0;
        Controls.Add(_layout);

        CaptureSettingsSource settings = new(useNullAndInvalidValues);
        AddBinding("Boolean", new BoolSetting("Enabled", defaultValue: false), "boolControl", settings);
        AddBinding("Choice", new ChoiceSetting("Mode", ["one", "two", "three"], "one"), "choiceControl", settings);
        AddBinding("String", new StringSetting("Command", "fetch --all"), "stringControl", settings);
        AddBinding("Password", new PasswordSetting("Token", "secret"), "passwordControl", settings);
        AddBinding("Integer", new NumberSetting<int>("Interval", 42), "numberControl", settings);

        NumberSetting<double> ratio = new("Ratio", 1.5)
        {
            CustomControl = new TextBox(),
        };
        Control numberTextControl = AddBinding("Decimal", ratio, "numberTextControl", settings);
        if (useNullAndInvalidValues)
        {
            numberTextControl.Text = "invalid";
        }

        CredentialsSetting credentials = new("Credentials", "Credentials", () => null);
        ISettingControlBinding credentialsBinding = SettingControlBindingsProvider.CreateControlBinding(credentials);
        Control credentialsControl = credentialsBinding.GetControl();
        credentialsControl.Name = "credentialsControl";
        if (useNullAndInvalidValues)
        {
            credentialsBinding.LoadSetting(new CaptureSettingsSource(useNullAndInvalidValues: true)
            {
                SettingLevel = SettingLevel.Distributed,
            });
        }
        else if (credentialsControl is GitExtensions.Extensibility.Settings.UserControls.CredentialsControl credentialsEditor)
        {
            credentialsEditor.UserName = "capture-user";
            credentialsEditor.Password = "capture-secret";
        }

        AddControl("Credentials", credentialsControl);

        PseudoSetting pseudo = new("Read-only plugin information", height: 40);
        AddBinding("Information", pseudo, "pseudoControl", settings);
    }

    private Control AddBinding(string caption, ISetting setting, string name, SettingsSource settings)
    {
        ISettingControlBinding binding = SettingControlBindingsProvider.CreateControlBinding(setting);
        Control control = binding.GetControl();
        control.Name = name;
        binding.LoadSetting(settings);
        AddControl(caption, control);
        return control;
    }

    private void AddControl(string caption, Control control)
    {
        int row = _layout.RowCount++;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Label label = new()
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 6, 3, 3),
            Text = caption,
        };
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        control.Margin = new Padding(3);
        _layout.Controls.Add(label, 0, row);
        _layout.Controls.Add(control, 1, row);
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
