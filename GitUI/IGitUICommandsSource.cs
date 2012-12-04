using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitUI
{

    public delegate void GitUICommandsChangedEventHandler(IGitUICommandsSource sender, GitUICommands oldCommands);
    public delegate void GitUICommandsSourceSetEventHandler(object sender, IGitUICommandsSource uiCommandsSource);

    public interface IGitUICommandsSource
    {
        event GitUICommandsChangedEventHandler GitUICommandsChanged;
        GitUICommands UICommands { get; }

    }
}
