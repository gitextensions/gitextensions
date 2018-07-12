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
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                HeaderText = "Message",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = DpiUtil.Scale(500)
            };
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in (Brush backBrush, Color foreColor, Font normalFont, Font boldFont) style)
        {
            var isRowSelected = e.State.HasFlag(DataGridViewElementStates.Selected);

            var indicator = new MultilineIndicator(e, revision);

            var messageBounds = indicator.RemainingCellBounds;

            var superprojectRefs = new List<IGitRef>();

            var offset = ColumnLeftMargin;

            var normalFont = style.normalFont;

            #region Draw super project references (for submodules)

            if (_grid.TryGetSuperProjectInfo(out var spi))
            {
                if (spi.Conflict_Base == revision.Guid)
                {
                    DrawSuperProjectRef("Base");
                }

                if (spi.Conflict_Local == revision.Guid)
                {
                    DrawSuperProjectRef("Local");
                }

                if (spi.Conflict_Remote == revision.Guid)
                {
                    DrawSuperProjectRef("Remote");
                }

                if (spi.Refs?.ContainsKey(revision.Guid) == true)
                {
                    superprojectRefs.AddRange(
                        spi.Refs[revision.Guid].Where(RevisionGridControl.ShowRemoteRef));
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
                var gitRefs = revision.Refs.ToList();
                gitRefs.Sort(CompareRefs);

                foreach (var gitRef in gitRefs.Where(FilterRef))
                {
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
                        ? style.boldFont
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
                var font = gitRef.IsSelected ? style.boldFont : normalFont;

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
                // Draw the summary text
                var text = (string)e.FormattedValue;
                var bounds = messageBounds.ReduceLeft(offset);
                _grid.DrawColumnText(e, text, hasSelectedRef ? style.boldFont : normalFont, style.foreColor, bounds);

                // Draw the multi-line indicator
                indicator.Render();
            }

            return;

            int CompareRefs(IGitRef left, IGitRef right)
            {
                if (left.IsTag != right.IsTag)
                {
                    // Tags towards start
                    return right.IsTag.CompareTo(left.IsTag);
                }

                if (left.IsRemote != right.IsRemote)
                {
                    // Remote refs towards end
                    return left.IsRemote.CompareTo(right.IsRemote);
                }

                if (left.IsSelected != right.IsSelected)
                {
                    // Selected ref towards start
                    return right.IsSelected.CompareTo(left.IsSelected);
                }

                // Otherwise sort by name
                return left.Name.CompareTo(right.Name);
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
            if (!revision.IsArtificial && revision.HasMultiLineMessage)
            {
                toolTip = revision.Body ?? revision.Subject + "\n\nFull message text is not present in older commits.\nSelect this commit to populate the full message.";
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
    }
}