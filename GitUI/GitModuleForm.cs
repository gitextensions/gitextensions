using GitCommands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public partial class GitModuleForm : GitExtensionsForm, IGitUICommandsSource
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
           : base()
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
