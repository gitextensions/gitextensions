using AwesomeAssertions;
using GitExtensions.ParityCapture;
using NUnit.Framework;

namespace GitExtensions.ParityDiff.Tests;

// parity-scaffolding: Proves aggregate color closure compares every declared semantic role.
[TestFixture]
[Category("P1_7b")]
public sealed class ColorRoleRunnerTests
{
    private static readonly ColorRoleDefinition PanelRole = new()
    {
        Id = "semantic.app.panel.background",
        Meaning = "The application panel background."
    };

    private static readonly ColorRoleDefinition ControlRole = new()
    {
        Id = "semantic.system.control.background",
        Meaning = "The normal desktop-control background."
    };

    [Test]
    public void Run_should_emit_an_empty_report_when_every_declared_role_matches()
    {
        using ParityDiffFixture fixture = new();
        fixture.WriteColorRoleCatalog(PanelRole, ControlRole);
        CaptureDocument document = WithRoles(
            fixture.CreateDocument("light"),
            (PanelRole.Id, "#FF102030"),
            (ControlRole.Id, "#FF405060"));
        fixture.WriteCaptureSet("reference", [document]);
        fixture.WriteCaptureSet("candidate", [document]);

        ColorRoleResult result = fixture.RunColors();

        result.Summary.ComparedCaptureCount.Should().Be(1);
        result.Summary.DeclaredRoleCount.Should().Be(2);
        result.Summary.RoleComparisonCount.Should().Be(2);
        result.Summary.MatchCount.Should().Be(2);
        result.Summary.FindingCount.Should().Be(0);
        File.ReadAllText(Path.Combine(fixture.ColorOutputDirectory, "color-findings.json"))
            .Should().Contain("\"findingCount\": 0");
        File.ReadAllText(Path.Combine(fixture.ColorOutputDirectory, "color-report.md"))
            .Should().Contain("- Findings: 0")
            .And.Contain(PanelRole.Meaning);
    }

    [Test]
    public void Run_should_localize_mismatched_missing_and_undeclared_roles()
    {
        using ParityDiffFixture fixture = new();
        fixture.WriteColorRoleCatalog(PanelRole, ControlRole);
        fixture.WriteCaptureSet(
            "reference",
            [WithRoles(
                fixture.CreateDocument("dark"),
                (PanelRole.Id, "#FF102030"),
                (ControlRole.Id, "#FF405060"))]);
        fixture.WriteCaptureSet(
            "candidate",
            [WithRoles(
                fixture.CreateDocument("dark"),
                (PanelRole.Id, "#FF102031"),
                ("semantic.test.undeclared", "#FF708090"))]);

        ColorRoleResult result = fixture.RunColors();

        ColorRoleCaptureComparison comparison = result.Captures.Should().ContainSingle().Subject;
        comparison.RoleComparisonCount.Should().Be(1);
        comparison.MatchCount.Should().Be(0);
        comparison.Findings.Select(finding => finding.Code).Should().BeEquivalentTo(
            "color.roleMismatch",
            "color.roleMissing",
            "color.roleUndeclared");
        comparison.Findings.Single(finding => finding.Code == "color.roleMismatch").Role
            .Should().Be(PanelRole.Id);
        comparison.Findings.Single(finding => finding.Code == "color.roleMissing").Role
            .Should().Be(ControlRole.Id);
    }

    [Test]
    public void Run_should_preserve_explicit_unavailable_capture_notes()
    {
        using ParityDiffFixture fixture = new();
        fixture.WriteColorRoleCatalog(PanelRole);
        fixture.WriteUnsupportedCaptureSet("reference", "A native 96-DPI monitor is required.");
        fixture.WriteUnsupportedCaptureSet("candidate", "The popup is an external surface.");

        ColorRoleResult result = fixture.RunColors();

        ColorRoleCaptureComparison comparison = result.Captures.Should().ContainSingle().Subject;
        comparison.Status.Should().Be("unavailable");
        comparison.ReferenceNote.Should().Be("A native 96-DPI monitor is required.");
        comparison.CandidateNote.Should().Be("The popup is an external surface.");
        comparison.Findings.Should().BeEmpty();
    }

    [Test]
    public void Catalog_should_reject_a_role_without_one_framework_neutral_meaning()
    {
        using ParityDiffFixture fixture = new();
        fixture.WriteColorRoleCatalog(new ColorRoleDefinition
        {
            Id = "semantic.app.invalid",
            Meaning = ""
        });

        Action action = () => ColorRoleCatalog.Load(fixture.ColorRoleCatalogFile);

        action.Should().Throw<InvalidDataException>()
            .WithMessage("*does not define a framework-neutral meaning*");
    }

    [Test]
    public void Catalog_should_reject_an_empty_role_set()
    {
        using ParityDiffFixture fixture = new();
        fixture.WriteColorRoleCatalog();

        Action action = () => ColorRoleCatalog.Load(fixture.ColorRoleCatalogFile);

        action.Should().Throw<InvalidDataException>()
            .WithMessage("*must declare at least one semantic role*");
    }

    private static CaptureDocument WithRoles(
        CaptureDocument document,
        params (string Name, string Value)[] roles)
    {
        CaptureSurface surface = document.Surfaces.Single();
        CaptureNode root = surface.Root;
        return document with
        {
            Surfaces =
            [
                surface with
                {
                    Root = root with
                    {
                        Colors = root.Colors with
                        {
                            Additional = roles.ToDictionary(role => role.Name, role => role.Value, StringComparer.Ordinal)
                        }
                    }
                }
            ]
        };
    }
}
