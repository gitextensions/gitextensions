using System;
using System.ComponentModel;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public class GitModuleControl : GitExtensionsControl
    {
        [Browsable(false)]
        public bool UICommandsSourceParentSearch { get; private set; }

        [Browsable(false)]
        public event GitUICommandsSourceSetEventHandler GitUICommandsSourceSet;
        private IGitUICommandsSource _uiCommandsSource;

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

                if (GitUICommandsSourceSet != null)
                    GitUICommandsSourceSet(this, _uiCommandsSource);
            }

        }

        [Browsable(false)]
        public GitUICommands UICommands 
        { 
            get 
            {
                return UICommandsSource.UICommands;
            }
        }

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

            base.Dispose(disposing);
        }

        protected virtual void DisposeUICommandsSource()
        {
            _uiCommandsSource = null;
        }

        private void SearchForUICommandsSource()
        {
            if (!UICommandsSourceParentSearch)
                return;

            lock (this)
            {
                if (_uiCommandsSource != null)
                    return;

                IGitUICommandsSource cs = null;
                Control p = Parent;
                while (p != null && cs == null)
                {
                    if (p is IGitUICommandsSource)
                        cs = p as IGitUICommandsSource;
                    else
                        p = p.Parent;
                }

                UICommandsSource = cs;
            }
        }

        protected override bool ExecuteCommand(int command)
        {
            if (ExecuteScriptCommand(command))
                return true;
            return base.ExecuteCommand(command);
        }

        protected bool ExecuteScriptCommand(int command)
        {
            return Script.ScriptRunner.ExecuteScriptCommand(this, Module, command, this as RevisionGrid);
        }
    }
}
