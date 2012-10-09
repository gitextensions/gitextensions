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

        private IGitUICommandsSource _UICommandsSource;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IGitUICommandsSource UICommandsSource 
        {
            get
            {
                if (_UICommandsSource == null)
                    SearchForUICommandsSource();
                return _UICommandsSource;
            }
            set
            {
                _UICommandsSource = value;
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

        private void SearchForUICommandsSource()
        {
            if (UICommandsSourceParentSearch)
            {
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
