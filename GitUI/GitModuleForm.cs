﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using GitCommands;
using GitUI.Infrastructure.Telemetry;
using GitUI.Script;

namespace GitUI
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Base <see cref="Form"/> that provides access to <see cref="GitModule"/> and <see cref="GitUICommands"/>.</summary>
    public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource
    {
        /// <inheritdoc />
        public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged;

        /// <summary>
        /// Indicates that the process is run by unit tests runner.
        /// </summary>
        internal static bool IsUnitTestActive { get; set; }

        public virtual RevisionGridControl? RevisionGridControl { get => null; }

        private GitUICommands? _uiCommands;

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
        [Browsable(false)]
        public GitModule Module => UICommands.Module;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        protected GitModuleForm()
        {
            if (!IsDesignMode && !IsUnitTestActive)
            {
                throw new InvalidOperationException(
                    "This constructor is only to be called by the Visual Studio designer, and the translation unit tests.");
            }
        }

        protected GitModuleForm(GitUICommands? commands, bool enablePositionRestore)
            : base(enablePositionRestore)
        {
            _uiCommands = commands;
            DiagnosticsClient.TrackPageView(GetType().FullName);
        }

        protected GitModuleForm([NotNull] GitUICommands commands)
            : this(commands, enablePositionRestore: true)
        {
        }

        protected override CommandStatus ExecuteCommand(int command)
        {
            var result = ScriptRunner.ExecuteScriptCommand(this, Module, command, UICommands, RevisionGridControl);

            if (!result.Executed)
            {
                result = base.ExecuteCommand(command);
            }

            return result;
        }
    }
}
