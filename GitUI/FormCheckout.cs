using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormCheckout : GitModuleForm
    {
        private readonly TranslationString _noRevisionSelectedMsgBox =
            new TranslationString("Select 1 revision to checkout.");
        private readonly TranslationString _noRevisionSelectedMsgBoxCaption =
            new TranslationString("Checkout");

        /// <summary>
        /// For VS designer
        /// </summary>
        private FormCheckout()
            : this(null)
        {
        }

        public FormCheckout(GitUICommands aCommands)
            : base(true, aCommands)
        {
            InitializeComponent();
            Translate();
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                if (RevisionGrid.GetSelectedRevisions().Count != 1)
                {
                    MessageBox.Show(this, _noRevisionSelectedMsgBox.Text, _noRevisionSelectedMsgBoxCaption.Text);
                    return;
                }

                string command = GitCommandHelpers.CheckoutCmd(RevisionGrid.GetSelectedRevisions()[0].Guid, 
                    Force.Checked ? LocalChangesAction.Reset : 0);

                FormProcess.ShowDialog(this, command);

                Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void FormCheckoutLoad(object sender, EventArgs e)
        {
            RevisionGrid.Load();
        }
    }
}