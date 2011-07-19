using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitUI.Properties;
using Microsoft.WindowsAPICodePack.Taskbar;
using ResourceManager.Translation;
using Settings = GitCommands.Settings;
using System.Collections.Generic;

namespace GitUI
{
    public class GitExtensionsForm : Form
    {
        private static Icon ApplicationIcon = GetApplicationIcon(Settings.IconStyle, Settings.IconColor);

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
        }

        #region Hotkeys

        /// <summary>Gets or sets a value that specifies if the hotkeys are used</summary>
        protected bool HotkeysEnabled { get; set; }

        /// <summary>Gets or sets the hotkeys</summary>
        protected IEnumerable<Hotkey.HotkeyCommand> Hotkeys { get; set; }

        /// <summary>Overridden: Checks if a hotkey wants to handle the key before letting the message propagate</summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (HotkeysEnabled && this.Hotkeys != null)
                foreach (var hotkey in this.Hotkeys)
                {
                    if (hotkey.KeyData == keyData)
                    {
                        return ExecuteCommand(hotkey.CommandCode);
                    }
                }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Override this method to handle form specific Hotkey commands
        /// This base method calls script-hotkeys
        /// </summary>
        /// <param name="command"></param>
        protected virtual bool ExecuteCommand(int command)
        {
            ExecuteScriptCommand(command, Keys.None);
            return true;
        }
        protected virtual bool ExecuteScriptCommand(int command, Keys keyData)
        {
            var curScripts = GitUI.Script.ScriptManager.GetScripts();

            foreach (GitUI.Script.ScriptInfo s in curScripts)
            {
                if (s.HotkeyCommandIdentifier == command)
                    GitUI.Script.ScriptRunner.RunScript(s.Name, null);
            }
            return true;
        }

        #endregion

        private void SetFont()
        {
            Font = SystemFonts.MessageBoxFont;
        }

        #region icon

        protected void RotateApplicationIcon()
        {
            ApplicationIcon = GetApplicationIcon(Settings.IconStyle, Settings.IconColor);
            Icon = ApplicationIcon;
        }

        protected static Icon GetApplicationIcon(string iconStyle, string iconColor)
        {
            var randomIcon = -1;
            if (iconColor.Equals("random"))
                randomIcon = new Random(DateTime.Now.Millisecond).Next(6);

            if (iconStyle.Equals("small", StringComparison.OrdinalIgnoreCase))
            {
                if (iconColor.Equals("default") || randomIcon == 0)
                    return Resources.x_with_arrow;
                if (iconColor.Equals("blue") || randomIcon == 1)
                    return Resources.x_with_arrow_blue;
                if (iconColor.Equals("green") || randomIcon == 3)
                    return Resources.x_with_arrow_green;
                if (iconColor.Equals("lightblue") || randomIcon == 1)
                    return Resources.x_with_arrow_lightblue;
                if (iconColor.Equals("purple") || randomIcon == 2)
                    return Resources.x_with_arrow_purple;
                if (iconColor.Equals("red") || randomIcon == 4)
                    return Resources.x_with_arrow_red;
                if (iconColor.Equals("yellow") || randomIcon == 5)
                    return Resources.x_with_arrow_yellow;
            }
            else
                if (iconStyle.Equals("large", StringComparison.OrdinalIgnoreCase))
                {
                    if (iconColor.Equals("default") || randomIcon == 0)
                        return Resources.git_extensions_logo_final;
                    if (iconColor.Equals("blue") || randomIcon == 1)
                        return Resources.git_extensions_logo_final_blue;
                    if (iconColor.Equals("green") || randomIcon == 3)
                        return Resources.git_extensions_logo_final_green;
                    if (iconColor.Equals("lightblue") || randomIcon == 1)
                        return Resources.git_extensions_logo_final_lightblue;
                    if (iconColor.Equals("purple") || randomIcon == 2)
                        return Resources.git_extensions_logo_final_purple;
                    if (iconColor.Equals("red") || randomIcon == 4)
                        return Resources.git_extensions_logo_final_mixed_red;
                    if (iconColor.Equals("yellow") || randomIcon == 5)
                        return Resources.git_extensions_logo_final_mixed_yellow;
                }
                else
                    if (iconStyle.Equals("cow", StringComparison.OrdinalIgnoreCase))
                    {
                        if (iconColor.Equals("default") || randomIcon == 0)
                            return Resources.cow_head;
                        if (iconColor.Equals("blue") || randomIcon == 1)
                            return Resources.cow_head_blue;
                        if (iconColor.Equals("green") || randomIcon == 3)
                            return Resources.cow_head_green;
                        if (iconColor.Equals("lightblue") || randomIcon == 1)
                            return Resources.cow_head_blue;
                        if (iconColor.Equals("purple") || randomIcon == 2)
                            return Resources.cow_head_purple;
                        if (iconColor.Equals("red") || randomIcon == 4)
                            return Resources.cow_head_red;
                        if (iconColor.Equals("yellow") || randomIcon == 5)
                            return Resources.cow_head_yellow;
                    }
                    else
                    {
                        if (iconColor.Equals("default") || randomIcon == 0)
                            return Resources.git_extensions_logo_final_mixed;
                        if (iconColor.Equals("blue") || randomIcon == 1)
                            return Resources.git_extensions_logo_final_mixed_blue;
                        if (iconColor.Equals("green") || randomIcon == 3)
                            return Resources.git_extensions_logo_final_mixed_green;
                        if (iconColor.Equals("lightblue") || randomIcon == 1)
                            return Resources.git_extensions_logo_final_mixed_lightblue;
                        if (iconColor.Equals("purple") || randomIcon == 2)
                            return Resources.git_extensions_logo_final_mixed_purple;
                        if (iconColor.Equals("red") || randomIcon == 4)
                            return Resources.git_extensions_logo_final_mixed_red;
                        if (iconColor.Equals("yellow") || randomIcon == 5)
                            return Resources.git_extensions_logo_final_mixed_yellow;
                    }

            return Resources.git_extensions_logo_final_mixed;
        }
        #endregion

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
            {
                try
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                }
                catch (InvalidOperationException) { }
            }
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

                if (Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(position.Rect)))
                {
                    return position;
                }
            }
            catch (ConfigurationException)
            {
                //TODO: howto restore a corrupted config? Properties.Settings.Default.Reset() doesn't work.
            }

            return null;
        }
    }
}