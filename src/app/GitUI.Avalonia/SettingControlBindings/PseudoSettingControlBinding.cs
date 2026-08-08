using Avalonia.Controls;
using GitExtensions.Extensibility.Settings;
using GitUI.Compat;

namespace GitUI.SettingControlBindings;

internal class PseudoSettingControlBinding : SettingControlBinding<PseudoSetting, Control>
{
    private PluginSettingControlFactory.ShimControlAdapter? _adapter;

    public PseudoSettingControlBinding(PseudoSetting setting, Control? customControl)
        : base(setting, customControl)
    {
    }

    public override Control CreateControl()
    {
        GitExtensions.Shims.WinForms.Control model = Setting.CustomControl
            ?? Setting.TextBoxCreator?.Invoke()
            ?? throw new InvalidOperationException("Pseudo setting did not supply a control model.");
        _adapter = PluginSettingControlFactory.CreateAdapter(model);
        return _adapter.Control;
    }

    public override void LoadSetting(SettingsSource settings, Control control)
    {
        // The native control mirrors the portable plugin model at the toolkit boundary.
        _adapter?.Load();
    }

    public override void SaveSetting(SettingsSource settings, Control control)
    {
        // The native control mirrors the portable plugin model at the toolkit boundary.
        _adapter?.Save();
    }
}
