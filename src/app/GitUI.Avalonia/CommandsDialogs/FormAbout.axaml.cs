using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility;
using GitUI.CommandsDialogs.AboutBoxDialog;
using GitUI.CommandsDialogs.BrowseDialog;
using ResourceManager;

namespace GitUI.CommandsDialogs;

public sealed partial class FormAbout : GitExtensionsForm
{
    private readonly TranslationString _thanksToContributors = new("Thanks to over {0:#,##0} contributors: ");
    private readonly TranslationString _copyTooltip = new("Copy environment info");

    private readonly DispatcherTimer _thanksTimer = new();

    public FormAbout()
    {
        InitializeComponent();
        _NO_TRANSLATE_labelProductName.Content = AppSettings.ApplicationName;
        InitializeComplete();

        environmentInfo.SetCopyButtonTooltip(_copyTooltip.Text);

        // Click handlers
        _NO_TRANSLATE_labelProductName.Click += delegate { OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions"); };
        _NO_TRANSLATE_ThanksTo.Click += delegate { ShowContributorsForm(); };
        pictureDonate.PointerPressed += delegate { OsShellUtil.OpenUrlInDefaultBrowser(FormDonate.DonationUrl); };
        linkLabelIcons.Click += delegate { OsShellUtil.OpenUrlInDefaultBrowser(@"http://p.yusukekamiyamane.com/"); };

        IReadOnlyList<string> contributorsList = GetContributorList();
        string thanksToContributorsText = string.Format(_thanksToContributors.Text, contributorsList.Count);

        Random random = new();

        _thanksTimer.Tick += delegate { ThankNextContributor(); };
        _thanksTimer.Interval = TimeSpan.FromMilliseconds(1000);
        _thanksTimer.Start();

        ThankNextContributor();

        return;

        void ShowContributorsForm()
        {
            using FormContributors formContributors = new();
            formContributors.ShowDialog(owner: this);
        }

        void ThankNextContributor()
        {
            // Select a contributor at random
            string contributorName = contributorsList[random.Next(contributorsList.Count)].Trim();

            _NO_TRANSLATE_ThanksTo.Content = thanksToContributorsText + contributorName;
        }

        IReadOnlyList<string> GetContributorList()
        {
            return new[] { Properties.Resources.Team, Properties.Resources.Coders, Properties.Resources.Translators, Properties.Resources.Designers }
                .Select(c => c.Replace(Environment.NewLine, ""))
                .SelectMany(line => line.LazySplit(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(contributor => contributor.Trim())
                .ToList();
        }
    }

    protected override void OnClosing(Avalonia.Controls.WindowClosingEventArgs e)
    {
        _thanksTimer.Stop();
        base.OnClosing(e);
    }
}
