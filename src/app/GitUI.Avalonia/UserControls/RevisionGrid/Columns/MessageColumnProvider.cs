using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Extensions;
using GitExtensions.Extensibility.Git;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class MessageColumnProvider : ColumnProvider
{
    private record struct Settings(
        bool FillRefLabels,
        bool NotesInSeparateColumn,
        bool ShowAnnotatedTagsMessages,
        bool ShowCommitBodyInRevisionGrid,
        bool ShowGitNotes,
        bool ShowRemoteBranches,
        bool ShowGitStatusForArtificialCommits,
        bool ShowRevisionGridTooltips,
        bool ShowTags);

    public const int MaxSuperprojectRefs = 4;

    private static readonly Cursor HandCursor = new(StandardCursorType.Hand);

    private readonly StringBuilder _toolTipBuilder = new(200);
    private readonly IImage _bisectGoodImage = Images.BisectGood;
    private readonly IImage _bisectBadImage = Images.BisectBad;
    private readonly IImage _fixupAndSquashImage = Images.FixupAndSquashMessageMarker;
    private readonly ICommitDataManager? _commitDataManager;
    private readonly RevisionGridControl _grid;
    private readonly IGitRevisionSummaryBuilder _gitRevisionSummaryBuilder;

    /// <summary>
    ///  Caches painted ref label hit regions per row index for mouse hit-testing.
    /// </summary>
    private readonly Dictionary<int, WeakReference<MessageCell>> _refLabelHitInfoByRow = [];

    // Pool of reusable lists to reduce allocations during scrolling.
    private readonly Stack<List<RevisionGridRefRenderer.RefLabelControl>> _hitInfoListPool = new();

    // Caches the configured push prefix per remote name to avoid repeated git-config reads during painting.
    private readonly Dictionary<string, string> _remotePrefixCache = [];
    private IReadOnlyDictionary<string, AheadBehindData>? _aheadBehindDataByLocalBranch;
    private IReadOnlyDictionary<string, AheadBehindData>? _aheadBehindDataByRemoteBranch;
    private IAheadBehindDataProvider? _aheadBehindDataProvider;
    private MessageCell? _highlightedCell;
    private RevisionGridRefRenderer.RefLabelControl? _highlightedLabel;

    // The ref currently under the mouse cursor, used to draw a highlight border.
    private IGitRef? _highlightedRef;

    // The row index of the currently highlighted ref label.
    private int _highlightedRowIndex = -1;

    // The row index of the currently highlighted stash label (which has no IGitRef).
    private int _highlightedStashRow = -1;
    private Settings _settings;

    public MessageColumnProvider(
        RevisionGridControl grid,
        IGitRevisionSummaryBuilder gitRevisionSummaryBuilder,
        ICommitDataManager? commitDataManager)
        : base("Message", new GridLength(1, GridUnitType.Star), minimumWidth: 25, resizable: true)
    {
        _commitDataManager = commitDataManager;
        _grid = grid;
        _gitRevisionSummaryBuilder = gitRevisionSummaryBuilder;
    }

    public void SetAheadBehindDataProvider(IAheadBehindDataProvider? provider)
    {
        _aheadBehindDataProvider = provider;
        Clear();
    }

    public override void ApplySettings()
    {
        Column.IsVisible = true;
        _settings = new Settings(
            AppSettings.FillRefLabels,
            AppSettings.ShowGitNotesColumn.Value,
            AppSettings.ShowAnnotatedTagsMessages,
            AppSettings.ShowCommitBodyInRevisionGrid,
            AppSettings.ShowGitNotes,
            AppSettings.ShowRemoteBranches,
            AppSettings.ShowGitStatusForArtificialCommits,
            AppSettings.ShowRevisionGridTooltips.Value,
            AppSettings.ShowTags);
    }

    public override Control CreateCell()
    {
        MessageCell panel = new(this)
        {
            Margin = new Thickness(ColumnLeftMargin, 0, 2, 0),
            ClipToBounds = true,
        };
        panel.Classes.Add("revision-message-cell");
        return panel;
    }

    public override void OnCellPainting(Control control, GitRevision revision)
    {
        MessageCell panel = (MessageCell)control;
        int rowIndex = _grid.GetRevisionIndex(revision);

        bool restoreHighlight = DetachHighlightForUpdate(panel, revision);
        panel.ContentPanel.ClearMinimumAdvances();
        panel.ContentPanel.Children.RemoveRange(0, panel.ContentPanel.Children.Count - 1);
        panel.FixupAndSquashMarker.IsVisible = false;
        panel.Body.Text = string.Empty;

        if (revision.IsArtificial)
        {
            RevisionGridRefRenderer.RefLabelControl artificialLabel = DrawArtificialRevision(revision);
            panel.ContentPanel.SetMinimumAdvance(artificialLabel, GetArtificialLabelWidth(panel));
            DrawRef(panel, artificialLabel);

            foreach (Control statusControl in CreateArtificialStatusControls(revision.ObjectId))
            {
                panel.ContentPanel.Children.Insert(panel.ContentPanel.Children.Count - 1, statusControl);
            }

            panel.Subject.Text = string.Empty;
            panel.Subject.FontWeight = FontWeight.Normal;
            panel.Revision = revision;
            panel.Indicator.Update(revision);
            ToolTip.SetTip(panel, GetArtificialToolTip(revision));
            RestoreHighlightAfterUpdate(panel, restoreHighlight);
            if (rowIndex >= 0)
            {
                _refLabelHitInfoByRow[rowIndex] = new WeakReference<MessageCell>(panel);
            }

            return;
        }

        // Draw super project references (for submodules)
        SuperProjectInfo? superProjectInfo = _grid.TryGetSuperProjectInfo(out SuperProjectInfo? info)
            ? info
            : null;
        IReadOnlyList<IGitRef>? superprojectRefs = superProjectInfo?.Refs is not null
            && superProjectInfo.Refs.TryGetValue(revision.ObjectId, out IReadOnlyList<IGitRef>? refs)
                ? refs
                : null;
        foreach (Control label in DrawSuperprojectInfo(revision, superProjectInfo)
                     .Concat(DrawSuperprojectRefs(revision, superProjectInfo)))
        {
            DrawRef(panel, label);
        }

        IReadOnlyList<IGitRef> gitRefs = SortRefs(revision.Refs.Where(FilterRef));
        foreach (IGitRef gitRef in gitRefs)
        {
            IImage? bisectImage = gitRef.IsBisectGood
                ? _bisectGoodImage
                : gitRef.IsBisectBad
                    ? _bisectBadImage
                    : null;
            if (bisectImage is not null)
            {
                panel.ContentPanel.Children.Insert(
                    panel.ContentPanel.Children.Count - 1,
                    DrawImage(bisectImage));
            }
        }

        IReadOnlyList<IGitRef> labelRefs = [.. gitRefs.Where(gitRef => !gitRef.IsBisectGood && !gitRef.IsBisectBad)];

        // When there is only one local branch on this commit, remote-ref labels can omit the branch name if equal.
        IGitRef? singleLocalBranch = labelRefs.Count(gitRef => gitRef.IsHead) == 1
            ? labelRefs.Single(gitRef => gitRef.IsHead)
            : null;
        string? singleTrackedLocalBranchName = singleLocalBranch is not null
            && labelRefs.Any(singleLocalBranch.IsTrackingRemote)
                ? singleLocalBranch.LocalName
                : null;

        // Remote refs that are tracked by a local branch in this row
        // are drawn condensed immediately after that local branch instead.
        // If this branch is at its tracked remote, draw them condensed.
        // If this branch has ahead/behind information, draw that info as virtual label of the tracked/tracking branch.
        // Builds a map of local branch name → remote ref that tracks it. No I/O is performed.
        // Draws a local branch capsule with its tracked remote capsule nestled against it, appearing as a single visual group.
        // Draw the gitRef with a '>' / '<' right edge that meets the nestledRef's matching left indent.
        // Compute the geometry to align the nestled notch/point exactly against the branch point/notch.
        // Position the NotchLeft rect so its notch tip (rect.X + pointWidth) aligns with the branch point tip (branchRect.Right), cancelling the inter-label margin.
        // Draw the nestled directly via DrawRefEx with RefLabelIcon.None — the nestled remote never shows a head indicator.
        // Draw highlight frames last so neither capsule overwrites the other's highlight edge.
        foreach (Control label in RevisionGridRefRenderer.CreateLabels(
                     labelRefs,
                     _settings.ShowTags,
                     _settings.ShowRemoteBranches,
                     _settings.FillRefLabels,
                     GetVirtualRef,
                     superprojectRefs?.Select(gitRef => gitRef.CompleteName).ToHashSet(StringComparer.Ordinal),
                     GetLabel))
        {
            if (_settings.ShowAnnotatedTagsMessages
                && label is RevisionGridRefRenderer.RefLabelControl { GitRef: { IsTag: true, IsDereference: true } } tagLabel)
            {
                tagLabel.AppendLabel(" [...]");
            }

            DrawRef(panel, label);
        }

        (string Label, string? HighlightedLabel) GetLabel(IGitRef gitRef)
        {
            if (singleTrackedLocalBranchName is not null
                && gitRef.IsRemote
                && gitRef.LocalName == GetRemotePrefix(gitRef.Module, gitRef.Remote) + singleTrackedLocalBranchName)
            {
                return (gitRef.Remote, gitRef.Name);
            }

            return (gitRef.Name, null);
        }

        if (revision.IsStash || revision.IsAutostash)
        {
            string stashLabel = revision.IsAutostash
                ? revision.Subject
                : (revision.ReflogSelector
                    ?? throw new InvalidOperationException($"{nameof(revision.ReflogSelector)} must not be null"))[5..];
            DrawRef(panel, RevisionGridRefRenderer.CreateSpecialLabel(stashLabel, RefLabelIcon.Stash, dashed: false));
        }

        string[] lines = revision.IsAutostash ? [] : GetCommitMessageLines(revision);
        DrawCommitMessage(panel, revision, lines);
        panel.Revision = revision;
        panel.Indicator.Update(revision);
        RestoreHighlightAfterUpdate(panel, restoreHighlight);

        // Register hit-boxes.
        if (rowIndex >= 0)
        {
            _refLabelHitInfoByRow[rowIndex] = new WeakReference<MessageCell>(panel);
        }

        bool DetachHighlightForUpdate(MessageCell messageCell, GitRevision updatedRevision)
        {
            if (!ReferenceEquals(_highlightedCell, messageCell))
            {
                return false;
            }

            if (_highlightedRowIndex != _grid.GetRevisionIndex(updatedRevision))
            {
                SetHighlight(cell: null, label: null);
                return false;
            }

            if (_highlightedLabel is not null)
            {
                _highlightedLabel.IsHighlighted = false;
                _highlightedLabel = null;
            }

            messageCell.Cursor = null;
            return true;
        }

        void RestoreHighlightAfterUpdate(MessageCell messageCell, bool restore)
        {
            if (!restore)
            {
                return;
            }

            RevisionGridRefRenderer.RefLabelControl? label = messageCell.GetVisualDescendants()
                .OfType<RevisionGridRefRenderer.RefLabelControl>()
                .FirstOrDefault(candidate => _highlightedRef is not null
                        ? Equals(candidate.GitRef, _highlightedRef)
                        : candidate.GitRef is null
                            && candidate.Icon == RefLabelIcon.Stash
                            && _highlightedStashRow == _grid.GetRevisionIndex(revision));
            if (label is null)
            {
                SetHighlight(cell: null, label: null);
                return;
            }

            _highlightedLabel = label;
            _highlightedLabel.IsHighlighted = true;
            messageCell.Cursor = HandCursor;
        }
    }

    public override void OnCellFormatting(Control control, GitRevision revision)
    {
        // Set the grid cell's accessibility text.
        AutomationProperties.SetName(control, revision.Subject.Trim());
    }

    public override bool TryGetToolTip(GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        _toolTipBuilder.Clear();

        if (!revision.IsArtificial && (revision.HasMultiLineMessage || revision.Refs.Count != 0))
        {
            // The body is not stored for older commits (to save memory)
            string bodySummary = _gitRevisionSummaryBuilder.BuildSummary(GetBody(revision))
                ?? revision.Subject + (revision.HasMultiLineMessage ? TranslatedStrings.BodyNotLoaded : "");
            _toolTipBuilder.EnsureCapacity(bodySummary.Length + 10);
            _toolTipBuilder.Append(bodySummary);

            if (revision.Refs.Count != 0)
            {
                if (_toolTipBuilder.Length != 0)
                {
                    _toolTipBuilder.AppendLine();
                    _toolTipBuilder.AppendLine();
                }

                foreach (IGitRef gitRef in SortRefs(revision.Refs))
                {
                    if (gitRef.IsBisectGood)
                    {
                        _toolTipBuilder.AppendLine(TranslatedStrings.MarkBisectAsGood);
                    }
                    else if (gitRef.IsBisectBad)
                    {
                        _toolTipBuilder.AppendLine(TranslatedStrings.MarkBisectAsBad);
                    }
                    else
                    {
                        _toolTipBuilder.Append('[').Append(gitRef.Name).Append(']');
                        if (GetAheadBehindData(gitRef.IsRemote, gitRef.CompleteName) is { } data)
                        {
                            _toolTipBuilder.Append("   ").Append(data.ToDisplay(reverse: gitRef.IsRemote));
                        }

                        _toolTipBuilder.AppendLine();
                    }
                }
            }

            toolTip = _toolTipBuilder.ToString();
            return true;
        }

        if (_settings.ShowGitStatusForArtificialCommits
            && _grid.GetChangeCount(revision.ObjectId) is ArtificialCommitChangeCount changeCount)
        {
            toolTip = _toolTipBuilder.Append(changeCount.GetSummary()).ToString();
            return true;
        }

        return base.TryGetToolTip(revision, out toolTip);
    }

    public override bool TryGetToolTip(
        GitRevision revision,
        IGitRef? highlightRef,
        [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        if (highlightRef is not null)
        {
            toolTip = GetRefToolTip(highlightRef);
            return toolTip is not null;
        }

        return base.TryGetToolTip(revision, highlightRef, out toolTip);
    }

    private static double GetArtificialLabelWidth(MessageCell panel)
    {
        double fontSize = panel.Subject.FontSize > 0 ? panel.Subject.FontSize : 12;
        Typeface typeface = new(
            panel.Subject.FontFamily,
            panel.Subject.FontStyle,
            panel.Subject.FontWeight);
        double workTreeWidth = Measure(ResourceManager.TranslatedStrings.Workspace);
        double indexWidth = Measure(ResourceManager.TranslatedStrings.Index);

        // RefLabelControl adds eight pixels of horizontal padding, one-pixel overlap
        // correction, and a five-pixel right margin.
        return Math.Ceiling(Math.Max(workTreeWidth, indexWidth)) + 12;

        double Measure(string text)
            => new FormattedText(
                text,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black).Width;
    }

    private IReadOnlyList<Control> CreateArtificialStatusControls(ObjectId objectId)
    {
        if (!_settings.ShowGitStatusForArtificialCommits
            || _grid.GetChangeCount(objectId) is not ArtificialCommitChangeCount changeCount)
        {
            return [];
        }

        if (!changeCount.DataValid)
        {
            return [CreateArtificialCount(icon: Images.RepoStateUnknown, count: null)];
        }

        if (!changeCount.HasChanges)
        {
            return [CreateArtificialCount(icon: Images.RepoStateClean, count: null)];
        }

        List<Control> controls = [];
        Add(changeCount.Changed, Images.FileStatusModified);
        Add(changeCount.New, Images.FileStatusAdded);
        Add(changeCount.Deleted, Images.FileStatusRemoved);
        Add(changeCount.SubmodulesChanged, Images.SubmoduleRevisionDown);
        Add(changeCount.SubmodulesDirty, Images.SubmoduleDirty);
        return controls;

        void Add(IReadOnlyList<GitItemStatus> items, IImage icon)
        {
            if (items.Count > 0)
            {
                controls.Add(CreateArtificialCount(icon, items.Count));
            }
        }
    }

    private string? GetArtificialToolTip(GitRevision revision)
    {
        if (_settings.ShowGitStatusForArtificialCommits
            && _grid.GetChangeCount(revision.ObjectId) is ArtificialCommitChangeCount changeCount)
        {
            return changeCount.GetSummary();
        }

        return _settings.ShowRevisionGridTooltips ? revision.Subject : null;
    }

    private static Control CreateArtificialCount(IImage icon, int? count)
    {
        StackPanel panel = new()
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 3,
            Margin = new Thickness(1, 0, 5, 0),
        };
        panel.Children.Add(new Image
        {
            Source = icon,
            Width = 12,
            Height = 12,
            VerticalAlignment = VerticalAlignment.Center,
        });
        if (count is int value)
        {
            panel.Children.Add(new TextBlock
            {
                Text = value.ToString(),
                MinWidth = 14,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        return panel;
    }

    private static RevisionGridRefRenderer.RefLabelControl DrawArtificialRevision(GitRevision revision)
    {
        // Add fake "refs" for artificial commits
        return RevisionGridRefRenderer.CreateSpecialLabel(
            revision.Subject,
            revision.ObjectId == ObjectId.IndexId
                ? RefLabelIcon.CommitIndex
                : RefLabelIcon.WorkingDirectory,
            dashed: false);
    }

    private static IReadOnlyList<Control> DrawSuperprojectRefs(
        GitRevision revision,
        SuperProjectInfo? superProjectInfo)
    {
        List<Control> labels = [];
        if (superProjectInfo?.Refs?.TryGetValue(revision.ObjectId, out IReadOnlyList<IGitRef>? refs) == true)
        {
            IEnumerable<IGitRef> additionalRefs = refs
                .Where(superProjectRef => revision.Refs.All(gitRef => gitRef.CompleteName != superProjectRef.CompleteName))
                .Take(MaxSuperprojectRefs);
            foreach (IGitRef gitRef in additionalRefs)
            {
                labels.Add(RevisionGridRefRenderer.CreateLabel(
                    gitRef,
                    gitRef.Name,
                    gitRef.IsTag ? RefLabelShape.PointLeft : RefLabelShape.Rect,
                    fill: false,
                    dashed: true));
            }
        }

        return labels;
    }

    private static IReadOnlyList<Control> DrawSuperprojectInfo(
        GitRevision revision,
        SuperProjectInfo? superProjectInfo)
    {
        if (superProjectInfo is null)
        {
            return [];
        }

        List<Control> labels = [];
        if (superProjectInfo.CurrentCommit == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel(string.Empty, RefLabelIcon.Head));
        }

        if (superProjectInfo.ConflictBase == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel("Base", RefLabelIcon.HeadMergeSource));
        }

        if (superProjectInfo.ConflictLocal == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel("Local", RefLabelIcon.HeadMergeSource));
        }

        if (superProjectInfo.ConflictRemote == revision.ObjectId)
        {
            labels.Add(RevisionGridRefRenderer.CreateSpecialLabel("Remote", RefLabelIcon.HeadMergeSource));
        }

        return labels;
    }

    internal static IReadOnlyList<Control> CreateSuperprojectLabels(
        GitRevision revision,
        SuperProjectInfo? superProjectInfo)
        => [.. DrawSuperprojectInfo(revision, superProjectInfo), .. DrawSuperprojectRefs(revision, superProjectInfo)];

    private static void DrawRef(MessageCell panel, Control label)
    {
        // see note on using IsDereference in CommitInfo class
        panel.ContentPanel.Children.Insert(panel.ContentPanel.Children.Count - 1, label);
    }

    private static Image DrawImage(IImage image)
    {
        Image marker = new()
        {
            Source = image,
            Width = 16,
            Height = 16,
            Margin = new Thickness(0, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        marker.Classes.Add("revision-bisect-marker");
        return marker;
    }

    private void DrawCommitMessage(MessageCell panel, GitRevision revision, string[] lines)
    {
        string commitTitle = lines.FirstOrDefault() ?? string.Empty;

        // Draw markers for fixup! and squash! commits
        panel.FixupAndSquashMarker.IsVisible = !revision.IsAutostash
            && (commitTitle.StartsWith(CommitKind.Fixup.GetPrefix(), StringComparison.Ordinal)
                || commitTitle.StartsWith(CommitKind.Squash.GetPrefix(), StringComparison.Ordinal)
                || commitTitle.StartsWith(CommitKind.Amend.GetPrefix(), StringComparison.Ordinal));
        panel.Subject.Text = revision.IsAutostash ? string.Empty : commitTitle;
        panel.Body.Text = !revision.IsAutostash && lines.Length > 1 && _settings.ShowCommitBodyInRevisionGrid
            ? string.Concat(lines.Skip(1).Select(line => " " + line))
            : string.Empty;

        // Draw the multi-line indicator
        bool emphasized = _grid.IsCurrentCheckout(revision);
        GitExtensions.Shims.WinForms.Font normalFont = AppSettings.Font;

        // Source BoldFont replaces configured Italic; NormalFont keeps its full style.
        panel.Subject.FontWeight = emphasized || normalFont.Bold
            ? FontWeight.Bold
            : FontWeight.Normal;
        panel.Subject.FontStyle = !emphasized && normalFont.Italic ? FontStyle.Italic : FontStyle.Normal;
        panel.Body.FontWeight = panel.Subject.FontWeight;
        panel.Body.FontStyle = panel.Subject.FontStyle;
    }

    private bool FilterRef(IGitRef gitRef)
    {
        if (gitRef.IsTag)
        {
            return _settings.ShowTags;
        }

        if (gitRef.IsRemote)
        {
            return _settings.ShowRemoteBranches;
        }

        return true;
    }

    private static IReadOnlyList<IGitRef> SortRefs(IEnumerable<IGitRef> refs)
    {
        List<IGitRef> sortedRefs = [.. refs];
        sortedRefs.Sort(CompareRefs);
        return sortedRefs;

        static int CompareRefs(IGitRef left, IGitRef right)
        {
            int result = GetRank(left).CompareTo(GetRank(right));
            return result == 0
                ? string.Compare(left.Name, right.Name, StringComparison.Ordinal)
                : result;
        }

        static int GetRank(IGitRef gitRef)
        {
            if (gitRef.IsBisect)
            {
                return 0;
            }

            if (gitRef.IsSelected)
            {
                return 1;
            }

            if (gitRef.IsSelectedHeadMergeSource)
            {
                return 2;
            }

            if (gitRef.IsHead)
            {
                return 3;
            }

            if (gitRef.IsRemote)
            {
                return 4;
            }

            return 5;
        }
    }

    private (IGitRef GitRef, string Name)? GetVirtualRef(IGitRef gitRef)
    {
        (string display, string trackedCompleteName, bool isGone) = GetAheadBehind(gitRef, withCounts: false);
        if (display.Length == 0)
        {
            return null;
        }

        return (new NestledVirtualRef(gitRef, trackedCompleteName, trackingBranchIsGone: isGone), display);
    }

    private string[] GetCommitMessageLines(GitRevision revision)
        => GetBody(revision)?.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries) ?? [revision.Subject];

    private string? GetBody(GitRevision revision)
    {
        if (revision.Body is null)
        {
            if (_commitDataManager is not null
                && (_settings.ShowCommitBodyInRevisionGrid || _settings.ShowGitNotes || _settings.NotesInSeparateColumn))
            {
                _commitDataManager.InitiateDelayedLoadingOfDetails(revision);
            }

            return null;
        }

        return _settings.NotesInSeparateColumn
            ? revision.Body
            : UIExtensions.FormatBodyAndNotes(revision.Body, revision.Notes);
    }

    private string GetRemotePrefix(IGitModule module, string remoteName)
    {
        if (!_remotePrefixCache.TryGetValue(remoteName, out string? prefix))
        {
            prefix = module.GetEffectiveSetting(string.Format(SettingKeyString.RemotePrefix, remoteName));
            _remotePrefixCache[remoteName] = prefix;
        }

        return prefix;
    }

    /// <summary>
    ///  Performs a hit test to find which ref label (if any) contains the given point in the specified row.
    /// </summary>
    /// <returns>The matching retained ref label, or <see langword="null"/> if no ref label was hit.</returns>
    public RevisionGridRefRenderer.RefLabelControl? HitTest(int rowIndex, Avalonia.Point gridClientPoint)
    {
        if (!_refLabelHitInfoByRow.TryGetValue(rowIndex, out WeakReference<MessageCell>? reference)
            || !reference.TryGetTarget(out MessageCell? cell))
        {
            _refLabelHitInfoByRow.Remove(rowIndex);
            return null;
        }

        List<RevisionGridRefRenderer.RefLabelControl> hitInfos = RentHitInfoList();
        try
        {
            hitInfos.AddRange(cell.GetVisualDescendants().OfType<RevisionGridRefRenderer.RefLabelControl>());
            return hitInfos.FirstOrDefault(label =>
                (label.GitRef is not null || label.Icon == RefLabelIcon.Stash)
                && _grid.TranslatePoint(gridClientPoint, label) is Avalonia.Point point
                && label.Contains(point));
        }
        finally
        {
            ReturnHitInfoList(hitInfos);
        }
    }

    /// <summary>
    ///  Sets the ref or stash label to be drawn with a highlight border, triggering a repaint if the highlight changed.
    /// </summary>
    /// <returns><see langword="true"/> if the highlight state changed and a repaint is needed.</returns>
    public bool SetHighlight(Control? cell, RevisionGridRefRenderer.RefLabelControl? label)
    {
        MessageCell? messageCell = cell as MessageCell;
        if (ReferenceEquals(_highlightedCell, messageCell) && ReferenceEquals(_highlightedLabel, label))
        {
            return false;
        }

        if (_highlightedLabel is not null)
        {
            _highlightedLabel.IsHighlighted = false;
        }

        if (_highlightedCell is not null)
        {
            _highlightedCell.Cursor = null;
        }

        _highlightedCell = messageCell;
        _highlightedLabel = label;
        _highlightedRef = label?.GitRef;
        _highlightedRowIndex = label is null || messageCell?.Revision is null
            ? -1
            : _grid.GetRevisionIndex(messageCell.Revision);
        _highlightedStashRow = label is { GitRef: null, Icon: RefLabelIcon.Stash }
            ? _highlightedRowIndex
            : -1;

        if (_highlightedLabel is not null)
        {
            _highlightedLabel.IsHighlighted = true;
        }

        if (_highlightedCell is not null)
        {
            _highlightedCell.Cursor = label is null ? null : HandCursor;
        }

        _grid.UpdateLaneHighlightForRevision(label?.GitRef, messageCell?.Revision);
        return true;
    }

    public override void Clear()
    {
        SetHighlight(cell: null, label: null);
        _aheadBehindDataByLocalBranch = null;
        _aheadBehindDataByRemoteBranch = null;
        _remotePrefixCache.Clear();
        _refLabelHitInfoByRow.Clear();
    }

    /// <summary>
    ///  Returns a tuple of the ahead/behind indicator for a local or remote branch ref label
    ///  and the <see cref="IGitRef.CompleteName"/> of the tracked (for a local ref) or tracking (for a remote ref) branch.
    /// </summary>
    /// <remarks>
    ///  Uses <see cref="AheadBehindData.ToDisplay"/> for consistent formatting with the push button and left panel.
    ///  When rendering a local branch's tracked remote as a virtual label, the perspective is inverted: what the local branch
    ///  is ahead of the remote appears as the remote being behind, and vice versa — so <see cref="AheadBehindData.BehindCount"/>
    ///  and <see cref="AheadBehindData.AheadCount"/> are swapped before formatting.
    ///  Returns an empty display string for untracked refs or when the provider is unavailable.
    /// </remarks>
    private (string Display, string TrackedCompleteName, bool IsGone) GetAheadBehind(IGitRef gitRef, bool withCounts = true)
    {
        _aheadBehindDataByLocalBranch ??= _aheadBehindDataProvider?.GetData()
            ?? FrozenDictionary<string, AheadBehindData>.Empty;

        if (gitRef.IsRemote)
        {
            // Match the remote ref via AheadBehindData.RemoteRef, which holds the full refs/remotes/… name
            // regardless of whether the remote branch is named differently from the local tracking branch.
            _aheadBehindDataByRemoteBranch ??= _aheadBehindDataByLocalBranch.Values
                .DistinctBy(data => data.RemoteRef)
                .ToFrozenDictionary(data => data.RemoteRef, data => data);

            if (_aheadBehindDataByRemoteBranch.TryGetValue(gitRef.CompleteName, out AheadBehindData aheadBehind))
            {
                return (aheadBehind.ToDisplay(withCounts), GitRefName.RefsHeadsPrefix + aheadBehind.Branch, aheadBehind.AheadCount == AheadBehindData.Gone);
            }
        }
        else if (_aheadBehindDataByLocalBranch.TryGetValue(gitRef.Name, out AheadBehindData aheadBehind))
        {
            // This info is displayed in a virtual remote ref label.
            // From the remote ref's perspective, ahead/behind are swapped relative to the local branch.
            return (aheadBehind.ToDisplay(withCounts, reverse: true), aheadBehind.RemoteRef, aheadBehind.AheadCount == AheadBehindData.Gone);
        }

        return (string.Empty, string.Empty, false);
    }

    public AheadBehindData? GetAheadBehindData(bool isRemote, string completeName)
    {
        _aheadBehindDataByLocalBranch ??= _aheadBehindDataProvider?.GetData()
            ?? FrozenDictionary<string, AheadBehindData>.Empty;

        if (isRemote)
        {
            _aheadBehindDataByRemoteBranch ??= _aheadBehindDataByLocalBranch.Values
                .DistinctBy(data => data.RemoteRef)
                .ToFrozenDictionary(data => data.RemoteRef, data => data);
            return _aheadBehindDataByRemoteBranch.TryGetValue(completeName, out AheadBehindData dataByRemote)
                ? dataByRemote
                : null;
        }

        string branchName = completeName.StartsWith(GitRefName.RefsHeadsPrefix, StringComparison.Ordinal)
            ? completeName[GitRefName.RefsHeadsPrefix.Length..]
            : completeName;
        return _aheadBehindDataByLocalBranch.TryGetValue(branchName, out AheadBehindData data)
            ? data
            : null;
    }

    private List<RevisionGridRefRenderer.RefLabelControl> RentHitInfoList()
        => _hitInfoListPool.TryPop(out List<RevisionGridRefRenderer.RefLabelControl>? list) ? list : [];

    private void ReturnHitInfoList(List<RevisionGridRefRenderer.RefLabelControl> list)
    {
        list.Clear();
        _hitInfoListPool.Push(list);
    }

    private string? GetRefToolTip(IGitRef? gitRef)
    {
        if (gitRef is null)
        {
            return null;
        }

        StringBuilder toolTip = new();
        if (gitRef is NestledVirtualRef aheadBehindRef)
        {
            bool realRefIsRemote = !aheadBehindRef.IsRemote;
            string realRefLocalName = aheadBehindRef.MergeWith;
            string realRefCompleteName = realRefIsRemote
                ? GitRefName.GetFullRemoteName(realRefLocalName, aheadBehindRef.TrackingRemote)
                : GitRefName.GetFullBranchName(realRefLocalName);
            string realRefName = RemovePrefix(
                realRefCompleteName,
                realRefIsRemote ? GitRefName.RefsRemotesPrefix : GitRefName.RefsHeadsPrefix);
            AheadBehindData? data = GetAheadBehindData(realRefIsRemote, realRefCompleteName);
            toolTip.Append('[').Append(realRefName).Append(']');
            if (realRefIsRemote)
            {
                toolTip.AppendLine().AppendFormat(
                    TranslatedStrings.IsTrackedBy_Branch_AheadBehind,
                    data?.Branch,
                    data?.ToDisplay());
            }
            else
            {
                AppendTrackingDetails(toolTip, data);
            }

            return toolTip.ToString();
        }

        AheadBehindData? aheadBehind = GetAheadBehindData(gitRef.IsRemote, gitRef.CompleteName);
        toolTip.Append('[').Append(gitRef.Name).Append(']');
        if (gitRef.IsRemote)
        {
            if (aheadBehind is not null)
            {
                toolTip.AppendLine().AppendFormat(
                    TranslatedStrings.IsTrackedBy_Branch_AheadBehind,
                    aheadBehind.Value.Branch,
                    aheadBehind.Value.ToDisplay());
            }
            else if (_settings.ShowRevisionGridTooltips)
            {
                toolTip.AppendLine().Append(TranslatedStrings.IsRemoteBranch);
            }
            else
            {
                return null;
            }
        }
        else if (gitRef.IsHead)
        {
            if (aheadBehind is not null)
            {
                AppendTrackingDetails(toolTip, aheadBehind);
            }
            else if (_settings.ShowRevisionGridTooltips)
            {
                toolTip.AppendLine().Append(TranslatedStrings.IsLocalBranch);
            }
            else
            {
                return null;
            }
        }
        else if (gitRef.IsTag)
        {
            if (_settings.ShowRevisionGridTooltips)
            {
                toolTip.AppendLine().Append(TranslatedStrings.IsTag);
            }
            else
            {
                return null;
            }
        }

        return toolTip.ToString();

        static void AppendTrackingDetails(StringBuilder builder, AheadBehindData? data)
        {
            string? remoteBranch = data is null
                ? null
                : RemovePrefix(data.Value.RemoteRef, GitRefName.RefsRemotesPrefix);
            if (data?.AheadCount == AheadBehindData.Gone)
            {
                builder.AppendLine().AppendFormat(TranslatedStrings.WasTracking_Remote, remoteBranch);
            }
            else
            {
                builder.Append("   ").AppendLine(data?.ToDisplay())
                    .AppendFormat(TranslatedStrings.IsTracking_Remote, remoteBranch);
            }
        }

        static string RemovePrefix(string value, string prefix)
            => value.StartsWith(prefix, StringComparison.Ordinal) ? value[prefix.Length..] : value;
    }

    private sealed class MessageContentPanel : Panel
    {
        private const int RefMarginRight = 5;

        private readonly Dictionary<Control, double> _minimumAdvances = [];

        public void ClearMinimumAdvances() => _minimumAdvances.Clear();

        public void SetMinimumAdvance(Control control, double advance)
            => _minimumAdvances[control] = advance;

        protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
        {
            double offset = 0;
            double height = 0;
            foreach (Control child in Children.Where(child => child.IsVisible))
            {
                double remainingWidth = Math.Max(0, availableSize.Width - offset);
                child.Measure(new Avalonia.Size(remainingWidth, availableSize.Height));
                offset += GetAdvance(child, remainingWidth);
                height = Math.Max(height, child.DesiredSize.Height);
            }

            return new Avalonia.Size(offset, height);
        }

        protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
        {
            double offset = 0;
            foreach (Control child in Children.Where(child => child.IsVisible))
            {
                double remainingWidth = Math.Max(0, finalSize.Width - offset);
                double advance = GetAdvance(child, remainingWidth);
                child.Arrange(new Rect(offset, 0, Math.Min(remainingWidth, advance), finalSize.Height));
                offset += advance;
            }

            return finalSize;
        }

        private double GetAdvance(Control child, double remainingWidth)
        {
            // A horizontal StackPanel measures refs with infinite width. DrawRefEx
            // instead clips each capsule to the remaining cell before advancing by
            // its painted width plus five; an empty capsule does not advance.
            double advance = child switch
            {
                RevisionGridRefRenderer.RefLabelControl label => GetRefAdvance(label, remainingWidth),
                RevisionGridRefRenderer.NestledRefLabelPanel pair => GetPairAdvance(pair, remainingWidth),
                _ => child.DesiredSize.Width,
            };

            // Artificial rows reserve the status-count span independently of the
            // capsule paint bounds. MinWidth would force that capsule past clipping.
            return Math.Max(advance, _minimumAdvances.GetValueOrDefault(child));
        }

        private static double GetRefAdvance(RevisionGridRefRenderer.RefLabelControl label, double remainingWidth)
        {
            int width = label.GetCapsuleWidth(remainingWidth);
            return width > 0 ? width + RefMarginRight : 0;
        }

        private static double GetPairAdvance(RevisionGridRefRenderer.NestledRefLabelPanel pair, double remainingWidth)
        {
            RevisionGridRefRenderer.RefLabelControl first = (RevisionGridRefRenderer.RefLabelControl)pair.Children[0];
            RevisionGridRefRenderer.RefLabelControl second = (RevisionGridRefRenderer.RefLabelControl)pair.Children[1];
            int firstWidth = first.GetCapsuleWidth(remainingWidth);
            if (firstWidth <= 0)
            {
                return 0;
            }

            // DrawNestled resets from the clipped first rectangle rather than its
            // advance, then the second DrawRefEx consumes only the remaining width.
            double secondX = Math.Max(0, firstWidth - pair.PointWidth + 1);
            int secondWidth = second.GetCapsuleWidth(Math.Max(0, remainingWidth - secondX));
            return secondX + (secondWidth > 0 ? secondWidth + RefMarginRight : 0);
        }
    }

    private sealed class MessageCell : DockPanel
    {
        private readonly MessageColumnProvider _provider;

        public MessageCell(MessageColumnProvider provider)
        {
            _provider = provider;

            // DataGridView routes input over the entire cell, including unpainted
            // rounded corners. Avalonia otherwise hit-tests only the drawn children.
            Background = Brushes.Transparent;
            FixupAndSquashMarker.Source = provider._fixupAndSquashImage;
            FixupAndSquashMarker.Classes.Add("revision-message-marker");
            Subject.Classes.Add("revision-subject");
            Subject.Margin = default;
            Body.Classes.Add("revision-body");
            MessagePanel.Children.Add(FixupAndSquashMarker);
            MessagePanel.Children.Add(Subject);
            MessagePanel.Children.Add(Body);
            ContentPanel.Children.Add(MessagePanel);
            SetDock(Indicator, Dock.Right);
            Children.Add(Indicator);
            Children.Add(ContentPanel);
            PointerMoved += OnPointerMoved;
            PointerExited += provider._grid.OnGridViewCellMouseLeave;
            PointerExited += (_, _) =>
            {
                ToolTip.SetTip(this, _provider._settings.ShowRevisionGridTooltips ? Revision?.Subject : null);
            };
            DoubleTapped += OnDoubleTapped;
        }

        public MessageContentPanel ContentPanel { get; } = new();

        public StackPanel MessagePanel { get; } = new() { Orientation = Orientation.Horizontal };

        public Image FixupAndSquashMarker { get; } = new()
        {
            Width = 16,
            Height = 16,
            Margin = new Thickness(0, 0, 4, 0),
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = false,
        };

        public TextBlock Subject { get; } = CreateTextBlock();

        public TextBlock Body { get; } = CreateTextBlock();

        public MultilineIndicator Indicator { get; } = new();

        public GitRevision? Revision { get; set; }

        protected override Avalonia.Size ArrangeOverride(Avalonia.Size finalSize)
        {
            Indicator.UpdateAvailableWidth(finalSize.Width);
            return base.ArrangeOverride(finalSize);
        }

        public void ClearHighlight()
        {
            if (ReferenceEquals(_provider._highlightedCell, this))
            {
                _provider.SetHighlight(cell: null, label: null);
            }
            else
            {
                Cursor = null;
            }
        }

        private RevisionGridRefRenderer.RefLabelControl? HitTest(Func<Visual, Avalonia.Point> getPosition)
            => Revision is not null
                ? _provider.HitTest(_provider._grid.GetRevisionIndex(Revision), getPosition(_provider._grid))
                : null;

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            RevisionGridRefRenderer.RefLabelControl? label = HitTest(e.GetPosition);
            _provider.SetHighlight(this, label);

            string? toolTip = null;
            if (Revision is not null)
            {
                _provider.TryGetToolTip(Revision, label?.GitRef, out toolTip);
            }

            ToolTip.SetTip(this, toolTip);
        }

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (HitTest(e.GetPosition) is { GitRef: not null } label
                && _provider._grid.TryGoToRelatedRef(label.GitRef))
            {
                e.Handled = true;
            }
        }
    }
}
