using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using GitExtensions.Extensibility.Git;
using GitExtensions.Plugins.GitStatistics.PieChart;
using GitExtUtils.GitUI.Theming;
using GitUI.Blame;
using GitUI.Editor;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;
using DrawingColor = System.Drawing.Color;

namespace GitExtensionsTests;

/// <summary>
///  Golden-image coverage for content colors rendered by Git Extensions itself. These images
///  complement exact algorithm tests by exercising the real Avalonia drawing surfaces.
/// </summary>
[TestFixture]
public sealed class ContentColorGoldenImageTests
{
    [AvaloniaTest]
    [Category("P8.6h.1")]
    public void Diff_should_render_add_remove_context_and_inline_highlights()
    {
        FileViewer viewer = new();
        viewer.ViewPatch(
            "diff --git a/src/App.cs b/src/App.cs\n"
            + "@@ -1,3 +1,4 @@\n"
            + " namespace Sample;\n"
            + "-public class OldName\n"
            + "+public sealed class NewName\n"
            + " {\n"
            + "+    public bool Enabled { get; set; }\n"
            + " }\n");

        GoldenImageVerifier.Verify(viewer, 560, 220, "content-diff");
    }

    [AvaloniaTest]
    public void Ref_labels_should_render_the_original_palette_and_shapes()
    {
        StackPanel labels = new()
        {
            Margin = new Avalonia.Thickness(12),
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                Wrap(
                    CreateRef("main", isHead: true, isSelected: true, localName: "main", mergeWith: "main", trackingRemote: "origin"),
                    CreateRef("origin/main", isRemote: true, isSelectedHeadMergeSource: true, localName: "main", remote: "origin")),
                Wrap(CreateRef("v1.0", isTag: true)),
            },
        };

        GoldenImageVerifier.Verify(labels, 360, 100, "content-refs");
    }

    [AvaloniaTest]
    [Category("P8.6h.1")]
    public void Blame_margin_should_render_age_buckets_and_author_runs()
    {
        DrawingColor recent = DrawingColor.FromArgb(0, 68, 27).AdaptBackColor();
        DrawingColor earlier = DrawingColor.FromArgb(247, 252, 245).AdaptBackColor();
        BlameAuthorMargin margin = new(new Typeface(FontFamily.Default), 12);
        margin.Initialize(
            "2026-07-31 - Recent Contributor\n\n2025-01-12 - Earlier Contributor\n",
            [
                new GitBlameEntry { AgeBucketIndex = 6, AgeBucketColor = recent },
                new GitBlameEntry { AgeBucketIndex = 6, AgeBucketColor = recent },
                new GitBlameEntry { AgeBucketIndex = 0, AgeBucketColor = earlier },
                new GitBlameEntry { AgeBucketIndex = 0, AgeBucketColor = earlier },
            ]);
        TextEditor editor = new()
        {
            Background = Brushes.White,
            FontFamily = FontFamily.Default,
            FontSize = 12,
            Text = "first line\ncontinuation\nolder line\ncontinuation\n",
        };
        editor.TextArea.LeftMargins.Insert(0, margin);

        GoldenImageVerifier.Verify(editor, 480, 104, "content-blame");
    }

    [AvaloniaTest]
    public void Statistics_chart_should_render_adapted_slices_edges_and_shadow()
    {
        GoldenImageVerifier.Verify(new ChartSurface(), 320, 240, "content-chart");
    }

    private static Control Wrap(params IGitRef[] refs)
    {
        StackPanel panel = new()
        {
            Orientation = Orientation.Horizontal,
            Spacing = 4,
        };
        panel.Children.AddRange(RevisionGridRefRenderer.CreateLabels(refs));
        return panel;
    }

    private static IGitRef CreateRef(
        string name,
        bool isHead = false,
        bool isRemote = false,
        bool isTag = false,
        bool isSelected = false,
        bool isSelectedHeadMergeSource = false,
        string? localName = null,
        string mergeWith = "",
        string remote = "",
        string trackingRemote = "")
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns(name);
        gitRef.IsHead.Returns(isHead);
        gitRef.IsRemote.Returns(isRemote);
        gitRef.IsTag.Returns(isTag);
        gitRef.IsSelected.Returns(isSelected);
        gitRef.IsSelectedHeadMergeSource.Returns(isSelectedHeadMergeSource);
        gitRef.LocalName.Returns(localName ?? name);
        gitRef.MergeWith.Returns(mergeWith);
        gitRef.Remote.Returns(remote);
        gitRef.TrackingRemote.Returns(trackingRemote);
        return gitRef;
    }

    private sealed class ChartSurface : Control
    {
        public override void Render(DrawingContext context)
        {
            context.FillRectangle(Brushes.White, new Avalonia.Rect(Bounds.Size));
            PieChart3D chart = new(
                20,
                20,
                280,
                200,
                [5, 3, 2],
                [Colors.Red, Colors.Green, Colors.Blue],
                sliceRelativeHeight: 0.2)
            {
                FitToBoundingRectangle = true,
                EdgeColorType = EdgeColorType.DarkerThanSurface,
                ShadowStyle = ShadowStyle.GradualShadow,
            };
            chart.SetInitialAngle(-30);
            chart.Draw(context);
        }
    }
}
