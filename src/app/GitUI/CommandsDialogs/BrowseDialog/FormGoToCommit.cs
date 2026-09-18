using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;

namespace GitUI.CommandsDialogs.BrowseDialog;

public sealed partial class FormGoToCommit : GitModuleForm
{
    private const int _maxDropDownCount = 1_000;

    /// <summary>
    /// this will be used when Go() is called.
    /// </summary>
    private string? _selectedRevision;

    // these two are used to prepare for _selectedRevision
    private IGitRef? _selectedTag;
    private IGitRef? _selectedBranch;

    private readonly AsyncLoader _tagsLoader = new();
    private readonly AsyncLoader _branchesLoader = new();

    public FormGoToCommit(IGitUICommands commands)
        : base(commands)
    {
        InitializeComponent();
        InitializeComplete();
    }

    private void FormGoToCommit_Closed(object sender, FormClosedEventArgs e)
    {
        _branchesLoader.Cancel();
        _tagsLoader.Cancel();
    }

    private void FormGoToCommit_Load(object sender, EventArgs e)
    {
        LoadTagsAsync().FileAndForget();
        LoadBranchesAsync().FileAndForget();
        SetCommitExpressionFromClipboard();
    }

    /// <summary>
    /// returns null if revision does not exist (could not be revparsed).
    /// </summary>
    public ObjectId ValidateAndGetSelectedObjectId()
    {
        return Module.RevParse(_selectedRevision!);
    }

    private void commitExpression_TextChanged(object sender, EventArgs e)
    {
        SetSelectedRevisionByFocusedControl();
    }

    private void Go()
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void goButton_Click(object sender, EventArgs e)
    {
        Go();
    }

    private void linkGitRevParse_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        OsShellUtil.OpenUrlInDefaultBrowser(@"https://git-scm.com/docs/git-rev-parse#_specifying_revisions");
    }

    private Task LoadTagsAsync()
    {
        comboBoxTags.Text = TranslatedStrings.LoadingData;
        return _tagsLoader.LoadAsync(
            () => Module.GetRefs(RefsFilter.Tags).Take(_maxDropDownCount).ToList(),
            list =>
            {
                comboBoxTags.Text = string.Empty;
                comboBoxTags.DisplayMember = nameof(IGitRef.LocalName);
                comboBoxTags.DataSource = list;
                comboBoxTags.TextChanged += comboBoxTags_TextChanged;
                SetSelectedRevisionByFocusedControl();
            });
    }

    private Task LoadBranchesAsync()
    {
        comboBoxBranches.Text = TranslatedStrings.LoadingData;
        return _branchesLoader.LoadAsync(
            () => Module.GetRefs(RefsFilter.Heads).Take(_maxDropDownCount).ToList(),
            list =>
            {
                comboBoxBranches.Text = string.Empty;
                comboBoxBranches.DisplayMember = nameof(IGitRef.LocalName);
                comboBoxBranches.DataSource = list;
                comboBoxBranches.TextChanged += comboBoxBranches_TextChanged;
                SetSelectedRevisionByFocusedControl();
            });
    }

    private static IReadOnlyList<IGitRef> DataSourceToGitRefs(ComboBox cb)
    {
        return (IReadOnlyList<IGitRef>)cb.DataSource!;
    }

    private void comboBoxTags_Enter(object sender, EventArgs e)
    {
        SetSelectedRevisionByFocusedControl(comboBoxTags);
    }

    private void comboBoxBranches_Enter(object sender, EventArgs e)
    {
        SetSelectedRevisionByFocusedControl(comboBoxBranches);
    }

    /// <summary>
    ///  Updates <see cref="_selectedRevision"/> from the control which supplies the revision to go to.
    /// </summary>
    /// <param name="enteredControl">
    ///  The control raising <see cref="Control.Enter"/>. Such a control does not report itself as
    ///  <see cref="Control.Focused"/> yet, so it has to be recognised explicitly; otherwise the focus
    ///  would be moved away from it below. Pass <see langword="null"/> outside of that event.
    /// </param>
    private void SetSelectedRevisionByFocusedControl(Control? enteredControl = null)
    {
        if (IsSupplyingRevision(textboxCommitExpression))
        {
            _selectedRevision = textboxCommitExpression.Text.Trim();
        }
        else if (IsSupplyingRevision(comboBoxTags))
        {
            _selectedRevision = _selectedTag is not null ? _selectedTag.Guid : "";
        }
        else if (IsSupplyingRevision(comboBoxBranches))
        {
            _selectedRevision = _selectedBranch is not null ? _selectedBranch.Guid : "";
        }
        else
        {
            // No control supplies a revision, e.g. when the refs have just been loaded into the
            // combo boxes: offer the commit expression for input.
            textboxCommitExpression.Focus();
        }

        return;

        bool IsSupplyingRevision(Control control) => control.Focused || control == enteredControl;
    }

    private void comboBoxTags_TextChanged(object? sender, EventArgs e)
    {
        if (comboBoxTags.DataSource is null)
        {
            return;
        }

        _selectedTag = DataSourceToGitRefs(comboBoxTags).FirstOrDefault(a => a.LocalName == comboBoxTags.Text);
        SetSelectedRevisionByFocusedControl();
    }

    private void comboBoxBranches_TextChanged(object? sender, EventArgs e)
    {
        if (comboBoxBranches.DataSource is null)
        {
            return;
        }

        _selectedBranch = DataSourceToGitRefs(comboBoxBranches).FirstOrDefault(a => a.LocalName == comboBoxBranches.Text);
        SetSelectedRevisionByFocusedControl();
    }

    private void comboBoxTags_SelectionChangeCommitted(object sender, EventArgs e)
    {
        if (comboBoxTags.SelectedValue is null)
        {
            return;
        }

        _selectedTag = (IGitRef)comboBoxTags.SelectedValue;
        SetSelectedRevisionByFocusedControl();
        Go();
    }

    private void comboBoxBranches_SelectionChangeCommitted(object sender, EventArgs e)
    {
        if (comboBoxBranches.SelectedValue is null)
        {
            return;
        }

        _selectedBranch = (IGitRef)comboBoxBranches.SelectedValue;
        SetSelectedRevisionByFocusedControl();
        Go();
    }

    private void comboBoxTags_KeyUp(object sender, KeyEventArgs e)
    {
        GoIfEnterKey(sender, e);
    }

    private void comboBoxBranches_KeyUp(object sender, KeyEventArgs e)
    {
        GoIfEnterKey(sender, e);
    }

    private void GoIfEnterKey(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            Go();
        }
    }

    private void SetCommitExpressionFromClipboard()
    {
        string text = Clipboard.GetText().Trim();
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        ObjectId objectId = Module.RevParse(text);
        if (!objectId.IsZero)
        {
            textboxCommitExpression.Text = text;
            textboxCommitExpression.SelectAll();
        }
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _tagsLoader.Dispose();
            _branchesLoader.Dispose();

            components?.Dispose();
        }

        base.Dispose(disposing);
    }
}
