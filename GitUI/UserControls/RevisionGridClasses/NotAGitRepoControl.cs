using System;
using System.IO;
using GitCommands;
using ResourceManager;

namespace GitUI.UserControls.RevisionGridClasses
{
    // TODO do we actually need this class? I don't think it's possible to make GE show this UI anywhere!

    public sealed partial class NotAGitRepoControl : GitModuleControl
    {
        public event EventHandler<GitModuleEventArgs> GitModuleChanged;
        public event EventHandler<EventArgs> Resolved;

        private readonly TranslationString _notARepo = new TranslationString("The selected directory is not a git repository.");

        public NotAGitRepoControl()
        {
            InitializeComponent();
            Translate();
            label.Text = _notARepo.Text;

            InitRepository.Click += (_, e) => UICommands.StartInitializeDialog(this, Module.WorkingDir, OnModuleChanged);
            CloneRepository.Click += (_, e) =>
            {
                if (UICommands.StartCloneDialog(this, null, OnModuleChanged))
                {
                    Resolved?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        public void Reload()
        {
            var dir = Module.WorkingDir;

            if (string.IsNullOrEmpty(dir) ||
                !Directory.Exists(dir) ||
                Directory.GetFileSystemEntries(dir).Length == 0)
            {
                CloneRepository.Show();
            }
            else
            {
                CloneRepository.Hide();
            }
        }

        private void OnModuleChanged(object sender, GitModuleEventArgs e)
        {
            GitModuleChanged?.Invoke(this, e);
        }
    }
}
