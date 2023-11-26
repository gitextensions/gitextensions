﻿using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using GitCommands;
using GitUI.Infrastructure.Telemetry;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Base <see cref="Form"/> that provides access to <see cref="GitModule"/> and <see cref="GitUICommands"/>.</summary>
    public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource, IGitModuleForm
    {
        private IHotkeySettingsLoader? _hotkeySettingsLoader;
        private IScriptsRunner? _scriptsRunner;
        private GitUICommands? _uiCommands;

        /// <inheritdoc />
        public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged;

        /// <summary>
        /// Indicates that the process is run by unit tests runner.
        /// </summary>
        internal static bool IsUnitTestActive { get; set; }

        public IHotkeySettingsLoader HotkeySettingsReader
        {
            get => _hotkeySettingsLoader ?? throw new InvalidOperationException($"{GetType().FullName} was constructed incorrectly.");
            private set => _hotkeySettingsLoader = value;
        }

        public IScriptsRunner ScriptsRunner
        {
            get => _scriptsRunner ?? throw new InvalidOperationException($"{GetType().FullName} was constructed incorrectly.");
            private set => _scriptsRunner = value;
        }

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
                ArgumentNullException.ThrowIfNull(value);

                GitUICommands oldCommands = _uiCommands;
                _uiCommands = value;

                _hotkeySettingsLoader = _uiCommands.GetRequiredService<IHotkeySettingsLoader>();
                _scriptsRunner = _uiCommands.GetRequiredService<IScriptsRunner>();

                OnUICommandsChanged(new GitUICommandsChangedEventArgs(oldCommands));
            }
        }

        /// <summary>
        ///  Gets the current instance of the git module.
        /// </summary>
        [Browsable(false)]
        public IGitModule Module => UICommands.Module;

        IGitUICommands IGitModuleForm.UICommands => UICommands;

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
            if (commands is null && _uiCommands is null)
            {
                // In some cases we may want to initialise a form without a module,
                // e.g. a process dialog that executes a git command which is completely self contained.
            }
            else
            {
                UICommands = commands;
            }

            DiagnosticsClient.TrackPageView(GetType().FullName);
        }

        protected GitModuleForm([NotNull] GitUICommands commands)
            : this(commands, enablePositionRestore: true)
        {
        }

        protected override bool ExecuteCommand(int command)
        {
            return ScriptsRunner.RunScript(command, owner: this, UICommands)
                || base.ExecuteCommand(command);
        }

        protected virtual void OnUICommandsChanged(GitUICommandsChangedEventArgs e)
        {
            UICommandsChanged?.Invoke(this, e);
        }
    }
}
