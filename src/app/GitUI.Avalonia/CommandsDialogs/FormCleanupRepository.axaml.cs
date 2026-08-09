using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GitCommands;
using GitCommands.Git;
using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public partial class FormCleanupRepository : GitModuleForm
{
    private readonly TranslationString _reallyCleanupQuestion =
        new("Are you sure you want to cleanup the repository?");
    private readonly TranslationString _reallyCleanupQuestionCaption = new("Cleanup");

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormCleanupRepository()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormCleanupRepository(IGitUICommands commands)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        Preview.Click += Preview_Click;
        AddInclusivePath.Click += AddIncludePath_Click;
        Cleanup.Click += Cleanup_Click;
        _NO_TRANSLATE_Close.Click += Close_Click;
        checkBoxIncludePathFilter.IsCheckedChanged += checkBoxPathFilter_CheckedChanged;
        checkBoxExcludePathFilter.IsCheckedChanged += checkBoxExcludePathFilter_CheckedChanged;
        AddExclusivePath.Click += AddExcludePath_Click;
        InitializeComplete();
        PreviewOutput.IsReadOnly = true;

        checkBoxPathFilter_CheckedChanged(this, EventArgs.Empty);
        checkBoxExcludePathFilter_CheckedChanged(this, EventArgs.Empty);
    }

    public void SetPathArgument(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            checkBoxExcludePathFilter.IsChecked = false;
            textBoxExcludePaths.Text = "";

            checkBoxIncludePathFilter.IsChecked = false;
            textBoxIncludePaths.Text = "";
        }
        else
        {
            checkBoxExcludePathFilter.IsChecked = true;
            textBoxExcludePaths.Text = path;

            checkBoxIncludePathFilter.IsChecked = true;
            textBoxIncludePaths.Text = path;
        }
    }

    private void CleanUp(bool dryRun)
    {
        string? includePathArgument = GetInclusivePathArgumentFromGui();
        string? excludePathArguments = GetExclusivePathArgumentFromGui();
        CleanMode mode = GetCleanMode();
        ArgumentString cleanUpCmd = Commands.Clean(mode, dryRun, directories: RemoveDirectories.IsChecked == true,
            paths: includePathArgument, excludes: excludePathArguments);

        string cmdOutput = FormProcess.ReadDialog(this, UICommands, arguments: cleanUpCmd, Module.WorkingDir, input: null, useDialogSettings: true);
        PreviewOutput.Text = EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(cmdOutput);

        if (CleanSubmodules.IsChecked == true)
        {
            ArgumentString cleanSubmodulesCmd = Commands.CleanSubmodules(mode, dryRun, directories: RemoveDirectories.IsChecked == true, paths: includePathArgument);
            cmdOutput = FormProcess.ReadDialog(this, UICommands, arguments: cleanSubmodulesCmd, Module.WorkingDir, input: null, useDialogSettings: true);
            PreviewOutput.Text += EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(cmdOutput);
        }
    }

    private void Preview_Click(object? sender, EventArgs e)
    {
        CleanUp(dryRun: true);
    }

    private void Cleanup_Click(object? sender, EventArgs e)
    {
        if (MessageBoxes.Show(this, _reallyCleanupQuestion.Text, _reallyCleanupQuestionCaption.Text, WinFormsShims.MessageBoxButtons.YesNo, WinFormsShims.MessageBoxIcon.Question) == WinFormsShims.DialogResult.Yes)
        {
            CleanUp(dryRun: false);
        }
    }

    private CleanMode GetCleanMode()
    {
        if (RemoveAll.IsChecked == true)
        {
            return CleanMode.All;
        }

        if (RemoveNonIgnored.IsChecked == true)
        {
            return CleanMode.OnlyNonIgnored;
        }

        if (RemoveIgnored.IsChecked == true)
        {
            return CleanMode.OnlyIgnored;
        }

        throw new NotSupportedException($"Unknown value for {nameof(CleanMode)}.");
    }

    private string? GetInclusivePathArgumentFromGui()
    {
        if (checkBoxIncludePathFilter.IsChecked != true)
        {
            return null;
        }

        // 1. get all lines from text box which are not empty
        // 2. wrap lines with ""
        // 3. join together with space as separator
        return string.Join(" ", Lines(textBoxIncludePaths).Where(p => !string.IsNullOrEmpty(p)).Select(p => $"\"{p}\""));
    }

    private string? GetExclusivePathArgumentFromGui()
    {
        if (checkBoxExcludePathFilter.IsChecked != true)
        {
            return null;
        }

        // 1. get all lines from text box which are not empty
        // 2. Prepend lines with '--exclude='
        // 3. Replace whitespace with '?' and convert to POSIX path
        // 4. join together with space as separator
        return string.Join(" ", Lines(textBoxExcludePaths).Where(p => !string.IsNullOrEmpty(p)).Select(p => $"--exclude={p.Replace(" ", "?")}".ToPosixPath()));
    }

    private void Close_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void checkBoxPathFilter_CheckedChanged(object? sender, EventArgs e)
    {
        bool filterByPath = checkBoxIncludePathFilter.IsChecked == true;
        textBoxIncludePaths.IsEnabled = filterByPath;
        labelPathHintInclude.IsVisible = filterByPath;
    }

    private void checkBoxExcludePathFilter_CheckedChanged(object? sender, EventArgs e)
    {
        bool filterByPath = checkBoxExcludePathFilter.IsChecked == true;
        textBoxExcludePaths.IsEnabled = filterByPath;
        labelPathHintExclude.IsVisible = filterByPath;
    }

    private void AddIncludePath_Click(object? sender, EventArgs e)
    {
        string? path = RequestUserFolderPath();

        if (path is not null)
        {
            textBoxIncludePaths.Text += path;
        }
    }

    private void AddExcludePath_Click(object? sender, EventArgs e)
    {
        string? path = RequestUserFilePath();

        if (path is not null)
        {
            path = path.Replace(Module.WorkingDir, "");
            textBoxExcludePaths.Text += path;
        }
    }

    private string? RequestUserFolderPath()
    {
        if (!PortalPickerGuard.IsAvailable())
        {
            return null;
        }

        FolderPickerOpenOptions options = new()
        {
            AllowMultiple = false,
        };
        if (Directory.Exists(Module.WorkingDir))
        {
            options.SuggestedStartLocation = DispatcherPump.Wait(() => StorageProvider.TryGetFolderFromPathAsync(Module.WorkingDir));
        }

        IReadOnlyList<IStorageFolder> folders = DispatcherPump.Wait(() => StorageProvider.OpenFolderPickerAsync(options));
        string? selectedPath = folders.FirstOrDefault()?.TryGetLocalPath();

        string? subFoldersToClean;
        if (string.IsNullOrEmpty(selectedPath)
            || !(subFoldersToClean = selectedPath).StartsWith(Module.WorkingDir)
            || !Directory.Exists(subFoldersToClean)
            || subFoldersToClean.Equals(PathUtil.RemoveTrailingPathSeparator(Module.WorkingDirGitDir)))
        {
            return null;
        }

        checkBoxIncludePathFilter.IsChecked = true;
        textBoxIncludePaths.IsEnabled = true;
        if (textBoxIncludePaths.Text?.Length is not (null or 0))
        {
            textBoxIncludePaths.Text += Environment.NewLine;
        }

        string userPath = string.Join(Environment.NewLine, subFoldersToClean);
        return userPath;
    }

    private string? RequestUserFilePath()
    {
        if (!PortalPickerGuard.IsAvailable())
        {
            return null;
        }

        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
        };
        if (Directory.Exists(Module.WorkingDir))
        {
            options.SuggestedStartLocation = DispatcherPump.Wait(() => StorageProvider.TryGetFolderFromPathAsync(Module.WorkingDir));
        }

        IReadOnlyList<IStorageFile> files = DispatcherPump.Wait(() => StorageProvider.OpenFilePickerAsync(options));
        string? selectedPath = files.FirstOrDefault()?.TryGetLocalPath();

        string? fileToExclude;
        if (string.IsNullOrEmpty(selectedPath)
            || !(fileToExclude = selectedPath).StartsWith(Module.WorkingDir))
        {
            return null;
        }

        checkBoxExcludePathFilter.IsChecked = true;
        textBoxExcludePaths.IsEnabled = true;

        if (textBoxExcludePaths.Text?.Length is not (null or 0))
        {
            textBoxExcludePaths.Text += Environment.NewLine;
        }

        string userPath = string.Join(Environment.NewLine, fileToExclude);
        return userPath;
    }

    // WinForms TextBox.Lines has no Avalonia equivalent; split the text on newline boundaries.
    private static string[] Lines(TextBox textBox)
        => (textBox.Text ?? "").Replace("\r\n", "\n").Split('\n');

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormCleanupRepository form)
    {
        public RadioButton RemoveAll => form.RemoveAll;
        public RadioButton RemoveNonIgnored => form.RemoveNonIgnored;
        public RadioButton RemoveIgnored => form.RemoveIgnored;
        public CheckBox RemoveDirectories => form.RemoveDirectories;
        public CheckBox CleanSubmodules => form.CleanSubmodules;
        public CheckBox IncludePathFilter => form.checkBoxIncludePathFilter;
        public CheckBox ExcludePathFilter => form.checkBoxExcludePathFilter;
        public TextBox IncludePaths => form.textBoxIncludePaths;
        public TextBox ExcludePaths => form.textBoxExcludePaths;
        public TextBox PreviewOutput => form.PreviewOutput;
        public Button PreviewButton => form.Preview;
        public Button CleanupButton => form.Cleanup;

        public CleanMode GetCleanMode() => form.GetCleanMode();
        public string? GetInclusivePathArgumentFromGui() => form.GetInclusivePathArgumentFromGui();
        public string? GetExclusivePathArgumentFromGui() => form.GetExclusivePathArgumentFromGui();
    }
}
