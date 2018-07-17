using System;
using System.ComponentModel;
using System.Windows.Forms;
using GitCommands;
using GitUI.Script;
using JetBrains.Annotations;

namespace GitUI
{
    /// <summary>Base class for a <see cref="Form"/> requiring
    /// <see cref="GitModule"/> and <see cref="GitUICommands"/>.</summary>
    public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource
    {
        [CanBeNull] private GitUICommands _uiCommands;

        /// <inheritdoc />
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
                UICommandsChanged?.Invoke(this, new GitUICommandsChangedEventArgs(oldCommands));
            }
        }

        /// <summary>true if <see cref="UICommands"/> has been initialized.</summary>
        protected bool IsUICommandsInitialized => _uiCommands != null;

        /// <summary>Gets a <see cref="GitModule"/> reference.</summary>
        [CanBeNull]
        [Browsable(false)]
        public GitModule Module => _uiCommands?.Module;

        /// <inheritdoc />
        public event EventHandler<GitUICommandsChangedEventArgs> UICommandsChanged;

        protected GitModuleForm()
        {
        }

        protected GitModuleForm([CanBeNull] GitUICommands commands)
            : this(true, commands)
        {
        }

        protected GitModuleForm(bool enablePositionRestore, [CanBeNull] GitUICommands commands)
            : base(enablePositionRestore)
        {
            UICommands = commands;
        }

        protected override bool ExecuteCommand(int command)
        {
            return ScriptRunner.ExecuteScriptCommand(this, Module, command)
                || base.ExecuteCommand(command);
        }
    }
}
