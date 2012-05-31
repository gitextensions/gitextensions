using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitUI.Properties;
#if !__MonoCS__
using Microsoft.WindowsAPICodePack.Taskbar;
#endif
using ResourceManager.Translation;
using Settings = GitCommands.Settings;
using System.Collections.Generic;

namespace GitUI
{
    public class GitExtensionsForm : Form, ITranslate
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
            FormClosing += GitExtensionsForm_FormClosing;
        }

        void GitExtensionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
#if !__MonoCS__
            if (TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                }
                catch (InvalidOperationException) { }
            }
#endif
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
                    if (hotkey != null && hotkey.KeyData == keyData)
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
        
        protected enum ColorIndex
        {
            Default,
            Blue,
            Green,
            LightBlue,
            Purple,
            Red,
            Yellow,
            Unknown = -1
        }

        protected static ColorIndex GetColorIndexByName(string color)
        {
            switch (color)
            {
                case "default":
                    return ColorIndex.Default;
                case "blue":
                    return ColorIndex.Blue;
                case "green":
                    return ColorIndex.Green;
                case "lightblue":
                    return ColorIndex.LightBlue;
                case "purple":
                    return ColorIndex.Purple;
                case "red":
                    return ColorIndex.Red;
                case "yellow":
                    return ColorIndex.Yellow;
                case "random":
                    return (ColorIndex)new Random(DateTime.Now.Millisecond).Next(6);
            }
            return ColorIndex.Unknown;
        }

        protected static Icon GetApplicationIcon(string iconStyle, string iconColor)
        {
            var colorIndex = (int)GetColorIndexByName(iconColor);
            if (colorIndex == (int) ColorIndex.Unknown)
                colorIndex = 0;

            if (iconStyle.Equals("small", StringComparison.OrdinalIgnoreCase))
            {
                Icon[] icons = {
                                    Resources.x_with_arrow,
                                    Resources.x_with_arrow_blue,
                                    Resources.x_with_arrow_green,
                                    Resources.x_with_arrow_lightblue,
                                    Resources.x_with_arrow_purple,
                                    Resources.x_with_arrow_red,
                                    Resources.x_with_arrow_yellow
                                };
                Debug.Assert(icons.Length == 7);
                return icons[colorIndex];
            }
            else if (iconStyle.Equals("large", StringComparison.OrdinalIgnoreCase))
            {
                Icon[] icons = {
                                    Resources.git_extensions_logo_final,
                                    Resources.git_extensions_logo_final_blue,
                                    Resources.git_extensions_logo_final_green,
                                    Resources.git_extensions_logo_final_lightblue,
                                    Resources.git_extensions_logo_final_purple,
                                    Resources.git_extensions_logo_final_red,
                                    Resources.git_extensions_logo_final_yellow
                                };
                Debug.Assert(icons.Length == 7);
                return icons[colorIndex];
            }
            else if (iconStyle.Equals("cow", StringComparison.OrdinalIgnoreCase))
            {
                Icon[] icons = {
                                    Resources.cow_head,
                                    Resources.cow_head_blue,
                                    Resources.cow_head_green,
                                    Resources.cow_head_blue,
                                    Resources.cow_head_purple,
                                    Resources.cow_head_red,
                                    Resources.cow_head_yellow
                                };
                Debug.Assert(icons.Length == 7);
                return icons[colorIndex];
            }
            else
            {
                Icon[] icons = {
                                    Resources.git_extensions_logo_final_mixed,
                                    Resources.git_extensions_logo_final_mixed_blue,
                                    Resources.git_extensions_logo_final_mixed_green,
                                    Resources.git_extensions_logo_final_mixed_lightblue,
                                    Resources.git_extensions_logo_final_mixed_purple,
                                    Resources.git_extensions_logo_final_mixed_red,
                                    Resources.git_extensions_logo_final_mixed_yellow
                                };
                Debug.Assert(icons.Length == 7);
                return icons[colorIndex];
            }
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
            Translator.Translate(this, Settings.Translation);
            _translated = true;
        }

        public virtual void CancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        private bool _windowCentred;

        /// <summary>
        ///   Restores the position of a form from the user settings. Does
        ///   nothing if there is no entry for the form in the settings, or the
        ///   setting would be invisible on the current display configuration.
        /// </summary>
        /// <param name = "name">The name to use when looking up the position in
        ///   the settings</param>
        protected void RestorePosition(String name)
        {
            if (!this.Visible || 
                WindowState == FormWindowState.Minimized)
                return;

            _windowCentred = (StartPosition == FormStartPosition.CenterParent);

            var position = LookupWindowPosition(name);

            if (position == null)
                return;

            StartPosition = FormStartPosition.Manual;
            Size = position.Rect.Size;
            if (Owner == null || !_windowCentred)
                Location = position.Rect.Location;
            else
            {
                // Calculate location for modal form with parent
                Location = new Point(Owner.Left + Owner.Width / 2 - Width / 2, 
                    Owner.Top + Owner.Height / 2 - Height / 2);
            }
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

                // Write to the user settings:
                if (Properties.Settings.Default.WindowPositions == null)
                    Properties.Settings.Default.WindowPositions = new WindowPositionList();
                WindowPosition windowPosition = (WindowPosition)Properties.Settings.Default.WindowPositions[name];
                // Don't save location when we center modal form
                if (windowPosition != null && Owner != null && _windowCentred)
                {
                    if (rectangle.Width <= windowPosition.Rect.Width && rectangle.Height <= windowPosition.Rect.Height)
                        rectangle.Location = windowPosition.Rect.Location;
                }

                var position = new WindowPosition(rectangle, formWindowState);
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

        public virtual void AddTranslationItems(Translation translation)
        {
            if (!string.IsNullOrEmpty(Text))
                translation.AddTranslationItem(Name, "$this", "Text", Text);
            TranslationUtl.AddTranslationItemsFromFields(Name, this, translation);
        }

        public virtual void TranslateItems(Translation translation)
        {
            Text = translation.TranslateItem(Name, "$this", "Text", Text);
            TranslationUtl.TranslateItemsFromFields(Name, this, translation);
        }

    }
}