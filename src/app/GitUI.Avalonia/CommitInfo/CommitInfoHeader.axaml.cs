using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.Compat;
using GitUIPluginInterfaces;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace GitUI.CommitInfo;

public partial class CommitInfoHeader : GitModuleControl
{
    private readonly IDateFormatter _dateFormatter = new DateFormatter();
    private readonly ILinkFactory _linkFactory = new LinkFactory();
    private readonly ICommitDataManager _commitDataManager;
    private readonly ICommitDataHeaderRenderer _commitDataHeaderRenderer;

    public event EventHandler<CommandEventArgs>? CommandClicked;

    internal string? SelectedLinkUri => rtbRevisionHeader.SelectedLinkUri;

    public CommitInfoHeader()
    {
        InitializeComponent();
        InitializeComplete();

        TabbedHeaderLabelFormatter labelFormatter = new();
        TabbedHeaderRenderStyleProvider headerRenderer = new();

        _commitDataManager = new CommitDataManager(() => Module);
        _commitDataHeaderRenderer = new CommitDataHeaderRenderer(labelFormatter, _dateFormatter, headerRenderer, _linkFactory);
    }

    // Avalonia constraint: ContextMenu is the native counterpart of ContextMenuStrip.
    public void SetContextMenuStrip(ContextMenu contextMenuStrip)
    {
        rtbRevisionHeader.ContextMenu = contextMenuStrip;
    }

    public void ShowCommitInfo(GitRevision revision, IReadOnlyList<ObjectId>? children)
    {
        this.InvokeAndForget(() =>
        {
            CommitData data = _commitDataManager.CreateFromRevision(revision, children);
            string header = _commitDataHeaderRenderer.Render(data, showRevisionsAsLinks: CommandClicked is not null);

            rtbRevisionHeader.Clear();
            rtbRevisionHeader.SetXHTMLText(header);
            rtbRevisionHeader.SelectionStart = 0; // scroll up
            rtbRevisionHeader.SelectionEnd = 0;   // scroll up

            LoadAuthorImage(revision);
        });
    }

    public string GetPlainText()
    {
        return _commitDataHeaderRenderer.GetPlainText(rtbRevisionHeader.GetPlainText());
    }

    private void LoadAuthorImage(GitRevision? revision)
    {
        bool showAvatar = AppSettings.ShowAuthorAvatarInCommitInfo;
        avatarControl.IsVisible = showAvatar;

        if (!showAvatar)
        {
            return;
        }

        if (revision is null)
        {
            avatarControl.LoadImage(null, null);
            return;
        }

        avatarControl.LoadImage(revision.AuthorEmail ?? revision.CommitterEmail, revision.Author ?? revision.Committer);
    }

    private void rtbRevisionHeader_KeyDown(object sender, KeyEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.Key != Key.C || sender is not XhtmlTextBlock rtb)
        {
            return;
        }

        // Override RichTextBox Ctrl-c handling to copy plain text
        ClipboardUtil.TrySetText(rtb.GetSelectionPlainText());
        e.Handled = true;
    }

    private void rtbRevisionHeader_LinkClicked(object sender, LinkClickedEventArgs e)
    {
        try
        {
            _linkFactory.ExecuteLink(e.LinkUri, commandEventArgs => CommandClicked?.Invoke(sender, commandEventArgs));
        }
        catch (Exception ex)
        {
            MessageBoxes.Show(this, ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Avalonia constraint: pointer button state is carried by PointerPressedEventArgs.
    private void rtbRevisionHeader_MouseDown(object sender, PointerPressedEventArgs e)
    {
        PointerPointProperties properties = e.GetCurrentPoint(rtbRevisionHeader).Properties;
        if (properties.IsXButton1Pressed)
        {
            DoCommandClick("navigatebackward");
        }
        else if (properties.IsXButton2Pressed)
        {
            DoCommandClick("navigateforward");
        }

        void DoCommandClick(string command)
        {
            CommandClicked?.Invoke(this, new CommandEventArgs(command, null));
        }
    }

    // WinForms catches a System.Reactive handle-creation race during resource disposal.
    // Avalonia has no ContentsResized subscription, so no custom resource is created here.

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(CommitInfoHeader header)
    {
        public AvatarControl Avatar => header.avatarControl;

        public XhtmlTextBlock RevisionHeader => header.rtbRevisionHeader;
    }
}
