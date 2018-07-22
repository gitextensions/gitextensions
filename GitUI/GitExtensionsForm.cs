using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Base class for a Git Extensions <see cref="Form"/>.</summary>
    /// <remarks>Includes support for font, hotkey, icon, translation, and position restore.</remarks>
    public class GitExtensionsForm : GitExtensionsFormBase
    {
        private static WindowPositionList _windowPositionList;

        private bool _needsPositionRestore;
        private bool _needsPositionSave;
        private bool _windowCentred;

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> without position restore.</summary>
        public GitExtensionsForm()
            : this(enablePositionRestore: false)
        {
        }

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> indicating position restore.</summary>
        /// <param name="enablePositionRestore">Indicates whether the <see cref="Form"/>'s position
        /// will be restored upon being re-opened.</param>
        protected GitExtensionsForm(bool enablePositionRestore)
        {
            _needsPositionSave = enablePositionRestore;
            _needsPositionRestore = enablePositionRestore;

            FormClosing += GitExtensionsForm_FormClosing;

            var cancelButton = new Button();
            cancelButton.Click += CancelButtonClick;
            CancelButton = cancelButton;

            void GitExtensionsForm_FormClosing(object sender, FormClosingEventArgs e)
            {
                SavePosition(GetType().Name);
                TaskbarProgress.Clear();
            }
        }

        public virtual void CancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            RestorePosition();

            // Should be called after restoring position
            base.OnLoad(e);

            if (!IsDesignModeActive)
            {
                OnRuntimeLoad(e);
            }
        }

        /// <summary>Invoked at runtime during the <see cref="OnLoad"/> method.</summary>
        /// <remarks>In particular, this method is not invoked when running in a designer.</remarks>
        protected virtual void OnRuntimeLoad(EventArgs e)
        {
        }

        #region Save & restore position

        /// <summary>
        ///   Restores the position of a form from the user settings. Does
        ///   nothing if there is no entry for the form in the settings, or the
        ///   setting would be invisible on the current display configuration.
        /// </summary>
        protected virtual void RestorePosition()
        {
            if (!_needsPositionRestore)
            {
                return;
            }

            if (WindowState == FormWindowState.Minimized)
            {
                return;
            }

            _windowCentred = StartPosition == FormStartPosition.CenterParent;

            var position = LookupWindowPosition(GetType().Name);

            if (position == null)
            {
                return;
            }

            _needsPositionRestore = false;

            if (!Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(position.Rect)))
            {
                if (position.State == FormWindowState.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }

                return;
            }

            SuspendLayout();

            StartPosition = FormStartPosition.Manual;

            if (FormBorderStyle == FormBorderStyle.Sizable ||
                FormBorderStyle == FormBorderStyle.SizableToolWindow)
            {
                Size = DpiUtil.Scale(position.Rect.Size, originalDpi: position.DeviceDpi);
            }

            if (Owner == null || !_windowCentred)
            {
                var location = DpiUtil.Scale(position.Rect.Location, originalDpi: position.DeviceDpi);

                if (FindWindowScreen(location) is Rectangle rect)
                {
                    location.Y = rect.Y;
                }

                DesktopLocation = location;
            }
            else
            {
                // Calculate location for modal form with parent
                Location = new Point(
                    Owner.Left + (Owner.Width / 2) - (Width / 2),
                    Math.Max(0, Owner.Top + (Owner.Height / 2) - (Height / 2)));
            }

            if (WindowState != position.State)
            {
                WindowState = position.State;
            }

            ResumeLayout();

            return;

            Rectangle? FindWindowScreen(Point location)
            {
                var distance = new SortedDictionary<float, Rectangle>();

                foreach (var rect in Screen.AllScreens.Select(screen => screen.WorkingArea))
                {
                    if (rect.Contains(location) && !distance.ContainsKey(0.0f))
                    {
                        return null; // title in screen
                    }

                    int midPointX = rect.X + (rect.Width / 2);
                    int midPointY = rect.Y + (rect.Height / 2);
                    var d = (float)Math.Sqrt(((location.X - midPointX) * (location.X - midPointX)) +
                                               ((location.Y - midPointY) * (location.Y - midPointY)));
                    distance.Add(d, rect);
                }

                return distance.FirstOrDefault().Value;
            }

            WindowPosition LookupWindowPosition(string name)
            {
                try
                {
                    if (_windowPositionList == null)
                    {
                        _windowPositionList = WindowPositionList.Load();
                    }

                    var pos = _windowPositionList?.Get(name);

                    if (pos != null && !pos.Rect.IsEmpty)
                    {
                        return pos;
                    }
                }
                catch
                {
                    // TODO: how to restore a corrupted config?
                }

                return null;
            }
        }

        /// <summary>
        ///   Save the position of a form to the user settings. Hides the window
        ///   as a side-effect.
        /// </summary>
        /// <param name = "name">The name to use when writing the position to the
        ///   settings</param>
        private void SavePosition(string name)
        {
            if (!_needsPositionSave)
            {
                return;
            }

            _needsPositionSave = false;

            try
            {
                var rectangle = WindowState == FormWindowState.Normal
                    ? DesktopBounds
                    : RestoreBounds;

                var formWindowState = WindowState == FormWindowState.Maximized
                    ? FormWindowState.Maximized
                    : FormWindowState.Normal;

                if (_windowPositionList == null)
                {
                    _windowPositionList = WindowPositionList.Load();

                    if (_windowPositionList == null)
                    {
                        return;
                    }
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
            catch
            {
                // TODO: how to restore a corrupted config?
            }
        }

        #endregion
    }
}
