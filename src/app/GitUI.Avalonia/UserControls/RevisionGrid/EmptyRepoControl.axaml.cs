using ResourceManager;

namespace GitUI.UserControls.RevisionGrid;

public sealed partial class EmptyRepoControl : GitModuleControl
{
    private readonly TranslationString _repoHasNoCommits = new("This repository does not yet contain any commits.");

    /// <summary>For VS designer.</summary>
    public EmptyRepoControl()
        : this(false)
    {
    }

    public EmptyRepoControl(bool isBareRepository)
    {
        InitializeComponent();
        InitializeComplete();

        lblEmptyRepository.Content = _repoHasNoCommits.Text;

        if (isBareRepository)
        {
            btnEditGitIgnore.IsVisible = false;
            btnOpenCommitForm.IsVisible = false;
        }
        else
        {
            btnEditGitIgnore.Click += (_, e) => UICommands.StartEditGitIgnoreDialog(this, localExcludes: false);
            btnOpenCommitForm.Click += (_, e) => UICommands.StartCommitDialog(this);
        }

        // Avalonia stretches UserControl content in its ContentPresenter instead of using WinForms DockStyle.Fill.
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
    }
}
