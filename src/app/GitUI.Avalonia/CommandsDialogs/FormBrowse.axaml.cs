using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitExtUtils;

namespace GitUI.CommandsDialogs;

// TODO(avalonia-port): skeleton shell proving milestone M1.0 "boots on Linux".
// It will be replaced by the real FormBrowse twin (revision grid, left panel, diff viewer)
// in milestones M1.1+ (see PLAN.md).
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

        lblHeading.Text = Title;
        lblRepoPath.Text = isValidWorkingDir
            ? $"Repository: {module.WorkingDir}"
            : "No git repository (start the app inside a repository or pass one on the command line)";
        lblBranch.Text = isValidWorkingDir ? $"Branch: {branchName}" : string.Empty;
        lblGitVersion.Text = $"git: {GitVersion.Current}";
        lblStatus.Text = "Avalonia port — Phase 1 skeleton. The revision grid, left panel, and diff viewer arrive in the next milestones (see PLAN.md).";
    }
}
