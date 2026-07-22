using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI.Compat;
using GitUI.HelperDialogs;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormReflog.cs. The read-only sortable DataGridView is
// represented by the same header-plus-ListBox pattern as the other Avalonia grid twins.
public sealed partial class FormReflog : GitModuleForm
{
    private readonly TranslationString _continueResetCurrentBranchEvenWithChangesText = new("You have changes in your working directory that could be lost.\n\nDo you want to continue?");
    private readonly TranslationString _continueResetCurrentBranchCaptionText = new("Changes not committed...");
    private readonly CancellationTokenSequence _loadSequence = new();
    private readonly TaskManager _loadOperations = ThreadHelper.CreateTaskManager();

    private IReadOnlyList<RefLine> _refLines = [];
    private string? _currentBranch;
    private bool _isBranchCheckedOut;
    private bool _isDirtyDir;
    private string? _sortColumn;
    private bool _sortAscending = true;

    [GeneratedRegex(@"^(?<sha>[^ ]+) (?<ref>[^:]+): (?<action>.+)$", RegexOptions.ExplicitCapture)]
    private static partial Regex ReflogRegex { get; }

    public FormReflog()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormReflog(IGitUICommands uiCommands)
        : base(uiCommands, enablePositionRestore: false)
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private void WireControls()
    {
        gridReflog.ItemTemplate = new FuncDataTemplate<RefLine>(CreateRefLineRow, supportsRecycling: false);
        gridReflog.SelectionChanged += (_, _) => UpdateActionState();
        gridReflog.AddHandler(PointerPressedEvent, gridReflog_PointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        Branches.SelectionChanged += Branches_SelectedIndexChanged;
        linkCurrentBranch.Click += linkCurrentBranch_LinkClicked;
        linkHead.Click += linkHead_Click;
        copySha1ToolStripMenuItem.Click += copySha1ToolStripMenuItem_Click;
        createABranchOnThisCommitToolStripMenuItem.Click += createABranchOnThisCommitToolStripMenuItem_Click;
        resetCurrentBranchOnThisCommitToolStripMenuItem.Click += resetCurrentBranchOnThisCommitToolStripMenuItem_Click;
        Sha.PointerReleased += Header_PointerReleased;
        Ref.PointerReleased += Header_PointerReleased;
        Action.PointerReleased += Header_PointerReleased;
        UpdateActionState();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (!TryGetUICommands(out _))
        {
            return;
        }

        SetRepositoryState(Module.IsDirtyDir(), Module.GetSelectedBranch());

        List<string> branches =
        [
            "HEAD",
            .. Module.GetRefs(RefsFilter.Heads).Select(gitRef => gitRef.Name).OrderBy(name => name),
            .. Module.GetRemoteBranches().Select(gitRef => gitRef.Name).OrderBy(name => name),
        ];
        Branches.ItemsSource = branches;
        Branches.SelectedIndex = branches.Count == 0 ? -1 : 0;
    }

    private void SetRepositoryState(bool isDirtyDir, string currentBranch)
    {
        _isDirtyDir = isDirtyDir;
        _currentBranch = currentBranch;
        _isBranchCheckedOut = _currentBranch != DetachedHeadParser.DetachedBranch;
        linkCurrentBranch.Content = "current branch (" + _currentBranch + ")";
        linkCurrentBranch.IsVisible = _isBranchCheckedOut;
        lblDirtyWorkingDirectory.IsVisible = _isDirtyDir;
        UpdateActionState();
    }

    protected override void OnClosed(EventArgs e)
    {
        _loadSequence.CancelCurrent();
        _loadOperations.JoinPendingOperations();
        _loadSequence.Dispose();
        base.OnClosed(e);
    }

    private Control CreateRefLineRow(RefLine? refLine, INameScope? nameScope)
    {
        Grid row = new() { ColumnDefinitions = new ColumnDefinitions("300,180,*") };
        if (refLine is null)
        {
            return row;
        }

        row.Children.Add(CreateCell(refLine.Sha.ToString(), column: 0, new FontFamily("monospace")));
        row.Children.Add(CreateCell(refLine.Ref, column: 1));
        row.Children.Add(CreateCell(refLine.Action, column: 2));
        return row;

        static TextBlock CreateCell(string text, int column, FontFamily? fontFamily = null)
        {
            TextBlock cell = new()
            {
                Text = text,
                Margin = new Avalonia.Thickness(6, 2),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            if (fontFamily is not null)
            {
                cell.FontFamily = fontFamily;
            }

            Grid.SetColumn(cell, column);
            return cell;
        }
    }

    private void Branches_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (Branches.SelectedItem is not string item || !TryGetUICommands(out _))
        {
            return;
        }

        DisplayRefLog(item);
    }

    private void DisplayRefLog(string item)
    {
        CancellationToken cancellationToken = _loadSequence.Next();
        Cursor = new Cursor(StandardCursorType.Wait);
        Branches.IsEnabled = false;

        _loadOperations.FileAndForget(async () =>
        {
            try
            {
                IReadOnlyList<RefLine> refLines = await Task.Run(() =>
                {
                    GitArgumentBuilder arguments = new("reflog")
                    {
                        "--no-abbrev",
                        item,
                    };
                    string output = Module.GitExecutable.GetOutput(arguments);
                    cancellationToken.ThrowIfCancellationRequested();
                    return ParseReflogOutput(output);
                }, cancellationToken);

                await _loadOperations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                Cursor = null;
                Branches.IsEnabled = true;
                _sortColumn = null;
                _sortAscending = true;
                SetRefLines(refLines, selectedRefLine: null);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        });
    }

    private static IReadOnlyList<RefLine> ParseReflogOutput(string output)
        =>
        [
            .. from line in output.LazySplit('\n')
               where line.Length != 0
               select ReflogRegex.Match(line)
               into match
               where match.Success
               select new RefLine(
                   ObjectId.Parse(match.Groups["sha"].Value),
                   match.Groups["ref"].Value,
                   match.Groups["action"].Value),
        ];

    private void Header_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        string column = ReferenceEquals(sender, Sha)
            ? nameof(RefLine.Sha)
            : ReferenceEquals(sender, Ref)
                ? nameof(RefLine.Ref)
                : nameof(RefLine.Action);
        SortBy(column);
        e.Handled = true;
    }

    private void SortBy(string column)
    {
        _sortAscending = _sortColumn != column || !_sortAscending;
        _sortColumn = column;

        RefLine? selectedRefLine = gridReflog.SelectedItem as RefLine;
        IEnumerable<RefLine> ordered = column switch
        {
            nameof(RefLine.Sha) => OrderBy(_refLines, refLine => refLine.Sha.ToString(), StringComparer.Ordinal),
            nameof(RefLine.Ref) => OrderBy(_refLines, refLine => refLine.Ref, StringComparer.Ordinal),
            _ => OrderBy(_refLines, refLine => refLine.Action, StringComparer.CurrentCulture),
        };
        SetRefLines(ordered.ToArray(), selectedRefLine);
    }

    private IEnumerable<RefLine> OrderBy(
        IEnumerable<RefLine> refLines,
        Func<RefLine, string> selector,
        StringComparer comparer)
        => _sortAscending
            ? refLines.OrderBy(selector, comparer)
            : refLines.OrderByDescending(selector, comparer);

    private void SetRefLines(IReadOnlyList<RefLine> refLines, RefLine? selectedRefLine)
    {
        _refLines = refLines;
        gridReflog.ItemsSource = refLines;
        gridReflog.SelectedItem = selectedRefLine is not null && refLines.Contains(selectedRefLine)
            ? selectedRefLine
            : refLines.FirstOrDefault();
        UpdateActionState();
    }

    private void createABranchOnThisCommitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (GetSelectedRefLine() is RefLine refLine)
        {
            UICommands.StartCreateBranchDialog(this, refLine.Sha);
        }
    }

    private void resetCurrentBranchOnThisCommitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (_isDirtyDir
            && MessageBoxes.Show(
                this,
                _continueResetCurrentBranchEvenWithChangesText.Text,
                _continueResetCurrentBranchCaptionText.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Warning,
                WinFormsShims.MessageBoxDefaultButton.Button2) == WinFormsShims.DialogResult.No)
        {
            return;
        }

        if (GetSelectedRefLine() is not RefLine refLine)
        {
            return;
        }

        GitRevision gitRevision = Module.GetRevision(refLine.Sha);
        FormResetCurrentBranch.ResetType resetType = GetResetType();
        UICommands.DoActionOnRepo(() =>
        {
            using FormResetCurrentBranch form = FormResetCurrentBranch.Create(UICommands, gitRevision, resetType);
            return form.ShowDialog(this) == WinFormsShims.DialogResult.OK;
        });
    }

    private FormResetCurrentBranch.ResetType GetResetType()
        => _isDirtyDir ? FormResetCurrentBranch.ResetType.Soft : FormResetCurrentBranch.ResetType.Hard;

    private void copySha1ToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        if (GetSelectedRefLine() is RefLine refLine)
        {
            ClipboardUtil.TrySetText(refLine.Sha.ToString());
        }
    }

    private void linkCurrentBranch_LinkClicked(object? sender, EventArgs e)
    {
        Branches.SelectedItem = _currentBranch;
    }

    private void linkHead_Click(object? sender, EventArgs e)
    {
        Branches.SelectedIndex = 0;
    }

    private void gridReflog_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PointerPoint point = e.GetCurrentPoint(gridReflog);
        if (!point.Properties.IsRightButtonPressed)
        {
            return;
        }

        ListBoxItem? item = (e.Source as Avalonia.Visual)?.FindAncestorOfType<ListBoxItem>(includeSelf: true);
        if (item?.DataContext is RefLine refLine)
        {
            gridReflog.SelectedItem = refLine;
        }
    }

    private RefLine? GetSelectedRefLine() => gridReflog.SelectedItem as RefLine;

    private void UpdateActionState()
    {
        bool hasSelection = GetSelectedRefLine() is not null;
        copySha1ToolStripMenuItem.IsEnabled = hasSelection;
        createABranchOnThisCommitToolStripMenuItem.IsEnabled = hasSelection;
        resetCurrentBranchOnThisCommitToolStripMenuItem.IsEnabled = hasSelection && _isBranchCheckedOut;
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddHeaderTranslationItem(translation, nameof(Sha), "SHA-1");
        AddHeaderTranslationItem(translation, nameof(Ref), "Ref");
        AddHeaderTranslationItem(translation, nameof(Action), "Action");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, Sha, nameof(Sha), "SHA-1");
        TranslateHeader(translation, Ref, nameof(Ref), "Ref");
        TranslateHeader(translation, Action, nameof(Action), "Action");
    }

    private static void AddHeaderTranslationItem(ITranslation translation, string fieldName, string text)
    {
        translation.AddTranslationItem(nameof(FormReflog), fieldName, "HeaderText", text);
    }

    private static void TranslateHeader(ITranslation translation, Border header, string fieldName, string defaultText)
    {
        string? text = translation.TranslateItem(nameof(FormReflog), fieldName, "HeaderText", () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormReflog form)
    {
        public ComboBox Branches => form.Branches;
        public MenuItem Copy => form.copySha1ToolStripMenuItem;
        public MenuItem CreateBranch => form.createABranchOnThisCommitToolStripMenuItem;
        public ListBox Reflog => form.gridReflog;
        public MenuItem Reset => form.resetCurrentBranchOnThisCommitToolStripMenuItem;
        public IReadOnlyList<RefLine> RefLines => form._refLines;
        public HyperlinkButton CurrentBranch => form.linkCurrentBranch;
        public TextBlock DirtyWorkingDirectory => form.lblDirtyWorkingDirectory;
        public FormResetCurrentBranch.ResetType ResetType => form.GetResetType();

        public static IReadOnlyList<RefLine> Parse(string output) => ParseReflogOutput(output);

        public void SetRefLines(IReadOnlyList<RefLine> refLines, RefLine? selectedRefLine = null)
            => form.SetRefLines(refLines, selectedRefLine);

        public void SetRepositoryState(bool isDirtyDir, string currentBranch)
            => form.SetRepositoryState(isDirtyDir, currentBranch);

        public void SortByRef() => form.SortBy(nameof(RefLine.Ref));
    }
}

internal sealed class RefLine
{
    public ObjectId Sha { get; set; }
    public string Ref { get; set; }
    public string Action { get; set; }

    public RefLine(ObjectId sha, string @ref, string action)
    {
        Sha = sha;
        Ref = @ref;
        Action = action;
    }
}
