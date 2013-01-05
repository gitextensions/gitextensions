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
        private IGitUICommandsSource _UICommandsSource;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IGitUICommandsSource UICommandsSource 
        {
            get
            {
                if (_UICommandsSource == null)
                    SearchForUICommandsSource();
                if (_UICommandsSource == null)
                    throw new NullReferenceException("UICommandsSource");
                return _UICommandsSource;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException("Can not assign null value to UICommandsSource");
                if (_UICommandsSource != null)
                    throw new ArgumentException("UICommandsSource is already set");

                _UICommandsSource = value;

                if (GitUICommandsSourceSet != null)
                    GitUICommandsSourceSet(this, _UICommandsSource);
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
            if (_UICommandsSource != null)
                DisposeUICommandsSource();

            base.Dispose(disposing);
        }

        protected virtual void DisposeUICommandsSource()
        {
            _UICommandsSource = null;
        }

        private void SearchForUICommandsSource()
        {
            if (!UICommandsSourceParentSearch)
                return;

            lock (this)
            {
                if (_UICommandsSource != null)
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
            else
                return base.ExecuteCommand(command);
        }

        protected virtual bool ExecuteScriptCommand(int command)
        {
            return GitUI.Script.ScriptRunner.ExecuteScriptCommand(Module, command);
        }
    }
}
