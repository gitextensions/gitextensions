using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace GitUI
{
    
    public delegate TReturn Func<T, TReturn>(T item);
    public delegate void Action<T>(T item);
    
    public partial class SearchWindow<T> : Form where T : class
    {
        private readonly Func<string, IList<T>> getCandidates;
        private Thread backgroundThread;
        private string m_SelectedText;

        public SearchWindow(Func<string, IList<T>> getCandidates)
        {
            InitializeComponent();
            
            if (getCandidates == null)
            {
                throw new InvalidOperationException("getCandidates cannot be null");
            }
            this.getCandidates = getCandidates;
        }

        private void SearchForCandidates()
        {
            IList<T> candidates = getCandidates(m_SelectedText);
            BeginInvoke(new Action(delegate
            {
                var selectionStart = textBox1.SelectionStart;
                var selectionLength = textBox1.SelectionLength;
                listBox1.BeginUpdate();
                listBox1.Items.Clear();
                
                for (int i = 0; i < candidates.Count && i < 10; i++)
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
                
            }));
        }

        public T SelectedItem
        {
            get
            {
                return (T)listBox1.SelectedItem;
            }
        }

        private void SearchWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            backgroundThread.Abort();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (backgroundThread != null)
            {
                backgroundThread.Abort();
            }

            backgroundThread = new Thread(SearchForCandidates)
            {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            backgroundThread.SetApartmentState(ApartmentState.STA);

            m_SelectedText = textBox1.Text;
            backgroundThread.Start();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Close();
            }

            if (e.KeyCode == Keys.Escape)
            {
                listBox1.SelectedItem = null;
                Close();
            }
            
            
            e.Handled = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (listBox1.Items.Count > 1)
                {
                    listBox1.SelectedIndex = (listBox1.SelectedIndex + 1) % listBox1.Items.Count;
                    e.Handled = true;
                }
            }
            if (e.KeyCode == Keys.Up)
            {
                if (listBox1.Items.Count > 1)
                {
                    listBox1.SelectedIndex = (listBox1.SelectedIndex - 1) % listBox1.Items.Count;
                    e.Handled = true;
                }
            }
            
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
