using System;
using System.ComponentModel;
using GitCommands;

namespace GitUI
{
    public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource
    {
        private GitUICommands _uiCommands;
        [Browsable(false)]
        public GitUICommands UICommands
        {
            get
            {
                if (_uiCommands == null)
                    throw new NullReferenceException("Commands");

                return _uiCommands;
            }

            protected set
            {
                GitUICommands oldCommands = _uiCommands;
                _uiCommands = value;
                if (GitUICommandsChanged != null)
                    GitUICommandsChanged(this, oldCommands);
            }
        }

        public bool IsUICommandsInitialized
        {
            get
            {
                return _uiCommands != null;
            }
        }

        [Browsable(false)]
        public GitModule Module { get { return UICommands.Module; } }
        public event GitUICommandsChangedEventHandler GitUICommandsChanged;

        protected GitModuleForm()
        {            
        }

        public GitModuleForm(GitUICommands aCommands)
            : this(true, aCommands)
        {         
        }

        public GitModuleForm(bool enablePositionRestore, GitUICommands aCommands)
            : base(enablePositionRestore)
        {
            UICommands = aCommands;
        }

        protected override bool ExecuteCommand(int command)
        {
            if (ExecuteScriptCommand(command))
                return true;
            return base.ExecuteCommand(command);
        }

        protected bool ExecuteScriptCommand(int command)
        {
            return Script.ScriptRunner.ExecuteScriptCommand(this, Module, command);
        }
    }
}
