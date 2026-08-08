using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;

namespace GitUI.SettingControlBindings;

internal class NumberSettingNumericUpDownBinding : SettingControlBinding<NumberSetting<int>, NumericUpDown>
{
    private string _toolTip = "";

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

        return numericUpDown;
    }

    public override void LoadSetting(SettingsSource settings, NumericUpDown control)
    {
        object? value = Setting[settings];
        if (value is null)
        {
            if (settings.SettingLevel != SettingLevel.Effective)
            {
                control.Value = null;
                _toolTip = NumberSettingControlBinding.PlaceholderText;
                ToolTip.SetTip(control, _toolTip);
                return;
            }

            value = Setting.DefaultValue;
        }

        control.Value = (int)value;

        // Avalonia updates its text when Value is assigned; this is needed if Value was cleared.
        _toolTip = "";
        ToolTip.SetTip(control, _toolTip);
    }

    public override void SaveSetting(SettingsSource settings, NumericUpDown control)
    {
        if (control.Value is null)
        {
            Setting[settings] = null;
            return;
        }

        int controlValue = decimal.ToInt32(control.Value.Value);

        if (settings.SettingLevel == SettingLevel.Effective && Setting.ValueOrDefault(settings) == controlValue)
        {
            return;
        }

        Setting[settings] = controlValue;
    }
}
