using GitUI.CommandsDialogs.SettingsDialog.Toolbars;

namespace GitUITests.CommandsDialogs.SettingsDialog.Toolbars;

public class ToolbarLayoutConfigTests
{
    [Test]
    public void SetCustomToolbarMetadata_should_write_both_lists()
    {
        ToolbarLayoutConfig config = new();

        config.SetCustomToolbarMetadata("Custom 01", row: 1, orderInRow: 0, visible: true, iconSize: 24, index: 3);

        config.ToolbarsVisibility.Should().ContainSingle(t => t.Name == "Custom 01" && t.Row == 1 && t.IconSize == 24);
        config.CustomToolbars.Should().ContainSingle(c => c.Name == "Custom 01" && c.Index == 3 && c.IconSize == 24);
    }

    [Test]
    public void RemoveCustomToolbarMetadata_should_also_drop_the_items_of_that_toolbar()
    {
        // Leaving them behind would grow the setting on every add/remove cycle and strand items
        // on a toolbar that no longer exists.
        ToolbarLayoutConfig config = new()
        {
            Items =
            {
                new ToolbarItemConfig { ItemName = "toolStripButtonPush", ToolbarName = "Custom 01" },
                new ToolbarItemConfig { ItemName = "toolStripButtonCommit", ToolbarName = "Standard" }
            }
        };
        config.SetCustomToolbarMetadata("Custom 01", row: 1, orderInRow: 0, visible: true, iconSize: 16);

        config.RemoveCustomToolbarMetadata("Custom 01");

        config.ToolbarsVisibility.Should().BeEmpty();
        config.CustomToolbars.Should().BeEmpty();
        config.Items.Should().ContainSingle(i => i.ToolbarName == "Standard");
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void SetCustomToolbarMetadata_should_reject_a_blank_name(string? name)
    {
        ToolbarLayoutConfig config = new();

        ((Action)(() => config.SetCustomToolbarMetadata(name!, row: 0, orderInRow: 0, visible: true, iconSize: 16)))
            .Should().Throw<ArgumentException>();
    }

    [TestCase(-1, 0)]
    [TestCase(33, 0)]
    [TestCase(0, -1)]
    [TestCase(0, 257)]
    public void SetCustomToolbarMetadata_should_reject_an_out_of_range_position(int row, int orderInRow)
    {
        ToolbarLayoutConfig config = new();

        ((Action)(() => config.SetCustomToolbarMetadata("Custom 01", row, orderInRow, visible: true, iconSize: 16)))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestCase(-1, 0)]
    [TestCase(int.MaxValue, 65_536)]
    public void SetCustomToolbarMetadata_should_clamp_the_index(int index, int expected)
    {
        // Callers derive the index from a user-supplied toolbar name, and it only sorts the
        // toolbars, so an extreme value must not bring the dialog down.
        ToolbarLayoutConfig config = new();

        config.SetCustomToolbarMetadata("Custom 01", row: 0, orderInRow: 0, visible: true, iconSize: 16, index: index);

        config.CustomToolbars[0].Index.Should().Be(expected);
    }

    [TestCase(0, 16)]
    [TestCase(30, 28)]
    [TestCase(9999, 72)]
    public void SetCustomToolbarMetadata_should_snap_the_icon_size(int iconSize, int expected)
    {
        // A caller can pass the DPI-scaled size of a live ToolStrip, which is a legitimate
        // in-between value, so it is normalized instead of rejected.
        ToolbarLayoutConfig config = new();

        config.SetCustomToolbarMetadata("Custom 01", row: 0, orderInRow: 0, visible: true, iconSize: iconSize);

        config.CustomToolbars[0].IconSize.Should().Be(expected);
    }
}
