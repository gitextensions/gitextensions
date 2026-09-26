using GitUI.CommandsDialogs;

namespace GitUITests.CommandsDialogs;

// ToolbarItemConverter is shared by FormBrowse (startup) and the Toolbars settings page (reload
// after a change) precisely so the two produce identical buttons, which makes what it guarantees
// worth pinning down.
public class ToolbarItemConverterTests
{
    [Test]
    public void Convert_should_turn_a_leaf_menu_item_into_a_button()
    {
        using ToolStripMenuItem menuItem = new("&Commit") { Name = "commitToolStripMenuItem", ToolTipText = "Commit changes" };

        using ToolStripItem converted = ToolbarItemConverter.Convert(menuItem, ToolStripItemDisplayStyle.Image);

        converted.Should().BeOfType<ToolStripButton>();
        converted.Name.Should().Be("btn_commitToolStripMenuItem");
        converted.Text.Should().Be("&Commit");
        converted.ToolTipText.Should().Be("Commit changes");
        converted.DisplayStyle.Should().Be(ToolStripItemDisplayStyle.Image);
        converted.Tag.Should().BeSameAs(menuItem);
    }

    [Test]
    public void Convert_should_fall_back_to_the_menu_text_as_tooltip()
    {
        // The ampersand is a mnemonic marker, not part of the tooltip.
        using ToolStripMenuItem menuItem = new("&Commit") { Name = "commitToolStripMenuItem" };

        using ToolStripItem converted = ToolbarItemConverter.Convert(menuItem, ToolStripItemDisplayStyle.Image);

        converted.ToolTipText.Should().Be("Commit");
    }

    [Test]
    public void Convert_should_forward_a_click_to_the_menu_item()
    {
        using ToolStripMenuItem menuItem = new("Commit") { Name = "commitToolStripMenuItem" };
        int clicks = 0;
        menuItem.Click += (s, e) => clicks++;

        using ToolStripItem converted = ToolbarItemConverter.Convert(menuItem, ToolStripItemDisplayStyle.Image);

        // The button inherits the menu item's visibility, and a menu item that has never been
        // displayed reports itself unavailable - which makes PerformClick a no-op. On a real
        // toolbar the button is available, so make it so here rather than testing nothing.
        converted.Available = true;
        converted.PerformClick();

        clicks.Should().Be(1);
    }

    [Test]
    public void Convert_should_mirror_a_check_state_changed_elsewhere()
    {
        // A toggle can be flipped from the menu bar or a keyboard shortcut, and every button
        // showing that action has to follow.
        using ToolStripMenuItem menuItem = new("Show tags") { Name = "showTagsToolStripMenuItem", CheckOnClick = true };

        using ToolStripButton converted = (ToolStripButton)ToolbarItemConverter.Convert(menuItem, ToolStripItemDisplayStyle.Image);
        converted.Checked.Should().BeFalse();

        menuItem.Checked = true;

        converted.Checked.Should().BeTrue();
    }

    [Test]
    public void Convert_should_stop_mirroring_once_the_button_is_disposed()
    {
        // The button outlives nothing: it is thrown away and rebuilt on every toolbar reload, so a
        // subscription left behind on the long-lived menu item would accumulate.
        using ToolStripMenuItem menuItem = new("Show tags") { Name = "showTagsToolStripMenuItem" };
        ToolStripButton converted = (ToolStripButton)ToolbarItemConverter.Convert(menuItem, ToolStripItemDisplayStyle.Image);

        converted.Dispose();
        menuItem.Checked = true;

        converted.Checked.Should().BeFalse();
    }

    [Test]
    public void Convert_should_turn_a_menu_item_with_children_into_a_split_button()
    {
        using ToolStripMenuItem menuItem = new("Navigate") { Name = "navigateToolStripMenuItem" };
        menuItem.DropDownItems.Add(new ToolStripMenuItem("Go to commit") { Name = "gotoCommitToolStripMenuItem" });
        menuItem.DropDownItems.Add(new ToolStripSeparator());

        using ToolStripItem converted = ToolbarItemConverter.Convert(menuItem, ToolStripItemDisplayStyle.ImageAndText);

        converted.Should().BeOfType<ToolStripSplitButton>();
        ToolStripSplitButton splitButton = (ToolStripSplitButton)converted;
        splitButton.DropDownItems.Cast<ToolStripItem>().Select(i => i.Name).Should().Contain("gotoCommitToolStripMenuItem");
        splitButton.DropDownItems.OfType<ToolStripSeparator>().Should().ContainSingle();
    }

    [Test]
    public void Convert_should_forward_a_dropdown_item_click_to_its_source()
    {
        using ToolStripMenuItem menuItem = new("Navigate") { Name = "navigateToolStripMenuItem" };
        using ToolStripMenuItem child = new("Go to commit") { Name = "gotoCommitToolStripMenuItem" };
        menuItem.DropDownItems.Add(child);
        int clicks = 0;
        child.Click += (s, e) => clicks++;

        using ToolStripItem converted = ToolbarItemConverter.Convert(menuItem, ToolStripItemDisplayStyle.Image);
        ((ToolStripSplitButton)converted).DropDownItems
            .Cast<ToolStripItem>()
            .Single(i => i.Name == "gotoCommitToolStripMenuItem")
            .PerformClick();

        clicks.Should().Be(1);
    }

    [Test]
    public void Convert_should_store_the_button_under_its_own_name()
    {
        using ToolStripMenuItem menuItem = new("Commit") { Name = "commitToolStripMenuItem" };
        Dictionary<string, ToolStripItem> store = [];

        using ToolStripItem converted = ToolbarItemConverter.Convert(menuItem, ToolStripItemDisplayStyle.Image, store);

        store.Should().ContainKey("btn_commitToolStripMenuItem");
        store["btn_commitToolStripMenuItem"].Should().BeSameAs(converted);
    }

    [Test]
    public void CloneItem_should_copy_a_button_and_point_back_at_it()
    {
        using ToolStripButton original = new("Commit") { Name = "toolStripButtonCommit", ToolTipText = "Commit changes" };

        using ToolStripItem clone = ToolbarItemConverter.CloneItem(original);

        clone.Name.Should().Be("clone_toolStripButtonCommit");
        clone.ToolTipText.Should().Be("Commit changes");
        clone.Tag.Should().BeSameAs(original);
    }

    [Test]
    public void CloneItem_should_forward_a_click_to_the_original()
    {
        using ToolStripButton original = new("Commit") { Name = "toolStripButtonCommit" };
        int clicks = 0;
        original.Click += (s, e) => clicks++;

        using ToolStripItem clone = ToolbarItemConverter.CloneItem(original);
        clone.PerformClick();

        clicks.Should().Be(1);
    }

    [TestCase(true, ToolStripItemDisplayStyle.ImageAndText)]
    [TestCase(false, ToolStripItemDisplayStyle.Image)]
    public void CloneItem_should_honour_its_own_text_preference(bool wantsText, ToolStripItemDisplayStyle expected)
    {
        // Each toolbar decides on its own whether its copy of an action shows a label, so a clone
        // that does not want text stays icon-only whatever the original does.
        using ToolStripButton original = new("Commit")
        {
            Name = "toolStripButtonCommit",
            DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
        };

        using ToolStripItem clone = ToolbarItemConverter.CloneItem(original, wantsText);

        clone.DisplayStyle.Should().Be(expected);
    }

    [Test]
    public void CloneItem_should_follow_the_original_when_it_changes()
    {
        using ToolStripButton original = new("Commit") { Name = "toolStripButtonCommit" };
        using ToolStripItem clone = ToolbarItemConverter.CloneItem(original, wantsText: true);

        original.Text = "Commit (2)";

        clone.Text.Should().Be("Commit (2)");
    }

    [Test]
    public void CloneItem_should_mirror_a_check_state()
    {
        using ToolStripButton original = new("Show tags") { Name = "toolStripButtonShowTags" };
        using ToolStripButton clone = (ToolStripButton)ToolbarItemConverter.CloneItem(original);

        original.Checked = true;

        clone.Checked.Should().BeTrue();
    }

    [Test]
    public void CloneItem_should_stop_following_the_original_once_disposed()
    {
        using ToolStripButton original = new("Commit") { Name = "toolStripButtonCommit" };
        ToolStripItem clone = ToolbarItemConverter.CloneItem(original, wantsText: true);

        clone.Dispose();
        original.Text = "Commit (2)";

        clone.Text.Should().Be("Commit");
    }

    [Test]
    public void CloneItem_should_give_a_push_button_its_own_label_state()
    {
        // A plain clone would redirect its label state to the original, so both toolbars would
        // always show or hide the "Push" label together.
        using ToolStripPushButton original = new() { Name = "toolStripButtonPush" };

        using ToolStripItem clone = ToolbarItemConverter.CloneItem(original, wantsText: true);

        clone.Should().BeAssignableTo<IPushLabelItem>();
        clone.Should().NotBeOfType<ToolStripPushButton>();
        ((IPushLabelItem)clone).ShowLabel.Should().BeTrue();
        original.ShowLabel.Should().BeFalse();
    }

    [Test]
    public void CloneItem_should_copy_the_dropdown_of_a_split_button()
    {
        using ToolStripSplitButton original = new("Working directory") { Name = "toolStripSplitButtonWorkingDir" };

        using ToolStripItem clone = ToolbarItemConverter.CloneItem(original);

        clone.Should().BeOfType<ToolStripSplitButton>();
        clone.Name.Should().Be("clone_toolStripSplitButtonWorkingDir");
        clone.Tag.Should().BeSameAs(original);
    }
}
