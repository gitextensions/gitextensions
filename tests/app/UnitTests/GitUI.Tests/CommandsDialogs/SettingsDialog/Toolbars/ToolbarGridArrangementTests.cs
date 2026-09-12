using GitUI.CommandsDialogs.SettingsDialog.Toolbars;

namespace GitUITests.CommandsDialogs.SettingsDialog.Toolbars;

// Dragging one toolbar moves the others: rows below an insertion point shift down, the row the
// toolbar left closes up, and empty rows disappear. These are the sums behind that.
public class ToolbarGridArrangementTests
{
    private static ToolbarLayoutItem Item(string name, int row, int orderInRow, bool builtIn = false)
        => new() { Name = name, DisplayName = name, Row = row, OrderInRow = orderInRow, IsBuiltIn = builtIn };

    // "Standard@0.0" for a toolbar named Standard sitting first on the top row.
    private static string[] Positions(List<ToolbarLayoutItem> items)
    {
        ToolbarGridArrangement.SortByPosition(items);
        return items.Select(i => $"{i.Name}@{i.Row}.{i.OrderInRow}").ToArray();
    }

    private static List<ToolbarLayoutItem> ThreeOnOneRow() =>
    [
        Item("Standard", 0, 0, builtIn: true),
        Item("Filters", 0, 1, builtIn: true),
        Item("Scripts", 0, 2, builtIn: true)
    ];

    [Test]
    public void SortByPosition_should_read_the_grid_top_row_first_then_left_to_right()
    {
        List<ToolbarLayoutItem> items = [Item("c", 1, 1), Item("a", 0, 1), Item("b", 1, 0), Item("d", 0, 0)];

        ToolbarGridArrangement.SortByPosition(items);

        items.Select(i => i.Name).Should().Equal("d", "a", "b", "c");
    }

    [Test]
    public void GetDefaultOrder_should_keep_the_built_in_toolbars_in_their_usual_order()
    {
        ToolbarGridArrangement.GetDefaultOrder("Standard").Should().BeLessThan(ToolbarGridArrangement.GetDefaultOrder("Filters"));
        ToolbarGridArrangement.GetDefaultOrder("Filters").Should().BeLessThan(ToolbarGridArrangement.GetDefaultOrder("Scripts"));
        ToolbarGridArrangement.GetDefaultOrder("Scripts").Should().BeLessThan(ToolbarGridArrangement.GetDefaultOrder("Custom 01"));
    }

    [Test]
    public void ResetToDefault_should_put_the_built_in_toolbars_back_on_the_first_row()
    {
        List<ToolbarLayoutItem> items =
        [
            Item("Scripts", 3, 0, builtIn: true),
            Item("Standard", 2, 1, builtIn: true),
            Item("Filters", 0, 0, builtIn: true)
        ];

        ToolbarGridArrangement.ResetToDefault(items);

        Positions(items).Should().Equal("Standard@0.0", "Filters@0.1", "Scripts@0.2");
        items.Should().OnlyContain(i => i.IsVisible);
    }

    [Test]
    public void ResetToDefault_should_give_each_custom_toolbar_a_row_of_its_own_and_hide_it()
    {
        // Leaving one on the first row makes the toolbars wider than the panel, which WinForms
        // resolves by wrapping them and leaving a gap at the start of the row.
        List<ToolbarLayoutItem> items = ThreeOnOneRow();
        items.Add(Item("Custom 01", 0, 3));
        items.Add(Item("Custom 02", 0, 4));

        ToolbarGridArrangement.ResetToDefault(items);

        Positions(items).Should().Equal("Standard@0.0", "Filters@0.1", "Scripts@0.2", "Custom 01@1.0", "Custom 02@2.0");
        items.Where(i => !i.IsBuiltIn).Should().OnlyContain(i => !i.IsVisible);
    }

    [Test]
    public void MoveToRow_should_insert_at_the_drop_position_within_the_same_row()
    {
        List<ToolbarLayoutItem> items = ThreeOnOneRow();

        ToolbarGridArrangement.MoveToRow(items, items.Single(i => i.Name == "Scripts"), targetRow: 0, dropIndex: 0);

        Positions(items).Should().Equal("Scripts@0.0", "Standard@0.1", "Filters@0.2");
    }

    [Test]
    public void MoveToRow_should_close_the_gap_left_in_the_row_it_came_from()
    {
        List<ToolbarLayoutItem> items = ThreeOnOneRow();
        items.Add(Item("Custom 01", 1, 0));

        ToolbarGridArrangement.MoveToRow(items, items.Single(i => i.Name == "Filters"), targetRow: 1, dropIndex: 1);

        Positions(items).Should().Equal("Standard@0.0", "Scripts@0.1", "Custom 01@1.0", "Filters@1.1");
    }

    [TestCase(-5)]
    [TestCase(99)]
    public void MoveToRow_should_take_a_drop_position_outside_the_row(int dropIndex)
    {
        // The drop position comes from where the cursor was, which can be past either end.
        List<ToolbarLayoutItem> items = ThreeOnOneRow();

        ToolbarGridArrangement.MoveToRow(items, items.Single(i => i.Name == "Standard"), targetRow: 0, dropIndex: dropIndex);

        items.Select(i => i.OrderInRow).Order().Should().Equal(0, 1, 2);
    }

    [Test]
    public void MoveToRow_should_drop_the_row_left_empty()
    {
        List<ToolbarLayoutItem> items = [Item("Standard", 0, 0, builtIn: true), Item("Custom 01", 1, 0)];

        ToolbarGridArrangement.MoveToRow(items, items.Single(i => i.Name == "Custom 01"), targetRow: 0, dropIndex: 1);

        Positions(items).Should().Equal("Standard@0.0", "Custom 01@0.1");
    }

    [Test]
    public void MoveToNewRow_should_push_the_rows_below_the_insertion_point_down()
    {
        List<ToolbarLayoutItem> items = ThreeOnOneRow();
        items.Add(Item("Custom 01", 1, 0));

        ToolbarGridArrangement.MoveToNewRow(items, items.Single(i => i.Name == "Filters"), newRow: 1);

        Positions(items).Should().Equal("Standard@0.0", "Scripts@0.1", "Filters@1.0", "Custom 01@2.0");
    }

    [Test]
    public void MoveToNewRow_should_insert_above_the_first_row()
    {
        List<ToolbarLayoutItem> items = ThreeOnOneRow();

        ToolbarGridArrangement.MoveToNewRow(items, items.Single(i => i.Name == "Scripts"), newRow: 0);

        Positions(items).Should().Equal("Scripts@0.0", "Standard@1.0", "Filters@1.1");
    }

    [Test]
    public void MoveToNewRow_should_close_the_row_it_came_from_even_once_that_row_has_shifted()
    {
        // The insertion pushes the source row down, so the row to close up is no longer the one the
        // toolbar started on - the case an off-by-one here would get wrong.
        List<ToolbarLayoutItem> items =
        [
            Item("Standard", 0, 0, builtIn: true),
            Item("Filters", 1, 0, builtIn: true),
            Item("Scripts", 1, 1, builtIn: true),
            Item("Custom 01", 1, 2)
        ];

        ToolbarGridArrangement.MoveToNewRow(items, items.Single(i => i.Name == "Scripts"), newRow: 1);

        Positions(items).Should().Equal("Standard@0.0", "Scripts@1.0", "Filters@2.0", "Custom 01@2.1");
    }

    [Test]
    public void MoveToNewRow_should_leave_a_lone_toolbar_where_it_is()
    {
        List<ToolbarLayoutItem> items = [Item("Standard", 0, 0, builtIn: true), Item("Custom 01", 1, 0)];

        ToolbarGridArrangement.MoveToNewRow(items, items.Single(i => i.Name == "Custom 01"), newRow: 1);

        Positions(items).Should().Equal("Standard@0.0", "Custom 01@1.0");
    }

    [Test]
    public void Compact_should_renumber_rows_so_none_is_empty()
    {
        List<ToolbarLayoutItem> items = [Item("Standard", 0, 0, builtIn: true), Item("Filters", 4, 0, builtIn: true), Item("Scripts", 9, 0, builtIn: true)];

        ToolbarGridArrangement.Compact(items);

        Positions(items).Should().Equal("Standard@0.0", "Filters@1.0", "Scripts@2.0");
    }

    [Test]
    public void Compact_should_renumber_positions_within_a_row_without_reordering_them()
    {
        List<ToolbarLayoutItem> items = [Item("Standard", 2, 7, builtIn: true), Item("Filters", 2, 3, builtIn: true), Item("Scripts", 2, 90, builtIn: true)];

        ToolbarGridArrangement.Compact(items);

        Positions(items).Should().Equal("Filters@0.0", "Standard@0.1", "Scripts@0.2");
    }

    [Test]
    public void Compact_should_accept_an_empty_grid()
    {
        List<ToolbarLayoutItem> items = [];

        ToolbarGridArrangement.Compact(items);

        items.Should().BeEmpty();
    }
}
