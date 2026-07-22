using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormCherryPick.cs. The parent ListView is represented by
// translated headers and one typed ListBox; command and sequential-dialog behavior stay at
// their original code-behind boundaries.
public partial class FormCherryPick : GitExtensionsDialog
{
    #region Translation
    private readonly TranslationString _noneParentSelectedText =
        new("None parent is selected!");
    #endregion

    private bool _isMerge;

    public GitRevision? Revision { get; set; }

    public FormCherryPick()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormCherryPick(IGitUICommands commands, GitRevision? revision)
        : base(commands, enablePositionRestore: false)
    {
        Revision = revision;

        InitializeComponent();
        WireControls();
        ManualSectionAnchorName = "cherry-pick-commit";
        ManualSectionSubfolder = "modify_history";
        InitializeComplete();
    }

    private void WireControls()
    {
        lvParentsList.ItemTemplate = new FuncDataTemplate<CherryPickParentRow>(CreateParentRow, supportsRecycling: false);
        btnPick.Click += btnPick_Click;
        btnAbort.Click += btnAbort_Click;
        btnChooseRevision.Click += btnChooseRevision_Click;
        AcceptButton = btnPick;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (!TryGetUICommands(out _))
        {
            return;
        }

        LoadSettings();
        OnRevisionChanged();
        if (lvParentsList.IsVisible)
        {
            lvParentsList.Focus();
        }
        else
        {
            cbxAutoCommit.Focus();
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        SaveSettings();
        base.OnClosing(e);
    }

    private void LoadSettings()
    {
        cbxAutoCommit.IsChecked = AppSettings.CommitAutomaticallyAfterCherryPick;
        cbxAddReference.IsChecked = AppSettings.AddCommitReferenceToCherryPick;
    }

    private void SaveSettings()
    {
        if (DialogResult == WinFormsShims.DialogResult.OK)
        {
            AppSettings.CommitAutomaticallyAfterCherryPick = cbxAutoCommit.IsChecked == true;
            AppSettings.AddCommitReferenceToCherryPick = cbxAddReference.IsChecked == true;
        }
    }

    private void OnRevisionChanged()
    {
        commitSummaryUserControl1.Revision = Revision;
        _isMerge = Revision is not null && Module.IsMerge(Revision.ObjectId);
        parentsPanel.IsVisible = _isMerge;
        lvParentsList.ItemsSource = null;

        if (!_isMerge || Revision is null)
        {
            return;
        }

        IReadOnlyList<GitRevision> parents = Module.GetParentRevisions(Revision.ObjectId);
        CherryPickParentRow[] rows = parents
            .Select((parent, index) => new CherryPickParentRow(index + 1, parent))
            .ToArray();
        lvParentsList.Height = 54 + (24 * rows.Length);
        lvParentsList.ItemsSource = rows;
        lvParentsList.SelectedIndex = rows.Length == 0 ? -1 : 0;
    }

    private Control CreateParentRow(CherryPickParentRow? parentRow, INameScope? nameScope)
    {
        Grid row = new() { ColumnDefinitions = new ColumnDefinitions("43,*,120,80") };
        if (parentRow is null)
        {
            return row;
        }

        row.Children.Add(CreateCell(parentRow.Number.ToString(), column: 0));
        row.Children.Add(CreateCell(parentRow.Revision.Subject, column: 1));
        row.Children.Add(CreateCell(parentRow.Revision.Author, column: 2));
        row.Children.Add(CreateCell(parentRow.Revision.CommitDate.ToShortDateString(), column: 3));
        return row;

        static TextBlock CreateCell(string? text, int column)
        {
            TextBlock cell = new()
            {
                Text = text,
                Margin = new Avalonia.Thickness(6, 2),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            Grid.SetColumn(cell, column);
            return cell;
        }
    }

    private void btnAbort_Click(object? sender, EventArgs e)
    {
        DialogResult = WinFormsShims.DialogResult.Cancel;
    }

    private void btnPick_Click(object? sender, EventArgs e)
    {
        if (_isMerge && lvParentsList.SelectedItem is null)
        {
            MessageBoxes.Show(
                this,
                _noneParentSelectedText.Text,
                TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
            return;
        }

        ArgumentString? command = BuildCherryPickCommand();
        if (command is null)
        {
            return;
        }

        // Do not verify success: a failed cherry-pick commonly means conflicts that the
        // existing conflict handler must inspect and offer to resolve.
        FormProcess.ShowDialog(this, UICommands, arguments: command.Value, Module.WorkingDir, input: null, useDialogSettings: true);
        MergeConflictHandler.HandleMergeConflicts(UICommands, this, cbxAutoCommit.IsChecked == true);
        DialogResult = WinFormsShims.DialogResult.OK;
        Close();
    }

    private ArgumentString? BuildCherryPickCommand()
    {
        if (Revision is null)
        {
            return null;
        }

        ArgumentBuilder arguments = [];
        if (_isMerge)
        {
            if (lvParentsList.SelectedItem is not CherryPickParentRow parent)
            {
                return null;
            }

            arguments.Add("-m " + parent.Number);
        }

        if (cbxAddReference.IsChecked == true)
        {
            arguments.Add("-x");
        }

        return Commands.CherryPick(Revision.ObjectId, cbxAutoCommit.IsChecked == true, arguments.ToString());
    }

    public void CopyOptions(FormCherryPick source)
    {
        cbxAutoCommit.IsChecked = source.cbxAutoCommit.IsChecked;
        cbxAddReference.IsChecked = source.cbxAddReference.IsChecked;
    }

    private void btnChooseRevision_Click(object? sender, EventArgs e)
    {
        using (FormChooseCommit chooseForm = new(UICommands, Revision?.Guid))
        {
            if (chooseForm.ShowDialog(this) == WinFormsShims.DialogResult.OK && chooseForm.SelectedRevision is not null)
            {
                Revision = chooseForm.SelectedRevision;
            }
        }

        OnRevisionChanged();
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormCherryPick form)
    {
        public Button Abort => form.btnAbort;
        public CheckBox AddReference => form.cbxAddReference;
        public CheckBox AutoCommit => form.cbxAutoCommit;
        public Button Pick => form.btnPick;
        public ListBox Parents => form.lvParentsList;
        public bool ParentsVisible => form.parentsPanel.IsVisible;

        public ArgumentString? BuildCherryPickCommand() => form.BuildCherryPickCommand();
        public void Load()
        {
            form.LoadSettings();
            form.OnRevisionChanged();
        }

        public void Save() => form.SaveSettings();
    }
}

internal sealed record CherryPickParentRow(int Number, GitRevision Revision);
