using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    public partial class Dashboard : GitModuleControl
    {
        private readonly TranslationString _cloneFork = new TranslationString("Clone {0} repository");
        private readonly TranslationString _cloneRepository = new TranslationString("Clone repository");
        private readonly TranslationString _createRepository = new TranslationString("Create new repository");
        private readonly TranslationString _develop = new TranslationString("Develop");
        private readonly TranslationString _donate = new TranslationString("Donate");
        private readonly TranslationString _issues = new TranslationString("Issues");
        private readonly TranslationString _openRepository = new TranslationString("Open repository");
        private readonly TranslationString _translate = new TranslationString("Translate");
        private readonly TranslationString _directoryIsNotAValidRepositoryCaption = new TranslationString("Open");
        private readonly TranslationString _directoryIsNotAValidRepository = new TranslationString("The selected item is not a valid git repository.\n\nDo you want to abort and remove it from the recent repositories list?");
        private readonly TranslationString _directoryIsNotAValidRepositoryOpenIt = new TranslationString("The selected item is not a valid git repository.\n\nDo you want to open it?");
        private readonly TranslationString _showCurrentBranch = new TranslationString("Show current branch");
        private bool _initialized;
        private readonly SplitterManager _splitterManager = new SplitterManager(new AppSettingsPath("Dashboard"));

        public Dashboard()
        {
            InitializeComponent();
            Translate();

            Bitmap image = Lemmings.GetPictureBoxImage(DateTime.Now);
            if (image != null)
            {
                pictureBox1.Image = image;
            }

            // Do this at runtime, because it is difficult to keep consistent at design time.
            pictureBox1.BringToFront();
            pictureBox1.Location = new Point(Width - pictureBox1.Image.Width - 10, Height - pictureBox1.Image.Height - 10);

            Load += Dashboard_Load;
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            //
            // create Show current branch menu item and add to Dashboard menu
            //
            var showCurrentBranchMenuItem = new ToolStripMenuItem(_showCurrentBranch.Text);
            showCurrentBranchMenuItem.Click += showCurrentBranchMenuItem_Click;
            showCurrentBranchMenuItem.Checked = AppSettings.DashboardShowCurrentBranch;

            var menuStrip = FindControl<MenuStrip>(Parent.Parent.Parent, p => true); // TODO: improve: Parent.Parent.Parent == FormBrowse
            var dashboardMenu = (ToolStripMenuItem)menuStrip.Items.Cast<ToolStripItem>().Single(p => p.Name == "dashboardToolStripMenuItem");
            dashboardMenu.DropDownItems.Add(showCurrentBranchMenuItem);
        }

        /// <summary>
        /// code duplicated from GerritPlugin.cs
        /// </summary>
        private T FindControl<T>(Control form, Func<T, bool> predicate)
            where T : Control
        {
            return FindControl(form.Controls, predicate);
        }

        /// <summary>
        /// code duplicated from GerritPlugin.cs
        /// </summary>
        private static T FindControl<T>(IEnumerable controls, Func<T, bool> predicate)
            where T : Control
        {
            foreach (Control control in controls)
            {
                if (control is T result && predicate(result))
                {
                    return result;
                }

                result = FindControl(control.Controls, predicate);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public void SaveSplitterPositions()
        {
            _splitterManager.SaveSplitters();
        }

        public event EventHandler<GitModuleEventArgs> GitModuleChanged;

        public virtual void OnModuleChanged(object sender, GitModuleEventArgs e)
        {
            GitModuleChanged?.Invoke(this, e);
        }

        public override void Refresh()
        {
            _initialized = false;
            ShowRecentRepositories();
        }

        public void ShowRecentRepositories()
        {
            if (!Visible)
            {
                return;
            }

            // Make sure the dashboard is only initialized once
            if (!_initialized)
            {
                _initialized = true;
            }

            commonSplitContainer.Panel1MinSize = 1;
            commonSplitContainer.Panel2MinSize = 1;
            splitContainer7.Panel1MinSize = 1;
            splitContainer7.Panel2MinSize = 1;

            pictureBox1.BringToFront();
        }

        private void showCurrentBranchMenuItem_Click(object sender, EventArgs e)
        {
            bool newValue = !AppSettings.DashboardShowCurrentBranch;
            AppSettings.DashboardShowCurrentBranch = newValue;
            ((ToolStripMenuItem)sender).Checked = newValue;
            Refresh();
        }

        public void SetSplitterPositions()
        {
            _splitterManager.AddSplitter(mainSplitContainer, "mainSplitContainer", 315);
            _splitterManager.RestoreSplitters();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Refresh();
        }

        private void groupLayoutPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] fileNameArray)
            {
                if (fileNameArray.Length != 1)
                {
                    return;
                }

                string dir = fileNameArray[0];
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    GitModule module = new GitModule(dir);

                    if (!module.IsValidGitWorkingDir())
                    {
                        DialogResult dialogResult = MessageBox.Show(this, _directoryIsNotAValidRepositoryOpenIt.Text,
                            _directoryIsNotAValidRepositoryCaption.Text, MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2);
                        if (dialogResult == DialogResult.No)
                        {
                            return;
                        }
                    }

                    ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(dir));
                    OnModuleChanged(this, new GitModuleEventArgs(module));
                }

                return;
            }

            var text = e.Data.GetData(DataFormats.UnicodeText) as string;
            if (!string.IsNullOrEmpty(text))
            {
                var lines = text.Split('\n');
                if (lines.Length != 1)
                {
                    return;
                }

                string url = lines[0];
                if (!string.IsNullOrEmpty(url))
                {
                    UICommands.StartCloneDialog(this, url, false, OnModuleChanged);
                }
            }
        }

        private void groupLayoutPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] fileNameArray)
            {
                if (fileNameArray.Length != 1)
                {
                    return;
                }

                string dir = fileNameArray[0];
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    // Allow drop (copy, not move) folders
                    e.Effect = DragDropEffects.Copy;
                }

                return;
            }

            var text = e.Data.GetData(DataFormats.UnicodeText) as string;
            if (!string.IsNullOrEmpty(text))
            {
                var lines = text.Split('\n');
                if (lines.Length != 1)
                {
                    return;
                }

                string url = lines[0];
                if (!string.IsNullOrEmpty(url))
                {
                    // Allow drop (copy, not move) folders
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }
    }
}
