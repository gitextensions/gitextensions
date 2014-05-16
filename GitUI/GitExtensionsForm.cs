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
using ResourceManager;
using Settings = GitCommands.AppSettings;
using System.Collections.Generic;

namespace GitUI
{
    /// <summary>Base class for a Git Extensions <see cref="Form"/>.
    /// <remarks>
    /// Includes support for font, hotkey, icon, translation, and position restore.
    /// </remarks></summary>
    public class GitExtensionsForm : Form, ITranslate
    {
        internal static Icon ApplicationIcon = GetApplicationIcon(Settings.IconStyle, Settings.IconColor);

        /// <summary>indicates whether the <see cref="Form"/> has been translated</summary>
        bool _translated;
        /// <summary>indicates whether the <see cref="Form"/>'s position will be restored</summary>
        bool _enablePositionRestore;

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> without position restore.</summary>
        public GitExtensionsForm()
            : this(false) { }

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> indicating position restore.</summary>
        /// <param name="enablePositionRestore">Indicates whether the <see cref="Form"/>'s position
        /// will be restored upon being re-opened.</param>
        public GitExtensionsForm(bool enablePositionRestore)
        {
            _enablePositionRestore = enablePositionRestore;

            Icon = ApplicationIcon;
            SetFont();

            ShowInTaskbar = Application.OpenForms.Count <= 0 || (Application.OpenForms.Count == 1 && Application.OpenForms[0] is FormSplash);

            var cancelButton = new Button();
            cancelButton.Click += CancelButtonClick;

            CancelButton = cancelButton;

            Load += GitExtensionsFormLoad;
            FormClosing += GitExtensionsForm_FormClosing;
        }

        void GitExtensionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_enablePositionRestore)
                SavePosition(GetType().Name);

#if !__MonoCS__
            if (GitCommands.Utils.EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
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
            if (HotkeysEnabled && Hotkeys != null)
                foreach (var hotkey in Hotkeys)
                {
                    if (hotkey != null && hotkey.KeyData == keyData)
                    {
                        return ExecuteCommand(hotkey.CommandCode);
                    }
                }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected Keys GetShortcutKeys(int commandCode)
        {
            var hotkey = GetHotkeyCommand(commandCode);
            return hotkey == null ? Keys.None : hotkey.KeyData;
        }

        protected Hotkey.HotkeyCommand GetHotkeyCommand(int commandCode)
        {
            if (Hotkeys == null)
                return null;

            return Hotkeys.FirstOrDefault(h => h.CommandCode == commandCode);
        }

        /// <summary>Override this method to handle form-specific Hotkey commands.</summary>
        protected virtual bool ExecuteCommand(int command)
        {
            return false;
        }

        #endregion

        private void SetFont()
        {
            Font = Settings.Font;
        }

        #region icon

        protected void RotateApplicationIcon()
        {
            ApplicationIcon = GetApplicationIcon(Settings.IconStyle, Settings.IconColor);
            Icon = ApplicationIcon;
        }

        /// <summary>Specifies a Git Extensions' color index.</summary>
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
                    return (ColorIndex)new Random(DateTime.Now.Millisecond).Next(7);
            }
            return ColorIndex.Unknown;
        }

        public static Icon GetApplicationIcon(string iconStyle, string iconColor)
        {
            var colorIndex = (int)GetColorIndexByName(iconColor);
            if (colorIndex == (int)ColorIndex.Unknown)
                colorIndex = 0;

            Icon appIcon;
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
                appIcon = icons[colorIndex];
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
                appIcon = icons[colorIndex];
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
                appIcon = icons[colorIndex];
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
                appIcon = icons[colorIndex];
            }
            Debug.Assert(appIcon != null);
            return appIcon;
        }

        #endregion icon

        /// <summary>Indicates whether this is a valid <see cref="IComponent"/> running in design mode.</summary>
        static bool CheckComponent(object value)
        {
            var component = value as IComponent;
            if (component == null)
                return false;

            var site = component.Site;
            return (site != null) && site.DesignMode;
        }

        /// <summary>Invoked at runtime during the <see cref="OnLoad"/> method.</summary>
        protected virtual void OnRuntimeLoad(EventArgs e)
        {

        }

        /// <summary>Sets <see cref="AutoScaleMode"/>, 
        /// restores position, raises the <see cref="Form.Load"/> event,
        /// and .
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            AutoScaleMode = Settings.EnableAutoScale
                ? AutoScaleMode.Dpi
                : AutoScaleMode.None;

            if (_enablePositionRestore)
                RestorePosition(GetType().Name);

            base.OnLoad(e);

            if (!CheckComponent(this))
                OnRuntimeLoad(e);
        }

        private void GitExtensionsFormLoad(object sender, EventArgs e)
        {
            // find out if the value is a component and is currently in design mode
            var isComponentInDesignMode = CheckComponent(this);

            if (!_translated && !isComponentInDesignMode)
                throw new Exception("The control " + GetType().Name +
                                    " is not translated in the constructor. You need to call Translate() right after InitializeComponent().");
        }

        /// <summary>Translates the <see cref="Form"/>'s fields and properties, including child controls.</summary>
        protected void Translate()
        {
            Translator.Translate(this, Settings.CurrentTranslation);
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
        private void RestorePosition(String name)
        {
            if (!Visible ||
                WindowState == FormWindowState.Minimized)
                return;

            _windowCentred = (StartPosition == FormStartPosition.CenterParent);

            var position = LookupWindowPosition(name);

            if (position == null)
                return;

            StartPosition = FormStartPosition.Manual;
            if (FormBorderStyle == FormBorderStyle.Sizable ||
                FormBorderStyle == FormBorderStyle.SizableToolWindow)
                Size = position.Rect.Size;
            if (Owner == null || !_windowCentred)
            {
                Point location = position.Rect.Location;
                Rectangle? rect = FindWindowScreen(location);
                if (rect != null)
                    location.Y = rect.Value.Y;
                DesktopLocation = location;
            }
            else
            {
                // Calculate location for modal form with parent
                Location = new Point(Owner.Left + Owner.Width / 2 - Width / 2,
                    Math.Max(0, Owner.Top + Owner.Height / 2 - Height / 2));
            }
            WindowState = position.State;
        }

        static Rectangle? FindWindowScreen(Point location)
        {
            SortedDictionary<float, Rectangle> distance = new SortedDictionary<float, Rectangle>();
            foreach (var rect in (from screen in Screen.AllScreens
                                  select screen.WorkingArea))
            {
                if (rect.Contains(location) && !distance.ContainsKey(0.0f))
                    return null; // title in screen

                int midPointX = (rect.X + rect.Width / 2);
                int midPointY = (rect.Y + rect.Height / 2);
                float d = (float)Math.Sqrt((location.X - midPointX) * (location.X - midPointX) +
                    (location.Y - midPointY) * (location.Y - midPointY));
                distance.Add(d, rect);
            }
            if (distance.Count > 0)
            {
                return distance.First().Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///   Save the position of a form to the user settings. Hides the window
        ///   as a side-effect.
        /// </summary>
        /// <param name = "name">The name to use when writing the position to the
        ///   settings</param>
        private void SavePosition(String name)
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

        public virtual void AddTranslationItems(ITranslation translation)
        {
            TranslationUtils.AddTranslationItemsFromFields(Name, this, translation);
        }

        public virtual void TranslateItems(ITranslation translation)
        {
            TranslationUtils.TranslateItemsFromFields(Name, this, translation);
        }
    }
}
