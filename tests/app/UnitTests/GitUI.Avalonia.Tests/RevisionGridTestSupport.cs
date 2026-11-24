using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitExtensionsTests;

// parity-scaffolding: Native-only fixture access stays in tests rather than extending the original product accessor.
internal static class RevisionGridTestSupport
{
    internal static Accessor GetNativeTestAccessor(this RevisionGridControl control) => new(control);

    internal static void ConfigureCancelledRevisionRead(this IGitModule module)
    {
        module.GitCommandRunner.RunDetached(Arg.Any<CancellationToken>(), Arg.Any<ArgumentString>(),
                Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<Encoding?>(), Arg.Any<bool>())
            .Returns<IProcess>(call =>
            {
                if (!call.ArgAt<ArgumentString>(1).ToString().StartsWith("stash list", StringComparison.Ordinal))
                {
                    throw new OperationCanceledException();
                }

                StreamReader output = new(new MemoryStream());
                IProcess process = Substitute.For<IProcess>();
                process.StandardOutput.Returns(output);
                process.When(value => value.Dispose()).Do(_ => output.Dispose());
                return process;
            });
    }

    internal static void OpenSelectedRevisionContextMenu(this RevisionGridControl control, Window owner, int opening = 0)
    {
        Dispatcher.UIThread.RunJobs();
        owner.UpdateLayout();
        ListBox revisions = control._gridView;
        ListBoxItem row = revisions.ContainerFromIndex(revisions.SelectedIndex) as ListBoxItem
            ?? throw new AssertionException("The selected revision must be realized.");
        Size doubleTapSize = owner.GetPlatformSettings()?.GetDoubleTapSize(PointerType.Mouse)
            ?? throw new InvalidOperationException("The platform must define pointer gesture bounds.");
        Point rowPoint = new(row.Bounds.Width - 5 - (opening * (doubleTapSize.Width + 1)), row.Bounds.Height / 2);
        Point point = row.TranslatePoint(rowPoint, owner)
            ?? throw new InvalidOperationException("The selected revision must be attached to the window.");
        owner.MouseMove(point, RawInputModifiers.None);
        owner.MouseDown(point, MouseButton.Right, RawInputModifiers.None);
        owner.MouseUp(point, MouseButton.Right, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        (revisions.ContextMenu?.IsOpen ?? false).Should().BeTrue("the real right-button route must open the revision menu");
    }

    internal readonly struct Accessor(RevisionGridControl control)
    {
        public Control? CurrentPage => control.revisionPage.Content as Control;

        public Window? OwnerWindow => GetField<Window?>("_ownerWindow");

        public ListBox Revisions => control._gridView;

        public ObjectId PendingSelectedObjectId => PendingSelectedObjectIds.FirstOrDefault();

        public IReadOnlyList<ObjectId> PendingSelectedObjectIds => GetField<IReadOnlyList<ObjectId>>("_pendingSelectedObjectIds");

        public void RaiseOwnerWindowDeactivated() => Invoke("OnOwnerWindowDeactivated", OwnerWindow, EventArgs.Empty);

        public void SetRevisions(IEnumerable<GitRevision> revisions)
        {
            Invoke("ApplyFinalRevisionOrder", (object)revisions.ToArray());
            Invoke("SetPage", control._gridView);
            Invoke("FocusRevisionGridWhenShown");
        }

        public void AppendRevisions(IEnumerable<GitRevision> revisions)
            => Invoke("AppendRevisions", revisions.ToArray(), default(CancellationToken));

        public void ReadRevisions(IEnumerable<GitRevision> revisions, CancellationToken cancellationToken = default)
        {
            GitRevision[] batch = [.. revisions];
            Invoke("AddToGraph", batch, cancellationToken);
            Invoke("AppendRevisions", batch, cancellationToken);
        }

        public void JoinBackgroundOperations() => GetField<TaskManager>("_taskManager").JoinPendingOperations();

        public void CompleteLoading(CancellationToken cancellationToken = default)
            => Invoke("OnLoadingCompleted", cancellationToken, default(ObjectId), null);

        public bool HasGraphParent(ObjectId childId, ObjectId parentId)
            => GetField<RevisionGraph>("_revisionGraph").TryGetNode(childId, out RevisionGraphRevision? child)
                && child.Parents.Any(parent => parent.Objectid == parentId);

        private T GetField<T>(string name)
            => (T)typeof(RevisionGridControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(control)!;

        private void Invoke(string name, params object?[] arguments)
        {
            try
            {
                typeof(RevisionGridControl).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(control, arguments);
            }
            catch (TargetInvocationException exception) when (exception.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            }
        }
    }
}
