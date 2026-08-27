using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityInventory.Tests;

// parity-scaffolding: Proves the temporary functional inventory schema and extractors.
[TestFixture]
[Category("P0_4")]
public sealed class ParityInventoryTests
{
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
    public void Run_should_record_an_exact_reviewed_framework_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteOriginal("Widget.cs", "namespace Sample; public sealed class Widget { }");
        fixture.WriteTwin("Widget.cs", "namespace Sample; public sealed class Widget { private int frameworkOnly; }");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 1,
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
    public void Run_should_reject_a_stale_reviewed_framework_adaptation()
    {
        using InventoryFixture fixture = new();
        fixture.WriteMatching("namespace Sample; public sealed class Widget { private int frameworkOnly; }");
        fixture.WriteFrameworkAdaptations("""
            {
              "schemaVersion": 1,
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
              "schemaVersion": 1,
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
              "schemaVersion": 1,
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
                private void InitializeComponent()
                {
                    menu.Items.AddRange(new[] { open });
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
                <MenuItem x:Name="open" Header="Open" />
              </ContextMenu>
            </UserControl>
            """);

        InventoryReport report = fixture.Run();

        report.Original.Menus.Should().ContainSingle(item =>
            item.Parent == "menu" && item.Name == "open");
        report.Twin.Menus.Should().ContainSingle(item =>
            item.Parent == "menu" && item.Name == "open");
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
