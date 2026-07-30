using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Proves the temporary anchored-comment parity inventory.
[TestFixture]
[Category("P0_8A")]
public sealed class CommentParityTests
{
    [Test]
    public void Run_should_inventory_xml_inline_todo_and_issue_comments()
    {
        const string code = """
            namespace Sample;
            public partial class Widget
            {
                /// <summary>Explains the operation.</summary>
                private void Work()
                {
                    // TODO: retain https://example.invalid/issues/42
                    /* NOTE: inline explanation. */
                    // HACK: preserve the workaround rationale.
                }
            }
            """;
        using InventoryFixture fixture = new();
        fixture.WriteMatching(code);

        SourceInventory inventory = fixture.Run().Original;

        inventory.Comments.Should().HaveCount(4);
        inventory.Comments.Select(comment => comment.Kind).Should().Equal(
            "xmlDoc",
            "singleLine",
            "multiLine",
            "singleLine");
        inventory.Comments.Should().OnlyContain(comment => comment.Anchor == "method:Work()");
        inventory.Comments.Should().Contain(comment =>
            comment.Text.Contains("https://example.invalid/issues/42", StringComparison.Ordinal));
        inventory.Comments.Should().Contain(comment =>
            comment.Text.StartsWith("HACK:", StringComparison.Ordinal));
    }

    [Test]
    public void Run_should_localize_a_deleted_comment_to_its_anchor_member()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public partial class Widget
            {
                private void Work()
                {
                    // Keep this explanation.
                }
            }
            """);
        fixture.WriteTwin("Widget.axaml.cs", """
            namespace Sample;
            public partial class Widget
            {
                private void Work()
                {
                }
            }
            """);

        InventoryReport report = fixture.Run();

        FunctionalFinding finding = report.Findings.Should()
            .ContainSingle(item => item.Code == "comment.missing").Subject;
        finding.Path.Should().Be("comment/Widget.cs/method:Work()/body/0");
        finding.Message.Should().Contain("Widget.cs:6");
        finding.OriginalValue.Should().Be("Keep this explanation.");
    }

    [Test]
    public void Run_should_not_cascade_findings_after_one_comment_is_deleted()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public partial class Widget
            {
                private void Work()
                {
                    // First explanation.
                    // Second explanation.
                }
            }
            """);
        fixture.WriteTwin("Widget.axaml.cs", """
            namespace Sample;
            public partial class Widget
            {
                private void Work()
                {
                    // Second explanation.
                }
            }
            """);

        InventoryReport report = fixture.Run();

        report.Findings.Where(item => item.Category == "comments")
            .Should().ContainSingle()
            .Which.Code.Should().Be("comment.missing");
    }

    [Test]
    public void Run_should_distinguish_changed_and_framework_adapted_comments()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public partial class Widget
            {
                private void Work()
                {
                    // Save before closing.
                    // The WinForms Form owns the dialog.
                }
            }
            """);
        fixture.WriteTwin("Widget.axaml.cs", """
            namespace Sample;
            public partial class Widget
            {
                private void Work()
                {
                    // Save after closing.
                    // The Avalonia Window owns the dialog.
                }
            }
            """);

        InventoryReport report = fixture.Run();

        FunctionalFinding changed = report.Findings.Should()
            .ContainSingle(item => item.Code == "comment.changed").Subject;
        changed.OriginalValue.Should().Be("Save before closing.");
        changed.TwinValue.Should().Be("Save after closing.");
        CommentAdaptation adapted = report.AdaptedComments.Should().ContainSingle().Subject;
        adapted.OriginalText.Should().Be("The WinForms Form owns the dialog.");
        adapted.TwinText.Should().Be("The Avalonia Window owns the dialog.");
        report.Summary.AdaptedCommentCount.Should().Be(1);
    }

    [Test]
    public void Run_should_report_a_comment_that_drifted_to_another_member()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", """
            namespace Sample;
            public partial class Widget
            {
                private void First()
                {
                    // Describes the first operation.
                }

                private void Second()
                {
                }
            }
            """);
        fixture.WriteTwin("Widget.axaml.cs", """
            namespace Sample;
            public partial class Widget
            {
                private void First()
                {
                }

                private void Second()
                {
                    // Describes the first operation.
                }
            }
            """);

        FunctionalFinding finding = fixture.Run().Findings.Should()
            .ContainSingle(item => item.Code == "comment.drifted").Subject;

        finding.Path.Should().Contain("method:First()");
        finding.Message.Should().Contain("method:Second()");
    }

    [Test]
    public void Run_should_report_a_comment_that_drifted_to_another_partial()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.First.cs", """
            namespace Sample;
            public partial class Widget
            {
                // Keep this with the first partial.
            }
            """);
        fixture.WriteOriginal("Widget.Second.cs", """
            namespace Sample;
            public partial class Widget
            {
            }
            """);
        fixture.WriteTwin("Widget.First.cs", """
            namespace Sample;
            public partial class Widget
            {
            }
            """);
        fixture.WriteTwin("Widget.Second.cs", """
            namespace Sample;
            public partial class Widget
            {
                // Keep this with the first partial.
            }
            """);

        FunctionalFinding finding = fixture.Run().Findings.Should()
            .ContainSingle(item => item.Code == "comment.drifted").Subject;

        finding.Path.Should().Contain("Widget.First.cs");
        finding.Message.Should().Contain("Widget.Second.cs");
    }
}
