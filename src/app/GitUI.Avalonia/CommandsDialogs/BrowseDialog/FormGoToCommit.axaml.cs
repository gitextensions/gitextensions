using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;

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
    private IReadOnlyList<IGitRef> _tags = [];
    private IReadOnlyList<IGitRef> _branches = [];
    private bool _tagsDropDownOpened;
    private bool _branchesDropDownOpened;

    public FormGoToCommit()
    {
        InitializeComponent();
        WireEvents();
        InitializeHelpText();
        InitializeComplete();
    }

    public FormGoToCommit(IGitUICommands commands)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        WireEvents();
        InitializeHelpText();
        AcceptButton = goButton;
        InitializeComplete();
    }

    protected override void OnClosed(EventArgs e)
    {
        _branchesLoader.Cancel();
        _tagsLoader.Cancel();
        _tagsLoader.Dispose();
        _branchesLoader.Dispose();
        base.OnClosed(e);
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        LoadTagsAsync().FileAndForget();
        LoadBranchesAsync().FileAndForget();
        SetCommitExpressionFromClipboard();
        textboxCommitExpression.Focus();
    }

    /// <summary>
    /// returns null if revision does not exist (could not be revparsed).
    /// </summary>
    public ObjectId ValidateAndGetSelectedObjectId()
    {
        return Module.RevParse(_selectedRevision!);
    }

    private void commitExpression_TextChanged(object? sender, EventArgs e)
    {
        SetSelectedRevisionByFocusedControl();
    }

    private void Go()
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void goButton_Click(object? sender, EventArgs e)
    {
        Go();
    }

    private void linkGitRevParse_LinkClicked(object? sender, PointerReleasedEventArgs e)
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
                _tags = list;

                // Avalonia's editable ComboBox requires display strings; keep the IGitRef
                // objects beside it so selection retains the original identity semantics.
                comboBoxTags.ItemsSource = list.Select(item => item.LocalName).ToList();
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
                _branches = list;

                // Avalonia's editable ComboBox requires display strings; keep the IGitRef
                // objects beside it so selection retains the original identity semantics.
                comboBoxBranches.ItemsSource = list.Select(item => item.LocalName).ToList();
                SetSelectedRevisionByFocusedControl();
            });
    }

    private void comboBoxTags_Enter(object? sender, RoutedEventArgs e)
    {
        SetSelectedRevisionByFocusedControl();
    }

    private void comboBoxBranches_Enter(object? sender, RoutedEventArgs e)
    {
        SetSelectedRevisionByFocusedControl();
    }

    private void SetSelectedRevisionByFocusedControl()
    {
        if (textboxCommitExpression.IsKeyboardFocusWithin)
        {
            _selectedRevision = (textboxCommitExpression.Text ?? string.Empty).Trim();
        }
        else if (comboBoxTags.IsKeyboardFocusWithin)
        {
            _selectedRevision = _selectedTag is not null ? _selectedTag.Guid : "";
        }
        else if (comboBoxBranches.IsKeyboardFocusWithin)
        {
            _selectedRevision = _selectedBranch is not null ? _selectedBranch.Guid : "";
        }
        else
        {
            textboxCommitExpression.Focus();
        }
    }

    private void comboBoxTags_TextChanged(object? sender, EventArgs e)
    {
        _selectedTag = _tags.FirstOrDefault(item => item.LocalName == comboBoxTags.Text);
        SetSelectedRevisionByFocusedControl();
    }

    private void comboBoxBranches_TextChanged(object? sender, EventArgs e)
    {
        _selectedBranch = _branches.FirstOrDefault(item => item.LocalName == comboBoxBranches.Text);
        SetSelectedRevisionByFocusedControl();
    }

    private void comboBoxTags_SelectionChangeCommitted(object? sender, EventArgs e)
    {
        if (!_tagsDropDownOpened || comboBoxTags.SelectedItem is not string selected)
        {
            return;
        }

        _tagsDropDownOpened = false;
        _selectedTag = _tags.FirstOrDefault(item => item.LocalName == selected);
        SetSelectedRevisionByFocusedControl();
        Go();
    }

    private void comboBoxBranches_SelectionChangeCommitted(object? sender, EventArgs e)
    {
        if (!_branchesDropDownOpened || comboBoxBranches.SelectedItem is not string selected)
        {
            return;
        }

        _branchesDropDownOpened = false;
        _selectedBranch = _branches.FirstOrDefault(item => item.LocalName == selected);
        SetSelectedRevisionByFocusedControl();
        Go();
    }

    private void comboBoxTags_KeyUp(object? sender, KeyEventArgs e)
    {
        GoIfEnterKey(sender, e);
    }

    private void comboBoxBranches_KeyUp(object? sender, KeyEventArgs e)
    {
        GoIfEnterKey(sender, e);
    }

    private void GoIfEnterKey(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Go();
        }
    }

    private void SetCommitExpressionFromClipboard()
    {
        string text = GitExtensions.Shims.WinForms.Clipboard.GetText().Trim();
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

    private void WireEvents()
    {
        goButton.Click += goButton_Click;
        textboxCommitExpression.TextChanged += commitExpression_TextChanged;
        linkGitRevParse.PointerReleased += linkGitRevParse_LinkClicked;
        comboBoxTags.GotFocus += comboBoxTags_Enter;
        comboBoxTags.PropertyChanged += (_, e) =>
        {
            if (e.Property == Avalonia.Controls.ComboBox.TextProperty)
            {
                comboBoxTags_TextChanged(comboBoxTags, EventArgs.Empty);
            }
        };
        comboBoxTags.KeyUp += comboBoxTags_KeyUp;
        comboBoxTags.DropDownOpened += (_, _) => _tagsDropDownOpened = true;
        comboBoxTags.DropDownClosed += comboBoxTags_SelectionChangeCommitted;
        comboBoxBranches.GotFocus += comboBoxBranches_Enter;
        comboBoxBranches.PropertyChanged += (_, e) =>
        {
            if (e.Property == Avalonia.Controls.ComboBox.TextProperty)
            {
                comboBoxBranches_TextChanged(comboBoxBranches, EventArgs.Empty);
            }
        };
        comboBoxBranches.KeyUp += comboBoxBranches_KeyUp;
        comboBoxBranches.DropDownOpened += (_, _) => _branchesDropDownOpened = true;
        comboBoxBranches.DropDownClosed += comboBoxBranches_SelectionChangeCommitted;
    }

    private void InitializeHelpText()
    {
        label2.Text = "Commit expression examples:\r\n- complete commit hash: e. g.: 8eab51fcb9c4538eb74c4dcd4c31ffd693ad25c9\r\n- partial commit hash (if unique): e. g.: 8eab51fcb9c453\r\n- tag name\r\n- branch name";
    }
}
