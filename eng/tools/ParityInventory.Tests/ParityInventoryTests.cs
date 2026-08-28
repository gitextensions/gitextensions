using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Proves the temporary functional inventory schema and extractors.
[TestFixture]
[Category("P0_4")]
public sealed class ParityInventoryTests
{
    [Test]
    public void Run_should_retain_comment_and_event_cascades_under_the_missing_handler_root()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private void Wire() => button.Click += HandleClick;

                // Preserve the product action.
                private void HandleClick(object sender, EventArgs e) { }
            }
            """);
        fixture.WriteTwin("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private void Wire() { }
            }
            """);

        InventoryReport report = fixture.Run();

        report.Findings.Should().Contain(item =>
            item.Code == "member.missing" && item.Path == "member/method:HandleClick");
        report.Findings.Should().NotContain(item =>
            item.Code == "comment.missing"
            || item.Code == "event.handler.missing"
            || item.Code == "event.wiring.missing");
        report.DependentFindings.Should().HaveCount(3).And.OnlyContain(item =>
            item.RootPath == "member/method:HandleClick");
        report.DependentFindings.Select(item => item.Finding.Code).Should().BeEquivalentTo(
            "comment.missing",
            "event.handler.missing",
            "event.wiring.missing");
        report.Summary.FindingCount.Should().Be(report.Findings.Count);
        report.Summary.DependentFindingCount.Should().Be(3);
        report.Summary.TotalDifferenceCount.Should().Be(report.Findings.Count + 3);
    }

    [Test]
    public void Run_should_keep_comment_and_event_findings_actionable_when_the_handler_exists()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private void Wire() => button.Click += HandleClick;

                // Preserve the product action.
                private void HandleClick(object sender, EventArgs e) { }
            }
            """);
        fixture.WriteTwin("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private void Wire() { }
                private void HandleClick(object sender, EventArgs e) { }
            }
            """);

        InventoryReport report = fixture.Run();

        report.DependentFindings.Should().BeEmpty();
        report.Findings.Should().Contain(item => item.Code == "comment.missing");
        report.Findings.Should().Contain(item => item.Code == "event.wiring.missing");
    }

    [Test]
    public void Run_should_report_missing_partial_with_expected_twin_path()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public partial class Widget { }
            """);
        fixture.WriteOriginal("Widget.Toolbar.cs", """
            namespace Sample;
            public partial class Widget { private void BuildToolbar() { } }
            """);
        fixture.WriteOriginal("Widget.Designer.cs", """
            namespace Sample;
            public partial class Widget { }
            """);
        fixture.WriteTwin("Widget.axaml.cs", """
            namespace Sample;
            public partial class Widget { }
            """);
        fixture.WriteTwin("Widget.axaml", """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         x:Class="Sample.Widget" />
            """);

        InventoryReport report = fixture.Run();

        FunctionalFinding finding = report.Findings.Should()
            .ContainSingle(item => item.Code == "partial.missing").Subject;
        finding.OriginalValue.Should().Be("Widget.Toolbar.cs");
        finding.Path.Should().Be("part/Widget.Toolbar.cs");
    }

    [Test]
    public void Run_should_expect_same_path_for_nonvisual_primary_source()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { private int value; }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { private int value; }");

        InventoryReport report = fixture.Run();

        report.Findings.Should().BeEmpty();
        report.Original.Parts.Should().ContainSingle()
            .Which.ExpectedTwinPath.Should().Be("Widget.cs");
    }

    [Test]
    public void Run_should_extract_members_events_hotkeys_and_settings()
    {
        const string code = """
            namespace Sample;
            public partial class Widget
            {
                private readonly ISettingsSource? settings;
                private int count;
                public string Name { get; set; }
                public Widget(ISettingsSource settings)
                {
                    this.settings = settings;
                    button.Click += HandleClick;
                    _ = AppSettings.ShowGitStatusInToolbar;
                    this.settings.SetBool("widget.enabled", true);
                }
                private void HandleClick(object sender, EventArgs e) { }
                private void ExecuteCommand(int command)
                {
                    switch (command)
                    {
                        case 42:
                            break;
                    }
                }
            }
            """;
        using InventoryFixture fixture = new();
        fixture.WriteMatching(code);

        SourceInventory inventory = fixture.Run().Original;

        inventory.Members.Select(item => item.Name).Should().ContainInOrder(
            "settings", "count", "Name", "Widget", "HandleClick", "ExecuteCommand");
        inventory.EventWiring.Should().ContainSingle(item =>
            item.Target == "button" && item.Event == "Click" && item.Handler == "HandleClick");
        inventory.EventHandlers.Should().Contain("HandleClick");
        inventory.HotkeyCommandIds.Should().Contain("42");
        inventory.Settings.Should().Contain(item =>
            item.Key == "ShowGitStatusInToolbar" && item.Access == "read");
        inventory.Settings.Should().Contain(item =>
            item.Key == "SetBool:\"widget.enabled\"" && item.Access == "write");
    }

    [TestCase("struct")]
    [TestCase("interface")]
    public void Run_should_extract_non_class_type_members_and_comments(string declarationKind)
    {
        string code = $$"""
            namespace Sample;
            /// <summary>Comparable type.</summary>
            public {{declarationKind}} Widget
            {
                /// <summary>Comparable value.</summary>
                public int Value { get; set; }
            }
            """;
        using InventoryFixture fixture = new();
        fixture.WriteMatching(code);

        SourceInventory inventory = fixture.Run().Original;

        inventory.Members.Should().ContainSingle(item => item.Name == "Value");
        inventory.Comments.Select(item => item.Text).Should().Contain(
            "<summary>Comparable type.</summary>",
            "<summary>Comparable value.</summary>");
    }

    [Test]
    public void Run_should_diff_member_accessibility_signature_and_order()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public partial class Widget
            {
                private int first;
                public string Value { get; set; }
            }
            """);
        fixture.WriteTwin("Widget.axaml.cs", """
            namespace Sample;
            public partial class Widget
            {
                internal int Value { get; set; }
                private int first;
            }
            """);

        InventoryReport report = fixture.Run();

        report.Findings.Select(item => item.Code).Should().Contain(
            "member.accessibility",
            "member.signature",
            "member.order");
    }

    [Test]
    public void Run_should_ignore_formatter_whitespace_inside_parameter_lists()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { private void Run(string value) { } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { private void Run( string value ) { } }");

        InventoryReport report = fixture.Run();

        report.Findings.Should().NotContain(item => item.Code == "member.signature");
    }

    [Test]
    public void Run_should_match_designer_fields_with_axaml_named_controls()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Designer.cs", """
            namespace Sample;
            public partial class Widget
            {
                private Button actionButton;
                private TextBox inputText;
            }
            """);
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml", """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         x:Class="Sample.Widget">
              <StackPanel>
                <TextBox x:Name="inputText" />
                <Button x:Name="actionButton" />
              </StackPanel>
            </UserControl>
            """);

        InventoryReport report = fixture.Run();

        report.Findings.Should().NotContain(item =>
            (item.Code == "member.missing" || item.Code == "member.extra")
            && (item.Path.EndsWith("field:actionButton", StringComparison.Ordinal)
                || item.Path.EndsWith("field:inputText", StringComparison.Ordinal)));
        report.Findings.Should().NotContain(item => item.Code == "member.signature");
        report.Findings.Should().NotContain(item => item.Code == "member.order");
    }

    [Test]
    public void Run_should_match_qualified_designer_field_with_axaml_xmlns_type()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Designer.cs", """
            namespace Sample;
            public partial class Widget
            {
                private Sample.Controls.NativeList nativeList;
            }
            """);
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml", """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:controls="using:Sample.Controls"
                         x:Class="Sample.Widget">
              <controls:NativeList x:Name="nativeList" />
            </UserControl>
            """);

        InventoryReport report = fixture.Run();

        report.Findings.Should().NotContain(item =>
            item.Code == "member.signature"
            && item.Path.Contains("nativeList", StringComparison.Ordinal));
    }

    [Test]
    public void Run_should_honor_axaml_field_modifier_accessibility()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Designer.cs", "namespace Sample; public partial class Widget { internal Button actionButton; }");
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml", """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         x:Class="Sample.Widget">
              <Button x:Name="actionButton" x:FieldModifier="internal" />
            </UserControl>
            """);

        InventoryReport report = fixture.Run();

        report.Findings.Should().NotContain(item =>
            item.Code == "member.accessibility" && item.Path.Contains("actionButton", StringComparison.Ordinal));
    }

    [TestCase("SelectedIndexChanged", "SelectionChanged")]
    [TestCase("Resize", "SizeChanged")]
    [TestCase("Enter", "GotFocus")]
    [TestCase("Leave", "LostFocus")]
    public void Run_should_match_framework_equivalent_event_names(string originalEvent, string twinEvent)
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", $$"""
            namespace Sample;
            public partial class Widget
            {
                private void Wire() => control.{{originalEvent}} += HandleChanged;
                private void HandleChanged(object sender, EventArgs e) { }
            }
            """);
        fixture.WriteTwin("Widget.axaml.cs", $$"""
            namespace Sample;
            public partial class Widget
            {
                private void Wire() => control.{{twinEvent}} += HandleChanged;
                private void HandleChanged(object sender, EventArgs e) { }
            }
            """);

        InventoryReport report = fixture.Run();

        report.Findings.Should().NotContain(item => item.Code.StartsWith("event.wiring", StringComparison.Ordinal));
    }

    [Test]
    public void Run_should_compare_relative_order_without_cascading_after_framework_only_member()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private int first;
                private int second;
            }
            """);
        fixture.WriteTwin("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private int frameworkOnly;
                private int first;
                private int second;
            }
            """);

        InventoryReport report = fixture.Run();

        report.Findings.Should().NotContain(item => item.Code == "member.order");
        report.Findings.Should().ContainSingle(item =>
            item.Code == "member.extra" && item.Path == "member/field:frameworkOnly");
    }

    [Test]
    public void Compare_should_order_linked_part_by_its_mapped_twin_path()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Part.cs", "namespace Sample; public partial class Widget { private int first; }");
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public partial class Widget { private int second; }");
        fixture.WriteTwin("Widget.Part.cs", "namespace Sample; public partial class Widget { private int first; }");
        fixture.WriteTwin("Linked/Widget.cs", "namespace Sample; public partial class Widget { private int second; }");

        SourceInventory original = SourceInventoryReader.Read(
            fixture.OriginalRoot,
            "Sample.Widget",
            new HashSet<string>(StringComparer.Ordinal),
            isTwin: false);
        original = original with
        {
            Parts = original.Parts.Select(part => part.Path == "Widget.cs"
                ? part with { ExpectedTwinPath = "Linked/Widget.cs" }
                : part).ToArray()
        };
        SourceInventory twin = SourceInventoryReader.Read(
            fixture.TwinRoot,
            "Sample.Widget",
            new HashSet<string>(StringComparer.Ordinal),
            isTwin: true);

        InventoryComparison comparison = InventoryComparer.Compare(original, twin);

        comparison.Findings.Should().NotContain(item => item.Code == "member.order");
    }

    [Test]
    public void Compare_should_still_report_a_member_moved_out_of_its_mapped_partial()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Part.cs", "namespace Sample; public partial class Widget { private int first; }");
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public partial class Widget { private int second; }");
        fixture.WriteTwin("Widget.Part.cs", "namespace Sample; public partial class Widget { private int first; private int second; }");
        fixture.WriteTwin("Linked/Widget.cs", "namespace Sample; public partial class Widget { }");

        SourceInventory original = SourceInventoryReader.Read(
            fixture.OriginalRoot,
            "Sample.Widget",
            new HashSet<string>(StringComparer.Ordinal),
            isTwin: false);
        original = original with
        {
            Parts = original.Parts.Select(part => part.Path == "Widget.cs"
                ? part with { ExpectedTwinPath = "Linked/Widget.cs" }
                : part).ToArray()
        };
        SourceInventory twin = SourceInventoryReader.Read(
            fixture.TwinRoot,
            "Sample.Widget",
            new HashSet<string>(StringComparer.Ordinal),
            isTwin: true);

        InventoryComparison comparison = InventoryComparer.Compare(original, twin);

        comparison.Findings.Should().ContainSingle(item =>
            item.Code == "member.partial" && item.Path == "member/field:second/part");
    }

    [Test]
    public void Run_should_record_an_exact_reviewed_framework_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { private int frameworkOnly; }");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "code": "member.extra",
                  "path": "member/field:frameworkOnly",
                  "twinPart": "Widget.cs",
                  "twinAccessibility": "private",
                  "twinSignature": "int frameworkOnly",
                  "rationale": "Avalonia requires this exact framework-owned state."
                }
              ]
            }
            """);

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);

        report.Findings.Should().BeEmpty();
        report.AcceptedFrameworkDeviations.Should().ContainSingle(item =>
            item.Code == "member.extra"
            && item.Path == "member/field:frameworkOnly"
            && item.TwinPart == "Widget.cs"
            && item.TwinValue == "private int frameworkOnly"
            && item.Rationale == "Avalonia requires this exact framework-owned state.");
    }

    [Test]
    public void Run_should_record_an_exact_reviewed_missing_generated_member()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Designer.cs", """
            namespace Sample;
            public partial class Widget
            {
                protected void Dispose(bool disposing) { }
            }
            """);
        fixture.WriteTwin("Widget.axaml.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml", "<UserControl />");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "code": "member.missing",
                  "path": "member/method:Dispose",
                  "originalPart": "Widget.Designer.cs",
                  "originalAccessibility": "protected",
                  "originalSignature": "void Dispose(bool disposing)",
                  "rationale": "AXAML replaces the generated WinForms component-container disposal hook."
                }
              ]
            }
            """);

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);

        report.Findings.Should().NotContain(item => item.Code == "member.missing");
        report.AcceptedFrameworkDeviations.Should().ContainSingle(item =>
            item.Code == "member.missing"
            && item.OriginalPart == "Widget.Designer.cs"
            && item.OriginalValue == "protected void Dispose(bool disposing)"
            && item.TwinPart == "(no Avalonia member)"
            && item.TwinValue == null);
    }

    [Test]
    public void Run_should_reject_source_drift_in_a_reviewed_missing_generated_member()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Designer.cs", """
            namespace Sample;
            public partial class Widget
            {
                public void Dispose(bool disposing) { }
            }
            """);
        fixture.WriteTwin("Widget.axaml.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml", "<UserControl />");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "code": "member.missing",
                  "path": "member/method:Dispose",
                  "originalPart": "Widget.Designer.cs",
                  "originalAccessibility": "protected",
                  "originalSignature": "void Dispose(bool disposing)",
                  "rationale": "Any source change requires a fresh review."
                }
              ]
            }
            """);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);

        action.Should().Throw<InvalidDataException>().WithMessage("*drifted*");
    }

    [Test]
    public void Run_should_reject_a_behavior_bearing_reviewed_missing_member()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { private void Run() { } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { }");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "code": "member.missing",
                  "path": "member/method:Run",
                  "originalPart": "Widget.cs",
                  "originalAccessibility": "private",
                  "originalSignature": "void Run()",
                  "rationale": "Behavior-bearing members must remain ordinary findings."
                }
              ]
            }
            """);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);

        action.Should().Throw<InvalidDataException>()
            .WithMessage("*not generated WinForms Designer lifecycle state*");
    }

    [Test]
    public void Run_should_record_an_exact_reviewed_field_signature_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { private Label caption; }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { private TextBlock caption; }");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "code": "member.signature",
                  "path": "member/field:caption/signature",
                  "originalPart": "Widget.cs",
                  "originalAccessibility": "private",
                  "originalSignature": "Label caption",
                  "twinPart": "Widget.cs",
                  "twinAccessibility": "private",
                  "twinSignature": "TextBlock caption",
                  "rationale": "Compiled AXAML generates the native Avalonia control field type."
                }
              ]
            }
            """);

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);

        report.Findings.Should().BeEmpty();
        report.AcceptedFrameworkDeviations.Should().ContainSingle(item =>
            item.Code == "member.signature"
            && item.Path == "member/field:caption/signature"
            && item.OriginalPart == "Widget.cs"
            && item.OriginalValue == "private Label caption"
            && item.TwinPart == "Widget.cs"
            && item.TwinValue == "private TextBlock caption");
    }

    [Test]
    public void Run_should_reject_source_drift_in_a_reviewed_field_signature_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { private LinkLabel caption; }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { private TextBlock caption; }");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "code": "member.signature",
                  "path": "member/field:caption/signature",
                  "originalPart": "Widget.cs",
                  "originalAccessibility": "private",
                  "originalSignature": "Label caption",
                  "twinPart": "Widget.cs",
                  "twinAccessibility": "private",
                  "twinSignature": "TextBlock caption",
                  "rationale": "Any source type change requires a fresh review."
                }
              ]
            }
            """);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);

        action.Should().Throw<InvalidDataException>().WithMessage("*source drifted*");
    }

    [Test]
    public void Run_should_record_an_exact_reviewed_designer_comment_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Designer.cs", """
            namespace Sample;
            public partial class Widget
            {
                // Generated WinForms cleanup contract.
                private void Dispose(bool disposing) { }
            }
            """);
        fixture.WriteTwin("Widget.axaml.cs", """
            namespace Sample;
            public partial class Widget
            {
                private void Dispose(bool disposing) { }
            }
            """);
        fixture.WriteTwin("Widget.axaml", "<UserControl />");
        FunctionalFinding finding = fixture.Run().Findings.Single(item => item.Code == "comment.missing");
        fixture.WriteFrameworkAdaptations($$"""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "category": "comments",
                  "code": "comment.missing",
                  "path": {{System.Text.Json.JsonSerializer.Serialize(finding.Path)}},
                  "originalPart": "Widget.Designer.cs",
                  "originalValue": {{System.Text.Json.JsonSerializer.Serialize(finding.OriginalValue)}},
                  "twinPart": "Widget.axaml",
                  "rationale": "AXAML replaces generated WinForms Designer comments."
                }
              ]
            }
            """);

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);

        report.Findings.Should().NotContain(item => item.Code == "comment.missing");
        report.AcceptedFrameworkDeviations.Should().ContainSingle(item =>
            item.Category == "comments"
            && item.Path == finding.Path
            && item.OriginalValue == finding.OriginalValue);
    }

    [Test]
    public void Run_should_record_exact_reviewed_event_wiring_adaptations()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private void Wire() => button.Click += OnClick;
                private void OnClick(object sender, EventArgs e) { }
            }
            """);
        fixture.WriteTwin("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private void Wire() => button.PointerReleased += OnClick;
                private void OnClick(object sender, EventArgs e) { }
            }
            """);
        FunctionalFinding missing = fixture.Run().Findings.Single(item => item.Code == "event.wiring.missing");
        FunctionalFinding extra = fixture.Run().Findings.Single(item => item.Code == "event.wiring.extra");
        fixture.WriteFrameworkAdaptations($$"""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "category": "events",
                  "code": "event.wiring.missing",
                  "path": {{System.Text.Json.JsonSerializer.Serialize(missing.Path)}},
                  "originalPart": "Widget.cs",
                  "originalValue": {{System.Text.Json.JsonSerializer.Serialize(missing.OriginalValue)}},
                  "rationale": "Avalonia exposes the equivalent pointer event."
                },
                {
                  "typeName": "Sample.Widget",
                  "category": "events",
                  "code": "event.wiring.extra",
                  "path": {{System.Text.Json.JsonSerializer.Serialize(extra.Path)}},
                  "twinPart": "Widget.cs",
                  "twinValue": {{System.Text.Json.JsonSerializer.Serialize(extra.TwinValue)}},
                  "rationale": "Avalonia exposes the equivalent pointer event."
                }
              ]
            }
            """);

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);

        report.Findings.Should().BeEmpty();
        report.AcceptedFrameworkDeviations.Should().HaveCount(2);
    }

    [Test]
    public void Run_should_reject_drift_in_an_exact_reviewed_event_wiring_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private void Wire() => button.Click += OnClick;
                private void OnClick(object sender, EventArgs e) { }
            }
            """);
        fixture.WriteTwin("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private void Wire() => button.PointerReleased += OnClick;
                private void OnClick(object sender, EventArgs e) { }
            }
            """);
        FunctionalFinding finding = fixture.Run().Findings.Single(item => item.Code == "event.wiring.extra");
        fixture.WriteFrameworkAdaptations($$"""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "category": "events",
                  "code": "event.wiring.extra",
                  "path": {{System.Text.Json.JsonSerializer.Serialize(finding.Path)}},
                  "twinPart": "Widget.cs",
                  "twinValue": "drifted wiring value",
                  "rationale": "Any event change requires a fresh review."
                }
              ]
            }
            """);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);

        action.Should().Throw<InvalidDataException>().WithMessage("*stale or drifted*");
    }

    [Test]
    public void Run_should_apply_an_exact_reviewed_extra_event_handler_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { }");
        fixture.WriteTwin("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private void Wire() => button.PointerReleased += OnPointerReleased;
                private void OnPointerReleased(object sender, PointerEventArgs e) { }
            }
            """);
        FunctionalFinding finding = fixture.Run().Findings.Single(item => item.Code == "event.handler.extra");
        fixture.WriteFrameworkAdaptations($$"""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "category": "events",
                  "code": "event.handler.extra",
                  "path": {{System.Text.Json.JsonSerializer.Serialize(finding.Path)}},
                  "twinValue": {{System.Text.Json.JsonSerializer.Serialize(finding.TwinValue)}},
                  "rationale": "Avalonia requires a routed pointer-event handler."
                }
              ]
            }
            """);

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);

        report.AcceptedFrameworkDeviations.Should().ContainSingle(item =>
            item.Code == "event.handler.extra" && item.Path == finding.Path);
        report.Findings.Should().Contain(item => item.Code == "event.wiring.extra");
    }

    [Test]
    public void Run_should_reject_a_reviewed_handwritten_comment_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                // Explains product behavior.
                private void Run() { }
            }
            """);
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { private void Run() { } }");
        FunctionalFinding finding = fixture.Run().Findings.Single(item => item.Code == "comment.missing");
        fixture.WriteFrameworkAdaptations($$"""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "category": "comments",
                  "code": "comment.missing",
                  "path": {{System.Text.Json.JsonSerializer.Serialize(finding.Path)}},
                  "originalPart": "Widget.cs",
                  "originalValue": {{System.Text.Json.JsonSerializer.Serialize(finding.OriginalValue)}},
                  "twinPart": "Widget.cs",
                  "rationale": "Handwritten comments must not be accepted."
                }
              ]
            }
            """);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);

        action.Should().Throw<InvalidDataException>().WithMessage("*not an exact supported generated-comment*");
    }

    [Test]
    public void Run_should_reject_a_stale_reviewed_framework_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteMatching("namespace Sample; public sealed class Widget { private int frameworkOnly; }");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "code": "member.extra",
                  "path": "member/field:frameworkOnly",
                  "twinPart": "Widget.axaml.cs",
                  "twinAccessibility": "private",
                  "twinSignature": "int frameworkOnly",
                  "rationale": "This entry must disappear when the source difference disappears."
                }
              ]
            }
            """);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);

        action.Should().Throw<InvalidDataException>().WithMessage("*is stale*");
    }

    [Test]
    public void Run_should_reject_signature_drift_in_a_reviewed_framework_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { private long frameworkOnly; }");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "code": "member.extra",
                  "path": "member/field:frameworkOnly",
                  "twinPart": "Widget.cs",
                  "twinAccessibility": "private",
                  "twinSignature": "int frameworkOnly",
                  "rationale": "A type change requires a fresh review."
                }
              ]
            }
            """);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);

        action.Should().Throw<InvalidDataException>().WithMessage("*drifted*");
    }

    [Test]
    public void Run_should_reject_accessibility_drift_in_a_reviewed_framework_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { public int frameworkOnly; }");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 4,
              "deviations": [
                {
                  "typeName": "Sample.Widget",
                  "code": "member.extra",
                  "path": "member/field:frameworkOnly",
                  "twinPart": "Widget.cs",
                  "twinAccessibility": "private",
                  "twinSignature": "int frameworkOnly",
                  "rationale": "An accessibility change requires a fresh review."
                }
              ]
            }
            """);

        Action action = () => fixture.Run(useFrameworkAdaptations: true);

        action.Should().Throw<InvalidDataException>().WithMessage("*drifted*");
    }

    [Test]
    public void Run_should_extract_csharp_and_axaml_menu_trees()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Designer.cs", """
            namespace Sample;
            public partial class Widget
            {
                private ContextMenuStrip menu;
                private ToolStripMenuItem open;
                private ToolStripSeparator separator;
                private void InitializeComponent()
                {
                    menu.Items.AddRange(new ToolStripItem[] { open, separator });
                }
            }
            """);
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml", """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         xmlns:wf="using:Sample.Compat"
                         x:Class="Sample.Widget">
              <wf:ContextMenuStrip x:Name="menu">
                <wf:ToolStripMenuItem x:Name="open" Header="Open" />
                <wf:ToolStripSeparator x:Name="separator" />
              </wf:ContextMenuStrip>
            </UserControl>
            """);

        InventoryReport report = fixture.Run();

        report.Original.Menus.Should().Contain(item =>
            item.Parent == "menu" && item.Name == "open");
        report.Twin.Menus.Should().Contain(item =>
            item.Parent == "menu" && item.Name == "open");
        report.Twin.Menus.Should().Contain(item =>
            item.Parent == "menu" && item.Name == "separator" && item.Kind == "separator");
    }

    [Test]
    public void Run_should_align_menu_sequences_after_an_omitted_item()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Designer.cs", """
            namespace Sample;
            public partial class Widget
            {
                private ContextMenuStrip menu;
                private ToolStripMenuItem first;
                private ToolStripMenuItem omitted;
                private ToolStripMenuItem last;
                private void InitializeComponent()
                {
                    menu.Items.AddRange(new ToolStripItem[] { first, omitted, last });
                }
            }
            """);
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml", """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         x:Class="Sample.Widget">
              <ContextMenu x:Name="menu">
                <MenuItem x:Name="first" Header="First" />
                <MenuItem x:Name="last" Header="Last" />
              </ContextMenu>
            </UserControl>
            """);

        InventoryReport report = fixture.Run();

        report.Findings.Where(item => item.Category == "menus").Should().ContainSingle(item =>
            item.Code == "menu.item.missing" && item.Path.EndsWith(":omitted", StringComparison.Ordinal));
    }

    [Test]
    public void Run_should_not_treat_non_menu_items_collections_as_menus()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private readonly ComboBox choices = new();
                private void Populate() => choices.Items.Add("choice");
            }
            """);
        fixture.WriteTwin("Widget.cs", """
            namespace Sample;
            public sealed class Widget
            {
                private readonly ComboBox choices = new();
                private void Populate() => choices.Items.Add("choice");
            }
            """);

        InventoryReport report = fixture.Run();

        report.Original.Menus.Should().BeEmpty();
        report.Twin.Menus.Should().BeEmpty();
    }

    [Test]
    public void Run_should_extract_translation_strings_and_catalog_presence()
    {
        const string code = """
            namespace Sample;
            public partial class Widget
            {
                private readonly TranslationString _caption = new("Caption");
            }
            """;
        using InventoryFixture fixture = new();
        fixture.WriteMatching(code);
        fixture.WriteEnglish("_caption.Text");

        InventoryReport report = fixture.Run();

        report.Original.TranslationStrings.Should().ContainSingle(item => item.Name == "_caption");
        report.Original.TranslationKeys.Should().ContainSingle(item =>
            item.Key == "_caption.Text" && item.InEnglishCatalog);
    }

    [Test]
    public void Run_should_use_the_original_translation_category_override()
    {
        const string code = """
            namespace Sample;
            public partial class Widget
            {
                private readonly TranslationString _caption = new("Caption");
                protected override string TranslationCategoryName => "SharedCategory";
            }
            """;
        using InventoryFixture fixture = new();
        fixture.WriteMatching(code);
        fixture.WriteEnglishCategory("SharedCategory", "_caption.Text");

        InventoryReport report = fixture.Run();

        report.Twin.TranslationKeys.Should().ContainSingle(item =>
            item.Key == "_caption.Text" && item.InEnglishCatalog);
        report.Findings.Should().NotContain(item => item.Code == "translation.not-in-english");
    }

    [Test]
    public void Run_should_recognize_named_content_control_with_text_block_content_as_a_translation_key()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.Designer.cs", """
            namespace Sample;
            public partial class Widget
            {
                private Label helpTextLbl;
                private void InitializeComponent()
                {
                    helpTextLbl.Text = "Translated help";
                }
            }
            """);
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml.cs", "namespace Sample; public partial class Widget { }");
        fixture.WriteTwin("Widget.axaml", """
            <UserControl xmlns="https://github.com/avaloniaui"
                         xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                         x:Class="Sample.Widget">
              <Label x:Name="helpTextLbl">
                <TextBlock Text="Translated help" />
              </Label>
            </UserControl>
            """);
        fixture.WriteEnglish("helpTextLbl.Text");

        InventoryReport report = fixture.Run();

        report.Twin.TranslationKeys.Should().ContainSingle(item =>
            item.Key == "helpTextLbl.Text" && item.InEnglishCatalog);
        report.Findings.Should().NotContain(item =>
            item.Code == "translation.key.missing" && item.Path == "translation.key/helpTextLbl.Text");
    }

    [Test]
    public void Run_should_not_report_a_shared_upstream_catalog_omission_as_port_debt()
    {
        const string code = """
            namespace Sample;
            public partial class Widget
            {
                private readonly TranslationString _caption = new("Caption");
            }
            """;
        using InventoryFixture fixture = new();
        fixture.WriteMatching(code);

        InventoryReport report = fixture.Run();

        report.Twin.TranslationKeys.Should().ContainSingle(item =>
            item.Key == "_caption.Text" && !item.InEnglishCatalog);
        report.Findings.Should().NotContain(item => item.Code == "translation.not-in-english");
    }

    [Test]
    public void Run_should_not_treat_runtime_text_assignment_as_designer_translation_key()
    {
        const string code = """
            namespace Sample;
            public sealed class Widget
            {
                private readonly Label _label = new();
                private void Update() => _label.Text = "runtime status";
            }
            """;
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", code);
        fixture.WriteTwin("Widget.cs", code);

        InventoryReport report = fixture.Run();

        report.Original.TranslationKeys.Should().BeEmpty();
        report.Findings.Should().NotContain(item => item.Code == "translation.not-in-english");
    }

    [Test]
    public void Run_should_write_byte_identical_json_for_identical_inputs()
    {
        using InventoryFixture fixture = new();
        fixture.WriteMatching("namespace Sample; public partial class Widget { private int value; }");

        fixture.Run();
        byte[] first = File.ReadAllBytes(fixture.OutputFile);
        fixture.Run();
        byte[] second = File.ReadAllBytes(fixture.OutputFile);

        second.Should().Equal(first);
    }
}

// parity-scaffolding: Provides isolated cross-platform source trees for inventory tests.
internal sealed class InventoryFixture : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "gitextensions-parity-inventory-tests",
        Guid.NewGuid().ToString("N"));

    public InventoryFixture()
    {
        Directory.CreateDirectory(OriginalRoot);
        Directory.CreateDirectory(TwinRoot);
        WriteEnglish();
    }

    public string OriginalRoot => Path.Combine(_root, "original");

    public string TwinRoot => Path.Combine(_root, "twin");

    public string OutputFile => Path.Combine(_root, "output", "functional-findings.json");

    public string FrameworkAdaptationsFile => Path.Combine(_root, "reviewed-framework-adaptations.json");

    public void WriteMatching(string code)
    {
        WriteOriginal("Widget.cs", code);
        WriteTwin("Widget.axaml.cs", code);
    }

    public void WriteOriginal(string relativePath, string content) =>
        Write(OriginalRoot, relativePath, content);

    public void WriteTwin(string relativePath, string content) =>
        Write(TwinRoot, relativePath, content);

    public void WriteEnglish(params string[] keys)
        => WriteEnglishCategory("Widget", keys);

    public void WriteEnglishCategory(string category, params string[] keys)
    {
        string units = string.Join(
            Environment.NewLine,
            keys.Select(key => $"<trans-unit id=\"{key}\"><source>{key}</source></trans-unit>"));
        File.WriteAllText(
            Path.Combine(_root, "English.xlf"),
            $"<xliff><file original=\"{category}\"><body>{units}</body></file></xliff>");
    }

    public void WriteFrameworkAdaptations(string content) =>
        File.WriteAllText(FrameworkAdaptationsFile, content);

    public InventoryReport Run(bool useFrameworkAdaptations = false) =>
        InventoryRunner.Run(new InventoryOptions
        {
            OriginalRoot = OriginalRoot,
            TwinRoot = TwinRoot,
            TypeName = "Sample.Widget",
            TranslationsFile = Path.Combine(_root, "English.xlf"),
            OutputFile = OutputFile,
            FrameworkAdaptationsFile = useFrameworkAdaptations ? FrameworkAdaptationsFile : null
        });

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private static void Write(string root, string relativePath, string content)
    {
        string path = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }
}
