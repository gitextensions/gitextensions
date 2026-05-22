using GitExtensions.Extensibility.Settings;

namespace GitUI.SettingControlBindings;

internal class NumberSettingNumericUpDownBinding : SettingControlBinding<NumberSetting<int>, NumericUpDown>
{
    private readonly ToolTip _toolTip = new();

    public NumberSettingNumericUpDownBinding(NumberSetting<int> setting, NumericUpDown? customControl)
        : base(setting, customControl)
    {
    }

    public override NumericUpDown CreateControl()
    {
        NumericUpDown numericUpDown = new()
        {
            // TODO: if we need negative values, int.MinValue should be the Minimum.
            //       Or, we can attempt to introduce a NumberSetting<int> constructor that accepts a min and max value parameter.
            Minimum = 0,
            Maximum = int.MaxValue
        };

        Setting.CustomControl = numericUpDown;
        return (NumericUpDown)Setting.CustomControl;
    }

    public override void LoadSetting(SettingsSource settings, NumericUpDown control)
    {
        object? value = Setting[settings];
        if (value is null)
        {
            if (settings.SettingLevel != SettingLevel.Effective)
            {
                control.ResetText();
                _toolTip.SetToolTip(control, NumberSettingControlBinding.PlaceholderText);
                return;
            }

            value = Setting.DefaultValue;
        }

        control.Value = (int)value;
        control.Text = control.Value.ToString(); // needed if Text was cleared
        _toolTip.SetToolTip(control, "");
    }

    public override void SaveSetting(SettingsSource settings, NumericUpDown control)
    {
        if (string.IsNullOrEmpty(control.Text))
        {
            Setting[settings] = null;
            return;
        }

        int controlValue = (int)control.Value;

        if (settings.SettingLevel == SettingLevel.Effective && Setting.ValueOrDefault(settings) == controlValue)
        {
            return;
        }

        Setting[settings] = controlValue;
    }
}
