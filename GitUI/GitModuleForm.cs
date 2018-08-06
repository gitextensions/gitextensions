using System;
using System.ComponentModel;
using System.Windows.Forms;
using GitCommands;
using GitUI.Script;
using JetBrains.Annotations;

namespace GitUI
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Base <see cref="Form"/> that provides access to <see cref="GitModule"/> and <see cref="GitUICommands"/>.</summary>
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
                // If this exception is seen, it's because the parameterless constructor was called.
                // That constructor is only for use by the VS designer, and translation unit tests.
                // Using it at run time is an error.
                return _uiCommands
                       ?? throw new InvalidOperationException(
                           $"{nameof(UICommands)} is null. {GetType().FullName} was constructed incorrectly.");
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
            : base(enablePositionRestore: true)
        {
            _uiCommands = commands;
        }

        protected override bool ExecuteCommand(int command)
        {
            return ScriptRunner.ExecuteScriptCommand(this, Module, command)
                || base.ExecuteCommand(command);
        }
    }
}
