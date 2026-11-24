using System.Text.Json;
using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Keeps reviewed absent and relocated comments exact without claiming their findings were fixed.
[TestFixture]
public sealed class ReviewedCommentDeviationTests
{
    [TestCase("mapped")]
    [TestCase("unmapped")]
    [TestCase("source")]
    [TestCase("partCase")]
    public void Run_should_bound_external_destinations_to_mapped_twin_part_directories(string change)
    {
        using InventoryFixture fixture = new();
        Dictionary<string, object?> pin = Prepare(fixture, "codeMoved");
        fixture.WriteTwin("../Portable/Widget.cs", Twin(""));
        const string target = "namespace Sample; class Destination { void Target() {\n// Keep the WinForms operation rationale. https://example.invalid/42\nWork(); } }";
        fixture.WriteTwin("../Portable/Destination.cs", target);
        fixture.WriteTwin("../Unmapped/Destination.cs", target);
        fixture.WriteOriginal("Destination.cs", target);
        SourceInventory original = fixture.Run().Original;
        SourceInventory twin = SourceInventoryReader.ReadFiles(
            fixture.TwinRoot, "Sample.Widget", new HashSet<string>(), isTwin: true,
            [Path.Combine(fixture.TwinRoot, "../Portable/Widget.cs")]);
        InventoryComparison comparison = InventoryComparer.Compare(original, twin);
        FunctionalFinding finding = comparison.Findings.Single(item => item.Category == "comments");
        pin["code"] = finding.Code;
        pin["twinValue"] = finding.TwinValue;
        pin["twinPart"] = change switch
        {
            "unmapped" => "../Unmapped/Destination.cs",
            "source" => "../original/Destination.cs",
            "partCase" => "../Portable/destination.cs",
            _ => "../Portable/Destination.cs"
        };
        pin["targetTypeName"] = "Sample.Destination";
        WritePin(fixture, pin);
        ReviewedFrameworkDeviationManifest manifest = ReviewedFrameworkDeviationManifest.Read(fixture.FrameworkAdaptationsFile);
        if (change != "mapped")
        {
            Action action = () => manifest.Apply("Sample.Widget", original, twin, comparison);
            action.Should().Throw<InvalidDataException>();
            return;
        }

        InventoryComparison accepted = manifest.Apply("Sample.Widget", original, twin, comparison);
        accepted.Findings.Should().NotContain(item => item.Category == "comments");
        accepted.AcceptedFrameworkDeviations.Should().ContainSingle().Subject.TwinPart.Should().Be("../Portable/Destination.cs");
    }

    [TestCase("valid")]
    [TestCase("nested")]
    [TestCase("type")]
    [TestCase("part")]
    [TestCase("partCase")]
    [TestCase("member")]
    [TestCase("text")]
    [TestCase("source")]
    [TestCase("outside")]
    [TestCase("order")]
    [TestCase("typeFormat")]
    public void Run_should_prove_the_exact_destination_when_code_moves_to_another_type(string change)
    {
        using InventoryFixture fixture = new();
        Dictionary<string, object?> pin = Prepare(fixture, "codeMoved");
        fixture.WriteTwin("Widget.cs", Twin(""));
        string target = "namespace Sample; class Destination { void Target() {\n// " + pin["originalValue"] + "\nWork(); } }";
        fixture.WriteTwin("Destination.cs", target);
        FunctionalFinding finding = fixture.Run().Findings.Single(item => item.Category == "comments");
        pin["code"] = finding.Code;
        pin["twinValue"] = finding.TwinValue;
        pin["twinPart"] = "Destination.cs";
        pin["targetTypeName"] = "Sample.Destination";
        switch (change)
        {
            case "nested":
                fixture.WriteTwin("Destination.cs", target.Replace("void Target()", "class Nested { void Target()", StringComparison.Ordinal) + "}");
                pin["targetMember"] = "class:Nested/method:Target()";
                break;
            case "type": pin["targetTypeName"] = "Sample.Other"; break;
            case "part": pin["twinPart"] = "Widget.cs"; break;
            case "partCase": pin["twinPart"] = "destination.cs"; break;
            case "member": pin["targetMember"] = "method:Other()"; break;
            case "text": fixture.WriteTwin("Destination.cs", target.Replace("WinForms", "changed", StringComparison.Ordinal)); break;
            case "source": fixture.WriteOriginal("Widget.cs", Original.Replace("WinForms", "changed", StringComparison.Ordinal)); break;
            case "outside": pin["twinPart"] = "../Outside.cs"; break;
            case "order": pin["targetCommentOrder"] = 1; break;
            case "typeFormat": pin["targetTypeName"] = 12; break;
        }

        WritePin(fixture, pin);
        if (change is not ("valid" or "nested"))
        {
            Action action = () => fixture.Run(useFrameworkAdaptations: true);
            action.Should().Throw<InvalidDataException>();
            return;
        }

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);
        report.Findings.Should().NotContain(item => item.Category == "comments");
        AcceptedFrameworkDeviation accepted = report.AcceptedFrameworkDeviations.Should().ContainSingle().Subject;
        accepted.TargetTypeName.Should().Be("Sample.Destination");
        accepted.TwinPart.Should().Be("Destination.cs");
    }

    [TestCase("first")]
    [TestCase("second")]
    [TestCase("missingOrder")]
    [TestCase("negativeOrder")]
    [TestCase("wrongOrder")]
    [TestCase("orderFormat")]
    [TestCase("reordered")]
    [TestCase("absent")]
    public void Run_should_pin_empty_moved_comment_separators_by_exact_destination_order(string change)
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { void Original() {\n//\nWork(); } }");
        const string twin = "namespace Sample; class Widget { void Original() { } void Target() {\n//\n//\nWork(); } }";
        fixture.WriteTwin("Widget.cs", twin);
        FunctionalFinding finding = fixture.Run().Findings.Single(item => item.Category == "comments");
        Dictionary<string, object?> pin = new()
        {
            ["typeName"] = "Sample.Widget", ["category"] = "comments", ["code"] = finding.Code,
            ["path"] = finding.Path, ["originalPart"] = "Widget.cs", ["originalValue"] = finding.OriginalValue,
            ["twinValue"] = finding.TwinValue, ["reason"] = "codeMoved", ["twinPart"] = "Widget.cs",
            ["targetMember"] = "method:Target()", ["targetCommentOrder"] = change == "second" ? 1 : 0,
            ["rationale"] = "The separator stays with the moved comment block and its exact member/order."
        };
        switch (change)
        {
            case "missingOrder": pin.Remove("targetCommentOrder"); break;
            case "negativeOrder": pin["targetCommentOrder"] = -1; break;
            case "wrongOrder": pin["targetCommentOrder"] = 2; break;
            case "orderFormat": pin["targetCommentOrder"] = "0"; break;
            case "reordered": fixture.WriteTwin("Widget.cs", twin.Replace("//\n//", "// Changed.\n//", StringComparison.Ordinal)); break;
            case "absent":
                pin["reason"] = "codeAbsent";
                pin.Remove("twinPart");
                pin.Remove("targetMember");
                pin.Remove("targetCommentOrder");
                break;
        }

        WritePin(fixture, pin);
        if (change is not ("first" or "second"))
        {
            Action action = () => fixture.Run(useFrameworkAdaptations: true);
            action.Should().Throw<InvalidDataException>();
            return;
        }

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);
        report.Findings.Should().NotContain(item => item.Category == "comments");
        report.AcceptedFrameworkDeviations.Should().ContainSingle().Subject.TargetCommentOrder.Should().Be(change == "second" ? 1 : 0);
    }

    [TestCase("valid")]
    [TestCase("sourceText")]
    [TestCase("twinText")]
    [TestCase("sourcePart")]
    [TestCase("twinPart")]
    [TestCase("restored")]
    [TestCase("missingTwinText")]
    [TestCase("missingOriginalText")]
    [TestCase("wrongReason")]
    [TestCase("decoyPart")]
    public void Run_should_exactly_pin_minimally_adapted_comments(string change)
    {
        using InventoryFixture fixture = new();
        const string original = "namespace Sample; class Widget { void Work() {\n// The WinForms control owns the native handle.\nSave(); } }";
        const string adapted = "The Avalonia control owns the visual.";
        fixture.WriteOriginal("Widget.cs", original);
        fixture.WriteTwin("Widget.cs", original.Replace("The WinForms control owns the native handle.", adapted, StringComparison.Ordinal));
        FunctionalFinding finding = fixture.Run().Findings.Single(item => item.Code == "comment.changed");
        Dictionary<string, object?> pin = new()
        {
            ["typeName"] = "Sample.Widget", ["category"] = "comments", ["code"] = finding.Code,
            ["path"] = finding.Path, ["originalPart"] = "Widget.cs", ["originalValue"] = finding.OriginalValue,
            ["twinPart"] = "Widget.cs", ["twinValue"] = finding.TwinValue,
            ["reason"] = "adaptedMinimal", ["rationale"] = "Only the false framework ownership facts change."
        };
        switch (change)
        {
            case "sourceText": pin["originalValue"] = "Changed original."; break;
            case "twinText": pin["twinValue"] = "Changed twin."; break;
            case "sourcePart": pin["originalPart"] = "Other.cs"; break;
            case "twinPart": pin["twinPart"] = "Other.cs"; break;
            case "restored": fixture.WriteTwin("Widget.cs", original); break;
            case "missingTwinText": pin.Remove("twinValue"); break;
            case "missingOriginalText": pin.Remove("originalValue"); break;
            case "wrongReason": pin["reason"] = "codeAbsent"; break;
            case "decoyPart":
                fixture.WriteTwin("Other.cs", "namespace Sample; partial class Widget { void Other() {\n// " + adapted + "\n} }");
                pin["twinPart"] = "Other.cs";
                break;
        }

        WritePin(fixture, pin);
        if (change != "valid")
        {
            Action action = () => fixture.Run(useFrameworkAdaptations: true);
            action.Should().Throw<InvalidDataException>();
            return;
        }

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);
        report.Findings.Should().NotContain(item => item.Code == "comment.changed");
        AcceptedFrameworkDeviation accepted = report.AcceptedFrameworkDeviations.Should().ContainSingle().Subject;
        accepted.Reason.Should().Be("adaptedMinimal");
        accepted.OriginalValue.Should().Be(finding.OriginalValue);
        accepted.TwinValue.Should().Be(adapted);
        accepted.OriginalPart.Should().Be("Widget.cs");
        accepted.TwinPart.Should().Be("Widget.cs");
    }

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
    [TestCase("emptyTargetText")]
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
            case "emptyTargetText": pin["targetText"] = ""; break;
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
