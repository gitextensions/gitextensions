// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    public class ToolbarCommand<TCommand> : CommandBase
        where TCommand : ItemCommandBase, new()
    {
        public ToolbarCommand(bool runForSelection = false)
        {
            RunForSelection = runForSelection;
        }

        public override void OnCommand(_DTE application, OutputWindowPane pane)
        {
            var command = new TCommand { RunForSelection = RunForSelection };
            command.OnCommand(application, pane);
        }

        public override bool IsEnabled(_DTE application)
        {
            return new TCommand().IsEnabled(application);
        }
    }
}
