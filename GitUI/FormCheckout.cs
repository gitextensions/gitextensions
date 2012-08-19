using System;
using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

namespace GitUI
{
    public partial class FormCheckout : GitExtensionsForm
    {
        private readonly TranslationString _noRevisionSelectedMsgBox =
            new TranslationString("Select 1 revision to checkout.");
        private readonly TranslationString _noRevisionSelectedMsgBoxCaption =
            new TranslationString("Checkout");

        public FormCheckout()
            : base(true)
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
                    Force.Checked ? Settings.LocalChanges.Reset : 0);

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