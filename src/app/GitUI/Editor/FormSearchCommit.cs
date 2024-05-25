using GitCommands;
using GitExtensions.Extensibility.Git;

namespace GitUI;

public partial class FormSearchCommit : GitExtensionsDialog
{
    private bool _hasLoaded = false;

    public FormSearchCommit(IGitUICommands commands)
            : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        InitializeComplete();

        ShowInTaskbar = false;
    }

    public string? SearchFor
    {
        get => txtSearchFor.Text;
        set
        {
            if (value is not null)
            {
                txtSearchFor.Text = value;
            }

            ActiveControl = txtSearchFor;
        }
    }

    public Action<string, int> SearchFunc;
    public Action<bool> EnableSearchBoxFunc;

    /// <summary>
    /// Set the search items in the search combobox dropdown,
    /// without changing the current search text.
    /// </summary>
    /// <param name="items">items to change</param>
    public void SetSearchItems(ComboBox.ObjectCollection items)
    {
        txtSearchFor.BeginUpdate();
        string search = txtSearchFor.Text;
        int selectionStart = txtSearchFor.SelectionStart;
        int selectionLength = txtSearchFor.SelectionLength;

        txtSearchFor.Items.Clear();
        foreach (object item in items)
        {
            txtSearchFor.Items.Add(item);
        }

        txtSearchFor.Text = search;
        txtSearchFor.SelectionStart = selectionStart;
        txtSearchFor.SelectionLength = selectionLength;
        txtSearchFor.EndUpdate();
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

    private void FormSearchCommit_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Close the search if search is not visible (or user has cleared input)
        if (string.IsNullOrEmpty(SearchFor) || !AppSettings.ShowSearchCommit.Value)
        {
            SearchFunc?.Invoke("", 0);
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        txtOptions.Text = AppSettings.GitGrepUserArguments.Value;
        chkMatchCase.Checked = !AppSettings.GitGrepIgnoreCase.Value;
        chkMatchWholeWord.Checked = AppSettings.GitGrepMatchWholeWord.Value;
        chkShowSearchBox.Checked = AppSettings.ShowSearchCommit.Value;
        txtSearchFor.Focus();
        _hasLoaded = true;
    }

    private void Search()
        => SearchFunc?.Invoke(SearchFor, 0);

    private void btnSearch_Click(object sender, EventArgs e)
    {
        Search();
    }

    private void txtOptions_TextChanged(object sender, EventArgs e)
    {
        AppSettings.GitGrepUserArguments.Value = txtOptions.Text;
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
        AppSettings.ShowSearchCommit.Value = chkShowSearchBox.Checked;
        if (!_hasLoaded)
        {
            return;
        }

        EnableSearchBoxFunc?.Invoke(chkShowSearchBox.Checked);
    }
}
