using System;
using System.Windows.Forms;
using FluentAssertions;
using GitUI;
using NUnit.Framework;

namespace GitExtUtilsTests
{
    [TestFixture]
    public class ComboBoxExtensionsTests
    {
        [Test]
        public void AdjustWidthToFitContent_should_throw_if_combo_null()
        {
            ((Action)(() => ((ComboBox)null).AdjustWidthToFitContent())).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ResizeDropDownWidth_ComboBox_should_throw_if_combo_null()
        {
            ((Action)(() => ((ComboBox)null).ResizeDropDownWidth(1, 2))).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ResizeDropDownWidth_ToolStripComboBox_should_throw_if_combo_null()
        {
            ((Action)(() => ((ToolStripComboBox)null).ResizeDropDownWidth(1, 2))).Should().Throw<ArgumentNullException>();
        }
    }
}
