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
        /// <inheritdoc />
        public event EventHandler<GitUICommandsChangedEventArgs> UICommandsChanged;

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
                var oldCommands = _uiCommands;
                _uiCommands = value ?? throw new ArgumentNullException(nameof(value));
                UICommandsChanged?.Invoke(this, new GitUICommandsChangedEventArgs(oldCommands));
            }
        }

        /// <summary>Gets a <see cref="GitModule"/> reference.</summary>
        [NotNull]
        [Browsable(false)]
        public GitModule Module => UICommands.Module;

        /// <summary>
        /// For VS designer and translation test.
        /// </summary>
        protected GitModuleForm()
        {
        }

        protected GitModuleForm([NotNull] GitUICommands commands)
            : this(true, commands)
        {
        }

        protected GitModuleForm(bool enablePositionRestore, [NotNull] GitUICommands commands)
            : base(enablePositionRestore)
        {
            if (commands != null)
            {
                UICommands = commands;
            }
        }

        protected override bool ExecuteCommand(int command)
        {
            return ScriptRunner.ExecuteScriptCommand(this, Module, command)
                || base.ExecuteCommand(command);
        }
    }
}
