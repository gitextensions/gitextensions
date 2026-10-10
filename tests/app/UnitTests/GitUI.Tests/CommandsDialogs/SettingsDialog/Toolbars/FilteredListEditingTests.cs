using GitUI.CommandsDialogs.SettingsDialog.Toolbars;

namespace GitUITests.CommandsDialogs.SettingsDialog.Toolbars;

// The settings page lets the user reorder a toolbar's items while a search filter hides some of
// them. The move must land in the whole list, and the hidden items must not notice.
public class FilteredListEditingTests
{
    private sealed class Row(string name)
    {
        public string Name { get; } = name;

        public override string ToString() => Name;
    }

    private static List<Row> Rows(params string[] names) => names.Select(name => new Row(name)).ToList();

    private static List<Row> Visible(List<Row> items, params string[] names)
        => items.Where(row => names.Contains(row.Name)).ToList();

    private static string[] Names(List<Row> items) => items.Select(row => row.Name).ToArray();

    [Test]
    public void Move_should_swap_neighbours_when_nothing_is_filtered()
    {
        List<Row> items = Rows("a", "b", "c");

        FilteredListEditing.Move(items, [.. items], fromIndex: 2, toIndex: 1);

        Names(items).Should().Equal("a", "c", "b");
    }

    [Test]
    public void Move_up_should_land_just_before_the_visible_neighbour()
    {
        List<Row> items = Rows("a", "hidden1", "b", "hidden2", "c");
        List<Row> visible = Visible(items, "a", "b", "c");

        FilteredListEditing.Move(items, visible, fromIndex: 2, toIndex: 1);

        Names(items).Should().Equal("a", "hidden1", "c", "b", "hidden2");
    }

    [Test]
    public void Move_down_should_land_just_after_the_visible_neighbour()
    {
        List<Row> items = Rows("a", "hidden1", "b", "hidden2", "c");
        List<Row> visible = Visible(items, "a", "b", "c");

        FilteredListEditing.Move(items, visible, fromIndex: 0, toIndex: 1);

        Names(items).Should().Equal("hidden1", "b", "a", "hidden2", "c");
    }

    [Test]
    public void Move_should_leave_the_view_reading_as_if_the_row_had_moved_within_it()
    {
        List<Row> items = Rows("h1", "a", "h2", "b", "h3", "c", "h4");
        string[] shown = ["a", "b", "c"];

        FilteredListEditing.Move(items, Visible(items, shown), fromIndex: 0, toIndex: 2);

        Names(Visible(items, shown)).Should().Equal("b", "c", "a");
    }

    [Test]
    public void Move_should_keep_the_hidden_items_and_their_order()
    {
        List<Row> items = Rows("h1", "a", "h2", "b", "h3");

        FilteredListEditing.Move(items, Visible(items, "a", "b"), fromIndex: 1, toIndex: 0);

        Names(items).Where(name => name.StartsWith('h')).Should().Equal("h1", "h2", "h3");
        items.Should().HaveCount(5);
    }

    [Test]
    public void Move_should_tell_identical_rows_apart_by_reference()
    {
        // Two separator rows look the same; moving the second must not move the first.
        Row first = new("separator");
        Row second = new("separator");
        Row other = new("x");
        List<Row> items = [first, other, second];

        FilteredListEditing.Move(items, [.. items], fromIndex: 2, toIndex: 1);

        items.Should().Equal(first, second, other);
    }

    [Test]
    public void Move_onto_itself_should_change_nothing()
    {
        List<Row> items = Rows("a", "b");

        FilteredListEditing.Move(items, [.. items], fromIndex: 1, toIndex: 1);

        Names(items).Should().Equal("a", "b");
    }

    [TestCase(-1, 0)]
    [TestCase(0, 2)]
    [TestCase(2, 0)]
    public void Move_should_reject_a_position_outside_the_view(int fromIndex, int toIndex)
    {
        List<Row> items = Rows("a", "b");

        Action move = () => FilteredListEditing.Move(items, [.. items], fromIndex, toIndex);

        move.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Move_should_reject_a_view_out_of_step_with_the_list()
    {
        List<Row> items = Rows("a", "b");

        Action move = () => FilteredListEditing.Move(items, [new Row("a"), items[1]], fromIndex: 0, toIndex: 1);

        move.Should().Throw<ArgumentException>();
    }
}
