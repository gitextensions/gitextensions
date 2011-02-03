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
            UpdateAuthorInfo("");
            Impact.UpdateData();
        }

        private void UpdateAuthorInfo(string author)
        {
            lblAuthor.Visible = pnlAuthorColor.Visible = !string.IsNullOrEmpty(author);

            if (lblAuthor.Visible)
            {
                GitCommands.Statistics.Impact.DataPoint data = Impact.GetAuthorInfo(author);
                lblAuthor.Text = author + "(" + data.Commits + " Commits, " + data.ChangedLines + " Changed Lines)";
                pnlAuthorColor.BackColor = Impact.GetAuthorColor(author);
            }
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
                UpdateAuthorInfo(author);
                Impact.Invalidate();
            }            
        }
    }
}
