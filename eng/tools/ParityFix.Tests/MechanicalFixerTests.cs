using AwesomeAssertions;
using GitExtensions.ParityInventory;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace GitExtensions.ParityFix.Tests;

// parity-scaffolding: Regression coverage for comment anchoring and behavior-preserving source edits.
[TestFixture]
public sealed class MechanicalFixerTests
{
    [TestCase("int", "int")]
    [TestCase("Control", "AvaloniaControl")]
    public void Fix_should_order_each_overload_using_the_inventory_match(string originalType, string twinType)
    {
        using Fixture fixture = new($"void A({originalType} value) {{ }}\n void A(string value) {{ }}",
            $"void A(string value) {{ }}\n void A({twinType} value) {{ }}");
        FileFix result = fixture.Fix();
        result.ProofPassed.Should().BeTrue();
        result.OrderChanged.Should().BeTrue();
        result.Text.IndexOf($"A({twinType}", StringComparison.Ordinal)
            .Should().BeLessThan(result.Text.IndexOf("A(string", StringComparison.Ordinal));
    }

    [Test]
    public void Fix_should_order_generic_overloads_with_the_same_parameters()
    {
        using Fixture fixture = new("void A<T>(int value) { }\n void A<T, U>(int value) { }",
            "void A<T, U>(int value) { }\n void A<T>(int value) { }");
        FileFix result = fixture.Fix();
        result.ProofPassed.Should().BeTrue();
        result.OrderChanged.Should().BeTrue();
        result.Text.IndexOf("A<T>(", StringComparison.Ordinal)
            .Should().BeLessThan(result.Text.IndexOf("A<T, U>(", StringComparison.Ordinal));
    }

    [Test]
    public void Fix_should_use_primary_constructor_and_record_parameter_order()
    {
        using Fixture fixture = new("record Inner(int Value, string Caption);",
            "record Inner { public string Caption { get; init; } public Inner(int Value, string Caption) { } public int Value { get; init; } }");

        FileFix result = fixture.Fix(forceOrder: true);

        result.ProofPassed.Should().BeTrue();
        result.OrderChanged.Should().BeTrue();
        result.Text.IndexOf("public Inner(", StringComparison.Ordinal)
            .Should().BeLessThan(result.Text.IndexOf("public int Value", StringComparison.Ordinal));
        result.Text.IndexOf("public int Value", StringComparison.Ordinal)
            .Should().BeLessThan(result.Text.IndexOf("public string Caption", StringComparison.Ordinal));
    }

    [Test]
    public void Fix_should_reorder_nested_members_without_changing_outer_order()
    {
        using Fixture fixture = new("class Inner { void A() { } void B() { } }\n int outer;",
            "class Inner { void B() { } void A() { } }\n int outer;");

        FileFix result = fixture.Fix();

        result.ProofPassed.Should().BeTrue();
        result.OrderChanged.Should().BeTrue();
        result.Text.IndexOf("void A()", StringComparison.Ordinal)
            .Should().BeLessThan(result.Text.IndexOf("void B()", StringComparison.Ordinal));
        result.Text.IndexOf("class Inner", StringComparison.Ordinal)
            .Should().BeLessThan(result.Text.IndexOf("int outer", StringComparison.Ordinal));
    }

    [Test]
    public void Fix_should_require_review_of_nested_initializers()
    {
        using Fixture fixture = new("class Inner { int first = 1; int second = 2; }",
            "class Inner { int second = 2; int first = 1; }");

        FileFix blocked = fixture.Fix();
        FileFix reviewed = fixture.Fix(allowInitializers: true);

        blocked.OrderChanged.Should().BeFalse();
        blocked.InitializedFields.Should().Contain("Sample.Widget.Inner.first");
        reviewed.OrderChanged.Should().BeTrue();
        reviewed.ProofPassed.Should().BeTrue();
    }

    [Test]
    public void MemberMultisetEquivalent_should_reject_a_changed_nested_method_body()
    {
        const string original = "namespace Sample; class Widget { class Inner { int A() => 1; } }";
        const string changed = "namespace Sample; class Widget { class Inner { int A() => 2; } }";

        MechanicalFixer.MemberMultisetEquivalent(CSharpSyntaxTree.ParseText(original).GetRoot(),
            CSharpSyntaxTree.ParseText(changed).GetRoot(), "Sample.Widget").Should().BeFalse();
    }

    [TestCase("// Retain the reason.")]
    [TestCase("/* Retain the reason. */")]
    [TestCase("/// <summary>Retain the reason.</summary>")]
    public void Fix_should_restore_verbatim_leading_comments(string comment)
    {
        using Fixture fixture = new($"{comment}\n    void A() {{ }}", "void A() { }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(1);
        result.Text.Should().Contain(comment);
        SyntaxFactory.AreEquivalent(CSharpSyntaxTree.ParseText(fixture.Twin).GetRoot(), CSharpSyntaxTree.ParseText(result.Text).GetRoot()).Should().BeTrue();
    }

    [TestCase("// Retain the reason.")]
    [TestCase("/* Retain the reason. */")]
    [TestCase("/// <summary>Retain the reason.</summary>")]
    public void Fix_should_restore_leading_comments_at_a_unique_anchor_with_a_changed_body(string comment)
    {
        using Fixture fixture = new($"{comment}\n    void A() {{ Windows(); }}", "void A() { Portable(); }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(1);
        result.Text.Should().Contain(comment).And.Contain("Portable();");
    }

    [Test]
    public void Fix_should_match_a_unique_leading_member_after_framework_parameter_changes()
    {
        using Fixture fixture = new("// Keep handler reasoning.\n    void A(EventArgs e) { Windows(); }", "void A(RoutedEventArgs e) { Portable(); }");
        fixture.Fix().CommentsRestored.Should().Be(1);
    }

    [Test]
    public void Fix_should_not_guess_between_changed_overload_anchors()
    {
        using Fixture fixture = new("// Keep handler reasoning.\n    void A(EventArgs e) { Windows(); }", "void A(RoutedEventArgs e) { Portable(); }\n    void A(int e) { }");
        fixture.Fix().CommentsRestored.Should().Be(0);
    }

    [Test]
    public void Fix_should_anchor_on_a_nested_next_statement_when_the_outer_statement_changes()
    {
        using Fixture fixture = new("void A()\n    {\n        if (flag)\n        {\n            Windows();\n            // Keep increment rationale.\n            count++;\n        }\n    }", "void A()\n    {\n        if (flag)\n        {\n            Portable();\n            count++;\n        }\n    }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(1);
        result.Text.Should().Contain("// Keep increment rationale.\r\n            count++;");
    }

    [Test]
    public void Fix_should_anchor_after_the_previous_unique_statement_when_the_next_statement_changes()
    {
        using Fixture fixture = new("void A()\n    {\n        Done();\n        // Completion before platform follow-up.\n        Windows();\n    }", "void A()\n    {\n        Done();\n        Portable();\n    }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(1);
        result.Text.Should().Contain("Done();\r\n        // Completion before platform follow-up.\r\n        Portable();");
    }

    [Test]
    public void Fix_should_not_cross_the_comments_control_flow_context_to_find_a_remote_anchor()
    {
        using Fixture fixture = new("void A()\n    {\n        Shared();\n        if (flag)\n        {\n            Windows();\n            // Missing operation rationale.\n            Missing();\n        }\n    }", "void A()\n    {\n        Shared();\n        if (flag)\n        {\n            Portable();\n        }\n    }");
        fixture.Fix().CommentsRestored.Should().Be(0);
    }

    [Test]
    public void Fix_should_find_the_nearest_equivalent_unique_statement_in_either_direction()
    {
        using Fixture fixture = new("void A()\n    {\n        Shared();\n        Windows();\n        // Shared state remains available.\n        Missing();\n    }", "void A()\n    {\n        Shared();\n        Portable();\n    }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(1);
        result.Text.Should().Contain("Shared();\r\n        // Shared state remains available.");
    }

    [Test]
    public void Fix_should_not_cross_local_function_contexts()
    {
        using Fixture fixture = new("void A()\n    {\n        void First()\n        {\n            // First operation rationale.\n            Shared();\n        }\n    }", "void A()\n    {\n        void Second()\n        {\n            Shared();\n        }\n    }");
        fixture.Fix().CommentsRestored.Should().Be(0);
    }

    [Test]
    public void Fix_should_restore_body_comments_at_the_same_token_boundary()
    {
        using Fixture fixture = new("void A()\n    {\n        // Before the increment.\n        count++;\n    }", "void A()\n    {\n        count++;\n    }");
        FileFix result = fixture.Fix();
        result.Text.Should().Contain("// Before the increment.\r\n        count++;");
        result.CommentsRestored.Should().Be(1);
    }

    [Test]
    public void Fix_should_preserve_the_blank_line_before_a_restored_comment()
    {
        using Fixture fixture = new("void A()\n    {\n        Done();\n\n        // Reason for the constant.\n        const int value = 1;\n    }", "void A()\n    {\n        Done();\n        const int value = 1;\n    }");
        FileFix result = fixture.Fix();
        result.Text.Should().Contain("Done();\r\n\r\n        // Reason for the constant.");
        result.CommentsRestored.Should().Be(1);
    }

    [Test]
    public void Fix_should_restore_body_comments_at_a_unique_unchanged_statement_in_a_ported_method()
    {
        using Fixture fixture = new("void A()\n    {\n        Windows();\n        // Keep the increment rationale.\n        count++;\n    }", "void A()\n    {\n        Portable();\n        count++;\n    }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(1);
        result.Text.Should().Contain("// Keep the increment rationale.\r\n        count++;");
    }

    [TestCase("count++;\n        count++;")]
    [TestCase("count--;")]
    public void Fix_should_leave_ambiguous_or_changed_statement_anchors_untouched(string statements)
    {
        using Fixture fixture = new("void A()\n    {\n        Windows();\n        // Reason for increment.\n        count++;\n    }", "void A()\n    {\n        Portable();\n        " + statements + "\n    }");
        fixture.Fix().Text.Should().Be(fixture.Twin);
    }

    [Test]
    public void Fix_should_leave_inline_comments_unplaced_when_no_safe_line_boundary_exists()
    {
        using Fixture fixture = new("void A()\n    {\n        count++; // Keep reason.\n    }", "void A()\n    {\n        count++;\n    }");
        fixture.Fix().Text.Should().Be(fixture.Twin);
    }

    [TestCase("void A()\n    {\n        Done();\n    }")]
    [TestCase("void A()\n    {\n        if (resized)\n        {\n            Done();\n        }\n    }")]
    public void Fix_should_not_move_comments_from_a_different_control_flow_context(string twin)
    {
        using Fixture fixture = new("void A()\n    {\n        if (rateLimited)\n        {\n            // Ignore API rate limiting.\n            Done();\n        }\n    }", twin);
        fixture.Fix().Text.Should().Be(fixture.Twin);
    }

    [Test]
    public void Fix_should_preserve_changed_comments_when_the_containing_type_deviates()
    {
        using Fixture fixture = new("// TreeNode reasoning.\n    bool Selected { get; set; }\n    void Windows() { }", "// TreeViewItem reasoning.\n    bool Selected { get; set; }\n    void Portable() { }");
        fixture.Fix().Text.Should().Be(fixture.Twin);
    }

    [TestCase("Avalonia.Controls.TreeViewItem.IsExpanded")]
    [TestCase("TreeViewItem.IsExpanded")]
    public void Fix_should_preserve_adapted_xml_references_in_identical_members(string reference)
    {
        using Fixture fixture = new("/// <summary>See <see cref=\"TreeNode.IsExpanded\"/>.</summary>\n    bool Selected { get; set; }", $"/// <summary>See <see cref=\"{reference}\"/>.</summary>\n    bool Selected {{ get; set; }}");
        fixture.Fix().Text.Should().Be(fixture.Twin);
    }

    [Test]
    public void Fix_should_leave_a_file_untouched_when_removing_drifted_trivia_would_remove_code()
    {
        using Fixture fixture = new("/* A reason. */\n    void A() { }\n    void B() { }", "void A() { }\n    /* A reason. */ void B() { }");
        FileFix result = fixture.Fix();
        result.Text.Should().Be(fixture.Twin);
        result.ProofPassed.Should().BeFalse();
    }

    [Test]
    public void Fix_should_restore_changed_xml_documentation_without_leaving_an_extra_delimiter()
    {
        using Fixture fixture = new("/// <summary>Original.</summary>\n    void A() { }", "/// <summary>Changed.</summary>\n    void A() { }");
        FileFix result = fixture.Fix();
        result.Text.Should().Be(fixture.Original);
        result.CommentsRestored.Should().Be(1);
    }

    [Test]
    public void Fix_should_restore_changed_comments_only_in_unchanged_members()
    {
        using Fixture fixture = new("// Original reason.\n    void A() { }", "// Altered reason.\n    void A() { }");
        FileFix result = fixture.Fix();
        result.Text.Should().Contain("// Original reason.").And.NotContain("// Altered reason.");
        result.CommentsRestored.Should().Be(1);
    }

    [Test]
    public void Fix_should_leave_deviation_comments_untouched()
    {
        using Fixture fixture = new("// Original reason.\n    void A() { Windows(); }", "// Platform substitute reason.\n    void A() { Portable(); }");
        FileFix result = fixture.Fix();
        result.Text.Should().Be(fixture.Twin);
        result.Unresolved.Should().HaveCount(1);
    }

    [TestCase("// Reason for A.")]
    [TestCase("/* Reason for A. */")]
    [TestCase("/// <summary>Reason for A.</summary>")]
    public void Fix_should_move_drifted_comments_and_remove_the_old_copy(string comment)
    {
        using Fixture fixture = new(comment + "\n    void A() { }\n    void B() { }", "void A() { }\n    " + comment + "\n    void B() { }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(1);
        result.Text.Should().Contain(comment + "\r\n    void A()");
        result.Text.Split(comment).Should().HaveCount(2);
    }

    [Test]
    public void Fix_should_keep_multiple_missing_comments_in_source_order()
    {
        using Fixture fixture = new("// First.\n    // Second.\n    void A() { }", "void A() { }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(2);
        result.Text.Should().Contain("// First.\r\n    // Second.");
    }

    [TestCase("// First.\n    ")]
    [TestCase("// Second.\n    ")]
    public void Fix_should_place_missing_comments_in_order_beside_retained_comments(string retained)
    {
        using Fixture fixture = new("// First.\n    // Second.\n    void A() { }", retained + "void A() { }");
        FileFix result = fixture.Fix();
        result.Text.Should().Contain("// First.\r\n    // Second.");
        result.CommentsRestored.Should().Be(1);
    }

    [Test]
    public void Fix_should_leave_an_unknown_framework_comment_group_for_hand_review()
    {
        using Fixture fixture = new("// First.\n    // Second.\n    void A() { }", "// Avalonia deviation rationale.\n    void A() { }");
        fixture.Fix().Text.Should().Be(fixture.Twin);
    }

    [Test]
    public void Fix_should_use_source_line_order_instead_of_lexical_finding_paths()
    {
        string[] comments = Enumerable.Range(0, 12).Select(index => $"// Rationale {index}.").ToArray();
        using Fixture fixture = new(string.Join("\n    ", comments) + "\n    void A() { }", "void A() { }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(12);
        result.Text.Should().Contain(string.Join("\r\n    ", comments));
    }

    [Test]
    public void Fix_should_restore_multiline_documentation_indentation()
    {
        using Fixture fixture = new("/// <summary>\n    ///  Keep this rationale.\n    /// </summary>\n    void A() { }", "void A() { }");
        FileFix result = fixture.Fix();
        result.CommentsRestored.Should().Be(1);
        result.Text.Should().Contain("/// <summary>\r\n    ///  Keep this rationale.\r\n    /// </summary>");
    }

    [TestCase("void B() { }\n    void A() { }")]
    [TestCase("void B() { }\n    void Extra() { }\n    void A() { }")]
    public void Fix_should_restore_order_and_keep_the_member_multiset(string twin)
    {
        using Fixture fixture = new("void A() { }\n    void B() { }", twin);
        FileFix result = fixture.Fix();
        result.OrderChanged.Should().BeTrue();
        result.Text.IndexOf("void A()", StringComparison.Ordinal).Should().BeLessThan(result.Text.IndexOf("void B()", StringComparison.Ordinal));
        MechanicalFixer.MemberMultisetEquivalent(CSharpSyntaxTree.ParseText(fixture.Twin).GetRoot(), CSharpSyntaxTree.ParseText(result.Text).GetRoot(), "Sample.Widget").Should().BeTrue();
    }

    [Test]
    public void Fix_should_require_review_for_initialized_fields()
    {
        using Fixture fixture = new("int a = 1;\n    int b = a;", "int b = a;\n    int a = 1;");
        FileFix result = fixture.Fix();
        result.OrderChanged.Should().BeFalse();
        result.Text.Should().Be(fixture.Twin);
        result.InitializedFields.Should().Equal("a", "b");
        fixture.Fix(allowInitializers: true).OrderChanged.Should().BeTrue();
    }

    [Test]
    public void Fix_should_not_reorder_ambiguous_overloads_by_name()
    {
        using Fixture fixture = new("void A(int n) { }\n    void A(string n) { }", "void A(bool n) { }\n    void A(long n) { }");
        FileFix result = fixture.Fix(forceOrder: true);
        result.OrderChanged.Should().BeFalse();
        result.Text.Should().Be(fixture.Twin);
    }

    [Test]
    public void Fix_should_not_cross_directive_barriers()
    {
        using Fixture fixture = new("void A() { }\n    void B() { }", "#region B\n    void B() { }\n    #endregion\n    void A() { }");
        fixture.Fix().Text.Should().Be(fixture.Twin);
    }

    [Test]
    public void Fix_should_move_methods_with_balanced_internal_warning_pragmas()
    {
        using Fixture fixture = new("void A() { }\n    void B() { }", "void B() {\n#pragma warning disable CS0168\nint unused;\n#pragma warning restore CS0168\n}\n    void A() { }");
        FileFix result = fixture.Fix();
        result.OrderChanged.Should().BeTrue();
        result.Text.IndexOf("void A()", StringComparison.Ordinal).Should().BeLessThan(result.Text.IndexOf("void B()", StringComparison.Ordinal));
        MechanicalFixer.MemberMultisetEquivalent(CSharpSyntaxTree.ParseText(fixture.Twin).GetRoot(), CSharpSyntaxTree.ParseText(result.Text).GetRoot(), "Sample.Widget").Should().BeTrue();
    }

    [TestCase("#pragma warning disable CS0168\n", "#pragma warning restore CS0168\n", "#pragma warning disable CS0168\n")]
    [TestCase("#pragma warning disable CS0168\n", "", "")]
    [TestCase("#pragma warning disable CS0168\n", "#pragma warning restore CS0219\n", "")]
    [TestCase("#pragma warning disable\n", "#pragma warning restore\n", "")]
    [TestCase("#if DEBUG\n", "#endif\n", "")]
    public void Fix_should_keep_warning_scope_and_other_directive_barriers(string start, string end, string ambient)
    {
        using Fixture fixture = new("void A() { }\n    void B() { }", ambient + "void B() {\n" + start + "int unused;\n" + end + "}\n    void A() { }");
        fixture.Fix().Text.Should().Be(fixture.Twin);
    }

    [Test]
    public void Fix_should_reject_syntax_errors_without_writing_a_candidate()
    {
        using Fixture fixture = new("void A() { }", "void A() { ");
        FileFix result = fixture.Fix();
        result.ProofPassed.Should().BeFalse();
        result.Text.Should().Be(fixture.Twin);
    }

    [Test]
    public void MemberMultisetEquivalent_should_reject_changed_bodies_and_changes_outside_the_type()
    {
        var before = CSharpSyntaxTree.ParseText("class A { void B() { } }").GetRoot();
        MechanicalFixer.MemberMultisetEquivalent(before, CSharpSyntaxTree.ParseText("class A { void B() { C(); } }").GetRoot(), "A").Should().BeFalse();
        MechanicalFixer.MemberMultisetEquivalent(before, CSharpSyntaxTree.ParseText("class A { void B() { } } class Extra { }").GetRoot(), "A").Should().BeFalse();
        MechanicalFixer.MemberMultisetEquivalent(before, CSharpSyntaxTree.ParseText("#nullable disable\nclass A { void B() { } }").GetRoot(), "A").Should().BeFalse();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Run_should_preview_apply_preserve_encoding_and_reject_stale_snapshots(bool bom)
    {
        using Fixture fixture = new("// Reason.\n    void A() { }", "void A() { }");
        File.WriteAllText(fixture.TwinPath, fixture.Twin, new System.Text.UTF8Encoding(bom));
        string inventory = Path.Combine(fixture.Root, "inventory");
        Directory.CreateDirectory(inventory);
        File.WriteAllText(Path.Combine(inventory, "Widget.json"), InventoryJson.Serialize(fixture.Report()));
        byte[] before = File.ReadAllBytes(fixture.TwinPath);
        FixRunner.Run(inventory, new HashSet<string> { "Sample.Widget" }, fixture.TwinRoot).Files.Should().ContainSingle();
        File.ReadAllBytes(fixture.TwinPath).Should().Equal(before);
        FixRunner.Run(inventory, new HashSet<string> { "Sample.Widget" }, fixture.TwinRoot, apply: true);
        File.ReadAllText(fixture.TwinPath).Should().Contain("// Reason.");
        File.ReadAllBytes(fixture.TwinPath).AsSpan().StartsWith(System.Text.Encoding.UTF8.Preamble).Should().Be(bom);
        Action stale = () => FixRunner.Run(inventory, new HashSet<string> { "Sample.Widget" }, fixture.TwinRoot, apply: true);
        stale.Should().Throw<InvalidDataException>().WithMessage("*Stale inventory*");
    }

    [Test]
    public void Run_should_reject_stale_original_facts_before_writing_any_twin()
    {
        using Fixture fixture = new("// Reason.\n    void A() { }", "void A() { }");
        string inventory = Path.Combine(fixture.Root, "inventory");
        Directory.CreateDirectory(inventory);
        File.WriteAllText(Path.Combine(inventory, "Widget.json"), InventoryJson.Serialize(fixture.Report()));
        File.WriteAllText(Path.Combine(fixture.Root, "original", "Widget.cs"), fixture.Original.Replace("Reason.", "New reason.", StringComparison.Ordinal));
        Action stale = () => FixRunner.Run(inventory, new HashSet<string> { "Sample.Widget" }, fixture.TwinRoot, apply: true);
        stale.Should().Throw<InvalidDataException>().WithMessage("*Stale inventory*");
        File.ReadAllText(fixture.TwinPath).Should().Be(fixture.Twin);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Run_should_require_all_mapped_twin_parts_inside_an_explicit_root(bool containsAll)
    {
        using Fixture fixture = new("void A() { }\n    void B() { }", "void B() { }\n    void A() { }");
        string originalPath = Path.Combine(fixture.Root, "original", "Widget.cs");
        string extraOriginal = Path.Combine(fixture.Root, "original", "Widget.Extra.cs");
        string extraDirectory = containsAll ? Path.Combine(fixture.TwinRoot, "nested") : Path.Combine(fixture.Root, "other");
        Directory.CreateDirectory(extraDirectory);
        string extraTwin = Path.Combine(extraDirectory, "Widget.Extra.cs");
        const string extraText = "namespace Sample; partial class Widget { void C() { } }";
        File.WriteAllText(extraOriginal, extraText);
        File.WriteAllText(extraTwin, extraText);
        SourceInventory source = SourceInventoryReader.ReadFiles(Path.GetDirectoryName(originalPath)!, "Sample.Widget", new HashSet<string>(), false, [originalPath, extraOriginal]);
        InventoryReport report = fixture.Report();
        report = report with
        {
            Twin = SourceInventoryReader.ReadFiles(fixture.Root, "Sample.Widget", new HashSet<string>(), true, [fixture.TwinPath, extraTwin]),
            Original = source with
            {
                Parts = source.Parts.Select(part => part with
                {
                    ExpectedTwinPath = part.Path == "Widget.cs" ? "twin/Widget.cs" : Path.GetRelativePath(fixture.Root, extraTwin).Replace('\\', '/')
                }).ToArray()
            }
        };
        string inventory = Path.Combine(fixture.Root, "inventory");
        Directory.CreateDirectory(inventory);
        File.WriteAllText(Path.Combine(inventory, "Widget.json"), InventoryJson.Serialize(report));
        byte[] original = File.ReadAllBytes(originalPath);
        Action apply = () => FixRunner.Run(inventory, new HashSet<string> { "Sample.Widget" }, fixture.TwinRoot, apply: true);
        if (containsAll)
        {
            apply.Should().NotThrow();
            string text = File.ReadAllText(fixture.TwinPath);
            text.IndexOf("void A()", StringComparison.Ordinal).Should().BeLessThan(text.IndexOf("void B()", StringComparison.Ordinal));
        }
        else
        {
            apply.Should().Throw<InvalidDataException>().WithMessage("*twin root*");
            File.ReadAllText(fixture.TwinPath).Should().Be(fixture.Twin);
        }

        File.ReadAllBytes(originalPath).Should().Equal(original);
        File.ReadAllText(extraOriginal).Should().Be(extraText);
        File.ReadAllText(extraTwin).Should().Be(extraText);
    }

    [Test]
    public void Run_should_reject_a_linked_original_inside_an_explicit_root()
    {
        using Fixture fixture = new("void A() { }\n    void B() { }", "void B() { }\n    void A() { }");
        string originalPath = Path.Combine(fixture.Root, "original", "Widget.cs");
        InventoryReport report = fixture.Report();
        report = report with
        {
            Twin = SourceInventoryReader.ReadFiles(fixture.Root, "Sample.Widget", new HashSet<string>(), true, [originalPath]),
            Original = report.Original with
            {
                Parts = report.Original.Parts.Select(part => part with { ExpectedTwinPath = "original/Widget.cs" }).ToArray()
            }
        };
        string inventory = Path.Combine(fixture.Root, "inventory");
        Directory.CreateDirectory(inventory);
        File.WriteAllText(Path.Combine(inventory, "Widget.json"), InventoryJson.Serialize(report));
        byte[] original = File.ReadAllBytes(originalPath);
        Action apply = () => FixRunner.Run(inventory, new HashSet<string> { "Sample.Widget" }, Path.GetDirectoryName(originalPath)!, apply: true);
        apply.Should().Throw<InvalidDataException>().WithMessage("*original file*");
        File.ReadAllBytes(originalPath).Should().Equal(original);
    }

    [TestCase("../original/Widget.cs", false)]
    [TestCase("Widget.cs", true)]
    [TestCase("child/Widget.cs", true)]
    public void IsWithin_should_reject_write_targets_outside_the_twin_root(string relative, bool expected)
    {
        string root = Path.GetFullPath("twin");
        FixRunner.IsWithin(root, Path.GetFullPath(Path.Combine(root, relative))).Should().Be(expected);
    }

    private sealed class Fixture : IDisposable
    {
        private readonly string _originalRoot;
        private readonly string _originalPath;

        public Fixture(string original, string twin)
        {
            Root = Path.Combine(Path.GetTempPath(), "ParityFix-" + Guid.NewGuid().ToString("N"));
            _originalRoot = Path.Combine(Root, "original");
            TwinRoot = Path.Combine(Root, "twin");
            Directory.CreateDirectory(_originalRoot);
            Directory.CreateDirectory(TwinRoot);
            _originalPath = Path.Combine(_originalRoot, "Widget.cs");
            TwinPath = Path.Combine(TwinRoot, "Widget.cs");
            Original = Wrap(original);
            Twin = Wrap(twin);
            File.WriteAllText(_originalPath, Original, new System.Text.UTF8Encoding(true));
            File.WriteAllText(TwinPath, Twin, new System.Text.UTF8Encoding(true));
        }

        public string Root { get; }

        public string TwinRoot { get; }

        public string TwinPath { get; }

        public string Original { get; }

        public string Twin { get; }

        public InventoryReport Report()
        {
            SourceInventory original = SourceInventoryReader.ReadFiles(_originalRoot, "Sample.Widget", new HashSet<string>(), false, [_originalPath]);
            SourceInventory twin = SourceInventoryReader.ReadFiles(TwinRoot, "Sample.Widget", new HashSet<string>(), true, [TwinPath]);
            InventoryComparison comparison = InventoryComparer.Compare(original, twin);
            return new InventoryReport
            {
                SchemaVersion = InventoryReport.CurrentSchemaVersion,
                TypeName = "Sample.Widget",
                Original = original,
                Twin = twin,
                Summary = new InventorySummary
                {
                    FindingCount = comparison.Findings.Count,
                    DependentFindingCount = 0,
                    TotalDifferenceCount = comparison.Findings.Count,
                    FindingsByCategory = new Dictionary<string, int>(),
                    AdaptedCommentCount = 0,
                    AcceptedFrameworkDeviationCount = 0
                },
                Findings = comparison.Findings,
                DependentFindings = [],
                AdaptedComments = [],
                AcceptedFrameworkDeviations = []
            };
        }

        public FileFix Fix(bool allowInitializers = false, bool forceOrder = false)
        {
            InventoryReport report = Report();
            List<FunctionalFinding> findings = report.Findings.Where(finding => finding.Code.StartsWith("comment.", StringComparison.Ordinal) || finding.Code == "member.order").ToList();
            if (forceOrder)
            {
                findings.Add(new FunctionalFinding { Category = "members", Code = "member.order", Path = "member/method:A", Message = "Order" });
            }

            return MechanicalFixer.Fix(TwinPath, Original, Twin, "Sample.Widget", findings, report.Original.Comments, report.Twin.Comments, allowInitializers);
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);

        private static string Wrap(string members) => ("namespace Sample;\nclass Widget\n{\n    " + members + "\n}\n").Replace("\n", "\r\n", StringComparison.Ordinal);
    }
}
