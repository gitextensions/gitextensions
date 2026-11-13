using System.Text.Json;
using AwesomeAssertions;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Verifies nested declarations without changing the shipping application.
public sealed class NestedMemberInventoryTests
{
    [TestCase("class", "public")]
    [TestCase("abstract class", "protected")]
    [TestCase("readonly struct", "public")]
    [TestCase("record", "public")]
    [TestCase("readonly record struct", "public")]
    public void Run_should_compare_primary_constructors_with_explicit_constructors(string kind, string accessibility)
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", $"namespace Sample; class Widget {{ {kind} Inner {{ {accessibility} Inner(int value) {{ }} }} }}");
        fixture.WriteTwin("Widget.cs", $"namespace Sample; class Widget {{ {kind} Inner(int value) {{ }} }}");

        InventoryReport report = fixture.Run();

        report.Findings.Should().NotContain(item => item.Path.StartsWith("member/Inner/constructor:Inner", StringComparison.Ordinal));
        report.Twin.Members.Should().ContainSingle(member => member.ContainingType == "Inner" && member.Kind == "constructor")
            .Which.Accessibility.Should().Be(accessibility);
    }

    [Test]
    public void Run_should_include_positional_record_properties()
    {
        using InventoryFixture fixture = new();
        const string source = "namespace Sample; class Widget { record Inner(int Value, string Caption); }";
        fixture.WriteOriginal("Widget.cs", source);
        fixture.WriteTwin("Widget.cs", source);

        InventoryReport report = fixture.Run();

        report.Original.Members.Where(member => member.ContainingType == "Inner").Select(member => member.Name)
            .Should().Equal("Inner", "Value", "Caption");
        report.Findings.Should().BeEmpty();
    }

    [Test]
    public void Run_should_not_duplicate_an_explicit_positional_record_property()
    {
        using InventoryFixture fixture = new();
        const string source = "namespace Sample; class Widget { record Inner(int Value) { public int Value { get; init; } = Value; } }";
        fixture.WriteOriginal("Widget.cs", source);
        fixture.WriteTwin("Widget.cs", source);

        InventoryReport report = fixture.Run();

        report.Original.Members.Where(member => member.ContainingType == "Inner").Select(member => member.Name)
            .Should().Equal("Inner", "Value");
    }

    [Test]
    public void Run_should_not_invent_fields_or_properties_for_class_primary_parameters()
    {
        using InventoryFixture fixture = new();
        const string source = "namespace Sample; class Widget { class Inner(int value) { int Read() => value; } }";
        fixture.WriteOriginal("Widget.cs", source);
        fixture.WriteTwin("Widget.cs", source);

        InventoryReport report = fixture.Run();

        report.Original.Members.Where(member => member.ContainingType == "Inner").Select(member => member.Kind)
            .Should().Equal("constructor", "method");
    }

    [Test]
    public void Run_should_not_confuse_an_outer_type_with_a_nested_type_of_the_same_name()
    {
        using InventoryFixture fixture = new();
        fixture.WriteMatching("namespace Sample; class Widget { int value; } class Other { class Widget { int unrelated; } }");

        InventoryReport report = fixture.Run();

        report.Original.Members.Select(member => member.Name).Should().Equal("value");
    }

    [Test]
    public void Run_should_keep_root_pins_exact_when_a_nested_member_has_the_same_name()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { class Inner { void A() { } } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { void A() { } class Inner { void A() { } } }");
        fixture.WriteFrameworkAdaptations("""
            { "schemaVersion": 4, "deviations": [ {
              "typeName": "Sample.Widget", "category": "members", "code": "member.extra",
              "path": "member/method:A", "twinPart": "Widget.cs", "twinAccessibility": "private",
              "twinSignature": "void A()", "rationale": "Framework lifecycle adapter on the outer type."
            } ] }
            """);

        InventoryReport report = fixture.Run(useFrameworkAdaptations: true);

        report.AcceptedFrameworkDeviations.Should().ContainSingle(item => item.Path == "member/method:A");
        report.Findings.Should().NotContain(item => item.Code == "member.extra");
    }

    [Test]
    public void Run_should_report_a_missing_nested_member()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { class Inner { int Missing; } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { class Inner { } }");

        InventoryReport report = fixture.Run();

        report.Findings.Should().ContainSingle(item =>
            item.Code == "member.missing" && item.Path == "member/Inner/field:Missing");
    }

    [Test]
    public void Run_should_compare_order_inside_each_containing_type()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { class Inner { int first; int second; } int a; int b; }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { class Inner { int second; int first; } int a; int b; }");

        InventoryReport report = fixture.Run();

        report.Findings.Where(item => item.Code == "member.order").Select(item => item.Path)
            .Should().BeEquivalentTo("member/Inner/field:first", "member/Inner/field:second");
    }

    [Test]
    public void Run_should_not_merge_same_named_members_in_sibling_nested_types()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { class A { class Inner { int value; } } class B { class Inner { int value; } } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { class A { class Inner { } } class B { class Inner { int value; } } }");

        InventoryReport report = fixture.Run();

        report.Findings.Should().ContainSingle(item =>
            item.Code == "member.missing" && item.Path == "member/A.Inner/field:value");
        report.Findings.Should().NotContain(item => item.Path.StartsWith("member/B.Inner/", StringComparison.Ordinal));
    }

    [Test]
    public void Run_should_keep_nested_partial_member_locations()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; partial class Widget { partial class Inner { int first; } }");
        fixture.WriteOriginal("Widget.Part.cs", "namespace Sample; partial class Widget { partial class Inner { int second; } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; partial class Widget { partial class Inner { int first; int second; } }");
        fixture.WriteTwin("Widget.Part.cs", "namespace Sample; partial class Widget { partial class Inner { } }");

        InventoryReport report = fixture.Run();

        report.Findings.Should().Contain(item =>
            item.Code == "member.partial" && item.Path == "member/Inner/field:second/part");
    }

    [Test]
    public void Run_should_attach_a_missing_nested_method_comment_to_its_missing_member()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { class Inner {\n// keep this rationale\nvoid Missing() { } } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { class Inner { } }");

        InventoryReport report = fixture.Run();

        report.DependentFindings.Should().Contain(item =>
            item.RootPath == "member/Inner/method:Missing" && item.Finding.Code == "comment.missing");
    }

    [Test]
    public void Run_should_count_TestAccessor_types_separately()
    {
        using InventoryFixture fixture = new();
        fixture.WriteMatching("namespace Sample; class Widget { class Inner { int value; } readonly struct TestAccessor { public int Value => 1; } }");

        InventoryReport report = fixture.Run();
        using JsonDocument json = JsonDocument.Parse(InventoryJson.Serialize(report));
        JsonElement summary = json.RootElement.GetProperty("summary");

        summary.GetProperty("originalNestedTypeCount").GetInt32().Should().Be(1);
        summary.GetProperty("originalTestAccessorTypeCount").GetInt32().Should().Be(1);
        summary.GetProperty("twinNestedTypeCount").GetInt32().Should().Be(1);
        summary.GetProperty("twinTestAccessorTypeCount").GetInt32().Should().Be(1);
    }
}
