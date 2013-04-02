using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager.Translation;

namespace Gerrit
{
    public class GerritPlugin : GitPluginBase, IGitPluginForRepository, ITranslate
    {
        #region Translation
        private readonly TranslationString _pluginDescription = new TranslationString("Gerrit Code Review");
        private readonly TranslationString _editGitReview = new TranslationString("Edit .gitreview");
        private readonly TranslationString _downloadGerritChange = new TranslationString("Download Gerrit Change");
        private readonly TranslationString _publishGerritChange = new TranslationString("Publish Gerrit Change");
        private readonly TranslationString _installCommitMsgHook = new TranslationString("Install Hook");
        private readonly TranslationString _installCommitMsgHookShortText = new TranslationString("Install commit-msg hook");
        private readonly TranslationString _installCommitMsgHookMessage = new TranslationString("Gerrit requires a commit-msg hook to be installed. Do you want to install the commit-msg hook into your repository?");
        private readonly TranslationString _installCommitMsgHookFailed = new TranslationString("Could not download the commit-msg file. Please install the commit-msg hook manually.");
        #endregion

        private static readonly Dictionary<string, bool> _validatedHooks = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _syncRoot = new object();

        private bool _initialized;
        private ToolStripItem[] _gerritMenuItems;
        private ToolStripMenuItem _gitReviewMenuItem;
        private Form _mainForm;
        private IGitUICommands _gitUiCommands;
        private ToolStripButton _installCommitMsgMenuItem;
        
        // public only because of FormTranslate
        public GerritPlugin()
        {
            Translator.Translate(this, GitCommands.Settings.CurrentTranslation);
        }

        public override string Description
        {
            get { return _pluginDescription.Text; }
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            _gitUiCommands = gitUiCommands;
            gitUiCommands.PostBrowseInitialize += gitUiCommands_PostBrowseInitialize;
            gitUiCommands.PostRegisterPlugin += gitUiCommands_PostRegisterPlugin;
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            gitUiCommands.PostBrowseInitialize -= gitUiCommands_PostBrowseInitialize;
            gitUiCommands.PostRegisterPlugin -= gitUiCommands_PostRegisterPlugin;
            _gitUiCommands = null;
        }

        public virtual void AddTranslationItems(Translation translation)
        {
            TranslationUtl.AddTranslationItemsFromFields(GetType().Name, this, translation);
        }

        public virtual void TranslateItems(Translation translation)
        {
            TranslationUtl.TranslateItemsFromFields(GetType().Name, this, translation);
        }

        void gitUiCommands_PostRegisterPlugin(object sender, GitUIBaseEventArgs e)
        {
            UpdateGerritMenuItems(e);
        }

        void gitUiCommands_PostBrowseInitialize(object sender, GitUIBaseEventArgs e)
        {
            UpdateGerritMenuItems(e);
        }

        private void UpdateGerritMenuItems(GitUIBaseEventArgs e)
        {
            if (!_initialized)
                Initialize((Form)e.OwnerForm);

            // Correct enabled/visibility of our menu/tool strip items.

            bool validWorkingDir = e.GitModule.IsValidGitWorkingDir();

            _gitReviewMenuItem.Enabled = validWorkingDir;

            bool showGerritItems = validWorkingDir && File.Exists(e.GitModule.GitWorkingDir + ".gitreview");

            foreach (var item in _gerritMenuItems)
            {
                item.Visible = showGerritItems;
            }

            _installCommitMsgMenuItem.Visible =
                showGerritItems &&
                !HaveValidCommitMsgHook(e.GitModule.GetGitDirectory());
        }

        private bool HaveValidCommitMsgHook(string gitDirectory)
        {
            return HaveValidCommitMsgHook(gitDirectory, false);
        }

        private bool HaveValidCommitMsgHook([NotNull] string gitDirectory, bool force)
        {
            if (gitDirectory == null)
                throw new ArgumentNullException("gitDirectory");

            string path = Path.Combine(gitDirectory, "hooks", "commit-msg");

            if (!File.Exists(path))
                return false;

            // We don't want to read the contents of the commit-msg every time
            // we call this method, so we cache the result if we aren't
            // forced.

            lock (_syncRoot)
            {
                bool isValid;

                if (!force && _validatedHooks.TryGetValue(path, out isValid))
                    return isValid;

                try
                {
                    string content = File.ReadAllText(path);

                    // Don't do an extensive check. If the commit-msg contains the
                    // text "gerrit", it's probably the Gerrit commit-msg hook.

                    isValid = content.IndexOf("gerrit", StringComparison.OrdinalIgnoreCase) != -1;
                }
                catch
                {
                    isValid = false;
                }

                _validatedHooks[path] = isValid;

                return isValid;
            }
        }

        private void Initialize(Form form)
        {
            // Prevent initialize being called multiple times when we fail to
            // initialize.

            _initialized = true;

            // Take a reference to the main form. We use this for ownership.

            _mainForm = form;

            // Find the controls we're going to extend.

            var menuStrip = FindControl<MenuStrip>(form, p => true);
            var toolStrip = FindControl<ToolStrip>(form, p => p.Name == "ToolStrip");

            if (menuStrip == null)
                throw new Exception("Cannot find main menu");
            if (toolStrip == null)
                throw new Exception("Cannot find main tool strip");

            // Create the Edit .gitreview button.

            var repositoryMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "repositoryToolStripMenuItem");
            if (repositoryMenu == null)
                throw new Exception("Cannot find Repository menu");

            var mailMapMenuItem = repositoryMenu.DropDownItems.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "editmailmapToolStripMenuItem");
            if (mailMapMenuItem == null)
                throw new Exception("Cannot find mailmap menu item");

            _gitReviewMenuItem = new ToolStripMenuItem
            {
                Text = _editGitReview.Text
            };

            _gitReviewMenuItem.Click += gitReviewMenuItem_Click;

            repositoryMenu.DropDownItems.Insert(
                repositoryMenu.DropDownItems.IndexOf(mailMapMenuItem) + 1,
                _gitReviewMenuItem
            );

            // Create the toolstrip items.

            var pushMenuItem = toolStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "toolStripButtonPush");
            if (pushMenuItem == null)
                throw new Exception("Cannot find push menu item");

            int nextIndex = toolStrip.Items.IndexOf(pushMenuItem) + 1;

            var separator = new ToolStripSeparator();

            toolStrip.Items.Insert(nextIndex++, separator);

            var downloadMenuItem = new ToolStripButton
            {
                Text = _downloadGerritChange.Text,
                Image = Properties.Resources.GerritDownload,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Visible = false
            };

            downloadMenuItem.Click += downloadMenuItem_Click;

            toolStrip.Items.Insert(nextIndex++, downloadMenuItem);

            var publishMenuItem = new ToolStripButton
            {
                Text = _publishGerritChange.Text,
                Image = Properties.Resources.GerritPublish,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Visible = false
            };

            publishMenuItem.Click += publishMenuItem_Click;

            toolStrip.Items.Insert(nextIndex++, publishMenuItem);

            _installCommitMsgMenuItem = new ToolStripButton
            {
                Text = _installCommitMsgHook.Text,
                ToolTipText = _installCommitMsgHookShortText.Text,
                Image = Properties.Resources.GerritInstallHook,
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                Visible = false
            };

            _installCommitMsgMenuItem.Click += installCommitMsgMenuItem_Click;

            toolStrip.Items.Insert(nextIndex++, _installCommitMsgMenuItem);

            // Keep a list of all items so we can show/hide them based in the
            // presence of the .gitreview file.

            _gerritMenuItems = new ToolStripItem[]
            {
                separator,
                downloadMenuItem,
                publishMenuItem
            };
        }

        void publishMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormGerritPublish(_gitUiCommands))
            {
                form.ShowDialog(_mainForm);
            }

            _gitUiCommands.RepoChangedNotifier.Notify();
        }

        void downloadMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormGerritDownload(_gitUiCommands))
            {
                form.ShowDialog(_mainForm);
            }

            _gitUiCommands.RepoChangedNotifier.Notify();
        }

        void installCommitMsgMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                _mainForm,
                _installCommitMsgHookMessage.Text,
                _installCommitMsgHookShortText.Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
                InstallCommitMsgHook();

            _gitUiCommands.RepoChangedNotifier.Notify();
        }

        private void InstallCommitMsgHook()
        {
            var settings = GerritSettings.Load(_mainForm, _gitUiCommands.GitModule);

            if (settings == null)
                return;

            string path = Path.Combine(
                _gitUiCommands.GitModule.GetGitDirectory(),
                "hooks",
                "commit-msg"
            );

            string content;

            try
            {
                content = DownloadFromScp(settings);
            }
            catch
            {
                content = null;
            }

            if (content == null)
            {
                MessageBox.Show(
                    _mainForm,
                    _installCommitMsgHookFailed.Text,
                    _installCommitMsgHookShortText.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            else
            {
                File.WriteAllText(path, content);

                // Update the cache.

                HaveValidCommitMsgHook(_gitUiCommands.GitModule.GetGitDirectory(), true);
            }
        }

        private string DownloadFromScp(GerritSettings settings)
        {
            // This is a very quick and dirty "implementation" of the scp
            // protocol. By sending the 0's as input, we trigger scp to
            // send the file.

            string content = GerritUtil.RunGerritCommand(
                _mainForm,
                _gitUiCommands.GitModule,
                "scp -f hooks/commit-msg",
                settings.DefaultRemote,
                new byte[] { 0, 0, 0, 0, 0, 0, 0 }
            );

            // The first line of the output contains the file we're receiving
            // in a format like "C0755 4248 commit-msg". 

            if (String.IsNullOrEmpty(content))
                return null;

            int index = content.IndexOf('\n');

            if (index == -1)
                return null;

            string header = content.Substring(0, index);

            if (!header.EndsWith(" commit-msg"))
                return null;

            // This looks like a valid scp response; return the rest of the
            // response.

            content = content.Substring(index + 1);

            // The file should be terminated by a nul.

            index = content.LastIndexOf((char)0);

            Debug.Assert(index == content.Length - 1);

            if (index != -1)
                content = content.Substring(0, index);

            return content;
        }

        void gitReviewMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormGitReview(_gitUiCommands))
            {
                form.ShowDialog(_mainForm);
            }

            _gitUiCommands.RepoChangedNotifier.Notify();
        }

        private T FindControl<T>(Control form, Func<T, bool> predicate)
            where T : Control
        {
            return FindControl(form.Controls, predicate);
        }

        private T FindControl<T>(IEnumerable controls, Func<T, bool> predicate)
            where T : Control
        {
            foreach (Control control in controls)
            {
                var result = control as T;

                if (result != null && predicate(result))
                    return result;

                result = FindControl(control.Controls, predicate);

                if (result != null)
                    return result;
            }

            return null;
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var form = new FormPluginInformation())
            {
                form.ShowDialog();
            }

            return false;
        }
    }
}
