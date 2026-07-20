using Avalonia.Controls;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog;

public interface IGlobalSettingsPage : ISettingsPage
{
    void SetGlobalSettings();
}

public interface ILocalSettingsPage : IGlobalSettingsPage
{
    void SetLocalSettings();

    void SetEffectiveSettings();
}

public interface IDistributedSettingsPage : ILocalSettingsPage
{
    void SetDistributedSettings();
}

public interface IGitConfigSettingsPage : ILocalSettingsPage
{
    void SetSystemSettings();
}

public sealed partial class SettingsPageHeader : TranslatedControl
{
    private readonly SettingsPageWithHeader? _page;

    public SettingsPageHeader()
        : this(page: null, canSaveInsideRepo: false)
    {
    }

    public SettingsPageHeader(SettingsPageWithHeader? page, bool canSaveInsideRepo)
    {
        InitializeComponent();
        _page = page;
        WireEvents();
        InitializeComplete();

        if (page is not null)
        {
            settingsPagePanel.Content = page;
            ConfigureHeader(canSaveInsideRepo);
        }
    }

    public bool ReadOnly
    {
        get => !settingsPagePanel.IsEnabled;
        private set => settingsPagePanel.IsEnabled = !value;
    }

    private void WireEvents()
    {
        GlobalRB.IsCheckedChanged += (_, _) =>
        {
            if (GlobalRB.IsChecked == true)
            {
                _page?.SetGlobalSettings();
                ReadOnly = false;
            }
        };
    }

    private void ConfigureHeader(bool canSaveInsideRepo)
    {
        if (!canSaveInsideRepo || _page is not ILocalSettingsPage localSettingsPage)
        {
            GlobalRB.IsChecked = true;
            SetVisible(false, EffectiveRB, arrowLocal, LocalRB, arrowDistributed, DistributedRB, arrowGlobal, arrowSystem, SystemRB);
            return;
        }

        LocalRB.IsCheckedChanged += (_, _) =>
        {
            if (LocalRB.IsChecked == true)
            {
                localSettingsPage.SetLocalSettings();
                ReadOnly = false;
            }
        };
        EffectiveRB.IsCheckedChanged += (_, _) =>
        {
            bool selected = EffectiveRB.IsChecked == true;
            SetArrowEmphasis(selected);
            if (selected)
            {
                localSettingsPage.SetEffectiveSettings();
                ReadOnly = true;
            }
        };

        if (localSettingsPage is IDistributedSettingsPage distributedSettingsPage)
        {
            DistributedRB.IsCheckedChanged += (_, _) =>
            {
                if (DistributedRB.IsChecked == true)
                {
                    distributedSettingsPage.SetDistributedSettings();
                    ReadOnly = false;
                }
            };
        }
        else
        {
            SetVisible(false, DistributedRB, arrowDistributed);
        }

        if (localSettingsPage is IGitConfigSettingsPage gitConfigSettingsPage)
        {
            SystemRB.IsCheckedChanged += (_, _) =>
            {
                if (SystemRB.IsChecked == true)
                {
                    gitConfigSettingsPage.SetSystemSettings();
                    ReadOnly = true;
                }
            };
        }
        else
        {
            SetVisible(false, SystemRB, arrowSystem);
        }

        EffectiveRB.IsChecked = true;
        ReadOnly = true;
    }

    private void SetArrowEmphasis(bool emphasized)
    {
        double opacity = emphasized ? 1 : 0;
        arrowLocal.Opacity = opacity;
        arrowDistributed.Opacity = opacity;
        arrowGlobal.Opacity = opacity;
        arrowSystem.Opacity = opacity;
    }

    private static void SetVisible(bool visible, params Control[] controls)
    {
        foreach (Control control in controls)
        {
            control.IsVisible = visible;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(SettingsPageHeader header)
    {
        public Control? Page => header.settingsPagePanel.Content as Control;

        public RadioButton Global => header.GlobalRB;

        public RadioButton Effective => header.EffectiveRB;
    }
}
