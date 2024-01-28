using GitCommands;

namespace GitUI;

public partial class SearchCommitForm : GitExtensionsForm
{
    private bool _hasLoaded = false;

    public SearchCommitForm()
    {
        InitializeComponent();
        InitializeComplete();

        ShowInTaskbar = false;
    }

    public string SearchFor
    {
        get => txtSearchFor.Text;
        set => txtSearchFor.Text = value;
    }

    public Action<string, int> SearchFunc;
    public Action<bool> EnableSearchBoxFunc;
    public ComboBox.ObjectCollection SearchItems { private get; set; }

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

    private void SearchCommitForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        // Close the search if search is not visible (or user has cleared input)
        if (string.IsNullOrEmpty(SearchFor) || !AppSettings.ShowSearchCommit)
        {
            SearchFunc?.Invoke("", 0);
        }
    }

    private void Search()
    {
        SearchFunc?.Invoke(SearchFor, 0);
        AddToSearchFilter(SearchFor);

        void AddToSearchFilter(string search)
        {
            if (txtSearchFor.Items.IndexOf(search) is int index && index >= 0)
            {
                if (index == 0)
                {
                    return;
                }

                txtSearchFor.Items.Insert(0, search);
                txtSearchFor.Items.RemoveAt(index + 1);
                txtSearchFor.Text = search;
                return;
            }

            const int SearchFilterMaxLength = 30;
            if (txtSearchFor.Items.Count == SearchFilterMaxLength)
            {
                txtSearchFor.Items.RemoveAt(SearchFilterMaxLength - 1);
            }

            txtSearchFor.Items.Insert(0, search);
        }
    }

    private void btnSearch_Click(object sender, EventArgs e)
    {
        Search();
    }

    private void SearchCommitForm_Load(object sender, EventArgs e)
    {
        txtOptions.Text = AppSettings.GitGrepUserArguments.Value;
        foreach (object item in SearchItems)
        {
            txtSearchFor.Items.Add(item);
        }

        chkMatchCase.Checked = !AppSettings.GitGrepIgnoreCase.Value;
        chkMatchWholeWord.Checked = AppSettings.GitGrepMatchWholeWord.Value;
        chkShowSearchBox.Checked = AppSettings.ShowSearchCommit;
        _hasLoaded = true;
    }

    private void txtSearchFor_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            Search();
        }
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
        AppSettings.ShowSearchCommit = chkShowSearchBox.Checked;
        if (!_hasLoaded)
        {
            return;
        }

        EnableSearchBoxFunc?.Invoke(chkShowSearchBox.Checked);
    }
}
