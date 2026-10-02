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
using LinkLabel = GitUI.Compat.WinFormsControls.LinkLabel;

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
        AttachedToLogicalTree += dashboard_ParentChanged;
        DetachedFromLogicalTree += dashboard_ParentChanged;
        SizeChanged += (_, _) => tableLayoutPanel1.Height = Math.Max(Bounds.Height, tableLayoutPanel1.MinHeight);
        tableLayoutPanel1.SizeChanged += (_, _) =>
        {
            // Native TableLayoutPanel truncates each percentage column independently.
            // Avalonia Grid retains fractions; allocate from the current client width instead
            // of rounding captured pixels or changing the authored percentages.
            double remainingWidth = Math.Max(0, tableLayoutPanel1.Bounds.Width - 213);
            double outsideWidth = Math.Floor(remainingWidth * 7.142857 / 100);
            double repositoryWidth = Math.Floor(remainingWidth * 85.71428 / 100);
            tableLayoutPanel1.ColumnDefinitions[0].Width = new GridLength(outsideWidth);
            tableLayoutPanel1.ColumnDefinitions[2].Width = new GridLength(repositoryWidth);
        };

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

        // The source recreates anonymous LinkLabels on refresh. Keep their runtime ownership
        // and order instead of adding permanent Designer fields and stale translated content.
        StackPanel startLinks = (StackPanel)flpnlStart.Child!;
        StackPanel contributionLinks = (StackPanel)flpnlContribute.Child!;
        contributionLinks.Children.Clear();
        contributionLinks.Children.Add(lblContribute);
        CreateLink(contributionLinks, _develop.Text, Images.Develop.AdaptLightness(), GitHubItem_Click);
        CreateLink(contributionLinks, _donate.Text, Images.DollarSign, DonateItem_Click);
        CreateLink(contributionLinks, _translate.Text, Images.Translate.AdaptLightness(), TranslateItem_Click);
        CreateLink(contributionLinks, _issues.Text, Images.Bug, IssuesItem_Click);
        startLinks.Children.Clear();
        CreateLink(startLinks, _createRepository.Text, Images.RepoCreate, createItem_Click);
        CreateLink(startLinks, _openRepository.Text, Images.RepoOpen, openItem_Click);
        CreateLink(startLinks, _cloneRepository.Text, Images.CloneRepoGit, cloneItem_Click);

        foreach (IRepositoryHostPlugin gitHoster in PluginRegistry.GitHosters)
        {
            CreateLink(startLinks, string.Format(_cloneFork.Text, gitHoster.Name), Images.CloneRepoGitHub,
                (repoSender, eventArgs) => UICommands.StartCloneForkFromHoster(this, gitHoster, GitModuleChanged));
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
        lblContribute.FontStyle = FontStyle.Normal;
        lblContribute.FontWeight = FontWeight.Normal;
        lblContribute.Padding = WinFormsTextMeasurer.GetTextRendererPadding(lblContribute);

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

        static void CreateLink(StackPanel container, string text, IImage icon, EventHandler<RoutedEventArgs> handler)
        {
            LinkLabel linkLabel = new()
            {
                Classes = { "dashboard-link" },
                Content = CreateLinkContent(icon, text),
                FontFamily = new FontFamily(AppSettings.Font.Name),
                FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size),
                FontStyle = AppSettings.Font.Italic ? FontStyle.Italic : FontStyle.Normal,
                FontWeight = AppSettings.Font.Bold ? FontWeight.Bold : FontWeight.Normal,
                Margin = new Thickness(3, 0, 3, 8),
                Padding = new Thickness(24, 3, 3, 3),
                TabIndex = container.Children.Count,
                HorizontalAlignment = HorizontalAlignment.Left,
                HorizontalContentAlignment = HorizontalAlignment.Left,
            };
            linkLabel.Click += handler;
            container.Children.Add(linkLabel);
        }

        static double SizeLinks(StackPanel panel)
        {
            double height = 0;
            LinkLabel[] links = [.. panel.Children.OfType<LinkLabel>()];
            foreach (LinkLabel link in links)
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
    {
        TextBlock caption = new()
        {
            Text = text,
            FontFamily = new FontFamily(AppSettings.Font.Name),
            FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size),
            FontStyle = AppSettings.Font.Italic ? FontStyle.Italic : FontStyle.Normal,
            FontWeight = AppSettings.Font.Bold ? FontWeight.Bold : FontWeight.Normal,
            VerticalAlignment = VerticalAlignment.Center,
        };
        caption.Padding = WinFormsTextMeasurer.GetTextRendererPadding(caption);
        return new Grid
        {
            Children =
            {
                new Image
                {
                    Width = 16,
                    Height = 16,
                    Source = icon,

                    // Label.CalcImageRenderBounds insets a left-aligned image by two pixels.
                    Margin = new Thickness(-24 + 2, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                caption,
            },
        };
    }

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
        internal Button Open => ((StackPanel)dashboard.flpnlStart.Child!).Children.OfType<LinkLabel>().ElementAt(1);
    }
}
