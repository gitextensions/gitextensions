// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using System;
using EnvDTE;
using EnvDTE80;
using System.Windows.Forms;
using System.IO;

namespace GitPlugin.Commands
{
    public class FileHistory : ItemCommandBase
	{
        public FileHistory():   base(true, false)
        {
        }

        public override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("filehistory", fileName);
        }
	}
}
