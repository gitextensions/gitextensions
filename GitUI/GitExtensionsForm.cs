using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using GitUI.Properties;

namespace GitUI
{
    public class GitExtensionsForm : Form
    {
        public GitExtensionsForm()
        {
            if (GitCommands.Settings.IconColor.Equals("default"))
                this.Icon = Resources.cow_head;
            else
            if (GitCommands.Settings.IconColor.Equals("blue"))
                this.Icon = Resources.cow_head_blue;
            else
            if (GitCommands.Settings.IconColor.Equals("purple"))
                this.Icon = Resources.cow_head_purple;
            else
            if (GitCommands.Settings.IconColor.Equals("green"))
                this.Icon = Resources.cow_head_green;
            else
            if (GitCommands.Settings.IconColor.Equals("red"))
                this.Icon = Resources.cow_head_red;
            else
            if (GitCommands.Settings.IconColor.Equals("yellow"))
                this.Icon = Resources.cow_head_yellow;
            else
                this.Icon = Resources.cow_head;

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
