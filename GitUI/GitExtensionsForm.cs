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

        /// <summary>
        /// Restores the position of a form from the user settings. Does
        /// nothing if there is no entry for the form in the settings, or the
        /// setting would be invisible on the current display configuration.
        /// </summary>
        /// <param name="name">The name to use when looking up the position in
        /// the settings</param>
        protected void RestorePosition(String name)
        {
            WindowPosition p = LookupWindowPosition(name);

            if (p != null)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.DesktopBounds = p.rect;
                this.WindowState = p.state;
            }
            else
            {
                this.StartPosition = FormStartPosition.WindowsDefaultBounds;
                this.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// Save the position of a form to the user settings. Hides the window
        /// as a side-effect.
        /// </summary>
        /// <param name="name">The name to use when writing the position to the
        /// settings</param>
        protected void SavePosition(String name)
        {
            WindowPosition p = new WindowPosition();

            // Get the windows's visibility state:
            p.state =
                this.WindowState == FormWindowState.Normal ||
                this.WindowState == FormWindowState.Maximized ?
                this.WindowState : FormWindowState.Normal;

            // Get the window's position:
            //this.Visible = false;
            //this.WindowState = FormWindowState.Normal;
            p.rect = this.DesktopBounds;

            // Write to the user settings:
            if (GitUI.Properties.Settings.Default.WindowPositions == null)
                GitUI.Properties.Settings.Default.WindowPositions
                    = new WindowPositionList();
            GitUI.Properties.Settings.Default.WindowPositions[name] = p;
            GitUI.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Looks up a window in the user settings and returns its saved position.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>The saved window position if it exists. Null if the entry
        /// doesn't exist, or would not be visible on any screen in the user's
        /// current display setup.</returns>
        private WindowPosition LookupWindowPosition(String name)
        {
            WindowPositionList list = GitUI.Properties.Settings.Default.WindowPositions;
            if (list == null)
                return null;

            WindowPosition p = (WindowPosition)list[name];
            if (p == null || p.rect.IsEmpty)
                return null;

            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(p.rect))
                {
                    return p;
                }
            }
            return null;
        }
    }
}
