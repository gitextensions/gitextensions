using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using GitCommands;
using GitUI.CommandsDialogs.AboutBoxDialog;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Properties;
using ResourceManager;
using Clipboard = System.Windows.Forms.Clipboard;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormAbout : GitExtensionsForm
    {
        private readonly TranslationString _thanksToContributors = new TranslationString("Thanks to over {0:#,##0} contributors: ");

        public FormAbout()
        {
            InitializeComponent();
            InitializeComplete();

            gitExtensionsVersion.Text = string.Format(gitExtensionsVersion.Text, AppSettings.ProductVersion);
            gitVersion.Text = string.Format(gitVersion.Text, GetGitVersionString());
            osVersion.Text = string.Format(osVersion.Text, GetOSVersionString());
            dotNetVersion.Text = string.Format(dotNetVersion.Text, Environment.Version);

            copyButton.Image = Images.CopyToClipboard;

            // Click handlers
            _NO_TRANSLATE_labelProductName.LinkClicked += (s, e) => { Process.Start(@"http://github.com/gitextensions/gitextensions"); };
            thanksTo.LinkClicked += delegate { ShowContributorsForm(); };
            pictureDonate.Click += delegate { Process.Start(FormDonate.DonationUrl); };
            linkLabelIcons.LinkClicked += delegate { Process.Start("http://p.yusukekamiyamane.com/"); };
            copyButton.Click += delegate { CopyVersionInfosToClipboard(); };

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

            void CopyVersionInfosToClipboard()
            {
                var sb = new StringBuilder();

                sb.AppendLine(gitExtensionsVersion.Text);
                sb.AppendLine(gitVersion.Text);
                sb.AppendLine(osVersion.Text);
                sb.AppendLine(dotNetVersion.Text);

                Clipboard.SetText(sb.ToString());
            }

            string GetGitVersionString()
            {
                if (!File.Exists(AppSettings.GitCommandValue))
                {
                    return "Not installed";
                }

                var versionInfo = FileVersionInfo.GetVersionInfo(AppSettings.GitCommandValue);

                return $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";
            }

            string GetOSVersionString()
            {
                return $"Windows {Environment.OSVersion.Version.Major} Build {Environment.OSVersion.Version.Build}";
            }
        }
    }
}
