using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using JetBrains.Annotations;

namespace GitUI
{
    internal interface IWindowPositionManager
    {
        /// <summary>
        /// Retrieves a persisted position for the given <paramref name="form"/>.
        /// </summary>
        /// <param name="form">The form to look the position for.</param>
        /// <returns>The form's persisted position; otherwise <see langword="null"/>.</returns>
        WindowPosition LoadPosition(Form form);

        /// <summary>
        ///   Save the position of a form to the user settings. Hides the window
        ///   as a side-effect.
        /// </summary>
        /// <param name="form">The form to save the position for.</param>
        void SavePosition(Form form);
    }

    internal sealed class WindowPositionManager : IWindowPositionManager
    {
        private static WindowPositionList _windowPositionList;

        /// <summary>
        /// Determines which screen a given point belongs to.
        /// </summary>
        [Pure]
        public static Rectangle? FindWindowScreen(Point location, IEnumerable<Rectangle> desktopWorkingArea)
        {
            var distance = new SortedDictionary<float, Rectangle>();

            foreach (var rect in desktopWorkingArea)
            {
                if (rect.Contains(location) && !distance.ContainsKey(0.0f))
                {
                    return null; // title in screen
                }

                int midPointX = rect.X + (rect.Width / 2);
                int midPointY = rect.Y + (rect.Height / 2);
                var d = (float)Math.Sqrt(((location.X - midPointX) * (location.X - midPointX)) +
                                         ((location.Y - midPointY) * (location.Y - midPointY)));

                // In a very unlikely scenario where a user has several monitors, which are arranged in a particular way
                // and a form's window happens to be in exact middle between monitors, if we don't check - it will crash
                if (!distance.ContainsKey(d))
                {
                    distance.Add(d, rect);
                }
            }

            return distance.FirstOrDefault().Value;
        }

        /// <summary>
        /// Retrieves a persisted position for the given <paramref name="form"/>.
        /// </summary>
        /// <param name="form">The form to look the position for.</param>
        /// <returns>The form's persisted position; otherwise <see langword="null"/>.</returns>
        public WindowPosition LoadPosition(Form form)
        {
            try
            {
                if (_windowPositionList == null)
                {
                    _windowPositionList = WindowPositionList.Load();
                }

                var pos = _windowPositionList?.Get(form.GetType().Name);
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

        /// <summary>
        ///   Save the position of a form to the user settings. Hides the window
        ///   as a side-effect.
        /// </summary>
        /// <param name="form">The form to save the position for.</param>
        public void SavePosition(Form form)
        {
            try
            {
                var rectangle = form.WindowState == FormWindowState.Normal
                    ? form.DesktopBounds
                    : form.RestoreBounds;

                var formWindowState = form.WindowState == FormWindowState.Maximized
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

                var name = form.GetType().Name;

                WindowPosition windowPosition = _windowPositionList.Get(name);
                var windowCentred = form.StartPosition == FormStartPosition.CenterParent;

                // Don't save location when we center modal form
                if (windowPosition != null && form.Owner != null && windowCentred)
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
    }
}