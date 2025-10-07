#nullable enable

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

public partial class SettingsPageHeader
{
    private readonly SettingsPageWithHeader? _page;

    public SettingsPageHeader(SettingsPageWithHeader? page, bool canSaveInsideRepo)
    {
        InitializeComponent();
        InitializeComplete();

        label1.Font = new Font(label1.Font, System.Drawing.FontStyle.Bold);

        if (page is not null)
        {
            settingsPagePanel.Controls.Add(page);
            page.Dock = DockStyle.Fill;
            _page = page;
            ConfigureHeader(canSaveInsideRepo);
        }
    }

    public bool ReadOnly
    {
        get => !settingsPagePanel.Enabled;
        private set
        {
            settingsPagePanel.Enabled = !value;
        }
    }

    private void ConfigureHeader(bool canSaveInsideRepo)
    {
        if (!canSaveInsideRepo || _page is not ILocalSettingsPage localSettingsPage)
        {
            GlobalRB.Checked = true;

            EffectiveRB.Visible = false;
            arrowLocal.Visible = false;
            LocalRB.Visible = false;
            arrowDistributed.Visible = false;
            DistributedRB.Visible = false;
            arrowGlobal.Visible = false;
            arrowSystem.Visible = false;
            SystemRB.Visible = false;
            tableLayoutPanel2.RowStyles[2].Height = 0;
            return;
        }

        LocalRB.CheckedChanged += (s, e) =>
        {
            if (LocalRB.Checked)
            {
                localSettingsPage.SetLocalSettings();
                ReadOnly = false;
            }
        };

        EffectiveRB.CheckedChanged += (s, e) =>
        {
            if (EffectiveRB.Checked)
            {
                arrowLocal.ForeColor = EffectiveRB.ForeColor;
                localSettingsPage.SetEffectiveSettings();
                ReadOnly = true;
            }
            else
            {
                arrowLocal.ForeColor = arrowLocal.BackColor;
            }

            arrowDistributed.ForeColor = arrowLocal.ForeColor;
            arrowGlobal.ForeColor = arrowLocal.ForeColor;
            arrowSystem.ForeColor = arrowLocal.ForeColor;
        };

        EffectiveRB.Checked = true;
        ReadOnly = true;

        if (localSettingsPage is not IDistributedSettingsPage distributedSettingsPage)
        {
            DistributedRB.Visible = false;
            arrowDistributed.Visible = false;
        }
        else
        {
            DistributedRB.CheckedChanged += (s, e) =>
            {
                if (DistributedRB.Checked)
                {
                    distributedSettingsPage.SetDistributedSettings();
                    ReadOnly = false;
                }
            };
        }

        if (localSettingsPage is not IGitConfigSettingsPage configFileSettingsPage)
        {
            SystemRB.Visible = false;
            arrowSystem.Visible = false;
        }
        else
        {
            SystemRB.CheckedChanged += (s, e) =>
            {
                if (SystemRB.Checked)
                {
                    configFileSettingsPage.SetSystemSettings();
                    ReadOnly = true;
                }
            };
        }
    }

    private void GlobalRB_CheckedChanged(object sender, EventArgs e)
    {
        if (GlobalRB.Checked)
        {
            _page?.SetGlobalSettings();
            ReadOnly = false;
        }
    }
}
