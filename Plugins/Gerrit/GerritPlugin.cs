using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;
using System.Windows.Forms;
using System.Drawing;

namespace Gerrit
{
    public class GerritPlugin : IGitPluginForRepository
    {
        private bool _initialized;
        private ToolStripItem[] _gerritMenuItems;
        private ToolStripMenuItem _gitReviewMenuItem;
        private Form _mainForm;
        private IGitUICommands _gitUiCommands;

        public string Description
        {
            get { return "Gerrit Code Review"; }
        }

        public IGitPluginSettingsContainer Settings { get; set; }

        public void Register(IGitUICommands gitUiCommands)
        {
            _gitUiCommands = gitUiCommands;

            gitUiCommands.PostBrowseInitialize += gitUiCommands_PostBrowseInitialize;
        }

        void gitUiCommands_PostBrowseInitialize(object sender, GitUIBaseEventArgs e)
        {
            if (!_initialized)
                Initialize((Form)e.OwnerForm);

            // Correct enabled/visibility of our menu/tool strip items.

            bool validWorkingDir = GitCommands.Settings.Module.ValidWorkingDir();

            _gitReviewMenuItem.Enabled = validWorkingDir;

            bool showGerritItems = validWorkingDir && File.Exists(GitCommands.Settings.WorkingDir + ".gitreview");

            foreach (var item in _gerritMenuItems)
            {
                item.Visible = showGerritItems;
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

            var settingsMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "settingsToolStripMenuItem1");
            if (settingsMenu == null)
                throw new Exception("Cannot find settings menu");

            var mailMapMenuItem = settingsMenu.DropDownItems.Cast<ToolStripItem>().SingleOrDefault(p => p.Name == "editmailmapToolStripMenuItem");
            if (mailMapMenuItem == null)
                throw new Exception("Cannot find mailmap menu item");

            _gitReviewMenuItem = new ToolStripMenuItem
            {
                Text = "Edit .gitreview"
            };

            _gitReviewMenuItem.Click += gitReviewMenuItem_Click;

            settingsMenu.DropDownItems.Insert(
                settingsMenu.DropDownItems.IndexOf(mailMapMenuItem) + 1,
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
                Text = "Download Gerrit change",
                Image = Properties.Resources.GerritDownload,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Visible = false
            };

            downloadMenuItem.Click += downloadMenuItem_Click;

            toolStrip.Items.Insert(nextIndex++, downloadMenuItem);

            var publishMenuItem = new ToolStripButton
            {
                Text = "Publish Gerrit change",
                Image = Properties.Resources.GerritPublish,
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                Visible = false
            };

            publishMenuItem.Click += publishMenuItem_Click;

            toolStrip.Items.Insert(nextIndex++, publishMenuItem);

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

            _gitUiCommands.RaiseBrowseInitialize();
        }

        void downloadMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormGerritDownload(_gitUiCommands))
            {
                form.ShowDialog(_mainForm);
            }

            _gitUiCommands.RaiseBrowseInitialize();
        }

        void gitReviewMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormGitReview())
            {
                form.ShowDialog(_mainForm);
            }

            _gitUiCommands.RaiseBrowseInitialize();
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

        public bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            return false;
        }
    }
}
