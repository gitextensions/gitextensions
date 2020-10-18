using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUI.Theming;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class MessageColumnProvider : ColumnProvider
    {
        public const int MaxSuperprojectRefs = 4;
        private readonly StringBuilder _toolTipBuilder = new StringBuilder(200);

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
            var indicator = new MultilineIndicator(e, revision);
            var messageBounds = indicator.RemainingCellBounds;
            var superprojectRefs = new List<IGitRef>();
            var offset = ColumnLeftMargin;

            if (_grid.TryGetSuperProjectInfo(out var spi))
            {
                // Draw super project references (for submodules)
                DrawSuperprojectInfo(e, spi, revision, style, messageBounds, ref offset);

                if (spi.Refs != null && revision.ObjectId != null &&
                    spi.Refs.TryGetValue(revision.ObjectId, out var refs))
                {
                    superprojectRefs.AddRange(refs.Where(RevisionGridControl.ShowRemoteRef));
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
                    if (superprojectRef != null)
                    {
                        superprojectRefs.Remove(superprojectRef);
                    }

                    DrawRef(e, gitRef, superprojectRef, style, messageBounds, ref offset);
                }
            }

            DrawSuperprojectRefs(e, superprojectRefs, style, messageBounds, ref offset);

            if (revision.IsArtificial)
            {
                DrawArtificialRevision(e, revision, style, messageBounds, ref offset);
            }
            else
            {
                DrawCommitMessage(e, revision, style, messageBounds, indicator, ref offset);
            }
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            _toolTipBuilder.Clear();

            if (!revision.IsArtificial && (revision.HasMultiLineMessage || revision.Refs.Count != 0))
            {
                var bodySummary = _gitRevisionSummaryBuilder.BuildSummary(revision.Body);
                var initialLength = (bodySummary?.Length ?? 50) + 10;
                _toolTipBuilder.EnsureCapacity(initialLength);

                _toolTipBuilder.Append(bodySummary ?? revision.Subject + Strings.BodyNotLoaded);

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
                            _toolTipBuilder.AppendLine(Strings.MarkBisectAsGood);
                        }
                        else if (gitRef.IsBisectBad)
                        {
                            _toolTipBuilder.AppendLine(Strings.MarkBisectAsBad);
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

            if (revision.IsArtificial)
            {
                var stats = _grid.GetChangeCount(revision.ObjectId);

                if (stats != null)
                {
                    void Append(IReadOnlyList<GitItemStatus> items, string singular)
                    {
                        if (items == null || items.Count == 0)
                        {
                            return;
                        }

                        if (_toolTipBuilder.Length != 0)
                        {
                            _toolTipBuilder.AppendLine();
                        }

                        _toolTipBuilder.Append(items.Count).Append(' ');

                        if (items.Count == 1)
                        {
                            _toolTipBuilder.AppendLine(singular);
                        }
                        else
                        {
                            _toolTipBuilder.Append(singular).AppendLine("s");
                        }

                        const int maxItems = 5;

                        for (var i = 0; i < maxItems && i < items.Count; i++)
                        {
                            _toolTipBuilder.Append("- ").AppendLine(items[i].Name);
                        }

                        if (items.Count > maxItems)
                        {
                            var unlistedCount = items.Count - maxItems;
                            _toolTipBuilder.Append("- (").Append(unlistedCount).Append(" more file");
                            if (unlistedCount != 1)
                            {
                                _toolTipBuilder.Append('s');
                            }

                            _toolTipBuilder.Append(')').AppendLine();
                        }
                    }

                    // TODO use translation strings here
                    Append(stats.Changed, "changed file");
                    Append(stats.Deleted, "deleted file");
                    Append(stats.New, "new file");
                    Append(stats.SubmodulesChanged, "changed submodule");
                    Append(stats.SubmodulesDirty, "dirty submodule");

                    toolTip = _toolTipBuilder.ToString();
                    return true;
                }
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
                fill: true);

            var max = Math.Max(
                TextRenderer.MeasureText(ResourceManager.Strings.Workspace, style.NormalFont).Width,
                TextRenderer.MeasureText(ResourceManager.Strings.Index, style.NormalFont).Width);

            offset = baseOffset + max + DpiUtil.Scale(6);

            // Summary of changes
            var changes = _grid.GetChangeCount(revision.ObjectId);
            if (changes != null)
            {
                DrawArtificialCount(e, changes.Changed, Images.FileStatusModified, style, messageBounds, ref offset);
                DrawArtificialCount(e, changes.New, Images.FileStatusAdded, style, messageBounds, ref offset);
                DrawArtificialCount(e, changes.Deleted, Images.FileStatusRemoved, style, messageBounds, ref offset);
                DrawArtificialCount(e, changes.SubmodulesChanged, Images.SubmoduleRevisionDown, style, messageBounds, ref offset);
                DrawArtificialCount(e, changes.SubmodulesDirty, Images.SubmoduleDirty, style, messageBounds, ref offset);
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
            if (spi.ConflictBase == revision.ObjectId)
            {
                DrawSuperProjectRef("Base", ref offset);
            }

            if (spi.ConflictLocal == revision.ObjectId)
            {
                DrawSuperProjectRef("Local", ref offset);
            }

            if (spi.ConflictRemote == revision.ObjectId)
            {
                DrawSuperProjectRef("Remote", ref offset);
            }

            void DrawSuperProjectRef(string label, ref int currentOffset)
            {
                RevisionGridRefRenderer.DrawRef(
                    e.State.HasFlag(DataGridViewElementStates.Selected),
                    style.NormalFont,
                    ref currentOffset,
                    label,
                    Color.OrangeRed,
                    RefArrowType.NotFilled,
                    messageBounds,
                    e.Graphics);
            }
        }

        private void DrawRef(
            DataGridViewCellPaintingEventArgs e,
            IGitRef gitRef,
            IGitRef superprojectRef,
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
                dashedLine: superprojectRef != null,
                fill: true);
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

        private void DrawArtificialCount(
            DataGridViewCellPaintingEventArgs e,
            IReadOnlyList<GitItemStatus> items,
            Image icon,
            in CellStyle style,
            Rectangle messageBounds,
            ref int offset)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            var imageVerticalPadding = DpiUtil.Scale(6);
            var textHorizontalPadding = DpiUtil.Scale(4);
            var imageSize = e.CellBounds.Height - imageVerticalPadding - imageVerticalPadding;
            var imageRect = new Rectangle(
                messageBounds.Left + offset,
                e.CellBounds.Top + imageVerticalPadding,
                imageSize,
                imageSize);

            var container = e.Graphics.BeginContainer();
            e.Graphics.DrawImage(icon, imageRect);
            e.Graphics.EndContainer(container);
            offset += imageSize + textHorizontalPadding;

            var text = items.Count.ToString();
            var bounds = messageBounds.ReduceLeft(offset);
            var color = e.State.HasFlag(DataGridViewElementStates.Selected)
                ? SystemColors.HighlightText
                : SystemColors.ControlText;
            var textWidth = Math.Max(
                _grid.DrawColumnText(e, text, style.NormalFont, color, bounds),
                TextRenderer.MeasureText("88", style.NormalFont).Width);
            offset += textWidth + textHorizontalPadding;
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
            if (commitTitle.StartsWith("fixup!") || commitTitle.StartsWith("squash!"))
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

            if (lines.Length == 1)
            {
                return;
            }

            var commitBody = string.Concat(lines.Skip(1).Select(_ => " " + _));
            var bodyBounds = messageBounds.ReduceLeft(offset);
            var bodyWidth = _grid.DrawColumnText(e, commitBody, font, style.CommitBodyForeColor, bodyBounds);
            offset += bodyWidth;

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
            (revision.Body?.Trim() ?? revision.Subject ?? string.Empty).SplitLines();
    }
}
