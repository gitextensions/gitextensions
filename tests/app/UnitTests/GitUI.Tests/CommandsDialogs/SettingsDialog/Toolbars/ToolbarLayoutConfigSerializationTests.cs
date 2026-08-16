using GitUI.CommandsDialogs.SettingsDialog.Toolbars;

namespace GitUITests.CommandsDialogs.SettingsDialog.Toolbars;

public class ToolbarLayoutConfigSerializationTests
{
    [Test]
    public void Serialize_then_deserialize_should_round_trip()
    {
        ToolbarLayoutConfig original = new()
        {
            Items = { new ToolbarItemConfig { ItemName = "toolStripButtonPush", ToolbarName = "Custom 02", Order = 1, ShowText = true } },
            CustomToolbars = { new ToolbarCustomMetadata { Name = "Custom 02", Index = 4, Row = 2, OrderInRow = 1, IconSize = 32 } },
            ToolbarsVisibility = { new ToolbarBuiltInMetadata { Name = "Custom 02", Row = 2, OrderInRow = 1, IconSize = 32 } }
        };

        string xml = ToolbarXmlSerializer.Serialize(original);
        ToolbarLayoutConfig? roundTripped = ToolbarXmlSerializer.Deserialize<ToolbarLayoutConfig>(xml);

        roundTripped.Should().NotBeNull();
        roundTripped!.Items.Should().ContainSingle(i => i.ItemName == "toolStripButtonPush" && i.ToolbarName == "Custom 02" && i.Order == 1 && i.ShowText);
        roundTripped.CustomToolbars.Should().ContainSingle(c => c.Name == "Custom 02" && c.Index == 4 && c.Row == 2 && c.OrderInRow == 1 && c.IconSize == 32);
        roundTripped.ToolbarsVisibility.Should().ContainSingle(t => t.Name == "Custom 02" && t.Row == 2 && t.OrderInRow == 1 && t.IconSize == 32);
    }

    [Test]
    public void Serialize_should_not_emit_a_byte_order_mark()
    {
        // A stray U+FEFF would be persisted into the settings file and break the next read.
        string xml = ToolbarXmlSerializer.Serialize(new ToolbarLayoutConfig());

        xml.Should().NotStartWith("﻿");
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("not xml at all")]
    [TestCase("<Unrelated />")]
    public void Deserialize_should_return_null_for_absent_or_unreadable_settings(string xml)
    {
        ToolbarXmlSerializer.Deserialize<ToolbarLayoutConfig>(xml).Should().BeNull();
    }
}
