using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

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
                RefreshListBox(LogItems, AppSettings.GitLog.GetCommands().Select(cle => cle.ToString()).ToArray());
        }

        private void RefreshCommandCacheItems()
        {
            if (TabControl.SelectedTab == tabPageCommandCache)
                RefreshListBox(CommandCacheItems, GitCommandCache.CachedCommands());
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

            byte[] cmdout, cmderr;
            if (GitCommandCache.TryGet(command, out cmdout, out cmderr))
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

        private void UnsubscribeFromEvents()
        {
            AppSettings.GitLog.CommandsChanged -= OnCommandsLogChanged;
            GitCommandCache.CachedCommandsChanged -= OnCachedCommandsLogChanged;
        }

        private void SubscribeToEvents()
        {
            AppSettings.GitLog.CommandsChanged += OnCommandsLogChanged;
            GitCommandCache.CachedCommandsChanged += OnCachedCommandsLogChanged;
        }

        private void OnCommandsLogChanged(object sender, EventArgs e)
        {
            syncContext.Post(_ => RefreshLogItems(), null);
        }

        private void OnCachedCommandsLogChanged(object sender, EventArgs e)
        {
            syncContext.Post(_ => RefreshCommandCacheItems(), null);
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
                instance.WindowState = FormWindowState.Normal;
            else
                instance.Activate();
        }

        #endregion
    }
}