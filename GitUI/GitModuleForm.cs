using System;
using System.ComponentModel;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    /// <summary>Base class for a <see cref="Form"/> requiring 
    /// <see cref="GitModule"/> and <see cref="GitUICommands"/>.</summary>
    public class GitModuleForm : GitExtensionsForm, IGitUICommandsSource
    {
        private GitUICommands _UICommands;
        /// <summary>Gets a <see cref="GitUICommands"/> reference.</summary>
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

        /// <summary>true if <see cref="UICommands"/> has been initialzed.</summary>
        public bool IsUICommandsInitialized
        {
            get
            {
                return _UICommands != null;
            }
        }

        /// <summary>Gets a <see cref="GitModule"/> reference.</summary>
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
