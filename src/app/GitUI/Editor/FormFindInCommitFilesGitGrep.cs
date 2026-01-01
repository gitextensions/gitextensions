using GitCommands;
using GitExtensions.Extensibility.Git;

namespace GitUI;

internal partial class FormFindInCommitFilesGitGrep : GitExtensionsDialog
{
    private bool _hasLoaded = false;

    /// <summary>
    ///  Action to search for files in the commit using git grep.
    /// </summary>
    public Action<string> FilesGitGrepLocator;

    /// <summary>
    /// Action to toggle the visibility of the "find in commit files" filter control.
    /// </summary>
    public Action<bool> FindInCommitFilesGitGrepToggle;

    public FormFindInCommitFilesGitGrep(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
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

            ActiveControl = cboFindInCommitFilesGitGrep;
        }
    }

    /// <summary>
    /// Set the search items in the search combobox dropdown,
    /// without changing the current search text.
    /// </summary>
    /// <param name="items">items to change</param>
    public void SetSearchItems(ComboBox.ObjectCollection items)
    {
        cboFindInCommitFilesGitGrep.BeginUpdate();
        string search = cboFindInCommitFilesGitGrep.Text;
        int selectionStart = cboFindInCommitFilesGitGrep.SelectionStart;
        int selectionLength = cboFindInCommitFilesGitGrep.SelectionLength;

        cboFindInCommitFilesGitGrep.Items.Clear();
        foreach (object item in items)
        {
            cboFindInCommitFilesGitGrep.Items.Add(item);
        }

        cboFindInCommitFilesGitGrep.Text = search;
        cboFindInCommitFilesGitGrep.SelectionStart = selectionStart;
        cboFindInCommitFilesGitGrep.SelectionLength = selectionLength;
        cboFindInCommitFilesGitGrep.EndUpdate();
    }

    internal void SetShowFindInCommitFilesGitGrep(bool visible)
    {
        chkShowSearchBox.Checked = visible;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Escape)
        {
            Close();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void FormFindInCommitFilesGitGrep_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Close the search if search is not visible (or user has cleared input)
        if (string.IsNullOrEmpty(GitGrepExpressionText) || !chkShowSearchBox.Checked)
        {
            FilesGitGrepLocator?.Invoke("");
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        txtOptions.Text = AppSettings.GitGrepUserArguments.Value;
        chkMatchCase.Checked = !AppSettings.GitGrepIgnoreCase.Value;
        chkMatchWholeWord.Checked = AppSettings.GitGrepMatchWholeWord.Value;
        cboFindInCommitFilesGitGrep.Focus();
        _hasLoaded = true;
    }

    private void Search()
        => FilesGitGrepLocator?.Invoke(GitGrepExpressionText);

    private void btnSearch_Click(object sender, EventArgs e)
    {
        Search();
    }

    private void chkMatchCase_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.GitGrepIgnoreCase.Value = !chkMatchCase.Checked;
    }

    private void chkMatchWholeWord_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.GitGrepMatchWholeWord.Value = chkMatchWholeWord.Checked;
    }

    private void chkShowSearchBox_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.ShowFindInCommitFilesGitGrep.Value = chkShowSearchBox.Checked;
        if (!_hasLoaded)
        {
            return;
        }

        FindInCommitFilesGitGrepToggle?.Invoke(chkShowSearchBox.Checked);
    }

    private void txtOptions_TextChanged(object sender, EventArgs e)
    {
        AppSettings.GitGrepUserArguments.Value = txtOptions.Text;
    }
}
