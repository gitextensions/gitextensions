using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using ResourceManager;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace GitUI;

// NOTE do not make this class abstract as it breaks the Avalonia designer

/// <summary>Base class for a Git Extensions <see cref="Window"/>.</summary>
/// <remarks>Includes support for font, hotkey, icon, translation, and position restore.</remarks>
public class GitExtensionsForm : GitExtensionsFormBase
{
    private IWindowPositionManager _windowPositionManager = new WindowPositionManager();
    private Func<IReadOnlyList<Rectangle>> _getScreensWorkingArea;
    private Func<bool> _supportsProgrammaticPositioning;
    private bool _needsPositionRestore;
    private Rectangle _restoreBounds;
    private bool _needsPositionSave;

    /// <summary>Creates a new <see cref="GitExtensionsForm"/> without position restore.</summary>
    public GitExtensionsForm()
        : this(enablePositionRestore: false)
    {
    }

    /// <summary>Creates a new <see cref="GitExtensionsForm"/> indicating position restore.</summary>
    /// <param name="enablePositionRestore">Indicates whether the <see cref="Window"/>'s position
    /// will be restored upon being re-opened.</param>
    protected GitExtensionsForm(bool enablePositionRestore)
    {
        _getScreensWorkingArea = GetScreensWorkingArea;
        _supportsProgrammaticPositioning = () => WindowPositionManager.SupportsProgrammaticPositioning;
        _needsPositionSave = enablePositionRestore;
        _needsPositionRestore = enablePositionRestore;

        PositionChanged += (_, _) => CaptureRestoreBounds();

        Button cancelButton = new();
        cancelButton.Click += CancelButtonClick;
        CancelButton = cancelButton;
    }

    internal Rectangle RestoreBounds
        => _restoreBounds.IsEmpty ? WindowPositionManager.GetBounds(this) : _restoreBounds;

    public virtual void CancelButtonClick(object? sender, EventArgs e)
    {
        Close();
    }

    protected override void OnOpened(EventArgs e)
    {
        RestorePosition();

        // Should be called after restoring position
        base.OnOpened(e);
        CaptureRestoreBounds();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (_needsPositionSave)
        {
            _needsPositionSave = false;
            _windowPositionManager.SavePosition(this);
        }

        base.OnClosing(e);
    }

    /// <summary>Invoked at runtime during the <see cref="OnOpened"/> method.</summary>
    /// <remarks>In particular, this method is not invoked when running in a designer.</remarks>
    protected override void OnRuntimeLoad(EventArgs e)
    {
    }

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

        if (WindowState == WindowState.Minimized)
        {
            // TODO: do we still need to assert when restored it is shown on the correct monitor?
            return;
        }

        WindowPosition? position = _windowPositionManager.LoadPosition(this);
        if (position is null)
        {
            return;
        }

        _needsPositionRestore = false;

        IReadOnlyList<Rectangle> workingAreas = _getScreensWorkingArea();
        bool supportsProgrammaticPositioning = _supportsProgrammaticPositioning();
        if (supportsProgrammaticPositioning
            && !workingAreas.Any(screen => screen.IntersectsWith(position.Rect)))
        {
            if (position.State == WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }

            return;
        }

        bool windowCentred = WindowStartupLocation == WindowStartupLocation.CenterOwner;
        double savedScaling = Math.Max(position.DeviceDpi / 96d, 1d);

        if (CanResize)
        {
            Width = position.Rect.Width / savedScaling;
            Height = position.Rect.Height / savedScaling;
        }

        if (!supportsProgrammaticPositioning)
        {
            // Wayland deliberately gives the compositor final placement authority. Retain
            // persisted size/state and ask it to center owned dialogs or otherwise center
            // on a screen instead of presenting a false coordinate-restore guarantee.
            WindowStartupLocation = Owner is not null && windowCentred
                ? WindowStartupLocation.CenterOwner
                : WindowStartupLocation.CenterScreen;
        }
        else
        {
            double currentScaling = Math.Max(RenderScaling, 1d);
            int width = (int)Math.Round(Width * currentScaling);
            int height = (int)Math.Round(Height * currentScaling);
            Point calculatedLocation;

            if (Owner is null || !windowCentred)
            {
                double scaleChange = currentScaling / savedScaling;
                calculatedLocation = new Point(
                    (int)Math.Round(position.Rect.X * scaleChange),
                    (int)Math.Round(position.Rect.Y * scaleChange));
            }
            else
            {
                // Calculate location for modal form with parent
                Rectangle ownerBounds = Owner is Window ownerWindow
                    ? WindowPositionManager.GetBounds(ownerWindow)
                    : position.Rect;
                calculatedLocation = new Point(
                    ownerBounds.Left + (ownerBounds.Width / 2) - (width / 2),
                    ownerBounds.Top + (ownerBounds.Height / 2) - (height / 2));
            }

            Point location = WindowPositionManager.FitWindowOnScreen(
                new Rectangle(calculatedLocation, new Size(width, height)),
                workingAreas);
            WindowStartupLocation = WindowStartupLocation.Manual;
            Position = new PixelPoint(location.X, location.Y);
        }

        if (WindowState != position.State)
        {
            WindowState = position.State;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BoundsProperty || change.Property == WindowStateProperty)
        {
            CaptureRestoreBounds();
        }
    }

    private void CaptureRestoreBounds()
    {
        if (WindowState == WindowState.Normal && Bounds.Width > 0 && Bounds.Height > 0)
        {
            _restoreBounds = WindowPositionManager.GetBounds(this);
        }
    }

    private IReadOnlyList<Rectangle> GetScreensWorkingArea()
    {
        return Screens.All
            .Select(screen => new Rectangle(
                screen.WorkingArea.X,
                screen.WorkingArea.Y,
                screen.WorkingArea.Width,
                screen.WorkingArea.Height))
            .ToArray();
    }

    // This is a base class for many forms, which have own GetTestAccessor() methods. This has to be unique
    internal GitExtensionsFormTestAccessor GetGitExtensionsFormTestAccessor() => new(this);

    internal readonly struct GitExtensionsFormTestAccessor
    {
        private readonly GitExtensionsForm _form;

        public GitExtensionsFormTestAccessor(GitExtensionsForm form)
        {
            _form = form;
        }

        public IWindowPositionManager WindowPositionManager
        {
            get => _form._windowPositionManager;
            set => _form._windowPositionManager = value;
        }

        public Func<IReadOnlyList<Rectangle>> GetScreensWorkingArea
        {
            get => _form._getScreensWorkingArea;
            set => _form._getScreensWorkingArea = value;
        }

        // parity-scaffolding: Exercises the Wayland compositor-owned fallback on every test host.
        public Func<bool> SupportsProgrammaticPositioning
        {
            get => _form._supportsProgrammaticPositioning;
            set => _form._supportsProgrammaticPositioning = value;
        }
    }
}
