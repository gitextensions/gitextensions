using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls.RevisionGridClasses.Columns
{
    internal abstract class ColumnProvider
    {
        public DataGridViewColumn Column { get; protected set; }

        public string Name { get; }

        protected ColumnProvider(string name) => Name = name;

        public int Index => Column.Index;

        public abstract void OnCellPainting(DataGridViewCellPaintingEventArgs e, GitRevision revision);
    }
}