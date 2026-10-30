using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
using GitUI.Theming;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;
using NSubstitute;
using SkiaSharp;
using ColorHelper = GitExtUtils.GitUI.Theming.ColorHelper;
using DrawingColor = System.Drawing.Color;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class GraphNativeMatrixTests
{
    private const int CanvasWidth = 112;
    private const int CanvasPadding = 8;
    private static readonly ObjectId NodeId = Id(1);
    private static readonly ObjectId OtherId = Id(99);

    private static IEnumerable<TestCaseData> NodeCases()
    {
        foreach (string theme in new[] { "light", "dark", "custom" })
        {
            foreach (bool square in new[] { false, true })
            {
                foreach (bool head in new[] { false, true })
                {
                    foreach (int rowHeight in new[] { 13, 23, 24, 26 })
                    {
                        foreach (PixelPoint origin in new[] { default(PixelPoint), new PixelPoint(6, CanvasPadding) })
                        {
                            foreach (bool clipped in new[] { false, true })
                            {
                                yield return new TestCaseData(theme, square, head, rowHeight, origin.X, origin.Y, clipped);
                            }
                        }
                    }
                }
            }
        }
    }

    private static IEnumerable<TestCaseData> TopologyCases()
    {
        foreach (string theme in new[] { "light", "dark", "custom" })
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
                                yield return new TestCaseData(theme, topology, diagonals, merged, presentation, rowHeight);
                            }
                        }
                    }
                }
            }
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(NodeCases))]
    public void DrawItem_should_preserve_native_node_allocation_origin_and_clip_inputs(
        string theme, bool square, bool head, int rowHeight, int originX, int originY, bool clipped)
    {
        WithSettings(() =>
        {
            using ThemeScope themeScope = new(theme);
            Configure(diagonals: false, merged: false);
            RevisionGraphRevision node = new(NodeId, guessScore: 1)
            {
                GitRevision = new GitRevision(NodeId) { Refs = square ? [Substitute.For<IGitRef>()] : [] },
            };
            RevisionGraphRow row = new(node, [], mergeGraphLanesHavingCommonParent: false);
            PixelRect rowClip = new(originX, originY, CanvasWidth - originX, rowHeight);
            PixelRect clip = clipped ? rowClip.Intersect(new PixelRect(originX + 5, originY + 2, 9, Math.Max(1, rowHeight - 4))) : rowClip;
            PixelSize size = new(CanvasWidth, originY + rowHeight + CanvasPadding);
            GraphControl control = new(new RevisionGraph().Config, index => index == 0 ? row : null, count: 1,
                rowHeight, originX, originY, RevisionGraphDrawStyle.Normal, head ? NodeId : OtherId, hovered: null,
                _ => clip, reverse: false);
            using RenderTargetBitmap frame = Render(control, size);
            using SKBitmap pixels = Decode(frame);
            PixelRect nodeBounds = new(originX + (GraphRenderer.LaneWidth / 2) - (GraphRenderer.NodeDimension / 2),
                originY + (rowHeight / 2) - (GraphRenderer.NodeDimension / 2), GraphRenderer.NodeDimension, GraphRenderer.NodeDimension);
            Retain(frame, new
            {
                kind = "node", theme, square, head, rowHeight,
                origin = new { X = originX, Y = originY }, clip, nodeBounds,
                sourceFill = Argb(RevisionGraphLaneColor.NonRelativeColor),
                sourceOutline = Argb(NativeRevisionGraphNodeRaster.GetOutlineColor()),
                sourceContext = DescribeContext(theme), actual = DescribeBitmap(frame, pixels),
            });
            AssertOutsideClipTransparent(pixels, clip);
            row.GetCurrentRevisionLane().Should().Be(0);
            if (square)
            {
                AssertNativeSquareMask(pixels, clip, nodeBounds, head);
            }
        });
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(TopologyCases))]
    public void DrawItem_should_preserve_actual_topology_hover_gray_modes_and_independent_cached_rows(
        string theme, string topology, bool diagonals, bool merged, string presentation, int rowHeight)
    {
        WithSettings(() =>
        {
            using ThemeScope themeScope = new(theme);
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
            const int left = 6;
            PixelSize size = new(CanvasWidth, (graph.Count * rowHeight) + (CanvasPadding * 2));
            using RenderTargetBitmap frame = Paint(reverse: false, clipped: false);
            using RenderTargetBitmap reverseFrame = Paint(reverse: true, clipped: false);
            using RenderTargetBitmap clippedFrame = Paint(reverse: false, clipped: true);
            using SKBitmap pixels = Decode(frame);
            using SKBitmap reversePixels = Decode(reverseFrame);
            using SKBitmap clippedPixels = Decode(clippedFrame);
            Retain(frame, new
            {
                kind = "topology", theme, topology, diagonals, merged, presentation, rowHeight,
                left, top = CanvasPadding, parentIndexes = parents,
                sourceConfig = DescribeConfig(graph.Config), style = style.ToString(),
                hoveredIds = hovered?.Select(id => id.ToString()).Order(StringComparer.Ordinal).ToArray(),
                rows = Enumerable.Range(0, graph.Count).Select(index => DescribeRow(graph, index)).ToArray(),
                sourceContext = DescribeContext(theme), actual = DescribeBitmap(frame, pixels),
            });
            Retain(reverseFrame, new
            {
                kind = "topology-reversed", theme, topology, diagonals, merged, presentation, rowHeight,
                left, top = CanvasPadding, parentIndexes = parents,
                clips = Enumerable.Range(0, graph.Count).Select(row => RowClip(row, rowHeight, left, clipped: false)).ToArray(),
                actual = DescribeBitmap(reverseFrame, reversePixels),
            }, "native-matrix-metamorphic");
            Retain(clippedFrame, new
            {
                kind = "topology-clipped", theme, topology, diagonals, merged, presentation, rowHeight,
                left, top = CanvasPadding, parentIndexes = parents,
                clips = Enumerable.Range(0, graph.Count).Select(row => RowClip(row, rowHeight, left, clipped: true)).ToArray(),
                actual = DescribeBitmap(clippedFrame, clippedPixels),
            }, "native-matrix-metamorphic");
            ReadPixels(reversePixels).Should().Equal(ReadPixels(pixels), "drawing cache rows in reverse must not change their source order or paint state");
            for (int row = 0; row < graph.Count; row++)
            {
                PixelRect clip = RowClip(row, rowHeight, left, clipped: true);
                for (int y = CanvasPadding + (row * rowHeight); y < CanvasPadding + ((row + 1) * rowHeight); y++)
                {
                    for (int x = 0; x < pixels.Width; x++)
                    {
                        SKColor expected = clip.ContainsExclusive(new PixelPoint(x, y)) ? pixels.GetPixel(x, y) : default;
                        clippedPixels.GetPixel(x, y).Should().Be(expected, "actual graph cache clip at ({0},{1})", x, y);
                    }
                }
            }

            pixels.GetPixel(left + (GraphRenderer.LaneWidth / 2), CanvasPadding + (rowHeight / 2)).Alpha.Should().Be(255);

            return;

            RenderTargetBitmap Paint(bool reverse, bool clipped)
            {
                GraphControl control = new(graph.Config, graph.GetSegmentsForRow, graph.Count, rowHeight, left, CanvasPadding,
                    style, graph.HeadId, hovered, row => RowClip(row, rowHeight, left, clipped), reverse);
                return Render(control, size);
            }
        });
    }

    private static IEnumerable<TestCaseData> AlphaCases()
    {
        foreach (string theme in new[] { "light", "dark", "custom" })
        {
            foreach ((string role, byte red, byte green, byte blue) in new (string, byte, byte, byte)[]
            {
                ("black", 0, 0, 0), ("white", 255, 255, 255), ("red", 255, 0, 0),
                ("green", 0, 255, 0), ("blue", 0, 0, 255), ("asymmetric-low", 1, 17, 239),
            })
            {
                foreach (bool head in new[] { false, true })
                {
                    yield return new TestCaseData(theme, role, red, green, blue, head);
                }
            }
        }
    }

    private static IEnumerable<TestCaseData> LayeredCases()
    {
        foreach (string theme in new[] { "light", "dark", "custom" })
        {
            foreach (string colorRole in new[] { "low-gray", "asymmetric-low", "source-first-lane" })
            {
                foreach (bool head in new[] { false, true })
                {
                    yield return new TestCaseData(theme, colorRole, head);
                }
            }
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(LayeredCases))]
    public void DrawItem_should_preserve_incoming_lane_fill_and_head_primitive_order(string theme, string colorRole, bool head)
    {
        WithSettings(() =>
        {
            using ThemeScope themeScope = new(theme);
            Configure(diagonals: false, merged: false);
            DrawingColor controlledFill = colorRole switch
            {
                "low-gray" => DrawingColor.FromArgb(2, 2, 2),
                "asymmetric-low" => DrawingColor.FromArgb(1, 17, 239),
                "source-first-lane" => BrushColor(RevisionGraphLaneColor.GetBrushForLane(0)),
                _ => throw new ArgumentOutOfRangeException(nameof(colorRole)),
            };
            RevisionGraphRevision upper = new(Id(1), guessScore: 1) { GitRevision = new GitRevision(Id(1)) { Refs = [] } };
            RevisionGraphRevision current = new(Id(2), guessScore: 2) { GitRevision = new GitRevision(Id(2)) { Refs = [] } };
            RevisionGraphRevision lower = new(Id(3), guessScore: 3) { GitRevision = new GitRevision(Id(3)) { Refs = [] } };
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
            FieldInfo brushField = typeof(RevisionGraphLaneColor).GetField("_nonRelativeBrush", BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new AssertionException("The actual portable non-relative input is missing.");
            object originalBrush = brushField.GetValue(null) ?? throw new AssertionException("The actual portable input brush is missing.");
            try
            {
                // The portable cached brush is immutable. Scope only its controlled input and
                // restore its exact reference; never reset the palette or the source's cache.
                brushField.SetValue(null, new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(controlledFill)).ToImmutable());
                RevisionGraphConfig config = new RevisionGraph().Config;
                LayeredGraphControl control = new(config, rows, head ? current.Objectid : OtherId);
                using RenderTargetBitmap frame = Render(control, new PixelSize(24, 24));
                using SKBitmap pixels = Decode(frame);
                string rawPixelHash = Convert.ToHexString(SHA256.HashData(CopyPremultipliedPixels(frame)));
                Retain(frame, new
                {
                    kind = "layered-node", theme, colorRole, head, square = false,
                    sourceFillArgb = Argb(controlledFill), sourceLaneArgb = Argb(controlledFill),
                    sourceOutlineArgb = Argb(NativeRevisionGraphNodeRaster.GetOutlineColor()),
                    rowHeight = 24, origin = new { X = 0, Y = 0 }, clip = new PixelRect(0, 0, 24, 24), nodeBounds = new PixelRect(3, 7, 10, 10),
                    style = RevisionGraphDrawStyle.DrawNonRelativesGray.ToString(),
                    primitiveOrder = new[] { "outgoing vertical lane", "incoming vertical lane", "circle fill", head ? "raw WindowText HEAD outline" : "no HEAD outline" },
                    rawPixelHash, sourceContext = DescribeContext(theme), actual = DescribeBitmap(frame, pixels),
                });
                rawPixelHash.Should().Be(NativeLayeredHash(theme, colorRole, head),
                    "the complete actual middle-row BGRA frame must match the recorded original sequence over its incoming and outgoing lane");
                rows[1].GetCurrentRevisionLane().Should().Be(0);
                pixels.GetPixel(8, 0).Should().Be(Pixel(controlledFill));
                pixels.GetPixel(8, 12).Should().Be(Pixel(controlledFill));
                if (theme == "light" && colorRole == "low-gray")
                {
                    byte expected = head ? (byte)2 : (byte)3;
                    pixels.GetPixel(7, 7).Should().Be(new SKColor(expected, expected, expected),
                        "the source rounds the fill over its incoming lane to three, then HEAD to two; flattening the node first changes this sequence");
                }
            }
            finally
            {
                brushField.SetValue(null, originalBrush);
            }

            ReferenceEquals(brushField.GetValue(null), originalBrush).Should().BeTrue();
        });
    }

    // Original DrawItem native96 AA/HQ raw24x24 BGRA frames, 2026-10-30, including actual
    // incoming/outgoing segments. The custom first-lane input F06595 differs from dark DB5B93.
    private static string NativeLayeredHash(string theme, string role, bool head) => (role, head, theme) switch
    {
        ("low-gray", false, _) => "9FDA2909A106A3D0877D794359B19902B8E755DC929F732831A390894C600BAC",
        ("asymmetric-low", false, _) => "ADFD79B1F6237157FE113964D0A98A245EBDCB31DFB4C62DB11EF7B964C0CE29",
        ("low-gray", true, "light") => "BDE79170BD459D729C83ABD365EED913DF30E9FEE4E65E6A48EE7D71D301C176",
        ("low-gray", true, _) => "85B13667C03FC5B997E14F30748C3BD7C9CBF3FE19AABC708965B7FD680DB35D",
        ("asymmetric-low", true, "light") => "230660ED7E09B111D292D5F52D0C9CCB2750A0D70AAD85005C309B55BE0D66BB",
        ("asymmetric-low", true, _) => "13F906A7E30BC184138B13F5A1EEF6DCE3B3F295078BE78E5DF2EE5EDD21214C",
        ("source-first-lane", false, "light") => "1D16A62EC47E3E020FBEC5A683FEE03BC276293943A2A6EC28DDFF38E1C9D759",
        ("source-first-lane", true, "light") => "4902A4891936084753777ED806DA274D6BD2871F44AE68D6453DCBFB6D55068E",
        ("source-first-lane", false, "dark") => "10CEA1977A330BBAA5D5121BD8DAC3876EBAF6C49398BE14D98486348B76A1C9",
        ("source-first-lane", true, "dark") => "91F60F042A5D02873953E411ED627BDC33F62F2045368B14919471689C89DA8F",
        ("source-first-lane", false, "custom") => "5FC891103250EF507C704579709D42211B83066547056B78BAC006BA0D29BD6B",
        ("source-first-lane", true, "custom") => "C0A126BAF94DE4BF40CF4F16530E1BC5E75E1E5AB2300D4E9F54D9B85780E68C",
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };

    [AvaloniaTest]
    [TestCaseSource(nameof(AlphaCases))]
    public void Native_node_adapter_should_match_every_original_brush_alpha_and_head_composition(
        string theme, string colorRole, byte red, byte green, byte blue, bool head)
    {
        using ThemeScope themeScope = new(theme);
        using IncrementalHash aggregate = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        List<object> samples = [];
        for (int alpha = 0; alpha <= byte.MaxValue; alpha++)
        {
            Avalonia.Media.Color fill = Avalonia.Media.Color.FromArgb((byte)alpha, red, green, blue);
            NativeNodeControl control = new(new SolidColorBrush(fill), new Rect(3, 7, 10, 10), head);
            using RenderTargetBitmap frame = Render(control, new PixelSize(24, 24));
            control.UsedNativeOperation.Should().BeTrue();
            byte[] raw = CopyPremultipliedPixels(frame);
            aggregate.AppendData(raw);
            samples.Add(new
            {
                alpha,
                rawPixelHash = Convert.ToHexString(SHA256.HashData(raw)),
                premultipliedArgbRuns = Enumerable.Range(0, 24).Select(y => EncodeRuns(Enumerable.Range(0, 24)
                    .Select(x => BitConverter.ToUInt32(raw, ((y * 24) + x) * sizeof(int))))).ToArray(),
            });
        }

        string actualHash = Convert.ToHexString(aggregate.GetHashAndReset());
        RetainAlpha(new
        {
            kind = "graph-node-alpha-series", theme, colorRole, red, green, blue, head,
            width = 24, height = 24, rowHeight = 24,
            origin = new { X = 0, Y = 0 }, clip = new PixelRect(0, 0, 24, 24), nodeBounds = new PixelRect(3, 7, 10, 10),
            sourceOutlineArgb = Argb(NativeRevisionGraphNodeRaster.GetOutlineColor()),
            aggregateRawHash = actualHash, sampleCount = samples.Count, samples,
            rawHashEncoding = "SHA256 over row-major BGRA premultiplied bytes without stride padding; aggregate concatenates all256 actual rawframes in alpha order",
            sourceContext = DescribeContext(theme),
        });
        actualHash.Should().Be(NativeAlphaHash(theme, colorRole, head),
            "the recorded native96 GDI/PArgb DrawItem oracle must match every actual rendered alpha byte, not a duplicate composition formula");
    }

    // SHA256 over the concatenated raw 24x24 BGRA frames for alpha0..255, measured from the
    // original DrawItem at native96 with AA/HQ, 2026-10-30. All three actual theme processes
    // were measured; dark/custom share the original raw WindowText F0F0F0 outline.
    private static string NativeAlphaHash(string theme, string role, bool head) => (role, head, theme == "light") switch
    {
        ("black", false, _) => "A5C18BAC811118FC5E5B2BAEB66AFFE9D4CB67795643D2CE5447882BA6BDEEF9",
        ("white", false, _) => "D500FBAA53A114404F2AF1BF4C99313AA3E04369A21ABA52934CF16825CC33FB",
        ("red", false, _) => "6A52606BE97E5D152620743F8F1288F9327CEE7077326E9668A1FEC1C030B819",
        ("green", false, _) => "0652EF69920EA601B1939FCFD946C1D0F6CD829DBC49E10B975C3A1053412621",
        ("blue", false, _) => "01733B9C1F3CCAEB1B25E70063D4D9AC614A3C1AF31BA2401E7163C0C3010E85",
        ("asymmetric-low", false, _) => "59C193340384C401D01CFC158219D88253DDAD7C6830A0B1B6A1D07732263DF1",
        ("black", true, true) => "67AE6CC697C3C60C86559B51CF7CE4E75F8BCDD4A573CB5D73EDCB54B6C32293",
        ("white", true, true) => "95A5396831F02492104B65D0EE8C2659D030446CD7674F429E000E2C1DF50E67",
        ("red", true, true) => "8033FA779867B182DD91FB0F290E21F9E8109412CF870EDA8E6107480EF01422",
        ("green", true, true) => "685D2311FBFA8B6773EEC701FFA6D93D45A19550721AD0D5767FB0EB3AAF8613",
        ("blue", true, true) => "D8D3F5BF4BB7B7097FFD2CD8B2443929E2F29BB7701EC22CC20F4280D0B23730",
        ("asymmetric-low", true, true) => "0F3382E4F03924BE9F819DECCFE8A7C73A924FA1C3CA6605BB76E7D166481DF2",
        ("black", true, false) => "61F132A88C028C6E8F2CCE0DF7075ACAB71A3941C0C8574A853BF2F9AF85D23A",
        ("white", true, false) => "12B19CB43E28B46B213E1FFDD2F91E4ADBB6D00B456810A4F63689C6072DC62E",
        ("red", true, false) => "35ED80B259A416553194C705DC8D9F3BCA942B9B1770BEFC41F528C08755B972",
        ("green", true, false) => "4A645F6FDAE714B52D9FE96B34C5F254A7D082B375AA5ED8EC9ABB5F47B88E11",
        ("blue", true, false) => "F66F7C974FA77A6097ACC92641080466303E50EC770DF78673210E9E6D9CEF47",
        ("asymmetric-low", true, false) => "A713926ECE2F10DFC96D359E1DB9CF67D773BCD70DAF73D96B3DDA183750203F",
        _ => throw new ArgumentOutOfRangeException(nameof(role)),
    };

    private static byte[] CopyPremultipliedPixels(Bitmap bitmap)
    {
        using WriteableBitmap copy = new(bitmap.PixelSize, bitmap.Dpi, PixelFormat.Bgra8888, AlphaFormat.Premul);
        using ILockedFramebuffer buffer = copy.Lock();
        bitmap.CopyPixels(buffer);
        int rowBytes = bitmap.PixelSize.Width * sizeof(int);
        byte[] bytes = new byte[rowBytes * bitmap.PixelSize.Height];
        for (int row = 0; row < bitmap.PixelSize.Height; row++)
        {
            Marshal.Copy(buffer.Address + (row * buffer.RowBytes), bytes, row * rowBytes, rowBytes);
        }

        return bytes;
    }

    private static void RetainAlpha(object report)
    {
        string? root = Environment.GetEnvironmentVariable("GITEXT_GRAPH_RASTER_EVIDENCE");
        if (string.IsNullOrWhiteSpace(root))
        {
            return;
        }

        string directory = Path.Combine(root, "native-matrix", TestContext.CurrentContext.Test.ID);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, Guid.NewGuid().ToString("N") + ".json"), JsonSerializer.Serialize(new
        {
            test = TestContext.CurrentContext.Test.FullName,
            captureMethod = "actualAvaloniaNativeRevisionGraphNodeRasterOwnedHeadlessBitmap",
            scope = "Actual native96 node-adapter frames, including all256 fill alpha values. The separate graph matrix covers actual GraphRenderer integration. Does not prove parent-backdrop composition or higher-DPI raster identity.",
            report,
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private sealed class NativeNodeControl(IBrush fill, Rect bounds, bool head) : Control
    {
        public bool UsedNativeOperation { get; private set; }

        public override void Render(DrawingContext context)
            => UsedNativeOperation = NativeRevisionGraphNodeRaster.TryDraw(context, fill, bounds, head);
    }

    private sealed class LayeredGraphControl(RevisionGraphConfig config, IRevisionGraphRow[] rows, ObjectId head) : Control
    {
        public override void Render(DrawingContext context)
        {
            using (context.PushClip(new Rect(0, 0, 24, 24)))
            {
                GraphRenderer.DrawItem(config, context, index: 1, rowHeight: 24,
                    index => index >= 0 && index < rows.Length ? rows[index] : null,
                    RevisionGraphDrawStyle.DrawNonRelativesGray, head);
            }
        }
    }

    private static void AssertNativeSquareMask(SKBitmap pixels, PixelRect clip, PixelRect bounds, bool head)
    {
        SKColor fill = Pixel(RevisionGraphLaneColor.NonRelativeColor);
        SKColor outline = Pixel(NativeRevisionGraphNodeRaster.GetOutlineColor());
        PixelRect frame = new(bounds.X - 2, bounds.Y - 2, bounds.Width + 4, bounds.Height + 4);
        for (int y = 0; y < pixels.Height; y++)
        {
            for (int x = 0; x < pixels.Width; x++)
            {
                PixelPoint point = new(x, y);
                SKColor expected = !clip.ContainsExclusive(point) ? default
                    : bounds.ContainsExclusive(point) ? fill
                    : head && frame.ContainsExclusive(point) ? outline : default;
                pixels.GetPixel(x, y).Should().Be(expected,
                    "the actual native96 PArgb/HQ oracle has an opaque ten-pixel fill and a two-pixel HEAD frame, with no half-pixel layout shift at ({0},{1})", x, y);
            }
        }
    }

    private static void AssertOutsideClipTransparent(SKBitmap pixels, PixelRect clip)
    {
        for (int y = 0; y < pixels.Height; y++)
        {
            for (int x = 0; x < pixels.Width; x++)
            {
                if (!clip.ContainsExclusive(new PixelPoint(x, y)))
                {
                    pixels.GetPixel(x, y).Alpha.Should().Be(0, "the real renderer must retain its source cache clip at ({0},{1})", x, y);
                }
            }
        }
    }

    private static PixelRect RowClip(int row, int rowHeight, int left, bool clipped)
    {
        int top = CanvasPadding + (row * rowHeight);
        return clipped ? new PixelRect(left + 5, top + 2, 25, rowHeight - 4) : new PixelRect(left, top, CanvasWidth - left, rowHeight);
    }

    private static object DescribeRow(RevisionGraph graph, int index)
    {
        IRevisionGraphRow row = graph.GetSegmentsForRow(index) ?? throw new AssertionException("Every graph row must be cached before rendering.");
        return new
        {
            index, id = row.Revision.Objectid.ToString(), row.Revision.Score, row.Revision.IsRelative,
            lane = row.GetCurrentRevisionLane(), lanes = row.GetLaneCount(), refs = row.Revision.GitRevision?.Refs.Count,
            segments = row.Segments.Select(segment => new
            {
                parent = segment.Parent.Objectid.ToString(), child = segment.Child.Objectid.ToString(),
                childRelative = segment.Child.IsRelative, colorIndex = segment.LaneInfo?.Color,
                resolvedArgb = segment.LaneInfo is null ? null : Argb(BrushColor(RevisionGraphLaneColor.GetBrushForLane(segment.LaneInfo.Color))),
                lane = row.GetLaneForSegment(segment),
            }).ToArray(),
        };
    }

    private static object DescribeConfig(RevisionGraphConfig config) => new
    {
        config.MergeGraphLanesHavingCommonParent, config.ReduceGraphCrossings, config.RenderGraphWithDiagonals,
        config.StraightenGraphDiagonals, config.StraightenGraphSegmentsLimit,
    };

    private static object DescribeContext(string theme) => new
    {
        theme, bitmapFormat = "Bgra8888 Premul", dpi = 96, dpiMode = "ownedHeadlessBitmap",
        palette = Enumerable.Range(0, RevisionGraphLaneColor.PresetGraphBrushes.Count)
            .Select(index => Argb(BrushColor(RevisionGraphLaneColor.GetBrushForLane(index)))).ToArray(),
        nonRelative = Argb(RevisionGraphLaneColor.NonRelativeColor),
        windowText = Argb(NativeRevisionGraphNodeRaster.GetOutlineColor()),
    };

    private static object DescribeBitmap(RenderTargetBitmap bitmap, SKBitmap pixels)
    {
        using WriteableBitmap copy = new(bitmap.PixelSize, bitmap.Dpi, PixelFormat.Bgra8888, AlphaFormat.Premul);
        using ILockedFramebuffer buffer = copy.Lock();
        bitmap.CopyPixels(buffer);
        int stride = buffer.RowBytes;
        byte[] bytes = new byte[stride * bitmap.PixelSize.Height];
        Marshal.Copy(buffer.Address, bytes, 0, bytes.Length);

        return new
        {
            Width = pixels.Width, Height = pixels.Height,
            runEncoding = "count:AARRGGBB;count:AARRGGBB; each row expands to Width pixels",
            argbRuns = Enumerable.Range(0, pixels.Height).Select(y => EncodeRuns(Enumerable.Range(0, pixels.Width).Select(x => (uint)pixels.GetPixel(x, y)))).ToArray(),
            premultipliedArgbRuns = Enumerable.Range(0, pixels.Height).Select(y => EncodeRuns(Enumerable.Range(0, pixels.Width).Select(x => BitConverter.ToUInt32(bytes, (y * stride) + (x * sizeof(int)))))).ToArray(),
        };
    }

    private static IEnumerable<uint> ReadPixels(SKBitmap bitmap)
        => Enumerable.Range(0, bitmap.Height).SelectMany(y => Enumerable.Range(0, bitmap.Width).Select(x => (uint)bitmap.GetPixel(x, y)));

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

    private static RenderTargetBitmap Render(Control control, PixelSize size)
    {
        control.Width = size.Width;
        control.Height = size.Height;
        control.Measure(new Size(size.Width, size.Height));
        control.Arrange(new Rect(0, 0, size.Width, size.Height));
        RenderTargetBitmap bitmap = new(size);
        bitmap.Dpi.Should().Be(new Vector(96, 96), "these source-sized primitive probes must use the actual unscaled bitmap context");
        bitmap.Render(control);
        return bitmap;
    }

    private static SKBitmap Decode(Bitmap bitmap)
    {
        using MemoryStream stream = new();
        bitmap.Save(stream, PngBitmapEncoderOptions.Default);
        return SKBitmap.Decode(stream.ToArray()) ?? throw new AssertionException("The actual graph PNG must decode.");
    }

    private static void Retain(Bitmap bitmap, object report, string category = "native-matrix")
    {
        string? root = Environment.GetEnvironmentVariable("GITEXT_GRAPH_RASTER_EVIDENCE");
        if (string.IsNullOrWhiteSpace(root))
        {
            return;
        }

        string directory = Path.Combine(root, category, TestContext.CurrentContext.Test.ID);
        Directory.CreateDirectory(directory);
        string identity = Guid.NewGuid().ToString("N");
        using (FileStream output = File.Create(Path.Combine(directory, identity + ".png")))
        {
            bitmap.Save(output, PngBitmapEncoderOptions.Default);
        }

        File.WriteAllText(Path.Combine(directory, identity + ".json"), JsonSerializer.Serialize(new
        {
            test = TestContext.CurrentContext.Test.FullName,
            captureMethod = "actualAvaloniaGraphRendererOwnedHeadlessBitmap",
            scope = "Native-identical model/theme/configuration/origin/clip inputs. Metamorphic tests do not assert GDI/Skia antialiased pixel identity.",
            report,
        }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void WithSettings(Action action)
    {
        string root = Path.Combine(Path.GetTempPath(), "GitExtensions.GraphNativeMatrix", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using GitExtSettingsCache cache = new(Path.Combine(root, "capture.settings"), autoSave: false);
            DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
            AppSettings.UsingContainer(settings, action);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static void Configure(bool diagonals, bool merged)
    {
        AppSettings.RenderGraphWithDiagonals.Value = diagonals;
        AppSettings.MergeGraphLanesHavingCommonParent.Value = merged;
        AppSettings.StraightenGraphDiagonals.Value = false;
        AppSettings.StraightenGraphSegmentsLimit.Value = 50;
    }

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

    private static DrawingColor BrushColor(IBrush brush)
    {
        Avalonia.Media.Color color = (brush as ISolidColorBrush)?.Color ?? throw new AssertionException("The graph source palette must contain solid brushes.");
        return DrawingColor.FromArgb(color.A, color.R, color.G, color.B);
    }

    private static SKColor Pixel(DrawingColor color) => new(color.R, color.G, color.B, color.A);

    private static string Argb(DrawingColor color) => $"#{unchecked((uint)color.ToArgb()):X8}";

    private sealed class GraphControl(RevisionGraphConfig config, Func<int, IRevisionGraphRow?> rows, int count,
        int rowHeight, int left, int top, RevisionGraphDrawStyle style, ObjectId head, IReadOnlySet<ObjectId>? hovered,
        Func<int, PixelRect> clip, bool reverse) : Control
    {
        public override void Render(DrawingContext context)
        {
            IEnumerable<int> order = Enumerable.Range(0, count);
            foreach (int row in reverse ? order.Reverse() : order)
            {
                PixelRect rectangle = clip(row);
                using (context.PushClip(new Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height)))
                using (context.PushTransform(Matrix.CreateTranslation(left, top + (row * rowHeight))))
                {
                    GraphRenderer.DrawItem(config, context, row, rowHeight, rows, style, head, hovered);
                }
            }
        }
    }

    private sealed class ThemeScope : IDisposable
    {
        private static readonly PropertyInfo ModuleSettings = typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings))
            ?? throw new InvalidOperationException("The original-shaped theme boundary is missing.");
        private static readonly PropertyInfo HelperSettings = typeof(ColorHelper).GetProperty(nameof(ColorHelper.ThemeSettings))
            ?? throw new InvalidOperationException("The shared theme boundary is missing.");
        private readonly ThemeSettings _original = ThemeModule.Settings;
        private readonly ThemeSettings _originalHelper = HelperSettings.GetValue(null) as ThemeSettings
            ?? throw new InvalidOperationException("The shared theme boundary has no current settings.");

        public ThemeScope(string name)
        {
            Theme theme = LoadTheme(name);
            ThemeSettings settings = new(theme, Theme.CreateDefaultTheme(), [], useSystemVisualStyle: name == "light");
            ModuleSettings.SetValue(null, settings);
            ColorHelper.ThemeSettings = settings;
            _ = RevisionGraphLaneColor.NonRelativeColor;
        }

        public void Dispose()
        {
            ModuleSettings.SetValue(null, _original);
            ColorHelper.ThemeSettings = _originalHelper;
        }

        private static Theme LoadTheme(string name, [CallerFilePath] string source = "")
        {
            if (name == "light")
            {
                return Theme.CreateDefaultTheme();
            }

            DirectoryInfo? root = new FileInfo(source).Directory;
            while (root is not null && !File.Exists(Path.Combine(root.FullName, "Directory.Packages.props")))
            {
                root = root.Parent;
            }

            string repository = root?.FullName ?? throw new AssertionException("The tracked source theme inputs must be available.");
            string themes = Path.Combine(repository, "src", "app", "GitUI", "Themes");
            string path = name == "custom"
                ? Path.Combine(repository, "eng", "tools", "WinFormsParityCapture", "Themes", "parity-custom.css")
                : Path.Combine(themes, "dark.css");
            IThemeCssUrlResolver resolver = Substitute.For<IThemeCssUrlResolver>();
            resolver.ResolveCssUrl(Arg.Any<string>()).Returns(call => Path.Combine(themes, call.Arg<string>()));
            ThemeLoader loader = new(resolver, new ThemeFileReader());
            return loader.LoadTheme(path, new ThemeId(name), []);
        }
    }
}
