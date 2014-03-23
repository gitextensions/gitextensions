using System;
using System.ComponentModel;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    /// <summary>Base class for a <see cref="UserControl"/> requiring 
    /// <see cref="GitModule"/> and <see cref="GitUICommands"/>.</summary>
    public class GitModuleControl : GitExtensionsControl
    {
        [Browsable(false)]
        public bool UICommandsSourceParentSearch { get; private set; }

        /// <summary>Occurs after the <see cref="UICommandsSource"/> is changed.</summary>
        [Browsable(false)]
        public event GitUICommandsSourceSetEventHandler GitUICommandsSourceSet;
        private IGitUICommandsSource _uiCommandsSource;


        /// <summary>Gets the <see cref="IGitUICommandsSource"/>.</summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IGitUICommandsSource UICommandsSource
        {
            get
            {
                if (_uiCommandsSource == null)
                    SearchForUICommandsSource();
                if (_uiCommandsSource == null)
                    throw new NullReferenceException("UICommandsSource");
                return _uiCommandsSource;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException("Can not assign null value to UICommandsSource");
                if (_uiCommandsSource != null)
                    throw new ArgumentException("UICommandsSource is already set");

                _uiCommandsSource = value;
                OnUICommandsSourceChanged(this, _uiCommandsSource);
            }
        }

        /// <summary>Gets the <see cref="UICommandsSource"/>'s <see cref="GitUICommands"/> reference.</summary>
        [Browsable(false)]
        public GitUICommands UICommands
        {
            get
            {
                return UICommandsSource.UICommands;
            }
        }

        /// <summary>Gets the <see cref="UICommands"/>' <see cref="GitModule"/> reference.</summary>
        [Browsable(false)]
        public GitModule Module
        {
            get
            {
                return UICommands.Module;
            }
        }

        public GitModuleControl()
        {
            UICommandsSourceParentSearch = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (_uiCommandsSource != null)
                DisposeUICommandsSource();

            DisposeCustomResources();

            base.Dispose(disposing);
        }

        protected virtual void DisposeCustomResources() { }

        /// <summary>Occurs when the <see cref="UICommandsSource"/> is disposed.</summary>
        protected virtual void DisposeUICommandsSource()
        {
            _uiCommandsSource = null;
        }

        /// <summary>Searches up the <see cref="UserControl"/>'s parent tree until it finds a <see cref="IGitUICommandsSource"/>.</summary>
        void SearchForUICommandsSource()
        {
            if (!UICommandsSourceParentSearch)
                return;

            lock (this)
            {
                if (_uiCommandsSource != null)
                    return;

                IGitUICommandsSource cmdsSrc = null;
                Control parent = Parent;
                while (parent != null && cmdsSrc == null)
                {
                    if (parent is IGitUICommandsSource)
                        cmdsSrc = parent as IGitUICommandsSource;
                    else
                        parent = parent.Parent;
                }

                UICommandsSource = cmdsSrc;
            }
        }

        protected override bool ExecuteCommand(int command)
        {
            return ExecuteScriptCommand(command)
                || base.ExecuteCommand(command);
        }

        /// <summary>Tries to run scripts identified by a <paramref name="command"/> 
        /// and returns true if any executed.</summary>
        protected bool ExecuteScriptCommand(int command)
        {
            return Script.ScriptRunner.ExecuteScriptCommand(this, Module, command, this as RevisionGrid);
        }

        /// <summary>Raises the <see cref="GitUICommandsSourceSet"/> event.</summary>
        protected virtual void OnUICommandsSourceChanged(object sender, IGitUICommandsSource newSource)
        {
            var handler = GitUICommandsSourceSet;
            if (handler != null)
                handler(this, newSource);
        }
    }
}
