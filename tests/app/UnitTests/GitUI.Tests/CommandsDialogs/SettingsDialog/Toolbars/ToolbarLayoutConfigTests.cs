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

    [TestCase("Standard", 0)]
    [TestCase("Filters", 1)]
    [TestCase("Scripts", 2)]
    public void SetToolbarVisibility_should_remember_a_built_in_toolbar_the_layout_never_mentioned(string name, int defaultOrder)
    {
        // A default install has no entry at all; hiding a toolbar must still survive a restart,
        // without moving it from where it sits by default.
        ToolbarLayoutConfig config = new();

        config.SetToolbarVisibility(name, visible: false);

        config.ToolbarsVisibility.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Name = name, Visible = false, Row = 0, OrderInRow = defaultOrder, IconSize = 16 });
        config.CustomToolbars.Should().BeEmpty();
    }

    [Test]
    public void SetToolbarVisibility_should_keep_the_place_of_a_built_in_toolbar_already_laid_out()
    {
        ToolbarLayoutConfig config = new()
        {
            ToolbarsVisibility = { new ToolbarBuiltInMetadata { Name = "Filters", Row = 2, OrderInRow = 1, IconSize = 24 } }
        };

        config.SetToolbarVisibility("Filters", visible: false);

        config.ToolbarsVisibility.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Visible = false, Row = 2, OrderInRow = 1, IconSize = 24 });
    }

    [Test]
    public void SetToolbarVisibility_should_update_both_lists_of_a_custom_toolbar()
    {
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("Review", row: 1, orderInRow: 0, visible: true, iconSize: 16);

        config.SetToolbarVisibility("Review", visible: false);

        config.ToolbarsVisibility.Should().ContainSingle(t => t.Name == "Review" && !t.Visible);
        config.CustomToolbars.Should().ContainSingle(c => c.Name == "Review" && !c.Visible);
    }

    [Test]
    public void SetToolbarVisibility_should_not_invent_a_custom_toolbar()
    {
        // Startup recreates custom toolbars from CustomToolbars; an entry for a name that has none
        // would describe a toolbar that does not exist.
        ToolbarLayoutConfig config = new();

        config.SetToolbarVisibility("Unknown", visible: false);

        config.ToolbarsVisibility.Should().BeEmpty();
        config.CustomToolbars.Should().BeEmpty();
    }

    [Test]
    public void SetToolbarVisibility_should_show_a_hidden_toolbar_again()
    {
        ToolbarLayoutConfig config = new();
        config.SetToolbarVisibility("Scripts", visible: false);

        config.SetToolbarVisibility("Scripts", visible: true);

        config.ToolbarsVisibility.Should().ContainSingle(t => t.Name == "Scripts" && t.Visible);
    }

    [Test]
    public void RenameCustomToolbar_should_rename_the_toolbar_everywhere_the_layout_names_it()
    {
        ToolbarLayoutConfig config = new()
        {
            Items =
            {
                new ToolbarItemConfig { ItemName = "toolStripButtonPush", ToolbarName = "Custom 01", Order = 0 },
                new ToolbarItemConfig { ItemName = "toolStripButtonCommit", ToolbarName = "Standard", Order = 0 }
            }
        };
        config.SetCustomToolbarMetadata("Custom 01", row: 2, orderInRow: 1, visible: false, iconSize: 24, index: 3);

        config.RenameCustomToolbar("Custom 01", "Review");

        config.ToolbarsVisibility.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Name = "Review", Row = 2, OrderInRow = 1, Visible = false, IconSize = 24 });
        config.CustomToolbars.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Name = "Review", Index = 3, Row = 2, OrderInRow = 1, Visible = false });
        config.Items.Should().ContainSingle(i => i.ItemName == "toolStripButtonPush").Which.ToolbarName.Should().Be("Review");
        config.Items.Should().ContainSingle(i => i.ItemName == "toolStripButtonCommit").Which.ToolbarName.Should().Be("Standard");
    }

    [Test]
    public void RenameCustomToolbar_should_leave_the_other_toolbars_alone()
    {
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("Custom 01", row: 1, orderInRow: 0, visible: true, iconSize: 16);
        config.SetCustomToolbarMetadata("Custom 02", row: 2, orderInRow: 0, visible: true, iconSize: 16);

        config.RenameCustomToolbar("Custom 01", "Review");

        config.CustomToolbars.Select(c => c.Name).Should().Equal("Review", "Custom 02");
    }

    [Test]
    public void RenameCustomToolbar_should_accept_a_change_of_case_only()
    {
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("review", row: 1, orderInRow: 0, visible: true, iconSize: 16);

        config.RenameCustomToolbar("review", "Review");

        config.CustomToolbars.Should().ContainSingle(c => c.Name == "Review");
    }

    [Test]
    public void RenameCustomToolbar_should_refuse_a_name_another_toolbar_holds()
    {
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("Custom 01", row: 1, orderInRow: 0, visible: true, iconSize: 16);
        config.SetCustomToolbarMetadata("Custom 02", row: 2, orderInRow: 0, visible: true, iconSize: 16);

        ((Action)(() => config.RenameCustomToolbar("Custom 01", "Custom 02"))).Should().Throw<ArgumentException>();
        config.CustomToolbars.Select(c => c.Name).Should().Equal("Custom 01", "Custom 02");
    }

    [TestCase("Standard", "Review")]
    [TestCase("Custom 01", "Filters")]
    [TestCase("Custom 01", "scripts")]
    public void RenameCustomToolbar_should_refuse_to_touch_a_built_in_name(string oldName, string newName)
    {
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("Custom 01", row: 1, orderInRow: 0, visible: true, iconSize: 16);

        ((Action)(() => config.RenameCustomToolbar(oldName, newName))).Should().Throw<ArgumentException>();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void RenameCustomToolbar_should_reject_a_blank_name(string? name)
    {
        ToolbarLayoutConfig config = new();
        config.SetCustomToolbarMetadata("Custom 01", row: 1, orderInRow: 0, visible: true, iconSize: 16);

        ((Action)(() => config.RenameCustomToolbar("Custom 01", name!))).Should().Throw<ArgumentException>();
        ((Action)(() => config.RenameCustomToolbar(name!, "Review"))).Should().Throw<ArgumentException>();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void SetToolbarVisibility_should_reject_a_blank_name(string? name)
    {
        ToolbarLayoutConfig config = new();

        ((Action)(() => config.SetToolbarVisibility(name!, visible: true))).Should().Throw<ArgumentException>();
    }
}
