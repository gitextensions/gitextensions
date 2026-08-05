using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
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

    public Dashboard()
    {
        InitializeComponent();
        createItem.Click += (_, _) => UICommands.StartInitializeDialog(this, Module.WorkingDir, OnModuleChanged);
        cloneItem.Click += (_, _) => UICommands.StartCloneDialog(this, null, false, OnModuleChanged);
        openItem.Click += (_, _) => OpenRepositoryRequested?.Invoke(this, EventArgs.Empty);
        developItem.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser("https://github.com/gitextensions/gitextensions");
        donateItem.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser("https://opencollective.com/gitextensions");
        translateItem.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser("https://github.com/gitextensions/gitextensions/wiki/Translations");
        issuesItem.Click += (_, _) =>
        {
            UserEnvironmentInformation.CopyInformation();
            OsShellUtil.OpenUrlInDefaultBrowser("https://github.com/gitextensions/gitextensions/issues");
        };
        InitializeComplete();
    }

    public event EventHandler<GitModuleEventArgs>? GitModuleChanged;
    public event EventHandler? ConfigureRepositoriesRequested;
    public event EventHandler? OpenRepositoryRequested;

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
        userRepositoriesList.ConfigureRequested += (_, _) => ConfigureRepositoriesRequested?.Invoke(this, EventArgs.Empty);
        userRepositoriesList.GitModuleChanged += OnModuleChanged;
    }

    public void RefreshContent()
    {
        DashboardTheme selectedTheme = ActualThemeVariant == ThemeVariant.Dark ? DashboardTheme.Dark : DashboardTheme.Light;

        createItem.Content = CreateLinkContent(Images.RepoCreate, _createRepository.Text);
        openItem.Content = CreateLinkContent(Images.RepoOpen, _openRepository.Text);
        cloneItem.Content = CreateLinkContent(Images.CloneRepoGit, _cloneRepository.Text);
        developItem.Content = CreateLinkContent(Images.Develop.AdaptLightness(), _develop.Text);
        donateItem.Content = CreateLinkContent(Images.DollarSign, _donate.Text);
        translateItem.Content = CreateLinkContent(Images.Translate.AdaptLightness(), _translate.Text);
        issuesItem.Content = CreateLinkContent(Images.Bug, _issues.Text);

        Button[] dynamicLinks = [.. flpnlStart.Children.OfType<Button>().Where(button => button.Tag is IRepositoryHostPlugin)];
        foreach (Button button in dynamicLinks)
        {
            flpnlStart.Children.Remove(button);
        }

        foreach (IRepositoryHostPlugin gitHoster in PluginRegistry.GitHosters)
        {
            // Avalonia uses the native button/access-key path for the original clickable LinkLabel.
            Button button = new()
            {
                Classes = { "dashboard-link" },
                Content = CreateLinkContent(Images.CloneRepoGitHub, string.Format(_cloneFork.Text, gitHoster.Name)),
                Tag = gitHoster,
            };
            button.Click += (_, _) => UICommands.StartCloneForkFromHoster(this, gitHoster, GitModuleChanged);
            flpnlStart.Children.Add(button);
        }

        backgroundImage.Source = selectedTheme.BackgroundImage;
        pnlLogo.Background = new SolidColorBrush(selectedTheme.LogoBackColor);
        pnlStart.Background = new SolidColorBrush(selectedTheme.StartBackColor);
        pnlContribute.Background = new SolidColorBrush(selectedTheme.ContributeBackColor);
        lblContribute.Foreground = new SolidColorBrush(selectedTheme.SecondaryHeadingText);
        userRepositoriesList.MainBackColor = AvaloniaThemeResources.ToMediaColor(
            AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, System.Drawing.KnownColor.Window));
        userRepositoriesList.BranchNameColor = selectedTheme.SecondaryText;
        userRepositoriesList.FavouriteColor = selectedTheme.AccentedText;
        userRepositoriesList.ForeColor = selectedTheme.PrimaryText;
        userRepositoriesList.HeaderColor = selectedTheme.SecondaryHeadingText;
        userRepositoriesList.HeaderBackColor = selectedTheme.HeaderBackColor;
        userRepositoriesList.HoverColor = selectedTheme.StartBackColor;
        userRepositoriesList.SearchBackColor = selectedTheme.SearchBackColor;
        userRepositoriesList.ShowRecentRepositories(reloadData: false);
    }

    private static Control CreateLinkContent(IImage icon, string text)
        => new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 7,
            Children =
            {
                new Image
                {
                    Width = 18,
                    Height = 18,
                    Source = icon,
                },
                new TextBlock
                {
                    Text = text,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            },
        };

    private void OnModuleChanged(object? sender, GitModuleEventArgs e)
        => GitModuleChanged?.Invoke(this, e);

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(Dashboard dashboard)
    {
        internal UserRepositoriesList Repositories => dashboard.userRepositoriesList;
        internal Button Open => dashboard.openItem;
    }
}
