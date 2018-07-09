﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Logging;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormGitCommandLog : GitExtensionsForm
    {
        private FormGitCommandLog()
            : base(true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ShowInTaskbar = true;
            InitializeComponent();
            Translate();

            var font = new Font(FontFamily.GenericMonospace, 9);
            LogItems.Font = font;
            CommandCacheItems.Font = font;
            LogOutput.Font = font;
            commandCacheOutput.Font = font;

            chkWordWrap.CheckedChanged += delegate
            {
                LogOutput.WordWrap = chkWordWrap.Checked;
                commandCacheOutput.WordWrap = chkWordWrap.Checked;
            };

            this.AdjustForDpiScaling();
        }

        private void GitLogFormLoad(object sender, EventArgs e)
        {
            CommandLog.CommandsChanged += OnGitCommandLogChanged;
            GitCommandCache.CachedCommandsChanged += OnCachedCommandsLogChanged;

            RefreshLogItems();
            RefreshCommandCacheItems();
        }

        private void GitLogForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CommandLog.CommandsChanged -= OnGitCommandLogChanged;
            GitCommandCache.CachedCommandsChanged -= OnCachedCommandsLogChanged;
            instance = null;
        }

        private void RefreshLogItems()
        {
            if (TabControl.SelectedTab == tabPageCommandLog)
            {
                RefreshListBox(LogItems, CommandLog.Commands.Select(cle => cle.ToString()).ToArray());
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
            var isLastIndexSelected = log.Items.Count == 0 || log.SelectedIndex == log.Items.Count - 1;
            var lastIndex = -1;
            if (!isLastIndexSelected)
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
            if (isLastIndexSelected)
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
            chkAlwaysOnTop.Checked = TopMost;
        }

        private void SaveToFileToolStripMenuItem_Click(object sender, System.EventArgs e)
        {
            using (var fileDialog = new SaveFileDialog
            {
                Title = Name,
                DefaultExt = ".txt",
                AddExtension = true,
                Filter = "All files (*.*)|*.*"
            })
            {
                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    var contents = string.Join(Environment.NewLine, CommandLog.Commands.Select(cle => cle.ToString()));
                    File.WriteAllText(fileDialog.FileName, contents);
                }
            }
        }

        private void OnGitCommandLogChanged()
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

        private static FormGitCommandLog instance;

        public static void ShowOrActivate(IWin32Window owner)
        {
            if (instance == null)
            {
                (instance = new FormGitCommandLog()).Show(owner);
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