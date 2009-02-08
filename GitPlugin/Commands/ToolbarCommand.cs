// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using EnvDTE;
using EnvDTE80;
using System.IO;

namespace GitPlugin.Commands
{
        class ToolbarCommand<ItemCommandT> : CommandBase
            where ItemCommandT : ItemCommandBase, new()
        {
            public override void OnCommand(DTE2 application, OutputWindowPane pane)
            {
                // Check if we've selected stuff in the solution explorer and we currently have this as the active window.
                if ("Tool" == application.ActiveWindow.Kind &&
                    application.ActiveWindow.Caption.StartsWith("Solution Explorer") &&
                    application.SelectedItems.Count > 0)
                {
                    new ItemCommandT().OnCommand(application, pane);
                }
                // let's just see if the text editor is active
                else if ("Document" == application.ActiveWindow.Kind && application.ActiveDocument != null)
                {
					// Let's go through the filesystem to figure out the correct case of the file.
					string realname = ResolveFileNameWithCase(application.ActiveDocument.FullName);
					new ItemCommandT().OnExecute(null, realname, pane);
                }
            }

            static public string ResolveFileNameWithCase(string fullpath)
			{
				string dirname = Path.GetDirectoryName(fullpath);
				string basename = Path.GetFileName(fullpath).ToLower();
				DirectoryInfo info = new DirectoryInfo(dirname);
				FileInfo[] files = info.GetFiles();

				foreach(FileInfo file in files)
				{
					if(file.Name.ToLower() == basename)
					{
						return file.FullName;
					}
				}
				
				// Should never happen...
				return fullpath;
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
        }

}
