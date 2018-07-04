﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;

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

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, in (Brush backBrush, Color foreColor, Font normalFont, Font boldFont) style)
        {
            var isRowSelected = e.State.HasFlag(DataGridViewElementStates.Selected);

            var indicator = new MultilineIndicator(e, revision);

            var messageBounds = indicator.RemainingCellBounds;

            var superprojectRefs = new List<IGitRef>();

            var offset = ColumnLeftMargin;

            #region Draw super project references (for submodules)

            if (_grid.TryGetSuperProjectInfo(out var spi))
            {
                if (spi.Conflict_Base == revision.Guid)
                {
                    DrawSuperProjectRef("Base", style.normalFont);
                }

                if (spi.Conflict_Local == revision.Guid)
                {
                    DrawSuperProjectRef("Local", style.normalFont);
                }

                if (spi.Conflict_Remote == revision.Guid)
                {
                    DrawSuperProjectRef("Remote", style.normalFont);
                }

                if (spi.Refs?.ContainsKey(revision.Guid) == true)
                {
                    superprojectRefs.AddRange(
                        spi.Refs[revision.Guid].Where(RevisionGridControl.ShowRemoteRef));
                }

                void DrawSuperProjectRef(string label, Font normalFont)
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
                        : style.normalFont;

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
                var font = gitRef.IsSelected ? style.boldFont : style.normalFont;

                RevisionGridRefRenderer.DrawRef(isRowSelected, font, ref offset, gitRefName, headColor, arrowType, messageBounds, e.Graphics, dashedLine: true);
            }

            if (revision.IsArtificial)
            {
                // Add fake "refs" for artificial commits
                RevisionGridRefRenderer.DrawRef(isRowSelected, style.normalFont, ref offset, revision.Subject, AppSettings.OtherTagColor, RefArrowType.None, messageBounds, e.Graphics, dashedLine: false, fill: true);

                // Summary of changes
                var count = _grid.GetChangeCount(revision.Guid);
                if (count != null && _grid.IsCountUpdated)
                {
                    ArtificialCount(count.Changed, Properties.Resources.IconFileStatusModified, style.normalFont, style.foreColor);
                    ArtificialCount(count.New, Properties.Resources.IconFileStatusAdded, style.normalFont, style.foreColor);
                    ArtificialCount(count.Deleted, Properties.Resources.IconFileStatusRemoved, style.normalFont, style.foreColor);
                    ArtificialCount(count.SubmodulesChanged, Properties.Resources.IconSubmoduleRevisionDown, style.normalFont, style.foreColor);
                    ArtificialCount(count.SubmodulesDirty, Properties.Resources.IconSubmoduleDirty, style.normalFont, style.foreColor);
                 }
            }
            else
            {
                // Draw the summary text
                var text = (string)e.FormattedValue;
                var bounds = messageBounds.ReduceLeft(offset);
                _grid.DrawColumnText(e, text, hasSelectedRef ? style.boldFont : style.normalFont, style.foreColor, bounds);

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

            void ArtificialCount(int count, Image icon, Font font, Color foreColor)
            {
                const int padding = 5;
                var imageSize = e.CellBounds.Height - padding - padding;
                if (count > 0)
                {
                    Image i = icon;
                    var rect = new Rectangle(
                        messageBounds.Left + offset + padding,
                        e.CellBounds.Top + padding,
                        imageSize,
                        imageSize);

                    var container = e.Graphics.BeginContainer();
                    e.Graphics.DrawImage(i, rect);
                    e.Graphics.EndContainer(container);
                    offset += imageSize + padding + padding;

                    var text = count.ToString();
                    var bounds = messageBounds.ReduceLeft(offset);
                    offset += _grid.DrawColumnText(e, text, font, foreColor, bounds) + padding;
                }
            }
        }

        public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            if (!revision.IsArtificial)
            {
                e.Value = revision.Subject;

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

            return base.TryGetToolTip(e, revision, out toolTip);
        }
    }
}