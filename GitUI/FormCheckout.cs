using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace GitUI
{
    public partial class FormCheckout : GitExtensionsForm
    {
        public FormCheckout()
        {
            InitializeComponent();
            Translate();
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                if (RevisionGrid.GetRevisions().Count != 1)
                {
                    MessageBox.Show("Select 1 revision to checkout.", "Checkout");
                    return;
                }

                var command = "checkout \"" + RevisionGrid.GetRevisions()[0].Guid + "\"";
                if (Force.Checked)
                    command += " --force";

                new FormProcess(command).ShowDialog();

                Close();
            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void FormCheckoutFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("checkout");
        }

        private void FormCheckoutLoad(object sender, EventArgs e)
        {
            RevisionGrid.Load();

            RestorePosition("checkout");
        }
    }
}