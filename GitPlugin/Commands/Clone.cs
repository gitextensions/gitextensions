﻿using System.IO;
using System.Linq;
using EnvDTE;

namespace GitPlugin.Commands
{
    public sealed class Clone : ItemCommandBase
    {
        protected override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            if (!string.IsNullOrEmpty(fileName) && Path.GetInvalidPathChars().Any(fileName.Contains))
                fileName = "";
            var directoryName = Path.GetDirectoryName(fileName);
            RunGitEx("clone", directoryName);
        }

        protected override CommandTarget SupportedTargets
        {
            get { return CommandTarget.Any; }
        }
    }
}
