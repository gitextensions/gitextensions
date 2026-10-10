using System.Drawing;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class TableLayoutSizingTests
{
    [Test]
    [TestCase(350)]
    [TestCase(500)]
    [TestCase(800)]
    public void Repository_auto_column_should_preserve_the_ListView_preferred_width_and_receive_surplus(int width)
    {
        using TableLayoutPanel table = new() { Size = new Size(width, 200), ColumnCount = 1, RowCount = 2 };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        using TextBox input = new() { Size = new Size(439, 20), Dock = DockStyle.Fill, Margin = new Padding(3) };
        using GitUI.UserControls.NativeListView list = new()
        {
            Size = new Size(445, 168), Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 3), BorderStyle = BorderStyle.None
        };
        table.Controls.Add(input, 0, 0);
        table.Controls.Add(list, 0, 1);
        table.CreateControl();
        table.PerformLayout();
        int columnWidth = Math.Max(445, width);
        table.GetColumnWidths()[0].Should().Be(columnWidth);
        list.Width.Should().Be(columnWidth);
        input.Width.Should().Be(columnWidth - 6);
    }
}
