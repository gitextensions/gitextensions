using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace GitPlugin.Commands
{
    public class SolveMergeConflicts : ItemCommandBase
    {
        public SolveMergeConflicts()
            : base(true, true)
        {
        }

        public override void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane)
        {
            RunGitEx("mergeconflicts", fileName);
        }
    }

}
