using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GitUI
{
    public class GitExtensionsForm : Form
    {
        public GitExtensionsForm()
        {
            if (Application.OpenForms.Count > 0)
                this.ShowInTaskbar = false;
            else
                this.ShowInTaskbar = true;

            this.AutoScaleMode = AutoScaleMode.None;

            Button cancelButton = new Button();
            cancelButton.Click += new EventHandler(cancelButton_Click);

            this.CancelButton = cancelButton;
        }

        public virtual void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
