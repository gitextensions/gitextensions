using System;
using System.ComponentModel;
using GitCommands;

namespace GitUI
{
    public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource
    {
        private GitUICommands _UICommands;
        [Browsable(false)]
        public GitUICommands UICommands
        {
            get
            {
                if (_UICommands == null)
                    throw new NullReferenceException("Commands");

                return _UICommands;
            }

            protected set
            {
                GitUICommands oldCommands = _UICommands;
                _UICommands = value;
                if (GitUICommandsChanged != null)
                    GitUICommandsChanged(this, oldCommands);
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
            else
                return base.ExecuteCommand(command);
        }

        protected virtual bool ExecuteScriptCommand(int command)
        {
            return GitUI.Script.ScriptRunner.ExecuteScriptCommand(Module, command);
        }

    }
}
