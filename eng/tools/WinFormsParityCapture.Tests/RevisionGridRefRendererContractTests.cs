using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using GitExtUtils.GitUI;
using GitUI;
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// Native contract fixture: exercises the unchanged product renderer in a real
// native control's Paint path. This is renderer evidence, not a complete grid/state
// or CSS-theme audit. No AppSettings, repository, custom theme or clipboard is used.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class RevisionGridRefRendererContractTests
{
    private const BindingFlags AllStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const string RendererName = "GitUI.UserControls.RevisionGrid.RevisionGridRefRenderer";
    private const string ShapeName = "GitUI.UserControls.RevisionGrid.RefLabelShape";
    private const string IconName = "GitUI.UserControls.RevisionGrid.RefLabelIcon";
    private const string HitInfoName = "GitUI.UserControls.RevisionGrid.Columns.RefLabelHitInfo";
    private static readonly string[] Shapes = ["Rect", "PointRight", "NotchLeft", "NotchRight", "PointLeft"];
    private static readonly Color HeadColor = Color.FromArgb(255, 0, 122, 0);
    private static readonly Color RemoteColor = Color.FromArgb(255, 0, 92, 180);
    private static readonly JsonSerializerOptions ReportOptions = new() { WriteIndented = true };
    private static readonly Assembly SourceAssembly = typeof(RevisionDataGridView).Assembly;
    private static readonly Type RendererType = RequiredType(RendererName);
    private static readonly Type ShapeType = RequiredType(ShapeName);
    private static readonly Type IconType = RequiredType(IconName);
    private static readonly Type HitInfoType = RequiredType(HitInfoName);
    private static readonly MethodInfo DrawMethod = RequiredMethod(RendererType, "DrawRefEx", AllStatic);
    private static readonly MethodInfo PointWidthMethod = RequiredMethod(RendererType, "GetPointWidth", AllStatic);
    private static readonly MethodInfo ContainsMethod = RequiredMethod(HitInfoType, "Contains", AllInstance);

    [TestCase("Segoe UI", 9, FontStyle.Regular)]
    [TestCase("Segoe UI", 9, FontStyle.Bold)]
    [TestCase("Segoe UI", 11, FontStyle.Regular)]
    [TestCase("Segoe UI", 11, FontStyle.Bold)]
    [TestCase("Arial", 9, FontStyle.Italic)]
    [TestCase("Consolas", 9, FontStyle.Regular)]
    public void DrawRefEx_should_record_native_metrics_paths_and_integer_hit_ownership(string family, int points, FontStyle style)
    {
        FieldInfo managerField = typeof(ThreadHelper).GetField("_taskManager", AllStatic)
            ?? throw new MissingFieldException(typeof(ThreadHelper).FullName, "_taskManager");
        object? originalManager = managerField.GetValue(null);
        SynchronizationContext? originalSynchronizationContext = SynchronizationContext.Current;
        PropertyInfo contextProperty = typeof(ThreadHelper).GetProperty(nameof(ThreadHelper.JoinableTaskContext), AllStatic)
            ?? throw new MissingMemberException(typeof(ThreadHelper).FullName, nameof(ThreadHelper.JoinableTaskContext));
        using JoinableTaskContext context = new();
        using Font font = new(family, points, style, GraphicsUnit.Point);
        using Form owner = new()
        {
            AutoScaleMode = AutoScaleMode.None,
            ClientSize = new Size(840, 730),
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.CenterScreen,
            Text = "Actual original revision-ref renderer contract",
        };
        using ProbeSurface surface = new(font) { Dock = DockStyle.Fill };
        Exception? threadException = null;
        ThreadExceptionEventHandler handler = (_, args) => threadException ??= args.Exception;
        Application.ThreadException += handler;
        try
        {
            contextProperty.SetValue(null, context);
            font.Name.Should().Be(family, "font substitution is not evidence for the requested source font");
            owner.Controls.Add(surface);
            owner.Show();
            owner.Activate();
            Application.DoEvents();
            owner.DeviceDpi.Should().Be(96);
            surface.DeviceDpi.Should().Be(96);
            DpiUtil.DpiX.Should().Be(96);
            DpiUtil.DpiY.Should().Be(96);
            surface.Refresh();
            Application.DoEvents();
            threadException.Should().BeNull();
            surface.PaintException.Should().BeNull();
            surface.Reports.Should().HaveCount(35);
            surface.NestledReports.Should().HaveCount(4);

            string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                "RevisionGridRefProbe", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            using CaptureImageResult capture = ImageCapture.Capture(owner, [], []);
            capture.Method.Should().Be(GitExtensions.ParityCapture.CaptureMethod.PrintWindow);
            capture.Bitmap.Save(Path.Combine(directory, "native-renderer.png"), ImageFormat.Png);
            surface.PaintException.Should().BeNull();
            threadException.Should().BeNull();
            File.WriteAllText(Path.Combine(directory, "native-contract.json"), JsonSerializer.Serialize(new
            {
                sourceType = RendererName,
                sourceHitType = HitInfoName,
                measurementRoute = "original DrawRefEx from native control Paint; native TextRenderer.NoPadding",
                dpiMode = "nativeMonitor",
                owner.DeviceDpi,
                captureMethod = capture.Method.ToString(),
                font = new { font.Name, font.SizeInPoints, style = font.Style.ToString() },
                sourceRowHeight = surface.SourceRowHeight,
                sourceRowMeasureHeight = surface.SourceRowMeasureHeight,
                sourceRowSpacing = DpiUtil.Scale(9),
                measurementVariants = surface.MeasurementVariants,
                headArgb = $"#{HeadColor.ToArgb():X8}",
                remoteArgb = $"#{RemoteColor.ToArgb():X8}",
                nativeWindowArgb = $"#{SystemColors.Window.ToArgb():X8}",
                noSettingsOrRepositoryUsed = true,
                note = "Unmodified native raster; no pixel-parity assertion and no CSS-theme, complete grid or high-DPI claim.",
                labels = surface.Reports,
                nestled = surface.NestledReports,
                explicitHitEdges = RecordHitEdges(),
            }, ReportOptions));
            TestContext.Out.WriteLine($"original revision-ref contract: {directory}");
        }
        finally
        {
            owner.Close();
            Application.ThreadException -= handler;
            managerField.SetValue(null, originalManager);
            SynchronizationContext.SetSynchronizationContext(originalSynchronizationContext);
        }
    }

    private static object[] RecordHitEdges()
    {
        List<object> reports = [];
        foreach (int height in new[] { 17, 18 })
        {
            Rectangle bounds = new(12, 14, 47, height);
            int pointWidth = height / 2;
            foreach (string shape in Shapes)
            {
                object hitInfo = CreateHitInfo(bounds, shape, pointWidth);
                List<string> rows = [];
                for (int y = bounds.Top - 1; y <= bounds.Bottom; y++)
                {
                    char[] row = new char[bounds.Width + 2];
                    for (int x = bounds.Left - 1; x <= bounds.Right; x++)
                    {
                        Point point = new(x, y);
                        bool actual = Contains(hitInfo, point);
                        actual.Should().Be(ExpectedContains(bounds, shape, pointWidth, point));
                        row[x - bounds.Left + 1] = actual ? '#' : '.';
                    }

                    rows.Add(new string(row));
                }

                Contains(hitInfo, new Point(bounds.Right, bounds.Top + (height / 2))).Should().BeFalse();
                Contains(hitInfo, new Point(bounds.Left, bounds.Bottom)).Should().BeFalse();
                if (shape == "Rect")
                {
                    Contains(hitInfo, bounds.Location).Should().BeTrue(
                        "the original Rect hit contract owns the bounding corner even though its painted path is rounded");
                }

                reports.Add(new { shape, bounds, pointWidth, mapOrigin = new Point(bounds.Left - 1, bounds.Top - 1), rows });
            }
        }

        return reports.ToArray();
    }

    private static bool ExpectedContains(Rectangle bounds, string shape, int pointWidth, Point point)
    {
        if (point.X < bounds.Left || point.X >= bounds.Right || point.Y < bounds.Top || point.Y >= bounds.Bottom)
        {
            return false;
        }

        int halfHeight = bounds.Height / 2;
        if (pointWidth <= 0 || shape == "Rect" || halfHeight <= 0)
        {
            return true;
        }

        int dy = Math.Min(Math.Abs(point.Y - (bounds.Top + halfHeight)), halfHeight);
        int slant = pointWidth * dy / halfHeight;
        return shape switch
        {
            "PointRight" => point.X <= bounds.Right - slant,
            "NotchRight" => point.X <= bounds.Right - pointWidth + slant,
            "PointLeft" => point.X >= bounds.Left + slant,
            "NotchLeft" => point.X >= bounds.Left + pointWidth - slant,
            _ => true,
        };
    }

    private static object CreateHitInfo(Rectangle bounds, string shape, int pointWidth)
        => Activator.CreateInstance(HitInfoType, AllInstance, binder: null,
            args: [bounds, Enum.Parse(ShapeType, shape), pointWidth, null, null], culture: null)
            ?? throw new InvalidDataException("The original ref hit-info constructor returned no value.");

    private static bool Contains(object hitInfo, Point point)
        => ContainsMethod.Invoke(hitInfo, [point]) is true;

    private static Type RequiredType(string name)
        => SourceAssembly.GetType(name, throwOnError: true) ?? throw new TypeLoadException(name);

    private static MethodInfo RequiredMethod(Type type, string name, BindingFlags flags)
        => type.GetMethod(name, flags) ?? throw new MissingMethodException(type.FullName, name);

    private sealed class ProbeSurface(Font font) : Control
    {
        public Exception? PaintException { get; private set; }

        public List<object> Reports { get; } = [];

        public List<object> NestledReports { get; } = [];

        public List<object> MeasurementVariants { get; } = [];

        public int SourceRowHeight { get; private set; }

        public float SourceRowMeasureHeight { get; private set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            try
            {
                Reports.Clear();
                NestledReports.Clear();
                MeasurementVariants.Clear();
                e.Graphics.Clear(SystemColors.Window);
                e.Graphics.DpiX.Should().Be(96);
                e.Graphics.DpiY.Should().Be(96);

                // This is the unchanged grid's configured-font row-template algorithm.
                // Additional allocations below test integer layout, not a different DPI.
                SourceRowMeasureHeight = e.Graphics.MeasureString("By", font).Height;
                SourceRowHeight = (int)SourceRowMeasureHeight + DpiUtil.Scale(9);
                foreach (string value in new[] { string.Empty, "main", "feature&parity", "featureparity", "feature&&parity", "feature&parity&", "&", "main&", "a&b", "ab", "a&&b", " " })
                {
                    MeasurementVariants.Add(new
                    {
                        value,
                        noPadding = TextRenderer.MeasureText(e.Graphics, value, font, Size.Empty, TextFormatFlags.NoPadding),
                        literalSingleLine = TextRenderer.MeasureText(e.Graphics, value, font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine),
                    });
                }

                int y = 8;
                foreach (string shape in Shapes)
                {
                    int rowStart = y;
                    foreach (string icon in new[] { "None", "Head", "HeadMergeSource" })
                    {
                        bool extraAllocation = icon == "Head";
                        int rowHeight = SourceRowHeight + (extraAllocation ? 1 : 0);
                        DrawAndRecord(e.Graphics, new Rectangle(8, y, 180, rowHeight), 3,
                            "main", shape, icon, false, false, false, false, HeadColor, deferHighlight: false,
                            allocationKind: extraAllocation ? "sourceRowHeightPlusOne-allocationContractOnly" : "sourceRowHeight");
                        y += rowHeight;
                    }

                    DrawAndRecord(e.Graphics, new Rectangle(210, rowStart, 180, SourceRowHeight), 3,
                        "feature/parity", shape, "Head", true, true, true, true, HeadColor, deferHighlight: false);
                    DrawAndRecord(e.Graphics, new Rectangle(450, rowStart, 24, SourceRowHeight), 3,
                        string.Empty, shape, "None", false, false, false, false, HeadColor, deferHighlight: false);
                    DrawAndRecord(e.Graphics, new Rectangle(560, rowStart, 24, SourceRowHeight), 3,
                        "clipped feature/parity", shape, "Head", true, false, false, false, HeadColor, deferHighlight: false);
                    DrawAndRecord(e.Graphics, new Rectangle(610, rowStart, 220, SourceRowHeight), 3,
                        "feature&parity", shape, "None", false, false, false, false, HeadColor, deferHighlight: false);
                }

                foreach ((string left, string right) in new[]
                {
                    ("PointRight", "NotchLeft"),
                    ("NotchRight", "PointLeft"),
                })
                {
                    foreach (bool remoteHighlight in new[] { false, true })
                    {
                        Rectangle row = new(8, y, 800, SourceRowHeight);
                        (Rectangle first, int _, Action? firstHighlight) = DrawAndRecord(e.Graphics,
                            row, 3, "main", left, "Head", false, true, false, !remoteHighlight,
                            HeadColor, deferHighlight: true, recordSingle: false);
                        int pointWidth = (int)(PointWidthMethod.Invoke(null, [font, e.Graphics])
                            ?? throw new InvalidDataException("The original point-width method returned no value."));
                        int remoteOffset = Math.Max(3, first.Right - row.X - pointWidth + 1);
                        (Rectangle second, int _, Action? secondHighlight) = DrawAndRecord(e.Graphics,
                            row, remoteOffset, "origin/main", right, "None", false, true, false, remoteHighlight,
                            RemoteColor, deferHighlight: true, recordSingle: false);
                        firstHighlight?.Invoke();
                        secondHighlight?.Invoke();
                        object firstHit = CreateHitInfo(first, left, pointWidth);
                        object secondHit = CreateHitInfo(second, right, pointWidth);
                        List<object> seam = [];
                        for (int pointY = first.Top; pointY < first.Bottom; pointY++)
                        {
                            for (int pointX = second.Left; pointX < first.Right; pointX++)
                            {
                                Point point = new(pointX, pointY);
                                bool localHit = RevisionGridRefRendererContractTests.Contains(firstHit, point);
                                bool remoteHit = RevisionGridRefRendererContractTests.Contains(secondHit, point);
                                seam.Add(new { point, localHit, remoteHit, owner = localHit ? "first" : remoteHit ? "second" : "none" });
                            }
                        }

                        NestledReports.Add(new { left, right, remoteHighlight, first, second, pointWidth, remoteOffset, seam });
                        y += SourceRowHeight;
                    }
                }
            }
            catch (Exception exception)
            {
                PaintException = exception;
            }
        }

        private (Rectangle Rect, int Offset, Action? Highlight) DrawAndRecord(
            Graphics graphics, Rectangle bounds, int initialOffset, string name, string shape,
            string icon, bool selected, bool fill, bool dashed, bool highlight, Color color,
            bool deferHighlight, bool recordSingle = true, string allocationKind = "sourceRowHeight")
        {
            object[] args = [selected, font, initialOffset, name, color, Enum.Parse(IconType, icon), bounds,
                graphics, dashed, fill, highlight, Enum.Parse(ShapeType, shape)];
            object result = DrawMethod.Invoke(null, args)
                ?? throw new InvalidDataException("The original ref renderer returned no result.");
            (Rectangle rect, Action? drawHighlight) = ((Rectangle, Action?))result;
            int offset = (int)args[2];
            Size text = string.IsNullOrEmpty(name)
                ? new Size(0, TextRenderer.MeasureText(graphics, " ", font, Size.Empty, TextFormatFlags.NoPadding).Height)
                : TextRenderer.MeasureText(graphics, name, font, Size.Empty, TextFormatFlags.NoPadding);
            int backgroundHeight = text.Height + (DpiUtil.Scale(2) * 2) - 1;
            int pointWidth = backgroundHeight / 2;
            int iconWidth = icon is "Head" or "HeadMergeSource" ? bounds.Height / 2 : 0;
            int padding = DpiUtil.Scale(string.IsNullOrEmpty(name) ? 1 : 4);
            int extraWidth = shape switch
            {
                "NotchLeft" or "NotchRight" => pointWidth,
                "PointLeft" or "PointRight" => pointWidth / 2,
                _ => 0,
            };
            Rectangle expected = new(bounds.X + initialOffset,
                bounds.Y + ((bounds.Height - backgroundHeight) / 2),
                Math.Min(bounds.Width - initialOffset, text.Width + iconWidth + (padding * 2) + extraWidth - 1),
                backgroundHeight);
            rect.Should().Be(expected);
            offset.Should().Be(initialOffset + rect.Width + DpiUtil.Scale(5));
            int nativePointWidth = (int)(PointWidthMethod.Invoke(null, [font, graphics])
                ?? throw new InvalidDataException("The original point-width method returned no value."));
            nativePointWidth.Should().Be(pointWidth);
            if (highlight)
            {
                drawHighlight.Should().NotBeNull();
            }
            else
            {
                drawHighlight.Should().BeNull();
            }

            string pathMethod = shape == "Rect" ? "CreateRoundRectPath" : "Create" + shape + "RoundRectPath";
            object[] pathArgs = shape == "Rect"
                ? [rect, DpiUtil.Scale(5)]
                : [rect, DpiUtil.Scale(5), pointWidth];
            using GraphicsPath path = RequiredMethod(RendererType, pathMethod, AllStatic).Invoke(null, pathArgs) as GraphicsPath
                ?? throw new InvalidDataException("The original renderer path factory returned no path.");
            object hitInfo = CreateHitInfo(rect, shape, pointWidth);
            List<string> hitRows = [];
            for (int pointY = rect.Top; pointY < rect.Bottom; pointY++)
            {
                char[] row = new char[rect.Width];
                for (int pointX = rect.Left; pointX < rect.Right; pointX++)
                {
                    Point point = new(pointX, pointY);
                    bool hit = RevisionGridRefRendererContractTests.Contains(hitInfo, point);
                    hit.Should().Be(ExpectedContains(rect, shape, pointWidth, point));
                    row[pointX - rect.Left] = hit ? '#' : '.';
                }

                hitRows.Add(new string(row));
            }

            if (recordSingle)
            {
                Reports.Add(new
                {
                    name, shape, icon, selected, fill, dashed, highlight, allocationKind, bounds, initialOffset,
                    text, backgroundHeight, pointWidth, iconWidth, padding, extraWidth, rect, offset,
                    pathPoints = path.PathPoints.Select(point => new { point.X, point.Y }),
                    pathTypes = path.PathTypes.Select(type => (int)type),
                    hitRows,
                });
            }

            if (!deferHighlight)
            {
                drawHighlight?.Invoke();
                drawHighlight = null;
            }

            return (rect, offset, drawHighlight);
        }
    }
}
