using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUIPluginInterfaces;

namespace GitUI;

// TODO(avalonia-port): milestone M1.1 — a plain revision list streamed by the shared
// RevisionReader. The graph column, ref labels, and the ColumnProvider pattern of the
// WinForms RevisionGridControl arrive with M1.2 (see PLAN.md).
public partial class RevisionGridControl : UserControl
{
    private readonly CancellationTokenSequence _refreshSequence = new();
    private readonly List<GitRevision> _revisions = [];

    public RevisionGridControl()
    {
        InitializeComponent();

        lstRevisions.ItemTemplate = new FuncDataTemplate<GitRevision>(CreateRevisionRow, supportsRecycling: true);
        lstRevisions.SelectionChanged += (_, _) => SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///  Gets the revision currently selected in the list, or <see langword="null"/>.
    /// </summary>
    public GitRevision? SelectedRevision => lstRevisions.SelectedItem as GitRevision;

    /// <summary>
    ///  Occurs when the selected revision changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    ///  Starts (re)loading the history of <paramref name="module"/> in the background,
    ///  streaming batches into the list as they are parsed.
    /// </summary>
    public void ReloadRevisions(IGitModule module)
    {
        CancellationToken cancellationToken = _refreshSequence.Next();

        _revisions.Clear();
        lstRevisions.ItemsSource = null;
        lblLoadingStatus.Text = "Loading…";

        RevisionObserver observer = new(this, cancellationToken);
        ThreadHelper.FileAndForget(() =>
        {
            RevisionReader reader = new(module);
            reader.GetLog(observer, revisionFilter: "--all", pathFilter: "", hasNotes: false, autostashLabel: "autostash", cancellationToken);
        });
    }

    private void AppendRevisions(IReadOnlyList<GitRevision> batch, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        _revisions.AddRange(batch);

        // Swapping the ItemsSource per batch keeps the virtualized list simple; batches are
        // few (the reader flushes at most every 500 ms). Selection tracking arrives with M1.2.
        lstRevisions.ItemsSource = _revisions.ToArray();
        lblLoadingStatus.Text = $"{_revisions.Count} revisions…";
    }

    private void OnLoadingCompleted(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            lblLoadingStatus.Text = $"{_revisions.Count} revisions";
        }
    }

    private void OnLoadingError(Exception exception, CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            lblLoadingStatus.Text = $"Failed to load revisions: {exception.Message}";
        }
    }

    private static Control CreateRevisionRow(GitRevision revision, INameScope nameScope)
    {
        Grid row = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,180,140"),
        };

        TextBlock subject = new()
        {
            Text = revision.Subject,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Avalonia.Thickness(2, 0),
        };
        TextBlock author = new()
        {
            Text = revision.Author ?? string.Empty,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Opacity = 0.85,
            Margin = new Avalonia.Thickness(8, 0, 2, 0),
        };
        TextBlock date = new()
        {
            Text = revision.AuthorDate.ToString("yyyy-MM-dd HH:mm"),
            Opacity = 0.7,
            Margin = new Avalonia.Thickness(8, 0, 2, 0),
        };

        Grid.SetColumn(subject, 0);
        Grid.SetColumn(author, 1);
        Grid.SetColumn(date, 2);
        row.Children.Add(subject);
        row.Children.Add(author);
        row.Children.Add(date);
        return row;
    }

    private sealed class RevisionObserver(RevisionGridControl owner, CancellationToken cancellationToken) : IObserver<IReadOnlyList<GitRevision>>
    {
        public void OnNext(IReadOnlyList<GitRevision> value)
            => Dispatcher.UIThread.Post(() => owner.AppendRevisions(value, cancellationToken));

        public void OnCompleted()
            => Dispatcher.UIThread.Post(() => owner.OnLoadingCompleted(cancellationToken));

        public void OnError(Exception error)
            => Dispatcher.UIThread.Post(() => owner.OnLoadingError(error, cancellationToken));
    }
}
