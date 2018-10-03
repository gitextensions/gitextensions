using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GitCommands;
using GitUI.CommandsDialogs.AboutBoxDialog;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormAbout : GitExtensionsForm
    {
        private readonly TranslationString _thanksToContributors = new TranslationString("Thanks to over {0:#,##0} contributors: ");

        public FormAbout()
        {
            InitializeComponent();
            InitializeComplete();

            _NO_TRANSLATE_labelVersionInfo.Text += AppSettings.ProductVersion;

            // Click handlers
            _NO_TRANSLATE_labelProductName.LinkClicked += (s, e) => { Process.Start(@"http://github.com/gitextensions/gitextensions"); };
            thanksTo.LinkClicked += delegate { ShowContributorsForm(); };
            pictureDonate.Click += delegate { Process.Start(FormDonate.DonationUrl); };
            linkLabelIcons.LinkClicked += delegate { Process.Start("http://p.yusukekamiyamane.com/"); };

            var contributorsList = GetContributorList();
            var thanksToContributorsText = string.Format(_thanksToContributors.Text, contributorsList.Count);

            var random = new Random();

            thanksTimer.Tick += delegate { ThankNextContributor(); };
            thanksTimer.Enabled = true;
            thanksTimer.Interval = 1000;
            thanksTimer.Start();

            ThankNextContributor();

            return;

            void ShowContributorsForm()
            {
                using (var formContributors = new FormContributors())
                {
                    formContributors.ShowDialog(owner: this);
                }
            }

            void ThankNextContributor()
            {
                // Select a contributor at random
                var contributorName = contributorsList[random.Next(contributorsList.Count - 1)].Trim();

                thanksTo.Text = thanksToContributorsText + contributorName;
            }

            IReadOnlyList<string> GetContributorList()
            {
                return new[] { Resources.Team, Resources.Coders, Resources.Translators, Resources.Designers }
                    .Select(c => c.Replace(Environment.NewLine, " "))
                    .SelectMany(line => line.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                    .ToList();
            }
        }
    }
}
