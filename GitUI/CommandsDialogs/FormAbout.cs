using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GitCommands;
using GitUI.CommandsDialogs.AboutBoxDialog;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Properties;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormAbout : GitExtensionsForm
    {
        public FormAbout()
        {
            InitializeComponent();
            InitializeComplete();

            _NO_TRANSLATE_labelVersionInfo.Text += AppSettings.ProductVersion;

            // Click handlers
            thanksTo.LinkClicked += delegate { ShowContributorsForm(); };
            pictureDonate.Click += delegate { Process.Start(FormDonate.DonationUrl); };
            linkLabelIcons.LinkClicked += delegate { Process.Start("http://p.yusukekamiyamane.com/"); };
            okButton.Click += delegate { Close(); };

            var contributorsList = GetContributorList();
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

                thanksTo.Text = $"Thanks to over {contributorsList.Count:#,##0} contributors: {contributorName}";
            }

            IReadOnlyList<string> GetContributorList()
            {
                var contributorListList = new[]
                {
                    Resources.Coders.Replace(Environment.NewLine, " "),
                    Resources.Translators.Replace(Environment.NewLine, " "),
                    Resources.Designers.Replace(Environment.NewLine, " "),
                };

                return contributorListList
                    .SelectMany(list => list.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                    .ToList();
            }
        }
    }
}
