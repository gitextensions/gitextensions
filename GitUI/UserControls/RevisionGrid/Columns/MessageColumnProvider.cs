using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    internal sealed class MessageColumnProvider : ColumnProvider
    {
        public const int MaxSuperprojectRefs = 4;

        private readonly RevisionGridControl _grid;

        public MessageColumnProvider(RevisionGridControl grid)
            : base("Message")
        {
            _grid = grid;

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

        private readonly Image _bisectGoodImage = DpiUtil.Scale(Images.BisectGood);
        private readonly Image _bisectBadImage = DpiUtil.Scale(Images.BisectBad);
        private readonly Image _fixupAndSquashImage = DpiUtil.Scale(Images.FixupAndSquashMessageMarker);

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style)
        {
            var isRowSelected = e.State.HasFlag(DataGridViewElementStates.Selected);

            var indicator = new MultilineIndicator(e, revision);

            var messageBounds = indicator.RemainingCellBounds;

            var superprojectRefs = new List<IGitRef>();

            var offset = ColumnLeftMargin;

            var normalFont = style.NormalFont;

            #region Draw super project references (for submodules)

            if (_grid.TryGetSuperProjectInfo(out var spi))
            {
                if (spi.ConflictBase == revision.ObjectId)
                {
                    DrawSuperProjectRef("Base");
                }

                if (spi.ConflictLocal == revision.ObjectId)
                {
                    DrawSuperProjectRef("Local");
                }

                if (spi.ConflictRemote == revision.ObjectId)
                {
                    DrawSuperProjectRef("Remote");
                }

                if (spi.Refs != null &&
                    revision.ObjectId != null &&
                    spi.Refs.TryGetValue(revision.ObjectId, out var refs))
                {
                    superprojectRefs.AddRange(refs.Where(RevisionGridControl.ShowRemoteRef));
                }

                void DrawSuperProjectRef(string label)
                {
                    RevisionGridRefRenderer.DrawRef(
                        isRowSelected,
                        normalFont,
                        ref offset,
                        label,
                        Color.OrangeRed,
                        RefArrowType.NotFilled,
                        messageBounds,
                        e.Graphics);
                }
            }

            #endregion

            var hasSelectedRef = false;

            if (revision.Refs.Count != 0)
            {
                var gitRefs = SortRefs(revision.Refs);

                foreach (var gitRef in gitRefs.Where(FilterRef))
                {
                    if (offset > indicator.RemainingCellBounds.Width)
                    {
                        // Stop drawing refs if we run out of room
                        break;
                    }

                    if (gitRef.IsBisect)
                    {
                        if (gitRef.IsBisectGood)
                        {
                            DrawImage(_bisectGoodImage);
                            continue;
                        }

                        if (gitRef.IsBisectBad)
                        {
                            DrawImage(_bisectBadImage);
                            continue;
                        }
                    }

                    if (gitRef.IsSelected)
                    {
                        hasSelectedRef = true;
                    }

                    var headColor = RevisionGridRefRenderer.GetHeadColor(gitRef);

                    var arrowType = gitRef.IsSelected
                        ? RefArrowType.Filled
                        : gitRef.IsSelectedHeadMergeSource
                            ? RefArrowType.NotFilled
                            : RefArrowType.None;

                    var font = gitRef.IsSelected
                        ? style.BoldFont
                        : normalFont;

                    var superprojectRef = superprojectRefs.FirstOrDefault(superGitRef => gitRef.CompleteName == superGitRef.CompleteName);

                    if (superprojectRef != null)
                    {
                        superprojectRefs.Remove(superprojectRef);
                    }

                    var name = gitRef.Name;

                    if (gitRef.IsTag &&
                        gitRef.IsDereference && // see note on using IsDereference in CommitInfo class
                        AppSettings.ShowAnnotatedTagsMessages)
                    {
                        name = name + " [...]";
                    }

                    RevisionGridRefRenderer.DrawRef(isRowSelected, font, ref offset, name, headColor, arrowType, messageBounds, e.Graphics, dashedLine: superprojectRef != null, fill: true);
                }
            }

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
                var font = gitRef.IsSelected ? style.BoldFont : normalFont;

                RevisionGridRefRenderer.DrawRef(isRowSelected, font, ref offset, gitRefName, headColor, arrowType, messageBounds, e.Graphics, dashedLine: true);
            }

            if (revision.IsArtificial)
            {
                var baseOffset = offset;

                // Add fake "refs" for artificial commits
                RevisionGridRefRenderer.DrawRef(isRowSelected, normalFont, ref offset, revision.Subject, AppSettings.OtherTagColor, RefArrowType.None, messageBounds, e.Graphics, dashedLine: false, fill: true);

                var max = Math.Max(
                    TextRenderer.MeasureText(Strings.Workspace, normalFont).Width,
                    TextRenderer.MeasureText(Strings.Index, normalFont).Width);
                offset = baseOffset + max + DpiUtil.Scale(6);

                // Summary of changes
                var changes = _grid.GetChangeCount(revision.ObjectId);
                if (changes != null)
                {
                    DrawArtificialCount(changes.Changed, Images.FileStatusModified);
                    DrawArtificialCount(changes.New, Images.FileStatusAdded);
                    DrawArtificialCount(changes.Deleted, Images.FileStatusRemoved);
                    DrawArtificialCount(changes.SubmodulesChanged, Images.SubmoduleRevisionDown);
                    DrawArtificialCount(changes.SubmodulesDirty, Images.SubmoduleDirty);
                 }
            }
            else
            {
                var text = (string)e.FormattedValue;

                // Draw markers for fixup! and squash! commits
                if (text.StartsWith("fixup!") || text.StartsWith("squash!"))
                {
                    DrawImage(_fixupAndSquashImage);
                }

                // Draw the summary text
                var bounds = messageBounds.ReduceLeft(offset);
                _grid.DrawColumnText(e, text, hasSelectedRef ? style.BoldFont : normalFont, style.ForeColor, bounds);

                // Draw the multi-line indicator
                indicator.Render();
            }

            return;

            void DrawImage(Image image)
            {
                var x = e.CellBounds.X + offset;
                if (x + image.Width < indicator.RemainingCellBounds.Right)
                {
                    var y = e.CellBounds.Y + ((e.CellBounds.Height - image.Height) / 2);
                    e.Graphics.DrawImage(image, x, y);
                    offset += image.Width + DpiUtil.Scale(4);
                }
            }

            bool FilterRef(IGitRef gitRef)
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

            void DrawArtificialCount(IReadOnlyList<GitItemStatus> items, Image icon)
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
                    _grid.DrawColumnText(e, text, normalFont, color, bounds),
                    TextRenderer.MeasureText("88", normalFont).Width);
                offset += textWidth + textHorizontalPadding;
            }
        }

        public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            if (!revision.IsArtificial)
            {
                e.Value = revision.Subject.Trim();

                e.FormattingApplied = true;
            }
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            if (!revision.IsArtificial && (revision.HasMultiLineMessage || revision.Refs.Count != 0))
            {
                var s = new StringBuilder();

                s.Append(revision.Body?.TrimEnd() ?? revision.Subject + Strings.BodyNotLoaded);

                if (revision.Refs.Count != 0)
                {
                    if (s.Length != 0)
                    {
                        s.AppendLine();
                        s.AppendLine();
                    }

                    foreach (var gitRef in SortRefs(revision.Refs))
                    {
                        if (gitRef.IsBisectGood)
                        {
                            s.AppendLine("Marked as good in bisect");
                        }
                        else if (gitRef.IsBisectBad)
                        {
                            s.AppendLine("Marked as bad in bisect");
                        }
                        else
                        {
                            s.Append('[').Append(gitRef.Name).Append(']').AppendLine();
                        }
                    }
                }

                toolTip = s.ToString();
                return true;
            }

            if (revision.IsArtificial)
            {
                var stats = _grid.GetChangeCount(revision.ObjectId);

                if (stats != null)
                {
                    var str = new StringBuilder();

                    void Append(IReadOnlyList<GitItemStatus> items, string singular)
                    {
                        if (items == null || items.Count == 0)
                        {
                            return;
                        }

                        if (str.Length != 0)
                        {
                            str.AppendLine();
                        }

                        str.Append(items.Count).Append(' ');

                        if (items.Count == 1)
                        {
                            str.AppendLine(singular);
                        }
                        else
                        {
                            str.Append(singular).AppendLine("s");
                        }

                        const int maxItems = 5;

                        for (var i = 0; i < maxItems && i < items.Count; i++)
                        {
                            str.Append("- ").AppendLine(items[i].Name);
                        }

                        if (items.Count > maxItems)
                        {
                            var unlistedCount = items.Count - maxItems;
                            str.Append("- (").Append(unlistedCount).Append(" more file");
                            if (unlistedCount != 1)
                            {
                                str.Append('s');
                            }

                            str.Append(')').AppendLine();
                        }
                    }

                    // TODO use translation strings here
                    Append(stats.Changed, "changed file");
                    Append(stats.Deleted, "deleted file");
                    Append(stats.New, "new file");
                    Append(stats.SubmodulesChanged, "changed submodule");
                    Append(stats.SubmodulesDirty, "dirty submodule");

                    toolTip = str.ToString();
                    return true;
                }
            }

            return base.TryGetToolTip(e, revision, out toolTip);
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
    }
}