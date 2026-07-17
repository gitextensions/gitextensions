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
using GitUI.Compat;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormRemotes.cs. Not ported: the PuTTY SSH panel and
// connection test (the twin relies on OpenSSH, per the platform matrix), the remote
// color button (needs a color picker; an existing color is preserved on save), the
// Active/Inactive list group headers (inactive remotes render gray instead), and the
// in-grid editing of the default pull behaviors (the detail fields below the list are
// the editing - and persistence - path, like the WinForms detail fields). The URL
// history combos hold path strings rather than Repository objects.
public sealed partial class FormRemotes : GitModuleForm
{
    private readonly FormRemotesController _formRemotesController = new();
    private IConfigFileRemoteSettingsManager? _remotesManager;
    private IGitBranchNameNormaliser _branchNameNormaliser = null!;
    private ConfigFileRemote? _selectedRemote;
    private IList<Repository>? _repositoryHistory;

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
    }

    public FormRemotes(IGitUICommands commands)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();

        _branchNameNormaliser = commands.GetRequiredService<IGitBranchNameNormaliser>();
        GitBranchNameOptions options = new(replacementToken: AppSettings.AutoNormaliseSymbol, allowTrailingSlash: true);
        txtRemotePrefix.LostFocus += (_, _) => txtRemotePrefix.Text = _branchNameNormaliser.Normalise(txtRemotePrefix.Text ?? string.Empty, options);

        ToolTip.SetTip(New, _btnNewTooltip.Text);
        ToolTip.SetTip(Delete, _btnDeleteTooltip.Text);
    }

    private void WireControls()
    {
        Remotes.ItemTemplate = new FuncDataTemplate<ConfigFileRemote>(
            (remote, _) =>
            {
                TextBlock item = new() { Text = remote?.Name ?? string.Empty };
                if (remote?.Disabled is true)
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

        // The WinForms list shows Active/Inactive groups; the twin orders the enabled
        // remotes first and renders the disabled ones gray.
        List<ConfigFileRemote> ordered = [.. UserGitRemotes.Where(r => !r.Disabled), .. UserGitRemotes.Where(r => r.Disabled)];
        Remotes.ItemsSource = ordered;

        if (UserGitRemotes.Count != 0)
        {
            ConfigFileRemote? preselected = !string.IsNullOrEmpty(preselectRemote)
                ? ordered.FirstOrDefault(r => r.Name == preselectRemote)
                : null;

            // default fallback - if the preselection didn't work select the first available one
            Remotes.SelectedItem = preselected ?? ordered[0];
            Remotes.Focus();
        }
        else
        {
            Delete.IsEnabled = false;
            btnToggleState.IsEnabled = false;
            RemoteName.Focus();
        }
    }

    private void BindBtnToggleState(bool disabled)
    {
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

        // The color button is not ported; keep an existing color.
        string? color = _selectedRemote?.Color;

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
        IGitRef? head = GetHeadForSelectedRemoteBranch();
        if (head is null)
        {
            return;
        }

        LocalBranchNameEdit.Text = head.Name;
        RemoteRepositoryCombo.SelectedItem = UserGitRemotes!.FirstOrDefault(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, head.TrackingRemote))?.Name;
        if (RemoteRepositoryCombo.SelectedItem is null)
        {
            RemoteRepositoryCombo.SelectedIndex = 0;
        }

        DefaultMergeWithCombo.Text = head.MergeWith;
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
    {
        IGitRef? head = GetHeadForSelectedRemoteBranch();
        if (head is null)
        {
            return;
        }

        head.TrackingRemote = RemoteRepositoryCombo.SelectedItem as string ?? string.Empty;
    }

    private void DefaultMergeWithComboValidated(object sender, EventArgs e)
    {
        IGitRef? head = GetHeadForSelectedRemoteBranch();
        if (head is null)
        {
            return;
        }

        head.MergeWith = DefaultMergeWithCombo.Text ?? string.Empty;
    }

    private void SaveDefaultPushPullClick(object sender, EventArgs e)
    {
        Initialize();
    }

    private void RemoteName_TextChanged(object sender, EventArgs e)
    {
        Save.IsEnabled = (RemoteName.Text ?? string.Empty).Trim().Length > 0;
    }

    private void Remotes_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (Remotes.SelectedItem is not null && _selectedRemote == Remotes.SelectedItem)
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

        if (Remotes.SelectedItem is null)
        {
            _selectedRemote = null;
            flpnlRemoteManagement.IsEnabled = true;
            return;
        }

        // reset all controls and disable all buttons until we have a selection
        _selectedRemote = Remotes.SelectedItem as ConfigFileRemote;
        if (_selectedRemote is null)
        {
            return;
        }

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

        label2.Text = AvaloniaTranslationUtils.ToAvaloniaMnemonics(visible
            ? _labelUrlAsFetch.Text
            : _labelUrlAsFetchPush.Text);
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

        // The WinForms columns of the default-pull grid store their captions as HeaderText.
        translation.AddTranslationItem(nameof(FormRemotes), nameof(BranchName), "HeaderText", "Local branch name");
        translation.AddTranslationItem(nameof(FormRemotes), nameof(RemoteCombo), "HeaderText", "Remote repository");
        translation.AddTranslationItem(nameof(FormRemotes), nameof(MergeWith), "HeaderText", "Default merge with");
    }

    public override void TranslateItems(GitExtensions.Extensibility.Translations.ITranslation translation)
    {
        base.TranslateItems(translation);

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
        public TextBox RemoteName => _form.RemoteName;
        public TextBox RemotePrefix => _form.txtRemotePrefix;
        public Button Save => _form.Save;
        public TabControl TabControl => _form.tabControl1;
        public Button ToggleState => _form.btnToggleState;
    }
}
