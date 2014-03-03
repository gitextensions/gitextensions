﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Git.hub;
using GitCommands.Config;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormUpdates : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _newVersionAvailable =
            new TranslationString("There is a new version {0} of Git Extensions available");
        private readonly TranslationString _noUpdatesFound =
            new TranslationString("No updates found");
        #endregion

        public IWin32Window OwnerWindow;
        public Version CurrentVersion;
        public bool UpdateFound;
        public string UpdateUrl;
        public string NewVersion;
        private readonly SynchronizationContext _syncContext;

        public FormUpdates(Version currentVersion)
        {
            _syncContext = SynchronizationContext.Current;
            InitializeComponent();
            Translate();
            UpdateFound = false;
            progressBar1.Visible = true;
            CurrentVersion = currentVersion;
            UpdateUrl = "";
            NewVersion = "";
            progressBar1.Style = ProgressBarStyle.Marquee;
        }

        private void CloseButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        public void SearchForUpdatesAndShow(IWin32Window aOwnerWindow, bool alwaysShow)
        {
            OwnerWindow = aOwnerWindow;
            new Thread(SearchForUpdates).Start();
            if (alwaysShow)
                ShowDialog(aOwnerWindow);
        }

        private void SearchForUpdates()
        {
            try
            {
                Client github = new Client();
                Repository gitExtRepo = github.getRepository("gitextensions", "gitextensions");
                if (gitExtRepo == null)
                    return;

                var configData = gitExtRepo.GetRef("heads/configdata");
                if (configData == null)
                    return;

                var tree = configData.GetTree();
                if (tree == null)
                    return;

                var releases = tree.Tree.Where(entry => "GitExtensions.releases".Equals(entry.Path, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (releases != null && releases.Blob.Value != null)
                {
                    CheckForNewerVersion(releases.Blob.Value.GetContent());
                }
            }
            catch (Exception ex)
            {
                _syncContext.Send((state) =>
                    {
                        GitCommands.ExceptionUtils.ShowException(this, ex, string.Empty, true);
                    }, null);
                Done();
            }

        }

        void CheckForNewerVersion(string releases)
        {
            var versions = ReleaseVersion.Parse(releases);
            var updates = versions.Where(version => version.Version.CompareTo(CurrentVersion) > 0);

            var update = updates.OrderBy(version => version.Version).LastOrDefault();
            if (update != null)
            {
                UpdateFound = true;
                UpdateUrl = update.DownloadPage;
                NewVersion = update.Version.ToString();
                Done();
                return;
            }
            UpdateUrl = "";
            UpdateFound = false;
            Done();
        }

        private void Done()
        {
            _syncContext.Send(o =>
            {
                progressBar1.Visible = false;

                if (UpdateFound)
                {
                    btnDownloadNow.Enabled = true;
                    UpdateLabel.Text = string.Format(_newVersionAvailable.Text, NewVersion);
                    linkChangeLog.Visible = true;

                    if (!Visible)
                        ShowDialog(OwnerWindow);
                }
                else
                {
                    UpdateLabel.Text = _noUpdatesFound.Text;
                }
            }, this);
        }

        private void linkChangeLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/gitextensions/gitextensions/blob/master/GitUI/Resources/ChangeLog.md");
        }

        private void btnDownloadNow_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(UpdateUrl);
            }
            catch (System.ComponentModel.Win32Exception)
            {
            }
        }
    }

    public class ReleaseVersion
    {
        public Version Version;
        public string ReleaseType;
        public string DownloadPage;

        public static ReleaseVersion FromSection(ConfigSection section)
        {
            Version ver;
            try
            {
                ver = new Version(section.SubSection);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                return null;
            }

            return new ReleaseVersion()
            {
                Version = ver,
                ReleaseType = section.GetValue("ReleaseType"),
                DownloadPage = section.GetValue("DownloadPage")
            };

        }

        public static IEnumerable<ReleaseVersion> Parse(string versionsStr)
        {
            ConfigFile cfg = new ConfigFile("", true);
            cfg.LoadFromString(versionsStr);
            var sections = cfg.GetConfigSections("Version");

            return sections.Select(FromSection).Where(version => version != null);
        }

    }

}