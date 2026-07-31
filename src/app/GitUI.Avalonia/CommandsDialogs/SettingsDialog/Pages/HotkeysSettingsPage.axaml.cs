using GitExtensions.Extensibility.Settings;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class HotkeysSettingsPage : SettingsPageWithHeader
{
    public HotkeysSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public HotkeysSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        if (ServiceProvider is not EmptyServiceProvider)
        {
            controlHotkeys.ReloadSettings();
        }

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        if (ServiceProvider is not EmptyServiceProvider)
        {
            controlHotkeys.SaveSettings();
        }

        base.PageToSettings();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(HotkeysSettingsPage));

    internal TestAccessor GetTestAccessor() => new(controlHotkeys);

    internal readonly struct TestAccessor(ControlHotkeys controlHotkeys)
    {
        public ControlHotkeys.TestAccessor Hotkeys => controlHotkeys.GetTestAccessor();
    }
}
