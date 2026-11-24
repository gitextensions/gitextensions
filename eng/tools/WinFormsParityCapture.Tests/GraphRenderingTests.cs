using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// parity-scaffolding: original graph bitmap oracle; no product consumer or native window acquisition.

/// <summary>
///  Measures original graph rasterization using the production cache's owned bitmap context.
/// </summary>
[TestFixture]
[NonParallelizable]
public sealed class GraphRenderingTests
{
    private const int LaneWidth = 16;
    private const int NodeDimension = 10;
    private const int CanvasWidth = 112;
    private const int CanvasPadding = 8;
    private const string PortableSettingName = "IsPortable";
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly ObjectId NodeId = Id(1);
    private static readonly ObjectId OtherId = Id(99);

    private string _themeName = string.Empty;
    private string _settingsDirectory = string.Empty;
    private ThemeSettings? _originalThemeSettings;
    private SystemColorMode _originalColorMode;
    private GitExtSettingsCache? _cache;
    private DistributedSettings? _settings;

    /// <summary>
    ///  Uses one real first-use lane palette per process without resetting its source static cache.
    /// </summary>
    [OneTimeSetUp]
    public void Prepare_isolated_source_settings_and_process_theme()
    {
        _themeName = Environment.GetEnvironmentVariable("GITEXT_GRAPH_PROBE_THEME") ?? "light";
        new[] { "light", "dark", "custom" }.Should().Contain(_themeName);
        _settingsDirectory = Path.Combine(Path.GetTempPath(), "GitExtensions.GraphBitmapProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_settingsDirectory);
        string settingsPath = Path.Combine(_settingsDirectory, "GitExtensions.settings");
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(_settingsDirectory);
        _cache = new GitExtSettingsCache(settingsPath, autoSave: false);
        _settings = new DistributedSettings(lowerPriority: null, _cache, SettingLevel.Unknown);
        _originalThemeSettings = ThemeModule.Settings;
        _originalColorMode = Application.ColorMode;
        Theme theme = LoadToolTheme(_themeName);
        Theme invariant = Theme.CreateDefaultTheme();
        ThemeSettings settings = new(theme, invariant, [], useSystemVisualStyle: _themeName == "light");
        SetThemeSettings(settings);
        Application.SetColorMode(theme.SystemColorMode);
        Color[] expectedPalette = [.. Enum.GetNames<AppColor>()
            .Where(name => name.StartsWith(nameof(AppColor.GraphBranch1)[..^1], StringComparison.Ordinal))
            .Select(name => Enum.Parse<AppColor>(name).GetThemeColor())
            .Where(color => !color.IsEmpty).Distinct()];
        expectedPalette.Length.Should().BeGreaterThanOrEqualTo(4);
        List<Brush> palette = ReadPalette();
        palette.Select(BrushColor).Select(color => color.ToArgb()).Should()
            .Equal(expectedPalette.Select(color => color.ToArgb()), "each theme must begin in a fresh process before the source's first graph palette use");
        RevisionGraphLaneColor.NonRelativeColor.ToArgb().Should().Be(AppColor.GraphNonRelativeBranch.GetThemeColor().ToArgb());
        TestContext.Out.WriteLine($"graphBitmapIsolatedSettings={_settingsDirectory}");
        TestContext.Out.WriteLine($"graphBitmapProcessTheme={_themeName}");
    }

    /// <summary>
    ///  Restores process state without saving or deleting any user-owned settings or palette.
    /// </summary>
    [OneTimeTearDown]
    public void Restore_process_theme_and_dispose_owned_settings()
    {
        if (_originalThemeSettings is not null)
        {
            SetThemeSettings(_originalThemeSettings);
        }

        Application.SetColorMode(_originalColorMode);
        _cache?.Dispose();
    }

    private static IEnumerable<TestCaseData> NodeCases()
    {
        foreach (bool square in new[] { false, true })
        {
            foreach (bool head in new[] { false, true })
            {
                foreach (int rowHeight in new[] { 13, 23, 24, 26 })
                {
                    foreach (Point origin in new[] { Point.Empty, new Point(6, CanvasPadding) })
                    {
                        foreach (bool clipped in new[] { false, true })
                        {
                            yield return new TestCaseData(square, head, rowHeight, origin.X, origin.Y, clipped);
                        }
                    }
                }
            }
        }
    }

    private static IEnumerable<TestCaseData> TopologyCases()
    {
        foreach (string topology in new[] { "straight", "fork", "merge", "octopus", "crossing", "shared-parent" })
        {
            foreach (bool diagonals in new[] { false, true })
            {
                foreach (bool merged in new[] { false, true })
                {
                    foreach (string presentation in new[] { "normal", "nonrelatives-gray", "highlight-selected", "hover-first", "hover-last", "hover-empty" })
                    {
                        foreach (int rowHeight in new[] { 23, 26 })
                        {
                            yield return new TestCaseData(topology, diagonals, merged, presentation, rowHeight);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    ///  Verifies every original node pixel against its independent GDI shape and outline formula.
    /// </summary>
    [TestCaseSource(nameof(NodeCases))]
    public void DrawItem_should_preserve_native_square_circle_head_origin_and_clip_pixels(
        bool square, bool head, int rowHeight, int originX, int originY, bool clipped)
    {
        WithIsolatedSettings(() =>
        {
            Configure(diagonals: false, merged: false);
            RevisionGraphRevision node = new(NodeId, guessScore: 1)
            {
                GitRevision = new GitRevision(NodeId) { Refs = square ? [Substitute.For<IGitRef>()] : [] },
            };
            IRevisionGraphRow row = new RevisionGraphRow(node, [], mergeGraphLanesHavingCommonParent: false);
            Func<int, IRevisionGraphRow?> rows = index => index == 0 ? row : null;
            object config = ReadConfig(new RevisionGraph());
            Point origin = new(originX, originY);
            Rectangle rowClip = new(originX, originY, CanvasWidth - originX, rowHeight);
            Rectangle clip = clipped ? Rectangle.Intersect(rowClip, new Rectangle(originX + 5, originY + 2, 9, Math.Max(1, rowHeight - 4))) : rowClip;
            Size size = new(CanvasWidth, originY + rowHeight + CanvasPadding);
            using Bitmap actual = CreateSourceBitmap(size);
            using Bitmap expected = CreateSourceBitmap(size);
            using (Graphics graphics = CreateSourceGraphics(actual))
            {
                graphics.SetClip(clip);
                graphics.RenderingOrigin = origin;
                InvokeOriginal(config, graphics, 0, rowHeight, rows, RevisionGraphDrawStyle.Normal, head ? NodeId : OtherId, null);
            }

            Rectangle nodeBounds = new(originX + (LaneWidth / 2) - (NodeDimension / 2), originY + (rowHeight / 2) - (NodeDimension / 2), NodeDimension, NodeDimension);
            using (Graphics graphics = CreateSourceGraphics(expected))
            {
                graphics.SetClip(clip);
                graphics.RenderingOrigin = origin;
                graphics.Clear(Color.Transparent);
                using Brush brush = new SolidBrush(RevisionGraphLaneColor.NonRelativeColor);
                graphics.SmoothingMode = square ? SmoothingMode.None : SmoothingMode.AntiAlias;
                if (square)
                {
                    graphics.FillRectangle(brush, nodeBounds);
                }
                else
                {
                    graphics.FillEllipse(brush, nodeBounds);
                }

                if (head)
                {
                    Rectangle outline = nodeBounds;
                    outline.Inflate(1, 1);
                    using Pen pen = new(SystemColors.WindowText, 2);
                    if (square)
                    {
                        graphics.DrawRectangle(pen, outline);
                    }
                    else
                    {
                        graphics.DrawEllipse(pen, outline);
                    }
                }
            }

            string directory = BitmapPaintProbe.CreateDirectory();
            actual.Save(Path.Combine(directory, "original-graph.png"), ImageFormat.Png);
            expected.Save(Path.Combine(directory, "independent-node-formula.png"), ImageFormat.Png);
            SaveComposites(directory, actual);
            BitmapPaintProbe.WriteReport(directory, new
            {
                kind = "node",
                theme = _themeName,
                square,
                head,
                rowHeight,
                origin,
                clip,
                nodeBounds,
                sourceFill = BitmapPaintProbe.Argb(RevisionGraphLaneColor.NonRelativeColor),
                sourceOutline = BitmapPaintProbe.Argb(SystemColors.WindowText),
                sourceContext = DescribeContext(),
                actual = DescribeBitmap(actual),
                expected = DescribeBitmapSummary(expected),
                captureMethod = "originalGraphRendererOwnedPArgbBitmap",
                scope = "actual source DrawItem and independent owned-bitmap GDI node formula; no controls, HWND, screen pixels, desktop input or full-window parity",
            });
            BitmapPaintProbe.AssertPixels(actual, expected);
        });
    }

    /// <summary>
    ///  Retains actual source topology pixels and checks per-row origin, order, clipping and resolved palette invariants.
    /// </summary>
    [TestCaseSource(nameof(TopologyCases))]
    public void DrawItem_should_preserve_actual_topology_hover_gray_modes_and_independent_cached_rows(
        string topology, bool diagonals, bool merged, string presentation, int rowHeight)
    {
        WithIsolatedSettings(() =>
        {
            Configure(diagonals, merged);
            int[][] parents = Parents(topology);
            RevisionGraph graph = new() { HeadId = NodeId };
            for (int index = 0; index < parents.Length; index++)
            {
                graph.Add(new GitRevision(Id(index + 1))
                {
                    ParentIds = [.. parents[index].Select(parent => Id(parent + 1))],
                    Refs = index == 0 || index == parents.Length - 1 ? [Substitute.For<IGitRef>()] : [],
                });
            }

            graph.LoadingCompleted();
            graph.CacheTo(graph.Count - 1, graph.Count - 1);
            graph.Count.Should().Be(parents.Length);
            graph.GetCachedCount().Should().Be(parents.Length);
            object config = ReadConfig(graph);
            IReadOnlySet<ObjectId>? hovered = presentation switch
            {
                "hover-first" => new HashSet<ObjectId> { NodeId },
                "hover-last" => new HashSet<ObjectId> { Id(parents.Length) },
                "hover-empty" => new HashSet<ObjectId>(),
                _ => null,
            };
            RevisionGraphDrawStyle style = presentation switch
            {
                "nonrelatives-gray" => RevisionGraphDrawStyle.DrawNonRelativesGray,
                "highlight-selected" => RevisionGraphDrawStyle.HighlightSelected,
                _ => RevisionGraphDrawStyle.Normal,
            };
            int left = 6;
            Size size = new(CanvasWidth, (graph.Count * rowHeight) + (CanvasPadding * 2));
            using Bitmap actual = CreateSourceBitmap(size);
            using Bitmap reverse = CreateSourceBitmap(size);
            using Bitmap clipped = CreateSourceBitmap(size);
            PaintRows(actual, graph, config, rowHeight, left, style, hovered, reverse: false, clipped: false);
            PaintRows(reverse, graph, config, rowHeight, left, style, hovered, reverse: true, clipped: false);
            PaintRows(clipped, graph, config, rowHeight, left, style, hovered, reverse: false, clipped: true);
            string directory = BitmapPaintProbe.CreateDirectory();
            actual.Save(Path.Combine(directory, "original-graph.png"), ImageFormat.Png);
            reverse.Save(Path.Combine(directory, "original-reverse-cache-order.png"), ImageFormat.Png);
            clipped.Save(Path.Combine(directory, "original-clipped-graph.png"), ImageFormat.Png);
            SaveComposites(directory, actual);
            BitmapPaintProbe.WriteReport(directory, new
            {
                kind = "topology",
                theme = _themeName,
                topology,
                diagonals,
                merged,
                presentation,
                rowHeight,
                left,
                top = CanvasPadding,
                parentIndexes = parents,
                sourceConfig = DescribeConfig(config),
                style = style.ToString(),
                hoveredIds = hovered?.Select(id => id.ToString()).Order(StringComparer.Ordinal).ToArray(),
                rows = Enumerable.Range(0, graph.Count).Select(index => DescribeRow(graph, index)).ToArray(),
                sourceContext = DescribeContext(),
                actual = DescribeBitmap(actual),
                reverse = DescribeBitmapSummary(reverse),
                clipped = DescribeBitmapSummary(clipped),
                captureMethod = "originalGraphRendererOwnedPArgbBitmap",
                scope = "actual original public RevisionGraph model plus private static DrawItem only; metamorphic cache-order/clip checks are not an independent topology algorithm or cross-backend equality claim",
            });
            BitmapPaintProbe.AssertPixels(actual, reverse);
            for (int row = 0; row < graph.Count; row++)
            {
                Rectangle clip = RowClip(row, rowHeight, left, clipped: true);
                for (int y = CanvasPadding + (row * rowHeight); y < CanvasPadding + ((row + 1) * rowHeight); y++)
                {
                    for (int x = 0; x < actual.Width; x++)
                    {
                        int expected = clip.Contains(x, y) ? actual.GetPixel(x, y).ToArgb() : Color.FromArgb(0).ToArgb();
                        clipped.GetPixel(x, y).ToArgb().Should().Be(expected, $"source cache clip at ({x},{y})");
                    }
                }
            }

            actual.GetPixel(left + (LaneWidth / 2), CanvasPadding + (rowHeight / 2)).A.Should().Be(255);
        });
    }

    private static IEnumerable<TestCaseData> AlphaCases()
    {
        foreach ((string role, Color color) in new[]
        {
            ("black", Color.Black),
            ("white", Color.White),
            ("red", Color.Red),
            ("green", Color.Lime),
            ("blue", Color.Blue),
            ("asymmetric-low", Color.FromArgb(1, 17, 239)),
        })
        {
            foreach (bool head in new[] { false, true })
            {
                yield return new TestCaseData(role, color.R, color.G, color.B, head);
            }
        }
    }

    /// <summary>
    ///  Verifies every controlled source-brush alpha using independent primitives and raw premultiplied pixels.
    /// </summary>
    [TestCaseSource(nameof(AlphaCases))]
    public void DrawItem_should_preserve_every_brush_alpha_channel_and_source_head_composition(
        string colorRole, byte red, byte green, byte blue, bool head)
    {
        WithIsolatedSettings(() =>
        {
            Configure(diagonals: false, merged: false);
            SolidBrush sourceBrush = typeof(RevisionGraphLaneColor).GetProperty("NonRelativeBrush", PrivateStatic)?.GetValue(null) as SolidBrush
                ?? throw new MissingMemberException(typeof(RevisionGraphLaneColor).FullName, "NonRelativeBrush");
            Color previousBrushColor = sourceBrush.Color;
            RevisionGraphRevision revision = new(NodeId, guessScore: 1)
            {
                GitRevision = new GitRevision(NodeId) { Refs = [] },
            };
            RevisionGraphRow row = new(revision, [], mergeGraphLanesHavingCommonParent: false);
            Func<int, IRevisionGraphRow?> rows = index => index == 0 ? row : null;
            object config = ReadConfig(new RevisionGraph());
            string directory = BitmapPaintProbe.CreateDirectory();
            List<object> samples = [];
            List<string> differences = [];
            int[] retainedAlphas = [0, 1, 127, 128, 254, 255];
            try
            {
                for (int alpha = 0; alpha <= byte.MaxValue; alpha++)
                {
                    Color controlledFill = Color.FromArgb(alpha, red, green, blue);
                    sourceBrush.Color = controlledFill;
                    using Bitmap actual = CreateSourceBitmap(new Size(24, 24));
                    using Bitmap expected = CreateSourceBitmap(new Size(24, 24));
                    using (Graphics graphics = CreateSourceGraphics(actual))
                    {
                        graphics.SetClip(new Rectangle(Point.Empty, actual.Size));
                        graphics.RenderingOrigin = Point.Empty;
                        InvokeOriginal(config, graphics, 0, 24, rows, RevisionGraphDrawStyle.Normal, head ? NodeId : OtherId, null);
                    }

                    using (Graphics graphics = CreateSourceGraphics(expected))
                    {
                        graphics.Clear(Color.Transparent);
                        using Brush fill = new SolidBrush(controlledFill);
                        Rectangle bounds = new(3, 7, 10, 10);
                        graphics.FillEllipse(fill, bounds);
                        if (head)
                        {
                            bounds.Inflate(1, 1);
                            using Pen outline = new(SystemColors.WindowText, 2);
                            graphics.DrawEllipse(outline, bounds);
                        }
                    }

                    byte[] actualRaw = ReadRawPremultiplied(actual);
                    byte[] expectedRaw = ReadRawPremultiplied(expected);
                    string actualHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(actualRaw));
                    string expectedHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(expectedRaw));
                    if (!actualRaw.AsSpan().SequenceEqual(expectedRaw))
                    {
                        differences.Add($"alpha={alpha}, actual={actualHash}, expected={expectedHash}");
                    }

                    if (retainedAlphas.Contains(alpha))
                    {
                        actual.Save(Path.Combine(directory, $"original-alpha-{alpha:D3}.png"), ImageFormat.Png);
                    }

                    samples.Add(new
                    {
                        alpha,
                        controlledFillArgb = BitmapPaintProbe.Argb(controlledFill),
                        actualSourceBrushArgb = BitmapPaintProbe.Argb(sourceBrush.Color),
                        rawPixelHash = actualHash,
                        independentRawPixelHash = expectedHash,
                        premultipliedArgbRuns = Enumerable.Range(0, actual.Height).Select(y => EncodeRuns(Enumerable.Range(0, actual.Width)
                            .Select(x => BitConverter.ToUInt32(actualRaw, ((y * actual.Width) + x) * sizeof(int))))).ToArray(),
                    });
                }
            }
            finally
            {
                // Mutate only this existing brush's input value; never replace, reset or dispose the source cache.
                sourceBrush.Color = previousBrushColor;
            }

            BitmapPaintProbe.WriteReport(directory, new
            {
                kind = "graph-node-alpha-series",
                theme = _themeName,
                colorRole,
                red,
                green,
                blue,
                head,
                width = 24,
                height = 24,
                rowHeight = 24,
                origin = Point.Empty,
                clip = new Rectangle(0, 0, 24, 24),
                nodeBounds = new Rectangle(3, 7, 10, 10),
                sourceOutlineArgb = BitmapPaintProbe.Argb(SystemColors.WindowText),
                originalCachedBrushArgb = BitmapPaintProbe.Argb(previousBrushColor),
                restoredCachedBrushArgb = BitmapPaintProbe.Argb(sourceBrush.Color),
                samples,
                sampleCount = samples.Count,
                retainedPngAlphas = retainedAlphas,
                differences,
                sourceContext = DescribeContext(),
                rawHashEncoding = "SHA256 over width*height*4 native row-major BGRA premultiplied bytes without stride padding",
                runEncoding = "count:AARRGGBB;count:AARRGGBB; each raw premultiplied row expands to width pixels",
                captureMethod = "originalGraphRendererOwnedPArgbBitmapAlphaSeries",
                scope = "controlled input on the existing original NonRelativeBrush plus independent GDI primitive/composition proof; not real theme-loading or UI state evidence; restores Color in finally without resetting or disposing the source palette",
            });
            TestContext.Out.WriteLine($"nativeGraphAlphaSamples={samples.Count}; actualNativeTestCases=1; directory={directory}");
            sourceBrush.Color.ToArgb().Should().Be(previousBrushColor.ToArgb());
            differences.Should().BeEmpty("the original and independent primitive must agree in raw premultiplied memory for every source brush alpha");
        });
    }

    /// <summary>
    ///  Distinguishes the original raw system-color outline from configurable theme WindowText overrides.
    /// </summary>
    [TestCase(false)]
    [TestCase(true)]
    public void DrawItem_should_keep_raw_system_head_outline_when_css_window_text_changes(bool square)
    {
        WithIsolatedSettings(() =>
        {
            Configure(diagonals: false, merged: false);
            ThemeSettings previous = ThemeModule.Settings;
            SystemColorMode mode = Application.ColorMode;
            Color rawOutline = SystemColors.WindowText;
            Color coldOverride = Color.FromArgb(255, 181, 17, 203);
            Color liveOverride = Color.FromArgb(255, 13, 197, 83);
            rawOutline.ToArgb().Should().NotBe(coldOverride.ToArgb());
            rawOutline.ToArgb().Should().NotBe(liveOverride.ToArgb());
            RevisionGraphRevision revision = new(NodeId, guessScore: 1)
            {
                GitRevision = new GitRevision(NodeId) { Refs = square ? [Substitute.For<IGitRef>()] : [] },
            };
            RevisionGraphRow row = new(revision, [], mergeGraphLanesHavingCommonParent: false);
            Func<int, IRevisionGraphRow?> rows = index => index == 0 ? row : null;
            object config = ReadConfig(new RevisionGraph());
            string directory = BitmapPaintProbe.CreateDirectory();
            List<object> states = [];
            using Bitmap before = CreateSourceBitmap(new Size(24, 24));
            try
            {
                using (Graphics graphics = CreateSourceGraphics(before))
                {
                    graphics.SetClip(new Rectangle(Point.Empty, before.Size));
                    graphics.RenderingOrigin = Point.Empty;
                    InvokeOriginal(config, graphics, 0, 24, rows, RevisionGraphDrawStyle.Normal, NodeId, null);
                }

                foreach ((string name, Color? overrideColor) in new[]
                {
                    ("before", (Color?)null),
                    ("cold-css-override", (Color?)coldOverride),
                    ("live-css-override", (Color?)liveOverride),
                })
                {
                    if (overrideColor is { } color)
                    {
                        IThemeSerializationData original = previous.Theme;
                        Dictionary<KnownColor, Color> systemColors = new(original.SysColorValues) { [KnownColor.WindowText] = color };
                        Theme alteredTheme = new(new Dictionary<AppColor, Color>(original.AppColorValues), systemColors, previous.Theme.Id);
                        SetThemeSettings(new ThemeSettings(alteredTheme, previous.InvariantTheme, previous.Variations, previous.UseSystemVisualStyle));
                        ThemeModule.Settings.Theme.GetColor(KnownColor.WindowText).ToArgb().Should().Be(color.ToArgb());
                    }

                    using Bitmap actual = CreateSourceBitmap(new Size(24, 24));
                    using (Graphics graphics = CreateSourceGraphics(actual))
                    {
                        graphics.SetClip(new Rectangle(Point.Empty, actual.Size));
                        graphics.RenderingOrigin = Point.Empty;
                        InvokeOriginal(config, graphics, 0, 24, rows, RevisionGraphDrawStyle.Normal, NodeId, null);
                    }

                    actual.Save(Path.Combine(directory, $"original-{name}.png"), ImageFormat.Png);
                    states.Add(new
                    {
                        name,
                        themeWindowTextArgb = BitmapPaintProbe.Argb(ThemeModule.Settings.Theme.GetColor(KnownColor.WindowText)),
                        rawSystemWindowTextArgb = BitmapPaintProbe.Argb(SystemColors.WindowText),
                        sourceColorMode = Application.ColorMode.ToString(),
                        actual = DescribeBitmap(actual),
                    });
                    Application.ColorMode.Should().Be(mode);
                    SystemColors.WindowText.ToArgb().Should().Be(rawOutline.ToArgb());
                    BitmapPaintProbe.AssertPixels(actual, before);
                }
            }
            finally
            {
                SetThemeSettings(previous);
            }

            BitmapPaintProbe.WriteReport(directory, new
            {
                kind = "outline-palette-override",
                theme = _themeName,
                square,
                head = true,
                rowHeight = 24,
                sourceOutlineArgb = BitmapPaintProbe.Argb(rawOutline),
                coldOverrideArgb = BitmapPaintProbe.Argb(coldOverride),
                liveOverrideArgb = BitmapPaintProbe.Argb(liveOverride),
                states,
                sourceContext = DescribeContext(),
                captureMethod = "originalGraphRendererOwnedPArgbBitmap",
                scope = "original raw SystemColors.WindowText outline with process ColorMode held unchanged while ThemeModule.Settings CSS role changes; existing palette/brushes are neither reset nor replaced; not a full theme-load/runtime claim",
            });
            ReferenceEquals(ThemeModule.Settings, previous).Should().BeTrue();
        });
    }

    private static byte[] ReadRawPremultiplied(Bitmap bitmap)
    {
        BitmapData data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        try
        {
            int rowBytes = bitmap.Width * sizeof(int);
            byte[] result = new byte[rowBytes * bitmap.Height];
            for (int row = 0; row < bitmap.Height; row++)
            {
                Marshal.Copy(data.Scan0 + (row * data.Stride), result, row * rowBytes, rowBytes);
            }

            return result;
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
    }

    private static IEnumerable<TestCaseData> LayeredCases()
    {
        foreach (string colorRole in new[] { "low-gray", "asymmetric-low", "configured-first-lane" })
        {
            foreach (bool head in new[] { false, true })
            {
                yield return new TestCaseData(colorRole, head);
            }
        }
    }

    /// <summary>
    ///  Preserves original primitive order where a detached-HEAD circle paints over its incoming and outgoing lane.
    /// </summary>
    [TestCaseSource(nameof(LayeredCases))]
    public void DrawItem_should_preserve_incoming_lane_fill_and_head_primitive_order(string colorRole, bool head)
    {
        WithIsolatedSettings(() =>
        {
            Configure(diagonals: false, merged: false);
            List<Brush> palette = ReadPalette();
            Brush[] previousBrushes = [.. palette];
            Color[] previousColors = [.. previousBrushes.Select(BrushColor)];
            Brush nonRelativeBrush = ReadNonRelativeBrush();
            Color nonRelativeColor = BrushColor(nonRelativeBrush);
            Color controlledFill = colorRole switch
            {
                "low-gray" => Color.FromArgb(2, 2, 2),
                "asymmetric-low" => Color.FromArgb(1, 17, 239),
                "configured-first-lane" => Enum.GetNames<AppColor>()
                    .Where(name => name.StartsWith(nameof(AppColor.GraphBranch1)[..^1], StringComparison.Ordinal))
                    .Select(name => Enum.Parse<AppColor>(name).GetThemeColor())
                    .First(color => !color.IsEmpty),
                _ => throw new ArgumentOutOfRangeException(nameof(colorRole)),
            };
            RevisionGraphRevision upper = new(Id(1), guessScore: 1) { IsRelative = true, GitRevision = new GitRevision(Id(1)) { Refs = [] } };
            RevisionGraphRevision current = new(Id(2), guessScore: 2) { IsRelative = true, GitRevision = new GitRevision(Id(2)) { Refs = [] } };
            RevisionGraphRevision lower = new(Id(3), guessScore: 3) { IsRelative = true, GitRevision = new GitRevision(Id(3)) { Refs = [] } };
            RevisionGraphSegment incoming = new(current, upper);
            incoming.LaneInfo = new LaneInfo(incoming, segmentToTheLeft: null);
            RevisionGraphSegment outgoing = new(lower, current);
            outgoing.LaneInfo = new LaneInfo(outgoing, segmentToTheLeft: null);
            IRevisionGraphRow[] rows =
            [
                new RevisionGraphRow(upper, [incoming], mergeGraphLanesHavingCommonParent: false),
                new RevisionGraphRow(current, [incoming, outgoing], mergeGraphLanesHavingCommonParent: false),
                new RevisionGraphRow(lower, [outgoing], mergeGraphLanesHavingCommonParent: false),
            ];
            Func<int, IRevisionGraphRow?> getRow = index => index >= 0 && index < rows.Length ? rows[index] : null;
            object config = ReadConfig(new RevisionGraph());
            using Bitmap actual = CreateSourceBitmap(new Size(24, 24));
            using Bitmap expected = CreateSourceBitmap(new Size(24, 24));
            using Bitmap precomposed = CreateSourceBitmap(new Size(24, 24));
            using SolidBrush controlledBrush = new(controlledFill);
            try
            {
                for (int index = 0; index < palette.Count; index++)
                {
                    palette[index] = controlledBrush;
                }

                using (Graphics graphics = CreateSourceGraphics(actual))
                {
                    graphics.SetClip(new Rectangle(Point.Empty, actual.Size));
                    graphics.RenderingOrigin = Point.Empty;
                    InvokeOriginal(config, graphics, 1, 24, getRow, RevisionGraphDrawStyle.Normal, head ? current.Objectid : OtherId, null);
                }

                using (Graphics graphics = CreateSourceGraphics(expected))
                {
                    graphics.Clear(Color.Transparent);
                    using Pen lane = new(controlledFill, 2);
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.DrawLine(lane, 8, 12, 8, 36);
                    graphics.DrawLine(lane, 8, -12, 8, 12);
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using Brush fill = new SolidBrush(controlledFill);
                    graphics.FillEllipse(fill, 3, 7, 10, 10);
                    if (head)
                    {
                        using Pen outline = new(SystemColors.WindowText, 2);
                        graphics.DrawEllipse(outline, 2, 6, 12, 12);
                    }
                }

                using Bitmap tile = CreateSourceBitmap(new Size(24, 24));
                using (Graphics graphics = CreateSourceGraphics(tile))
                {
                    using Brush fill = new SolidBrush(controlledFill);
                    graphics.FillEllipse(fill, 3, 7, 10, 10);
                    if (head)
                    {
                        using Pen outline = new(SystemColors.WindowText, 2);
                        graphics.DrawEllipse(outline, 2, 6, 12, 12);
                    }
                }

                using (Graphics graphics = CreateSourceGraphics(precomposed))
                {
                    using Pen lane = new(controlledFill, 2);
                    graphics.SmoothingMode = SmoothingMode.None;
                    graphics.DrawLine(lane, 8, 12, 8, 36);
                    graphics.DrawLine(lane, 8, -12, 8, 12);
                    graphics.DrawImageUnscaled(tile, Point.Empty);
                }
            }
            finally
            {
                for (int index = 0; index < previousBrushes.Length; index++)
                {
                    palette[index] = previousBrushes[index];
                }
            }

            string directory = BitmapPaintProbe.CreateDirectory();
            actual.Save(Path.Combine(directory, "original-layered-graph.png"), ImageFormat.Png);
            precomposed.Save(Path.Combine(directory, "independent-precomposed-node-over-lane.png"), ImageFormat.Png);
            BitmapPaintProbe.WriteReport(directory, new
            {
                kind = "layered-node",
                theme = _themeName,
                colorRole,
                head,
                square = false,
                sourceFillArgb = BitmapPaintProbe.Argb(controlledFill),
                sourceLaneArgb = BitmapPaintProbe.Argb(controlledFill),
                sourceOutlineArgb = BitmapPaintProbe.Argb(SystemColors.WindowText),
                originalPaletteArgb = previousColors.Select(BitmapPaintProbe.Argb).ToArray(),
                restoredPaletteArgb = palette.Select(BrushColor).Select(BitmapPaintProbe.Argb).ToArray(),
                paletteListReferenceRetained = ReferenceEquals(ReadPalette(), palette),
                paletteBrushReferencesRestored = palette.Select((brush, index) => ReferenceEquals(brush, previousBrushes[index])).ToArray(),
                originalNonRelativeArgb = BitmapPaintProbe.Argb(nonRelativeColor),
                restoredNonRelativeArgb = BitmapPaintProbe.Argb(BrushColor(ReadNonRelativeBrush())),
                nonRelativeBrushReferenceRetained = ReferenceEquals(ReadNonRelativeBrush(), nonRelativeBrush),
                rowHeight = 24,
                origin = Point.Empty,
                clip = new Rectangle(0, 0, 24, 24),
                nodeBounds = new Rectangle(3, 7, 10, 10),
                style = RevisionGraphDrawStyle.Normal.ToString(),
                relativeRows = rows.Select(row => row.Revision.IsRelative).ToArray(),
                primitiveOrder = new[] { "outgoing vertical lane", "incoming vertical lane", "circle fill", head ? "raw WindowText HEAD outline" : "no HEAD outline" },
                actual = DescribeBitmap(actual),
                expected = DescribeBitmapSummary(expected),
                precomposed = DescribeBitmap(precomposed),
                diagnosticPoint = new
                {
                    X = 7,
                    Y = 7,
                    sequential = BitmapPaintProbe.Argb(actual.GetPixel(7, 7)),
                    precomposed = BitmapPaintProbe.Argb(precomposed.GetPixel(7, 7)),
                },
                sourceContext = DescribeContext(),
                captureMethod = "originalGraphRendererOwnedPArgbBitmap",
                scope = "actual original relative middle-row DrawItem under Normal style and independent original GDI primitive order; owned controlled solid brush temporarily occupies existing palette slots, with every exact original slot reference restored; non-relative brush is never mutated; no static cache reset or native window",
            });
            BitmapPaintProbe.AssertPixels(actual, expected);
            ReadPalette().Should().BeSameAs(palette);
            palette.Should().HaveCount(previousBrushes.Length);
            palette.Select((brush, index) => ReferenceEquals(brush, previousBrushes[index])).Should().OnlyContain(same => same);
            palette.Select(BrushColor).Select(color => color.ToArgb()).Should().Equal(previousColors.Select(color => color.ToArgb()));
            ReadNonRelativeBrush().Should().BeSameAs(nonRelativeBrush);
            BrushColor(nonRelativeBrush).ToArgb().Should().Be(nonRelativeColor.ToArgb());
            if (_themeName == "light" && colorRole == "low-gray" && head)
            {
                actual.GetPixel(7, 7).ToArgb().Should().Be(Color.FromArgb(2, 2, 2).ToArgb());
                precomposed.GetPixel(7, 7).ToArgb().Should().Be(Color.FromArgb(1, 1, 1).ToArgb(),
                    "precomposing the source fill and outline changes integer rounding over an existing lane");
            }
        });
    }

    /// <summary>
    ///  Retains first-use graph brushes and a real cached lane when later theme settings expose a smaller palette.
    /// </summary>
    [TestCase(false)]
    [TestCase(true)]
    public void DrawItem_should_retain_first_use_palette_and_cached_lane_across_later_theme_changes(bool square)
    {
        WithIsolatedSettings(() =>
        {
            Configure(diagonals: false, merged: false);
            ThemeSettings previous = ThemeModule.Settings;
            SystemColorMode mode = Application.ColorMode;
            Color rawOutline = SystemColors.WindowText;
            List<Brush> palette = ReadPalette();
            Brush[] previousBrushes = [.. palette];
            Color[] previousColors = [.. previousBrushes.Select(BrushColor)];
            Brush nonRelativeBrush = ReadNonRelativeBrush();
            Color nonRelativeColor = RevisionGraphLaneColor.NonRelativeColor;
            Color nonRelativeBrushColor = BrushColor(nonRelativeBrush);
            int retainedIndex = palette.Count - 1;
            retainedIndex.Should().BeGreaterThanOrEqualTo(3);
            int[] seeds = [-1024, -17, -1, 0, 1, 17, 1024, int.MaxValue, retainedIndex, retainedIndex + palette.Count];
            int[] initialIndices = [.. seeds.Select(RevisionGraphLaneColor.GetColorForLane)];
            ObjectId retainedId = ObjectId.Parse(Convert.ToHexString(BitConverter.GetBytes(retainedIndex)) + new string('0', 32));
            RevisionGraphRevision upper = new(Id(1), guessScore: 1) { IsRelative = true, GitRevision = new GitRevision(Id(1)) { Refs = [] } };
            RevisionGraphRevision current = new(retainedId, guessScore: 2)
            {
                IsRelative = true,
                GitRevision = new GitRevision(retainedId) { Refs = square ? [Substitute.For<IGitRef>()] : [] },
            };
            RevisionGraphRevision lower = new(Id(3), guessScore: 3) { IsRelative = true, GitRevision = new GitRevision(Id(3)) { Refs = [] } };
            RevisionGraphSegment incoming = new(current, upper);
            LaneInfo retainedLane = new(incoming, segmentToTheLeft: null);
            retainedLane.Color.Should().Be(retainedIndex, "the actual source LaneInfo must retain an index beyond the later configured palette");
            incoming.LaneInfo = retainedLane;
            RevisionGraphSegment outgoing = new(lower, current) { LaneInfo = retainedLane };
            IRevisionGraphRow[] rows =
            [
                new RevisionGraphRow(upper, [incoming], mergeGraphLanesHavingCommonParent: false),
                new RevisionGraphRow(current, [incoming, outgoing], mergeGraphLanesHavingCommonParent: false),
                new RevisionGraphRow(lower, [outgoing], mergeGraphLanesHavingCommonParent: false),
            ];
            foreach (IRevisionGraphRow row in rows)
            {
                row.GetCurrentRevisionLane().Should().Be(0);
            }

            RevisionGraphRevision detached = new(NodeId, guessScore: 1)
            {
                GitRevision = new GitRevision(NodeId) { Refs = square ? [Substitute.For<IGitRef>()] : [] },
            };
            RevisionGraphRow detachedRow = new(detached, [], mergeGraphLanesHavingCommonParent: false);
            object config = ReadConfig(new RevisionGraph());
            string directory = BitmapPaintProbe.CreateDirectory();
            List<object> states = [];
            using Bitmap initialRelative = Paint(relative: true);
            using Bitmap initialNonRelative = Paint(relative: false);
            AppColor[] branchRoles = [.. Enum.GetNames<AppColor>()
                .Where(name => name.StartsWith(nameof(AppColor.GraphBranch1)[..^1], StringComparison.Ordinal))
                .Select(name => Enum.Parse<AppColor>(name))];
            IThemeSerializationData sourceTheme = previous.Theme;
            Dictionary<AppColor, Color> laterValues = new(sourceTheme.AppColorValues);
            Color replacementFirst = Color.FromArgb(1, 2, 3);
            Color replacementSecond = Color.FromArgb(5, 7, 11);
            Color replacementNonRelative = Color.FromArgb(13, 17, 19);
            Color inPlaceBranch = Color.FromArgb(37, 41, 43);
            Color inPlaceNonRelative = Color.FromArgb(53, 59, 61);
            previousColors.Select(color => color.ToArgb()).Should().NotContain(new[]
            {
                replacementFirst.ToArgb(), replacementSecond.ToArgb(), inPlaceBranch.ToArgb(),
            });
            replacementNonRelative.ToArgb().Should().NotBe(nonRelativeColor.ToArgb());
            inPlaceNonRelative.ToArgb().Should().NotBe(nonRelativeColor.ToArgb());
            Theme laterTheme = new(laterValues, new Dictionary<KnownColor, Color>(sourceTheme.SysColorValues), previous.Theme.Id);
            ThemeSettings laterSettings = new(laterTheme, previous.InvariantTheme, previous.Variations, previous.UseSystemVisualStyle);
            try
            {
                RetainState("before", initialRelative, initialNonRelative);
                foreach (string state in new[] { "replacement", "in-place" })
                {
                    for (int index = 0; index < branchRoles.Length; index++)
                    {
                        laterValues[branchRoles[index]] = state == "in-place" ? inPlaceBranch : index % 2 == 0 ? replacementFirst : replacementSecond;
                    }

                    laterValues[AppColor.GraphNonRelativeBranch] = state == "in-place" ? inPlaceNonRelative : replacementNonRelative;
                    if (state == "replacement")
                    {
                        SetThemeSettings(laterSettings);
                    }

                    ReferenceEquals(ThemeModule.Settings, laterSettings).Should().BeTrue();
                    Color[] publishedColors = [.. branchRoles.Select(role => role.GetThemeColor()).Where(color => !color.IsEmpty).Distinct()];
                    publishedColors.Should().HaveCount(state == "replacement" ? 2 : 1);
                    publishedColors.Length.Should().BeLessThan(previousBrushes.Length);
                    AppColor.GraphNonRelativeBranch.GetThemeColor().ToArgb().Should().Be(laterValues[AppColor.GraphNonRelativeBranch].ToArgb());
                    using Bitmap relative = Paint(relative: true);
                    using Bitmap nonRelative = Paint(relative: false);
                    RetainState(state, relative, nonRelative);
                    ReadRawPremultiplied(relative).Should().Equal(ReadRawPremultiplied(initialRelative));
                    ReadRawPremultiplied(nonRelative).Should().Equal(ReadRawPremultiplied(initialNonRelative));
                }
            }
            finally
            {
                SetThemeSettings(previous);
            }

            BitmapPaintProbe.WriteReport(directory, new
            {
                kind = "graph-palette-lifetime",
                theme = _themeName,
                square,
                head = true,
                rowHeight = 24,
                origin = Point.Empty,
                clip = new Rectangle(0, 0, 24, 24),
                nodeBounds = new Rectangle(3, 7, 10, 10),
                style = RevisionGraphDrawStyle.Normal.ToString(),
                initialPaletteArgb = previousColors.Select(BitmapPaintProbe.Argb).ToArray(),
                retainedLane = new
                {
                    colorIndex = retainedLane.Color,
                    startRevision = retainedLane.StartRevision.Objectid.ToString(),
                    segmentChildRevision = incoming.Child.Objectid.ToString(),
                    segmentParentRevision = incoming.Parent.Objectid.ToString(),
                    brushArgb = BitmapPaintProbe.Argb(BrushColor(RevisionGraphLaneColor.GetBrushForLane(retainedLane.Color))),
                },
                seeds,
                initialIndices,
                replacementBranchArgb = new[] { replacementFirst, replacementSecond }.Select(BitmapPaintProbe.Argb).ToArray(),
                replacementNonRelativeArgb = BitmapPaintProbe.Argb(replacementNonRelative),
                inPlaceBranchArgb = BitmapPaintProbe.Argb(inPlaceBranch),
                inPlaceNonRelativeArgb = BitmapPaintProbe.Argb(inPlaceNonRelative),
                states,
                themeSettingsReferenceRestored = ReferenceEquals(ThemeModule.Settings, previous),
                sourceContext = DescribeContext(),
                captureMethod = "originalGraphRendererOwnedPArgbBitmap",
                scope = "actual original first-use source palette and retained real high-index LaneInfo across a new smaller-palette ThemeSettings and an in-place mutation of that owned later theme dictionary; original theme dictionaries and all cached brushes stay untouched; actual relative-lane and detached non-relative HEAD pixels are compared raw before/after; no palette reset, theme-reload UI, Control or native window",
            });
            ReferenceEquals(ThemeModule.Settings, previous).Should().BeTrue();

            Bitmap Paint(bool relative)
            {
                Bitmap bitmap = CreateSourceBitmap(new Size(24, 24));
                try
                {
                    using Graphics graphics = CreateSourceGraphics(bitmap);
                    graphics.SetClip(new Rectangle(Point.Empty, bitmap.Size));
                    graphics.RenderingOrigin = Point.Empty;
                    InvokeOriginal(config, graphics, relative ? 1 : 0, 24,
                        relative ? index => index >= 0 && index < rows.Length ? rows[index] : null : index => index == 0 ? detachedRow : null,
                        RevisionGraphDrawStyle.Normal, relative ? current.Objectid : detached.Objectid, null);
                    return bitmap;
                }
                catch
                {
                    bitmap.Dispose();
                    throw;
                }
            }

            void RetainState(string name, Bitmap relative, Bitmap nonRelative)
            {
                relative.Save(Path.Combine(directory, $"original-relative-{name}.png"), ImageFormat.Png);
                nonRelative.Save(Path.Combine(directory, $"original-non-relative-{name}.png"), ImageFormat.Png);
                List<Brush> currentPalette = ReadPalette();
                bool[] sameBrushes = [.. currentPalette.Select((brush, index) => ReferenceEquals(brush, previousBrushes[index]))];
                states.Add(new
                {
                    name,
                    publishedPaletteArgb = branchRoles.Select(role => role.GetThemeColor()).Where(color => !color.IsEmpty).Distinct().Select(BitmapPaintProbe.Argb).ToArray(),
                    publishedNonRelativeArgb = BitmapPaintProbe.Argb(AppColor.GraphNonRelativeBranch.GetThemeColor()),
                    cachedPaletteArgb = currentPalette.Select(BrushColor).Select(BitmapPaintProbe.Argb).ToArray(),
                    paletteListReferenceRetained = ReferenceEquals(currentPalette, palette),
                    paletteBrushReferencesRetained = sameBrushes,
                    retainedLaneColorIndex = retainedLane.Color,
                    retainedLaneBrushReferenceRetained = ReferenceEquals(RevisionGraphLaneColor.GetBrushForLane(retainedLane.Color), previousBrushes[retainedIndex]),
                    currentIndices = seeds.Select(RevisionGraphLaneColor.GetColorForLane).ToArray(),
                    cachedNonRelativeArgb = BitmapPaintProbe.Argb(RevisionGraphLaneColor.NonRelativeColor),
                    cachedNonRelativeBrushArgb = BitmapPaintProbe.Argb(BrushColor(ReadNonRelativeBrush())),
                    nonRelativeBrushReferenceRetained = ReferenceEquals(ReadNonRelativeBrush(), nonRelativeBrush),
                    rawSystemWindowTextArgb = BitmapPaintProbe.Argb(SystemColors.WindowText),
                    sourceColorMode = Application.ColorMode.ToString(),
                    relative = DescribeBitmap(relative),
                    nonRelative = DescribeBitmap(nonRelative),
                });
                currentPalette.Should().BeSameAs(palette);
                currentPalette.Should().HaveCount(previousBrushes.Length);
                sameBrushes.Should().OnlyContain(same => same);
                currentPalette.Select(BrushColor).Select(color => color.ToArgb()).Should().Equal(previousColors.Select(color => color.ToArgb()));
                seeds.Select(RevisionGraphLaneColor.GetColorForLane).Should().Equal(initialIndices);
                retainedLane.Color.Should().Be(retainedIndex);
                retainedLane.StartRevision.Should().BeSameAs(incoming.Child);
                incoming.Child.Should().BeSameAs(upper);
                incoming.Parent.Should().BeSameAs(current);
                RevisionGraphLaneColor.GetBrushForLane(retainedLane.Color).Should().BeSameAs(previousBrushes[retainedIndex]);
                RevisionGraphLaneColor.NonRelativeColor.ToArgb().Should().Be(nonRelativeColor.ToArgb());
                ReadNonRelativeBrush().Should().BeSameAs(nonRelativeBrush);
                BrushColor(nonRelativeBrush).ToArgb().Should().Be(nonRelativeBrushColor.ToArgb());
                Application.ColorMode.Should().Be(mode);
                SystemColors.WindowText.ToArgb().Should().Be(rawOutline.ToArgb());
            }
        });
    }

    private static void PaintRows(Bitmap bitmap, RevisionGraph graph, object config, int rowHeight, int left,
        RevisionGraphDrawStyle style, IReadOnlySet<ObjectId>? hovered, bool reverse, bool clipped)
    {
        using Graphics graphics = CreateSourceGraphics(bitmap);
        IEnumerable<int> order = Enumerable.Range(0, graph.Count);
        foreach (int row in reverse ? order.Reverse() : order)
        {
            graphics.SetClip(RowClip(row, rowHeight, left, clipped));
            graphics.RenderingOrigin = new Point(left, CanvasPadding + (row * rowHeight));
            InvokeOriginal(config, graphics, row, rowHeight, graph.GetSegmentsForRow, style, graph.HeadId, hovered);
        }
    }

    private static Rectangle RowClip(int row, int rowHeight, int left, bool clipped)
    {
        int top = CanvasPadding + (row * rowHeight);
        return clipped ? new Rectangle(left + 5, top + 2, 25, rowHeight - 4) : new Rectangle(left, top, CanvasWidth - left, rowHeight);
    }

    private static object DescribeRow(RevisionGraph graph, int index)
    {
        IRevisionGraphRow row = graph.GetSegmentsForRow(index) ?? throw new AssertionException("Every source graph row must be cached.");
        return new
        {
            index,
            id = row.Revision.Objectid.ToString(),
            row.Revision.Score,
            row.Revision.IsRelative,
            lane = row.GetCurrentRevisionLane(),
            lanes = row.GetLaneCount(),
            refs = row.Revision.GitRevision?.Refs.Count,
            segments = row.Segments.Select(segment => new
            {
                parent = segment.Parent.Objectid.ToString(),
                child = segment.Child.Objectid.ToString(),
                childRelative = segment.Child.IsRelative,
                colorIndex = segment.LaneInfo?.Color,
                resolvedArgb = segment.LaneInfo is null ? null : BitmapPaintProbe.Argb(BrushColor(RevisionGraphLaneColor.GetBrushForLane(segment.LaneInfo.Color))),
                lane = row.GetLaneForSegment(segment),
            }).ToArray(),
        };
    }

    private static object DescribeConfig(object config) => config.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .ToDictionary(property => property.Name, property => property.GetValue(config));

    private object DescribeContext() => new
    {
        theme = _themeName,
        bitmapFormat = PixelFormat.Format32bppPArgb.ToString(),
        smoothingMode = SmoothingMode.AntiAlias.ToString(),
        pixelOffsetMode = PixelOffsetMode.HighQuality.ToString(),
        dpi = 96,
        dpiMode = "nativeOwnedBitmap",
        dpiQuery = "source DpiUtil screen DC GetDeviceCaps only; no screen pixel acquisition",
        settingsDirectory = _settingsDirectory,
        settingsAutoSave = false,
        palette = ReadPalette().Select(BrushColor).Select(BitmapPaintProbe.Argb).ToArray(),
        nonRelative = BitmapPaintProbe.Argb(RevisionGraphLaneColor.NonRelativeColor),
        windowText = BitmapPaintProbe.Argb(SystemColors.WindowText),
    };

    private static object DescribeBitmap(Bitmap image)
    {
        BitmapData data = image.LockBits(new Rectangle(Point.Empty, image.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppPArgb);
        string[] premultiplied;
        try
        {
            premultiplied = new string[image.Height];
            byte[] row = new byte[image.Width * sizeof(int)];
            for (int y = 0; y < image.Height; y++)
            {
                Marshal.Copy(data.Scan0 + (y * data.Stride), row, 0, row.Length);
                premultiplied[y] = EncodeRuns(Enumerable.Range(0, image.Width).Select(x => BitConverter.ToUInt32(row, x * sizeof(int))));
            }
        }
        finally
        {
            image.UnlockBits(data);
        }

        return new
        {
            image.Width,
            image.Height,
            pixelHash = BitmapPaintProbe.PixelHash(image),
            ink = BitmapPaintProbe.Ink(image, Color.FromArgb(0)),
            runEncoding = "count:AARRGGBB;count:AARRGGBB; each row expands to Width pixels",
            argbRuns = Enumerable.Range(0, image.Height).Select(y => EncodeRuns(Enumerable.Range(0, image.Width).Select(x => unchecked((uint)image.GetPixel(x, y).ToArgb())))).ToArray(),
            premultipliedArgbRuns = premultiplied,
        };
    }

    private static object DescribeBitmapSummary(Bitmap image) => new
    {
        image.Width,
        image.Height,
        pixelHash = BitmapPaintProbe.PixelHash(image),
        ink = BitmapPaintProbe.Ink(image, Color.FromArgb(0)),
    };

    private static string EncodeRuns(IEnumerable<uint> values)
    {
        List<string> runs = [];
        uint previous = 0;
        int count = 0;
        foreach (uint value in values)
        {
            if (count != 0 && value != previous)
            {
                runs.Add($"{count}:{previous:X8}");
                count = 0;
            }

            previous = value;
            count++;
        }

        if (count != 0)
        {
            runs.Add($"{count}:{previous:X8}");
        }

        return string.Join(';', runs);
    }

    private void SaveComposites(string directory, Bitmap image)
    {
        foreach ((string role, Color color) in new[]
        {
            ("black", Color.Black),
            ("white", Color.White),
            ("theme", AppColor.PanelBackground.GetThemeColor()),
        })
        {
            using Bitmap composite = BitmapPaintProbe.CreateBitmap(image.Size, color);
            using (Graphics graphics = Graphics.FromImage(composite))
            {
                graphics.DrawImage(image, 0, 0);
            }

            composite.Save(Path.Combine(directory, $"source-composite-{role}.png"), ImageFormat.Png);
        }
    }

    private static Bitmap CreateSourceBitmap(Size size)
    {
        DpiUtil.DpiX.Should().Be(96);
        DpiUtil.DpiY.Should().Be(96);
        Bitmap bitmap = new(size.Width, size.Height, PixelFormat.Format32bppPArgb);
        using Graphics graphics = CreateSourceGraphics(bitmap);
        graphics.DpiX.Should().Be(96);
        graphics.DpiY.Should().Be(96);
        graphics.Clear(Color.Transparent);
        return bitmap;
    }

    private static Graphics CreateSourceGraphics(Bitmap bitmap)
    {
        Graphics graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        return graphics;
    }

    private static void InvokeOriginal(object config, Graphics graphics, int index, int rowHeight,
        Func<int, IRevisionGraphRow?> rows, RevisionGraphDrawStyle style, ObjectId head, IReadOnlySet<ObjectId>? hovered)
    {
        Type type = typeof(RevisionGraph).Assembly.GetType("GitUI.UserControls.RevisionGrid.Graph.Rendering.GraphRenderer", throwOnError: true)
            ?? throw new TypeLoadException("GraphRenderer");
        MethodInfo method = type.GetMethod("DrawItem", BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMethodException(type.FullName, "DrawItem");
        method.Invoke(null, [config, graphics, index, rowHeight, rows, style, head, hovered]);
    }

    private static object ReadConfig(RevisionGraph graph) => typeof(RevisionGraph).GetProperty("Config", PrivateInstance)?.GetValue(graph)
        ?? throw new MissingMemberException(typeof(RevisionGraph).FullName, "Config");

    private static List<Brush> ReadPalette() => typeof(RevisionGraphLaneColor).GetField("PresetGraphBrushes", PrivateStatic)?.GetValue(null) as List<Brush>
        ?? throw new MissingFieldException(typeof(RevisionGraphLaneColor).FullName, "PresetGraphBrushes");

    private static Brush ReadNonRelativeBrush() => typeof(RevisionGraphLaneColor).GetProperty("NonRelativeBrush", PrivateStatic)?.GetValue(null) as Brush
        ?? throw new MissingMemberException(typeof(RevisionGraphLaneColor).FullName, "NonRelativeBrush");

    private static Color BrushColor(Brush brush) => brush is SolidBrush solid ? solid.Color : throw new AssertionException("The original source lane palette must use solid brushes.");

    private static void SetThemeSettings(ThemeSettings settings)
    {
        PropertyInfo property = typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings), BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMemberException(typeof(ThemeModule).FullName, nameof(ThemeModule.Settings));
        property.SetValue(null, settings);
    }

    private static void Configure(bool diagonals, bool merged)
    {
        AppSettings.RenderGraphWithDiagonals.Value = diagonals;
        AppSettings.MergeGraphLanesHavingCommonParent.Value = merged;
        AppSettings.StraightenGraphDiagonals.Value = false;
        AppSettings.StraightenGraphSegmentsLimit.Value = 50;
    }

    private void WithIsolatedSettings(Action action) => AppSettings.UsingContainer(
        _settings ?? throw new InvalidOperationException("The isolated source settings must be initialized first."), action);

    private static ObjectId Id(int number) => ObjectId.Parse(number.ToString("x40"));

    private static int[][] Parents(string topology) => topology switch
    {
        "straight" => [[1], [2], [3], []],
        "fork" => [[2], [2], [3], []],
        "merge" => [[1, 2], [3], [3], []],
        "octopus" => [[1, 2, 3], [4], [4], [4], []],
        "crossing" => [[2, 3], [3, 4], [5], [5], [5], []],
        "shared-parent" => [[1, 2, 3], [4], [4], [4], [5], []],
        _ => throw new ArgumentOutOfRangeException(nameof(topology)),
    };

    private static Theme LoadToolTheme(string name)
    {
        if (name == "light")
        {
            return Theme.CreateDefaultTheme();
        }

        string root = Path.Combine(TestContext.CurrentContext.TestDirectory, "Themes");
        IThemeCssUrlResolver resolver = Substitute.For<IThemeCssUrlResolver>();
        resolver.ResolveCssUrl(Arg.Any<string>()).Returns(call => Path.Combine(root, call.Arg<string>()));
        ThemeLoader loader = new(resolver, new ThemeFileReader());
        return loader.LoadTheme(Path.Combine(root, name == "custom" ? "parity-custom.css" : "dark.css"), new ThemeId(name), []);
    }

    private static void InitializeAppSettingsWithoutRealConfiguration(string directory)
    {
        Type settingsType = typeof(AppSettings).Assembly.GetType("GitCommands.Properties.Settings", throwOnError: true)
            ?? throw new TypeLoadException("GitCommands.Properties.Settings");
        ApplicationSettingsBase configuration = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ApplicationSettingsBase
            ?? throw new MissingMemberException(settingsType.FullName, "Default");
        _ = configuration[PortableSettingName];
        SettingsPropertyValue portable = configuration.PropertyValues[PortableSettingName]
            ?? throw new MissingMemberException(settingsType.FullName, PortableSettingName);
        object originalPortableValue = portable.PropertyValue;
        bool originalDirty = portable.IsDirty;
        FieldInfo frameworkPath = typeof(Application).GetField("s_executablePath", PrivateStatic)
            ?? throw new MissingFieldException(typeof(Application).FullName, "s_executablePath");
        object? originalFrameworkPath = frameworkPath.GetValue(null);
        string actualExecutablePath = Application.ExecutablePath;
        string isolatedExecutablePath = Path.Combine(directory, "GitExtensions.exe");
        try
        {
            // The original static constructor opens its cache before a container scope exists.
            // Alias only that executable path, without constructing a Control or touching user settings.
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
}
