using System;
using System.Threading;
using System.Windows.Forms;
using GitCommands.Statistics;
using GitUIPluginInterfaces;

namespace GitImpact
{
    public partial class FormImpact : Form
    {
        private readonly SynchronizationContext syncContext;

        public FormImpact(IGitModule Module)
        {
            syncContext = SynchronizationContext.Current;

            InitializeComponent();
            UpdateAuthorInfo("");
            Impact.Init(Module);
            Impact.UpdateData();
            Impact.Invalidated += Impact_Invalidated;
        }

        protected override void OnClosed(EventArgs e)
        {
            Impact.Stop();

            base.OnClosed(e);
        }

        void Impact_Invalidated(object sender, InvalidateEventArgs e)
        {
            syncContext.Send(o => UpdateAuthorInfo(Impact.GetSelectedAuthor()), this);
        }

        private void UpdateAuthorInfo(string author)
        {
            lblAuthor.Visible = pnlAuthorColor.Visible = !string.IsNullOrEmpty(author);

            if (lblAuthor.Visible)
            {
                ImpactLoader.DataPoint data = Impact.GetAuthorInfo(author);
                lblAuthor.Text = author + "(" + data.Commits + " Commits, " + data.ChangedLines + " Changed Lines)";
                pnlAuthorColor.BackColor = Impact.GetAuthorColor(author);

                lblAuthor.Refresh();
                pnlAuthorColor.Refresh();
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

        private void cbShowSubmodules_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAuthorInfo("");
            Impact.ShowSubmodules = cbIncludingSubmodules.Checked;
        }
    }
}
