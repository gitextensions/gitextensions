using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GitUI.Properties;
using ResourceManager.Translation;
using Settings = GitCommands.Settings;
using System.Configuration;
using System.IO;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace GitUI
{
    public class GitExtensionsForm : Form
    {
        private static readonly Icon ApplicationIcon = GetApplicationIcon();

        private bool _translated;

        public GitExtensionsForm()
        {
            Icon = ApplicationIcon;
            SetFont();

            ShowInTaskbar = Application.OpenForms.Count <= 0 || (Application.OpenForms.Count == 1 && Application.OpenForms[0] is FormSplash);
            AutoScaleMode = AutoScaleMode.None;

            var cancelButton = new Button();
            cancelButton.Click += CancelButtonClick;

            CancelButton = cancelButton;

            Load += GitExtensionsFormLoad;
            FormClosed += GitExtensionsFormFormClosed;
            Shown += GitExtensionsForm_Shown;
        }

        private static void GitExtensionsForm_Shown(object sender, EventArgs e)
        {
            //This is not very good practice, but it works and it is fast.
            FormSplash.Hide();
        }

        private void SetFont()
        {
            Font = SystemFonts.MessageBoxFont;
        }

        private static Icon GetApplicationIcon()
        {
            var randomIcon = -1;
            if (Settings.IconColor.Equals("random"))
                randomIcon = new Random(DateTime.Now.Millisecond).Next(6);

            if (Settings.IconColor.Equals("default") || randomIcon == 0)
                return Resources.cow_head;
            if (Settings.IconColor.Equals("blue") || randomIcon == 1)
                return Resources.cow_head_blue;
            if (Settings.IconColor.Equals("purple") || randomIcon == 2)
                return Resources.cow_head_purple;
            if (Settings.IconColor.Equals("green") || randomIcon == 3)
                return Resources.cow_head_green;
            if (Settings.IconColor.Equals("red") || randomIcon == 4)
                return Resources.cow_head_red;
            if (Settings.IconColor.Equals("yellow") || randomIcon == 5)
                return Resources.cow_head_yellow;

            return Resources.cow_head;
        }

        private static bool CheckComponent(object value)
        {
            var component = value as IComponent;
            if (component == null)
                return false;

            var site = component.Site;
            return (site != null) && site.DesignMode;
        }

        private void GitExtensionsFormLoad(object sender, EventArgs e)
        {
            // find out if the value is a component and is currently in design mode
            var isComponentInDesignMode = CheckComponent(this);

            if (!_translated && !isComponentInDesignMode)
                throw new Exception("The control " + GetType().Name +
                                    " is not translated in the constructor. You need to call Translate() right after InitializeComponent().");
        }

        protected void Translate()
        {
            var translator = new Translator(Settings.Translation);
            translator.TranslateControl(this);
            _translated = true;

        }

        public virtual void CancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        public virtual void GitExtensionsFormFormClosed(object sender, EventArgs e)
        {
            if (TaskbarManager.IsPlatformSupported)
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
        }

        /// <summary>
        ///   Restores the position of a form from the user settings. Does
        ///   nothing if there is no entry for the form in the settings, or the
        ///   setting would be invisible on the current display configuration.
        /// </summary>
        /// <param name = "name">The name to use when looking up the position in
        ///   the settings</param>
        protected void RestorePosition(String name)
        {
            var position = LookupWindowPosition(name);

            if (position == null)
                return;

            StartPosition = FormStartPosition.Manual;
            DesktopBounds = position.Rect;
            WindowState = position.State;
        }

        /// <summary>
        ///   Save the position of a form to the user settings. Hides the window
        ///   as a side-effect.
        /// </summary>
        /// <param name = "name">The name to use when writing the position to the
        ///   settings</param>
        protected void SavePosition(String name)
        {
            try
            {
                var rectangle =
                    WindowState == FormWindowState.Normal
                        ? DesktopBounds
                        : RestoreBounds;

                var formWindowState =
                    WindowState == FormWindowState.Maximized
                        ? FormWindowState.Maximized
                        : FormWindowState.Normal;

                var position = new WindowPosition(rectangle, formWindowState);


                // Write to the user settings:
                if (Properties.Settings.Default.WindowPositions == null)
                    Properties.Settings.Default.WindowPositions = new WindowPositionList();
                Properties.Settings.Default.WindowPositions[name] = position;
                Properties.Settings.Default.Save();
            }
            catch (ConfigurationException)
            {
                //TODO: howto restore a corrupted config? Properties.Settings.Default.Reset() doesn't work.
            }


        }

        /// <summary>
        ///   Looks up a window in the user settings and returns its saved position.
        /// </summary>
        /// <param name = "name">The name.</param>
        /// <returns>
        ///   The saved window position if it exists. Null if the entry
        ///   doesn't exist, or would not be visible on any screen in the user's
        ///   current display setup.
        /// </returns>
        private static WindowPosition LookupWindowPosition(String name)
        {
            try
            {
                var list = Properties.Settings.Default.WindowPositions;
                if (list == null)
                    return null;

                var position = (WindowPosition)list[name];
                if (position == null || position.Rect.IsEmpty)
                    return null;

                foreach (var screen in Screen.AllScreens)
                {
                    if (screen.WorkingArea.IntersectsWith(position.Rect))
                        return position;
                }
            }
            catch(ConfigurationException)
            {
                //TODO: howto restore a corrupted config? Properties.Settings.Default.Reset() doesn't work.
            }

            return null;
        }
    }
}