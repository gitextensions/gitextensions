using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormInit.cs. The directory history items are the
// repository paths (strings) rather than Repository objects, and the folder picker is
// the shared FolderBrowserButton twin.
public sealed partial class FormInit : GitExtensionsDialog
{
    private readonly TranslationString _chooseDirectory =
        new("Please choose a directory.");

    private readonly TranslationString _chooseDirectoryCaption =
        new("Choose directory");

    private readonly TranslationString _chooseDirectoryNotFile =
        new("Cannot initialize a new repository on a file.\nPlease choose a directory.");

    private readonly TranslationString _initMsgBoxCaption =
        new("Create new repository");

    private readonly EventHandler<GitModuleEventArgs>? _gitModuleChanged;

    public FormInit()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="FormInit"/> class.
    /// </summary>
    /// <param name="commands">The <see cref="IGitUICommands"/> instance, mainly in its role as <see cref="IServiceProvider"/>.</param>
    /// <param name="dir">The initial directory path.</param>
    /// <param name="gitModuleChanged">The event handler for Git module changes.</param>
    public FormInit(IGitUICommands commands, string dir, EventHandler<GitModuleEventArgs>? gitModuleChanged)
        : base(commands, enablePositionRestore: true)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        _gitModuleChanged = gitModuleChanged;
        InitializeComponent();
        WireControls();
        AcceptButton = Init;

        InitializeComplete();

        IList<Repository> repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(RepositoryHistoryManager.Locals.LoadRecentHistoryAsync);
        _NO_TRANSLATE_Directory.ItemsSource = repositoryHistory.Select(repository => repository.Path).ToArray();
        _NO_TRANSLATE_Directory.SelectedIndex = -1;
        _NO_TRANSLATE_Directory.Text = string.IsNullOrEmpty(dir) ? AppSettings.DefaultCloneDestinationPath : dir;
    }

    private void WireControls()
    {
        Browse.PathShowingControl = _NO_TRANSLATE_Directory;
        Init.Click += InitClick;
    }

    private void InitClick(object sender, EventArgs e)
    {
        string directoryPath = _NO_TRANSLATE_Directory.SelectedItem as string ?? _NO_TRANSLATE_Directory.Text ?? string.Empty;

        if (!IsRootedDirectoryPath(directoryPath))
        {
            MessageBoxes.Show(this, _chooseDirectory.Text, _chooseDirectoryCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        if (File.Exists(directoryPath))
        {
            MessageBoxes.Show(this, _chooseDirectoryNotFile.Text, TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        GitModule module = new(UICommands.GetRequiredService<IGitExecutorProvider>(), directoryPath);

        if (!System.IO.Directory.Exists(module.WorkingDir))
        {
            System.IO.Directory.CreateDirectory(module.WorkingDir);
        }

        MessageBoxes.Show(this, module.Init(Central.IsChecked == true, Central.IsChecked == true), _initMsgBoxCaption.Text, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Information);

        _gitModuleChanged?.Invoke(this, new GitModuleEventArgs(module));

        ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.AddAsMostRecentAsync(directoryPath));
        Close();
    }

    private static bool IsRootedDirectoryPath(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            // this is going to throw if it's an invalid path (e.g. contains special chars)
            DirectoryInfo info = new(path);

            return Path.IsPathRooted(path.Trim());
        }
        catch (Exception)
        {
            // The code in the try block is expected to throw when the input is not a valid directory path
            // OR when the user does not have the required permission.
            // In both cases we return "false" since the path is not representing a valid "usable" directory.
            // This is also the reason why we are catching all kind of exception here and not IO-related ones.
            return false;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FormInit _form;

        public TestAccessor(FormInit form)
        {
            _form = form;
        }

        public Avalonia.Controls.ComboBox DirectoryCombo => _form._NO_TRANSLATE_Directory;

        public bool IsRootedDirectoryPath(string path)
        {
            return FormInit.IsRootedDirectoryPath(path);
        }
    }
}
