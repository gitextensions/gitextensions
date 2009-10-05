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
        }
    }
}
