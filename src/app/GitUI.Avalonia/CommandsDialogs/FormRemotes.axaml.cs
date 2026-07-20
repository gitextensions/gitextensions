using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Remotes;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Theming;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;
using MediaColor = Avalonia.Media.Color;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormRemotes.cs. The PuTTY SSH panel and connection test
// remain intentionally absent because the portable application uses OpenSSH. Active and
// inactive remotes retain the original grouping, remote colors use Avalonia's native color
// picker, and the original detail controls edit each branch's pull behavior. The URL history
// combos hold path strings rather than Repository objects.
public sealed partial class FormRemotes : GitModuleForm
{
    private const string BtnRemoteColorText = "Set &color";

    private sealed record RemoteListItem(string? Header, ConfigFileRemote? Remote);

    private readonly FormRemotesController _formRemotesController = new();
    private IConfigFileRemoteSettingsManager? _remotesManager;
    private IGitBranchNameNormaliser _branchNameNormaliser = null!;
    private ConfigFileRemote? _selectedRemote;
    private IGitRef? _selectedPullBehaviorHead;
    private IList<Repository>? _repositoryHistory;
    private object? _btnRemoteColorText;
    private MediaColor? _remoteColor;
    private bool _settingRemoteColor;
    private bool _updatingPullBehaviorControls;

    private string[] _genericRemotesNames = ["origin", "upstream", "fork", "remote", "internal", .. AppSettings.CustomGenericRemoteNames];

    #region Translation
    private readonly TranslationString _questionAutoPullBehaviour =
        new("You have added a new remote repository." + Environment.NewLine +
                              "Do you want to automatically configure the default push and pull behavior for this remote?");

    private readonly TranslationString _questionAutoPullBehaviourCaption =
        new("New remote");

    private readonly TranslationString _gitMessage =
      new("Message");

    private readonly TranslationString _questionDeleteRemote =
        new("Are you sure you want to delete this remote?");

    private readonly TranslationString _questionDeleteRemoteCaption =
        new("Delete");

    private readonly TranslationString _labelUrlAsFetch =
        new("Fetch &Url");

    private readonly TranslationString _labelUrlAsFetchPush =
        new("&Url");

    private readonly TranslationString _gbMgtPanelHeaderNew =
        new("Create New Remote");

    private readonly TranslationString _gbMgtPanelHeaderEdit =
        new("Edit Remote Details");

    private readonly TranslationString _btnDeleteTooltip =
        new("Delete the selected remote");

    private readonly TranslationString _btnNewTooltip =
        new("Add a new remote");

    private readonly TranslationString _btnToggleStateTooltip_Activate =
        new("Activate the selected remote");

    private readonly TranslationString _btnToggleStateTooltip_Deactivate =
        new(@"Deactivate the selected remote.
Inactive remote is completely invisible to git.");

    private readonly TranslationString _lvgEnabledHeader =
        new("Active");

    private readonly TranslationString _lvgDisabledHeader =
        new("Inactive");

    private readonly TranslationString _enabledRemoteAlreadyExists =
        new("An active remote named \"{0}\" already exists.");

    private readonly TranslationString _disabledRemoteAlreadyExists =
        new("An inactive remote named \"{0}\" already exists.");
    #endregion

    public FormRemotes()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
        _btnRemoteColorText = btnRemoteColor.Content;
    }

    public FormRemotes(IGitUICommands commands)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
        _btnRemoteColorText = btnRemoteColor.Content;

        _branchNameNormaliser = commands.GetRequiredService<IGitBranchNameNormaliser>();
        GitBranchNameOptions options = new(replacementToken: AppSettings.AutoNormaliseSymbol, allowTrailingSlash: true);
        txtRemotePrefix.LostFocus += (_, _) => txtRemotePrefix.Text = _branchNameNormaliser.Normalise(txtRemotePrefix.Text ?? string.Empty, options);

        ToolTip.SetTip(New, _btnNewTooltip.Text);
        ToolTip.SetTip(Delete, _btnDeleteTooltip.Text);
    }

    private void WireControls()
    {
        Remotes.ItemTemplate = new FuncDataTemplate<RemoteListItem>(
            (entry, _) =>
            {
                if (entry?.Remote is null)
                {
                    return new TextBlock
                    {
                        Text = entry?.Header ?? string.Empty,
                        FontWeight = FontWeight.SemiBold,
                        Margin = new Avalonia.Thickness(4, 4, 4, 2),
                    };
                }

                TextBlock item = new()
                {
                    Text = entry.Remote.Name ?? string.Empty,
                    Margin = new Avalonia.Thickness(12, 1, 4, 1),
                };
                if (entry.Remote.Disabled)
                {
                    item.Foreground = Brushes.Gray;
                }

                return item;
            },
            supportsRecycling: false);
        RemoteBranches.ItemTemplate = new FuncDataTemplate<IGitRef>(
            (head, _) => CreateRemoteBranchRow(head),
            supportsRecycling: false);

        Remotes.SelectionChanged += Remotes_SelectedIndexChanged;
        Remotes.ContainerPrepared += Remotes_ContainerPrepared;
        New.Click += NewClick;
        Delete.Click += DeleteClick;
        btnToggleState.Click += btnToggleState_Click;
        Save.Click += SaveClick;
        RemoteName.TextChanged += RemoteName_TextChanged;
        RemoteName.GotFocus += RemoteName_Enter;
        Url.GotFocus += Url_Enter;
        comboBoxPushUrl.GotFocus += ComboBoxPushUrl_Enter;
        checkBoxSepPushUrl.IsCheckedChanged += checkBoxSepPushUrl_CheckedChanged;
        folderBrowserButtonUrl.PathShowingControl = Url;
        folderBrowserButtonPushUrl.PathShowingControl = comboBoxPushUrl;
        btnRemoteColor.ColorChanged += btnRemoteColor_ColorChanged;
        btnRemoteColorReset.Click += btnRemoteColorReset_Click;

        RemoteBranches.SelectionChanged += RemoteBranchesSelectionChanged;
        RemoteRepositoryCombo.LostFocus += RemoteRepositoryComboValidated;
        DefaultMergeWithCombo.LostFocus += DefaultMergeWithComboValidated;
        DefaultMergeWithCombo.DropDownOpened += DefaultMergeWithComboDropDown;
        SaveDefaultPushPull.Click += SaveDefaultPushPullClick;
    }

    private static Control CreateRemoteBranchRow(IGitRef? head)
    {
        Grid row = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
        };
        TextBlock branchName = new() { Text = head?.LocalName ?? string.Empty, Margin = new Avalonia.Thickness(2) };
        TextBlock trackingRemote = new() { Text = head?.TrackingRemote ?? string.Empty, Margin = new Avalonia.Thickness(2) };
        TextBlock mergeWith = new() { Text = head?.MergeWith ?? string.Empty, Margin = new Avalonia.Thickness(2) };
        Grid.SetColumn(trackingRemote, 1);
        Grid.SetColumn(mergeWith, 2);
        row.Children.Add(branchName);
        row.Children.Add(trackingRemote);
        row.Children.Add(mergeWith);
        return row;
    }

    /// <summary>
    /// If this is not null before showing the dialog the given
    /// remote name will be preselected in the listbox.
    /// </summary>
    /// <remarks>exclusive of <see cref="PreselectLocalOnLoad"/>.</remarks>
    public string? PreselectRemoteOnLoad { get; set; }

    /// <summary>
    /// If this is not null before showing the dialog tab "Default push behavior" is opened
    /// and the given local name will be preselected in the listbox.
    /// </summary>
    /// <remarks>exclusive of <see cref="PreselectRemoteOnLoad"/>.</remarks>
    public string? PreselectLocalOnLoad { get; set; }

    /// <summary>
    /// Gets the list of remotes configured in .git/config file.
    /// </summary>
    private List<ConfigFileRemote>? UserGitRemotes { get; set; }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        // make sure only single load option is given
        if (PreselectRemoteOnLoad is not null && PreselectLocalOnLoad is not null)
        {
            throw new ArgumentException($"Only one option allowed:" +
                $" Either {nameof(PreselectRemoteOnLoad)} or {nameof(PreselectLocalOnLoad)}");
        }

        if (!AppSettings.AlwaysShowAdvOpt)
        {
            lblRemoteColor.IsVisible = false;
            flpnlRemoteColors.IsVisible = false;
            lblRemotePrefix.IsVisible = false;
            txtRemotePrefix.IsVisible = false;
        }

        _remotesManager = new ConfigFileRemoteSettingsManager(() => Module);

        // load the data for the very first time
        Initialize(PreselectRemoteOnLoad, PreselectLocalOnLoad);
    }

    private void Url_Enter(object sender, EventArgs e)
        => FillWithSomeGeneratedRemoteUrls(Url, r => r.Url!);

    private void ComboBoxPushUrl_Enter(object sender, EventArgs e)
        => FillWithSomeGeneratedRemoteUrls(comboBoxPushUrl, r => r.PushUrl!);

    private void BindRemotes(string? preselectRemote)
    {
        Validates.NotNull(UserGitRemotes);

        List<RemoteListItem> items = [];
        ConfigFileRemote[] enabled = [.. UserGitRemotes.Where(remote => !remote.Disabled)];
        ConfigFileRemote[] disabled = [.. UserGitRemotes.Where(remote => remote.Disabled)];
        if (enabled.Length > 0)
        {
            items.Add(new RemoteListItem(_lvgEnabledHeader.Text, Remote: null));
            items.AddRange(enabled.Select(remote => new RemoteListItem(Header: null, Remote: remote)));
        }

        if (disabled.Length > 0)
        {
            items.Add(new RemoteListItem(_lvgDisabledHeader.Text, Remote: null));
            items.AddRange(disabled.Select(remote => new RemoteListItem(Header: null, Remote: remote)));
        }

        Remotes.ItemsSource = items;

        if (UserGitRemotes.Count != 0)
        {
            RemoteListItem? preselected = !string.IsNullOrEmpty(preselectRemote)
                ? items.FirstOrDefault(item => item.Remote?.Name == preselectRemote)
                : null;

            // default fallback - if the preselection didn't work select the first available one
            Remotes.SelectedItem = preselected ?? items.First(item => item.Remote is not null);
            Remotes.Focus();
        }
        else
        {
            Delete.IsEnabled = false;
            btnToggleState.IsEnabled = false;
            RemoteName.Focus();
        }
    }

    private void Remotes_ContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        bool isHeader = e.Index >= 0
            && e.Index < Remotes.ItemCount
            && Remotes.Items[e.Index] is RemoteListItem { Remote: null };
        e.Container.IsEnabled = !isHeader;
        e.Container.Focusable = !isHeader;
        e.Container.IsHitTestVisible = !isHeader;
    }

    private void BindBtnToggleState(bool disabled)
    {
        btnToggleState.Icon = disabled
            ? Properties.Images.EyeOpened
            : Properties.Images.EyeClosed;
        ToolTip.SetTip(btnToggleState, disabled
            ? (_btnToggleStateTooltip_Activate.Text ?? "").Trim()
            : (_btnToggleStateTooltip_Deactivate.Text ?? "").Trim());
    }

    private IGitRef? GetHeadForSelectedRemoteBranch()
        => RemoteBranches.SelectedItem as IGitRef;

    private void Initialize(string? preselectRemote = null, string? preselectLocal = null)
    {
        Validates.NotNull(_remotesManager);

        // refresh registered git remotes
        UserGitRemotes = [.. _remotesManager.LoadRemotes(true)];

        InitialiseTabRemotes(preselectRemote);

        if (preselectLocal is not null && UserGitRemotes.Count != 0)
        {
            ActivateTabDefaultPullBehaviors();
        }

        InitialiseTabDefaultPullBehaviors(preselectLocal);
    }

    private void ActivateTabDefaultPullBehaviors()
    {
        tabControl1.SelectedItem = tabPage2;
    }

    private void InitialiseTabRemotes(string? preselectRemote = null)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync);

        _repositoryHistory = repositoryHistory;
        Url.ItemsSource = repositoryHistory.Select(repository => repository.Path).ToList();
        Url.SelectedItem = null;

        comboBoxPushUrl.ItemsSource = repositoryHistory.Select(repository => repository.Path).ToList();
        comboBoxPushUrl.SelectedItem = null;

        BindRemotes(preselectRemote);
    }

    private void InitialiseTabDefaultPullBehaviors(string? preselectLocal = null)
    {
        List<IGitRef> heads = [.. Module.GetRefs(RefsFilter.Heads).OrderBy(r => r.LocalName)];

        RemoteRepositoryCombo.ItemsSource = new[] { string.Empty }.Concat(UserGitRemotes!.Select(remote => remote.Name ?? string.Empty)).ToList();

        RemoteBranches.SelectionChanged -= RemoteBranchesSelectionChanged;
        _selectedPullBehaviorHead = null;
        RemoteBranches.ItemsSource = heads;
        RemoteBranches.SelectedItem = null;
        RemoteBranches.SelectionChanged += RemoteBranchesSelectionChanged;

        IGitRef? preselectLocalRef = heads.FirstOrDefault(r => r.LocalName == preselectLocal);
        if (preselectLocalRef is not null)
        {
            RemoteBranches.SelectedItem = preselectLocalRef;
        }
        else if (heads.Count > 0)
        {
            RemoteBranches.SelectedItem = heads[0];
        }
    }

    private void SetRemoteColor(MediaColor? color)
    {
        _settingRemoteColor = true;
        try
        {
            _remoteColor = color;
            MediaColor pickerColor = color ?? GetDefaultRemoteColor();
            btnRemoteColor.Color = pickerColor;
            btnRemoteColor.Content = color is null
                ? _btnRemoteColorText
                : new Border
                {
                    Width = 32,
                    Height = 14,
                    Background = new SolidColorBrush(pickerColor),
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Avalonia.Thickness(1),
                };
            btnRemoteColorReset.IsVisible = color is not null;
        }
        finally
        {
            _settingRemoteColor = false;
        }
    }

    private static MediaColor GetDefaultRemoteColor()
    {
        System.Drawing.Color color = AppColor.RemoteBranch.GetThemeColor();
        return MediaColor.FromArgb(color.A, color.R, color.G, color.B);
    }

    private void btnRemoteColor_ColorChanged(object? sender, ColorChangedEventArgs e)
    {
        if (!_settingRemoteColor)
        {
            SetRemoteColor(e.NewColor);
        }
    }

    private void btnRemoteColorReset_Click(object? sender, EventArgs e)
        => SetRemoteColor(color: null);

    private void btnToggleState_Click(object sender, EventArgs e)
    {
        if (_selectedRemote is null)
        {
            btnToggleState.IsVisible = false;
            return;
        }

        Validates.NotNull(_remotesManager);

        _selectedRemote.Disabled = !_selectedRemote.Disabled;

        Validates.NotNull(_selectedRemote.Name);

        _remotesManager.ToggleRemoteState(_selectedRemote.Name, _selectedRemote.Disabled);

        Initialize(_selectedRemote.Name);
    }

    private bool ValidateRemoteDoesNotExist(string remote)
    {
        Validates.NotNull(_remotesManager);

        if (_remotesManager.EnabledRemoteExists(remote))
        {
            MessageBoxes.Show(this, string.Format(_enabledRemoteAlreadyExists.Text, remote), _gitMessage.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Exclamation);

            return false;
        }

        if (_remotesManager.DisabledRemoteExists(remote))
        {
            MessageBoxes.Show(this, string.Format(_disabledRemoteAlreadyExists.Text, remote), _gitMessage.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Exclamation);

            return false;
        }

        return true;
    }

    private void SaveClick(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RemoteName.Text))
        {
            return;
        }

        string remote = RemoteName.Text.Trim();
        string remoteUrl = (Url.Text ?? string.Empty).Trim();
        string remotePushUrl = (comboBoxPushUrl.Text ?? string.Empty).Trim();
        bool creatingNew = _selectedRemote is null;
        string remotePrefix = txtRemotePrefix.Text ?? string.Empty;

        string? color = _remoteColor is MediaColor remoteColor
            ? $"#{remoteColor.R:X2}{remoteColor.G:X2}{remoteColor.B:X2}"
            : null;

        try
        {
            // disable the control while saving
            tabControl1.IsEnabled = false;

            if ((string.IsNullOrEmpty(remotePushUrl) && checkBoxSepPushUrl.IsChecked == true) ||
                (!string.IsNullOrEmpty(remotePushUrl) && remotePushUrl.Equals(remoteUrl, StringComparison.OrdinalIgnoreCase)))
            {
                checkBoxSepPushUrl.IsChecked = false;
            }

            if (creatingNew && !ValidateRemoteDoesNotExist(remote))
            {
                return;
            }

            Validates.NotNull(_remotesManager);

            // update all other remote properties
            ConfigFileRemoteSaveResult result = _remotesManager.SaveRemote(_selectedRemote,
                                                   remote,
                                                   remoteUrl,
                                                   checkBoxSepPushUrl.IsChecked == true ? remotePushUrl : null,
                                                   _selectedRemote?.PuttySshKey ?? string.Empty,
                                                   color,
                                                   remotePrefix);

            if (!string.IsNullOrEmpty(result.UserMessage))
            {
                MessageBoxes.Show(this, result.UserMessage, _gitMessage.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                // This will cause the module's remotes colors to reload
                Module.ResetRemoteColors();

                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    IList<Repository> repositoryHistory = await RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync();

                    // The JTF-integrated switch: the Avalonia dispatcher is not pumped
                    // while JoinableTaskFactory.Run blocks this (the main) thread.
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    _formRemotesController.RemoteUpdate(repositoryHistory, _selectedRemote?.Url, remoteUrl);
                    if (checkBoxSepPushUrl.IsChecked == true)
                    {
                        _formRemotesController.RemoteUpdate(repositoryHistory, _selectedRemote?.PushUrl, remotePushUrl);
                    }

                    await RepositoryHistoryManager.Remotes.SaveRecentHistoryAsync(repositoryHistory);
                });
            }

            // if the user has just created a fresh new remote
            // there may be a need to configure it
            if (result.ShouldUpdateRemote &&
                !string.IsNullOrEmpty(remoteUrl) &&
                MessageBoxes.Show(this,
                    _questionAutoPullBehaviour.Text,
                    _questionAutoPullBehaviourCaption.Text,
                    WinFormsShims.MessageBoxButtons.YesNo,
                    WinFormsShims.MessageBoxIcon.Question) == WinFormsShims.DialogResult.Yes)
            {
                UICommands.StartPullDialogAndPullImmediately(
                    remote: remote,
                    pullAction: GitPullAction.Fetch);
                _remotesManager.ConfigureRemotes(remote);
                UICommands.RepoChangedNotifier.Notify();
            }
        }
        finally
        {
            // re-enable the control and re-initialize
            tabControl1.IsEnabled = true;
            Initialize(remote);
        }
    }

    private void NewClick(object sender, EventArgs e)
    {
        Remotes.SelectedItem = null;
        RemoteName.Focus();
    }

    private void DeleteClick(object sender, EventArgs e)
    {
        if (_selectedRemote is null)
        {
            return;
        }

        if (MessageBoxes.Show(this,
                            _questionDeleteRemote.Text,
                            _questionDeleteRemoteCaption.Text,
                            WinFormsShims.MessageBoxButtons.YesNo,
                            WinFormsShims.MessageBoxIcon.Warning,
                            WinFormsShims.MessageBoxDefaultButton.Button2) == WinFormsShims.DialogResult.Yes)
        {
            Validates.NotNull(_remotesManager);

            string output = _remotesManager.RemoveRemote(_selectedRemote);
            if (!string.IsNullOrEmpty(output))
            {
                MessageBoxes.Show(this, output, _gitMessage.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Information);
            }

            // Deleting a remote from the history list may be undesirable as
            // it would hinder user's ability to *quickly* clone the remote repository
            // The flipside is that the history list may grow long without a UI to manage it

            Initialize();
        }
    }

    private void RemoteBranchesSelectionChanged(object? sender, EventArgs e)
    {
        CommitPullBehaviorEdits();

        IGitRef? head = GetHeadForSelectedRemoteBranch();
        if (head is null)
        {
            _selectedPullBehaviorHead = null;
            return;
        }

        _selectedPullBehaviorHead = head;
        _updatingPullBehaviorControls = true;
        try
        {
            LocalBranchNameEdit.Text = head.Name;
            RemoteRepositoryCombo.SelectedItem = UserGitRemotes!.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, head.TrackingRemote))?.Name;
            if (RemoteRepositoryCombo.SelectedItem is null)
            {
                RemoteRepositoryCombo.SelectedIndex = 0;
            }

            DefaultMergeWithCombo.Text = head.MergeWith;
        }
        finally
        {
            _updatingPullBehaviorControls = false;
        }
    }

    private void DefaultMergeWithComboDropDown(object sender, EventArgs e)
    {
        IGitRef? head = GetHeadForSelectedRemoteBranch();
        if (head is null)
        {
            return;
        }

        List<string> items = [""];

        string currentSelectedRemote = (RemoteRepositoryCombo.SelectedItem as string ?? string.Empty).Trim();

        if (!string.IsNullOrEmpty(head.TrackingRemote) && !string.IsNullOrEmpty(currentSelectedRemote))
        {
            string remoteUrl = Module.GetSetting(string.Format(SettingKeyString.RemoteUrl, currentSelectedRemote));
            if (!string.IsNullOrEmpty(remoteUrl))
            {
                foreach (IGitRef remoteHead in Module.GetRefs(RefsFilter.Remotes))
                {
                    if (remoteHead.Name.ToLower().Contains(currentSelectedRemote.ToLower()))
                    {
                        items.Add(remoteHead.LocalName);
                    }
                }
            }
        }

        DefaultMergeWithCombo.ItemsSource = items;
    }

    private void RemoteRepositoryComboValidated(object sender, EventArgs e)
        => CommitPullBehaviorEdits();

    private void DefaultMergeWithComboValidated(object sender, EventArgs e)
        => CommitPullBehaviorEdits();

    private void CommitPullBehaviorEdits()
    {
        if (_updatingPullBehaviorControls || _selectedPullBehaviorHead is null)
        {
            return;
        }

        _selectedPullBehaviorHead.TrackingRemote = RemoteRepositoryCombo.SelectedItem as string ?? string.Empty;
        _selectedPullBehaviorHead.MergeWith = DefaultMergeWithCombo.Text ?? string.Empty;
    }

    private void SaveDefaultPushPullClick(object sender, EventArgs e)
    {
        CommitPullBehaviorEdits();
        Initialize();
    }

    private void RemoteName_TextChanged(object sender, EventArgs e)
    {
        Save.IsEnabled = (RemoteName.Text ?? string.Empty).Trim().Length > 0;
    }

    private void Remotes_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ConfigFileRemote? selectedRemote = (Remotes.SelectedItem as RemoteListItem)?.Remote;
        if (selectedRemote is not null && _selectedRemote == selectedRemote)
        {
            return;
        }

        Delete.IsEnabled = btnToggleState.IsEnabled = false;
        RemoteName.Text = string.Empty;
        Url.Text = string.Empty;
        comboBoxPushUrl.Text = string.Empty;
        checkBoxSepPushUrl.IsChecked = false;
        gbMgtPanel.Header = _gbMgtPanelHeaderNew.Text;
        txtRemotePrefix.Text = string.Empty;
        SetRemoteColor(color: null);

        if (selectedRemote is null)
        {
            _selectedRemote = null;
            flpnlRemoteManagement.IsEnabled = true;
            return;
        }

        // reset all controls and disable all buttons until we have a selection
        _selectedRemote = selectedRemote;

        Delete.IsEnabled = btnToggleState.IsEnabled = true;
        RemoteName.Text = _selectedRemote.Name;
        Url.Text = _selectedRemote.Url;
        comboBoxPushUrl.Text = _selectedRemote.PushUrl;
        checkBoxSepPushUrl.IsChecked = !string.IsNullOrEmpty(_selectedRemote.PushUrl);
        gbMgtPanel.Header = _gbMgtPanelHeaderEdit.Text;
        BindBtnToggleState(_selectedRemote.Disabled);
        btnToggleState.IsVisible = true;
        flpnlRemoteManagement.IsEnabled = !_selectedRemote.Disabled;
        txtRemotePrefix.Text = _selectedRemote.Prefix;

        SetRemoteColor(MediaColor.TryParse(_selectedRemote.Color, out MediaColor color)
            ? color
            : null);
    }

    private void checkBoxSepPushUrl_CheckedChanged(object sender, EventArgs e)
    {
        ShowSeparatePushUrl(checkBoxSepPushUrl.IsChecked == true);
    }

    private void ShowSeparatePushUrl(bool visible)
    {
        labelPushUrl.IsVisible = visible;
        comboBoxPushUrl.IsVisible = visible;
        folderBrowserButtonPushUrl.IsVisible = visible;

        label2.Text = AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(
            AvaloniaTranslationUtils.ToAvaloniaMnemonics(visible
                ? _labelUrlAsFetch.Text
                : _labelUrlAsFetchPush.Text));
    }

    private void FillWithSomeGeneratedRemoteUrls(ComboBox combobox, Func<ConfigFileRemote, string> urlGetter)
    {
        string remoteName = RemoteName.Text ?? string.Empty;
        bool fillEmptyUrl = true;

        if (string.IsNullOrWhiteSpace(remoteName) || _genericRemotesNames.Contains(remoteName))
        {
            remoteName = "TO_REPLACE";
            fillEmptyUrl = false;
        }

        if (UserGitRemotes?.Count != 0)
        {
            HashSet<string> candidates = new(UserGitRemotes!.Count);

            GitHostingRemoteParser gitHostingRemoteParser = new();
            foreach (ConfigFileRemote remote in UserGitRemotes)
            {
                string url = urlGetter(remote);

                if (string.IsNullOrEmpty(url))
                {
                    continue;
                }

                // Simple replace tentative
                if (url.Contains(remote.Name!))
                {
                    candidates.Add(urlGetter(remote).Replace($"{remote.Name}/", $"{remoteName}/"));
                }

                // Extract from "known" git hosting pattern
                if (gitHostingRemoteParser.TryExtractGitHostingDataFromRemoteUrl(remote.Url!, out _, out string? owner, out _))
                {
                    candidates.Add(url.Replace($"{owner}/", $"{remoteName}/"));
                }
            }

            if (candidates.Count > 0)
            {
                string previousValues = combobox.Text ?? string.Empty;
                List<string> proposedRepositories = [.. _repositoryHistory!.Select(r => r.Path)];
                bool added = false;
                foreach (string url in candidates)
                {
                    if (!proposedRepositories.Any(r => r == url))
                    {
                        added = true;
                        proposedRepositories.Insert(0, url);
                    }
                }

                if (added)
                {
                    // if there is a previous value, keep it
                    if (!string.IsNullOrEmpty(previousValues))
                    {
                        proposedRepositories.Insert(0, previousValues);
                    }

                    combobox.ItemsSource = proposedRepositories;

                    // Don't auto select a value when generic remote name entered or more than 1 result added.
                    // (The WinForms DataSource binding auto-selects the first item otherwise.)
                    combobox.Text = string.IsNullOrEmpty(previousValues) && (!fillEmptyUrl || candidates.Count > 1)
                        ? string.Empty
                        : proposedRepositories[0];
                }
            }
        }
    }

    private void RemoteName_Enter(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(RemoteName.Text) || string.IsNullOrEmpty(Url.Text))
        {
            return;
        }

        GitHostingRemoteParser gitHostingRemoteParser = new();
        if (gitHostingRemoteParser.TryExtractGitHostingDataFromRemoteUrl(Url.Text, out _, out string? owner, out _))
        {
            RemoteName.Text = owner;
            RemoteName.SelectAll();
        }
    }

    public override void AddTranslationItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(FormRemotes), nameof(btnRemoteColor), "Text", BtnRemoteColorText);

        // The WinForms columns of the default-pull grid store their captions as HeaderText.
        translation.AddTranslationItem(nameof(FormRemotes), nameof(BranchName), "HeaderText", "Local branch name");
        translation.AddTranslationItem(nameof(FormRemotes), nameof(RemoteCombo), "HeaderText", "Remote repository");
        translation.AddTranslationItem(nameof(FormRemotes), nameof(MergeWith), "HeaderText", "Default merge with");
    }

    public override void TranslateItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.TranslateItems(translation);

        string? remoteColorText = translation.TranslateItem(
            nameof(FormRemotes),
            nameof(btnRemoteColor),
            "Text",
            () => BtnRemoteColorText);
        if (!string.IsNullOrEmpty(remoteColorText))
        {
            _btnRemoteColorText = AvaloniaTranslationUtils.ToAvaloniaMnemonics(remoteColorText);
            if (_remoteColor is null)
            {
                btnRemoteColor.Content = _btnRemoteColorText;
            }
        }

        TranslateHeader(translation, nameof(BranchName), BranchName, "Local branch name");
        TranslateHeader(translation, nameof(RemoteCombo), RemoteCombo, "Remote repository");
        TranslateHeader(translation, nameof(MergeWith), MergeWith, "Default merge with");
    }

    private static void TranslateHeader(GitExtensions.Extensibility.Translations.ITranslation translation, string fieldName, Border header, string defaultText)
    {
        string? text = translation.TranslateItem(nameof(FormRemotes), fieldName, "HeaderText", () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FormRemotes _form;

        public TestAccessor(FormRemotes form)
        {
            _form = form;
        }

        public Button Delete => _form.Delete;
        public ColorPicker RemoteColor => _form.btnRemoteColor;
        public Button RemoteColorReset => _form.btnRemoteColorReset;
        public TextBox RemoteName => _form.RemoteName;
        public TextBox RemotePrefix => _form.txtRemotePrefix;
        public int RemoteCount => _form.Remotes.Items.OfType<RemoteListItem>().Count(item => item.Remote is not null);
        public IReadOnlyList<string> RemoteGroupHeaders => [.. _form.Remotes.Items
            .OfType<RemoteListItem>()
            .Where(item => item.Header is not null)
            .Select(item => item.Header!)];
        public Button Save => _form.Save;
        public TabControl TabControl => _form.tabControl1;
        public Button ToggleState => _form.btnToggleState;
    }
}
