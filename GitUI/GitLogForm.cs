using System;
using System.Windows.Forms;
using GitCommands;
using System.Linq;
using GitCommands.Logging;
using System.Threading;

namespace GitUI
{
    public partial class GitLogForm : GitExtensionsForm
    {

        protected readonly SynchronizationContext syncContext;

        public GitLogForm()
        {
            ShowInTaskbar = true;
            syncContext = SynchronizationContext.Current;
            InitializeComponent();
            Translate();
        }

        private void GitLogFormFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("log");
        }

        private void GitLogFormLoad(object sender, EventArgs e)
        {
            RestorePosition("log");

            Settings.GitLog.CommandsChanged += (CommandLogger log) =>
            {
                RefreshLogItems(log);
            };

            GitCommandCache.CachedCommandsChanged += () =>
            {
                RefreshCommandCacheItems();
            };

            RefreshLogItems(Settings.GitLog);

            RefreshCommandCacheItems();

        }

        protected void RefreshLogItems(CommandLogger log)
        {
            SendOrPostCallback method = o =>
            {
                if (TabControl.SelectedTab == tabPageCommandLog)
                {
                    bool selectLastIndex = LogItems.Items.Count == 0 || LogItems.SelectedIndex == LogItems.Items.Count - 1;
                    LogItems.DataSource = log.Commands();
                    if (selectLastIndex && LogItems.Items.Count > 0)
                        LogItems.SelectedIndex = LogItems.Items.Count - 1;
                }
            };
            syncContext.Post(method, this);

        }

        protected void RefreshCommandCacheItems()
        {
            SendOrPostCallback method = o =>
            {
                if (TabControl.SelectedTab == tabPageCommandCache)
                {
                    bool selectLastIndex = CommandCacheItems.Items.Count == 0 || CommandCacheItems.SelectedIndex == CommandCacheItems.Items.Count - 1;
                    CommandCacheItems.DataSource = GitCommandCache.CachedCommands();
                    if (selectLastIndex && CommandCacheItems.Items.Count > 0)
                        CommandCacheItems.SelectedIndex = CommandCacheItems.Items.Count - 1;
                }

            };
            syncContext.Post(method, this);
        }


        private void CommandCacheItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            string command = CommandCacheItems.SelectedItem as string;

            string output;
            if (GitCommandCache.TryGet(command, Settings.Encoding, out output))
            {
                commandCacheOutput.Text = command + "\n-------------------------------------\n\n";
                commandCacheOutput.Text += output.Replace("\0", "\\0");
            }
            else
            {
                commandCacheOutput.Text = string.Empty;
            }
        }

        private void LogItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            string command = LogItems.SelectedItem as string;

            LogOutput.Text = command;
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshLogItems(Settings.GitLog);
            RefreshCommandCacheItems();
        }

        private void alwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
            alwaysOnTopCheckBox.Checked = this.TopMost;
        }
    }
}