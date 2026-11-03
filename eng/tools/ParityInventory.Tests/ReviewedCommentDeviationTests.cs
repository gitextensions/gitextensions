using System.Text.Json;
using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Keeps reviewed absent and relocated comments exact without claiming their findings were fixed.
[TestFixture]
public sealed class ReviewedCommentDeviationTests
{
    [TestCase("codeAbsent")]
    [TestCase("codeMoved")]
    [TestCase("adaptedMoved")]
    [TestCase("absentDrifted")]
    [TestCase("designerAbsent")]
    [TestCase("designerMoved")]
    public void Run_should_retain_exact_reviewed_comment_deviations(string scenario)
    {
        using InventoryFixture fixture = new();
        Dictionary<string, object?> pin = Prepare(fixture, scenario);
        WritePin(fixture, pin);

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);

        report.Findings.Should().NotContain(finding => finding.Category == "comments");
        AcceptedFrameworkDeviation accepted = report.AcceptedFrameworkDeviations.Should().ContainSingle().Subject;
        accepted.OriginalPart.Should().Be((string?)pin["originalPart"]);
        accepted.OriginalValue.Should().Be("Keep the WinForms operation rationale. https://example.invalid/42");
        accepted.Reason.Should().Be(IsAbsent(scenario) ? "codeAbsent" : "codeMoved");
        accepted.TargetMember.Should().Be(IsAbsent(scenario) ? null : "method:Target()");
        accepted.TargetText.Should().Be((string?)pin.GetValueOrDefault("targetText")
            ?? (IsAbsent(scenario) ? null : (string?)pin["originalValue"]));
        report.Summary.AcceptedFrameworkDeviationCount.Should().Be(1);
    }

    [TestCase("text")]
    [TestCase("part")]
    [TestCase("path")]
    [TestCase("restored")]
    [TestCase("sourceChanged")]
    [TestCase("targetMember")]
    [TestCase("targetPart")]
    [TestCase("targetText")]
    [TestCase("targetRemoved")]
    [TestCase("targetDuplicated")]
    public void Run_should_fail_stale_or_drifted_comment_pins(string change)
    {
        using InventoryFixture fixture = new();
        Dictionary<string, object?> pin = Prepare(fixture, "codeMoved");
        switch (change)
        {
            case "text": pin["originalValue"] = "Different rationale."; break;
            case "part": pin["originalPart"] = "Other.cs"; break;
            case "path": pin["path"] = "comment/*"; break;
            case "restored":
                fixture.WriteTwin("Widget.cs", Original);
                break;
            case "sourceChanged":
                fixture.WriteOriginal("Widget.cs", Original.Replace("WinForms", "original", StringComparison.Ordinal));
                break;
            case "targetMember": pin["targetMember"] = "method:Original()"; break;
            case "targetPart": pin["twinPart"] = "Other.cs"; break;
            case "targetText": pin["targetText"] = "Changed target."; break;
            case "targetRemoved": fixture.WriteTwin("Widget.cs", Twin("") + "\n"); break;
            case "targetDuplicated":
                fixture.WriteTwin("Widget.cs", Twin("// " + pin["originalValue"] + "\n// " + pin["originalValue"]));
                break;
        }

        WritePin(fixture, pin);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);
        action.Should().Throw<InvalidDataException>().WithMessage("*drifted*");
    }

    [TestCase("unknownReason")]
    [TestCase("changed")]
    [TestCase("missingOriginalPart")]
    [TestCase("missingOriginalText")]
    [TestCase("missingTarget")]
    [TestCase("missingTargetPart")]
    [TestCase("typeTarget")]
    [TestCase("absentTarget")]
    [TestCase("absentTwinValue")]
    [TestCase("missingReason")]
    [TestCase("emptyRationale")]
    public void Run_should_reject_incomplete_or_unsupported_comment_pins(string change)
    {
        using InventoryFixture fixture = new();
        Dictionary<string, object?> pin = Prepare(fixture, "codeMoved");
        switch (change)
        {
            case "unknownReason": pin["reason"] = "framework"; break;
            case "changed": pin["code"] = "comment.changed"; break;
            case "missingOriginalPart": pin.Remove("originalPart"); break;
            case "missingOriginalText": pin.Remove("originalValue"); break;
            case "missingTarget": pin.Remove("targetMember"); break;
            case "missingTargetPart": pin.Remove("twinPart"); break;
            case "typeTarget": pin["targetMember"] = "$type"; break;
            case "absentTarget": pin["reason"] = "codeAbsent"; break;
            case "absentTwinValue":
                pin["reason"] = "codeAbsent";
                pin["twinValue"] = "Wrong observed comment.";
                pin.Remove("targetMember");
                pin.Remove("twinPart");
                break;
            case "missingReason": pin.Remove("reason"); break;
            case "emptyRationale": pin["rationale"] = " "; break;
        }

        WritePin(fixture, pin);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);
        action.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void Sweep_should_fail_on_an_unused_comment_pin()
    {
        using InventoryFixture fixture = new();
        Dictionary<string, object?> pin = Prepare(fixture, "codeAbsent");
        pin["typeName"] = "Sample.RemovedType";
        WritePin(fixture, pin);
        using BaselineFixture sweep = new();

        Action action = () => InventorySweepRunner.Run(sweep.Options with { FrameworkAdaptationsFile = fixture.FrameworkAdaptationsFile });

        action.Should().Throw<InvalidDataException>().WithMessage("*unapplied*Sample.RemovedType*");
    }

    [Test]
    public void Run_should_fail_an_absent_pin_when_the_comment_returns()
    {
        using InventoryFixture fixture = new();
        Dictionary<string, object?> pin = Prepare(fixture, "codeAbsent");
        WritePin(fixture, pin);
        fixture.WriteTwin("Widget.cs", Original);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);

        action.Should().Throw<InvalidDataException>().WithMessage("*stale or drifted*");
    }

    [Test]
    public void Run_should_reject_duplicate_comment_pins()
    {
        using InventoryFixture fixture = new();
        Dictionary<string, object?> pin = Prepare(fixture, "codeAbsent");
        WritePin(fixture, pin, pin);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);
        action.Should().Throw<InvalidDataException>().WithMessage("*duplicate*");
    }

    private static Dictionary<string, object?> Prepare(InventoryFixture fixture, string scenario)
    {
        const string text = "Keep the WinForms operation rationale. https://example.invalid/42";
        string originalPart = scenario.StartsWith("designer", StringComparison.Ordinal) ? "Widget.Designer.cs" : "Widget.cs";
        fixture.WriteOriginal(originalPart, Original);
        string? target = scenario switch
        {
            "adaptedMoved" => text.Replace("WinForms", "Avalonia", StringComparison.Ordinal),
            "codeAbsent" or "designerAbsent" => null,
            _ => text
        };
        string twin = Twin(target is null ? "" : "// " + target);
        fixture.WriteTwin("Widget.cs", IsAbsent(scenario) ? twin.Replace("Work();", "", StringComparison.Ordinal) : twin);
        FunctionalFinding finding = fixture.Run().Findings.Single(finding => finding.Category == "comments");
        Dictionary<string, object?> pin = new()
        {
            ["typeName"] = "Sample.Widget",
            ["category"] = "comments",
            ["code"] = finding.Code,
            ["path"] = finding.Path,
            ["originalPart"] = originalPart,
            ["originalValue"] = finding.OriginalValue,
            ["twinValue"] = finding.TwinValue,
            ["reason"] = IsAbsent(scenario) ? "codeAbsent" : "codeMoved",
            ["rationale"] = "Reviewed both methods and the whole type; the operation is absent or moved to Target."
        };
        if (!IsAbsent(scenario))
        {
            pin["twinPart"] = "Widget.cs";
            pin["targetMember"] = "method:Target()";
            if (target != text)
            {
                pin["targetText"] = target;
            }
        }

        return pin;
    }

    private static bool IsAbsent(string scenario) => scenario is "codeAbsent" or "absentDrifted" or "designerAbsent";

    private static void WritePin(InventoryFixture fixture, params Dictionary<string, object?>[] pins) =>
        fixture.WriteFrameworkAdaptations(JsonSerializer.Serialize(new { schemaVersion = 4, deviations = pins }));

    private const string Original = """
        namespace Sample;
        public sealed class Widget
        {
            private void Original()
            {
                // Keep the WinForms operation rationale. https://example.invalid/42
                Work();
            }

            private void Target() { }
        }
        """;

    private static string Twin(string comment) => $$"""
        namespace Sample;
        public sealed class Widget
        {
            private void Original() { }
            private void Target()
            {
                {{comment}}
                Work();
            }
        }
        """;
}
