using System.Collections.Generic;
using System.Drawing;
using GitCommands;

namespace GitUI.UserControls.ToolStripClasses
{
    internal interface ICommitIconProvider
    {
        Image DefaultIcon { get; }
        Image GetCommitIcon(IList<GitItemStatus> allChangedFiles);
    }
}