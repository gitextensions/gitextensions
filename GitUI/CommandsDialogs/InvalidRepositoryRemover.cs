using System.Linq;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using Microsoft.WindowsAPICodePack.Dialogs;

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
            int dialogResult = -1;

            using var dialog = new TaskDialog
            {
                InstructionText = Strings.DirectoryInvalidRepository,
                Caption = Strings.Open,
                Icon = TaskDialogStandardIcon.Error,
                StandardButtons = TaskDialogStandardButtons.Cancel,
                Cancelable = true,
            };
            var btnRemoveSelectedInvalidRepository = new TaskDialogCommandLink("RemoveSelectedInvalidRepository", null, Strings.RemoveSelectedInvalidRepository);
            btnRemoveSelectedInvalidRepository.Click += (s, e) =>
            {
                dialogResult = 0;
                dialog.Close();
            };
            dialog.Controls.Add(btnRemoveSelectedInvalidRepository);
            if (invalidPathCount > 1)
            {
                var btnRemoveAllInvalidRepositories = new TaskDialogCommandLink("RemoveAllInvalidRepositories", null, string.Format(Strings.RemoveAllInvalidRepositories, invalidPathCount));
                btnRemoveAllInvalidRepositories.Click += (s, e) =>
                {
                    dialogResult = 1;
                    dialog.Close();
                };
                dialog.Controls.Add(btnRemoveAllInvalidRepositories);
            }

            dialog.Show();

            switch (dialogResult)
            {
                case 0:
                    {
                        /* Remove selected invalid repo */
                        ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveRecentAsync(repositoryPath));
                        return true;
                    }

                case 1:
                    {
                        /* Remove all invalid repos */
                        ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveInvalidRepositoriesAsync(repoPath => GitModule.IsValidGitWorkingDir(repoPath)));
                        return true;
                    }

                default:
                    {
                        /* Cancel */
                        return false;
                    }
            }
        }
    }
}
