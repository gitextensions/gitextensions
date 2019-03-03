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
            _fileStatusList = null;
            _form = null;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _form.Dispose();
            _fileStatusList.Dispose();
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
    }
}