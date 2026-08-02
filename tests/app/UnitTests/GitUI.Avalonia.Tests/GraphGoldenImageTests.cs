using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUI.UserControls.RevisionGrid.Graph.Rendering;
using GitUIPluginInterfaces;
using SkiaSharp;

namespace GitExtensionsTests;

/// <summary>
///  Golden-image tests for the ported commit graph renderer: known topologies are rendered
///  with headless Skia and compared pixel-wise against the images in <c>GoldenImages/</c>.
///  Run with the environment variable <c>GITEXT_UPDATE_GOLDEN_IMAGES=1</c> to (re)create
///  the golden images after a deliberate rendering change.
/// </summary>
[TestFixture]
public sealed class GraphGoldenImageTests
{
    // Mirrors the row metrics of RevisionGridControl.
    private const int RowHeight = 24;
    private const int GraphColumnWidth = 160;

    // Per-channel difference below this is ignored (anti-aliasing variance between
    // Skia builds); a small share of pixels may exceed it before the test fails.
    private const int ChannelTolerance = 8;
    private const double MaxBadPixelShare = 0.002;

    [AvaloniaTest]
    public void Graph_should_render_a_linear_chain()
    {
        ObjectId head = Id('1');
        RevisionGraph graph = BuildGraph(
            head,
            Commit(Id('1'), Id('2')),
            Commit(Id('2'), Id('3')),
            Commit(Id('3'), Id('4')),
            Commit(Id('4')));

        VerifyGolden(graph, head, "graph-linear-chain");
    }

    [AvaloniaTest]
    public void Graph_should_render_a_merge()
    {
        ObjectId head = Id('1');
        RevisionGraph graph = BuildGraph(
            head,
            Commit(Id('1'), Id('2'), Id('3')),
            Commit(Id('2'), Id('4')),
            Commit(Id('3'), Id('4')),
            Commit(Id('4')));

        VerifyGolden(graph, head, "graph-merge");
    }

    [AvaloniaTest]
    public void Graph_should_render_crossing_lanes_and_non_relatives()
    {
        // A merge plus an unrelated tip ("a", not an ancestor of the head) sharing a
        // parent: exercises lane crossings and the non-relative gray rendering.
        ObjectId head = Id('1');
        RevisionGraph graph = BuildGraph(
            head,
            Commit(Id('1'), Id('2'), Id('3')),
            Commit(Id('a'), Id('3')),
            Commit(Id('2'), Id('4')),
            Commit(Id('3'), Id('4')),
            Commit(Id('4')));

        VerifyGolden(graph, head, "graph-crossing");
    }

    private static ObjectId Id(char hexDigit) => ObjectId.Parse(new string(hexDigit, ObjectId.Sha1CharCount));

    private static GitRevision Commit(ObjectId objectId, params ObjectId[] parentIds)
        => new(objectId) { ParentIds = parentIds };

    /// <summary>
    ///  Feeds the revisions (in the topo order the log would emit them) through the same
    ///  sequence RevisionGridControl uses: add, highlight the head branch, complete, cache.
    /// </summary>
    private static RevisionGraph BuildGraph(ObjectId headId, params GitRevision[] revisions)
    {
        RevisionGraph graph = new()
        {
            HeadId = headId,
        };

        foreach (GitRevision revision in revisions)
        {
            graph.Add(revision);
        }

        graph.HighlightBranch(headId);
        graph.LoadingCompleted();
        graph.CacheTo(graph.Count - 1, graph.Count - 1);
        return graph;
    }

    private static void VerifyGolden(RevisionGraph graph, ObjectId headId, string goldenName)
    {
        // The golden images are rendered with the default revision graph settings; the row
        // cache reads them from AppSettings, so bail out (rather than fail) if this machine
        // has customized them.
        RevisionGraphConfig config = graph.Config;
        if (!config.MergeGraphLanesHavingCommonParent
            || !config.RenderGraphWithDiagonals
            || !config.StraightenGraphDiagonals
            || config.StraightenGraphSegmentsLimit != 80)
        {
            Assert.Inconclusive("The golden images assume default revision graph settings (AppSettings), which are customized on this machine.");
        }

        graph.GetCachedCount().Should().Be(graph.Count);

        Window window = new()
        {
            Width = GraphColumnWidth,
            Height = graph.Count * RowHeight,
            SizeToContent = SizeToContent.Manual,
            Content = new GraphSheetControl(graph, headId),
        };
        window.Show();

        try
        {
            WriteableBitmap? frame = window.CaptureRenderedFrame();
            frame.Should().NotBeNull("headless Skia rendering should produce a frame");

            using MemoryStream actualStream = new();
            frame!.Save(actualStream, PngBitmapEncoderOptions.Default);
            byte[] actualPng = actualStream.ToArray();

            string goldenPath = Path.Combine(GetGoldenDirectory(), goldenName + ".png");

            if (Environment.GetEnvironmentVariable("GITEXT_UPDATE_GOLDEN_IMAGES") == "1")
            {
                Directory.CreateDirectory(Path.GetDirectoryName(goldenPath)!);
                File.WriteAllBytes(goldenPath, actualPng);
                Assert.Inconclusive($"Golden image regenerated at {goldenPath}; review and commit it, then rerun without GITEXT_UPDATE_GOLDEN_IMAGES.");
            }

            if (!File.Exists(goldenPath))
            {
                Assert.Fail($"Golden image {goldenPath} is missing; run this test with GITEXT_UPDATE_GOLDEN_IMAGES=1 to create it.");
            }

            AssertImagesMatch(File.ReadAllBytes(goldenPath), actualPng, goldenName);
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertImagesMatch(byte[] goldenPng, byte[] actualPng, string goldenName)
    {
        using SKBitmap golden = SKBitmap.Decode(goldenPng);
        using SKBitmap actual = SKBitmap.Decode(actualPng);

        if (golden.Width != actual.Width || golden.Height != actual.Height)
        {
            Assert.Fail($"Rendered image is {actual.Width}x{actual.Height}, the golden image {goldenName} is {golden.Width}x{golden.Height}. {SaveActual(actualPng, goldenName)}");
        }

        int badPixels = 0;
        for (int y = 0; y < golden.Height; y++)
        {
            for (int x = 0; x < golden.Width; x++)
            {
                SKColor expected = golden.GetPixel(x, y);
                SKColor got = actual.GetPixel(x, y);
                if (Math.Abs(expected.Red - got.Red) > ChannelTolerance
                    || Math.Abs(expected.Green - got.Green) > ChannelTolerance
                    || Math.Abs(expected.Blue - got.Blue) > ChannelTolerance
                    || Math.Abs(expected.Alpha - got.Alpha) > ChannelTolerance)
                {
                    badPixels++;
                }
            }
        }

        int maxBadPixels = (int)(golden.Width * golden.Height * MaxBadPixelShare);
        if (badPixels > maxBadPixels)
        {
            Assert.Fail($"{badPixels} pixels differ from the golden image {goldenName} (allowed: {maxBadPixels}). {SaveActual(actualPng, goldenName)} If the rendering change is intended, regenerate with GITEXT_UPDATE_GOLDEN_IMAGES=1.");
        }
    }

    private static string SaveActual(byte[] actualPng, string goldenName)
    {
        string path = Path.Combine(TestContext.CurrentContext.WorkDirectory, goldenName + ".actual.png");
        File.WriteAllBytes(path, actualPng);
        return $"Actual image saved to {path}.";
    }

    private static string GetGoldenDirectory([CallerFilePath] string thisFilePath = "")
        => Path.Combine(Path.GetDirectoryName(thisFilePath)!, "GoldenImages");

    /// <summary>
    ///  Renders every graph row like a column of RevisionGridControl graph cells: each row
    ///  clipped to its cell, drawn by the shared <see cref="GraphRenderer"/>.
    /// </summary>
    private sealed class GraphSheetControl(RevisionGraph graph, ObjectId headId) : Control
    {
        public override void Render(DrawingContext context)
        {
            context.FillRectangle(Brushes.White, new Rect(Bounds.Size));

            for (int rowIndex = 0; rowIndex < graph.Count; rowIndex++)
            {
                using (context.PushTransform(Matrix.CreateTranslation(0, rowIndex * RowHeight)))
                using (context.PushClip(new Rect(0, 0, GraphColumnWidth, RowHeight)))
                {
                    GraphRenderer.DrawItem(graph.Config, context, rowIndex, RowHeight,
                        graph.GetSegmentsForRow,
                        RevisionGraphDrawStyle.DrawNonRelativesGray,
                        headId);
                }
            }
        }
    }
}
