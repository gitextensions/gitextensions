using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GitCommands;
using GitExtUtils;
using GitUI.CommandsDialogs.AboutBoxDialog;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormAbout : GitExtensionsForm
    {
        private readonly TranslationString _thanksToContributors = new("Thanks to over {0:#,##0} contributors: ");
        private readonly TranslationString _copyTooltip = new("Copy environment info");

        public FormAbout()
        {
            InitializeComponent();
            _NO_TRANSLATE_labelProductName.Text = AppSettings.ApplicationName;
            InitializeComplete();

            environmentInfo.SetCopyButtonTooltip(_copyTooltip.Text);

            Color clrLink = SystemColors.Highlight;
            _NO_TRANSLATE_labelProductName.LinkColor = clrLink;
            _NO_TRANSLATE_ThanksTo.LinkColor = clrLink;
            linkLabelIcons.LinkColor = clrLink;

            // Click handlers
            _NO_TRANSLATE_labelProductName.LinkClicked += delegate { OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions"); };
            _NO_TRANSLATE_ThanksTo.LinkClicked += delegate { ShowContributorsForm(); };
            pictureDonate.Click += delegate { OsShellUtil.OpenUrlInDefaultBrowser(FormDonate.DonationUrl); };
            linkLabelIcons.LinkClicked += delegate { OsShellUtil.OpenUrlInDefaultBrowser(@"http://p.yusukekamiyamane.com/"); };

            var contributorsList = GetContributorList();
            var thanksToContributorsText = string.Format(_thanksToContributors.Text, contributorsList.Count);

            Random random = new();

            thanksTimer.Tick += delegate { ThankNextContributor(); };
            thanksTimer.Enabled = true;
            thanksTimer.Interval = 1000;
            thanksTimer.Start();

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
                var contributorName = contributorsList[random.Next(contributorsList.Count)].Trim();

                _NO_TRANSLATE_ThanksTo.Text = thanksToContributorsText + contributorName;
            }

            IReadOnlyList<string> GetContributorList()
            {
                return new[] { Resources.Team, Resources.Coders, Resources.Translators, Resources.Designers }
                    .Select(c => c.Replace(Environment.NewLine, ""))
                    .SelectMany(line => line.LazySplit(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(contributor => contributor.Trim())
                    .ToList();
            }
        }
    }
}
