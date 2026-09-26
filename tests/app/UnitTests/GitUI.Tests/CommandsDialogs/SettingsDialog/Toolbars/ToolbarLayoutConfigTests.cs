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

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    [TestCase(65_537)]
    [TestCase(int.MaxValue)]
    public void SetCustomToolbarMetadata_should_reject_an_out_of_range_index(int index)
    {
        ToolbarLayoutConfig config = new();

        ((Action)(() => config.SetCustomToolbarMetadata("Custom 01", row: 0, orderInRow: 0, visible: true, iconSize: 16, index: index)))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void SetCustomToolbarMetadata_should_take_the_next_free_index_when_given_none()
    {
        // Counting the toolbars instead would hand out an index another one already holds as soon
        // as the indices are no longer an unbroken run - which one add/remove cycle is enough to do.
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("Custom 01", row: 0, orderInRow: 0, visible: true, iconSize: 16, index: 3);
        config.SetCustomToolbarMetadata("Custom 02", row: 0, orderInRow: 1, visible: true, iconSize: 16, index: 7);
        config.RemoveCustomToolbarMetadata("Custom 01");

        config.SetCustomToolbarMetadata("Custom 03", row: 0, orderInRow: 2, visible: true, iconSize: 16);

        config.CustomToolbars.Should().ContainSingle(c => c.Name == "Custom 03" && c.Index == 8);
        config.CustomToolbars.Select(c => c.Index).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void SetCustomToolbarMetadata_should_start_custom_indices_after_the_built_in_toolbars()
    {
        ToolbarLayoutConfig config = new();

        config.SetCustomToolbarMetadata("Custom 01", row: 0, orderInRow: 0, visible: true, iconSize: 16);

        config.CustomToolbars[0].Index.Should().Be(3);
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
