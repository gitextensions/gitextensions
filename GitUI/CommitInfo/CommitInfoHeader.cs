using System.Reactive.Linq;
using GitCommands;
using GitExtUtils;
using GitUI.Editor.RichTextBoxExtension;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using ResourceManager;
using ResourceManager.CommitDataRenders;

namespace GitUI.CommitInfo
{
    public partial class CommitInfoHeader : GitModuleControl
    {
        private readonly IDateFormatter _dateFormatter = new DateFormatter();
        private readonly ILinkFactory _linkFactory = new LinkFactory();
        private readonly ICommitDataManager _commitDataManager;
        private readonly ICommitDataHeaderRenderer _commitDataHeaderRenderer;
        private readonly IDisposable _rtbResizedSubscription;

        public event EventHandler<CommandEventArgs>? CommandClicked;

        public CommitInfoHeader()
        {
            InitializeComponent();
            InitializeComplete();

            TabbedHeaderLabelFormatter labelFormatter = new();
            TabbedHeaderRenderStyleProvider headerRenderer = new();

            _commitDataManager = new CommitDataManager(() => Module);
            _commitDataHeaderRenderer = new CommitDataHeaderRenderer(labelFormatter, _dateFormatter, headerRenderer, _linkFactory);

            using (var g = CreateGraphics())
            {
                rtbRevisionHeader.Font = _commitDataHeaderRenderer.GetFont(g);
            }

            rtbRevisionHeader.SelectionTabs = _commitDataHeaderRenderer.GetTabStops().ToArray();

            _rtbResizedSubscription = Observable
                .FromEventPattern<ContentsResizedEventHandler, ContentsResizedEventArgs>(
                    h => rtbRevisionHeader.ContentsResized += h,
                    h => rtbRevisionHeader.ContentsResized -= h)
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(MainThreadScheduler.Instance)
                .Subscribe(_ => rtbRevisionHeader_ContentsResized(_.EventArgs));
        }

        public void SetContextMenuStrip(ContextMenuStrip contextMenuStrip)
        {
            rtbRevisionHeader.ContextMenuStrip = contextMenuStrip;
        }

        public void ShowCommitInfo(GitRevision revision, IReadOnlyList<ObjectId>? children)
        {
            this.InvokeAsync(() =>
            {
                var data = _commitDataManager.CreateFromRevision(revision, children);
                var header = _commitDataHeaderRenderer.Render(data, showRevisionsAsLinks: CommandClicked is not null);

                rtbRevisionHeader.SuspendLayout();

                rtbRevisionHeader.Clear();
                rtbRevisionHeader.SetXHTMLText(header);

                rtbRevisionHeader.SelectionStart = 0; // scroll up
                rtbRevisionHeader.ScrollToCaret();    // scroll up

                rtbRevisionHeader.ResumeLayout(true);

                LoadAuthorImage(revision);
            }).FileAndForget();
        }

        public string GetPlainText()
        {
            return _commitDataHeaderRenderer.GetPlainText(rtbRevisionHeader.GetPlainText());
        }

        private void LoadAuthorImage(GitRevision? revision)
        {
            var showAvatar = AppSettings.ShowAuthorAvatarInCommitInfo;
            avatarControl.Visible = showAvatar;

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

        private void rtbRevisionHeader_ContentsResized(ContentsResizedEventArgs e)
        {
            rtbRevisionHeader.ClientSize = e.NewRectangle.Size;
        }

        private void rtbRevisionHeader_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Control || e.KeyCode != Keys.C || sender is not RichTextBox rtb)
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
                string? linkUri = rtbRevisionHeader.GetLink(e.LinkStart);
                _linkFactory.ExecuteLink(linkUri, commandEventArgs => CommandClicked?.Invoke(sender, commandEventArgs));
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void rtbRevisionHeader_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
            {
                DoCommandClick("navigatebackward");
            }
            else if (e.Button == MouseButtons.XButton2)
            {
                DoCommandClick("navigateforward");
            }

            void DoCommandClick(string command)
            {
                CommandClicked?.Invoke(this, new CommandEventArgs(command, null));
            }
        }

        protected override void DisposeCustomResources()
        {
            try
            {
                _rtbResizedSubscription?.Dispose();

                base.DisposeCustomResources();
            }
            catch (InvalidOperationException)
            {
                // System.Reactive causes the app to fail with: 'Invoke or BeginInvoke cannot be called on a control until the window handle has been created.'
            }
        }
    }
}
