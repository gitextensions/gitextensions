using System.Collections.Immutable;
using Avalonia.Controls;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SubmodulesDialog;

// Twin of GitUI/CommandsDialogs/SubmodulesDialog/FormAddSubmodule.cs. Editable combo items
// are repository path strings so Avalonia retains their display text after selection.
public sealed partial class FormAddSubmodule : GitModuleForm
{
    private readonly TranslationString _remoteAndLocalPathRequired
        = new("A remote path and local path are required");

    public FormAddSubmodule()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormAddSubmodule(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        InitializeComponent();
        WireControls();
        AcceptButton = Add;
        InitializeComplete();

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Remotes.LoadRecentHistoryAsync);
        Directory.ItemsSource = repositoryHistory.Select(repository => repository.Path).ToArray();
        Directory.SelectedIndex = -1;
        Directory.Text = string.Empty;
        LocalPath.Text = string.Empty;
    }

    private void WireControls()
    {
        Browse.Click += BrowseClick;
        Add.Click += AddClick;
        Directory.SelectionChanged += DirectorySelectedIndexChanged;
        Directory.PropertyChanged += (sender, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                DirectoryTextUpdate(sender!, EventArgs.Empty);
            }
        };
        Branch.DropDownOpened += BranchDropDown;
    }

    private string GetDirectoryText()
        => Directory.SelectedItem as string ?? Directory.Text ?? string.Empty;

    private string GetBranchText()
        => Branch.SelectedItem as string ?? Branch.Text ?? string.Empty;

    private void BrowseClick(object? sender, EventArgs e)
    {
        string? userSelectedPath = OsShellUtil.PickFolder(this, GetDirectoryText());

        if (userSelectedPath is not null)
        {
            Directory.Text = userSelectedPath;
        }
    }

    private void AddClick(object? sender, EventArgs e)
    {
        string directory = GetDirectoryText();
        string localPath = LocalPath.Text ?? string.Empty;
        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(localPath))
        {
            MessageBoxes.Show(
                this,
                _remoteAndLocalPathRequired.Text,
                Text ?? string.Empty,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        using (WaitCursorScope.Enter())
        {
            ArgumentString command = Commands.AddSubmodule(directory, localPath, GetBranchText(), chkForce.IsChecked == true);
            FormProcess.ShowDialog(this, UICommands, arguments: command, Module.WorkingDir, input: null, useDialogSettings: true);
            Close();
        }
    }

    private void DirectorySelectedIndexChanged(object? sender, EventArgs e)
    {
        DirectoryTextUpdate(this, EventArgs.Empty);
    }

    private void BranchDropDown(object? sender, EventArgs e)
    {
        Branch.ItemsSource = LoadRemoteRepoBranches(Module.GitExecutable, GetDirectoryText()).ToArray();
    }

    private void DirectoryTextUpdate(object? sender, EventArgs e)
    {
        string path = PathUtil.GetRepositoryName(GetDirectoryText());

        if (path != string.Empty)
        {
            LocalPath.Text = path;
        }
    }

    /// <summary>
    /// Returns the branches of a remote repository as strings; ignores git errors and warnings.
    /// </summary>
    /// <remarks>
    /// <c>git ls-remote --heads "URL"</c> is completely independent from a local repo clone,
    /// so there is no need for a GitModule.
    /// </remarks>
    /// <param name="gitExecutable">The git executable.</param>
    /// <param name="url">The repo URL; can also be a local path.</param>
    private static IEnumerable<string> LoadRemoteRepoBranches(IExecutable gitExecutable, string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return [];
        }

        GitArgumentBuilder gitArguments = new("ls-remote") { "--heads", url.ToPosixPath().Quote() };
        string heads = gitExecutable.GetOutput(gitArguments);
        return heads.LazySplit('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(head =>
                    {
                        int branchIndex = head.IndexOf(GitRefName.RefsHeadsPrefix);
                        return branchIndex == -1 ? null : head[(branchIndex + GitRefName.RefsHeadsPrefix.Length)..];
                    })
                    .WhereNotNull()
                    .ToImmutableList();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormAddSubmodule form)
    {
        public ComboBox Directory => form.Directory;
        public TextBox LocalPath => form.LocalPath;
        public ComboBox Branch => form.Branch;

        public static IEnumerable<string> LoadRemoteRepoBranches(IExecutable gitExecutable, string url)
            => FormAddSubmodule.LoadRemoteRepoBranches(gitExecutable, url);
    }
}
