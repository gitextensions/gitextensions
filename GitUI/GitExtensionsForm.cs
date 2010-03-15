using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using GitUI.Properties;
using System.Drawing;

namespace GitUI
{
    public class GitExtensionsForm : Form
    {
        private static Icon ApplicationIcon = GetApplicationIcon();

        private static Icon GetApplicationIcon()
        {
            int randomIcon = -1;
            if (GitCommands.Settings.IconColor.Equals("random"))
                randomIcon = new Random(DateTime.Now.Millisecond).Next(6);

            if (GitCommands.Settings.IconColor.Equals("default") || randomIcon == 0)
                return Resources.cow_head;
            if (GitCommands.Settings.IconColor.Equals("blue") || randomIcon == 1)
                return Resources.cow_head_blue;
            if (GitCommands.Settings.IconColor.Equals("purple") || randomIcon == 2)
                return Resources.cow_head_purple;
            if (GitCommands.Settings.IconColor.Equals("green") || randomIcon == 3)
                return Resources.cow_head_green;
            if (GitCommands.Settings.IconColor.Equals("red") || randomIcon == 4)
                return Resources.cow_head_red;
            if (GitCommands.Settings.IconColor.Equals("yellow") || randomIcon == 5)
                return Resources.cow_head_yellow;

            return Resources.cow_head;
        }

        public GitExtensionsForm()
        {
            this.Icon = ApplicationIcon;

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
