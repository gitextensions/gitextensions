using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitUI.CommandsDialogs;

namespace GitUI.Commands
{
    internal sealed class SearchFileGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly IFindFilePredicateProvider _findFilePredicateProvider;

        public SearchFileGitExtensionCommand(
            GitUICommands gitUICommands)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _findFilePredicateProvider = new FindFilePredicateProvider();
        }

        public bool Execute()
        {
            var searchWindow = new SearchWindow<string>(FindFileMatches);

            Application.Run(searchWindow);

            if (searchWindow.SelectedItem != null)
            {
                // We need to return the file that has been found, the visual studio plugin uses the return value
                // to open the selected file.
                Console.WriteLine(Path.Combine(_gitUICommands.Module.WorkingDir, searchWindow.SelectedItem));

                return true;
            }

            return false;
        }

        private IEnumerable<string> FindFileMatches(string name)
        {
            var candidates = _gitUICommands.Module.GetFullTree(id: "HEAD");
            var predicate = _findFilePredicateProvider.Get(name, _gitUICommands.Module.WorkingDir);

            return candidates.Where(predicate);
        }
    }
}
