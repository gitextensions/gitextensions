using System.Reflection;
using System.Runtime.ExceptionServices;
using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;

namespace GitExtensionsTests;

// parity-scaffolding: Native-only fixture access stays in tests rather than extending the original product accessor.
internal static class RevisionGridTestSupport
{
    internal static Accessor GetNativeTestAccessor(this RevisionGridControl control) => new(control);

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

        public void CompleteLoading(CancellationToken cancellationToken = default)
            => Invoke("OnLoadingCompleted", cancellationToken, default(ObjectId), null);

        public IObserver<IReadOnlyList<GitRevision>> CreateRevisionObserver(CancellationToken cancellationToken)
        {
            IGitUICommands commands = (IGitUICommands)typeof(RevisionGridControl)
                .GetProperty("UICommands", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .GetValue(control)!;
            RevisionLoadEventArgs args = new(control, commands,
                new Lazy<IReadOnlyList<IGitRef>>(() => []),
                new Lazy<IReadOnlyCollection<GitRevision>>(() => []), forceRefresh: false);
            return (IObserver<IReadOnlyList<GitRevision>>)Activator.CreateInstance(
                typeof(RevisionGridControl).GetNestedType("RevisionObserver", BindingFlags.NonPublic)!,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null, args: [control, cancellationToken, args, null], culture: null)!;
        }

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
