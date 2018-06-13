using System;
using System.Windows.Forms;
using FluentAssertions;
using GitUI;
using NUnit.Framework;

namespace GitExtUtilsTests
{
    [SetCulture("en-US")]
    [TestFixture]
    public class TableLayoutPanelExtensionsTests
    {
        [Test]
        public void AdjustWidthToSize_should_throw_if_table_null()
        {
            ((Action)(() => ((TableLayoutPanel)null).AdjustWidthToSize(0, 0))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AdjustWidthToSize_should_throw_if_table_has_no_columns()
        {
            ((Action)(() => new TableLayoutPanel().AdjustWidthToSize(0, 0))).Should().Throw<ArgumentException>()
                .WithMessage("The table must have at least one column");
        }

        [Test]
        public void AdjustWidthToSize_should_throw_if_index_outside_table_columns_count()
        {
            var table = new TableLayoutPanel
            {
                ColumnCount = 3
            };
            ((Action)(() => table.AdjustWidthToSize(-1, 0))).Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("Column index must be within [0, 2] range\nParameter name: columnIndex\nActual value was -1.");
            ((Action)(() => table.AdjustWidthToSize(3, 0))).Should().Throw<ArgumentOutOfRangeException>()
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
            ((Action)(() => table.AdjustWidthToSize(0, Array.Empty<float>()))).Should().Throw<ArgumentException>()
                .WithMessage("At least one width is required\nParameter name: widths");
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

            table.AdjustWidthToSize(0, 3f, 6f, 1f, 10f, 2f);

            table.ColumnStyles[0].SizeType.Should().Be(SizeType.Absolute);
            table.ColumnStyles[0].Width.Should().Be(10f);
        }
    }
}