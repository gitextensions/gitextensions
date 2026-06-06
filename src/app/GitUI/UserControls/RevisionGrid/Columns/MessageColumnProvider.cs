using System.Diagnostics.CodeAnalysis;
using System.Text;
using GitCommands;
using GitCommands.Config;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Extensions;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.Theming;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.UserControls.RevisionGrid.Columns;

internal sealed class MessageColumnProvider : ColumnProvider
{
    private record struct Settings
        (bool FillRefLabels,
        bool NotesInSeparateColumn,
        bool ShowAnnotatedTagsMessages,
        bool ShowCommitBodyInRevisionGrid,
        bool ShowGitNotes,
        bool ShowGitStatusForArtificialCommits,
        bool ShowRemoteBranches,
        bool ShowTags);

    public const int MaxSuperprojectRefs = 4;

    private readonly StringBuilder _toolTipBuilder = new(200);

    private readonly Image _bisectGoodImage = DpiUtil.Scale(Images.BisectGood);
    private readonly Image _bisectBadImage = DpiUtil.Scale(Images.BisectBad);
    private readonly Image _fixupAndSquashImage = DpiUtil.Scale(Images.FixupAndSquashMessageMarker);

    private readonly ICommitDataManager? _commitDataManager;
    private readonly RevisionGridControl _grid;
    private readonly IGitRevisionSummaryBuilder _gitRevisionSummaryBuilder;

    /// <summary>
    ///  Caches painted ref label hit regions per row index for mouse hit-testing.
    /// </summary>
    private readonly Dictionary<int, List<RefLabelHitInfo>> _refLabelHitInfoByRow = [];

    // Pool of reusable lists to reduce allocations during scrolling.
    private readonly Stack<List<RefLabelHitInfo>> _hitInfoListPool = new();

    // Caches the configured push prefix per remote name to avoid repeated git-config reads during painting.
    private readonly Dictionary<string, string> _remotePrefixCache = [];

    // The ref currently under the mouse cursor, used to draw a highlight border.
    private IGitRef? _highlightedRef;

    // The row index of the currently highlighted ref label.
    private int _highlightedRowIndex = -1;

    // The row index of the currently highlighted stash label (which has no IGitRef).
    private int _highlightedStashRow = -1;

    private Settings _settings;

    public MessageColumnProvider(RevisionGridControl grid, IGitRevisionSummaryBuilder gitRevisionSummaryBuilder, ICommitDataManager? commitDataManager)
        : base("Message")
    {
        _commitDataManager = commitDataManager;
        _grid = grid;
        _gitRevisionSummaryBuilder = gitRevisionSummaryBuilder;

        Column = new DataGridViewTextBoxColumn
        {
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            HeaderText = "Message",
            ReadOnly = true,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            Width = DpiUtil.Scale(500),
            MinimumWidth = DpiUtil.Scale(25)
        };
    }

    public override void ApplySettings()
    {
        _settings = new Settings(
            FillRefLabels: AppSettings.FillRefLabels,
            NotesInSeparateColumn: AppSettings.ShowGitNotesColumn.Value,
            ShowAnnotatedTagsMessages: AppSettings.ShowAnnotatedTagsMessages,
            ShowCommitBodyInRevisionGrid: AppSettings.ShowCommitBodyInRevisionGrid,
            ShowGitNotes: AppSettings.ShowGitNotes,
            ShowGitStatusForArtificialCommits: AppSettings.ShowGitStatusForArtificialCommits,
            ShowRemoteBranches: AppSettings.ShowRemoteBranches,
            ShowTags: AppSettings.ShowTags);
    }

    public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
    {
        Validates.NotNull(e.Graphics);

        MultilineIndicator indicator = new(e, revision);
        Rectangle messageBounds = indicator.RemainingCellBounds;
        List<IGitRef> superprojectRefs = [];
        int offset = ColumnLeftMargin;

        List<RefLabelHitInfo>? hitInfos = null;

        if (_grid.TryGetSuperProjectInfo(out SuperProjectInfo? spi))
        {
            // Draw super project references (for submodules)
            DrawSuperprojectInfo(e, spi, revision, style, messageBounds, ref offset);

            if (spi.Refs is not null && !revision.ObjectId.IsZero &&
                spi.Refs.TryGetValue(revision.ObjectId, out IReadOnlyList<IGitRef>? refs))
            {
                superprojectRefs.AddRange(refs);
            }
        }

        if (revision.Refs.Count != 0)
        {
            IReadOnlyList<IGitRef> gitRefs = SortRefs(revision.Refs.Where(FilterRef));
            Dictionary<string, IGitRef> trackedRemotes = BuildTrackedRemoteMap(gitRefs);

            // When there is only one local branch on this commit, remote-ref labels can omit the branch name if equal.
            int localBranchCount = gitRefs.Count(gitRef => gitRef.IsHead);
            string? singleLocalBranchName = localBranchCount == 1 ? trackedRemotes.Keys.FirstOrDefault() : null;

            foreach (IGitRef gitRef in gitRefs)
            {
                if (offset > messageBounds.Width)
                {
                    // Stop drawing refs if we run out of room
                    break;
                }

                // Remote refs that are tracked by a local branch in this row
                // are drawn condensed immediately after that local branch instead.
                if (trackedRemotes.ContainsValue(gitRef))
                {
                    continue;
                }

                IGitRef? superprojectRef = superprojectRefs.FirstOrDefault(superGitRef => gitRef.CompleteName == superGitRef.CompleteName);
                if (superprojectRef is not null)
                {
                    superprojectRefs.Remove(superprojectRef);
                }

                bool isHighlighted = _highlightedRowIndex == e.RowIndex && ReferenceEquals(_highlightedRef, gitRef);

                // If this branch is at its tracked remote, draw them condensed.
                if (gitRef.IsHead && trackedRemotes.TryGetValue(gitRef.Name, out IGitRef? remote))
                {
                    DrawBranchWithNestledRemote(gitRef, superprojectRef, style, messageBounds, ref offset, isHighlighted, remote, ref hitInfos);
                    continue;
                }

                DrawSeparateRef(gitRef, superprojectRef, style, messageBounds, ref offset, isHighlighted, singleLocalBranchName, ref hitInfos);
            }
        }

        DrawSuperprojectRefs(e, superprojectRefs, style, messageBounds, ref offset);

        if (revision.IsStash || revision.IsAutostash)
        {
            bool isStashHighlighted = _highlightedRowIndex == e.RowIndex && _highlightedRef is null && _highlightedStashRow == e.RowIndex;
            Rectangle stashRect = RevisionGridRefRenderer.DrawRef(
                e.State.HasFlag(DataGridViewElementStates.Selected),
                style.NormalFont,
                ref offset,
                revision.IsAutostash ? revision.Subject : (revision.ReflogSelector ?? throw new InvalidOperationException($"{nameof(revision.ReflogSelector)} must not be null"))[5..],
                AppColor.OtherTag.GetThemeColor(),
                RefLabelIcon.Stash,
                messageBounds,
                e.Graphics,
                dashedLine: false,
                fill: _settings.FillRefLabels,
                highlight: isStashHighlighted);
            if (stashRect != Rectangle.Empty)
            {
                hitInfos ??= RentHitInfoList();
                hitInfos.Add(new RefLabelHitInfo(stashRect, GitRef: null, StashReflogSelector: revision.ReflogSelector));
            }
        }

        if (revision.IsArtificial)
        {
            DrawArtificialRevision(e, revision, style, messageBounds, ref offset);
        }
        else if (!revision.IsAutostash)
        {
            DrawCommitMessage(e, revision, style, messageBounds, indicator, ref offset);
        }

        if (hitInfos is not null)
        {
            if (_refLabelHitInfoByRow.Remove(e.RowIndex, out List<RefLabelHitInfo>? oldList))
            {
                ReturnHitInfoList(oldList);
            }

            _refLabelHitInfoByRow[e.RowIndex] = hitInfos;
        }
        else if (_refLabelHitInfoByRow.Remove(e.RowIndex, out List<RefLabelHitInfo>? oldList))
        {
            ReturnHitInfoList(oldList);
        }

        return;

        // Builds a map of local branch name → remote ref that tracks it. No I/O is performed.
        static Dictionary<string, IGitRef> BuildTrackedRemoteMap(IReadOnlyList<IGitRef> refs)
        {
            IReadOnlyList<IGitRef> localBranches = [.. refs.Where(r => r.IsHead)];
            if (localBranches.Count == 0)
            {
                return [];
            }

            Dictionary<string, IGitRef> remoteByLocal = [];
            foreach (IGitRef remote in refs)
            {
                if (!remote.IsRemote)
                {
                    continue;
                }

                foreach (IGitRef local in localBranches)
                {
                    if (local.MergeWith != remote.LocalName || local.TrackingRemote != remote.Remote)
                    {
                        continue;
                    }

                    if (!remoteByLocal.TryAdd(local.LocalName, remote))
                    {
                        throw new InvalidOperationException($"Multiple remote refs {remote.Name} and {remoteByLocal[local.LocalName].Name} claim they were tracked by local branch '{local.LocalName}'.");
                    }
                }
            }

            return remoteByLocal;
        }

        // Draws a local branch capsule with its tracked remote capsule nestled against it, appearing as a single visual group.
        void DrawBranchWithNestledRemote(
            IGitRef gitRef,
            IGitRef? superprojectRef,
            CellStyle style,
            Rectangle messageBounds,
            ref int offset,
            bool isHighlighted,
            IGitRef remote,
            ref List<RefLabelHitInfo>? hitInfos)
        {
            bool isRemoteHighlighted = _highlightedRowIndex == e.RowIndex && ReferenceEquals(_highlightedRef, remote);

            if (!style.RemoteColors.TryGetValue(remote.Remote, out Color remoteColor))
            {
                remoteColor = RevisionGridRefRenderer.GetHeadColor(remote);
            }

            // Draw the branch with a '>' right edge that meets the remote's matching left indent.
            (Rectangle branchRect, Action? drawBranchHighlight) = DrawRef(e, gitRef, superprojectRef, style, messageBounds, ref offset, isHighlighted, gitRef.Name, RefLabelShape.PointRight);
            if (branchRect == Rectangle.Empty)
            {
                return;
            }

            // Compute the point geometry to align the remote notch exactly against the branch point tip.
            RefLabelIcon branchIcon = gitRef.IsSelected ? RefLabelIcon.Head : RefLabelIcon.LocalBranch;
            Font branchFont = gitRef.IsSelected ? style.BoldFont : style.NormalFont;
            (_, int backgroundHeight) = RevisionGridRefRenderer.MeasureRef(branchFont, gitRef.Name, branchIcon, messageBounds.Height, e.Graphics!);
            int remotePointWidth = backgroundHeight / 2;

            // Position the NotchLeft rect so its notch tip (rect.X + pointWidth) aligns with the branch point tip (branchRect.Right), cancelling the inter-label margin.
            offset = branchRect.Right - remotePointWidth - messageBounds.X + 1;

            // Show only the remote name when the tracked branch has the same local name,
            // accounting for an optional prefix configured for the remote.
            string remoteName = remote.LocalName == GetRemotePrefix(remote.Module, remote.Remote) + gitRef.Name ? remote.Remote : remote.Name;

            // Draw the remote directly via DrawRefEx with RefLabelIcon.None — the nestled remote never shows an arrow.
            (Rectangle remoteRect, Action? drawRemoteHighlight) = RevisionGridRefRenderer.DrawRefEx(
                e.State.HasFlag(DataGridViewElementStates.Selected),
                style.NormalFont,
                ref offset,
                remoteName,
                remoteColor,
                RefLabelIcon.None,
                messageBounds,
                e.Graphics!,
                fill: _settings.FillRefLabels,
                highlight: isRemoteHighlighted,
                shape: RefLabelShape.NotchLeft);

            // Draw highlight frames last so neither capsule overwrites the other's highlight edge.
            drawBranchHighlight?.Invoke();
            drawRemoteHighlight?.Invoke();

            // Register hit-boxes.
            hitInfos ??= RentHitInfoList();
            hitInfos.Add(new RefLabelHitInfo(branchRect, gitRef, StashReflogSelector: null));

            if (remoteRect == Rectangle.Empty)
            {
                return;
            }

            // The visible remote area starts at the notch tip (remoteRect.X + remotePointWidth).
            int remoteVisibleLeft = remoteRect.X + remotePointWidth - 1;
            hitInfos.Add(new RefLabelHitInfo(
                remoteRect with { X = remoteVisibleLeft, Width = remoteRect.Right - remoteVisibleLeft },
                remote,
                StashReflogSelector: null));
        }

        void DrawSeparateRef(
            IGitRef gitRef,
            IGitRef? superprojectRef,
            CellStyle style,
            Rectangle messageBounds,
            ref int offset,
            bool isHighlighted,
            string? singleLocalBranchName,
            ref List<RefLabelHitInfo>? hitInfos)
        {
            string label = !isHighlighted
                            && singleLocalBranchName is not null
                            && gitRef.IsRemote
                            && gitRef.LocalName == GetRemotePrefix(gitRef.Module, gitRef.Remote) + singleLocalBranchName
                ? gitRef.Remote : gitRef.Name;
            RefLabelShape shape = RefLabelShape.Rect;
            (Rectangle refRect, Action? drawHighlight) = DrawRef(e, gitRef, superprojectRef, style, messageBounds, ref offset, isHighlighted, label, shape);
            drawHighlight?.Invoke();
            if (refRect != Rectangle.Empty)
            {
                hitInfos ??= RentHitInfoList();
                hitInfos.Add(new RefLabelHitInfo(refRect, gitRef, StashReflogSelector: null));
            }
        }
    }

    public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
    {
        // Set the grid cell's accessibility text
        e.Value = revision.Subject.Trim();
    }

    public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, [NotNullWhen(returnValue: true)] out string? toolTip)
    {
        _toolTipBuilder.Clear();

        if (!revision.IsArtificial && (revision.HasMultiLineMessage || revision.Refs.Count != 0))
        {
            // The body is not stored for older commits (to save memory)
            string bodySummary = _gitRevisionSummaryBuilder.BuildSummary(GetBody(revision))
                ?? revision.Subject + (revision.HasMultiLineMessage ? TranslatedStrings.BodyNotLoaded : "");
            int initialLength = bodySummary.Length + 10;
            _toolTipBuilder.EnsureCapacity(initialLength);

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
                        _toolTipBuilder.Append('[').Append(gitRef.Name).Append(']').AppendLine();
                    }
                }
            }

            toolTip = _toolTipBuilder.ToString();
            return true;
        }

        if (_settings.ShowGitStatusForArtificialCommits && _grid.GetChangeCount(revision.ObjectId) is ArtificialCommitChangeCount changeCount)
        {
            toolTip = _toolTipBuilder.Append(changeCount.GetSummary()).ToString();
            return true;
        }

        return base.TryGetToolTip(e, revision, out toolTip);
    }

    private void DrawArtificialRevision(
        DataGridViewCellPaintingEventArgs e,
        GitRevision revision,
        CellStyle style,
        Rectangle messageBounds,
        ref int offset)
    {
        Graphics graphics = e.Graphics!; // validated by caller
        int baseOffset = offset;

        // Add fake "refs" for artificial commits
        RevisionGridRefRenderer.DrawRef(
            e.State.HasFlag(DataGridViewElementStates.Selected),
            style.NormalFont,
            ref offset,
            revision.Subject,
            AppColor.OtherTag.GetThemeColor(),
            revision.ObjectId == ObjectId.IndexId ? RefLabelIcon.CommitIndex : RefLabelIcon.WorkingDirectory,
            messageBounds,
            graphics,
            dashedLine: false,
            fill: _settings.FillRefLabels);

        int max = Math.Max(
            TextRenderer.MeasureText(ResourceManager.TranslatedStrings.Workspace, style.NormalFont).Width,
            TextRenderer.MeasureText(ResourceManager.TranslatedStrings.Index, style.NormalFont).Width);

        offset = baseOffset + max + DpiUtil.Scale(6);

        // Summary of changes
        if (!_settings.ShowGitStatusForArtificialCommits || _grid.GetChangeCount(revision.ObjectId) is not ArtificialCommitChangeCount changeCount)
        {
            return;
        }

        if (changeCount.DataValid)
        {
            if (changeCount.HasChanges)
            {
                DrawArtificialCount(changeCount.Changed, Images.FileStatusModified, ref offset);
                DrawArtificialCount(changeCount.New, Images.FileStatusAdded, ref offset);
                DrawArtificialCount(changeCount.Deleted, Images.FileStatusRemoved, ref offset);
                DrawArtificialCount(changeCount.SubmodulesChanged, Images.SubmoduleRevisionDown, ref offset);
                DrawArtificialCount(changeCount.SubmodulesDirty, Images.SubmoduleDirty, ref offset);
            }
            else
            {
                DrawArtificialCount(items: null, Images.RepoStateClean, ref offset);
            }
        }
        else
        {
            DrawArtificialCount(items: null, Images.RepoStateUnknown, ref offset);
        }

        return;

        void DrawArtificialCount(
            IReadOnlyList<GitItemStatus>? items,
            Image icon,
            ref int offset)
        {
            if (items?.Count is 0)
            {
                return;
            }

            int imageVerticalPadding = DpiUtil.Scale(6);
            int textHorizontalPadding = DpiUtil.Scale(4);
            int imageSize = e.CellBounds.Height - imageVerticalPadding - imageVerticalPadding;
            Rectangle imageRect = new(
                messageBounds.Left + offset,
                e.CellBounds.Top + imageVerticalPadding,
                imageSize,
                imageSize);

            System.Drawing.Drawing2D.GraphicsContainer container = graphics.BeginContainer();
            graphics.DrawImage(icon, imageRect);
            graphics.EndContainer(container);
            offset += imageSize + textHorizontalPadding;

            string text = items?.Count.ToString() ?? "";
            Rectangle bounds = messageBounds.ReduceLeft(offset);
            int textWidth = Math.Max(
                _grid.DrawColumnText(e, text, style.NormalFont, style.ForeColor, bounds),
                TextRenderer.MeasureText("88", style.NormalFont).Width);
            offset += textWidth + textHorizontalPadding;
        }
    }

    private static void DrawSuperprojectRefs(
        DataGridViewCellPaintingEventArgs e,
        List<IGitRef> superprojectRefs,
        CellStyle style,
        Rectangle messageBounds,
        ref int offset)
    {
        for (int i = 0; i < Math.Min(MaxSuperprojectRefs, superprojectRefs.Count); i++)
        {
            IGitRef gitRef = superprojectRefs[i];
            Color headColor = RevisionGridRefRenderer.GetHeadColor(gitRef);
            string gitRefName = i < (MaxSuperprojectRefs - 1) ? gitRef.Name : "…";

            RefLabelIcon icon = gitRef.IsSelected
                ? RefLabelIcon.Head
                : gitRef.IsSelectedHeadMergeSource
                    ? RefLabelIcon.HeadMergeSource
                    : gitRef.IsTag
                        ? RefLabelIcon.Tag
                        : gitRef.IsRemote
                            ? RefLabelIcon.Remote
                            : RefLabelIcon.LocalBranch;
            Font font = gitRef.IsSelected ? style.BoldFont : style.NormalFont;

            RevisionGridRefRenderer.DrawRef(
                e.State.HasFlag(DataGridViewElementStates.Selected),
                font,
                ref offset,
                gitRefName,
                headColor,
                icon,
                messageBounds,
                e.Graphics!,
                dashedLine: true);
        }
    }

    private static void DrawSuperprojectInfo(
        DataGridViewCellPaintingEventArgs e,
        SuperProjectInfo spi,
        GitRevision revision,
        CellStyle style,
        Rectangle messageBounds,
        ref int offset)
    {
        if (spi.CurrentCommit == revision.ObjectId)
        {
            DrawSuperProjectRef("", ref offset, isSelected: true);
        }

        if (spi.ConflictBase == revision.ObjectId)
        {
            DrawSuperProjectRef("Base", ref offset, isSelected: false);
        }

        if (spi.ConflictLocal == revision.ObjectId)
        {
            DrawSuperProjectRef("Local", ref offset, isSelected: false);
        }

        if (spi.ConflictRemote == revision.ObjectId)
        {
            DrawSuperProjectRef("Remote", ref offset, isSelected: false);
        }

        void DrawSuperProjectRef(string label, ref int currentOffset, bool isSelected)
        {
            // Rectangle does not have a BackColor property. Use the cell's background color instead.
            Color backColor = e.CellStyle?.BackColor ?? ThemeSettings.Default.Theme.GetColor(AppColor.EditorBackground);
            RevisionGridRefRenderer.DrawRef(
                e.State.HasFlag(DataGridViewElementStates.Selected),
                style.NormalFont,
                ref currentOffset,
                label,
                headColor: Color.OrangeRed.AdaptForeColor(backColor),
                isSelected ? RefLabelIcon.Head : RefLabelIcon.HeadMergeSource,
                messageBounds,
                e.Graphics!,
                dashedLine: true);
        }
    }

    private (Rectangle Rect, Action? DrawHighlight) DrawRef(
        DataGridViewCellPaintingEventArgs e,
        IGitRef gitRef,
        IGitRef? superprojectRef,
        CellStyle style,
        Rectangle messageBounds,
        ref int offset,
        bool highlight,
        string name,
        RefLabelShape shape)
    {
        if (gitRef.IsBisect)
        {
            if (gitRef.IsBisectGood)
            {
                DrawImage(e, _bisectGoodImage, messageBounds, ref offset);
                return (Rectangle.Empty, DrawHighlight: null);
            }

            if (gitRef.IsBisectBad)
            {
                DrawImage(e, _bisectBadImage, messageBounds, ref offset);
                return (Rectangle.Empty, DrawHighlight: null);
            }
        }

        if (!style.RemoteColors.TryGetValue(gitRef.Remote, out Color headColor))
        {
            headColor = RevisionGridRefRenderer.GetHeadColor(gitRef);
        }

        RefLabelIcon icon = gitRef.IsSelected
            ? RefLabelIcon.Head
            : gitRef.IsSelectedHeadMergeSource
                ? RefLabelIcon.HeadMergeSource
                : gitRef.IsTag
                    ? RefLabelIcon.Tag
                    : gitRef.IsRemote
                        ? RefLabelIcon.Remote
                        : RefLabelIcon.LocalBranch;

        Font font = gitRef.IsSelected
            ? style.BoldFont
            : style.NormalFont;

        if (gitRef.IsTag &&
            gitRef.IsDereference && // see note on using IsDereference in CommitInfo class
            _settings.ShowAnnotatedTagsMessages)
        {
            name += " [...]";
        }

        return RevisionGridRefRenderer.DrawRefEx(
            e.State.HasFlag(DataGridViewElementStates.Selected),
            font,
            ref offset,
            name,
            headColor,
            icon,
            messageBounds,
            e.Graphics!,
            dashedLine: superprojectRef is not null,
            fill: _settings.FillRefLabels,
            highlight,
            shape);
    }

    private static void DrawImage(
        DataGridViewCellPaintingEventArgs e,
        Image image,
        Rectangle messageBounds,
        ref int offset)
    {
        int x = e.CellBounds.X + offset;
        if (x + image.Width < messageBounds.Right)
        {
            int y = e.CellBounds.Y + ((e.CellBounds.Height - image.Height) / 2);
            e.Graphics!.DrawImage(image, x, y);
            offset += image.Width + DpiUtil.Scale(4);
        }
    }

    private void DrawCommitMessage(
        DataGridViewCellPaintingEventArgs e,
        GitRevision revision,
        CellStyle style,
        Rectangle messageBounds,
        MultilineIndicator indicator,
        ref int offset)
    {
        string[] lines = GetCommitMessageLines(revision);
        if (lines.Length == 0)
        {
            return;
        }

        string commitTitle = lines[0];

        // Draw markers for fixup! and squash! commits
        if (commitTitle.StartsWith(CommitKind.Fixup.GetPrefix()) || commitTitle.StartsWith(CommitKind.Squash.GetPrefix()) || commitTitle.StartsWith(CommitKind.Amend.GetPrefix()))
        {
            DrawImage(e, _fixupAndSquashImage, messageBounds, ref offset);
        }

        if (offset > messageBounds.Width)
        {
            return;
        }

        Font font = revision.ObjectId == _grid.CurrentCheckout ? style.BoldFont : style.NormalFont;
        Rectangle titleBounds = messageBounds.ReduceLeft(offset);
        int titleWidth = _grid.DrawColumnText(e, commitTitle, font, style.ForeColor, titleBounds);
        offset += titleWidth;

        if (offset > messageBounds.Width)
        {
            return;
        }

        if (lines.Length > 1 && _settings.ShowCommitBodyInRevisionGrid)
        {
            string commitBody = string.Concat(lines.Skip(1).Select(_ => " " + _));
            Rectangle bodyBounds = messageBounds.ReduceLeft(offset);
            int bodyWidth = _grid.DrawColumnText(e, commitBody, font, style.CommitBodyForeColor, bodyBounds);
            offset += bodyWidth;
        }

        // Draw the multi-line indicator
        indicator.Render();
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
            int leftTypeRank = RefTypeRank(left);
            int rightTypeRank = RefTypeRank(right);

            int c = leftTypeRank.CompareTo(rightTypeRank);

            return c == 0
                ? string.Compare(left.Name, right.Name, StringComparison.Ordinal)
                : c;

            static int RefTypeRank(IGitRef gitRef)
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
    }

    private string[] GetCommitMessageLines(GitRevision revision)
        => GetBody(revision)?.Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries) ?? [revision.Subject];

    private string? GetBody(GitRevision revision)
    {
        if (revision.Body is null)
        {
            if (_commitDataManager is not null && (_settings.ShowCommitBodyInRevisionGrid || _settings.ShowGitNotes || _settings.NotesInSeparateColumn))
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
    /// <returns>The matching <see cref="RefLabelHitInfo"/>, or <see langword="null"/> if no ref label was hit.</returns>
    public RefLabelHitInfo? HitTest(int rowIndex, Point gridClientPoint)
    {
        if (!_refLabelHitInfoByRow.TryGetValue(rowIndex, out List<RefLabelHitInfo>? hitInfos))
        {
            return null;
        }

        foreach (RefLabelHitInfo hitInfo in hitInfos)
        {
            if (hitInfo.Bounds.Contains(gridClientPoint))
            {
                return hitInfo;
            }
        }

        return null;
    }

    /// <summary>
    ///  Sets the ref or stash label to be drawn with a highlight border, triggering a repaint if the highlight changed.
    /// </summary>
    /// <returns><see langword="true"/> if the highlight state changed and a repaint is needed.</returns>
    public bool SetHighlight(int rowIndex, RefLabelHitInfo? hitInfo)
    {
        IGitRef? gitRef = hitInfo?.GitRef;
        int stashRow = hitInfo?.StashReflogSelector is not null ? rowIndex : -1;

        if (_highlightedRowIndex == rowIndex && ReferenceEquals(_highlightedRef, gitRef) && _highlightedStashRow == stashRow)
        {
            return false;
        }

        _highlightedRef = gitRef;
        _highlightedRowIndex = hitInfo is not null ? rowIndex : -1;
        _highlightedStashRow = stashRow;
        return true;
    }

    public override void Clear()
    {
        foreach (List<RefLabelHitInfo> list in _refLabelHitInfoByRow.Values)
        {
            ReturnHitInfoList(list);
        }

        _refLabelHitInfoByRow.Clear();
        _remotePrefixCache.Clear();
        _highlightedRef = null;
        _highlightedRowIndex = -1;
        _highlightedStashRow = -1;
    }

    private List<RefLabelHitInfo> RentHitInfoList()
    {
        return _hitInfoListPool.TryPop(out List<RefLabelHitInfo>? list) ? list : [];
    }

    private void ReturnHitInfoList(List<RefLabelHitInfo> list)
    {
        list.Clear();
        _hitInfoListPool.Push(list);
    }
}
