using Avalonia.Controls;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitUI.Compat;
using GitUIPluginInterfaces;

namespace GitUI.UserControls;

public partial class CommitDiff : GitModuleControl
{
    private readonly CancellationTokenSequence _viewChangesSequence = new();
    private string _text = string.Empty;

    /// <summary>
    /// Raised when the Escape key is pressed (and only when no selection exists, as the default behaviour of escape is to clear the selection).
    /// </summary>
    public event Action? EscapePressed;

    /// <summary>
    /// Raised when the descriptive dialog text changes.
    /// </summary>
    public event EventHandler? TextChanged;

    public CommitDiff()
    {
        InitializeComponent();
        InitializeComplete();

        DiffText.EscapePressed += () => EscapePressed?.Invoke();
        DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
        DiffText.TopScrollReached += FileViewer_TopScrollReached;
        DiffText.BottomScrollReached += FileViewer_BottomScrollReached;
        DiffFiles.SelectedIndexChanged += DiffFiles_SelectedIndexChanged;
        DiffFiles.Focus();
        DiffFiles.ClearDiffs();
        DetachedFromLogicalTree += (_, _) => _viewChangesSequence.CancelCurrent();
    }

    /// <summary>
    /// Gets the descriptive dialog text (named like the WinForms property).
    /// </summary>
    public string Text
    {
        get => _text;
        private set
        {
            if (_text == value)
            {
                return;
            }

            _text = value;
            TextChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    internal FileStatusList FileStatusList => DiffFiles;

    internal Editor.FileViewer FileViewer => DiffText;

    internal CommitInfo.CommitInfo CommitInformation => commitInfo;

    private void FileViewer_TopScrollReached(object? sender, EventArgs e)
    {
        DiffFiles.SelectPreviousVisibleItem();
        DiffText.ScrollToBottom();
    }

    private void FileViewer_BottomScrollReached(object? sender, EventArgs e)
    {
        DiffFiles.SelectNextVisibleItem();
        DiffText.ScrollToTop();
    }

    public void SetRevision(ObjectId objectId, string? fileToSelect)
    {
        // We cannot use the GitRevision from revision grid. When a filtered commit list
        // is shown (file history/normal filter) the parent guids are not the 'real' parents,
        // but the parents in the filtered list.
        GitRevision? revision = Module.GetRevision(objectId);

        if (revision is not null)
        {
            DiffFiles.SetDiffs([revision]);
            if (fileToSelect is not null)
            {
                FileStatusItem? itemToSelect = DiffFiles.AllItems.FirstOrDefault(i => i.Item.Name == fileToSelect);
                if (itemToSelect is not null)
                {
                    DiffFiles.SelectedGitItem = itemToSelect.Item;
                }
            }

            commitInfo.Revision = revision;

            Text = "Diff - " + revision.ObjectId.ToShortString() + " - " + revision.AuthorDate + " - " + revision.Author + " - " + Module.WorkingDir;
        }
    }

    private void DiffFiles_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ViewSelectedDiff();
    }

    private void DiffText_ExtraDiffArgumentsChanged(object? sender, EventArgs e)
    {
        ViewSelectedDiff();
    }

    private void ViewSelectedDiff()
    {
        DiffText.InvokeAndForget(() => DiffText.ViewChangesAsync(DiffFiles.SelectedFileStatusItem, cancellationToken: _viewChangesSequence.Next()));
    }
}
