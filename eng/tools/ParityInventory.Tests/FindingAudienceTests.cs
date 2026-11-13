using System.Text.Json;
using AwesomeAssertions;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Keeps product and test-support findings visible as independent counts.
public sealed class FindingAudienceTests
{
    [Test]
    public void Run_should_partition_product_and_accessor_members_without_changing_the_total()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { int Product; struct TestAccessor { int Missing; } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { struct TestAccessor { int Extra; } }");

        AssertPartition(fixture.Run(), product: 1, testOnly: 2);
    }

    [Test]
    public void Run_should_count_a_missing_accessor_declaration_and_its_support_types_as_test_only()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { struct TestAccessor { class Helper { int Value; } } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { }");

        AssertPartition(fixture.Run(), product: 0, testOnly: 3);
    }

    [TestCase("TestAccessor")]
    [TestCase("TestAccessor?")]
    [TestCase("Widget.TestAccessor")]
    public void Run_should_count_the_accessor_entry_point_as_test_only(string returnType)
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { struct TestAccessor { } " + returnType + " GetTestAccessor() => default; }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { struct TestAccessor { } }");

        AssertPartition(fixture.Run(), product: 0, testOnly: 1);
    }

    [Test]
    public void Run_should_keep_similarly_named_product_members_in_the_product_count()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { int TestAccessor; int GetTestAccessor() => 0; class TestAccessorState { int Value; } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { class TestAccessorState { } }");

        AssertPartition(fixture.Run(), product: 3, testOnly: 0);
    }

    [Test]
    public void Run_should_partition_independent_product_and_accessor_order_findings()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { int first; int second; struct TestAccessor { int first; int second; } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { int second; int first; struct TestAccessor { int second; int first; } }");

        InventoryReport report = fixture.Run();

        report.Findings.Should().OnlyContain(finding => finding.Code == "member.order");
        AssertPartition(report, product: 2, testOnly: 2);
    }

    [Test]
    public void Run_should_partition_accessor_accessibility_and_signature_findings()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { struct TestAccessor { public int Value; } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { struct TestAccessor { private string Value; } }");

        AssertPartition(fixture.Run(), product: 0, testOnly: 2);
    }

    [Test]
    public void Run_should_count_accessor_comments_as_test_only()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; class Widget { struct TestAccessor {\n// retain this test rationale\nvoid Run() { } } }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; class Widget { struct TestAccessor { void Run() { } } }");

        AssertPartition(fixture.Run(), product: 0, testOnly: 1);
    }

    [Test]
    public void Run_should_not_classify_a_product_comment_by_its_file_name()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("TestAccessor.cs", "namespace Sample; class Widget {\n// retain this product rationale\nvoid Run() { } }");
        fixture.WriteTwin("TestAccessor.cs", "namespace Sample; class Widget { void Run() { } }");

        AssertPartition(fixture.Run(), product: 1, testOnly: 0);
    }

    [Test]
    public void Sweep_should_keep_the_partition_and_existing_total_at_every_level()
    {
        using BaselineFixture fixture = new();
        File.WriteAllText(Path.Combine(fixture.Options.OriginalRoot, "Widget.cs"),
            "namespace Sample; partial class Widget { int Product; struct TestAccessor { int Missing; } }");
        File.WriteAllText(Path.Combine(fixture.Options.TwinRoot, "Widget.axaml.cs"),
            "namespace Sample; partial class Widget { struct TestAccessor { int Extra; } }");

        InventorySweepResult result = InventorySweepRunner.Run(fixture.Options);
        using JsonDocument json = JsonDocument.Parse(InventoryJson.Serialize(result));
        JsonElement summary = json.RootElement.GetProperty("summary");
        int product = summary.GetProperty("productFindingCount").GetInt32();
        int testOnly = summary.GetProperty("testOnlyFindingCount").GetInt32();

        (product + testOnly).Should().Be(result.Summary.FindingCount);
        product.Should().BeGreaterThan(0);
        testOnly.Should().Be(2);
        foreach (JsonElement type in json.RootElement.GetProperty("types").EnumerateArray())
        {
            (type.GetProperty("productFindingCount").GetInt32()
                + type.GetProperty("testOnlyFindingCount").GetInt32())
                .Should().Be(type.GetProperty("findingCount").GetInt32());
        }
    }

    private static void AssertPartition(InventoryReport report, int product, int testOnly)
    {
        report.Summary.FindingCount.Should().Be(product + testOnly);
        using JsonDocument json = JsonDocument.Parse(InventoryJson.Serialize(report));
        JsonElement summary = json.RootElement.GetProperty("summary");

        summary.GetProperty("productFindingCount").GetInt32().Should().Be(product);
        summary.GetProperty("testOnlyFindingCount").GetInt32().Should().Be(testOnly);
        JsonElement[] findings = json.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        findings.Count(finding => finding.GetProperty("isTestOnly").GetBoolean()).Should().Be(testOnly);
        findings.Count(finding => !finding.GetProperty("isTestOnly").GetBoolean()).Should().Be(product);
    }
}
