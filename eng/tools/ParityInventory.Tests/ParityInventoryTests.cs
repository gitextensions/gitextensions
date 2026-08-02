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
        fixture.WriteTwin("Widget.axaml.cs", """
            namespace Sample;
            public partial class Widget { }
            """);

        InventoryReport report = fixture.Run();

        FunctionalFinding finding = report.Findings.Should()
            .ContainSingle(item => item.Code == "partial.missing").Subject;
        finding.OriginalValue.Should().Be("Widget.Toolbar.cs");
        finding.Path.Should().Be("part/Widget.Toolbar.cs");
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
    {
        string units = string.Join(
            Environment.NewLine,
            keys.Select(key => $"<trans-unit id=\"{key}\"><source>{key}</source></trans-unit>"));
        File.WriteAllText(
            Path.Combine(_root, "English.xlf"),
            $"<xliff><file original=\"Widget\"><body>{units}</body></file></xliff>");
    }

    public InventoryReport Run() =>
        InventoryRunner.Run(new InventoryOptions
        {
            OriginalRoot = OriginalRoot,
            TwinRoot = TwinRoot,
            TypeName = "Sample.Widget",
            TranslationsFile = Path.Combine(_root, "English.xlf"),
            OutputFile = OutputFile
        });

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private static void Write(string root, string relativePath, string content)
    {
        string path = Path.Combine(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }
}
