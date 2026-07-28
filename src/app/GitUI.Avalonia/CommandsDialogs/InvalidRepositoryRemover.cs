using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitUI.Compat;

namespace GitUI.CommandsDialogs;

public interface IInvalidRepositoryRemover
{
    bool ShowDeleteInvalidRepositoryDialog(string repositoryPath);
}

internal sealed class InvalidRepositoryRemover : IInvalidRepositoryRemover
{
    public bool ShowDeleteInvalidRepositoryDialog(string repositoryPath)
    {
        int invalidPathCount = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync)
            .Count(repository => !GitModule.IsValidGitWorkingDir(repository.Path));
        TaskDialogPage page = new()
        {
            Heading = TranslatedStrings.DirectoryInvalidRepository,
            Caption = TranslatedStrings.Open,
            Icon = TaskDialogIcon.Error,
            Buttons = { TaskDialogButton.Cancel },
            AllowCancel = true,
            SizeToContent = true,
        };
        TaskDialogCommandLinkButton removeSelected = new(TranslatedStrings.RemoveSelectedInvalidRepository);
        page.Buttons.Add(removeSelected);
        TaskDialogCommandLinkButton removeAll = new(string.Format(TranslatedStrings.RemoveAllInvalidRepositories, invalidPathCount));
        if (invalidPathCount > 1)
        {
            page.Buttons.Add(removeAll);
        }

        TaskDialogButton result = TaskDialog.ShowDialog(owner: null, page);
        if (result == removeSelected)
        {
            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.RemoveRecentAsync(repositoryPath));
            return true;
        }

        if (result == removeAll)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                () => RepositoryHistoryManager.Locals.RemoveInvalidRepositoriesAsync(GitModule.IsValidGitWorkingDir));
            return true;
        }

        return false;
    }
}
