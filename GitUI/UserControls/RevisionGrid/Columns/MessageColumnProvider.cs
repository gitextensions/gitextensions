using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
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
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                HeaderText = "Message",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                FillWeight = 80
            };
        }

        public override void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, (Brush backBrush, Color backColor, Color foreColor, Font normalFont, Font boldFont) style)
        {
            var isRowSelected = e.State.HasFlag(DataGridViewElementStates.Selected);

            var indicator = new MultilineIndicator(e, revision, style);

            var messageBounds = indicator.RemainingCellBounds;

            var superprojectRefs = new List<IGitRef>();

            var offset = 0;

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
                        style.normalFont,
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
                        AppSettings.ShowAnnotatedTagsMessages &&
                        AppSettings.ShowIndicatorForMultilineMessage)
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

            // Add fake "refs" for artificial commits
            if (revision.IsArtificial)
            {
                RevisionGridRefRenderer.DrawRef(isRowSelected, style.normalFont, ref offset, revision.Subject, AppSettings.OtherTagColor, RefArrowType.None, messageBounds, e.Graphics, dashedLine: false, fill: true);
            }

            // Draw the summary text
            var text = (string)e.FormattedValue;
            var bounds = messageBounds.ReduceLeft(offset);
            _grid.DrawColumnText(e, text, hasSelectedRef ? style.boldFont : style.normalFont, style.foreColor, bounds);

            // Draw the multi-line indicator
            indicator.Render();

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
        }

        public override void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
            e.Value = revision.IsArtificial
                ? revision.SubjectCount
                : revision.Subject;
            e.FormattingApplied = true;
        }

        public override bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, out string toolTip)
        {
            if (revision.HasMultiLineMessage && revision.Body != null)
            {
                toolTip = revision.Body;
                return true;
            }

            return base.TryGetToolTip(e, revision, out toolTip);
        }
    }
}