using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs
{
    public partial class SearchWindow<T> : Form where T : class
    {
        private readonly SearchControl<T> _searchControl;

        public SearchWindow(Func<string, IEnumerable<T>> getCandidates)
        {
            InitializeComponent();
            _searchControl = new SearchControl<T>(getCandidates, OnChildSizeChanged);
            _searchControl.OnTextEntered += Close;
            _searchControl.OnCancelled += Close;
            _searchControl.Dock = DockStyle.Fill;
            tableLayoutPanel1.Controls.Add(_searchControl, 0, 1);
        }

        private void OnChildSizeChanged(Size newSize)
        {
            tableLayoutPanel1.Width = newSize.Width;
            Width = tableLayoutPanel1.Margin.Left + tableLayoutPanel1.Margin.Right + newSize.Width;
            tableLayoutPanel1.Height = newSize.Height + lblEnterFileName.Height;
            Height = tableLayoutPanel1.Height;
        }

        public T SelectedItem => _searchControl.SelectedItem;
    }
}
