using System.Linq;
using System.Windows.Forms;
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

            TaskDialogPage page = new()
            {
                Heading = TranslatedStrings.DirectoryInvalidRepository,
                Caption = TranslatedStrings.Open,
                Icon = TaskDialogIcon.Error,
                Buttons = { TaskDialogButton.Cancel },
                AllowCancel = true,
                SizeToContent = true
            };
            TaskDialogCommandLinkButton btnRemoveSelectedInvalidRepository = new(TranslatedStrings.RemoveSelectedInvalidRepository);
            page.Buttons.Add(btnRemoveSelectedInvalidRepository);

            TaskDialogCommandLinkButton btnRemoveAllInvalidRepositories = new(string.Format(TranslatedStrings.RemoveAllInvalidRepositories, invalidPathCount));
            if (invalidPathCount > 1)
            {
                page.Buttons.Add(btnRemoveAllInvalidRepositories);
            }

            TaskDialogButton result = TaskDialog.ShowDialog(page);

            if (result == btnRemoveSelectedInvalidRepository)
            {
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveRecentAsync(repositoryPath));
                return true;
            }

            if (result == btnRemoveAllInvalidRepositories)
            {
                ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveInvalidRepositoriesAsync(repoPath => GitModule.IsValidGitWorkingDir(repoPath)));
                return true;
            }

            return false;
        }
    }
}
