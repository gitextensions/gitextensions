using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI;
using GitUI.Compat;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormStash.cs. The file list and viewer retain the original
// stash inspection and selected-patch application boundary.
public sealed partial class FormStash : GitModuleForm
{
    private readonly TranslationString _currentWorkingDirChanges = new("Current working directory changes");
    private readonly TranslationString _noStashes = new("There are no stashes.");

    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private readonly AsyncLoader _asyncLoader = new();
    private int _lastSelectedStashIndex = -1;
    private GitStash? _currentWorkingDirStashItem;

    public FormStash()
    {
        InitializeComponent();
        InitializeComplete();
    }

    public FormStash(IGitUICommands commands, string? initialStash = null)
        : base(commands, true)
    {
        InitializeComponent();

        Stashed.UICommandsSource = this;
        View.UICommandsSource = this;
        Stashed.SelectionMode = SelectionMode.Multiple;
        Stashes.ItemTemplate = new FuncDataTemplate<GitStash>(
            (stash, _) => new TextBlock { Text = stash?.Summary ?? string.Empty },
            supportsRecycling: false);
        Stashed.Bind(() => RefreshAll());
        Stashed.BindContextMenu(View.CherryPickAllChanges, () => View.SupportLinePatching);
        Stashed.SelectedIndexChanged += StashedSelectedIndexChanged;
        View.ExtraDiffArgumentsChanged += StashedSelectedIndexChanged;
        View.TopScrollReached += FileViewer_TopScrollReached;
        View.BottomScrollReached += FileViewer_BottomScrollReached;
        View.EscapePressed += Close;
        Stashes.SelectionChanged += StashesSelectedIndexChanged;
        Stash.Click += StashClick;
        StashSelectedFiles.Click += StashSelectedFiles_Click;
        Clear.Click += ClearClick;
        Apply.Click += ApplyClick;

        if (initialStash is not null)
        {
            string initialIndex = initialStash.SubstringAfter('{').SubstringUntil('}');
            if (int.TryParse(initialIndex, out int result))
            {
                _lastSelectedStashIndex = result + 1;
            }
        }

        HotkeysEnabled = true;
        InitializeComplete();
    }

    public bool ManageStashes { get; set; }

    public static readonly string HotkeySettingsName = "Stash";

    internal enum Command
    {
        NextStash = 0,
        PreviousStash = 1,
        Refresh = 2
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(FormStash), nameof(Stashes), "ToolTipText", "Select a stash");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string? toolTip = translation.TranslateItem(
            nameof(FormStash),
            nameof(Stashes),
            "ToolTipText",
            () => "Select a stash");
        ToolTip.SetTip(Stashes, toolTip);
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        StashKeepIndex.IsChecked = AppSettings.StashKeepIndex;
        chkIncludeUntrackedFiles.IsChecked = AppSettings.IncludeUntrackedFilesInManualStash;
        LoadHotkeys(HotkeySettingsName);
        RefreshAll(force: true);
    }

    protected override void OnClosed(EventArgs e)
    {
        AppSettings.StashKeepIndex = StashKeepIndex.IsChecked == true;
        AppSettings.IncludeUntrackedFilesInManualStash = chkIncludeUntrackedFiles.IsChecked == true;
        _asyncLoader.Dispose();
        _viewChangesSequence.Dispose();

        base.OnClosed(e);
    }

    private void Initialize()
    {
        List<GitStash> stashedItems = [.. Module.GetStashes(noLocks: false)];

        _currentWorkingDirStashItem = new GitStash(-1, _currentWorkingDirChanges.Text);
        stashedItems.Insert(0, _currentWorkingDirStashItem);

        StashMessage.Text = string.Empty;
        Stashes.SelectedItem = null;
        Stashes.ItemsSource = stashedItems;

        if (_lastSelectedStashIndex > 0)
        {
            if (_lastSelectedStashIndex >= stashedItems.Count)
            {
                _lastSelectedStashIndex--;
            }

            Stashes.SelectedIndex = _lastSelectedStashIndex;
            _lastSelectedStashIndex = -1;
        }
        else if (ManageStashes && stashedItems.Count > 1)
        {
            Stashes.SelectedIndex = 1;
            ManageStashes = false;
        }
        else if (stashedItems.Count > 0)
        {
            Stashes.SelectedIndex = 0;
        }
    }

    private void InitializeSoft()
    {
        GitStash? gitStash = Stashes.SelectedItem as GitStash;

        Stashed.ClearDiffs();
        View.ViewPatch(null);
        Loading.IsVisible = true;
        Stashes.IsEnabled = false;
        StashMessage.IsReadOnly = true;
        if (gitStash == _currentWorkingDirStashItem)
        {
            StashMessage.IsReadOnly = false;
            _ = _asyncLoader.LoadAsync(() => Module.GetAllChangedFiles(), LoadGitItemStatuses);
            Clear.IsEnabled = false;
            Apply.IsEnabled = false;
        }
        else if (gitStash is not null)
        {
            _ = _asyncLoader.LoadAsync(() => Module.GetStashDiffFiles(gitStash.Name), LoadGitItemStatuses);
            Clear.IsEnabled = true;
            Apply.IsEnabled = true;
        }
    }

    private void FileViewer_TopScrollReached(object? sender, EventArgs e)
    {
        Stashed.SelectPreviousVisibleItem();
        View.ScrollToBottom();
    }

    private void FileViewer_BottomScrollReached(object? sender, EventArgs e)
    {
        Stashed.SelectNextVisibleItem();
        View.ScrollToTop();
    }

    private bool ChangeSelectedStash(bool next = true)
    {
        int index = Stashes.SelectedIndex + (next ? -1 : 1);

        if (index >= Stashes.ItemCount || index < 0)
        {
            return false;
        }

        Stashes.SelectedIndex = index;
        return true;
    }

    protected override bool ExecuteCommand(int cmd)
    {
        switch ((Command)cmd)
        {
            case Command.NextStash: return ChangeSelectedStash(next: true);
            case Command.PreviousStash: return ChangeSelectedStash(next: false);
            case Command.Refresh: RefreshAll(); return true;
            default: return base.ExecuteCommand(cmd);
        }
    }

    private void LoadGitItemStatuses(IReadOnlyList<GitItemStatus> gitItemStatuses)
    {
        GitStash gitStash = (GitStash)Stashes.SelectedItem!;
        if (gitStash == _currentWorkingDirStashItem)
        {
            ObjectId headId = Module.RevParse("HEAD");
            GitRevision workTreeRev = new(ObjectId.WorkTreeId)
            {
                ParentIds = [ObjectId.IndexId]
            };
            if (headId.IsZero)
            {
                Stashed.SetDiffs(null, workTreeRev, gitItemStatuses);
            }
            else
            {
                GitRevision headRev = new(headId);
                GitRevision indexRev = new(ObjectId.IndexId)
                {
                    ParentIds = [headId]
                };
                List<GitItemStatus> indexItems = [.. gitItemStatuses.Where(item => item.Staged == StagedStatus.Index)];
                List<GitItemStatus> workTreeItems = [.. gitItemStatuses.Where(item => item.Staged != StagedStatus.Index)];
                Stashed.SetStashDiffs(
                    headRev,
                    indexRev,
                    ResourceManager.TranslatedStrings.Index,
                    indexItems,
                    workTreeRev,
                    ResourceManager.TranslatedStrings.Workspace,
                    workTreeItems);
            }
        }
        else
        {
            ObjectId firstId = Module.RevParse(gitStash.Name + "^");
            GitRevision? firstRev = firstId.IsZero ? null : new GitRevision(firstId);

            ObjectId selectedId = Module.RevParse(gitStash.Name);
            if (selectedId.IsZero)
            {
                throw new InvalidOperationException("selectedId must not be zero");
            }

            GitRevision secondRev = new(selectedId);
            if (!firstId.IsZero)
            {
                secondRev.ParentIds = [firstId];
            }

            Stashed.SetDiffs(firstRev, secondRev, gitItemStatuses);
        }

        Loading.IsVisible = false;
        Stashes.IsEnabled = true;
    }

    private void StashedSelectedIndexChanged(object? sender, EventArgs e)
    {
        CancellationToken cancellationToken = _viewChangesSequence.Next();
        this.InvokeAndForget(
            () => View.ViewChangesAsync(Stashed.SelectedFileStatusItem, cancellationToken),
            cancellationToken: cancellationToken);
        EnablePartialStash();
    }

    private void StashClick(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            string message = !string.IsNullOrWhiteSpace(StashMessage.Text)
                ? " " + StashMessage.Text.Trim()
                : string.Empty;
            UICommands.StashSave(
                this,
                chkIncludeUntrackedFiles.IsChecked == true,
                StashKeepIndex.IsChecked == true,
                message);
            Initialize();
        }
    }

    private void StashSelectedFiles_Click(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            string message = !string.IsNullOrWhiteSpace(StashMessage.Text)
                ? " " + StashMessage.Text.Trim()
                : string.Empty;
            UICommands.StashSave(
                this,
                chkIncludeUntrackedFiles.IsChecked == true,
                StashKeepIndex.IsChecked == true,
                message,
                Stashed.SelectedItems.Select(item => item.Item.Name).ToList());
            Initialize();
        }
    }

    private void ClearClick(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            string stashName = GetStashName();
            if (!AppSettings.DontConfirmStashDrop)
            {
                TaskDialogPage page = new()
                {
                    Text = TranslatedStrings.AreYouSure,
                    Caption = TranslatedStrings.StashDropConfirmTitle,
                    Heading = TranslatedStrings.CannotBeUndone,
                    Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                    Icon = TaskDialogIcon.Information,
                    Verification = new TaskDialogVerificationCheckBox
                    {
                        Text = TranslatedStrings.DontShowAgain
                    },
                    SizeToContent = true
                };

                TaskDialogButton result = TaskDialog.ShowDialog(this, page);

                if (result == TaskDialogButton.Yes)
                {
                    _lastSelectedStashIndex = Stashes.SelectedIndex;
                    UICommands.StashDrop(this, stashName);
                    Initialize();
                }

                if (page.Verification.Checked)
                {
                    AppSettings.DontConfirmStashDrop = true;
                }
            }
            else
            {
                _lastSelectedStashIndex = Stashes.SelectedIndex;
                UICommands.StashDrop(this, stashName);
                Initialize();
            }
        }
    }

    private string GetStashName() => ((GitStash)Stashes.SelectedItem!).Name;

    private void ApplyClick(object? sender, EventArgs e)
    {
        UICommands.StashApply(this, GetStashName());
        Initialize();
    }

    private void StashesSelectedIndexChanged(object? sender, EventArgs e)
    {
        EnablePartialStash();

        using (WaitCursorScope.Enter())
        {
            InitializeSoft();

            if (Stashes.SelectedItem is GitStash gitStash)
            {
                StashMessage.Text = gitStash != _currentWorkingDirStashItem
                    ? gitStash.Message
                    : string.Empty;
            }

            if (Stashes.ItemCount == 1)
            {
                StashMessage.Text = _noStashes.Text;
            }
        }
    }

    private void EnablePartialStash()
    {
        StashSelectedFiles.IsEnabled = Stashes.SelectedIndex == 0 && Stashed.SelectedItems.Any();
    }

    private void RefreshAll(bool force = false)
    {
        if (!force && Stashes.SelectedIndex != 0)
        {
            return;
        }

        using (WaitCursorScope.Enter())
        {
            Initialize();
        }
    }
}
