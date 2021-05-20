using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using GitExtUtils.GitUI;

namespace GitUI
{
    internal interface IWindowPositionManager
    {
        /// <summary>
        /// Retrieves a persisted position for the given <paramref name="form"/>.
        /// </summary>
        /// <param name="form">The form to look the position for.</param>
        /// <returns>The form's persisted position; otherwise <see langword="null"/>.</returns>
        WindowPosition? LoadPosition(Form form);

        /// <summary>
        ///   Save the position of a form to the user settings. Hides the window
        ///   as a side-effect.
        /// </summary>
        /// <param name="form">The form to save the position for.</param>
        void SavePosition(Form form);
    }

    internal sealed class WindowPositionManager : IWindowPositionManager
    {
        private static WindowPositionList? _windowPositionList;

        public static Point FitWindowOnScreen(Rectangle calculatedWindowBounds, IEnumerable<Rectangle> workingArea)
        {
            // Ensure the window with its new size and location will be accessible to the user
            // If all fails, we'll display the window the (0, 0)
            Point location = Point.Empty;
            foreach (Rectangle screen in workingArea)
            {
                bool isDisplayed = IsDisplayedOn10Percent(screen, calculatedWindowBounds);
                if (isDisplayed)
                {
                    location = calculatedWindowBounds.Location;
                    break;
                }
            }

            return location;
        }

        private static bool IsDisplayedOn10Percent(Rectangle screen, Rectangle window)
        {
            if (screen.IsEmpty || window.IsEmpty)
            {
                return false;
            }

            // We insist that any window to cover at least 10% of a screen realestate both horizontally and vertically
            // However, check if the window is smaller than the minimum presence requirement.
            // If so, adjust the requirements to the size of the window.
            const float MinimumScreenPresence = 0.1f; // 10%
            int requiredHeight = Math.Min((int)(screen.Height * MinimumScreenPresence), window.Height);
            int requireWidth = Math.Min((int)(screen.Width * MinimumScreenPresence), window.Width);

            Point p;
            if (screen.Contains(window.Location))
            {
                p = new Point(window.Left + requireWidth, window.Top + requiredHeight);
                bool leftTop = screen.Contains(p);
                if (leftTop)
                {
                    Debug.WriteLine($"{screen.ToString()} contains {p} (L, T)");
                    return true;
                }
            }

            if (screen.Contains(new Point(window.Left + (window.Width / 2), window.Top)))
            {
                p = new Point(window.Left + (window.Width / 2) - requireWidth, window.Top + requiredHeight);
                bool middleTop = screen.Contains(p);
                if (middleTop)
                {
                    Debug.WriteLine($"{screen.ToString()} contains {p} (W/2-, T)");
                    return true;
                }

                p = new Point(window.Left + (window.Width / 2) + requireWidth, window.Top + requiredHeight);
                middleTop = screen.Contains(p);
                if (middleTop)
                {
                    Debug.WriteLine($"{screen.ToString()} contains {p} (W/2+, T)");
                    return true;
                }
            }

            if (screen.Contains(new Point(window.Right, window.Top)))
            {
                p = new Point(window.Right - requireWidth, window.Top + requiredHeight);
                bool rightTop = screen.Contains(p);
                if (rightTop)
                {
                    Debug.WriteLine($"{screen.ToString()} contains {p} (R, T)");
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Retrieves a persisted position for the given <paramref name="form"/>.
        /// </summary>
        /// <param name="form">The form to look the position for.</param>
        /// <returns>The form's persisted position; otherwise <see langword="null"/>.</returns>
        public WindowPosition? LoadPosition(Form form)
        {
            try
            {
                _windowPositionList ??= WindowPositionList.Load();

                var pos = _windowPositionList?.Get(form.GetType().Name);
                if (pos is not null && !pos.Rect.IsEmpty)
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

                if (_windowPositionList is null)
                {
                    _windowPositionList = WindowPositionList.Load();
                    if (_windowPositionList is null)
                    {
                        return;
                    }
                }

                var name = form.GetType().Name;

                WindowPosition? windowPosition = _windowPositionList.Get(name);
                var windowCentred = form.StartPosition == FormStartPosition.CenterParent;

                // Don't save location when we center modal form
                if (windowPosition is not null && form.Owner is not null && windowCentred)
                {
                    if (rectangle.Width <= windowPosition.Rect.Width && rectangle.Height <= windowPosition.Rect.Height)
                    {
                        rectangle.Location = windowPosition.Rect.Location;
                    }
                }

                WindowPosition position = new(rectangle, DpiUtil.DpiX, formWindowState, name);
                _windowPositionList.AddOrUpdate(position);
                _windowPositionList.Save();
            }
            catch
            {
                // TODO: how to restore a corrupted config?
            }
        }

        internal TestAccessor GetTestAccessor()
        {
            return new TestAccessor(this);
        }

        internal readonly struct TestAccessor
        {
            private readonly WindowPositionManager _windowPositionManager;

            internal TestAccessor(WindowPositionManager windowPositionManager)
            {
                _windowPositionManager = windowPositionManager;
            }

            public static bool IsDisplayedOn10Percent(Rectangle screen, Rectangle window)
                => WindowPositionManager.IsDisplayedOn10Percent(screen, window);
        }
    }
}
