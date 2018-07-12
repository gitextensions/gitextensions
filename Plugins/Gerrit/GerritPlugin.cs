using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gerrit.Properties;
using GitUI;
using GitUIPluginInterfaces;
using JetBrains.Annotations;
using ResourceManager;

namespace Gerrit
{
    [Export(typeof(IGitPlugin))]
    public class GerritPlugin : GitPluginBase, IGitPluginForRepository
    {
        #region Translation
        private readonly TranslationString _editGitReview = new TranslationString("Edit .gitreview");
        private readonly TranslationString _downloadGerritChange = new TranslationString("Download Gerrit Change");
        private readonly TranslationString _publishGerritChange = new TranslationString("Publish Gerrit Change");
        private readonly TranslationString _installCommitMsgHook = new TranslationString("Install Hook");
        private readonly TranslationString _installCommitMsgHookShortText = new TranslationString("Install commit-msg hook");
        private readonly TranslationString _installCommitMsgHookMessage = new TranslationString("Gerrit requires a commit-msg hook to be installed. Do you want to install the commit-msg hook into your repository?");
        private readonly TranslationString _installCommitMsgHookFolderCreationFailed = new TranslationString("Could not create the hooks folder. Please create the folder manually and try again.");
        private readonly TranslationString _installCommitMsgHookDownloadFileFailed = new TranslationString("Could not download the commit-msg file. Please install the commit-msg hook manually.");
        #endregion

        private static readonly Dictionary<string, bool> _validatedHooks = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _syncRoot = new object();

        private const string HooksFolderName = "hooks";
        private const string CommitMessageHookFileName = "commit-msg";

        private bool _initialized;
        private ToolStripItem[] _gerritMenuItems;
        private ToolStripMenuItem _gitReviewMenuItem;
        private Form _mainForm;
        private IGitUICommands _gitUiCommands;
        private ToolStripButton _installCommitMsgMenuItem;

        // public only because of FormTranslate
        public GerritPlugin()
        {
            SetNameAndDescription("Gerrit Code Review");
            Translate();
            Icon = Resources.IconGerrit;
        }

        public override void Register(IGitUICommands gitUiCommands)
        {
            _gitUiCommands = gitUiCommands;
            gitUiCommands.PostBrowseInitialize += UpdateGerritMenuItems;
            gitUiCommands.PostRegisterPlugin += UpdateGerritMenuItems;
        }

        public override void Unregister(IGitUICommands gitUiCommands)
        {
            gitUiCommands.PostBrowseInitialize -= UpdateGerritMenuItems;
            gitUiCommands.PostRegisterPlugin -= UpdateGerritMenuItems;
            _gitUiCommands = null;
        }

        private void UpdateGerritMenuItems(object sender, GitUIEventArgs e)
        {
            if (!_initialized)
            {
                Initialize((Form)e.OwnerForm);
            }

            // Correct enabled/visibility of our menu/tool strip items.

            bool validWorkingDir = e.GitModule.IsValidGitWorkingDir();

            _gitReviewMenuItem.Enabled = validWorkingDir;

            bool showGerritItems = validWorkingDir && File.Exists(e.GitModule.WorkingDir + ".gitreview");

            foreach (var item in _gerritMenuItems)
            {
                item.Visible = showGerritItems;
            }

            _installCommitMsgMenuItem.Visible =
                showGerritItems &&
                !HaveValidCommitMsgHook(e.GitModule);
        }

        private static bool HaveValidCommitMsgHook([NotNull] IGitModule gitModule, bool force = false)
        {
            if (gitModule == null)
            {
                throw new ArgumentNullException(nameof(gitModule));
            }

            string path = Path.Combine(gitModule.ResolveGitInternalPath(HooksFolderName), CommitMessageHookFileName);

            if (!File.Exists(path))
            {
                return false;
            }

            // We don't want to read the contents of the commit-msg every time
            // we call this method, so we cache the result if we aren't
            // forced.

            lock (_syncRoot)
            {
                if (!force && _validatedHooks.TryGetValue(path, out var isValid))
                {
                    return isValid;
                }

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

            var menuStrip = form.FindDescendantOfType<MenuStrip>(p => p.Name == "menuStrip1");
            var toolStrip = form.FindDescendantOfType<ToolStrip>(p => p.Name == "ToolStrip");

            if (menuStrip == null)
            {
                throw new Exception("Cannot find main menu");
            }

            if (toolStrip == null)
            {
                throw new Exception("Cannot find main tool strip");
            }

            // Create the Edit .gitreview button.

            var repositoryMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "repositoryToolStripMenuItem");
            if (repositoryMenu == null)
            {
                throw new Exception("Cannot find Repository menu");
            }

            var mailMapMenuItem = repositoryMenu.DropDownItems.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "editmailmapToolStripMenuItem");
            if (mailMapMenuItem == null)
            {
                throw new Exception("Cannot find mailmap menu item");
            }

            _gitReviewMenuItem = new ToolStripMenuItem
            {
                Text = _editGitReview.Text
            };

            _gitReviewMenuItem.Click += gitReviewMenuItem_Click;

            repositoryMenu.DropDownItems.Insert(
                repositoryMenu.DropDownItems.IndexOf(mailMapMenuItem) + 1,
                _gitReviewMenuItem);

            // Create the tool strip items.

            var pushMenuItem = toolStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "toolStripButtonPush");
            if (pushMenuItem == null)
            {
                throw new Exception("Cannot find push menu item");
            }

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

        private void publishMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormGerritPublish(_gitUiCommands))
            {
                form.ShowDialog(_mainForm);
            }

            _gitUiCommands.RepoChangedNotifier.Notify();
        }

        private void downloadMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormGerritDownload(_gitUiCommands))
            {
                form.ShowDialog(_mainForm);
            }

            _gitUiCommands.RepoChangedNotifier.Notify();
        }

        private void installCommitMsgMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                _mainForm,
                _installCommitMsgHookMessage.Text,
                _installCommitMsgHookShortText.Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ThreadHelper.JoinableTaskFactory.Run(InstallCommitMsgHookAsync);
            }

            _gitUiCommands.RepoChangedNotifier.Notify();
        }

        private async Task InstallCommitMsgHookAsync()
        {
            await _mainForm.SwitchToMainThreadAsync();

            var settings = GerritSettings.Load(_mainForm, _gitUiCommands.GitModule);

            if (settings == null)
            {
                return;
            }

            var hooksFolderPath = _gitUiCommands.GitModule.ResolveGitInternalPath(HooksFolderName);
            if (!Directory.Exists(hooksFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(hooksFolderPath);
                }
                catch
                {
                    MessageBox.Show(
                        _mainForm,
                        _installCommitMsgHookFolderCreationFailed.Text,
                        _installCommitMsgHookShortText.Text,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }
            }

            var commitMessageHookPath = Path.Combine(hooksFolderPath, CommitMessageHookFileName);

            string content;

            try
            {
                content = await DownloadFromScpAsync(settings);
            }
            catch
            {
                content = null;
            }

            await _mainForm.SwitchToMainThreadAsync();
            if (content == null)
            {
                MessageBox.Show(
                    _mainForm,
                    _installCommitMsgHookDownloadFileFailed.Text,
                    _installCommitMsgHookShortText.Text,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                File.WriteAllText(commitMessageHookPath, content);

                // Update the cache.

                HaveValidCommitMsgHook(_gitUiCommands.GitModule, true);
            }
        }

        [ItemCanBeNull]
        private async Task<string> DownloadFromScpAsync(GerritSettings settings)
        {
            // This is a very quick and dirty "implementation" of the scp
            // protocol. By sending the 0's as input, we trigger scp to
            // send the file.

            string content = await GerritUtil.RunGerritCommandAsync(
                _mainForm,
                _gitUiCommands.GitModule,
                "scp -f hooks/commit-msg",
                settings.DefaultRemote,
                new byte[] { 0, 0, 0, 0, 0, 0, 0 }).ConfigureAwait(false);

            // The first line of the output contains the file we're receiving
            // in a format like "C0755 4248 commit-msg".

            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            int index = content.IndexOf('\n');

            if (index == -1)
            {
                return null;
            }

            string header = content.Substring(0, index);

            if (!header.EndsWith(" commit-msg"))
            {
                return null;
            }

            // This looks like a valid scp response; return the rest of the
            // response.

            content = content.Substring(index + 1);

            // The file should be terminated by a nul.

            index = content.LastIndexOf((char)0);

            Debug.Assert(index == content.Length - 1, "index == content.Length - 1");

            if (index != -1)
            {
                content = content.Substring(0, index);
            }

            return content;
        }

        private void gitReviewMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormGitReview(_gitUiCommands))
            {
                form.ShowDialog(_mainForm);
            }

            _gitUiCommands.RepoChangedNotifier.Notify();
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using (var form = new FormPluginInformation())
            {
                form.ShowDialog();
            }

            return false;
        }
    }
}
