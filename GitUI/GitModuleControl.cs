using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI
{
    public sealed class GitUICommandsSourceEventArgs : EventArgs
    {
        public GitUICommandsSourceEventArgs(IGitUICommandsSource gitUiCommandsSource)
        {
            GitUICommandsSource = gitUiCommandsSource;
        }

        public IGitUICommandsSource GitUICommandsSource { get; }
    }

    /// <summary>
    /// Base class for a <see cref="UserControl"/> requiring <see cref="GitModule"/> and <see cref="GitUICommands"/>.
    /// </summary>
    public class GitModuleControl : GitExtensionsControl
    {
        private readonly object _lock = new();

        private int _isDisposed;

        /// <summary>
        /// Occurs after the <see cref="UICommandsSource"/> is set.
        /// Will only occur once, as the source cannot change after being set.
        /// </summary>
        [Browsable(false)]
        public event EventHandler<GitUICommandsSourceEventArgs>? UICommandsSourceSet;

        private IGitUICommandsSource? _uiCommandsSource;

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
                    throw new ArgumentException($"{nameof(UICommandsSource)} is already set.");
                }

                _uiCommandsSource = value ?? throw new ArgumentException($"Can not assign null value to {nameof(UICommandsSource)}.");
                OnUICommandsSourceSet(_uiCommandsSource);
            }
        }

        /// <summary>Gets the <see cref="UICommandsSource"/>'s <see cref="GitUICommands"/> reference.</summary>
        [Browsable(false)]
        public GitUICommands UICommands => UICommandsSource.UICommands;

        /// <summary>
        /// Gets the UI commands, if they've initialised.
        /// </summary>
        /// <remarks>
        /// <para>This method will not attempt to initialise the commands if they have not
        /// yet been initialised.</para>
        /// <para>By contrast, the <see cref="UICommands"/> property attempts to initialise
        /// the value if not previously initialised.</para>
        /// </remarks>
        public bool TryGetUICommands([NotNullWhen(returnValue: true)] out GitUICommands? commands)
        {
            commands = _uiCommandsSource?.UICommands;
            return commands is not null;
        }

        /// <summary>Gets the <see cref="UICommands"/>' <see cref="GitModule"/> reference.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GitModule Module => UICommands.Module;

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

        protected override CommandStatus ExecuteCommand(int command)
        {
            var result = ExecuteScriptCommand();
            if (!result.Executed)
            {
                result = base.ExecuteCommand(command);
            }

            return result;

            CommandStatus ExecuteScriptCommand()
            {
                var revisionGridControl = this as RevisionGridControl;
                if (revisionGridControl is null)
                {
                    revisionGridControl = (FindForm() as GitModuleForm)?.RevisionGridControl;
                }

                return Script.ScriptRunner.ExecuteScriptCommand(this, Module, command, UICommands, revisionGridControl);
            }
        }

        /// <summary>Raises the <see cref="UICommandsSourceSet"/> event.</summary>
        protected virtual void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
            UICommandsSourceSet?.Invoke(this, new GitUICommandsSourceEventArgs(source));
        }
    }
}
