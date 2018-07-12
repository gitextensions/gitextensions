using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI
{
    /// <summary>
    /// Base class for a <see cref="UserControl"/> requiring <see cref="GitModule"/> and <see cref="GitUICommands"/>.
    /// </summary>
    public class GitModuleControl : GitExtensionsControl
    {
        private readonly object _lock = new object();

        [Browsable(false)]
        private bool UICommandsSourceParentSearch { get; }

        /// <summary>Occurs after the <see cref="UICommandsSource"/> is changed.</summary>
        [Browsable(false)]
        public event EventHandler<GitUICommandsSourceEventArgs> GitUICommandsSourceSet;

        private IGitUICommandsSource _uiCommandsSource;

        /// <summary>Gets the <see cref="IGitUICommandsSource"/>.</summary>
        [CanBeNull]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IGitUICommandsSource UICommandsSource
        {
            get
            {
                if (_uiCommandsSource == null && UICommandsSourceParentSearch)
                {
                    lock (_lock)
                    {
                        // Double check locking
                        if (_uiCommandsSource == null)
                        {
                            // Search ancestors for an implementation of IGitUICommandsSource
                            UICommandsSource = this.FindAncestors().OfType<IGitUICommandsSource>().FirstOrDefault()
                                               ?? throw new InvalidOperationException("The UI Command Source is not available for this control. Are you calling methods before adding it to the parent control?");
                        }
                    }
                }

                return _uiCommandsSource;
            }
            set
            {
                if (_uiCommandsSource != null)
                {
                    throw new ArgumentException($"{nameof(UICommandsSource)} is already set.");
                }

                _uiCommandsSource = value ?? throw new ArgumentException($"Can not assign null value to {nameof(UICommandsSource)}.");
                OnUICommandsSourceChanged(_uiCommandsSource);
            }
        }

        /// <summary>Gets the <see cref="UICommandsSource"/>'s <see cref="GitUICommands"/> reference.</summary>
        [Browsable(false)]
        public GitUICommands UICommands => UICommandsSource.UICommands;

        /// <summary>true if <see cref="UICommands"/> has been initialized.</summary>
        public bool IsUICommandsInitialized
        {
            get
            {
                try
                {
                    return UICommandsSource != null;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
        }

        /// <summary>Gets the <see cref="UICommands"/>' <see cref="GitModule"/> reference.</summary>
        [Browsable(false)]
        public GitModule Module => UICommands.Module;

        protected GitModuleControl()
        {
            UICommandsSourceParentSearch = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (_uiCommandsSource != null)
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
            _uiCommandsSource = null;
        }

        protected override bool ExecuteCommand(int command)
        {
            return ExecuteScriptCommand()
                || base.ExecuteCommand(command);

            bool ExecuteScriptCommand()
            {
                return Script.ScriptRunner.ExecuteScriptCommand(this, Module, command, this as RevisionGridControl);
            }
        }

        /// <summary>Raises the <see cref="GitUICommandsSourceSet"/> event.</summary>
        protected virtual void OnUICommandsSourceChanged(IGitUICommandsSource newSource)
        {
            GitUICommandsSourceSet?.Invoke(this, new GitUICommandsSourceEventArgs(newSource));
        }
    }
}
