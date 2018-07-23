using System;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;

namespace GitExtensionsVSIX.Commands
{
    /// <summary>
    /// An item command is a command associated with selected items in solution explorer.
    /// </summary>
    public abstract class ItemCommandBase : CommandBase
    {
        public override void OnCommand(_DTE application, OutputWindowPane pane)
        {
            if (!RunForSelection)
            {
                var activeDocument = application.ActiveDocument;

                if (activeDocument?.ProjectItem == null)
                {
                    // no active document - try solution target
                    if (application.Solution.IsOpen && IsTargetSupported(CommandTarget.Solution))
                    {
                        OnExecute(null, application.Solution.FullName, pane);
                    }

                    // solution (or not supported) - try empty target
                    else if (IsTargetSupported(CommandTarget.Empty))
                    {
                        OnExecute(null, null, pane);
                    }

                    return;
                }

                var fileName = activeDocument.ProjectItem.FileNames[1];

                SelectedItem selectedItem = application.SelectedItems
                    .Cast<SelectedItem>()
                    .FirstOrDefault(solutionItem => solutionItem.ProjectItem != null && solutionItem.ProjectItem.FileNames[1] == fileName);

                OnExecute(selectedItem, fileName, pane);
                return;
            }

            if (application.SelectedItems.Count == 0)
            {
                // nothing is selected, so if we have current solution and command supports that target, execute command on whole solution
                if (application.Solution.IsOpen && IsTargetSupported(CommandTarget.Solution))
                {
                    OnExecute(null, application.Solution.FullName, pane);
                }

                // there is no opened solution, so try execute command for empty target
                if (IsTargetSupported(CommandTarget.Empty))
                {
                    OnExecute(null, null, pane);
                }

                return;
            }

            foreach (SelectedItem solutionItem in application.SelectedItems)
            {
                ExecuteOnSolutionItem(solutionItem, application, pane);
            }
        }

        private void ExecuteOnSolutionItem(SelectedItem solutionItem, _DTE application, OutputWindowPane pane)
        {
            if (solutionItem.ProjectItem != null && IsTargetSupported(GetProjectItemTarget(solutionItem.ProjectItem)))
            {
                OnExecute(solutionItem, solutionItem.ProjectItem.FileNames[1], pane);
                return;
            }

            if (solutionItem.Project != null && IsTargetSupported(CommandTarget.Project))
            {
                OnExecute(solutionItem, solutionItem.Project.FullName, pane);
                return;
            }

            if (application.Solution.IsOpen && IsTargetSupported(CommandTarget.Solution))
            {
                OnExecute(solutionItem, application.Solution.FullName, pane);
                return;
            }

            if (IsTargetSupported(CommandTarget.Empty))
            {
                OnExecute(solutionItem, null, pane);
                return;
            }

            MessageBox.Show("You need to select a file or project to use this function.", "Git Extensions", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public override bool IsEnabled(_DTE application)
        {
            return application.SelectedItems.Count == 0
                ? IsTargetSupported(application.Solution.IsOpen ? CommandTarget.Solution : CommandTarget.Empty)
                : application.SelectedItems
                    .Cast<SelectedItem>()
                    .All(item => IsTargetSupported(GetSelectedItemTarget(item, application)));
        }

        protected abstract void OnExecute(SelectedItem item, string fileName, OutputWindowPane pane);

        protected abstract CommandTarget SupportedTargets { get; }

        private bool IsTargetSupported(CommandTarget commandTarget)
        {
            return (SupportedTargets & commandTarget) == commandTarget;
        }

        private static CommandTarget GetSelectedItemTarget(SelectedItem selectedItem, _DTE application)
        {
            if (selectedItem.ProjectItem != null)
            {
                return GetProjectItemTarget(selectedItem.ProjectItem);
            }

            if (selectedItem.Project != null)
            {
                return CommandTarget.Project;
            }

            if (application.Solution.IsOpen)
            {
                return CommandTarget.Solution;
            }

            return CommandTarget.Empty;
        }

        private static CommandTarget GetProjectItemTarget(ProjectItem projectItem)
        {
            switch (projectItem.Kind.ToUpper())
            {
                case Constants.vsProjectItemKindPhysicalFile:
                    return CommandTarget.File;
                case Constants.vsProjectItemKindVirtualFolder:
                    return CommandTarget.VirtualFolder;
                case Constants.vsProjectItemKindPhysicalFolder:
                    return CommandTarget.PhysicalFolder;
                default:
                    return CommandTarget.Any;
            }
        }

        [Flags]
        protected enum CommandTarget
        {
            /// <summary>
            /// Solution file selected in solution explorer.
            /// </summary>
            Solution = 1,

            /// <summary>
            /// Project file selected in solution explorer.
            /// </summary>
            Project = 2,

            /// <summary>
            /// Physical folder selected in solution explorer.
            /// </summary>
            PhysicalFolder = 4,

            /// <summary>
            /// Project item file selected in solution explorer.
            /// </summary>
            File = 8,

            /// <summary>
            /// Virtual folder selected in solution explorer.
            /// </summary>
            VirtualFolder = 16,

            /// <summary>
            /// Nothing is selected, no current solution.
            /// </summary>
            Empty = 32,

            /// <summary>
            /// Any solution explorer item that presented by physical file.
            /// </summary>
            SolutionExplorerFileItem = Solution | Project | File,

            /// <summary>
            /// Any target including empty.
            /// </summary>
            Any = SolutionExplorerFileItem | PhysicalFolder | VirtualFolder | Empty
        }
    }
}
