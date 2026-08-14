using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace GitExtensions.ParityDiff.Tests;

// parity-scaffolding: Proves the temporary comparison tool localizes perturbations.
[TestFixture]
[Category("P0_3")]
public sealed class ParityDiffRunnerTests
{
    [Test]
    public void Run_should_emit_no_findings_for_identical_sets()
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument document = fixture.CreateDocument("light");
        fixture.WriteCaptureSet("reference", [document]);
        fixture.WriteCaptureSet("candidate", [document]);

        ParityDiffResult result = fixture.Run();

        result.Summary.ComparedCaptureCount.Should().Be(1);
        result.Summary.FindingCount.Should().Be(0);
        PixelMetrics pixels = result.Captures.Should().ContainSingle().Which.Pixels
            ?? throw new InvalidOperationException("A compared capture must include pixel metrics.");
        pixels.Ssim.Should().Be(1);
        File.ReadAllText(Path.Combine(fixture.OutputDirectory, "findings.json"))
            .Should().Contain("\"findingCount\": 0");
        File.ReadAllText(Path.Combine(fixture.OutputDirectory, "report.md"))
            .Should().Contain("- Findings: 0");
    }

    [Test]
    public void Run_should_detect_and_localize_one_metric_perturbation()
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument reference = fixture.CreateDocument("light");
        CaptureDocument candidate = ChangeTarget(
            reference,
            node => node with
            {
                BoundsDip = node.BoundsDip with { Width = node.BoundsDip.Width + 2 }
            });
        fixture.WriteCaptureSet("reference", [reference]);
        fixture.WriteCaptureSet("candidate", [candidate]);

        ParityDiffResult result = fixture.Run();

        ParityFinding finding = result.Captures.Should().ContainSingle()
            .Which.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be("geometry.width");
        finding.Path.Should().Be("surface[primary]/control[btnTarget]/boundsDip");
        finding.Delta.Should().Be("2");
        finding.Tolerance.Should().Be("0.5");
    }

    [Test]
    public void Run_should_compare_resolved_item_height_as_geometry()
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument reference = ChangeTarget(
            fixture.CreateDocument("light"),
            node => node with { ItemHeightDip = 25.6m });
        CaptureDocument candidate = ChangeTarget(
            reference,
            node => node with { ItemHeightDip = 24m });
        fixture.WriteCaptureSet("reference", [reference]);
        fixture.WriteCaptureSet("candidate", [candidate]);

        ParityFinding finding = fixture.Run().Captures.Should().ContainSingle()
            .Which.Findings.Should().ContainSingle().Subject;

        finding.Code.Should().Be("geometry.itemHeightDip");
        finding.Delta.Should().Be("1.6");
        finding.Tolerance.Should().Be("0.5");
    }

    [Test]
    public void Run_should_not_compare_geometry_until_both_controls_are_visible()
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument reference = ChangeTarget(
            fixture.CreateDocument("light"),
            node => node with { Visible = false });
        CaptureDocument candidate = ChangeTarget(
            reference,
            node => node with
            {
                BoundsDip = node.BoundsDip with { Width = node.BoundsDip.Width + 20 },
                ClientSizeDip = node.ClientSizeDip with { Width = node.ClientSizeDip.Width + 20 }
            });
        fixture.WriteCaptureSet("reference", [reference]);
        fixture.WriteCaptureSet("candidate", [candidate]);

        ParityDiffResult result = fixture.Run();

        result.Captures.Should().ContainSingle().Which.Findings.Should().BeEmpty(
            "hidden controls have no rendered geometry to compare until an opened state realizes them");
    }

    [Test]
    public void Run_should_join_framework_specific_columns_by_index()
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument reference = ChangeTarget(
            fixture.CreateDocument("light"),
            node => node with
            {
                Columns =
                [
                    CreateColumn(fieldName: "_maximizedColumn", headerText: "Message", widthDip: 100),
                ]
            });
        CaptureDocument candidate = ChangeTarget(
            reference,
            node => node with
            {
                Columns =
                [
                    CreateColumn(fieldName: null, headerText: "Message", widthDip: 120),
                ]
            });
        fixture.WriteCaptureSet("reference", [reference]);
        fixture.WriteCaptureSet("candidate", [candidate]);

        ParityDiffResult result = fixture.Run();

        result.Captures.Should().ContainSingle().Which.Findings.Select(finding => finding.Code)
            .Should().BeEquivalentTo("column.fieldName", "column.widthDip");
        result.Captures.Single().Findings.Select(finding => finding.Code)
            .Should().NotContain("column.missing", "column.extra");
    }

    [Test]
    public void Run_should_localize_a_dark_theme_only_color_perturbation()
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument light = fixture.CreateDocument("light");
        CaptureDocument dark = fixture.CreateDocument("dark");
        CaptureDocument changedDark = ChangeTarget(
            dark,
            node => node with
            {
                Colors = node.Colors with { Background = "#FF102030" }
            });
        fixture.WriteCaptureSet("reference", [light, dark]);
        fixture.WriteCaptureSet("candidate", [light, changedDark]);

        ParityDiffResult result = fixture.Run();

        CaptureComparison comparison = result.Captures.Should()
            .ContainSingle(item => item.Findings.Count > 0).Subject;
        comparison.Key.ThemeId.Should().Be("dark");
        ParityFinding finding = comparison.Findings.Should().ContainSingle().Subject;
        finding.Code.Should().Be("color.background");
        finding.Path.Should().Be("surface[primary]/control[btnTarget]");
        result.Captures.Single(item => item.Key.ThemeId == "light").Findings.Should().BeEmpty();
    }

    [Test]
    public void Run_should_localize_a_uniquely_renamed_control()
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument reference = fixture.CreateDocument("light");
        CaptureDocument candidate = ChangeTarget(
            reference,
            node => node with
            {
                FieldName = "btnRenamed",
                Name = "btnRenamed"
            });
        fixture.WriteCaptureSet("reference", [reference]);
        fixture.WriteCaptureSet("candidate", [candidate]);

        ParityDiffResult result = fixture.Run();

        ParityFinding finding = result.Captures.Should().ContainSingle()
            .Which.Findings.Should().ContainSingle(item => item.Code == "control.renamed").Subject;
        finding.Code.Should().Be("control.renamed");
        finding.ReferenceValue.Should().Be("btnTarget");
        finding.CandidateValue.Should().Be("btnRenamed");
    }

    [Test]
    public void Run_should_report_missing_and_extra_controls_without_a_semantic_join()
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument reference = fixture.CreateDocument("light");
        CaptureDocument candidate = ChangeTarget(
            reference,
            node => node with
            {
                FieldName = "txtUnrelated",
                Name = "txtUnrelated",
                ControlKind = "textBox",
                Text = "Unrelated"
            });
        fixture.WriteCaptureSet("reference", [reference]);
        fixture.WriteCaptureSet("candidate", [candidate]);

        ParityDiffResult result = fixture.Run();

        result.Captures.Should().ContainSingle().Which.Findings
            .Select(finding => finding.Code)
            .Should().Contain("control.missing", "control.extra");
    }

    [TestCase(2, 2, 1, 0, 0)]
    [TestCase(2, 1, 1, 1, 0)]
    [TestCase(1, 2, 1, 0, 1)]
    public void Run_should_report_duplicate_field_identities_without_aborting(
        int referenceCount,
        int candidateCount,
        int expectedDuplicateFindings,
        int expectedMissingFindings,
        int expectedExtraFindings)
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument reference = RepeatTarget(fixture.CreateDocument("light"), referenceCount);
        CaptureDocument candidate = RepeatTarget(fixture.CreateDocument("light"), candidateCount);
        fixture.WriteCaptureSet("reference", [reference]);
        fixture.WriteCaptureSet("candidate", [candidate]);

        ParityDiffResult result = fixture.Run();

        CaptureComparison comparison = result.Captures.Should().ContainSingle().Subject;
        comparison.Status.Should().Be("compared");
        ParityFinding duplicate = comparison.Findings
            .Should().ContainSingle(finding => finding.Code == "control.duplicateIdentity").Subject;
        duplicate.Path.Should().Be("surface[primary]/control[btnTarget]");
        duplicate.ReferenceValue.Should().Be(referenceCount.ToString());
        duplicate.CandidateValue.Should().Be(candidateCount.ToString());
        comparison.Findings.Count(finding => finding.Code == "control.duplicateIdentity")
            .Should().Be(expectedDuplicateFindings);
        comparison.Findings.Count(finding => finding.Code == "control.missing")
            .Should().Be(expectedMissingFindings);
        comparison.Findings.Count(finding => finding.Code == "control.extra")
            .Should().Be(expectedExtraFindings);
        comparison.Findings.Where(finding => finding.Code is "control.missing" or "control.extra")
            .Select(finding => finding.Path)
            .Should().OnlyContain(path => path == "surface[primary]/control[btnTarget][2]");
    }

    [Test]
    public void Run_should_apply_ssim_and_per_pixel_channel_budgets()
    {
        using ParityDiffFixture fixture = new();
        CaptureDocument document = fixture.CreateDocument("light");
        fixture.WriteCaptureSet("reference", [document], red: 0);
        fixture.WriteCaptureSet("candidate", [document], red: 255);

        ParityDiffResult result = fixture.Run();

        CaptureComparison comparison = result.Captures.Should().ContainSingle().Subject;
        comparison.Findings.Select(finding => finding.Code).Should().Contain(
            "image.ssim",
            "image.differentPixelFraction",
            "image.maximumChannelDelta");
        PixelMetrics pixels = comparison.Pixels
            ?? throw new InvalidOperationException("A compared capture must include pixel metrics.");
        pixels.MaximumChannelDelta.Should().Be(255);
        pixels.DifferentPixelFraction.Should().Be(1);
    }

    [Test]
    public void Run_should_preserve_explicit_unsupported_state_notes()
    {
        using ParityDiffFixture fixture = new();
        fixture.WriteUnsupportedCaptureSet("reference", "Reference cannot open the popup.");
        fixture.WriteUnsupportedCaptureSet("candidate", "Candidate cannot compose the popup.");

        ParityDiffResult result = fixture.Run();

        CaptureComparison comparison = result.Captures.Should().ContainSingle().Subject;
        comparison.Status.Should().Be("unavailable");
        comparison.Findings.Should().BeEmpty();
        comparison.ReferenceNote.Should().Be("Reference cannot open the popup.");
        comparison.CandidateNote.Should().Be("Candidate cannot compose the popup.");
        File.ReadAllText(Path.Combine(fixture.OutputDirectory, "report.md"))
            .Should().Contain("Reference cannot open the popup.")
            .And.Contain("Candidate cannot compose the popup.");
    }

    private static CaptureDocument ChangeTarget(
        CaptureDocument document,
        Func<CaptureNode, CaptureNode> transform)
    {
        CaptureSurface surface = document.Surfaces.Single();
        CaptureNode root = surface.Root;
        CaptureNode target = root.Children.Single();
        return document with
        {
            Surfaces =
            [
                surface with
                {
                    Root = root with { Children = [transform(target)] }
                }
            ]
        };
    }

    private static CaptureDocument RepeatTarget(CaptureDocument document, int count)
    {
        CaptureSurface surface = document.Surfaces.Single();
        CaptureNode root = surface.Root;
        CaptureNode target = root.Children.Single();
        return document with
        {
            Surfaces =
            [
                surface with
                {
                    Root = root with
                    {
                        Children = Enumerable.Range(1, count)
                            .Select(index => target with { Id = $"{target.Id}/{index}" })
                            .ToArray()
                    }
                }
            ]
        };
    }

    private static CaptureColumn CreateColumn(string? fieldName, string headerText, decimal widthDip)
        => new()
        {
            FieldName = fieldName,
            Name = null,
            Type = "Tests.Column",
            Index = 0,
            DisplayIndex = 0,
            WidthPx = (int)widthDip,
            WidthDip = widthDip,
            Visible = true,
            Resizable = true,
            SortMode = "NotSortable",
            Alignment = "NotSet",
            HeaderText = headerText,
            HeaderAlignment = "NotSet",
            Colors = new CaptureColors
            {
                Foreground = null,
                Background = null,
                Border = null,
                SelectionForeground = null,
                SelectionBackground = null,
                InactiveSelectionForeground = null,
                InactiveSelectionBackground = null,
                DisabledForeground = null,
                DisabledBackground = null,
                GridLine = null,
                Additional = new Dictionary<string, string>()
            }
        };
}
