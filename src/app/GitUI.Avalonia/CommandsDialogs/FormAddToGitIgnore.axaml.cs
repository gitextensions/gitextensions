using System.Text;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using Microsoft;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormAddToGitIgnore : GitModuleForm
{
    private readonly TranslationString _addToLocalExcludeTitle = new("Add file(s) to .git/info/exclude");
    private readonly TranslationString _matchingFilesString = new("{0} file(s) matched");
    private readonly TranslationString _updateStatusString = new("Updating ...");

    private readonly AsyncLoader _ignoredFilesLoader = new();
    private readonly IFullPathResolver _fullPathResolver = null!;
    private readonly bool _localExclude;

    // parity-scaffolding: Avalonia's view inventory and designer require a parameterless constructor.
    public FormAddToGitIgnore()
    {
        InitializeComponent();
        WireEvents();
        InitializeComplete();
    }

    public FormAddToGitIgnore(IGitUICommands commands, bool localExclude, params string[] filePatterns)
        : base(commands, enablePositionRestore: false)
    {
        _localExclude = localExclude;

        InitializeComponent();
        WireEvents();
        InitializeComplete();

        if (localExclude)
        {
            Text = _addToLocalExcludeTitle.Text;
        }

        if (filePatterns is not null)
        {
            FilePattern.Text = string.Join(Environment.NewLine, filePatterns);
        }

        _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
    }

    // WinForms wired FilePattern.TextChanged and AddToIgnore.Click in the Designer.
    private void WireEvents()
    {
        AddToIgnore.Click += AddToIgnoreClick;
        FilePattern.TextChanged += FilePattern_TextChanged;
    }

    private string? ExcludeFile
    {
        get
        {
            if (!_localExclude)
            {
                return _fullPathResolver.Resolve(".gitignore");
            }
            else
            {
                return Path.Join(Module.ResolveGitInternalPath("info"), "exclude");
            }
        }
    }

    private void AddToIgnoreClick(object sender, EventArgs e)
    {
        string[] patterns = [.. GetCurrentPatterns()];
        if (patterns.Length == 0)
        {
            Close();
            return;
        }

        try
        {
            string? fileName = ExcludeFile;
            Validates.NotNull(fileName);
            FileInfoExtensions.MakeFileTemporaryWritable(fileName, x =>
            {
                StringBuilder gitIgnoreFileAddition = new();

                if (File.Exists(fileName) && !File.ReadAllText(fileName, GitModule.SystemEncoding).EndsWith(Environment.NewLine))
                {
                    gitIgnoreFileAddition.Append(Environment.NewLine);
                }

                foreach (string pattern in patterns)
                {
                    gitIgnoreFileAddition.Append(pattern);
                    gitIgnoreFileAddition.Append(Environment.NewLine);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(x)!);
                using StreamWriter writer = new(x, append: true, GitModule.SystemEncoding);
                writer.Write(gitIgnoreFileAddition);
            });
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(this, ex.ToString(), TranslatedStrings.Error, WinFormsShims.MessageBoxButtons.OK, WinFormsShims.MessageBoxIcon.Error);
        }

        Close();
    }

    private void UpdatePreviewPanel(IReadOnlyList<string> ignoredFiles)
    {
        _NO_TRANSLATE_Preview.ItemsSource = ignoredFiles;
        _NO_TRANSLATE_filesWillBeIgnored.Content = string.Format(_matchingFilesString.Text, _NO_TRANSLATE_Preview.ItemCount);
        _NO_TRANSLATE_Preview.IsEnabled = true;
        noMatchPanel.IsVisible = _NO_TRANSLATE_Preview.ItemCount == 0;
    }

    private IEnumerable<string> GetCurrentPatterns()
    {
        return GetLines().Where(line => !string.IsNullOrEmpty(line));
    }

    private string[] GetLines()
        => FilePattern.Text?.Split(["\r\n", "\r", "\n"], StringSplitOptions.None) ?? [];

    private void FilePattern_TextChanged(object sender, EventArgs e)
    {
        _ignoredFilesLoader.Cancel();
        if (_NO_TRANSLATE_Preview.IsEnabled)
        {
            _ignoredFilesLoader.Delay = TimeSpan.FromMilliseconds(300);
            _NO_TRANSLATE_filesWillBeIgnored.Content = _updateStatusString.Text;
            _NO_TRANSLATE_Preview.ItemsSource = new List<string> { _updateStatusString.Text };
            _NO_TRANSLATE_Preview.IsEnabled = false;
        }

        _ignoredFilesLoader.LoadAsync(() => Module.GetIgnoredFiles(GetCurrentPatterns()), UpdatePreviewPanel);
    }

    protected override void OnClosed(EventArgs e)
    {
        _ignoredFilesLoader.Dispose();
        base.OnClosed(e);
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormAddToGitIgnore form)
    {
        public TextBox FilePattern => form.FilePattern;
        public ListBox Preview => form._NO_TRANSLATE_Preview;
        public Button AddToIgnore => form.AddToIgnore;
        public Button Cancel => form.btnCancel;
        public Border NoMatchPanel => form.noMatchPanel;

        public void UpdatePreview(IReadOnlyList<string> ignoredFiles) => form.UpdatePreviewPanel(ignoredFiles);
    }
}
