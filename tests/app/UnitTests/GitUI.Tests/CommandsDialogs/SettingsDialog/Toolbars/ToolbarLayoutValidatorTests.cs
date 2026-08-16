using GitUI.CommandsDialogs.SettingsDialog.Toolbars;

namespace GitUITests.CommandsDialogs.SettingsDialog.Toolbars;

public class ToolbarLayoutValidatorTests
{
    private static ToolbarLayoutConfig ValidConfig() => new()
    {
        Items = { new ToolbarItemConfig { ItemName = "toolStripButtonPush", ToolbarName = "Custom 01", Order = 0 } },
        CustomToolbars = { new ToolbarCustomMetadata { Name = "Custom 01", Index = 3, Row = 1, OrderInRow = 0, IconSize = 24 } },
        ToolbarsVisibility = { new ToolbarBuiltInMetadata { Name = "Custom 01", Row = 1, OrderInRow = 0, IconSize = 24 } }
    };

    [Test]
    public void TryNormalize_should_accept_a_valid_configuration()
    {
        ToolbarLayoutConfig config = ValidConfig();

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
        config.CustomToolbars[0].IconSize.Should().Be(24);
    }

    [Test]
    public void TryNormalize_should_reject_null()
    {
        ToolbarLayoutValidator.TryNormalize(null).Should().BeFalse();
    }

    [Test]
    public void TryNormalize_should_reject_a_null_collection()
    {
        // DataContractSerializer assigns members directly, so an absent or xsi:nil element
        // defeats the property initializer.
        ToolbarLayoutConfig config = ValidConfig();
        config.Items = null!;

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [Test]
    public void TryNormalize_should_reject_a_null_entry()
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.ToolbarsVisibility.Add(null!);

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [Test]
    public void Deserialize_then_TryNormalize_should_reject_a_nil_collection()
    {
        // Proves the case above is reachable from a settings file, not just constructible in code.
        string xml = ToolbarXmlSerializer.Serialize(ValidConfig())
            .Replace("<Items>", """<Items xmlns:i="http://www.w3.org/2001/XMLSchema-instance" i:nil="true">""");

        ToolbarLayoutConfig? config = ToolbarXmlSerializer.Deserialize<ToolbarLayoutConfig>(xml);

        // Assert the payload really deserialized into a null collection, otherwise this would
        // pass merely because the XML was rejected.
        config.Should().NotBeNull();
        config!.Items.Should().BeNull();

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("with\u0001control")]
    public void TryNormalize_should_reject_an_invalid_toolbar_name(string? name)
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.ToolbarsVisibility[0].Name = name!;

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [Test]
    public void TryNormalize_should_reject_an_overlong_toolbar_name()
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.ToolbarsVisibility[0].Name = new string('a', 129);

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [TestCase(-1)]
    [TestCase(33)]
    [TestCase(int.MaxValue)]
    public void TryNormalize_should_reject_an_out_of_range_row(int row)
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.ToolbarsVisibility[0].Row = row;

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    public void TryNormalize_should_reject_a_negative_order_in_row(int orderInRow)
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.CustomToolbars[0].OrderInRow = orderInRow;

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    [TestCase(65_537)]
    [TestCase(int.MaxValue)]
    public void TryNormalize_should_reject_an_out_of_range_index(int index)
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.CustomToolbars[0].Index = index;

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [Test]
    public void TryNormalize_should_reject_duplicate_toolbar_names()
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.ToolbarsVisibility.Add(new ToolbarBuiltInMetadata { Name = "CUSTOM 01" });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [Test]
    public void TryNormalize_should_reject_duplicate_custom_indices()
    {
        // The index decides the order the custom toolbars are rebuilt in, so a tie leaves it
        // undecided which of the two comes first.
        ToolbarLayoutConfig config = ValidConfig();
        config.CustomToolbars.Add(new ToolbarCustomMetadata { Name = "Custom 02", Index = 3 });
        config.ToolbarsVisibility.Add(new ToolbarBuiltInMetadata { Name = "Custom 02" });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [Test]
    public void TryNormalize_should_accept_distinct_custom_indices()
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.CustomToolbars.Add(new ToolbarCustomMetadata { Name = "Custom 02", Index = 4 });
        config.ToolbarsVisibility.Add(new ToolbarBuiltInMetadata { Name = "Custom 02" });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
    }

    [Test]
    public void TryNormalize_should_reject_a_custom_toolbar_shadowing_a_built_in_one()
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.CustomToolbars[0].Name = "Standard";
        config.ToolbarsVisibility[0].Name = "Standard";
        config.Items[0].ToolbarName = "Standard";

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [Test]
    public void TryNormalize_should_drop_an_item_pointing_at_an_unknown_toolbar()
    {
        // Such an item is unreachable rather than ambiguous, so it is removed instead of
        // invalidating the layout it belongs to.
        ToolbarLayoutConfig config = ValidConfig();
        config.Items.Add(new ToolbarItemConfig { ItemName = "toolStripButtonPull", ToolbarName = "Deleted toolbar" });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
        config.Items.Should().ContainSingle(i => i.ToolbarName == "Custom 01");
    }

    [Test]
    public void TryNormalize_should_accept_an_item_on_a_built_in_toolbar_that_has_no_metadata()
    {
        // Item layouts are persisted on their own, so a built-in toolbar can carry items long
        // before anything writes its visibility metadata. Rejecting that would wipe the layout.
        ToolbarLayoutConfig config = new()
        {
            Items = { new ToolbarItemConfig { ItemName = "toolStripButtonCommit", ToolbarName = "Standard" } }
        };

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
    }

    [Test]
    public void TryNormalize_should_reject_an_oversized_collection()
    {
        ToolbarLayoutConfig config = ValidConfig();
        for (int i = 0; i <= ToolbarLayoutValidator.MaxItems; i++)
        {
            config.Items.Add(new ToolbarItemConfig { ItemName = $"item{i}", ToolbarName = "Custom 01" });
        }

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [TestCase(null, TestName = "null item name")]
    [TestCase("", TestName = "empty item name")]
    [TestCase("   ", TestName = "blank item name")]
    [TestCase("_LABEL_abc", TestName = "label without an order suffix")]
    [TestCase("_LABEL_a b_0", TestName = "label whose text was not escaped")]
    [TestCase("_SEPARATOR_x", TestName = "separator without an index")]
    [TestCase("no name at all", TestName = "not a control name")]
    public void TryNormalize_should_drop_an_item_whose_name_could_not_have_been_written(string? itemName)
    {
        // Such an item names nothing the loader could resolve, so it is unreachable rather than
        // ambiguous: dropping it keeps the rest of the user's layout.
        ToolbarLayoutConfig config = ValidConfig();
        config.Items.Add(new ToolbarItemConfig { ItemName = itemName!, ToolbarName = "Custom 01" });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
        config.Items.Should().ContainSingle(i => i.ItemName == "toolStripButtonPush");
    }

    [Test]
    public void TryNormalize_should_drop_a_label_whose_text_hides_control_characters()
    {
        // Percent-escaping hides them from the name itself, so only the decoded text reveals them.
        ToolbarLayoutConfig config = ValidConfig();
        config.Items.Add(new ToolbarItemConfig
        {
            ItemName = $"{ToolbarItemNames.LabelPrefix}{Uri.EscapeDataString($"a{(char)1}b")}_0",
            ToolbarName = "Custom 01"
        });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
        config.Items.Should().ContainSingle(i => i.ItemName == "toolStripButtonPush");
    }

    [Test]
    public void TryNormalize_should_drop_a_label_whose_text_is_unbounded()
    {
        // A label this long would make its toolbar unusable, and the dialog cannot produce one.
        ToolbarLayoutConfig config = ValidConfig();
        config.Items.Add(new ToolbarItemConfig
        {
            ItemName = $"{ToolbarItemNames.LabelPrefix}{new string('a', 2000)}_0",
            ToolbarName = "Custom 01"
        });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
        config.Items.Should().ContainSingle(i => i.ItemName == "toolStripButtonPush");
    }

    [Test]
    public void TryNormalize_should_keep_an_unknown_but_well_formed_item_id()
    {
        // An action that no longer exists, or one contributed by a plugin that is not loaded right
        // now, is skipped by the loader and must not cost the user the item's toolbar.
        ToolbarLayoutConfig config = ValidConfig();
        config.Items.Add(new ToolbarItemConfig { ItemName = "someRemovedToolStripMenuItem", ToolbarName = "Custom 01" });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
        config.Items.Should().HaveCount(2);
    }

    [Test]
    public void TryNormalize_should_keep_an_item_named_with_a_leading_underscore()
    {
        // Several real menu items are named that way; they are not placeholders.
        ToolbarLayoutConfig config = ValidConfig();
        config.Items.Add(new ToolbarItemConfig { ItemName = "_viewPullRequestsToolStripMenuItem", ToolbarName = "Custom 01" });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
        config.Items.Should().HaveCount(2);
    }

    [TestCase(0, 16)]
    [TestCase(int.MinValue, 16)]
    [TestCase(30, 28)]
    [TestCase(1000, 72)]
    [TestCase(int.MaxValue, 72)]
    public void TryNormalize_should_snap_an_icon_size_into_the_supported_range(int iconSize, int expected)
    {
        // An icon size is cosmetic and a live ToolStrip reports a DPI-scaled one, so it is
        // normalized rather than used to invalidate the whole layout.
        ToolbarLayoutConfig config = ValidConfig();
        config.ToolbarsVisibility[0].IconSize = iconSize;

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
        config.ToolbarsVisibility[0].IconSize.Should().Be(expected);
    }
}
