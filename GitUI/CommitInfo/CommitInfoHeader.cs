using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;
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

        public event EventHandler<CommandEventArgs> CommandClicked;

        public CommitInfoHeader()
        {
            InitializeComponent();
            InitializeComplete();

            var labelFormatter = new TabbedHeaderLabelFormatter();
            var headerRenderer = new TabbedHeaderRenderStyleProvider();

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

        public void ShowCommitInfo(GitRevision revision, IReadOnlyList<ObjectId> children)
        {
            this.InvokeAsync(() =>
            {
                var data = _commitDataManager.CreateFromRevision(revision, children);
                var header = _commitDataHeaderRenderer.Render(data, showRevisionsAsLinks: CommandClicked != null);

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
            return rtbRevisionHeader.GetPlainText();
        }

        private void LoadAuthorImage(GitRevision revision)
        {
            var showAvatar = AppSettings.ShowAuthorAvatarInCommitInfo;
            avatarControl.Visible = showAvatar;

            if (!showAvatar)
            {
                return;
            }

            if (revision == null)
            {
                avatarControl.LoadImage(null);
                return;
            }

            avatarControl.LoadImage(revision.AuthorEmail ?? revision.CommitterEmail);
        }

        private void rtbRevisionHeader_ContentsResized(ContentsResizedEventArgs e)
        {
            rtbRevisionHeader.ClientSize = e.NewRectangle.Size;
        }

        private void rtbRevisionHeader_KeyDown(object sender, KeyEventArgs e)
        {
            var rtb = sender as RichTextBox;
            if (rtb == null || !e.Control || e.KeyCode != Keys.C)
            {
                return;
            }

            // Override RichTextBox Ctrl-c handling to copy plain text
            ClipboardUtil.TrySetText(rtb.GetSelectionPlainText());
            e.Handled = true;
        }

        private void rtbRevisionHeader_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            var link = _linkFactory.ParseLink(e.LinkText);

            try
            {
                var result = new Uri(link);
                if (result.Scheme == "gitext")
                {
                    CommandClicked?.Invoke(sender, new CommandEventArgs(result.Host, result.AbsolutePath.TrimStart('/')));
                }
                else
                {
                    using (var process = new Process
                    {
                        EnableRaisingEvents = false,
                        StartInfo = { FileName = result.AbsoluteUri }
                    })
                    {
                        process.Start();
                    }
                }
            }
            catch (UriFormatException)
            {
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
            _rtbResizedSubscription.Dispose();
            base.DisposeCustomResources();
        }
    }
}
