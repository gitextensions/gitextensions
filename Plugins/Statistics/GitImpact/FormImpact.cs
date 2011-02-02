using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands.Statistics;

namespace GitImpact
{
    public partial class FormImpact : Form
    {
        public FormImpact()
        {
            InitializeComponent();
            Impact.UpdateData();
        }

        private void Impact_MouseMove(object sender, MouseEventArgs e)
        {
            // Are we hovering above an author path?
            string author = Impact.GetAuthorByScreenPosition(e.X, e.Y);
            if (!string.IsNullOrEmpty(author))
            {
                // Push that author to the top of the stack
                // -> Draw it above all others
                Impact.SelectAuthor(author);
                Impact.Invalidate();
            }            
        }

        private void Impact_MouseClick(object sender, MouseEventArgs e)
        {
            // Are we hovering above an author path?
            string author = Impact.GetAuthorByScreenPosition(e.X, e.Y);
            if (!string.IsNullOrEmpty(author))
                MessageBox.Show("Author: " + author);
        }
    }
}
