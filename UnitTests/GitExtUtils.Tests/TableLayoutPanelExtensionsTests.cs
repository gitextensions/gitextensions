using System;
using System.Windows.Forms;
using FluentAssertions;
using GitUI;
using NUnit.Framework;

namespace GitExtUtilsTests
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    public class TableLayoutPanelExtensionsTests
    {
        [Test]
        public void AdjustWidthToSize_should_throw_if_table_null()
        {
            ((Action)(() => ((TableLayoutPanel)null).AdjustWidthToSize(0, Array.Empty<Control>()))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AdjustWidthToSize_should_throw_if_table_has_no_columns()
        {
            ((Action)(() => new TableLayoutPanel().AdjustWidthToSize(0, Array.Empty<Control>()))).Should().Throw<ArgumentException>()
                .WithMessage("The table must have at least one column");
        }

        [Test]
        public void AdjustWidthToSize_should_throw_if_index_outside_table_columns_count()
        {
            var table = new TableLayoutPanel
            {
                ColumnCount = 3
            };
            ((Action)(() => table.AdjustWidthToSize(-1, Array.Empty<Control>()))).Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Column index must be within [0, 2] range\nParameter name: columnIndex\nActual value was -1.");
            ((Action)(() => table.AdjustWidthToSize(3, Array.Empty<Control>()))).Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Column index must be within [0, 2] range\nParameter name: columnIndex\nActual value was 3.");
        }

        [Test]
        public void AdjustWidthToSize_should_throw_if_no_widths_given()
        {
            var table = new TableLayoutPanel
            {
                ColumnCount = 3
            };
            ((Action)(() => table.AdjustWidthToSize(0, null))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AdjustWidthToSize_should_throw_if_no_widths_given1()
        {
            var table = new TableLayoutPanel
            {
                ColumnCount = 3
            };
            ((Action)(() => table.AdjustWidthToSize(0, Array.Empty<Control>()))).Should().Throw<ArgumentException>()
                .WithMessage("At least one control is required\nParameter name: controls");
        }

        [Test]
        public void AdjustWidthToSize_should_set_width_to_largest_value()
        {
            var table = new TableLayoutPanel
            {
                ColumnCount = 3
            };
            table.ColumnStyles.Add(new ColumnStyle());
            table.ColumnStyles.Add(new ColumnStyle());
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.ColumnStyles[0].SizeType = SizeType.AutoSize;

            table.AdjustWidthToSize(0, new Label { Width = 3, Margin = new Padding(3) },
                new TextBox { Width = 6, Margin = new Padding(3) },
                new Label { Width = 9, Margin = new Padding(3) },
                new TextBox { Width = 10, Margin = new Padding(3) },
                new CheckBox { Width = 3, Margin = new Padding(3) });

            table.ColumnStyles[0].SizeType.Should().Be(SizeType.Absolute);
            table.ColumnStyles[0].Width.Should().Be(16f);
        }
    }
}