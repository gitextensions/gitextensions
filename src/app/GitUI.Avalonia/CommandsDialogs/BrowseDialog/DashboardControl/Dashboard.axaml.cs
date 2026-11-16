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

        userRepositoriesList.GitModuleChanged += OnModuleChanged;

        // apply scaling
        userRepositoriesList.HeaderHeight = 68;
    }

    public void Initialize(IRepositoryHistoryUIService repositoryHistoryUIService)
    {
        userRepositoriesList.Initialize(repositoryHistoryUIService);
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

        InitDashboardLayout();
        ApplyTheme();
        userRepositoriesList.ShowRecentRepositories(reloadData: false);

        return;

        void ApplyTheme()
        {
            // The source paints its tiled BackgroundImage in the Dashboard's client, not
            // in the scrolling child table. Keep the tile origin fixed when focus scrolls.
            ((ScrollViewer)Content!).Background = new ImageBrush((IImageBrushSource)selectedTheme.BackgroundImage)
            {
                TileMode = TileMode.Tile,
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                DestinationRect = new RelativeRect(new Rect(default, selectedTheme.BackgroundImage.Size), RelativeUnit.Absolute),
            };
            pnlLogo.Background = new SolidColorBrush(selectedTheme.LogoBackColor);
            flpnlStart.Background = new SolidColorBrush(selectedTheme.StartBackColor);
            flpnlContribute.Background = new SolidColorBrush(selectedTheme.ContributeBackColor);
            Resources["DashboardLinkForegroundBrush"] = new SolidColorBrush(selectedTheme.PrimaryText);
            Resources["DashboardLinkHoverForegroundBrush"] = new SolidColorBrush(selectedTheme.AccentedText);
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
            Background = new SolidColorBrush(userRepositoriesList.MainBackColor);
        }

        void InitDashboardLayout()
        {
            StackPanel startLinks = (StackPanel)flpnlStart.Child!;
            StackPanel contributionLinks = (StackPanel)flpnlContribute.Child!;
            try
            {
                AddLinks(contributionLinks,
                    panel =>
                    {
                        panel.Children.Add(lblContribute);
                        lblContribute.FontFamily = new FontFamily(AppSettings.Font.Name);
                        lblContribute.FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size + 5.5F);
                        lblContribute.FontStyle = FontStyle.Normal;
                        lblContribute.FontWeight = FontWeight.Normal;
                        lblContribute.Padding = WinFormsTextMeasurer.GetTextRendererPadding(lblContribute);

                        CreateLink(panel, _develop.Text, Images.Develop.AdaptLightness(), GitHubItem_Click);
                        CreateLink(panel, _donate.Text, Images.DollarSign, DonateItem_Click);
                        CreateLink(panel, _translate.Text, Images.Translate.AdaptLightness(), TranslateItem_Click);
                        Control lastControl = CreateLink(panel, _issues.Text, Images.Bug, IssuesItem_Click);
                        return lastControl;
                    },
                    (panel, lastControl) =>
                    {
                        Avalonia.Size heading = WinFormsTextMeasurer.MeasureTextRenderer(lblContribute, lblContribute.Text ?? string.Empty);
                        lblContribute.Width = Math.Ceiling(heading.Width);
                        lblContribute.Height = Math.Ceiling(heading.Height);
                        double height = SizeLinks(panel) + lblContribute.Height + lblContribute.Margin.Top + lblContribute.Margin.Bottom
                            + flpnlContribute.Padding.Top + flpnlContribute.Padding.Bottom;
                        flpnlContribute.Height = height;
                        flpnlContribute.MinHeight = height;
                    });

                AddLinks(startLinks,
                    panel =>
                    {
                        CreateLink(panel, _createRepository.Text, Images.RepoCreate, createItem_Click);
                        CreateLink(panel, _openRepository.Text, Images.RepoOpen, openItem_Click);
                        Control lastControl = CreateLink(panel, _cloneRepository.Text, Images.CloneRepoGit, cloneItem_Click);
                        foreach (IRepositoryHostPlugin gitHoster in PluginRegistry.GitHosters)
                        {
                            lastControl = CreateLink(panel, string.Format(_cloneFork.Text, gitHoster.Name), Images.CloneRepoGitHub,
                                (repoSender, eventArgs) => UICommands.StartCloneForkFromHoster(this, gitHoster, GitModuleChanged));
                        }

                        return lastControl;
                    },
                    (panel, lastControl) => flpnlStart.MinHeight = SizeLinks(panel) + flpnlStart.Padding.Top + flpnlStart.Padding.Bottom);
            }
            finally
            {
                tableLayoutPanel1.MinHeight = pnlLogo.Height + flpnlStart.MinHeight + flpnlContribute.MinHeight;
                tableLayoutPanel1.Height = Math.Max(Bounds.Height, tableLayoutPanel1.MinHeight);
            }

            return;

            static void AddLinks(StackPanel panel, Func<StackPanel, Control> addLinks, Action<StackPanel, Control> onLayout)
            {
                panel.Children.Clear();
                Control lastControl = addLinks(panel);
                onLayout(panel, lastControl);
            }

            Control CreateLink(Control container, string text, IImage icon, EventHandler<RoutedEventArgs> handler)
            {
                TextBlock caption = new DashboardTextBlock()
                {
                    Text = text,
                    FontFamily = new FontFamily(AppSettings.Font.Name),
                    FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size),
                    FontStyle = AppSettings.Font.Italic ? FontStyle.Italic : FontStyle.Normal,
                    FontWeight = AppSettings.Font.Bold ? FontWeight.Bold : FontWeight.Normal,
                    VerticalAlignment = VerticalAlignment.Top,

                    // Native LinkLabel clips once at its HWND client. This anonymous renderer
                    // has a smaller Skia line box, which must not clip native-em descenders.
                    ClipToBounds = false,
                };
                caption.Padding = WinFormsTextMeasurer.GetTextRendererPadding(caption);
                Control content = new Grid
                {
                    Children =
                    {
                        new Image
                        {
                            Width = 16,
                            Height = 16,
                            Source = icon,

                            // Label.CalcImageRenderBounds insets a left-aligned image by two pixels.
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                        },
                        caption,
                    },
                };

                LinkLabel linkLabel = new DashboardLinkLabel()
                {
                    Classes = { "dashboard-link" },
                    Content = content,
                    FontFamily = new FontFamily(AppSettings.Font.Name),
                    FontSize = AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size),
                    FontStyle = AppSettings.Font.Italic ? FontStyle.Italic : FontStyle.Normal,
                    FontWeight = AppSettings.Font.Bold ? FontWeight.Bold : FontWeight.Normal,
                    Margin = new Thickness(3, 0, 3, 8),
                    Padding = new Thickness(24, 3, 3, 3),
                    TabIndex = ((StackPanel)container).Children.Count,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                };

                // Avalonia supplies delayed hover/leave colors through DashboardLinkLabel and its native styles.
                linkLabel.Click += handler;
                ((StackPanel)container).Children.Add(linkLabel);
                return linkLabel;
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
}
