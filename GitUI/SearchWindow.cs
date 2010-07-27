using System;
using System.Collections.Generic;
using System.Drawing;
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
            pictureBox1.Visible = false;

            pictureBox1.Location = new Point(Width / 2 - pictureBox1.Width / 2, Height / 2 - pictureBox1.Height / 2);
            
            if (getCandidates == null)
            {
                throw new InvalidOperationException("getCandidates cannot be null");
            }
            this.getCandidates = getCandidates;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBox1_TextUpdate(object sender, EventArgs e)
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
            
            m_SelectedText = comboBox1.Text;
            pictureBox1.Visible = true;
            backgroundThread.Start();
        }
        
        private void SearchForCandidates()
        {
            IList<T> candidates = getCandidates(m_SelectedText);
            BeginInvoke(new Action(delegate
            {
                var selectionStart = comboBox1.SelectionStart;
                var selectionLength = comboBox1.SelectionLength;
                comboBox1.Items.Clear();
                for (int i = 0; i < candidates.Count && i < 10; i++)
                {
                    comboBox1.Items.Add(candidates[i]);
                }
                comboBox1.SelectionStart = selectionStart;
                comboBox1.SelectionLength = selectionLength;
                
                pictureBox1.Visible = false;
            }));
        }

        public T SelectedItem
        {
            get
            {
                return (T)comboBox1.SelectedItem;
            }
        }

        private void comboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Close();
            }

            if (e.KeyCode == Keys.Escape)
            {
                comboBox1.SelectedItem = null;
                Close();
            }
        }

        private void SearchWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            backgroundThread.Abort();
        }
    }
}
