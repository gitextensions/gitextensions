using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormArchive.cs. The native Avalonia storage provider is
// the only platform boundary; archive selection and command construction retain the original
// code-behind shape.
public sealed partial class FormArchive : GitModuleForm
{
    private readonly TranslationString _saveFileDialogFilterZip =
        new("Zip file (*.zip)");

    private readonly TranslationString _saveFileDialogFilterTar =
        new("Tar file (*.tar)");

    private readonly TranslationString _saveFileDialogCaption =
        new("Save archive as");

    private readonly TranslationString _noRevisionSelected =
        new("You need to choose a target revision.");

    private GitRevision? _selectedRevision;
    public GitRevision? SelectedRevision
    {
        get { return _selectedRevision; }
        set
        {
            _selectedRevision = value;
            commitSummaryUserControl1.Revision = _selectedRevision;
        }
    }

    private GitRevision? _diffSelectedRevision;
    private GitRevision? DiffSelectedRevision
    {
        get { return _diffSelectedRevision; }
        set
        {
            _diffSelectedRevision = value;
            if (_diffSelectedRevision is null)
            {
                const string defaultString = "...";
                labelDateCaption.Text = $"{ResourceManager.TranslatedStrings.CommitDate}:";
                labelAuthor.Text = defaultString;
                gbDiffRevision.Header = defaultString;
                labelMessage.Text = defaultString;
            }
            else
            {
                labelDateCaption.Text = $"{ResourceManager.TranslatedStrings.CommitDate}: {_diffSelectedRevision.CommitDate}";
                labelAuthor.Text = _diffSelectedRevision.Author;
                gbDiffRevision.Header = _diffSelectedRevision.ObjectId.ToShortString();
                labelMessage.Text = _diffSelectedRevision.Subject;
            }
        }
    }

    public void SetDiffSelectedRevision(GitRevision? revision)
    {
        checkboxRevisionFilter.IsChecked = revision is not null;
        DiffSelectedRevision = revision;
    }

    public void SetPathArgument(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            checkBoxPathFilter.IsChecked = false;
            textBoxPaths.Text = string.Empty;
        }
        else
        {
            checkBoxPathFilter.IsChecked = true;
            textBoxPaths.Text = path;
        }
    }

    private enum OutputFormat
    {
        Zip,
        Tar
    }

    public FormArchive()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormArchive(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private void WireControls()
    {
        buttonArchiveRevision.Click += Save_Click;
        btnChooseRevision.Click += btnChooseRevision_Click;
        btnDiffChooseRevision.Click += btnDiffChooseRevision_Click;
        checkBoxPathFilter.IsCheckedChanged += checkBoxPathFilter_CheckedChanged;
        checkboxRevisionFilter.IsCheckedChanged += checkboxRevisionFilter_CheckedChanged;
        AcceptButton = buttonArchiveRevision;
        UpdateFilterState();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        buttonArchiveRevision.Focus();
        UpdateFilterState();
    }

    private void Save_Click(object? sender, EventArgs e)
    {
        this.InvokeAndForget(SaveAsync);
    }

    private async Task SaveAsync()
    {
        if (SelectedRevision is null || (checkboxRevisionFilter.IsChecked == true && DiffSelectedRevision is null))
        {
            MessageBoxes.Show(
                this,
                _noRevisionSelected.Text,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Exclamation);
            return;
        }

        FilePickerSaveOptions options = CreateSaveFileOptions();
        IStorageFile? file = await StorageProvider.SaveFilePickerAsync(options);
        string? fileName = file?.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        string arguments = BuildArchiveArguments(fileName);
        FormProcess.ShowDialog(this, UICommands, arguments, Module.WorkingDir, input: null, useDialogSettings: true);
        Close();
    }

    private FilePickerSaveOptions CreateSaveFileOptions()
    {
        OutputFormat outputFormat = GetSelectedOutputFormat();
        string fileFilterCaption = outputFormat == OutputFormat.Zip
            ? _saveFileDialogFilterZip.Text
            : _saveFileDialogFilterTar.Text;
        string extension = outputFormat == OutputFormat.Zip ? "zip" : "tar";
        return new FilePickerSaveOptions
        {
            Title = _saveFileDialogCaption.Text,
            SuggestedFileName = GetSuggestedFileName(),
            DefaultExtension = extension,
            FileTypeChoices =
            [
                new FilePickerFileType(fileFilterCaption) { Patterns = [$"*.{extension}"] },
            ],
        };
    }

    private string GetSuggestedFileName()
    {
        string? revision = SelectedRevision?.Guid;
        string filenameSuggestion = string.Format("{0}_{1}", new DirectoryInfo(Module.WorkingDir).Name, revision);
        string[] paths = GetPathLines();
        if (checkBoxPathFilter.IsChecked == true && paths.Length == 1 && !string.IsNullOrWhiteSpace(paths[0]))
        {
            filenameSuggestion += "_" + paths[0].Trim().Replace(".", "_", StringComparison.Ordinal);
        }

        return filenameSuggestion;
    }

    private string BuildArchiveArguments(string fileName)
    {
        string format = GetSelectedOutputFormat() == OutputFormat.Zip ? "zip" : "tar";
        return string.Format(
            "archive --format=\"{0}\" {1} --output {2} {3}",
            format,
            SelectedRevision?.Guid,
            fileName.Quote(),
            GetPathArgumentFromGui());
    }

    private string GetPathArgumentFromGui()
    {
        if (checkBoxPathFilter.IsChecked == true)
        {
            return string.Join(" ", GetPathLines().Select(path => path.QuoteNE()));
        }

        if (checkboxRevisionFilter.IsChecked == true)
        {
            IEnumerable<GitItemStatus> files = UICommands.Module
                .GetDiffFilesWithUntracked(
                    DiffSelectedRevision?.Guid,
                    SelectedRevision?.Guid,
                    StagedStatus.None,
                    noCache: false,
                    cancellationToken: default)
                .Where(file => !file.IsDeleted);
            return string.Join(" ", files.Select(file => file.Name.QuoteNE()));
        }

        return string.Empty;
    }

    private string[] GetPathLines()
        => (textBoxPaths.Text ?? string.Empty).Split(Delimiters.NewLines, StringSplitOptions.None);

    private OutputFormat GetSelectedOutputFormat()
    {
        return _NO_TRANSLATE_radioButtonFormatZip.IsChecked == true ? OutputFormat.Zip : OutputFormat.Tar;
    }

    private void btnChooseRevision_Click(object? sender, EventArgs e)
    {
        using FormChooseCommit chooseForm = new(UICommands, SelectedRevision?.Guid);
        if (chooseForm.ShowDialog(this) == WinFormsShims.DialogResult.OK && chooseForm.SelectedRevision is not null)
        {
            SelectedRevision = chooseForm.SelectedRevision;
        }
    }

    private void checkBoxPathFilter_CheckedChanged(object? sender, EventArgs e)
    {
        if (checkBoxPathFilter.IsChecked == true)
        {
            checkboxRevisionFilter.IsChecked = false;
        }

        UpdateFilterState();
    }

    private void btnDiffChooseRevision_Click(object? sender, EventArgs e)
    {
        using FormChooseCommit chooseForm = new(UICommands, DiffSelectedRevision?.Guid ?? string.Empty);
        if (chooseForm.ShowDialog(this) == WinFormsShims.DialogResult.OK && chooseForm.SelectedRevision is not null)
        {
            DiffSelectedRevision = chooseForm.SelectedRevision;
        }
    }

    private void checkboxRevisionFilter_CheckedChanged(object? sender, EventArgs e)
    {
        if (checkboxRevisionFilter.IsChecked == true)
        {
            checkBoxPathFilter.IsChecked = false;
        }

        UpdateFilterState();
    }

    private void UpdateFilterState()
    {
        textBoxPaths.IsEnabled = checkBoxPathFilter.IsChecked == true;
        bool revisionFilterEnabled = checkboxRevisionFilter.IsChecked == true;
        gbDiffRevision.IsEnabled = revisionFilterEnabled;
        btnDiffChooseRevision.IsEnabled = revisionFilterEnabled;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormArchive form)
    {
        public Button Archive => form.buttonArchiveRevision;
        public Button ChooseDiffRevision => form.btnDiffChooseRevision;
        public CheckBox PathFilter => form.checkBoxPathFilter;
        public CheckBox RevisionFilter => form.checkboxRevisionFilter;
        public TextBox Paths => form.textBoxPaths;
        public RadioButton Tar => form._NO_TRANSLATE_radioButtonFormatTar;
        public string? DiffAuthor => form.labelAuthor.Text;
        public object? DiffHeader => form.gbDiffRevision.Header;
        public string? DiffMessage => form.labelMessage.Text;

        public string BuildArchiveArguments(string fileName) => form.BuildArchiveArguments(fileName);
        public FilePickerSaveOptions CreateSaveFileOptions() => form.CreateSaveFileOptions();
        public string GetPathArgument() => form.GetPathArgumentFromGui();
    }
}
