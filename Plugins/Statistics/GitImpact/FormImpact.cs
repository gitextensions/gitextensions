using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GitCommands.Statistics;
using System.Threading;

namespace GitImpact
{
    public partial class FormImpact : Form
    {
        private readonly SynchronizationContext syncContext;

        public FormImpact()
        {
            syncContext = SynchronizationContext.Current;

            InitializeComponent();
            UpdateAuthorInfo("");
            Impact.UpdateData();
            Impact.Invalidated += new InvalidateEventHandler(Impact_Invalidated);
        }

        void Impact_Invalidated(object sender, InvalidateEventArgs e)
        {
            syncContext.Send(new SendOrPostCallback(delegate(object o)
            {
                UpdateAuthorInfo(Impact.GetSelectedAuthor());
            }), this);
        }

        private void UpdateAuthorInfo(string author)
        {
            lblAuthor.Visible = pnlAuthorColor.Visible = !string.IsNullOrEmpty(author);

            if (lblAuthor.Visible)
            {
                GitCommands.Statistics.ImpactLoader.DataPoint data = Impact.GetAuthorInfo(author);
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
                Impact.Invalidate();
            }            
        }
    }
}
