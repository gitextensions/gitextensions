using System;
using System.ComponentModel;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    /// <summary>Base class for a <see cref="Form"/> requiring
    /// <see cref="GitModule"/> and <see cref="GitUICommands"/>.</summary>
    public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource
    {
        private GitUICommands _uiCommands;

        /// <summary>Gets a <see cref="GitUICommands"/> reference.</summary>
        [Browsable(false)]
        public GitUICommands UICommands
        {
            get
            {
                if (_uiCommands == null)
                {
                    throw new InvalidOperationException("UICommands is null");
                }

                return _uiCommands;
            }

            protected set
            {
                GitUICommands oldCommands = _uiCommands;
                _uiCommands = value;
                GitUICommandsChanged?.Invoke(this, new GitUICommandsChangedEventArgs(oldCommands));
            }
        }

        /// <summary>true if <see cref="UICommands"/> has been initialized.</summary>
        public bool IsUICommandsInitialized => _uiCommands != null;

        /// <summary>Gets a <see cref="GitModule"/> reference.</summary>
        [Browsable(false)]
        public GitModule Module => _uiCommands?.Module;

        public event EventHandler<GitUICommandsChangedEventArgs> GitUICommandsChanged;

        protected GitModuleForm()
        {
        }

        public GitModuleForm(GitUICommands commands)
            : this(true, commands)
        {
        }

        public GitModuleForm(bool enablePositionRestore, GitUICommands commands)
            : base(enablePositionRestore)
        {
            UICommands = commands;
        }

        protected override bool ExecuteCommand(int command)
        {
            if (ExecuteScriptCommand(command))
            {
                return true;
            }

            return base.ExecuteCommand(command);
        }

        protected bool ExecuteScriptCommand(int command)
        {
            return Script.ScriptRunner.ExecuteScriptCommand(this, Module, command);
        }
    }
}
