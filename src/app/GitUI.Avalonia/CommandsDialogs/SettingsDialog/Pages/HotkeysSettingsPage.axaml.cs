using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;

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

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(
            nameof(HotkeysSettingsPage),
            "$this",
            "Text",
            Text ?? "Hotkeys");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string neutralText = Text ?? "Hotkeys";
        Text = translation.TranslateItem(
            nameof(HotkeysSettingsPage),
            "$this",
            "Text",
            () => neutralText) ?? neutralText;
    }

    internal TestAccessor GetTestAccessor() => new(controlHotkeys);

    internal readonly struct TestAccessor(ControlHotkeys controlHotkeys)
    {
        public ControlHotkeys.TestAccessor Hotkeys => controlHotkeys.GetTestAccessor();
    }
}
