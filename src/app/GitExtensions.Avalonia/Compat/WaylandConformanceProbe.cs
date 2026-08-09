using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;

namespace GitExtensions.Compat;

// parity-scaffolding: records native Wayland protocol evidence until the platform gate closes.
internal sealed class WaylandConformanceProbe
{
    internal const string ReportPathEnvironmentVariable = "GITEXTENSIONS_WAYLAND_CONFORMANCE_REPORT";
    internal const string WindowTitle = "Git Extensions Wayland conformance";
    internal const string ClipboardText = "Git Extensions Wayland plain-text clipboard";
    internal const string ClipboardHtml = "<strong>Git Extensions Wayland rich clipboard</strong>";
    internal const string DragText = "Git Extensions Wayland drag-and-drop";

    private static readonly DataFormat<string> HtmlFormat =
        DataFormat.CreateStringPlatformFormat("text/html");

    private readonly string _reportPath;
    private readonly Window _mainWindow;
    private readonly Window _probeWindow;
    private readonly Border _dragSource;
    private readonly Border _dropTarget;
    private readonly Button _edgeButton;
    private readonly List<double> _observedScales = [];
    private readonly DispatcherTimer _reportTimer;
    private DataTransfer? _clipboardData;
    private bool _clipboardPublished;
    private bool _dragStarted;
    private bool _dropReceived;
    private bool _tooltipOpened;
    private bool _contextMenuOpened;
    private string? _error;

    private WaylandConformanceProbe(string reportPath, Window mainWindow)
    {
        _reportPath = reportPath;
        _mainWindow = mainWindow;
        (_probeWindow, _dragSource, _dropTarget, _edgeButton) = CreateProbeWindow();
        _reportTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, (_, _) => WriteReport());
    }

    internal static bool IsSupportedRequest(string? reportPath, bool isLinux, string? waylandDisplay)
        => !string.IsNullOrWhiteSpace(reportPath)
            && isLinux
            && !string.IsNullOrWhiteSpace(waylandDisplay);

    internal static void StartIfRequested(IClassicDesktopStyleApplicationLifetime desktop)
    {
        string? reportPath = Environment.GetEnvironmentVariable(ReportPathEnvironmentVariable);
        if (!IsSupportedRequest(reportPath, OperatingSystem.IsLinux(), Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"))
            || desktop.MainWindow is not { } mainWindow)
        {
            return;
        }

        WaylandConformanceProbe probe = new(Path.GetFullPath(reportPath!), mainWindow);
        probe.Start();
    }

    private void Start()
    {
        _probeWindow.Opened += (_, _) => WriteReport();
        _probeWindow.Closed += (_, _) =>
        {
            _reportTimer.Stop();
            (_clipboardData as IDisposable)?.Dispose();
        };
        _reportTimer.Start();
        WriteReport();
        _ = _probeWindow.ShowDialog(_mainWindow);
    }

    private (Window Window, Border DragSource, Border DropTarget, Button EdgeButton) CreateProbeWindow()
    {
        TextBlock introduction = new()
        {
            Text = "Native Wayland protocol probe",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
        };
        Border dragSource = CreateDropSurface("Drag source", Brushes.LightSkyBlue);
        Border dropTarget = CreateDropSurface("Drop target", Brushes.LightGreen);
        Button edgeButton = new()
        {
            Content = "Edge popup",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Width = 120,
            Height = 42,
        };

        ToolTip.SetTip(edgeButton, "Wayland edge tooltip");
        ToolTip.AddToolTipOpeningHandler(edgeButton, (_, _) =>
        {
            _tooltipOpened = true;
            WriteReport();
        });
        ToolTip.SetShowDelay(edgeButton, 500);
        edgeButton.PointerEntered += (_, _) =>
        {
            if (!_tooltipOpened)
            {
                ToolTip.SetIsOpen(edgeButton, true);
            }
        };

        ContextMenu contextMenu = new()
        {
            ItemsSource = new object[]
            {
                new MenuItem { Header = "Wayland edge context menu" },
            },
        };
        contextMenu.Opened += (_, _) =>
        {
            _contextMenuOpened = true;
            WriteReport();
        };
        edgeButton.ContextMenu = contextMenu;
        edgeButton.Click += (_, _) => _ = PublishClipboardAndReportAsync();
        edgeButton.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(edgeButton).Properties.IsRightButtonPressed)
            {
                ToolTip.SetIsOpen(edgeButton, false);
                contextMenu.Open(edgeButton);
            }
        };

        Grid transferGrid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 24,
            Children =
            {
                dragSource,
                dropTarget,
            },
        };
        Grid.SetColumn(dropTarget, 1);

        Grid root = new()
        {
            Margin = new Thickness(24),
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            RowSpacing = 20,
            Children =
            {
                introduction,
                transferGrid,
                edgeButton,
            },
        };
        Grid.SetRow(transferGrid, 1);
        Grid.SetRow(edgeButton, 2);

        Window window = new()
        {
            Title = WindowTitle,
            Width = 680,
            Height = 460,
            MinWidth = 680,
            MinHeight = 460,
            CanResize = false,
            Content = root,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
        };
        window.KeyDown += (_, e) =>
        {
            if (e.Key == Key.F6)
            {
                _ = PublishClipboardAndReportAsync();
            }
        };

        dragSource.PointerPressed += DragSource_PointerPressed;
        DragDrop.SetAllowDrop(dropTarget, true);
        DragDrop.AddDragOverHandler(dropTarget, DropTarget_DragOver);
        DragDrop.AddDropHandler(dropTarget, DropTarget_Drop);

        return (window, dragSource, dropTarget, edgeButton);
    }

    private static Border CreateDropSurface(string text, IBrush background)
        => new()
        {
            Background = background,
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            MinHeight = 220,
            Child = new TextBlock
            {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 18,
            },
        };

    private async Task PublishClipboardAndReportAsync()
    {
        try
        {
            await PublishClipboardAsync();
        }
        catch (Exception exception)
        {
            _error = exception.ToString();
        }

        WriteReport();
    }

    private void DragSource_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(_dragSource).Properties.IsLeftButtonPressed)
        {
            return;
        }

        _ = StartDragAsync(e);
    }

    private async Task StartDragAsync(PointerPressedEventArgs e)
    {
        _dragStarted = true;
        WriteReport();
        using DataTransfer data = new();
        data.Add(DataTransferItem.CreateText(DragText));
        await DragDrop.DoDragDropAsync(e, data, DragDropEffects.Copy);
    }

    private static void DropTarget_DragOver(object? sender, DragEventArgs e)
        => e.DragEffects = e.DataTransfer.TryGetText() == DragText
            ? DragDropEffects.Copy
            : DragDropEffects.None;

    private void DropTarget_Drop(object? sender, DragEventArgs e)
    {
        _dropReceived = e.DataTransfer.TryGetText() == DragText;
        WriteReport();
    }

    private async Task PublishClipboardAsync()
    {
        IClipboard? clipboard = _probeWindow.Clipboard;
        if (clipboard is null)
        {
            throw new InvalidOperationException("The native Wayland window did not expose a clipboard.");
        }

        DataTransferItem item = new();
        item.SetText(ClipboardText);
        item.Set(HtmlFormat, ClipboardHtml);
        _clipboardData = new DataTransfer();
        _clipboardData.Add(item);
        await clipboard.SetDataAsync(_clipboardData);
        _clipboardPublished = true;
    }

    private void WriteReport()
    {
        try
        {
            if (_probeWindow.IsVisible
                && !_observedScales.Any(scale => Math.Abs(scale - _probeWindow.RenderScaling) < 0.001))
            {
                _observedScales.Add(_probeWindow.RenderScaling);
            }

            Screen? currentScreen = _probeWindow.Screens.ScreenFromWindow(_probeWindow);
            object report = new
            {
                schemaVersion = 1,
                backend = "wayland",
                waylandDisplay = Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"),
                mainWindow = WindowSnapshot(_mainWindow),
                modalProbe = new
                {
                    window = WindowSnapshot(_probeWindow),
                    ownerTitle = (_probeWindow.Owner as Window)?.Title,
                    ownerMatchesMainWindow = ReferenceEquals(_probeWindow.Owner, _mainWindow),
                    currentScreen = currentScreen is null ? null : ScreenSnapshot(currentScreen),
                    observedScales = _observedScales.Order().ToArray(),
                    dragSourceCenter = ControlCenter(_dragSource),
                    dropTargetCenter = ControlCenter(_dropTarget),
                    edgeButtonCenter = ControlCenter(_edgeButton),
                },
                clipboard = new
                {
                    published = _clipboardPublished,
                    plainText = ClipboardText,
                    html = ClipboardHtml,
                },
                interactions = new
                {
                    dragStarted = _dragStarted,
                    dropReceived = _dropReceived,
                    tooltipOpened = _tooltipOpened,
                    contextMenuOpened = _contextMenuOpened,
                },
                error = _error,
            };

            string? parentDirectory = Path.GetDirectoryName(_reportPath);
            if (!string.IsNullOrEmpty(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            string temporaryPath = $"{_reportPath}.tmp";
            File.WriteAllText(
                temporaryPath,
                JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
            File.Move(temporaryPath, _reportPath, overwrite: true);
        }
        catch (Exception exception)
        {
            _error ??= exception.ToString();
        }
    }

    private static object WindowSnapshot(Window window)
        => new
        {
            window.Title,
            window.IsVisible,
            window.RenderScaling,
            position = new { window.Position.X, window.Position.Y },
            clientSize = new { window.ClientSize.Width, window.ClientSize.Height },
            windowState = window.WindowState.ToString(),
        };

    private static object ScreenSnapshot(Screen screen)
        => new
        {
            screen.DisplayName,
            screen.IsPrimary,
            screen.Scaling,
            bounds = new
            {
                screen.Bounds.X,
                screen.Bounds.Y,
                screen.Bounds.Width,
                screen.Bounds.Height,
            },
            workingArea = new
            {
                screen.WorkingArea.X,
                screen.WorkingArea.Y,
                screen.WorkingArea.Width,
                screen.WorkingArea.Height,
            },
        };

    private static object? ControlCenter(Control control)
    {
        if (!control.IsEffectivelyVisible || control.Bounds.Width <= 0 || control.Bounds.Height <= 0)
        {
            return null;
        }

        PixelPoint point = control.PointToScreen(new Point(control.Bounds.Width / 2, control.Bounds.Height / 2));
        return new { x = point.X, y = point.Y };
    }
}
