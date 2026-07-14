using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtUtils;

namespace GitUI.CommandsDialogs;

// TODO(avalonia-port): reduced shell for milestones M1.0/M1.1 — hosts the revision list.
// The left panel, diff viewer, commit info, menus, and toolbars of the real FormBrowse twin
// arrive in later milestones (see PLAN.md).
public partial class FormBrowse : Window
{
    public FormBrowse()
    {
        InitializeComponent();
    }

    public FormBrowse(IServiceProvider serviceProvider, GitModule module)
        : this()
    {
        bool isValidWorkingDir = module.IsValidGitWorkingDir();
        string branchName = isValidWorkingDir ? module.GetSelectedBranch() : string.Empty;

        IAppTitleGenerator appTitleGenerator = serviceProvider.GetRequiredService<IAppTitleGenerator>();
        Title = appTitleGenerator.Generate(module.WorkingDir, isValidWorkingDir, branchName);

        if (isValidWorkingDir)
        {
            lblRepoPath.Text = $"{module.WorkingDir}  —  {branchName}";
            lblStatus.Text = $"git: {GitVersion.Current}";
            RevisionGrid.ReloadRevisions(module);
        }
        else
        {
            lblRepoPath.Text = "No git repository";
            lblStatus.Text = "Start the app inside a repository or pass one on the command line: GitExtensions.Avalonia browse <path>";
        }
    }
}
