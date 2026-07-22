using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;

namespace GitUI.CommandsDialogs.WorktreeDialog;

// Twin of GitUI/CommandsDialogs/WorktreeDialog/FormManageWorktree.cs. The read-only
// DataGridView becomes a typed ListBox with the original column translation identities.
public partial class FormManageWorktree : GitExtensionsDialog
{
    private IReadOnlyList<GitWorktree>? _worktrees;

    public bool ShouldRefreshRevisionGrid { get; private set; }

    public FormManageWorktree()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormManageWorktree(IGitUICommands commands)
        : base(commands, enablePositionRestore: false)
    {
        InitializeComponent();
        WireControls();
        AcceptButton = buttonOpenSelectedWorktree;
        InitializeComplete();
    }

    /// <summary>
    /// If this is not null before showing the dialog the given
    /// remote name will be preselected in the listbox.
    /// </summary>
    public string? PreselectRemoteOnLoad { get; set; }

    private void WireControls()
    {
        Worktrees.ItemTemplate = new FuncDataTemplate<GitWorktree>(CreateWorktreeRow, supportsRecycling: false);
        Worktrees.SelectionChanged += Worktrees_SelectionChanged;
        Worktrees.DoubleTapped += WorktreesOnDoubleTapped;
        Worktrees.KeyDown += Worktrees_KeyDown;
        buttonPruneWorktrees.Click += buttonPruneWorktrees_Click;
        buttonDeleteSelectedWorktree.Click += buttonDeleteSelectedWorktree_Click;
        buttonOpenSelectedWorktree.Click += buttonOpenSelectedWorktree_Click;
        buttonCreateNewWorktree.Click += buttonCreateNewWorktree_Click;
        UpdateActionState();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (TryGetUICommands(out _))
        {
            Initialize();
        }
    }

    private Control CreateWorktreeRow(GitWorktree? worktree, INameScope? nameScope)
    {
        Grid row = new() { ColumnDefinitions = new ColumnDefinitions("2*,100,140,*") };
        if (worktree is null)
        {
            return row;
        }

        TextDecorationCollection? decorations = worktree.IsDeleted ? TextDecorations.Strikethrough : null;
        row.Children.Add(CreateCell(worktree.Path, column: 0, decorations));
        row.Children.Add(CreateCell(worktree.HeadType.ToString(), column: 1, decorations));
        row.Children.Add(CreateCell(worktree.Branch ?? string.Empty, column: 2, decorations));
        row.Children.Add(CreateCell(worktree.Sha1 ?? string.Empty, column: 3, decorations, wrap: true, fontFamily: new FontFamily("monospace")));
        return row;

        static TextBlock CreateCell(
            string text,
            int column,
            TextDecorationCollection? textDecorations,
            bool wrap = false,
            FontFamily? fontFamily = null)
        {
            TextBlock cell = new()
            {
                Text = text,
                Margin = new Avalonia.Thickness(6, 2),
                TextDecorations = textDecorations,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
            };
            if (fontFamily is not null)
            {
                cell.FontFamily = fontFamily;
            }

            Grid.SetColumn(cell, column);
            return cell;
        }
    }

    private void Initialize()
    {
        string? selectedPath = (Worktrees.SelectedItem as GitWorktree)?.Path;
        ApplyWorktrees(Module.GetWorktrees(), selectedPath);
    }

    private void ApplyWorktrees(IReadOnlyList<GitWorktree> worktrees, string? selectedPath = null)
    {
        _worktrees = worktrees;
        Worktrees.ItemsSource = worktrees;
        Worktrees.SelectedItem = worktrees.FirstOrDefault(worktree => PathsEqual(worktree.Path, selectedPath))
            ?? worktrees.FirstOrDefault();
        buttonPruneWorktrees.IsEnabled = worktrees.Skip(1).Any(worktree => worktree.IsDeleted);
        UpdateActionState();
    }

    private void buttonPruneWorktrees_Click(object? sender, EventArgs e) => PruneWorktrees();

    private void PruneWorktrees()
    {
        UICommands.StartCommandLineProcessDialog(this, command: null, "worktree prune");
        Initialize();
    }

    private void buttonDeleteSelectedWorktree_Click(object? sender, EventArgs e)
    {
        if (!CanActOnSelectedWorkspace(out GitWorktree? workTree))
        {
            return;
        }

        if (UICommands.WorktreeDelete(this, workTree.Path))
        {
            Initialize();
        }
    }

    private void buttonOpenSelectedWorktree_Click(object? sender, EventArgs e)
    {
        OpenSelectedWorktree();
    }

    private void WorktreesOnDoubleTapped(object? sender, TappedEventArgs e)
    {
        OpenSelectedWorktree();
    }

    private void Worktrees_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            OpenSelectedWorktree();
        }
    }

    private void OpenSelectedWorktree()
    {
        if (!CanActOnSelectedWorkspace(out GitWorktree? workTree))
        {
            return;
        }

        if (UICommands.WorktreeSwitch(this, workTree.Path))
        {
            Close();
        }
    }

    private void Worktrees_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateActionState();
    }

    private void UpdateActionState()
    {
        buttonDeleteSelectedWorktree.IsEnabled = CanDeleteSelectedWorkspace();
        buttonOpenSelectedWorktree.IsEnabled = CanActOnSelectedWorkspace(out _);
    }

    private bool CanDeleteSelectedWorkspace()
        => CanActOnSelectedWorkspace(out _) && Worktrees.SelectedIndex != 0;

    private bool CanActOnSelectedWorkspace([NotNullWhen(true)] out GitWorktree? workTree)
    {
        workTree = null;

        if (_worktrees is null or { Count: <= 1 } || Worktrees.SelectedItem is not GitWorktree selectedWorktree)
        {
            return false;
        }

        workTree = selectedWorktree;
        return !workTree.IsDeleted && !IsCurrentlyOpenedWorktree(workTree);
    }

    private bool IsCurrentlyOpenedWorktree(GitWorktree workTree)
        => TryGetUICommands(out IGitUICommands? commands)
            && PathsEqual(commands.Module.WorkingDir, workTree.Path);

    private static bool PathsEqual(string? first, string? second)
    {
        if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
        {
            return false;
        }

        string firstPath = System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(first));
        string secondPath = System.IO.Path.TrimEndingDirectorySeparator(System.IO.Path.GetFullPath(second));
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return string.Equals(firstPath, secondPath, comparison);
    }

    private void buttonCreateNewWorktree_Click(object? sender, EventArgs e)
    {
        string basePath = _worktrees is { Count: > 0 }
            ? _worktrees[0].Path
            : UICommands.Module.WorkingDir;

        if (UICommands.WorktreeCreate(this, basePath))
        {
            ShouldRefreshRevisionGrid = true;
            Initialize();
        }
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(FormManageWorktree), nameof(Path), "HeaderText", "Path");
        translation.AddTranslationItem(nameof(FormManageWorktree), nameof(Type), "HeaderText", "Type");
        translation.AddTranslationItem(nameof(FormManageWorktree), nameof(Branch), "HeaderText", "Branch");
        translation.AddTranslationItem(nameof(FormManageWorktree), nameof(Sha1), "HeaderText", "SHA-1");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, Path, nameof(Path), "Path");
        TranslateHeader(translation, Type, nameof(Type), "Type");
        TranslateHeader(translation, Branch, nameof(Branch), "Branch");
        TranslateHeader(translation, Sha1, nameof(Sha1), "SHA-1");
    }

    private static void TranslateHeader(ITranslation translation, Border header, string fieldName, string defaultText)
    {
        string? text = translation.TranslateItem(nameof(FormManageWorktree), fieldName, "HeaderText", () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormManageWorktree form)
    {
        public Button Create => form.buttonCreateNewWorktree;
        public Button Delete => form.buttonDeleteSelectedWorktree;
        public Button Open => form.buttonOpenSelectedWorktree;
        public Button Prune => form.buttonPruneWorktrees;
        public ListBox Worktrees => form.Worktrees;

        public void SetWorktrees(IReadOnlyList<GitWorktree> worktrees, string? selectedPath = null)
            => form.ApplyWorktrees(worktrees, selectedPath);

        public static bool PathsEqual(string first, string second) => FormManageWorktree.PathsEqual(first, second);
    }
}
