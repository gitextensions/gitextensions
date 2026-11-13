using AwesomeAssertions;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Exercises overload measurement through actual source extraction and exact pins.
public sealed class OverloadInventoryTests
{
    [TestCase("void Render(int value) { }", "void Render(string value) { }")]
    [TestCase("public Widget(int value) { }", "public Widget(string value) { }")]
    [TestCase("void Render<T>(int value) { }", "void Render<T, U>(int value) { }")]
    [TestCase("int this[int value] => 0;", "int this[string value] => 0;")]
    [TestCase("public static Widget operator +(Widget left, int right) => left;", "public static Widget operator +(Widget left, string right) => left;")]
    [TestCase("public Widget() { }", "static Widget() { }")]
    [TestCase("void IFoo.Render(int value) { }", "void IBar.Render(int value) { }")]
    [TestCase("int IFoo.this[int value] => 0;", "int IBar.this[int value] => 0;")]
    [TestCase("public static Widget operator +(Widget left, Widget right) => left;", "public static Widget operator checked +(Widget left, Widget right) => left;")]
    [TestCase("public static explicit operator int(Widget value) => 0;", "public static explicit operator checked int(Widget value) => 0;")]
    public void Run_should_measure_each_missing_overload(string retained, string missing)
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", $"namespace Sample; class Widget {{ {retained} {missing} }}");
        fixture.WriteTwin("Widget.cs", $"namespace Sample; class Widget {{ {retained} }}");

        InventoryReport report = fixture.Run();

        report.Findings.Should().ContainSingle(item => item.Code == "member.missing");
        report.Findings.Should().NotContain(item => item.Code == "member.extra");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Run_should_match_both_partial_method_declarations_without_collapsing_them(bool samePart)
    {
        using InventoryFixture fixture = new();
        string declaration = "namespace Sample; partial class Widget { partial void Render(int value); }";
        string implementation = "namespace Sample; partial class Widget { partial void Render(int value) { } }";
        fixture.WriteOriginal("Widget.cs", samePart ? "namespace Sample; partial class Widget { partial void Render(int value); partial void Render(int value) { } }" : declaration);
        fixture.WriteTwin("Widget.cs", samePart ? "namespace Sample; partial class Widget { partial void Render(int value); partial void Render(int value) { } }" : declaration);
        if (!samePart)
        {
            fixture.WriteOriginal("Widget.More.cs", implementation);
            fixture.WriteTwin("Widget.More.cs", implementation);
        }

        fixture.Run().Findings.Should().BeEmpty();
    }

    [Test]
    public void Run_should_measure_each_extra_overload()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { void Render(int value) { } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { void Render(int value) { } void Render(string value) { } }");

        fixture.Run().Findings.Should().ContainSingle(item => item.Code == "member.extra");
    }

    [Test]
    public void Run_should_compare_overloaded_signatures_and_accessibility()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { public int Render(int value) => 0; void Render(string value) { } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { private long Render(int value) => 0; void Render(string value) { } }");

        InventoryReport report = fixture.Run();

        report.Findings.Should().ContainSingle(item => item.Code == "member.signature");
        report.Findings.Should().ContainSingle(item => item.Code == "member.accessibility");
        report.Findings.Should().NotContain(item => item.Code == "member.missing" || item.Code == "member.extra");
    }

    [TestCase("void Render(int value) { }", "void Render(string value) { }")]
    [TestCase("public Widget(int value) { }", "public Widget(string value) { }")]
    public void Run_should_compare_overload_order(string first, string second)
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", $"namespace Sample; class Widget {{ {first} {second} }}");
        fixture.WriteTwin("Widget.cs", $"namespace Sample; class Widget {{ {second} {first} }}");

        InventoryReport report = fixture.Run();

        report.Findings.Where(item => item.Code == "member.order").Should().HaveCount(2);
        report.Findings.Should().NotContain(item => item.Code == "member.missing" || item.Code == "member.extra");
    }

    [Test]
    public void Run_should_keep_overload_partial_boundaries()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; partial class Widget { void Render(int value) { } }");
        fixture.WriteOriginal("Widget.More.cs", "namespace Sample; partial class Widget { void Render(string value) { } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; partial class Widget { void Render(int value) { } void Render(string value) { } }");
        fixture.WriteTwin("Widget.More.cs", "namespace Sample; partial class Widget { }");

        fixture.Run().Findings.Should().ContainSingle(item => item.Code == "member.partial");
    }

    [Test]
    public void Run_should_pair_framework_signatures_without_hiding_the_difference()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { void Render(Control control) { } void Render(string value) { } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { void Render(AvaloniaControl control) { } void Render(string value) { } }");

        InventoryReport report = fixture.Run();

        report.Findings.Should().ContainSingle(item => item.Code == "member.signature");
        report.Findings.Should().NotContain(item => item.Code == "member.missing" || item.Code == "member.extra");
    }

    [Test]
    public void Run_should_not_pair_unrelated_ambiguous_overloads_by_position()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { void Render(int x) { } void Render(string x) { } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { void Render(bool x) { } void Render(long x) { } }");

        InventoryReport report = fixture.Run();

        report.Findings.Count(item => item.Code == "member.missing").Should().Be(2);
        report.Findings.Count(item => item.Code == "member.extra").Should().Be(2);
        report.Findings.Select(item => item.Path).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void Run_should_separate_product_and_test_only_overloads()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { void Render(int x) { } void Render(string x) { } struct TestAccessor { void Read(int x) { } void Read(string x) { } } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { void Render(int x) { } struct TestAccessor { void Read(int x) { } } }");

        InventoryReport report = fixture.Run();

        report.Summary.ProductFindingCount.Should().Be(1);
        report.Summary.TestOnlyFindingCount.Should().Be(1);
        report.Summary.FindingCount.Should().Be(2);
    }

    [Test]
    public void Run_should_attach_comment_dependencies_only_to_the_missing_overload()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { void Render(int x) { }\n// Required string route.\nvoid Render(string x) { } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { void Render(int x) { } }");

        InventoryReport report = fixture.Run();

        FunctionalFinding missing = report.Findings.Single(item => item.Code == "member.missing");
        report.DependentFindings.Should().ContainSingle().Which.RootPath.Should().Be(missing.Path);
        report.Findings.Should().NotContain(item => item.Code == "comment.missing");
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Run_should_validate_overload_pins_exactly(bool drift)
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { void Render(int value) { } void Render(string value) { } }");
        fixture.WriteTwin("Widget.cs", $"namespace Sample; class Widget {{ void Render({(drift ? "bool" : "long")} value) {{ }} void Render(string value) {{ }} }}");
        fixture.WriteFrameworkAdaptations("""
            { "schemaVersion": 4, "deviations": [ {
              "typeName": "Sample.Widget", "category": "members", "code": "member.signature",
              "path": "member/method:Render(int value)/signature",
              "originalPart": "Widget.cs", "originalAccessibility": "private", "originalSignature": "void Render(int value)",
              "twinPart": "Widget.cs", "twinAccessibility": "private", "twinSignature": "void Render(long value)",
              "rationale": "Exact framework signature substitute."
            } ] }
            """);

        if (drift)
        {
            Action run = () => fixture.Run(useFrameworkAdaptations: true);
            run.Should().Throw<InvalidDataException>().WithMessage("*drifted*");
        }
        else
        {
            InventoryReport report = fixture.Run(useFrameworkAdaptations: true);
            report.Findings.Should().BeEmpty();
            report.AcceptedFrameworkDeviations.Should().ContainSingle();
        }
    }
}
