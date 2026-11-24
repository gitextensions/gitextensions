using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitUI;
using GitUI.CommitInfo;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

// parity-scaffolding: inspect native fields without extending the original CommitInfo accessor.
internal static class CommitInfoTestSupport
{
    public static Accessor GetNativeTestSupport(this CommitInfo control) => new(control);

    public static XhtmlTextBlock GetRevisionHeader(this CommitInfoHeader header)
        => header.FindControl<XhtmlTextBlock>("rtbRevisionHeader")!;

    public static AvatarControl GetAvatar(this CommitInfoHeader header)
        => header.FindControl<AvatarControl>("avatarControl")!;

    public static void WaitForContentsLayout(Window window)
    {
        DispatcherPump.Wait(async () =>
        {
            Stopwatch watch = Stopwatch.StartNew();
            while (watch.Elapsed < TimeSpan.FromSeconds(5))
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Dispatcher.UIThread.RunJobs();
                CommitInfo[] controls = window.GetVisualDescendants().OfType<CommitInfo>()
                    .Where(control => control.IsEffectivelyVisible && control.GetNativeTestSupport().TableLayout.IsVisible).ToArray();
                if (controls.All(control =>
                    {
                        Accessor accessor = control.GetNativeTestSupport();
                        return accessor.CommitMessageHeight == accessor.CommitMessage.ContentsHeight
                            && accessor.RevisionInfoHeight == accessor.RevisionInfo.ContentsHeight;
                    }) && window.IsMeasureValid && window.IsArrangeValid)
                {
                    return true;
                }

                await Task.Delay(10);
            }

            throw new AssertionException("The real throttled CommitInfo contents and layout did not settle.");
        });
    }

    // parity-scaffolding: render fixtures own an actual UI task context and restore the exact previous task manager.
    internal sealed class UiContextScope : IDisposable
    {
        private static readonly FieldInfo TaskManagerField = typeof(ThreadHelper).GetField("_taskManager", BindingFlags.Static | BindingFlags.NonPublic)!;
        private static readonly FieldInfo CollectionField = typeof(TaskManager).GetField("_joinableTaskCollection", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private readonly object? _previousManager;
        private readonly SynchronizationContext? _previousSynchronizationContext;
        private readonly JoinableTaskContext _context;
        private readonly JoinableTaskCollection _collection;

        public UiContextScope()
        {
            _previousManager = TaskManagerField.GetValue(null);
            _previousSynchronizationContext = SynchronizationContext.Current;
            AvaloniaSynchronizationContext.InstallIfNeeded();
            _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
            ThreadHelper.JoinableTaskContext = _context;
            _collection = (JoinableTaskCollection)CollectionField.GetValue(TaskManagerField.GetValue(null))!;
        }

        public void Dispose()
        {
            try
            {
                Dispatcher.UIThread.RunJobs();
                using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(5));
                DispatcherPump.Wait(async () =>
                {
                    await ThreadHelper.JoinPendingOperationsAsync(deadline.Token);
                    _collection.Should().BeEmpty("all work owned by the disposed render controls must finish before their UI context is released");
                    return true;
                });
            }
            finally
            {
                TaskManagerField.SetValue(null, _previousManager);
                try
                {
                    _context.Dispose();
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(_previousSynchronizationContext);
                }
            }
        }
    }

    internal readonly struct Accessor(CommitInfo control)
    {
        public AvatarControl Avatar => Header.GetAvatar();

        public XhtmlTextBlock CommitMessage => control.GetTestAccessor().CommitMessage;

        public XhtmlTextBlock RevisionInfo => control.GetTestAccessor().RevisionInfo;

        public CommitInfoHeader Header => control.FindControl<CommitInfoHeader>("commitInfoHeader")!;

        public MenuItem AddNoteMenuItem => control.FindControl<MenuItem>("addNoteToolStripMenuItem")!;

        public ContextMenu ContextMenu => Get<ContextMenu>("commitInfoContextMenuStrip");

        public MenuItem CopyLinkMenuItem => control.FindControl<MenuItem>("copyLinkToolStripMenuItem")!;

        public MenuItem ShowLocalBranchesMenuItem => control.FindControl<MenuItem>("showContainedInBranchesToolStripMenuItem")!;

        public MenuItem ShowRemoteBranchesMenuItem => control.FindControl<MenuItem>("showContainedInBranchesRemoteToolStripMenuItem")!;

        public MenuItem ShowRemoteBranchesIfNoLocalMenuItem => control.FindControl<MenuItem>("showContainedInBranchesRemoteIfNoLocalToolStripMenuItem")!;

        public MenuItem ShowTagsMenuItem => control.FindControl<MenuItem>("showContainedInTagsToolStripMenuItem")!;

        public MenuItem ShowAnnotatedTagMessagesMenuItem => control.FindControl<MenuItem>("showMessagesOfAnnotatedTagsToolStripMenuItem")!;

        public MenuItem ShowDerivedTagMenuItem => control.FindControl<MenuItem>("showTagThisCommitDerivesFromMenuItem")!;

        public Grid TableLayout => control.FindControl<Grid>("tableLayout")!;

        public Border CommitMessagePanel => control.FindControl<Border>("pnlCommitMessage")!;

        public int CommitMessageHeight => Get<int>("_commitMessageHeight");

        public int RevisionInfoHeight => Get<int>("_revisionInfoHeight");

        public IDictionary<string, int> GetSortedTags() => control.GetTestAccessor().GetSortedTags();

        private T Get<T>(string name) => (T)typeof(CommitInfo)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(control)!;
    }
}
