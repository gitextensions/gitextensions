using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using JetBrains.Annotations;

namespace GitUI.UserControls.RevisionGrid.Columns
{
    /// <summary>
    /// Base class for columns shown in the revisions grid control.
    /// </summary>
    internal abstract class ColumnProvider
    {
        public int ColumnLeftMargin { get; } = DpiUtil.Scale(6);

        /// <summary>The DataGrid column object that models this column.</summary>
        public DataGridViewColumn Column { get; protected set; }

        /// <summary>The display friendly name of this column.</summary>
        public string Name { get; }

        protected ColumnProvider(string name) => Name = name;

        public virtual void Refresh(int rowHeight, in VisibleRowRange range) => Column.Visible = true;

        public int Index => Column.Index;

        /// <summary>Renders the content of a cell in this column.</summary>
        public abstract void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision, int rowHeight, in CellStyle style);

        /// <summary>Formats the textual representation of a cell in this column.</summary>
        /// <remarks>Implementations may set <c>e.Value</c> to the required string, and then set <c>e.FormattingApplied</c> to <c>true</c>.</remarks>
        public virtual void OnCellFormatting(DataGridViewCellFormattingEventArgs e, GitRevision revision)
        {
        }

        public virtual void Clear()
        {
        }

        public virtual void OnColumnWidthChanged(DataGridViewColumnEventArgs e)
        {
        }

        public virtual void OnVisibleRowsChanged(in VisibleRowRange visibleRowRange)
        {
        }

        /// <summary>Attempts to get custom tool tip text for a cell in this column.</summary>
        /// <remarks>Returning <c>false</c> here will not stop a tool tip being automatically displayed for truncated text.</remarks>
        [ContractAnnotation("=>false,toolTip:null")]
        [ContractAnnotation("=>true,toolTip:notnull")]
        public virtual bool TryGetToolTip(DataGridViewCellMouseEventArgs e, GitRevision revision, [CanBeNull] out string toolTip)
        {
            toolTip = default;
            return false;
        }
    }
}