using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using FluentAssertions;
using GitUI;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [Apartment(ApartmentState.STA)]
    public class FileStatusListTests
    {
        // Created once for each test
        private Form _form;
        private FileStatusList _fileStatusList;

        [SetUp]
        public void SetUp()
        {
            _form = new Form();
            _fileStatusList = new FileStatusList();
            _fileStatusList.Parent = _form;
            _form.Show(); // must be visible to be able to change the focus
        }

        [TearDown]
        public void TearDown()
        {
            _fileStatusList?.Dispose();
            _fileStatusList = null;
            _form?.Dispose();
            _form = null;
        }

        [Test]
        public void Test_FilterWatermarkLabelVisibility_on_FilterVisibleChange(
            [Values(null, "", "x")] string filterText,
            [Values(true, false)] bool filterFocused,
            [Values(true, false)] bool filterVisible)
        {
            var accessor = _fileStatusList.GetTestAccessor();

            accessor.FilterComboBox.Text = filterText; // must be set first because it does not need to update the visibility

            _fileStatusList.FilterVisible = !filterVisible; // force a change
            _fileStatusList.FilterVisible = filterVisible;

            if (filterFocused)
            {
                accessor.FilterComboBox.Focus(); // must be done after FilterVisible = true
            }

            accessor.FilterWatermarkLabelVisible.Should().Be(filterVisible && !filterFocused && string.IsNullOrEmpty(filterText));
        }

        [Test]
        public void Test_FilterWatermarkLabelVisibility_on_Focus()
        {
            var accessor = _fileStatusList.GetTestAccessor();

            accessor.FilterComboBox.Text = "";
            _fileStatusList.FilterVisible = true;

            accessor.FilterWatermarkLabelVisible.Should().BeTrue();

            accessor.FilterComboBox.Focus();

            accessor.FilterWatermarkLabelVisible.Should().BeFalse();

            accessor.FileStatusListView.Focus();

            accessor.FilterWatermarkLabelVisible.Should().BeTrue();
        }

        [TestCase(null, false)]
        [TestCase("", false)]
        [TestCase("\\.cs", true)]
        public void Test_StoreFilter_valid(string regex, bool active)
        {
            var accessor = _fileStatusList.GetTestAccessor();

            var expectedColor = active ? accessor.ActiveInputColor : SystemColors.Window;
            string expectedRegex = string.IsNullOrEmpty(regex) ? null : regex;

            accessor.StoreFilter(regex);

            CheckStoreFilter(expectedColor, expectedRegex, accessor);
        }

        [Test]
        public void Test_StoreFilter_invalid()
        {
            const string validRegex = "\\.cs";
            const string invalidRegex = "(";

            var accessor = _fileStatusList.GetTestAccessor();

            // set a valid Filter that must not change
            accessor.StoreFilter(validRegex);

            ((Action)(() => accessor.StoreFilter(invalidRegex))).Should().Throw<ArgumentException>();

            CheckStoreFilter(accessor.InvalidInputColor, validRegex, accessor);
        }

        private static void CheckStoreFilter(Color expectedColor, string expectedRegex, FileStatusList.TestAccessor accessor)
        {
            accessor.FilterComboBox.BackColor.Should().Be(expectedColor);
            accessor.Filter?.ToString().Should().Be(expectedRegex);
        }
    }
}