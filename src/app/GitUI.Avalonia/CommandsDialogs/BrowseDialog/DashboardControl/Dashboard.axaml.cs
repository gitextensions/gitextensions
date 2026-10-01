using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Compat;
using GitUI.Properties;
using GitUI.Theming;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

public partial class Dashboard : GitModuleControl
{
    private readonly TranslationString _cloneFork = new("Clone {0} repository");
    private readonly TranslationString _cloneRepository = new("Clone repository");
    private readonly TranslationString _createRepository = new("Create new repository");
    private readonly TranslationString _develop = new("Develop");
    private readonly TranslationString _donate = new("Donate");
    private readonly TranslationString _issues = new("Issues");
    private readonly TranslationString _openRepository = new("Open repository");
    private readonly TranslationString _translate = new("Translate");

    public event EventHandler<GitModuleEventArgs>? GitModuleChanged;

    public Dashboard()
    {
        InitializeComponent();
        ConfigureLink(createItem, createItem_Click);
        ConfigureLink(cloneItem, cloneItem_Click);
        ConfigureLink(openItem, openItem_Click);
        ConfigureLink(developItem, GitHubItem_Click);
        ConfigureLink(donateItem, DonateItem_Click);
        ConfigureLink(translateItem, TranslateItem_Click);
        ConfigureLink(issuesItem, IssuesItem_Click);
        AttachedToLogicalTree += dashboard_ParentChanged;
        DetachedFromLogicalTree += dashboard_ParentChanged;
        SizeChanged += (_, _) => tableLayoutPanel1.Height = Math.Max(Bounds.Height, tableLayoutPanel1.MinHeight);

        // Native UserControl.Focus selects its first input once the window is shown.
        Loaded += (_, _) =>
        {
            if (IsEffectivelyVisible)
            {
                OnVisibleChanged(EventArgs.Empty);
            }
        };
        IsVisible = false;
        InitializeComplete();

        // apply scaling
        userRepositoriesList.HeaderHeight = 68;

        static void ConfigureLink(Button linkLabel, EventHandler<RoutedEventArgs> handler)
            => linkLabel.Click += handler;
    }

    public void Initialize(IRepositoryHistoryUIService repositoryHistoryUIService)
        => Initialize(
            UICommands.GetRequiredService<IUserRepositoriesListController>(),
            repositoryHistoryUIService);

    internal void Initialize(
        IUserRepositoriesListController controller,
        IRepositoryHistoryUIService? repositoryHistoryUIService)
    {
        userRepositoriesList.Initialize(
            controller,
            repositoryHistoryUIService,
            () => UICommands);
        userRepositoriesList.GitModuleChanged += OnModuleChanged;
    }

    protected virtual void OnVisibleChanged(EventArgs e)
    {
        // Focus the control in order for the search bar to have focus once the dashboard is shown
        userRepositoriesList.Focus();
    }

    public void RefreshContent()
    {
        DashboardTheme selectedTheme = ThemeModule.Settings.Theme.SystemColorMode == GitExtensions.Shims.WinForms.SystemColorMode.Dark
            ? DashboardTheme.Dark : DashboardTheme.Light;

        createItem.Content = CreateLinkContent(Images.RepoCreate, _createRepository.Text);
        openItem.Content = CreateLinkContent(Images.RepoOpen, _openRepository.Text);
        cloneItem.Content = CreateLinkContent(Images.CloneRepoGit, _cloneRepository.Text);
        developItem.Content = CreateLinkContent(Images.Develop.AdaptLightness(), _develop.Text);
        donateItem.Content = CreateLinkContent(Images.DollarSign, _donate.Text);
        translateItem.Content = CreateLinkContent(Images.Translate.AdaptLightness(), _translate.Text);
        issuesItem.Content = CreateLinkContent(Images.Bug, _issues.Text);

        StackPanel startLinks = (StackPanel)flpnlStart.Child!;
        Button[] dynamicLinks = [.. startLinks.Children.OfType<Button>().Where(button => button.Tag is IRepositoryHostPlugin)];
        foreach (Button button in dynamicLinks)
        {
            startLinks.Children.Remove(button);
        }

        foreach (IRepositoryHostPlugin gitHoster in PluginRegistry.GitHosters)
        {
            // Avalonia uses the native button/access-key path for the original clickable LinkLabel.
            Button linkLabel = new()
            {
                Classes = { "dashboard-link" },
                Content = CreateLinkContent(Images.CloneRepoGitHub, string.Format(_cloneFork.Text, gitHoster.Name)),
                Tag = gitHoster,
            };
            linkLabel.Click += (repoSender, eventArgs) => UICommands.StartCloneForkFromHoster(this, gitHoster, GitModuleChanged);
            startLinks.Children.Add(linkLabel);
        }

        backgroundImage.Source = selectedTheme.BackgroundImage;
        pnlLogo.Background = new SolidColorBrush(selectedTheme.LogoBackColor);
        flpnlStart.Background = new SolidColorBrush(selectedTheme.StartBackColor);
        flpnlContribute.Background = new SolidColorBrush(selectedTheme.ContributeBackColor);
        Resources["DashboardLinkForegroundBrush"] = new SolidColorBrush(selectedTheme.PrimaryText);
        Resources["DashboardLinkHoverForegroundBrush"] = new SolidColorBrush(selectedTheme.AccentedText);
        lblContribute.Foreground = new SolidColorBrush(selectedTheme.SecondaryHeadingText);
        lblContribute.FontFamily = new FontFamily(AppSettings.Font.Name);
        lblContribute.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size + 5.5F);

        // Native LinkLabel AutoSize measures its text plus padding, not an extra image column.
        // Keep the source runtime minimum-height calculation so smaller hosts scroll instead
        // of silently clipping the last start or contribution command.
        Avalonia.Size heading = WinFormsTextMeasurer.MeasureTextRenderer(lblContribute, lblContribute.Text ?? string.Empty);
        lblContribute.Width = Math.Ceiling(heading.Width);
        lblContribute.Height = Math.Ceiling(heading.Height);
        double startHeight = SizeLinks(startLinks) + flpnlStart.Padding.Top + flpnlStart.Padding.Bottom;
        double contributionHeight = SizeLinks((StackPanel)flpnlContribute.Child!)
            + lblContribute.Height + lblContribute.Margin.Top + lblContribute.Margin.Bottom
            + flpnlContribute.Padding.Top + flpnlContribute.Padding.Bottom;
        flpnlStart.MinHeight = startHeight;
        flpnlContribute.Height = contributionHeight;
        tableLayoutPanel1.MinHeight = pnlLogo.Height + startHeight + contributionHeight;

        // Dock.Fill inside native AutoScroll uses the host/minimum height, not the background's preferred size.
        tableLayoutPanel1.Height = Math.Max(Bounds.Height, tableLayoutPanel1.MinHeight);
        userRepositoriesList.MainBackColor = AvaloniaThemeResources.ToMediaColor(
            AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, System.Drawing.KnownColor.Window));
        userRepositoriesList.BranchNameColor = selectedTheme.SecondaryText;
        userRepositoriesList.FavouriteColor = selectedTheme.AccentedText;
        userRepositoriesList.ForeColor = selectedTheme.PrimaryText;
        userRepositoriesList.HeaderColor = selectedTheme.SecondaryHeadingText;
        userRepositoriesList.HeaderBackColor = selectedTheme.HeaderBackColor;
        userRepositoriesList.HoverColor = selectedTheme.StartBackColor;
        userRepositoriesList.SearchBackColor = selectedTheme.SearchBackColor;
        Background = new SolidColorBrush(userRepositoriesList.MainBackColor);
        userRepositoriesList.ShowRecentRepositories(reloadData: false);

        return;

        static double SizeLinks(StackPanel panel)
        {
            double height = 0;
            Button[] links = [.. panel.Children.OfType<Button>()];
            foreach (Button link in links)
            {
                TextBlock text = ((Grid)link.Content!).Children.OfType<TextBlock>().Single();
                Avalonia.Size measured = WinFormsTextMeasurer.MeasureTextRenderer(text, text.Text ?? string.Empty);
                link.Width = Math.Ceiling(measured.Width) + link.Padding.Left + link.Padding.Right;
                link.Height = Math.Ceiling(measured.Height) + link.Padding.Top + link.Padding.Bottom;
                height += link.Height + link.Margin.Top + link.Margin.Bottom;
            }

            return height - (links.LastOrDefault()?.Margin.Bottom ?? 0);
        }
    }

    private static Control CreateLinkContent(IImage icon, string text)
        => new Grid
        {
            Children =
            {
                new Image
                {
                    Width = 16,
                    Height = 16,
                    Source = icon,
                    Margin = new Thickness(-24, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                new TextBlock
                {
                    Text = text,
                    FontFamily = new FontFamily(AppSettings.Font.Name),
                    FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size),
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };

    protected virtual void OnModuleChanged(object? sender, GitModuleEventArgs e)
    {
        EventHandler<GitModuleEventArgs>? handler = GitModuleChanged;
        handler?.Invoke(this, e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsVisibleProperty)
        {
            OnVisibleChanged(EventArgs.Empty);
        }
    }

    private void dashboard_ParentChanged(object sender, EventArgs e)
    {
        IsVisible = Parent is not null;
    }

    private static void TranslateItem_Click(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/wiki/Translations");
    }

    private static void GitHubItem_Click(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions");
    }

    private static void IssuesItem_Click(object? sender, EventArgs e)
    {
        UserEnvironmentInformation.CopyInformation();
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/issues");
    }

    private void openItem_Click(object? sender, EventArgs e)
    {
        IGitModule? module = FormOpenDirectory.OpenModule(this, UICommands.GetRequiredService<IGitExecutorProvider>(), currentModule: null);
        if (module is not null)
        {
            OnModuleChanged(this, new GitModuleEventArgs(module));
        }
    }

    private void cloneItem_Click(object? sender, EventArgs e)
    {
        UICommands.StartCloneDialog(this, null, false, OnModuleChanged);
    }

    private void createItem_Click(object? sender, EventArgs e)
    {
        UICommands.StartInitializeDialog(this, Module.WorkingDir, OnModuleChanged);
    }

    private static void DonateItem_Click(object? sender, EventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(FormDonate.DonationUrl);
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(Dashboard dashboard)
    {
        internal UserRepositoriesList Repositories => dashboard.userRepositoriesList;
        internal Button Open => dashboard.openItem;
    }
}
