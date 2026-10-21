using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI;
using GitUI;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;
using OriginalCommitInfo = GitUI.CommitInfo.CommitInfo;

namespace WinFormsParityCapture.Tests;

// Exercises the actual source subscriptions and parent layout. The header remains
// in its constructor state; no rendered avatar-off or revision state is claimed.
// Temporary configuration isolation is performed before first AppSettings access.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class CommitInfoParentSizingTests
{
    private const string PlainParagraphs = "first short line\nsecond line with descenders gjpq\nlast line";
    private const string WrappedParagraph = "first short line second line with descenders gjpq last line repeated ordinary words for genuine RichEdit word wrapping";
    private const string MixedParagraphs = "first <a href='https://example.invalid/source'>caption</a> line\nsecond line\nlast line";
    private const string SettingsFileName = "GitExtensions.settings";
    private const uint GetFormattingRectangle = 0xB2;
    private const uint RequestNativeContentsResize = 0x441;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly Type XhtmlExtension = typeof(OriginalCommitInfo).Assembly.GetType(
        "GitUI.Editor.RichTextBoxExtension.RichTextBoxXhtmlSupportExtension", throwOnError: true)
        ?? throw new TypeLoadException("The original RichEdit XHTML extension must be present.");
    private static readonly MethodInfo XhtmlLoader = XhtmlExtension.GetMethod("SetXHTMLText", BindingFlags.Public | BindingFlags.Static,
        [typeof(RichTextBox), typeof(string)])
        ?? throw new MissingMethodException(XhtmlExtension.FullName, "SetXHTMLText");
    private static readonly MethodInfo PlainTextReader = XhtmlExtension.GetMethod("GetPlainText", BindingFlags.Public | BindingFlags.Static,
        [typeof(RichTextBox)])
        ?? throw new MissingMethodException(XhtmlExtension.FullName, "GetPlainText");

    [TestCase("Segoe UI", 9)]
    [TestCase("Segoe UI", 11)]
    [TestCase("Segoe UI", 18)]
    [TestCase("Segoe UI", 22)]
    [TestCase("Consolas", 9)]
    [TestCase("Consolas", 11)]
    [TestCase("Consolas", 18)]
    [TestCase("Consolas", 22)]
    public void Actual_parent_should_consume_native_contents_and_preserve_source_anchor_geometry(string family, int points)
    {
        WithIsolatedSettings(settingsDirectory =>
        {
            using Font font = new(family, points, FontStyle.Regular, GraphicsUnit.Point);
            font.Name.Should().Be(family);
            AppSettings.Font = font;
            AppSettings.CommitFont = font;
            WithOriginalParent((host, control, checkThreadException) =>
            {
                TableLayoutPanel table = GetField<TableLayoutPanel>(control, "tableLayout");
                RichTextBox body = GetField<RichTextBox>(control, "rtbxCommitMessage");
                RichTextBox refs = GetField<RichTextBox>(control, "RevisionInfo");
                using ContentsObserver bodyObserver = new(body);
                using ContentsObserver refsObserver = new(refs);
                body.Font.Name.Should().Be(family);
                body.Font.SizeInPoints.Should().Be(points);
                refs.Font.Name.Should().Be(family);
                refs.Font.SizeInPoints.Should().Be(points);
                body.WordWrap.Should().BeTrue();
                refs.WordWrap.Should().BeTrue();
                body.DeviceDpi.Should().Be(96);
                DpiUtil.ScaleX.Should().Be(1);
                DpiUtil.ScaleY.Should().Be(1);
                foreach ((string stage, string xhtml) in new[]
                {
                    ("plain", PlainParagraphs),
                    ("trailing-newline", PlainParagraphs + "\n"),
                    ("mixed-anchor", MixedParagraphs),
                    ("wrapped-wide", WrappedParagraph),
                })
                {
                    XhtmlLoader.Invoke(null, [body, xhtml]);
                    XhtmlLoader.Invoke(null, [refs, xhtml]);
                    Capture(stage);
                }

                control.Width = 240;
                Capture("wrapped-narrow");
                control.Width = 912;
                Capture("wrapped-larger");
                control.Width = 472;
                Capture("wrapped-restored");
                using Font grownFont = new(family, points * 2, FontStyle.Regular, GraphicsUnit.Point);
                body.Font = grownFont;
                refs.Font = grownFont;
                Capture("font-grow");
                body.Font = font;
                refs.Font = font;
                Capture("font-shrink");
                control.Height = 120;
                Capture("vertical-overflow");
                control.Height = 900;
                Capture("vertical-overflow-restored");
                body.Clear();
                refs.Clear();
                Capture("clear");

                return;

                void Capture(string stage)
                {
                    bodyObserver.Request();
                    refsObserver.Request();
                    WaitForSettledParent(control, bodyObserver, refsObserver, checkThreadException);
                    Control panel = body.Parent ?? throw new InvalidDataException("The original body has no Designer panel.");
                    Control header = GetField<Control>(control, "commitInfoHeader");
                    int nativeBodyHeight = bodyObserver.Contents.Height;
                    int nativeRefsHeight = refsObserver.Contents.Height;
                    int[] sourceHeights =
                    [
                        header.Height + header.Margin.Vertical,
                        nativeBodyHeight + body.Margin.Vertical + panel.Margin.Vertical,
                        nativeRefsHeight + refs.Margin.Vertical,
                    ];
                    int sourceTotalHeight = sourceHeights.Sum();
                    int sourceClientWidth = control.Width - (sourceTotalHeight > control.Height ? SystemInformation.VerticalScrollBarWidth : 0);
                    int sourceColumnWidth = Math.Max(sourceClientWidth, header.Width + header.Margin.Horizontal);
                    object report = new
                    {
                        stage,
                        source = "CommitInfo.ContentsResized/OnLayout",
                        headerState = "constructor",
                        dpiMode = "nativeMonitor",
                        requestedFamily = family,
                        requestedPoints = points,
                        actualSettingsFilePath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath,
                        settingsDirectory,
                        host = DescribeControl(host),
                        control = DescribeControl(control),
                        table = DescribeControl(table),
                        header = DescribeControl(header),
                        panel = DescribeControl(panel),
                        body = DescribeEditor(body, bodyObserver),
                        refs = DescribeEditor(refs, refsObserver),
                        sourceHeights,
                        sourceTotalHeight,
                        sourceClientWidth,
                        sourceColumnWidth,
                        stockVerticalScrollBarWidth = SystemInformation.VerticalScrollBarWidth,
                        actualRowHeights = table.GetRowHeights(),
                        actualColumnWidths = table.GetColumnWidths(),
                        rowStyles = table.RowStyles.Cast<RowStyle>().Select(row => new { sizeType = row.SizeType.ToString(), row.Height }).ToArray(),
                        columnStyles = table.ColumnStyles.Cast<ColumnStyle>().Select(column => new { sizeType = column.SizeType.ToString(), column.Width }).ToArray(),
                    };
                    TestContext.Out.WriteLine(JsonSerializer.Serialize(report));
                    table.RowStyles[0].SizeType.Should().Be(SizeType.AutoSize);
                    table.RowStyles[1].SizeType.Should().Be(SizeType.Absolute);
                    table.RowStyles[1].Height.Should().Be(sourceHeights[1]);
                    table.RowStyles[2].SizeType.Should().Be(SizeType.Absolute);
                    table.RowStyles[2].Height.Should().Be(sourceHeights[2]);
                    table.ColumnStyles[0].SizeType.Should().Be(SizeType.Absolute);
                    table.ColumnStyles[0].Width.Should().Be(sourceColumnWidth);
                    table.Size.Should().Be(new Size(sourceColumnWidth, sourceTotalHeight));

                    // The actual child/client heights and effective anchor edges are logged,
                    // not asserted equal to contents height or declared Designer margins.
                    Path.GetFullPath(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath).Should()
                        .Be(Path.Combine(settingsDirectory, SettingsFileName));
                }
            });
        });
    }

    private static object DescribeControl(Control control)
        => new
        {
            control.Name,
            bounds = control.Bounds,
            clientRectangle = control.ClientRectangle,
            displayRectangle = control.DisplayRectangle,
            margin = control.Margin,
            padding = control.Padding,
            anchor = control.Anchor.ToString(),
            dock = control.Dock.ToString(),
            control.AutoSize,
            control.Visible,
            control.DeviceDpi,
            rightGap = control.Parent is null ? (int?)null : control.Parent.ClientSize.Width - control.Right,
            bottomGap = control.Parent is null ? (int?)null : control.Parent.ClientSize.Height - control.Bottom,
        };

    private static object DescribeEditor(RichTextBox editor, ContentsObserver observer)
    {
        SendMessage(editor.Handle, GetFormattingRectangle, 0, out NativeRectangle rectangle);
        List<object> origins = [];
        for (int line = 0; ; line++)
        {
            int index = editor.GetFirstCharIndexFromLine(line);
            if (index < 0)
            {
                break;
            }

            origins.Add(new { line, index, point = editor.GetPositionFromCharIndex(index) });
        }

        return new
        {
            geometry = DescribeControl(editor),
            observer.Contents,
            observer.EventCount,
            formattingRectangle = new { rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom },
            editor.WordWrap,
            editor.ReadOnly,
            scrollBars = editor.ScrollBars.ToString(),
            font = new { editor.Font.Name, editor.Font.SizeInPoints, style = editor.Font.Style.ToString() },
            text = PlainTextReader.Invoke(null, [editor]) as string
                ?? throw new InvalidDataException("The original XHTML reader returned no plain text."),
            visualLineOrigins = origins,
        };
    }

    private static void WaitForSettledParent(OriginalCommitInfo control, ContentsObserver body, ContentsObserver refs, Action checkThreadException)
    {
        Stopwatch elapsed = Stopwatch.StartNew();
        Stopwatch stable = new();
        string? previous = null;
        while (elapsed.Elapsed < TimeSpan.FromSeconds(5))
        {
            Application.DoEvents();
            control.Update();
            checkThreadException();
            int bodyCachedHeight = GetField<int>(control, "_commitMessageHeight");
            int refsCachedHeight = GetField<int>(control, "_revisionInfoHeight");
            TableLayoutPanel table = GetField<TableLayoutPanel>(control, "tableLayout");
            string current = $"{body.Contents}/{refs.Contents}/{bodyCachedHeight}/{refsCachedHeight}/{table.Bounds}";
            bool consumed = body.EventCount > 0 && refs.EventCount > 0
                && bodyCachedHeight == body.Contents.Height && refsCachedHeight == refs.Contents.Height;
            if (!consumed || current != previous)
            {
                previous = current;
                stable.Restart();
            }
            else if (stable.Elapsed >= TimeSpan.FromMilliseconds(250))
            {
                return;
            }

            Thread.Sleep(5);
        }

        throw new AssertionException($"The actual source throttle/layout did not settle: {previous}");
    }

    private static T GetField<T>(object owner, string name)
        => owner.GetType().GetField(name, PrivateInstance)?.GetValue(owner) is T value
            ? value
            : throw new MissingFieldException(owner.GetType().FullName, name);

    private static void WithOriginalParent(Action<Form, OriginalCommitInfo, Action> action)
    {
        FieldInfo managerField = typeof(ThreadHelper).GetField("_taskManager", PrivateStatic)
            ?? throw new MissingFieldException(typeof(ThreadHelper).FullName, "_taskManager");
        object? originalManager = managerField.GetValue(null);
        SynchronizationContext? originalSynchronizationContext = SynchronizationContext.Current;
        using Form host = new() { AutoScaleMode = AutoScaleMode.None, ClientSize = new Size(1000, 1000), ShowInTaskbar = false };
        using JoinableTaskContext context = new();
        PropertyInfo contextProperty = typeof(ThreadHelper).GetProperty(nameof(ThreadHelper.JoinableTaskContext), BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMemberException(typeof(ThreadHelper).FullName, nameof(ThreadHelper.JoinableTaskContext));
        contextProperty.SetValue(null, context);
        Exception? threadException = null;
        ThreadExceptionEventHandler threadExceptionHandler = (_, e) => threadException ??= e.Exception;
        Application.ThreadException += threadExceptionHandler;
        try
        {
            using OriginalCommitInfo control = new() { Size = new Size(472, 900) };
            host.Controls.Add(control);
            GetField<TableLayoutPanel>(control, "tableLayout").Visible = true;
            host.Show();
            Application.DoEvents();
            action(host, control, () =>
            {
                if (threadException is not null)
                {
                    throw new InvalidOperationException("The actual source parent raised a UI-thread exception.", threadException);
                }
            });
        }
        finally
        {
            // Original CommitInfo disposal cancels/disposes its real Rx subscriptions.
            // Do not install a test scheduler or call its resize handlers directly.
            host.Close();
            Application.DoEvents();
            Application.ThreadException -= threadExceptionHandler;
            managerField.SetValue(null, originalManager);
            SynchronizationContext.SetSynchronizationContext(originalSynchronizationContext);
        }
    }

    private static void WithIsolatedSettings(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.CommitInfoParentProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string settingsPath = Path.Combine(directory, SettingsFileName);
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(directory);
        using GitExtSettingsCache cache = new(settingsPath, autoSave: false);
        DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
        AppSettings.UsingContainer(settings, () => action(directory));

        // Match the existing native-grid fixture lifetime: the first source cache owns
        // a watcher. Retain exact roots for the later validated tranche cleanup.
        TestContext.Out.WriteLine($"isolated original settings retained: {directory}");
    }

    private static void InitializeAppSettingsWithoutRealConfiguration(string directory)
    {
        Type settingsType = typeof(AppSettings).Assembly.GetType("GitCommands.Properties.Settings", throwOnError: true)
            ?? throw new TypeLoadException("GitCommands.Properties.Settings");
        ApplicationSettingsBase configuration = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ApplicationSettingsBase
            ?? throw new MissingMemberException(settingsType.FullName, "Default");
        _ = configuration["IsPortable"];
        SettingsPropertyValue portable = configuration.PropertyValues["IsPortable"]
            ?? throw new MissingMemberException(settingsType.FullName, "IsPortable");
        object originalPortableValue = portable.PropertyValue;
        bool originalDirty = portable.IsDirty;
        FieldInfo frameworkPath = typeof(Application).GetField("s_executablePath", PrivateStatic)
            ?? throw new MissingFieldException(typeof(Application).FullName, "s_executablePath");
        object? originalFrameworkPath = frameworkPath.GetValue(null);
        string actualExecutablePath = Application.ExecutablePath;
        string isolatedExecutablePath = Path.Combine(directory, "GitExtensions.exe");
        try
        {
            portable.PropertyValue = true;
            frameworkPath.SetValue(null, isolatedExecutablePath);
            _ = AppSettings.SettingsContainer;
            FieldInfo sourcePath = typeof(AppSettings).GetField("_applicationExecutablePath", PrivateStatic)
                ?? throw new MissingFieldException(typeof(AppSettings).FullName, "_applicationExecutablePath");
            if (Equals(sourcePath.GetValue(null), isolatedExecutablePath))
            {
                sourcePath.SetValue(null, actualExecutablePath);
            }
        }
        finally
        {
            frameworkPath.SetValue(null, originalFrameworkPath);
            portable.PropertyValue = originalPortableValue;
            portable.IsDirty = originalDirty;
        }
    }

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nuint firstParameter, nint secondParameter);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nuint firstParameter, out NativeRectangle rectangle);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    private sealed class ContentsObserver : IDisposable
    {
        private readonly RichTextBox _editor;

        public ContentsObserver(RichTextBox editor)
        {
            _editor = editor;
            editor.ContentsResized += OnContentsResized;
        }

        public Rectangle Contents { get; private set; }

        public int EventCount { get; private set; }

        public void Request() => SendMessage(_editor.Handle, RequestNativeContentsResize, 0, 0);

        public void Dispose() => _editor.ContentsResized -= OnContentsResized;

        private void OnContentsResized(object? sender, ContentsResizedEventArgs e)
        {
            Contents = e.NewRectangle;
            EventCount++;
        }
    }
}
