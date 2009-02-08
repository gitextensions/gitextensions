// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using EnvDTE;
using EnvDTE80;

namespace GitPlugin.Commands
{
        // an item command is a command associated with selected items in solution explorer
        public abstract class ItemCommandBase : CommandBase
        {
            private bool m_executeForFileItems = true;
            private bool m_executeForProjectItems = true;

            protected ItemCommandBase()
            {
            }

            protected ItemCommandBase(bool executeForFileItems, bool executeForProjectItems)
            {
                m_executeForFileItems = executeForFileItems;
                m_executeForProjectItems = executeForProjectItems;
            }

            private const string m_fileItemGUID = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";

            public override void OnCommand(DTE2 application, OutputWindowPane pane)
            {
                foreach (SelectedItem sel in application.SelectedItems)
                {
                    if (m_executeForFileItems && sel.ProjectItem != null && m_fileItemGUID == sel.ProjectItem.Kind)
                        OnExecute(sel, sel.ProjectItem.get_FileNames(0), pane);
                    else if (m_executeForProjectItems && sel.Project != null)
                        OnExecute(sel, sel.Project.FullName, pane);
                }
            }

            public override bool IsEnabled(DTE2 application)
            {
                return application.SelectedItems.Count > 0;
            }

            public abstract void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane);
        }
}
