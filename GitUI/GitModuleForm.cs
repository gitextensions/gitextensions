using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI
{
    /// <summary>Base class for a <see cref="Form"/> requiring
    /// <see cref="GitModule"/> and <see cref="GitUICommands"/>.</summary>
    public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource
    {
        protected readonly TranslationString noDiffWarnLimit = new TranslationString("Diff warn settings missing");

        protected readonly TranslationString noDiffWarnLimitMessage = new TranslationString("There is no diff warn. Please go to settings and set a diff warn limit!");

        private readonly TranslationString diffWarnLimitExceeded = new TranslationString("Diff warn limit exceeded");

        private readonly TranslationString diffWarnLimitExceededConfirmation =
            new TranslationString("Diff warn limit exceeded.\nDo you want to continue?");

        private GitUICommands _uiCommands;
        /// <summary>Gets a <see cref="GitUICommands"/> reference.</summary>
        [Browsable(false)]
        public GitUICommands UICommands
        {
            get
            {
                if (_uiCommands == null)
                    throw new InvalidOperationException("UICommands is null");

                return _uiCommands;
            }

            protected set
            {
                GitUICommands oldCommands = _uiCommands;
                _uiCommands = value;
                if (GitUICommandsChanged != null)
                    GitUICommandsChanged(this, new GitUICommandsChangedEventArgs(oldCommands));
            }
        }

        /// <summary>true if <see cref="UICommands"/> has been initialzed.</summary>
        public bool IsUICommandsInitialized
        {
            get
            {
                return _uiCommands != null;
            }
        }

        /// <summary>Gets a <see cref="GitModule"/> reference.</summary>
        [Browsable(false)]
        public GitModule Module { get { return _uiCommands != null ? _uiCommands.Module : null; } }

        public event EventHandler<GitUICommandsChangedEventArgs> GitUICommandsChanged;

        protected GitModuleForm()
        {
        }

        public GitModuleForm(GitUICommands aCommands)
            : this(true, aCommands)
        {
        }

        public GitModuleForm(bool enablePositionRestore, GitUICommands aCommands)
            : base(enablePositionRestore)
        {
            UICommands = aCommands;
        }

        protected override bool ExecuteCommand(int command)
        {
            if (ExecuteScriptCommand(command))
                return true;
            return base.ExecuteCommand(command);
        }

        protected bool ExecuteScriptCommand(int command)
        {
            return Script.ScriptRunner.ExecuteScriptCommand(this, Module, command);
        }

        protected bool checkDiffSelectedItemsLimit(List<GitItemStatus> list)
        {
            bool ret = true;

            string diffWarnLimitStr = Module.GetEffectiveSetting("diff.warn.limit");
            if (string.IsNullOrEmpty(diffWarnLimitStr))
            {
                MessageBox.Show(this, noDiffWarnLimitMessage.Text, noDiffWarnLimit.Text);
                ret = false;
            }
            else
            {
                int diffWarnLimit = Int32.Parse(diffWarnLimitStr);
                if (list.Count > diffWarnLimit)
                {
                    if (MessageBox.Show(this, diffWarnLimitExceededConfirmation.Text, diffWarnLimitExceeded.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        ret = false;
                    }
                }
            }

            return ret;
        }
    }
}
