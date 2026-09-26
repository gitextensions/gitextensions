using GitCommands;
using GitUI.CommandsDialogs.SettingsDialog.Toolbars;

namespace GitUITests.CommandsDialogs.SettingsDialog.Toolbars;

// ToolbarLayoutStore.Load is the single boundary between the settings file and the rest of the
// application: everything past it works with a layout whose invariants have been checked. These
// tests drive it through AppSettings, the way the application does.
public class ToolbarLayoutStoreTests
{
    private string _originalXml = string.Empty;

    [SetUp]
    public void Setup()
    {
        _originalXml = AppSettings.ToolbarLayoutXml;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.ToolbarLayoutXml = _originalXml;
    }

    [Test]
    public void Load_should_return_an_empty_configuration_when_the_setting_is_absent()
    {
        AppSettings.ToolbarLayoutXml = string.Empty;

        ToolbarLayoutConfig config = ToolbarLayoutStore.Load();

        config.Items.Should().BeEmpty();
        config.CustomToolbars.Should().BeEmpty();
        config.ToolbarsVisibility.Should().BeEmpty();
    }

    [TestCase("not xml at all", TestName = "unparseable")]
    [TestCase("<Unrelated />", TestName = "foreign root element")]
    public void Load_should_return_an_empty_configuration_when_the_setting_is_unreadable(string xml)
    {
        AppSettings.ToolbarLayoutXml = xml;

        ToolbarLayoutStore.Load().Items.Should().BeEmpty();
    }

    [Test]
    public void Load_should_discard_a_layout_that_breaks_an_invariant()
    {
        // A duplicate toolbar name makes the layout ambiguous, so the whole payload goes rather
        // than letting an arbitrary resolution reach the main window.
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("Custom 01", row: 1, orderInRow: 0, visible: true, iconSize: 24);
        config.ToolbarsVisibility.Add(new ToolbarBuiltInMetadata { Name = "CUSTOM 01" });
        AppSettings.ToolbarLayoutXml = ToolbarXmlSerializer.Serialize(config);

        ToolbarLayoutStore.Load().ToolbarsVisibility.Should().BeEmpty();
    }

    [Test]
    public void Load_should_normalize_what_it_can_rather_than_discard_it()
    {
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("Custom 01", row: 1, orderInRow: 0, visible: true, iconSize: 24);
        config.CustomToolbars[0].IconSize = 30;
        config.Items.Add(new ToolbarItemConfig { ItemName = "toolStripButtonPush", ToolbarName = "Custom 01" });
        config.Items.Add(new ToolbarItemConfig { ItemName = "_LABEL_broken", ToolbarName = "Custom 01" });
        AppSettings.ToolbarLayoutXml = ToolbarXmlSerializer.Serialize(config);

        ToolbarLayoutConfig loaded = ToolbarLayoutStore.Load();

        loaded.CustomToolbars.Should().ContainSingle(c => c.Name == "Custom 01" && c.IconSize == 28);
        loaded.Items.Should().ContainSingle(i => i.ItemName == "toolStripButtonPush");
    }

    [Test]
    public void Save_then_Load_should_round_trip_a_layout()
    {
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("Custom 01", row: 2, orderInRow: 1, visible: true, iconSize: 32, index: 4);
        config.Items.Add(new ToolbarItemConfig { ItemName = "toolStripButtonPush", ToolbarName = "Custom 01", Order = 0, ShowText = true });

        ToolbarLayoutStore.Save(config);
        ToolbarLayoutConfig loaded = ToolbarLayoutStore.Load();

        loaded.CustomToolbars.Should().ContainSingle(c => c.Name == "Custom 01" && c.Index == 4 && c.Row == 2 && c.OrderInRow == 1 && c.IconSize == 32);
        loaded.ToolbarsVisibility.Should().ContainSingle(t => t.Name == "Custom 01" && t.Visible);
        loaded.Items.Should().ContainSingle(i => i.ItemName == "toolStripButtonPush" && i.ShowText);
    }
}
