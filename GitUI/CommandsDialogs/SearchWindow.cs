using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    public partial class SearchWindow<T> : Form where T : class
    {
        private readonly Func<string, IList<T>> getCandidates;
        private AsyncLoader backgroundLoader = new AsyncLoader();
        
        public SearchWindow(Func<string, IList<T>> getCandidates)
        {
            InitializeComponent();
            textBox1.Select();

            if (getCandidates == null)
            {
                throw new InvalidOperationException("getCandidates cannot be null");
            }
            this.getCandidates = getCandidates;
            AutoFit();

            if (Parent == null)
            {
                TopMost = true;
                CenterToScreen();
            }
        }

        private void SearchForCandidates(IList<T> candidates)
        {
            var selectionStart = textBox1.SelectionStart;
            var selectionLength = textBox1.SelectionLength;
            listBox1.BeginUpdate();
            listBox1.Items.Clear();

            for (int i = 0; i < candidates.Count && i < 20; i++)
            {
                listBox1.Items.Add(candidates[i]);
            }

            listBox1.EndUpdate();
            if (candidates.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }
            textBox1.SelectionStart = selectionStart;
            textBox1.SelectionLength = selectionLength;
            AutoFit();
        }

        private void AutoFit()
        {
            if (listBox1.Items.Count == 0)
                listBox1.Visible = false;

            listBox1.Visible = true;

            int width = 300;

            using (Graphics g = listBox1.CreateGraphics())
            {
                for (int i1 = 0; i1 < listBox1.Items.Count; i1++)
                {
                    int itemWidth = Convert.ToInt32(g.MeasureString(Convert.ToString(listBox1.Items[i1]), listBox1.Font).Width);
                    width = Math.Max(width, itemWidth);
                }
            }

            listBox1.Width = width;
            listBox1.Height = Math.Min(800, listBox1.Font.Height * (listBox1.Items.Count + 1));

            Width = listBox1.Width + 15;
        }

        public T SelectedItem
        {
            get { return (T)listBox1.SelectedItem; }
        }

        private void SearchWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            backgroundLoader.Cancel();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string  _selectedText = textBox1.Text;

            backgroundLoader.Load(() => getCandidates(_selectedText), SearchForCandidates);
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                Close();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                listBox1.SelectedItem = null;
                e.SuppressKeyPress = true;
                Close();
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (listBox1.Items.Count > 1)
                {
                    listBox1.SelectedIndex = (listBox1.SelectedIndex + 1) % listBox1.Items.Count;
                    e.SuppressKeyPress = true;
                }
            }

            if (e.KeyCode == Keys.Up)
            {
                if (listBox1.Items.Count > 1)
                {
                    var newSelectedIndex = listBox1.SelectedIndex - 1;
                    if (newSelectedIndex < 0)
                        newSelectedIndex = listBox1.Items.Count - 1;

                    listBox1.SelectedIndex = newSelectedIndex;
                    e.SuppressKeyPress = true;
                }
            }

            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                e.SuppressKeyPress = true;
        }

        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            textBox1_KeyUp(sender, e);
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
