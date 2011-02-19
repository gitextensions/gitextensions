using System;
using System.Windows.Forms;
using GitCommands;
using System.Linq;

namespace GitUI
{
    public partial class GitLogForm : GitExtensionsForm
    {
        public GitLogForm()
        {
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

            LogItems.DataSource = Settings.GitLog.Commands();

            CommandCacheItems.DataSource = GitCommandCache.CachedCommands().ToList();
        }

        private void CommandCacheItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            string command = CommandCacheItems.SelectedItem as string;

            string output;
            if (GitCommandCache.TryGet(command, out output))
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
    }
}