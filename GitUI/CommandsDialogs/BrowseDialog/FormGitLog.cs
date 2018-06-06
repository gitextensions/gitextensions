using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormGitLog : GitExtensionsForm
    {
        private FormGitLog()
            : base(true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ShowInTaskbar = true;
            InitializeComponent();
            Translate();
        }

        private void GitLogFormLoad(object sender, EventArgs e)
        {
            AppSettings.GitLog.CommandsChanged += OnCommandsLogChanged;
            GitCommandCache.CachedCommandsChanged += OnCachedCommandsLogChanged;

            RefreshLogItems();
            RefreshCommandCacheItems();
        }

        private void GitLogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            AppSettings.GitLog.CommandsChanged -= OnCommandsLogChanged;
            GitCommandCache.CachedCommandsChanged -= OnCachedCommandsLogChanged;
            instance = null;
        }

        private void RefreshLogItems()
        {
            if (TabControl.SelectedTab == tabPageCommandLog)
            {
                RefreshListBox(LogItems, AppSettings.GitLog.GetCommands().Select(cle => cle.ToString()).ToArray());
            }
        }

        private void RefreshCommandCacheItems()
        {
            if (TabControl.SelectedTab == tabPageCommandCache)
            {
                RefreshListBox(CommandCacheItems, GitCommandCache.CachedCommands());
            }
        }

        private static void RefreshListBox(ListBox log, string[] items)
        {
            var isLastIndexSeleted = log.Items.Count == 0 || log.SelectedIndex == log.Items.Count - 1;
            var lastIndex = -1;
            if (!isLastIndexSeleted)
            {
                lastIndex = log.SelectedIndex;
            }

            try
            {
                log.BeginUpdate();
                log.DataSource = items;
            }
            finally
            {
                log.EndUpdate();
            }

            if (log.Items.Count < 1)
            {
                return;
            }

            // select the very last item first, then select the previously selected item, if any
            log.SelectedIndex = log.Items.Count - 1;
            if (isLastIndexSeleted)
            {
                log.SelectedIndex = log.Items.Count - 1;
            }
            else if (lastIndex >= 0)
            {
                log.SelectedIndex = lastIndex;
            }
        }

        private void CommandCacheItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            string command = (string)CommandCacheItems.SelectedItem;

            if (GitCommandCache.TryGet(command, out var cmdout, out var cmderr))
            {
                Encoding encoding = GitModule.SystemEncoding;
                commandCacheOutput.Text =
                    command +
                    "\n-------------------------------------\n\n" +
                    EncodingHelper.DecodeString(cmdout, cmderr, ref encoding).Replace("\0", "\\0");
            }
            else
            {
                commandCacheOutput.Text = string.Empty;
            }
        }

        private void LogItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            LogOutput.Text = (string)LogItems.SelectedItem;
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshLogItems();
            RefreshCommandCacheItems();
        }

        private void alwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = !TopMost;
            alwaysOnTopCheckBox.Checked = TopMost;
        }

        private void SaveToFileToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            using (var fileDialog = new SaveFileDialog
            {
                Title = Name,
                DefaultExt = ".txt",
                AddExtension = true
            })
            {
                fileDialog.Filter =
                    "All files (*.*)|*.*";
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    File.WriteAllText(fileDialog.FileName, AppSettings.GitLog.ToString());
                }
            }
        }

        private void OnCommandsLogChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await this.SwitchToMainThreadAsync();
                    RefreshLogItems();
                });
        }

        private void OnCachedCommandsLogChanged(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    await this.SwitchToMainThreadAsync();
                    RefreshCommandCacheItems();
                });
        }

        #region Single instance static members

        private static FormGitLog instance;

        public static void ShowOrActivate(IWin32Window owner)
        {
            if (instance == null)
            {
                (instance = new FormGitLog()).Show(owner);
            }
            else if (instance.WindowState == FormWindowState.Minimized)
            {
                instance.WindowState = FormWindowState.Normal;
            }
            else
            {
                instance.Activate();
            }
        }

        #endregion
    }
}