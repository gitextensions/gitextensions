// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;

namespace GitPlugin.Commands
{
        // an item command is a command associated with selected items in solution explorer
        public abstract class ItemCommandBase : CommandBase
        {
            private readonly bool m_executeForFileItems = true;
            private readonly bool m_executeForProjectItems = true;

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
                if (application.SelectedItems.Count == 0)
                {
                    OnExecute(null, null, pane);
                }

                foreach (SelectedItem sel in application.SelectedItems)
                {
                    if (m_executeForFileItems && sel.ProjectItem != null && m_fileItemGUID == sel.ProjectItem.Kind)
                    {
                        //The try catch block belowe fixed issue 57:
                        //http://github.com/spdr870/gitextensions/issues/#issue/57
                        try
                        {
                            OnExecute(sel, sel.ProjectItem.get_FileNames(0), pane);
                        }
                        catch (ArgumentException)
                        {
                            if (sel.ProjectItem.FileCount > 0)
                            {
                                OnExecute(sel, sel.ProjectItem.get_FileNames(1), pane);
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    else
                        if (m_executeForProjectItems && sel.Project != null)
                            OnExecute(sel, sel.Project.FullName, pane);
                        else
                            MessageBox.Show("You need to select a file or project to use this function.", "Git", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }

            public override bool IsEnabled(DTE2 application)
            {
                return
                    ("Tool" == application.ActiveWindow.Kind &&
                    application.ActiveWindow.Caption.StartsWith("Solution Explorer") &&
                    application.SelectedItems.Count > 0) ||
                    ("Document" == application.ActiveWindow.Kind &&
                    application.ActiveDocument != null);

            }

            public abstract void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane);
        }
}
