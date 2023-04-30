using System.Diagnostics.CodeAnalysis;
using System.Text;
using GitCommands;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUI.Theming;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class MessageColumnProvider : ColumnProvider
    {
        public const int MaxSuperprojectRefs = 4;
        private readonly StringBuilder _toolTipBuilder = new(200);

        private readonly Image _bisectGoodImage = DpiUtil.Scale(Images.BisectGood);
        private readonly Image _bisectBadImage = DpiUtil.Scale(Images.BisectBad);
        private readonly Image _fixupAndSquashImage = DpiUtil.Scale(Images.FixupAndSquashMessageMarker);

        private readonly RevisionGridControl _grid;
        private readonly IGitRevisionSummaryBuilder _gitRevisionSummaryBuilder;

        public MessageColumnProvider(RevisionGridControl grid, IGitRevisionSummaryBuilder gitRevisionSummaryBuilder)
            : base("Message")
        {
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

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            MultilineIndicator indicator = new(e, revision);
            var messageBounds = indicator.RemainingCellBounds;
            List<IGitRef> superprojectRefs = new();
            var offset = ColumnLeftMargin;

            if (_grid.TryGetSuperProjectInfo(out var spi))
            {
                // Draw super project references (for submodules)
                DrawSuperprojectInfo(e, spi, revision, style, messageBounds, ref offset);

                if (spi.Refs is not null && revision.ObjectId is not null &&
                    spi.Refs.TryGetValue(revision.ObjectId, out var refs))
                {
                    superprojectRefs.AddRange(refs);
                }
            }

            if (revision.Refs.Count != 0)
            {
                var gitRefs = SortRefs(revision.Refs.Where(FilterRef));
                foreach (var gitRef in gitRefs)
                {
                    if (offset > messageBounds.Width)
                    {
                        // Stop drawing refs if we run out of room
                        break;
                    }

                    var superprojectRef = superprojectRefs.FirstOrDefault(superGitRef => gitRef.CompleteName == superGitRef.CompleteName);
                    if (superprojectRef is not null)
                    {
                        superprojectRefs.Remove(superprojectRef);
                    }

                    DrawRef(e, gitRef, superprojectRef, style, messageBounds, ref offset);
                }
            }

            DrawSuperprojectRefs(e, superprojectRefs, style, messageBounds, ref offset);

            if (revision.IsStash)
            {
                RevisionGridRefRenderer.DrawRef(
                    e.State.HasFlag(DataGridViewElementStates.Selected),
                    style.NormalFont,
                    ref offset,
                    revision.ReflogSelector.Substring(5),
                    AppColor.OtherTag.GetThemeColor(),
                    RefArrowType.None,
                    messageBounds,
                    e.Graphics,
                    dashedLine: false,
                    fill: AppSettings.FillRefLabels);
            }

            if (revision.IsArtificial)
            {
                DrawArtificialRevision(e, revision, style, messageBounds, ref offset);
            }
            else
            {
                DrawCommitMessage(e, revision, style, messageBounds, indicator, ref offset);
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
                string bodySummary = _gitRevisionSummaryBuilder.BuildSummary(revision.Body)
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

                    foreach (var gitRef in SortRefs(revision.Refs))
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

            ArtificialCommitChangeCount changeCount = _grid.GetChangeCount(revision.ObjectId);
            if (changeCount is not null && AppSettings.ShowGitStatusForArtificialCommits)
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
            var baseOffset = offset;

            // Add fake "refs" for artificial commits
            RevisionGridRefRenderer.DrawRef(
                e.State.HasFlag(DataGridViewElementStates.Selected),
                style.NormalFont,
                ref offset,
                revision.Subject,
                AppColor.OtherTag.GetThemeColor(),
                RefArrowType.None,
                messageBounds,
                e.Graphics,
                dashedLine: false,
                fill: AppSettings.FillRefLabels);

            var max = Math.Max(
                TextRenderer.MeasureText(ResourceManager.TranslatedStrings.Workspace, style.NormalFont).Width,
                TextRenderer.MeasureText(ResourceManager.TranslatedStrings.Index, style.NormalFont).Width);

            offset = baseOffset + max + DpiUtil.Scale(6);

            // Summary of changes
            ArtificialCommitChangeCount changeCount = _grid.GetChangeCount(revision.ObjectId);
            if (changeCount is null || !AppSettings.ShowGitStatusForArtificialCommits)
            {
                return;
            }

            if (changeCount.DataValid)
            {
                if (changeCount.HasChanges)
                {
                    DrawArtificialCount(_grid, e, changeCount.Changed, Images.FileStatusModified, style, messageBounds, ref offset);
                    DrawArtificialCount(_grid, e, changeCount.New, Images.FileStatusAdded, style, messageBounds, ref offset);
                    DrawArtificialCount(_grid, e, changeCount.Deleted, Images.FileStatusRemoved, style, messageBounds, ref offset);
                    DrawArtificialCount(_grid, e, changeCount.SubmodulesChanged, Images.SubmoduleRevisionDown, style, messageBounds, ref offset);
                    DrawArtificialCount(_grid, e, changeCount.SubmodulesDirty, Images.SubmoduleDirty, style, messageBounds, ref offset);
                }
                else
                {
                    DrawArtificialCount(_grid, e, items: null, Images.RepoStateClean, style, messageBounds, ref offset);
                }
            }
            else
            {
                DrawArtificialCount(_grid, e, items: null, Images.RepoStateUnknown, style, messageBounds, ref offset);
            }

            return;

            static void DrawArtificialCount(
                RevisionGridControl grid,
                DataGridViewCellPaintingEventArgs e,
                IReadOnlyList<GitItemStatus>? items,
                Image icon,
                in CellStyle style,
                Rectangle messageBounds,
                ref int offset)
            {
                if (items is not null && items.Count == 0)
                {
                    return;
                }

                var imageVerticalPadding = DpiUtil.Scale(6);
                var textHorizontalPadding = DpiUtil.Scale(4);
                var imageSize = e.CellBounds.Height - imageVerticalPadding - imageVerticalPadding;
                Rectangle imageRect = new(
                    messageBounds.Left + offset,
                    e.CellBounds.Top + imageVerticalPadding,
                    imageSize,
                    imageSize);

                var container = e.Graphics.BeginContainer();
                e.Graphics.DrawImage(icon, imageRect);
                e.Graphics.EndContainer(container);
                offset += imageSize + textHorizontalPadding;

                var text = items?.Count.ToString() ?? "";
                var bounds = messageBounds.ReduceLeft(offset);
                var textWidth = Math.Max(
                    grid.DrawColumnText(e, text, style.NormalFont, style.ForeColor, bounds),
                    TextRenderer.MeasureText("88", style.NormalFont).Width);
                offset += textWidth + textHorizontalPadding;
            }
        }

        private void DrawSuperprojectRefs(
            DataGridViewCellPaintingEventArgs e,
            List<IGitRef> superprojectRefs,
            CellStyle style,
            Rectangle messageBounds,
            ref int offset)
        {
            for (var i = 0; i < Math.Min(MaxSuperprojectRefs, superprojectRefs.Count); i++)
            {
                var gitRef = superprojectRefs[i];
                var headColor = RevisionGridRefRenderer.GetHeadColor(gitRef);
                var gitRefName = i < (MaxSuperprojectRefs - 1) ? gitRef.Name : "…";

                var arrowType = gitRef.IsSelected
                    ? RefArrowType.Filled
                    : gitRef.IsSelectedHeadMergeSource
                        ? RefArrowType.NotFilled
                        : RefArrowType.None;
                var font = gitRef.IsSelected ? style.BoldFont : style.NormalFont;

                RevisionGridRefRenderer.DrawRef(
                    e.State.HasFlag(DataGridViewElementStates.Selected),
                    font,
                    ref offset,
                    gitRefName,
                    headColor,
                    arrowType,
                    messageBounds,
                    e.Graphics,
                    dashedLine: true);
            }
        }

        private void DrawSuperprojectInfo(
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
                RevisionGridRefRenderer.DrawRef(
                    e.State.HasFlag(DataGridViewElementStates.Selected),
                    style.NormalFont,
                    ref currentOffset,
                    label,
                    Color.OrangeRed,
                    isSelected ? RefArrowType.Filled : RefArrowType.NotFilled,
                    messageBounds,
                    e.Graphics,
                    dashedLine: true);
            }
        }

        private void DrawRef(
            DataGridViewCellPaintingEventArgs e,
            IGitRef gitRef,
            IGitRef? superprojectRef,
            CellStyle style,
            Rectangle messageBounds,
            ref int offset)
        {
            if (gitRef.IsBisect)
            {
                if (gitRef.IsBisectGood)
                {
                    DrawImage(e, _bisectGoodImage, messageBounds, ref offset);
                    return;
                }

                if (gitRef.IsBisectBad)
                {
                    DrawImage(e, _bisectBadImage, messageBounds, ref offset);
                    return;
                }
            }

            var headColor = RevisionGridRefRenderer.GetHeadColor(gitRef);

            var arrowType = gitRef.IsSelected
                ? RefArrowType.Filled
                : gitRef.IsSelectedHeadMergeSource
                    ? RefArrowType.NotFilled
                    : RefArrowType.None;

            var font = gitRef.IsSelected
                ? style.BoldFont
                : style.NormalFont;

            var name = gitRef.Name;

            if (gitRef.IsTag &&
                gitRef.IsDereference && // see note on using IsDereference in CommitInfo class
                AppSettings.ShowAnnotatedTagsMessages)
            {
                name = name + " [...]";
            }

            RevisionGridRefRenderer.DrawRef(
                e.State.HasFlag(DataGridViewElementStates.Selected),
                font,
                ref offset,
                name,
                headColor,
                arrowType,
                messageBounds,
                e.Graphics,
                dashedLine: superprojectRef is not null,
                fill: AppSettings.FillRefLabels);
        }

        private void DrawImage(
            DataGridViewCellPaintingEventArgs e,
            Image image,
            Rectangle messageBounds,
            ref int offset)
        {
            var x = e.CellBounds.X + offset;
            if (x + image.Width < messageBounds.Right)
            {
                var y = e.CellBounds.Y + ((e.CellBounds.Height - image.Height) / 2);
                e.Graphics.DrawImage(image, x, y);
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
            var lines = GetCommitMessageLines(revision);
            if (lines.Length == 0)
            {
                return;
            }

            var commitTitle = lines[0];

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
            var titleBounds = messageBounds.ReduceLeft(offset);
            int titleWidth = _grid.DrawColumnText(e, commitTitle, font, style.ForeColor, titleBounds);
            offset += titleWidth;

            if (offset > messageBounds.Width)
            {
                return;
            }

            if (lines.Length > 1 && AppSettings.ShowCommitBodyInRevisionGrid)
            {
                var commitBody = string.Concat(lines.Skip(1).Select(_ => " " + _));
                var bodyBounds = messageBounds.ReduceLeft(offset);
                var bodyWidth = _grid.DrawColumnText(e, commitBody, font, style.CommitBodyForeColor, bodyBounds);
                offset += bodyWidth;
            }

            // Draw the multi-line indicator
            indicator.Render();
        }

        private bool FilterRef(IGitRef gitRef)
        {
            if (gitRef.IsTag)
            {
                return AppSettings.ShowTags;
            }

            if (gitRef.IsRemote)
            {
                return AppSettings.ShowRemoteBranches;
            }

            return true;
        }

        private static IReadOnlyList<IGitRef> SortRefs(IEnumerable<IGitRef> refs)
        {
            var sortedRefs = refs.ToList();
            sortedRefs.Sort(CompareRefs);
            return sortedRefs;

            int CompareRefs(IGitRef left, IGitRef right)
            {
                var leftTypeRank = RefTypeRank(left);
                var rightTypeRank = RefTypeRank(right);

                var c = leftTypeRank.CompareTo(rightTypeRank);

                return c == 0
                    ? string.Compare(left.Name, right.Name, StringComparison.Ordinal)
                    : c;

                int RefTypeRank(IGitRef gitRef)
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

        private static string[] GetCommitMessageLines(GitRevision revision) =>
            (revision.Body?.Trim() ?? revision.Subject).Split(Delimiters.LineFeed, StringSplitOptions.RemoveEmptyEntries);
    }
}
