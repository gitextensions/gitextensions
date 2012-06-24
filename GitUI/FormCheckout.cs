﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
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

                var command = "checkout \"" + RevisionGrid.GetSelectedRevisions()[0].Guid + "\"";
                if (Force.Checked)
                    command += " --force";

                using (var frm = new FormProcess(command)) frm.ShowDialog(this);

                Close();
            }
            catch (Exception ex)
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