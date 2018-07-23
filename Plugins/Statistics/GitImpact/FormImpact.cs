using System;
using System.Windows.Forms;
using GitCommands.Statistics;
using GitUI;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitImpact
{
    public sealed partial class FormImpact : GitExtensionsFormBase
    {
        private readonly TranslationString _authorCommits = new TranslationString("{0} ({1} Commits, {2} Changed Lines)");

        public FormImpact(IGitModule module)
        {
            InitializeComponent();
            InitializeComplete();
            UpdateAuthorInfo("");
            if (module != null)
            {
                Impact.Init(module);
                Impact.UpdateData();
                Impact.Invalidated += Impact_Invalidated;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Impact.Stop();

            base.OnClosed(e);
        }

        private void Impact_Invalidated(object sender, InvalidateEventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(
                async () =>
                {
                    await this.SwitchToMainThreadAsync();
                    UpdateAuthorInfo(Impact.GetSelectedAuthor());
                });
        }

        private void UpdateAuthorInfo(string author)
        {
            lblAuthor.Visible = pnlAuthorColor.Visible = !string.IsNullOrEmpty(author);

            if (lblAuthor.Visible)
            {
                ImpactLoader.DataPoint data = Impact.GetAuthorInfo(author);
                lblAuthor.Text = string.Format(_authorCommits.Text, author, data.Commits, data.ChangedLines);
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
