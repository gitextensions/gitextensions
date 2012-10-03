﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public class GitModuleControl : GitExtensionsControl
    {
        public bool UICommandsSourceParentSearch { get; set; }
        private IGitUICommandsSource _UICommandsSource;
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
        public GitModule Module { get { return UICommands.Module; } }

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
