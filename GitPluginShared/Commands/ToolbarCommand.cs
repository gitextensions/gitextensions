// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using EnvDTE;
using EnvDTE80;

namespace GitPluginShared.Commands
{
    public class ToolbarCommand<ItemCommandT> : CommandBase
        where ItemCommandT : ItemCommandBase, new()
    {
        public ToolbarCommand(bool runForSelection = false)
        {
            RunForSelection = runForSelection;
        }

        public override void OnCommand(DTE2 application, OutputWindowPane pane)
        {
            var command = new ItemCommandT { RunForSelection = RunForSelection };
            command.OnCommand(application, pane);
        }

        public override bool IsEnabled(DTE2 application)
        {
            return new ItemCommandT().IsEnabled(application);
        }
    }

}
