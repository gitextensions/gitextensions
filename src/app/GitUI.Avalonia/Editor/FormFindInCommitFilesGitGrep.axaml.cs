using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility.Git;

namespace GitUI;

internal partial class FormFindInCommitFilesGitGrep : GitExtensionsDialog
{
    private bool _hasLoaded = false;

    /// <summary>
    ///  Action to search for files in the commit using git grep.
    /// </summary>
    public Action<string> FilesGitGrepLocator = null!;

    /// <summary>
    /// Action to toggle the visibility of the "find in commit files" filter control.
    /// </summary>
    public Action<bool> FindInCommitFilesGitGrepToggle = null!;

    public FormFindInCommitFilesGitGrep()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormFindInCommitFilesGitGrep(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();

        btnSearch.Click += btnSearch_Click;
        chkMatchCase.IsCheckedChanged += chkMatchCase_CheckedChanged;
        chkMatchWholeWord.IsCheckedChanged += chkMatchWholeWord_CheckedChanged;
        chkShowSearchBox.IsCheckedChanged += chkShowSearchBox_CheckedChanged;
        txtOptions.TextChanged += txtOptions_TextChanged;
        Opened += (_, e) => OnShown(e);
        Closing += FormFindInCommitFilesGitGrep_FormClosing;
        AcceptButton = btnSearch;
        ManualSectionAnchorName = "diff";
        ManualSectionSubfolder = "browse_repository";

        InitializeComplete();
        ShowInTaskbar = false;
    }

    public string? GitGrepExpressionText
    {
        get => cboFindInCommitFilesGitGrep.Text;
        set
        {
            if (value is not null)
            {
                cboFindInCommitFilesGitGrep.Text = value;
            }

            cboFindInCommitFilesGitGrep.Focus();
        }
    }

    /// <summary>
    /// Set the search items in the search combobox dropdown,
    /// without changing the current search text.
    /// </summary>
    /// <param name="items">items to change</param>
    public void SetSearchItems(IEnumerable<object?> items)
    {
        string? search = cboFindInCommitFilesGitGrep.Text;
        TextBox? textBox = cboFindInCommitFilesGitGrep.GetVisualDescendants().OfType<TextBox>().FirstOrDefault();
        int selectionStart = textBox?.SelectionStart ?? 0;
        int selectionEnd = textBox?.SelectionEnd ?? selectionStart;

        cboFindInCommitFilesGitGrep.ItemsSource = items.ToArray();
        cboFindInCommitFilesGitGrep.Text = search;
        if (textBox is not null)
        {
            textBox.SelectionStart = Math.Min(selectionStart, search?.Length ?? 0);
            textBox.SelectionEnd = Math.Min(selectionEnd, search?.Length ?? 0);
        }
    }

    internal void SetShowFindInCommitFilesGitGrep(bool visible)
    {
        chkShowSearchBox.IsChecked = visible;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }

    private void FormFindInCommitFilesGitGrep_FormClosing(object? sender, WindowClosingEventArgs e)
    {
        // Close the search if search is not visible (or user has cleared input)
        if (string.IsNullOrEmpty(GitGrepExpressionText) || chkShowSearchBox.IsChecked != true)
        {
            FilesGitGrepLocator?.Invoke("");
        }
    }

    // Avalonia has OnOpened rather than WinForms OnShown; retain the original product method boundary.
    protected void OnShown(EventArgs e)
    {
        txtOptions.Text = AppSettings.GitGrepUserArguments.Value;
        chkMatchCase.IsChecked = !AppSettings.GitGrepIgnoreCase.Value;
        chkMatchWholeWord.IsChecked = AppSettings.GitGrepMatchWholeWord.Value;
        cboFindInCommitFilesGitGrep.Focus();
        _hasLoaded = true;
    }

    private void Search()
        => FilesGitGrepLocator?.Invoke(GitGrepExpressionText!);

    private void btnSearch_Click(object sender, EventArgs e)
    {
        Search();
    }

    private void chkMatchCase_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.GitGrepIgnoreCase.Value = chkMatchCase.IsChecked != true;
    }

    private void chkMatchWholeWord_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.GitGrepMatchWholeWord.Value = chkMatchWholeWord.IsChecked == true;
    }

    private void chkShowSearchBox_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.ShowFindInCommitFilesGitGrep.Value = chkShowSearchBox.IsChecked == true;
        if (!_hasLoaded)
        {
            return;
        }

        FindInCommitFilesGitGrepToggle?.Invoke(chkShowSearchBox.IsChecked == true);
    }

    private void txtOptions_TextChanged(object sender, EventArgs e)
    {
        AppSettings.GitGrepUserArguments.Value = txtOptions.Text ?? string.Empty;
    }

    // parity-scaffolding: exposes the original dialog state to focused parity tests.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor
    {
        private readonly FormFindInCommitFilesGitGrep _form;

        public TestAccessor(FormFindInCommitFilesGitGrep form)
        {
            _form = form;
        }

        public ComboBox SearchBox => _form.cboFindInCommitFilesGitGrep;
        public CheckBox MatchCase => _form.chkMatchCase;
        public CheckBox MatchWholeWord => _form.chkMatchWholeWord;
        public CheckBox ShowSearchBox => _form.chkShowSearchBox;
        public Button SearchButton => _form.btnSearch;
        public void Search() => _form.Search();
    }
}
