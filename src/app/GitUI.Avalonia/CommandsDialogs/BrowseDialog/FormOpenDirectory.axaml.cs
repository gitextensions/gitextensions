using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.BrowseDialog;

public partial class FormOpenDirectory : GitExtensionsForm
{
    private readonly TranslationString _warningOpenFailed = new("The selected directory is not a valid git repository.");

    private readonly IGitExecutorProvider _executorProvider = null!;
    private IGitModule? _chosenModule;

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormOpenDirectory()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormOpenDirectory(IGitExecutorProvider executorProvider, IGitModule? currentModule)
    {
        _executorProvider = executorProvider;

        ThreadHelper.ThrowIfNotOnUIThread();

        InitializeComponent();
        Load.Click += LoadClick;
        folderBrowserButton.Click += folderBrowserButton_Click;
        folderGoUpButton.Click += folderGoUpButton_Click;
        _NO_TRANSLATE_Directory.KeyDown += DirectoryKeyDown;
        _NO_TRANSLATE_Directory.PropertyChanged += (sender, args) =>
        {
            if (args.Property == ComboBox.TextProperty)
            {
                _NO_TRANSLATE_Directory_TextChanged(sender, EventArgs.Empty);
            }
        };
        InitializeComplete();

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
        _NO_TRANSLATE_Directory.ItemsSource = GetDirectories(currentModule, repositoryHistory);

        _NO_TRANSLATE_Directory.Focus();
    }

    private static IReadOnlyList<string> GetDirectories(IGitModule? currentModule, IEnumerable<Repository> repositoryHistory)
    {
        List<string> directories = [];

        if (!string.IsNullOrWhiteSpace(AppSettings.DefaultCloneDestinationPath))
        {
            directories.Add(AppSettings.DefaultCloneDestinationPath.EnsureTrailingPathSeparator());
        }

        if (!string.IsNullOrWhiteSpace(currentModule?.WorkingDir))
        {
            DirectoryInfo di = new(currentModule.WorkingDir);
            if (di.Parent is not null)
            {
                directories.Add(di.Parent.FullName.EnsureTrailingPathSeparator());
            }
        }

        directories.AddRange(repositoryHistory.Select(r => r.Path));

        if (directories.Count == 0)
        {
            if (!string.IsNullOrWhiteSpace(AppSettings.RecentWorkingDir))
            {
                directories.Add(AppSettings.RecentWorkingDir.EnsureTrailingPathSeparator());
            }

            string homeDir = EnvironmentConfiguration.GetHomeDir();
            if (!string.IsNullOrWhiteSpace(homeDir))
            {
                directories.Add(homeDir.EnsureTrailingPathSeparator());
            }
        }

        return directories.Distinct().ToList();
    }

    public static IGitModule? OpenModule(WinFormsShims.IWin32Window owner, IGitExecutorProvider executorProvider, IGitModule? currentModule)
    {
        using FormOpenDirectory open = new(executorProvider, currentModule);
        open.ShowDialog(owner);
        return open._chosenModule;
    }

    private void LoadClick(object? sender, EventArgs e)
    {
        _NO_TRANSLATE_Directory.Text = (_NO_TRANSLATE_Directory.Text ?? string.Empty).Trim();

        _chosenModule = OpenGitRepository(_executorProvider, _NO_TRANSLATE_Directory.Text, RepositoryHistoryManager.Locals);
        if (_chosenModule is not null)
        {
            Close();
            return;
        }

        MessageBoxes.Show(this, _warningOpenFailed.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
    }

    private void DirectoryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            LoadClick(this, EventArgs.Empty);
        }
    }

    private void folderBrowserButton_Click(object? sender, EventArgs e)
    {
        string? userSelectedPath = OsShellUtil.PickFolder(this, _NO_TRANSLATE_Directory.Text);
        if (!string.IsNullOrEmpty(userSelectedPath))
        {
            _NO_TRANSLATE_Directory.Text = userSelectedPath;
            LoadClick(this, EventArgs.Empty);
        }
    }

    private void folderGoUpButton_Click(object? sender, EventArgs e)
    {
        try
        {
            DirectoryInfo currentDirectory = new(_NO_TRANSLATE_Directory.Text ?? string.Empty);
            if (currentDirectory.Parent is null)
            {
                return;
            }

            // WinForms appended a separator through SendKeys to trigger the file-system
            // auto-complete; Avalonia has no equivalent, so set the parent path directly.
            _NO_TRANSLATE_Directory.Text = currentDirectory.Parent.FullName.EnsureTrailingPathSeparator();
            _NO_TRANSLATE_Directory.Focus();
        }
        catch
        {
            // no-op
        }
    }

    private void _NO_TRANSLATE_Directory_TextChanged(object? sender, EventArgs e)
    {
        try
        {
            DirectoryInfo currentDirectory = new(_NO_TRANSLATE_Directory.Text ?? string.Empty);
            folderGoUpButton.IsEnabled = currentDirectory.Exists && currentDirectory.Parent is not null;
        }
        catch
        {
            folderGoUpButton.IsEnabled = false;
        }
    }

    private static IGitModule? OpenGitRepository(IGitExecutorProvider executorProvider, string path, ILocalRepositoryManager localRepositoryManager)
    {
        if (!Directory.Exists(path))
        {
            return null;
        }

        GitModule chosenModule = new(executorProvider, path.EnsureTrailingPathSeparator());
        if (!chosenModule.IsValidGitWorkingDir())
        {
            return null;
        }

        ThreadHelper.JoinableTaskFactory.Run(() => localRepositoryManager.AddAsMostRecentAsync(chosenModule.WorkingDir));
        return chosenModule;
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FormOpenDirectory _form;

        public TestAccessor(FormOpenDirectory form)
        {
            _form = form;
        }

        public ComboBox Directory => _form._NO_TRANSLATE_Directory;
        public Button OpenButton => _form.Load;

        public static IGitModule? OpenGitRepository(IGitExecutorProvider executorProvider, string path, ILocalRepositoryManager localRepositoryManager)
            => FormOpenDirectory.OpenGitRepository(executorProvider, path, localRepositoryManager);
    }
}
