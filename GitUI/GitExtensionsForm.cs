using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitUI.Properties;
using Microsoft.WindowsAPICodePack.Taskbar;
using ResourceManager;
using Settings = GitCommands.AppSettings;

namespace GitUI
{
    /// <summary>Base class for a Git Extensions <see cref="Form"/>.
    /// <remarks>
    /// Includes support for font, hotkey, icon, translation, and position restore.
    /// </remarks></summary>
    public class GitExtensionsForm : GitExtensionsFormBase
    {
        internal static Icon ApplicationIcon = GetApplicationIcon(Settings.IconStyle, Settings.IconColor);

        /// <summary>indicates whether the <see cref="Form"/>'s position will be restored</summary>
        private readonly bool _enablePositionRestore;

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> without position restore.</summary>
        public GitExtensionsForm()
            : this(false)
        {
        }

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> indicating position restore.</summary>
        /// <param name="enablePositionRestore">Indicates whether the <see cref="Form"/>'s position
        /// will be restored upon being re-opened.</param>
        public GitExtensionsForm(bool enablePositionRestore)
        {
            _enablePositionRestore = enablePositionRestore;

            Icon = ApplicationIcon;
            FormClosing += GitExtensionsForm_FormClosing;

            var cancelButton = new Button();
            cancelButton.Click += CancelButtonClick;

            CancelButton = cancelButton;
        }

        public virtual void CancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        private void GitExtensionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_enablePositionRestore)
            {
                SavePosition(GetType().Name);
            }

            if (GitCommands.Utils.EnvUtils.RunningOnWindows() && TaskbarManager.IsPlatformSupported)
            {
                try
                {
                    TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                }
                catch (InvalidOperationException)
                {
                }
            }
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
            {
                colorIndex = 0;
            }

            Icon[] icons;
            if (iconStyle.Equals("small", StringComparison.OrdinalIgnoreCase))
            {
                icons = new[]
                {
                    Resources.x_with_arrow,
                    Resources.x_with_arrow_blue,
                    Resources.x_with_arrow_green,
                    Resources.x_with_arrow_lightblue,
                    Resources.x_with_arrow_purple,
                    Resources.x_with_arrow_red,
                    Resources.x_with_arrow_yellow
                };
            }
            else if (iconStyle.Equals("large", StringComparison.OrdinalIgnoreCase))
            {
                icons = new[]
                {
                    Resources.git_extensions_logo_final,
                    Resources.git_extensions_logo_final_blue,
                    Resources.git_extensions_logo_final_green,
                    Resources.git_extensions_logo_final_lightblue,
                    Resources.git_extensions_logo_final_purple,
                    Resources.git_extensions_logo_final_red,
                    Resources.git_extensions_logo_final_yellow
                };
            }
            else if (iconStyle.Equals("cow", StringComparison.OrdinalIgnoreCase))
            {
                icons = new[]
                {
                    Resources.cow_head,
                    Resources.cow_head_blue,
                    Resources.cow_head_green,
                    Resources.cow_head_blue,
                    Resources.cow_head_purple,
                    Resources.cow_head_red,
                    Resources.cow_head_yellow
                };
            }
            else
            {
                icons = new[]
                {
                    Resources.git_extensions_logo_final_mixed,
                    Resources.git_extensions_logo_final_mixed_blue,
                    Resources.git_extensions_logo_final_mixed_green,
                    Resources.git_extensions_logo_final_mixed_lightblue,
                    Resources.git_extensions_logo_final_mixed_purple,
                    Resources.git_extensions_logo_final_mixed_red,
                    Resources.git_extensions_logo_final_mixed_yellow
                };
            }

            Debug.Assert(icons.Length == 7, "icons.Length == 7");
            var appIcon = icons[colorIndex];
            Debug.Assert(appIcon != null, "appIcon != null");
            return appIcon;
        }

        #endregion icon

        /// <summary>Sets <see cref="AutoScaleMode"/>,
        /// restores position, raises the <see cref="Form.Load"/> event,
        /// and .
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            if (_enablePositionRestore)
            {
                RestorePosition(GetType().Name);
            }

            // Should be called after restoring position
            base.OnLoad(e);

            if (!CheckComponent(this))
            {
                OnRuntimeLoad(e);
            }
        }

        /// <summary>Invoked at runtime during the <see cref="OnLoad"/> method.</summary>
        protected virtual void OnRuntimeLoad(EventArgs e)
        {
        }

        private bool _windowCentred;

        /// <summary>
        ///   Restores the position of a form from the user settings. Does
        ///   nothing if there is no entry for the form in the settings, or the
        ///   setting would be invisible on the current display configuration.
        /// </summary>
        /// <param name = "name">The name to use when looking up the position in
        ///   the settings</param>
        private void RestorePosition(string name)
        {
            if (!Visible ||
                WindowState == FormWindowState.Minimized)
            {
                return;
            }

            _windowCentred = StartPosition == FormStartPosition.CenterParent;

            var position = LookupWindowPosition(name);
            if (position == null)
            {
                return;
            }

            float scale = (float)DpiUtil.DpiX / position.DeviceDpi;

            StartPosition = FormStartPosition.Manual;

            if (Owner == null || !_windowCentred)
            {
                Point location = position.Rect.Location;
                location.X = (int)(location.X * scale);
                location.Y = (int)(location.Y * scale);
                Rectangle? rect = FindWindowScreen(location);
                if (rect != null)
                {
                    location.Y = rect.Value.Y;
                }

                DesktopLocation = location;
            }
            else
            {
                // Calculate location for modal form with parent
                Location = new Point(Owner.Left + (Owner.Width / 2) - (Width / 2),
                    Math.Max(0, Owner.Top + (Owner.Height / 2) - (Height / 2)));
            }

            if (WindowState != position.State)
            {
                WindowState = position.State;
            }
        }

        private static Rectangle? FindWindowScreen(Point location)
        {
            SortedDictionary<float, Rectangle> distance = new SortedDictionary<float, Rectangle>();
            foreach (var rect in from screen in Screen.AllScreens
                                 select screen.WorkingArea)
            {
                if (rect.Contains(location) && !distance.ContainsKey(0.0f))
                {
                    return null; // title in screen
                }

                int midPointX = rect.X + (rect.Width / 2);
                int midPointY = rect.Y + (rect.Height / 2);
                float d = (float)Math.Sqrt(((location.X - midPointX) * (location.X - midPointX)) +
                    ((location.Y - midPointY) * (location.Y - midPointY)));
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

        private static WindowPositionList _windowPositionList;

        /// <summary>
        ///   Save the position of a form to the user settings. Hides the window
        ///   as a side-effect.
        /// </summary>
        /// <param name = "name">The name to use when writing the position to the
        ///   settings</param>
        private void SavePosition(string name)
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
                if (_windowPositionList == null)
                {
                    _windowPositionList = WindowPositionList.Load();
                }

                WindowPosition windowPosition = _windowPositionList.Get(name);

                // Don't save location when we center modal form
                if (windowPosition != null && Owner != null && _windowCentred)
                {
                    if (rectangle.Width <= windowPosition.Rect.Width && rectangle.Height <= windowPosition.Rect.Height)
                    {
                        rectangle.Location = windowPosition.Rect.Location;
                    }
                }

                var position = new WindowPosition(rectangle, DpiUtil.DpiX, formWindowState, name);
                _windowPositionList.AddOrUpdate(position);
                _windowPositionList.Save();
            }
            catch (Exception)
            {
                // TODO: howto restore a corrupted config?
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
        private static WindowPosition LookupWindowPosition(string name)
        {
            try
            {
                if (_windowPositionList == null)
                {
                    _windowPositionList = WindowPositionList.Load();
                }

                var position = _windowPositionList?.Get(name);
                if (position == null || position.Rect.IsEmpty)
                {
                    return null;
                }

                if (Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(position.Rect)))
                {
                    return position;
                }
            }
            catch (Exception)
            {
                // TODO: howto restore a corrupted config?
            }

            return null;
        }
    }
}
