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
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormRevertCommit.cs. The parent ListView is represented by
// translated headers and one typed ListBox; command, message, and conflict behavior stay at
// their original code-behind boundaries.
public partial class FormRevertCommit : GitExtensionsDialog
{
    private readonly TranslationString _noneParentSelectedText = new("None parent is selected!");

    private bool _isMerge;

    public GitRevision Revision { get; } = null!;

    public FormRevertCommit()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormRevertCommit(IGitUICommands commands, GitRevision revision)
        : base(commands, enablePositionRestore: false)
    {
        Revision = revision;

        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private void WireControls()
    {
        lvParentsList.ItemTemplate = new FuncDataTemplate<RevertParentRow>(CreateParentRow, supportsRecycling: false);
        Revert.Click += Revert_Click;
        btnAbort.Click += btnAbort_Click;
        AcceptButton = Revert;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (!TryGetUICommands(out _))
        {
            return;
        }

        LoadRevisionInfo();
        if (lvParentsList.IsVisible)
        {
            lvParentsList.Focus();
        }
        else
        {
            AutoCommit.Focus();
        }
    }

    private void LoadRevisionInfo()
    {
        commitSummaryUserControl1.Revision = Revision;
        _isMerge = Module.IsMerge(Revision.ObjectId);
        parentsPanel.IsVisible = _isMerge;
        lvParentsList.ItemsSource = null;

        if (!_isMerge)
        {
            return;
        }

        IReadOnlyList<GitRevision> parents = Module.GetParentRevisions(Revision.ObjectId);
        RevertParentRow[] rows = parents
            .Select((parent, index) => new RevertParentRow(index + 1, parent))
            .ToArray();
        lvParentsList.Height = 54 + (24 * rows.Length);
        lvParentsList.ItemsSource = rows;
        lvParentsList.SelectedIndex = rows.Length == 0 ? -1 : 0;
    }

    private Control CreateParentRow(RevertParentRow? parentRow, INameScope? nameScope)
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

    private void Revert_Click(object? sender, EventArgs e)
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

        ArgumentString? command = BuildRevertCommand();
        if (command is null)
        {
            return;
        }

        using WinFormsShims.Control commitMessageManagerOwner = new();
        CommitMessageManager commitMessageManager = new(
            commitMessageManagerOwner,
            Module.WorkingDirGitDir,
            Module.CommitEncoding);
        string existingCommitMessage = ThreadHelper.JoinableTaskFactory.Run(
            () => commitMessageManager.GetMergeOrCommitMessageAsync());

        // Do not verify success: a failed revert commonly means conflicts that the existing
        // conflict handler must inspect and offer to resolve.
        FormProcess.ShowDialog(this, UICommands, arguments: command.Value, Module.WorkingDir, input: null, useDialogSettings: true);

        if (!string.IsNullOrWhiteSpace(existingCommitMessage))
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                try
                {
                    await TaskScheduler.Default;

                    string message = await commitMessageManager.GetMergeOrCommitMessageAsync();
                    string newCommitMessageContent = $"{existingCommitMessage}\n\n{message}";
                    await commitMessageManager.WriteCommitMessageToFileAsync(
                        newCommitMessageContent,
                        CommitMessageType.Merge,
                        usingCommitTemplate: false,
                        ensureCommitMessageSecondLineEmpty: false);
                }
                catch (Exception)
                {
                }
            });
        }

        MergeConflictHandler.HandleMergeConflicts(UICommands, this, AutoCommit.IsChecked == true);
        DialogResult = WinFormsShims.DialogResult.OK;
        Close();
    }

    private ArgumentString? BuildRevertCommand()
    {
        if (Revision is null)
        {
            return null;
        }

        int parentIndex = 0;
        if (_isMerge)
        {
            if (lvParentsList.SelectedItem is not RevertParentRow parent)
            {
                return null;
            }

            parentIndex = parent.Number;
        }

        return Commands.Revert(Revision.ObjectId, AutoCommit.IsChecked == true, parentIndex);
    }

    private void btnAbort_Click(object? sender, EventArgs e)
    {
        DialogResult = WinFormsShims.DialogResult.Cancel;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormRevertCommit form)
    {
        public Button Abort => form.btnAbort;
        public CheckBox AutoCommit => form.AutoCommit;
        public ListBox Parents => form.lvParentsList;
        public bool ParentsVisible => form.parentsPanel.IsVisible;
        public Button Revert => form.Revert;

        public ArgumentString? BuildRevertCommand() => form.BuildRevertCommand();
        public void Load() => form.LoadRevisionInfo();
    }
}

internal sealed record RevertParentRow(int Number, GitRevision Revision);
