using System.Text;
using GitCommands;
using GitCommands.Logging;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public sealed partial class FormGitCommandLog : GitExtensionsForm
    {
        private FormGitCommandLog()
            : base(enablePositionRestore: true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ShowInTaskbar = true;
            InitializeComponent();
            InitializeComplete();
            ActiveControl = LogItems;

            LogItems.DisplayMember = nameof(CommandLogEntry.ColumnLine);

            Font font = new(FontFamily.GenericMonospace, 9);
            LogItems.Font = font;
            CommandCacheItems.Font = font;
            LogOutput.Font = font;
            commandCacheOutput.Font = font;

            chkCaptureCallStacks.Checked = AppSettings.LogCaptureCallStacks;
            chkCaptureCallStacks.CheckedChanged += delegate { AppSettings.LogCaptureCallStacks = chkCaptureCallStacks.Checked; };

            chkWordWrap.CheckedChanged += delegate
            {
                LogOutput.WordWrap = chkWordWrap.Checked;
                commandCacheOutput.WordWrap = chkWordWrap.Checked;
            };

            Load += delegate
            {
                CommandLog.CommandsChanged += OnGitCommandLogChanged;
                GitModule.GitCommandCache.Changed += OnCachedCommandsLogChanged;

                RefreshLogItems();
                RefreshCommandCacheItems();
            };

            FormClosed += delegate
            {
                CommandLog.CommandsChanged -= OnGitCommandLogChanged;
                GitModule.GitCommandCache.Changed -= OnCachedCommandsLogChanged;
                instance = null;
            };

            void OnGitCommandLogChanged()
            {
                this.InvokeAndForget(RefreshLogItems);
            }

            void OnCachedCommandsLogChanged(object sender, EventArgs e)
            {
                this.InvokeAndForget(RefreshCommandCacheItems);
            }
        }

        private void RefreshLogItems()
        {
            if (TabControl.SelectedTab == tabPageCommandLog)
            {
                RefreshListBox(LogItems, CommandLog.Commands.ToArray());
            }
        }

        private void RefreshCommandCacheItems()
        {
            if (TabControl.SelectedTab == tabPageCommandCache)
            {
                RefreshListBox(CommandCacheItems, GitModule.GitCommandCache.GetCachedCommands());
            }
        }

        private static void RefreshListBox(ListBox log, object dataSource)
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
                log.DataSource = dataSource;
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
            var command = (string)CommandCacheItems.SelectedItem;

            if (GitModule.GitCommandCache.TryGet(command, out var cmdOut, out var cmdErr))
            {
                Encoding encoding = GitModule.SystemEncoding;
                commandCacheOutput.Text =
                    command +
                    "\n-------------------------------------\n\n" +
                    EncodingHelper.DecodeString(cmdOut, cmdErr, ref encoding).Replace("\0", "\\0");
            }
            else
            {
                commandCacheOutput.Text = string.Empty;
            }
        }

        private void LogItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            var entry = (CommandLogEntry)LogItems.SelectedItem;

            LogOutput.Text = entry.Detail;
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

        private void mnuSaveToFile_Click(object sender, EventArgs e)
        {
            using SaveFileDialog fileDialog = new()
            {
                Title = Name,
                DefaultExt = ".txt",
                AddExtension = true,
                Filter = "Text files (*.txt)|*.txt|CSV files|*.csv|All files *.*|*.*"
            };
            if (fileDialog.ShowDialog(this) == DialogResult.OK)
            {
                var separator = fileDialog.FileName.EndsWith("csv") ?
                    System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator :
                    "\t";
                File.WriteAllLines(
                    fileDialog.FileName,
                    CommandLog.Commands.Select(cle => cle.FullLine(separator)));
            }
        }

        private void mnuClear_Click(object sender, EventArgs e)
        {
            CommandLog.Clear();
        }

        private void mnuCopyCommandLine_Click(object sender, EventArgs e)
        {
            if (LogItems.SelectedItem is CommandLogEntry commandLogEntry)
            {
                Clipboard.SetText(commandLogEntry.CommandLine);
            }
        }

        #region Single instance static members

        private static FormGitCommandLog? instance;

        public static void ShowOrActivate(IWin32Window owner)
        {
            if (instance is null)
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
