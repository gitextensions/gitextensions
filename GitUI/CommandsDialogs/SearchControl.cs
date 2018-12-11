using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public partial class SearchControl<T> : UserControl, IDisposable where T : class
    {
        private readonly Func<string, IEnumerable<T>> _getCandidates;
        private readonly Action<Size> _onSizeChanged;
        private readonly AsyncLoader _backgroundLoader = new AsyncLoader();
        private bool _isUpdatingTextFromCode;
        public event Action OnTextEntered;
        public event Action OnCancelled;

        public override string Text
        {
            get => txtSearchBox.Text;
            set => txtSearchBox.Text = value;
        }

        public SearchControl([NotNull]Func<string, IEnumerable<T>> getCandidates, Action<Size> onSizeChanged)
        {
            InitializeComponent();

            txtSearchBox.LostFocus += delegate { CloseDropDownWhenLostFocus(); };
            listBoxSearchResult.LostFocus += delegate { CloseDropDownWhenLostFocus(); };
            listBoxSearchResult.Left = Left;
            txtSearchBox.Select();

            _getCandidates = getCandidates;
            _onSizeChanged = onSizeChanged;
            AutoFit();

            void CloseDropDownWhenLostFocus()
            {
                if (!txtSearchBox.Focused && !listBoxSearchResult.Focused)
                {
                    CloseDropdown();
                }
            }
        }

        public void CloseDropdown()
        {
            listBoxSearchResult.Visible = false;
        }

        private void SearchForCandidates(IEnumerable<T> candidates)
        {
            var selectionStart = txtSearchBox.SelectionStart;
            var selectionLength = txtSearchBox.SelectionLength;
            listBoxSearchResult.BeginUpdate();
            listBoxSearchResult.Items.Clear();

            foreach (var candidate in candidates.Take(20))
            {
                listBoxSearchResult.Items.Add(candidate);
            }

            listBoxSearchResult.EndUpdate();
            if (listBoxSearchResult.Items.Count > 0)
            {
                listBoxSearchResult.SelectedIndex = 0;
            }

            txtSearchBox.SelectionStart = selectionStart;
            txtSearchBox.SelectionLength = selectionLength;
            AutoFit();
        }

        private void AutoFit()
        {
            if (listBoxSearchResult.Items.Count == 0)
            {
                listBoxSearchResult.Visible = false;
                return;
            }

            listBoxSearchResult.Visible = true;

            var txtBoxOnScreen = PointToScreen(txtSearchBox.Location + new Size(0, txtSearchBox.Height));
            if (ParentForm != null && !ParentForm.Controls.Contains(listBoxSearchResult))
            {
                ParentForm.Controls.Add(listBoxSearchResult);
                var listBoxLocationOnScreen = txtBoxOnScreen;
                listBoxSearchResult.Location = ParentForm.PointToClient(listBoxLocationOnScreen);
            }

            int width = 300;

            using (Graphics g = listBoxSearchResult.CreateGraphics())
            {
                for (int i1 = 0; i1 < listBoxSearchResult.Items.Count; i1++)
                {
                    int itemWidth = Convert.ToInt32(g.MeasureString(Convert.ToString(listBoxSearchResult.Items[i1]), listBoxSearchResult.Font).Width);
                    width = Math.Max(width, itemWidth);
                }
            }

            listBoxSearchResult.Width = width;
            listBoxSearchResult.Height = Math.Min(800, listBoxSearchResult.Font.Height * (listBoxSearchResult.Items.Count + 1));

            _onSizeChanged(new Size(width + Margin.Right + Margin.Left,
                listBoxSearchResult.Height + txtSearchBox.Height));

            listBoxSearchResult.BringToFront();
        }

        public T SelectedItem => (T)listBoxSearchResult.SelectedItem;

        void IDisposable.Dispose()
        {
            _backgroundLoader?.Dispose();
        }

        private void txtSearchBox_TextChange(object sender, EventArgs e)
        {
            OnTextChanged(e);
            if (_isUpdatingTextFromCode)
            {
                _isUpdatingTextFromCode = false;
                return;
            }

            string selectedText = txtSearchBox.Text;

            _backgroundLoader.LoadAsync(() => _getCandidates(selectedText), SearchForCandidates);
        }

        private void txtSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                ItemSelectedFromList();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                listBoxSearchResult.SelectedItem = null;
                listBoxSearchResult.Visible = false;
                e.SuppressKeyPress = true;
                OnCancelled?.Invoke();
            }
        }

        private void ItemSelectedFromList()
        {
            _isUpdatingTextFromCode = true;
            if (listBoxSearchResult.SelectedItem != null)
            {
                txtSearchBox.Text = listBoxSearchResult.SelectedItem.ToString();
            }

            listBoxSearchResult.Visible = false;
            OnTextEntered?.Invoke();
        }

        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (listBoxSearchResult.Items.Count > 1)
                {
                    listBoxSearchResult.SelectedIndex = (listBoxSearchResult.SelectedIndex + 1) % listBoxSearchResult.Items.Count;
                    e.SuppressKeyPress = true;
                }
            }

            if (e.KeyCode == Keys.Up)
            {
                if (listBoxSearchResult.Items.Count > 1)
                {
                    var newSelectedIndex = listBoxSearchResult.SelectedIndex - 1;
                    if (newSelectedIndex < 0)
                    {
                        newSelectedIndex = listBoxSearchResult.Items.Count - 1;
                    }

                    listBoxSearchResult.SelectedIndex = newSelectedIndex;
                    e.SuppressKeyPress = true;
                }
            }

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void listBoxSearchResult_DoubleClick(object sender, EventArgs e)
        {
            ItemSelectedFromList();
        }
    }
}
