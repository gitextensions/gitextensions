using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormGitLog : GitExtensionsForm
    {
        private readonly SynchronizationContext syncContext;

        private FormGitLog()
            : base(true)
        {
            ShowInTaskbar = true;
            syncContext = SynchronizationContext.Current;
            InitializeComponent();
            Translate();
        }

        private void GitLogFormLoad(object sender, EventArgs e)
        {            
            SubscribeToEvents();
            RefreshLogItems();

            RefreshCommandCacheItems();
        }

        private void GitLogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnsubscribeFromEvents();
            instance = null;
        }

        private void RefreshLogItems()
        {
            if (TabControl.SelectedTab == tabPageCommandLog)
                RefreshListBox(LogItems, Settings.GitLog.GetCommands());
        }

        private void RefreshCommandCacheItems()
        {
            if (TabControl.SelectedTab == tabPageCommandCache)
                RefreshListBox(CommandCacheItems, GitCommandCache.CachedCommands());
        }

        private static void RefreshListBox(ListBox log, string[] items)
        {
            var selectLastIndex = log.Items.Count == 0 || log.SelectedIndex == log.Items.Count - 1;
            log.DataSource = items;
            if (selectLastIndex && log.Items.Count > 0)
                log.SelectedIndex = log.Items.Count - 1;
        }

        private void CommandCacheItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            string command = (string)CommandCacheItems.SelectedItem;

            byte[] cmdout, cmderr;
            if (GitCommandCache.TryGet(command, out cmdout, out cmderr))
            {
                commandCacheOutput.Text = command + "\n-------------------------------------\n\n";
                Encoding encoding = GitModule.SystemEncoding;
                commandCacheOutput.Text += EncodingHelper.DecodeString(cmdout, cmderr, ref encoding).Replace("\0", "\\0");
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

        private void UnsubscribeFromEvents()
        {
            Settings.GitLog.CommandsChanged -= OnCommandsLogChanged;
            GitCommandCache.CachedCommandsChanged -= OnCachedCommandsLogChanged;
        }

        private void SubscribeToEvents()
        {
            Settings.GitLog.CommandsChanged += OnCommandsLogChanged;
            GitCommandCache.CachedCommandsChanged += OnCachedCommandsLogChanged;
        }

        private void OnCommandsLogChanged()
        {
            syncContext.Post(_ => RefreshLogItems(), null);
        }

        private void OnCachedCommandsLogChanged()
        {
            syncContext.Post(_ => RefreshCommandCacheItems(), null);
        }

        #region Single instance static members
        private static FormGitLog instance;

        public static void ShowOrActivate(IWin32Window owner)
        {
            if (instance == null)
                (instance = new FormGitLog()).Show();
            else if (instance.WindowState == FormWindowState.Minimized)
                instance.WindowState = FormWindowState.Normal;
            else
                instance.Activate();
        }
        #endregion
    }
}