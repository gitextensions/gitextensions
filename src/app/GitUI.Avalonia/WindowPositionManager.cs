using System.Diagnostics;
using System.Drawing;
using Avalonia.Controls;
using Point = System.Drawing.Point;

namespace GitUI;

internal interface IWindowPositionManager
{
    /// <summary>
    /// Retrieves a persisted position for the given <paramref name="form"/>.
    /// </summary>
    /// <param name="form">The form to look the position for.</param>
    /// <returns>The form's persisted position; otherwise <see langword="null"/>.</returns>
    WindowPosition? LoadPosition(Window form);

    /// <summary>
    ///   Save the position of a form to the user settings.
    /// </summary>
    /// <param name="form">The form to save the position for.</param>
    void SavePosition(Window form);
}

internal sealed class WindowPositionManager : IWindowPositionManager
{
    private static WindowPositionList? _windowPositionList;

    /// <summary>
    /// Ensures the window with its new size and location will be accessible to the user.
    /// If all fails, we will display the window at the top left of the first screen or at (0, 0).
    /// </summary>
    /// <param name="calculatedWindowBounds">The intended location of the window.</param>
    /// <param name="workingAreas">The working areas of all available screens.</param>
    /// <returns>A most likely visible location for the window.</returns>
    public static Point FitWindowOnScreen(Rectangle calculatedWindowBounds, IEnumerable<Rectangle> workingAreas)
    {
        foreach (Rectangle screen in workingAreas)
        {
            bool isDisplayed = IsDisplayedOn10Percent(screen, calculatedWindowBounds);
            if (isDisplayed)
            {
                return calculatedWindowBounds.Location;
            }
        }

        return workingAreas.FirstOrDefault(screen => !screen.IsEmpty).Location;
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
                Debug.WriteLine($"{screen} contains {p} (L, T)");
                return true;
            }
        }

        if (screen.Contains(new Point(window.Left + (window.Width / 2), window.Top)))
        {
            p = new Point(window.Left + (window.Width / 2) - requireWidth, window.Top + requiredHeight);
            bool middleTop = screen.Contains(p);
            if (middleTop)
            {
                Debug.WriteLine($"{screen} contains {p} (W/2-, T)");
                return true;
            }

            p = new Point(window.Left + (window.Width / 2) + requireWidth, window.Top + requiredHeight);
            middleTop = screen.Contains(p);
            if (middleTop)
            {
                Debug.WriteLine($"{screen} contains {p} (W/2+, T)");
                return true;
            }
        }

        if (screen.Contains(new Point(window.Left, window.Top + (window.Height / 2))))
        {
            p = new Point(window.Left + requireWidth, window.Top + (window.Height / 2) - requiredHeight);
            bool middleTop = screen.Contains(p);
            if (middleTop)
            {
                Debug.WriteLine($"{screen} contains {p} (L, H/2-)");
                return true;
            }

            p = new Point(window.Left + requireWidth, window.Top + (window.Height / 2) + requiredHeight);
            middleTop = screen.Contains(p);
            if (middleTop)
            {
                Debug.WriteLine($"{screen} contains {p} (L, H/2+)");
                return true;
            }
        }

        if (screen.Contains(new Point(window.Right, window.Top)))
        {
            p = new Point(window.Right - requireWidth, window.Top + requiredHeight);
            bool rightTop = screen.Contains(p);
            if (rightTop)
            {
                Debug.WriteLine($"{screen} contains {p} (R, T)");
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
    public WindowPosition? LoadPosition(Window form)
    {
        try
        {
            _windowPositionList ??= WindowPositionList.Load();

            WindowPosition? pos = _windowPositionList?.Get(form.GetType().Name);
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
    ///   Save the position of a form to the user settings.
    /// </summary>
    /// <param name="form">The form to save the position for.</param>
    public void SavePosition(Window form)
    {
        try
        {
            Rectangle rectangle = form is GitExtensionsForm gitExtensionsForm
                ? gitExtensionsForm.RestoreBounds
                : GetBounds(form);

            WindowState formWindowState = form.WindowState == WindowState.Maximized
                ? WindowState.Maximized
                : WindowState.Normal;

            if (_windowPositionList is null)
            {
                _windowPositionList = WindowPositionList.Load();
                if (_windowPositionList is null)
                {
                    return;
                }
            }

            string name = form.GetType().Name;

            WindowPosition? windowPosition = _windowPositionList.Get(name);
            bool windowCentred = form.WindowStartupLocation == WindowStartupLocation.CenterOwner;

            // Don't save location when we center modal form
            if (windowPosition is not null && form.Owner is not null && windowCentred)
            {
                if (rectangle.Width <= windowPosition.Rect.Width && rectangle.Height <= windowPosition.Rect.Height)
                {
                    rectangle.Location = windowPosition.Rect.Location;
                }
            }

            // Wayland compositors own top-level placement. Preserve the last meaningful
            // location while still saving the window's size and state.
            if (!SupportsProgrammaticPositioning && windowPosition is not null)
            {
                rectangle.Location = windowPosition.Rect.Location;
            }

            WindowPosition position = new(rectangle, GetDeviceDpi(form), formWindowState, name);
            _windowPositionList.AddOrUpdate(position);
            _windowPositionList.Save();
        }
        catch
        {
            // TODO: how to restore a corrupted config?
        }
    }

    internal static bool SupportsProgrammaticPositioning
        => SupportsProgrammaticPositioningFor(
            OperatingSystem.IsLinux(),
            Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));

    // parity-scaffolding: Makes the platform decision deterministic in the cross-platform base-layer tests.
    internal static bool SupportsProgrammaticPositioningFor(bool isLinux, string? waylandDisplay)
        => !isLinux || string.IsNullOrWhiteSpace(waylandDisplay);

    internal static int GetDeviceDpi(Window form)
        => (int)Math.Round(96 * form.RenderScaling);

    internal static Rectangle GetBounds(Window form)
    {
        double scaling = form.RenderScaling;
        return new Rectangle(
            form.Position.X,
            form.Position.Y,
            (int)Math.Round(form.Bounds.Width * scaling),
            (int)Math.Round(form.Bounds.Height * scaling));
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
