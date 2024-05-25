using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using GitExtensions.Extensibility.Git;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI
{
    /// <summary>
    /// Base class for a <see cref="UserControl"/> requiring <see cref="IGitModule"/> and <see cref="GitUICommands"/>.
    /// </summary>
    public class GitModuleControl : GitExtensionsControl, IGitModuleControl
    {
        private readonly object _lock = new();
        private int _isDisposed;
        private IGitUICommandsSource? _uiCommandsSource;

        /// <summary>
        /// Occurs after the <see cref="UICommandsSource"/> is set.
        /// Will only occur once, as the source cannot change after being set.
        /// </summary>
        [Browsable(false)]
        public event EventHandler<GitUICommandsSourceEventArgs>? UICommandsSourceSet;

        /// <summary>
        /// Gets a <see cref="IGitUICommandsSource"/> for this control.
        /// </summary>
        /// <remarks>
        /// If the commands source has not yet been initialised, this property's getter attempts
        /// to find a control-tree ancestor of type <see cref="IGitUICommandsSource"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Unable to initialise the source as
        /// no ancestor of type <see cref="IGitUICommandsSource"/> was found.</exception>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IGitUICommandsSource UICommandsSource
        {
            get
            {
                if (_uiCommandsSource is null)
                {
                    lock (_lock)
                    {
                        // Double check locking
                        if (_uiCommandsSource is null)
                        {
                            // Search ancestors for an implementation of IGitUICommandsSource
                            UICommandsSource = this.FindAncestors().OfType<IGitUICommandsSource>().FirstOrDefault()
                                               ?? throw new InvalidOperationException("The UI Command Source is not available for this control. Are you calling methods before adding it to the parent control?");
                        }
                    }
                }

                // ReSharper disable once AssignNullToNotNullAttribute
                return _uiCommandsSource!;
            }
            set
            {
                if (_uiCommandsSource is not null)
                {
                    throw new InvalidOperationException($"{nameof(UICommandsSource)} is already set.");
                }

                ArgumentNullException.ThrowIfNull(value);
                _uiCommandsSource = value;

                OnUICommandsSourceSet(_uiCommandsSource);
            }
        }

        /// <summary>
        ///  Gets the <see cref="UICommandsSource"/>'s <see cref="GitUICommands"/> reference.
        /// </summary>
        [Browsable(false)]
        public IGitUICommands UICommands => UICommandsSource.UICommands;

        IGitUICommands IGitModuleControl.UICommands => UICommandsSource.UICommands;

        /// <summary>
        /// Gets the UI commands, if they've initialised.
        /// </summary>
        /// <remarks>
        /// <para>This method will not attempt to initialise the commands if they have not
        /// yet been initialised.</para>
        /// <para>By contrast, the <see cref="UICommands"/> property attempts to initialise
        /// the value if not previously initialised.</para>
        /// </remarks>
        internal bool TryGetUICommandsDirect([NotNullWhen(returnValue: true)] out IGitUICommands? commands)
        {
            commands = _uiCommandsSource?.UICommands;
            return commands is not null;
        }

        /// <summary>Gets the <see cref="UICommands"/>' <see cref="IGitModule"/> reference.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IGitModule Module => UICommands.Module;

        protected GitModuleControl()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
            {
                return;
            }

            if (_uiCommandsSource is not null)
            {
                DisposeUICommandsSource();
            }

            DisposeCustomResources();

            base.Dispose(disposing);
        }

        protected virtual void DisposeCustomResources()
        {
        }

        /// <summary>Occurs when the <see cref="UICommandsSource"/> is disposed.</summary>
        protected virtual void DisposeUICommandsSource()
        {
        }

        protected override bool ExecuteCommand(int command)
        {
            return ExecuteScriptCommand()
                || base.ExecuteCommand(command);

            bool ExecuteScriptCommand()
            {
                IScriptsManager scriptsManager = UICommands.GetRequiredService<IScriptsManager>();
                ScriptInfo? scriptInfo = scriptsManager.GetScript(command);
                if (scriptInfo is null)
                {
                    return false;
                }

                IScriptsRunner scriptsRunner = UICommands.GetRequiredService<IScriptsRunner>();
                _ = scriptsRunner.RunScript(scriptInfo, owner: this, UICommands, FindScriptOptionsProvider());
                return true;

                IScriptOptionsProvider? FindScriptOptionsProvider()
                {
                    for (Control control = this; control != null; control = control.Parent)
                    {
                        if (control is GitModuleControl gitModuleControl && gitModuleControl.GetScriptOptionsProvider() is IScriptOptionsProvider scriptOptionsProvider)
                        {
                            return scriptOptionsProvider;
                        }

                        if (control is GitModuleForm gitModuleForm)
                        {
                            return gitModuleForm.GetScriptOptionsProvider();
                        }
                    }

                    return null;
                }
            }
        }

        protected virtual IScriptOptionsProvider? GetScriptOptionsProvider()
        {
            return null;
        }

        /// <summary>Raises the <see cref="UICommandsSourceSet"/> event.</summary>
        protected virtual void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
            UICommandsSourceSet?.Invoke(this, new GitUICommandsSourceEventArgs(source));
        }
    }
}
