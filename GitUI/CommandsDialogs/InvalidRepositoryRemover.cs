using System.Linq;
using GitCommands;
using GitCommands.UserRepositoryHistory;

namespace GitUI.CommandsDialogs
{
    public interface IInvalidRepositoryRemover
    {
        /// <summary>
        /// Shows a dialog to remove the provided invalid repository, or all invalid repositories.
        /// </summary>
        /// <param name="repositoryPath">An invalid repository.</param>
        /// <returns><see langword="true"/> if any repositories were removed; otherwise <see langword="false"/>.</returns>
        /// <remarks>The method does not verify that the provided <paramref name="repositoryPath"/> is invalid.</remarks>
        bool ShowDeleteInvalidRepositoryDialog(string repositoryPath);
    }

    internal class InvalidRepositoryRemover : IInvalidRepositoryRemover
    {
        /// <inheritdoc/>
        public bool ShowDeleteInvalidRepositoryDialog(string repositoryPath)
        {
            int invalidPathCount = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadRecentHistoryAsync())
                                                                   .Count(repo => !GitModule.IsValidGitWorkingDir(repo.Path));
            string commandButtonCaptions = Strings.RemoveSelectedInvalidRepository;
            if (invalidPathCount > 1)
            {
                commandButtonCaptions =
                    string.Format("{0}|{1}", commandButtonCaptions, string.Format(Strings.RemoveAllInvalidRepositories, invalidPathCount));
            }

            int dialogResult = PSTaskDialog.cTaskDialog.ShowCommandBox(
                Title: Strings.Open,
                MainInstruction: Strings.DirectoryInvalidRepository,
                Content: "",
                CommandButtons: commandButtonCaptions,
                ShowCancelButton: true);

            if (dialogResult < 0)
            {
                /* Cancel */
                return false;
            }
            else if (PSTaskDialog.cTaskDialog.CommandButtonResult == 0)
            {
                /* Remove selected invalid repo */
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveRecentAsync(repositoryPath));
            }
            else if (PSTaskDialog.cTaskDialog.CommandButtonResult == 1)
            {
                /* Remove all invalid repos */
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveInvalidRepositoriesAsync(repoPath => GitModule.IsValidGitWorkingDir(repoPath)));
            }

            return true;
        }
    }
}
