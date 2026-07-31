using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl;

public partial class Dashboard : GitModuleControl
{
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
    {
        userRepositoriesList.Initialize(repositoryHistoryUIService, () => UICommands);
        userRepositoriesList.ConfigureRequested += (_, _) => ConfigureRepositoriesRequested?.Invoke(this, EventArgs.Empty);
        userRepositoriesList.GitModuleChanged += OnModuleChanged;
    }

    public void RefreshContent()
    {
        createItem.Content = CreateLinkContent(Images.RepoCreate, _createRepository.Text);
        openItem.Content = CreateLinkContent(Images.RepoOpen, _openRepository.Text);
        cloneItem.Content = CreateLinkContent(Images.CloneRepoGit, _cloneRepository.Text);
        developItem.Content = CreateLinkContent(Images.Develop.AdaptLightness(), _develop.Text);
        donateItem.Content = CreateLinkContent(Images.DollarSign, _donate.Text);
        translateItem.Content = CreateLinkContent(Images.Translate.AdaptLightness(), _translate.Text);
        issuesItem.Content = CreateLinkContent(Images.Bug, _issues.Text);
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
