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

    [TestCase(-1, 0)]
    [TestCase(int.MinValue, 0)]
    [TestCase(int.MaxValue, 65_536)]
    public void TryNormalize_should_clamp_an_out_of_range_index(int index, int expected)
    {
        // The settings page turns the digits of a user-supplied name into an index, so a large
        // value is reachable; it only sorts the toolbars, so clamping is enough.
        ToolbarLayoutConfig config = ValidConfig();
        config.CustomToolbars[0].Index = index;

        ToolbarLayoutValidator.TryNormalize(config).Should().BeTrue();
        config.CustomToolbars[0].Index.Should().Be(expected);
    }

    [Test]
    public void TryNormalize_should_reject_duplicate_toolbar_names()
    {
        ToolbarLayoutConfig config = ValidConfig();
        config.ToolbarsVisibility.Add(new ToolbarBuiltInMetadata { Name = "CUSTOM 01" });

        ToolbarLayoutValidator.TryNormalize(config).Should().BeFalse();
    }

    [Test]
    public void TryNormalize_should_accept_duplicate_custom_indices()
    {
        // The settings page derives the index from the name for "Custom NN" but from the combo
        // box position otherwise, so the two schemes legitimately collide. The index only orders
        // the toolbars on load, so a tie is harmless and must not discard the whole layout.
        ToolbarLayoutConfig config = ValidConfig();
        config.CustomToolbars.Add(new ToolbarCustomMetadata { Name = "Custom 02", Index = 3 });
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
