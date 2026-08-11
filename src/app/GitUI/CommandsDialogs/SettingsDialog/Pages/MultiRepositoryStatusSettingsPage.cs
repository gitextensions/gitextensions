using GitCommands;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

internal sealed partial class MultiRepositoryStatusSettingsPage : SettingsPageWithHeader
{
    private readonly CheckBox _autoFetchEnabled = new()
    {
        AutoSize = true,
        Text = "系统空闲时自动 Fetch 收藏仓库"
    };

    private readonly NumericUpDown _idleMinutes = CreateNumberControl(1, 1440);
    private readonly NumericUpDown _fetchIntervalMinutes = CreateNumberControl(1, 1440);
    private readonly NumericUpDown _concurrency = CreateNumberControl(1, 16);
    private readonly NumericUpDown _timeoutSeconds = CreateNumberControl(10, 3600);
    private readonly TableLayoutPanel _settingsTable = new();

    public MultiRepositoryStatusSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        Text = "仓库状态总览";
        AutoScroll = true;
        Dock = DockStyle.Fill;
        Padding = new Padding(12);

        Label explanation = new()
        {
            AutoSize = true,
            MaximumSize = new Size(700, 0),
            Text = "总览仅检查收藏仓库。自动和手动 Fetch 都会访问每个仓库配置的全部远端。"
        };

        _settingsTable.AutoSize = true;
        _settingsTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _settingsTable.ColumnCount = 2;
        _settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _settingsTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _settingsTable.Dock = DockStyle.Top;
        _settingsTable.Padding = new Padding(0, 12, 0, 0);
        _settingsTable.Controls.Add(_autoFetchEnabled, 0, 0);
        _settingsTable.SetColumnSpan(_autoFetchEnabled, 2);
        AddSettingRow(1, "系统空闲多少分钟后开始", _idleMinutes);
        AddSettingRow(2, "持续空闲时每隔多少分钟执行", _fetchIntervalMinutes);
        AddSettingRow(3, "最大并发仓库数", _concurrency);
        AddSettingRow(4, "单仓库 Fetch 超时秒数", _timeoutSeconds);

        Controls.Add(_settingsTable);
        Controls.Add(explanation);
        _autoFetchEnabled.CheckedChanged += (_, _) => UpdateEnabledState();
        InitializeComplete();
    }

    protected override void SettingsToPage()
    {
        _autoFetchEnabled.Checked = AppSettings.MultiRepositoryStatusAutoFetchEnabled;
        _idleMinutes.Value = Math.Clamp(AppSettings.MultiRepositoryStatusIdleMinutes, (int)_idleMinutes.Minimum, (int)_idleMinutes.Maximum);
        _fetchIntervalMinutes.Value = Math.Clamp(AppSettings.MultiRepositoryStatusFetchIntervalMinutes, (int)_fetchIntervalMinutes.Minimum, (int)_fetchIntervalMinutes.Maximum);
        _concurrency.Value = Math.Clamp(AppSettings.MultiRepositoryStatusFetchConcurrency, (int)_concurrency.Minimum, (int)_concurrency.Maximum);
        _timeoutSeconds.Value = Math.Clamp(AppSettings.MultiRepositoryStatusFetchTimeoutSeconds, (int)_timeoutSeconds.Minimum, (int)_timeoutSeconds.Maximum);
        UpdateEnabledState();
        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.MultiRepositoryStatusAutoFetchEnabled = _autoFetchEnabled.Checked;
        AppSettings.MultiRepositoryStatusIdleMinutes = (int)_idleMinutes.Value;
        AppSettings.MultiRepositoryStatusFetchIntervalMinutes = (int)_fetchIntervalMinutes.Value;
        AppSettings.MultiRepositoryStatusFetchConcurrency = (int)_concurrency.Value;
        AppSettings.MultiRepositoryStatusFetchTimeoutSeconds = (int)_timeoutSeconds.Value;
        base.PageToSettings();
    }

    private static NumericUpDown CreateNumberControl(decimal minimum, decimal maximum)
        => new()
        {
            Minimum = minimum,
            Maximum = maximum,
            Width = 90,
            TextAlign = HorizontalAlignment.Right
        };

    private void AddSettingRow(int row, string text, Control control)
    {
        _settingsTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _settingsTable.Controls.Add(new Label
        {
            AutoSize = true,
            Margin = new Padding(24, 8, 12, 3),
            Text = text
        }, 0, row);
        control.Margin = new Padding(3, 4, 3, 3);
        _settingsTable.Controls.Add(control, 1, row);
    }

    private void UpdateEnabledState()
    {
        _idleMinutes.Enabled = _autoFetchEnabled.Checked;
        _fetchIntervalMinutes.Enabled = _autoFetchEnabled.Checked;
    }
}
