using System.Threading;
using FluentAssertions;
using GitCommands.Settings;
using GitUI.Editor;
using NUnit.Framework;

namespace GitUITests.Editor
{
    [Apartment(ApartmentState.STA)]
    public class FileViewerTests
    {
        private FileViewer _fileViewer;

        [SetUp]
        public void SetUp()
        {
            _fileViewer = new FileViewer();
        }

        [TearDown]
        public void TearDown()
        {
            _fileViewer.Dispose();
        }

        [Test]
        [TestCase(IgnoreWhitespaceKind.None, IgnoreWhitespaceKind.Eol, true, false, false)]
        [TestCase(IgnoreWhitespaceKind.None, IgnoreWhitespaceKind.Change, true, true, false)]
        [TestCase(IgnoreWhitespaceKind.None, IgnoreWhitespaceKind.AllSpace, true, true, true)]
        [TestCase(IgnoreWhitespaceKind.Eol, IgnoreWhitespaceKind.Change, true, true, false)]
        [TestCase(IgnoreWhitespaceKind.Eol, IgnoreWhitespaceKind.AllSpace, true, true, true)]
        [TestCase(IgnoreWhitespaceKind.Change, IgnoreWhitespaceKind.Eol, true, false, false)]
        [TestCase(IgnoreWhitespaceKind.Change, IgnoreWhitespaceKind.AllSpace, true, true, true)]
        [TestCase(IgnoreWhitespaceKind.AllSpace, IgnoreWhitespaceKind.Eol, true, false, false)]
        [TestCase(IgnoreWhitespaceKind.AllSpace, IgnoreWhitespaceKind.Change, true, true, false)]
        public void Should_correctly_setup_IgnoreWhitespaceMethod_with_one_click(IgnoreWhitespaceKind oldIgnoreWhitespace, IgnoreWhitespaceKind newIgnoreWhitespace, bool ignoreEol, bool ignoreChange, bool ignoreAllSpace)
        {
            Assert.AreNotEqual(oldIgnoreWhitespace, newIgnoreWhitespace);

            var accessor = _fileViewer.GetTestAccessor();

            accessor.IgnoreWhitespace = oldIgnoreWhitespace;

            switch (newIgnoreWhitespace)
            {
                case IgnoreWhitespaceKind.Eol:
                    accessor.IgnoreWhitespaceAtEolToolStripMenuItem_Click(null, null);
                    break;
                case IgnoreWhitespaceKind.Change:
                    accessor.IgnoreWhitespaceChangesToolStripMenuItemClick(null, null);
                    break;
                case IgnoreWhitespaceKind.AllSpace:
                    accessor.IgnoreAllWhitespaceChangesToolStripMenuItem_Click(null, null);
                    break;
            }

            accessor.IgnoreWhitespace.Should().Be(newIgnoreWhitespace);

            accessor.IgnoreWhitespaceAtEolButton.Checked.Should().Be(ignoreEol);
            accessor.IgnoreWhitespaceAtEolMenuItem.Checked.Should().Be(ignoreEol);

            accessor.IgnoreWhiteSpacesButton.Checked.Should().Be(ignoreChange);
            accessor.IgnoreWhiteSpacesMenuItem.Checked.Should().Be(ignoreChange);

            accessor.IgnoreAllWhitespacesButton.Checked.Should().Be(ignoreAllSpace);
            accessor.IgnoreAllWhitespacesMenuItem.Checked.Should().Be(ignoreAllSpace);
        }

        [Test]
        [TestCase(IgnoreWhitespaceKind.None, IgnoreWhitespaceKind.Eol)]
        [TestCase(IgnoreWhitespaceKind.None, IgnoreWhitespaceKind.Change)]
        [TestCase(IgnoreWhitespaceKind.None, IgnoreWhitespaceKind.AllSpace)]
        [TestCase(IgnoreWhitespaceKind.Eol, IgnoreWhitespaceKind.Change)]
        [TestCase(IgnoreWhitespaceKind.Eol, IgnoreWhitespaceKind.AllSpace)]
        [TestCase(IgnoreWhitespaceKind.Change, IgnoreWhitespaceKind.Eol)]
        [TestCase(IgnoreWhitespaceKind.Change, IgnoreWhitespaceKind.AllSpace)]
        [TestCase(IgnoreWhitespaceKind.AllSpace, IgnoreWhitespaceKind.Eol)]
        [TestCase(IgnoreWhitespaceKind.AllSpace, IgnoreWhitespaceKind.Change)]
        public void Should_correctly_reset_IgnoreWhitespaceMethod_to_None_with_two_clicks(IgnoreWhitespaceKind oldIgnoreWhitespace, IgnoreWhitespaceKind newIgnoreWhitespace)
        {
            Assert.AreNotEqual(oldIgnoreWhitespace, newIgnoreWhitespace);

            var accessor = _fileViewer.GetTestAccessor();

            accessor.IgnoreWhitespace = oldIgnoreWhitespace;

            switch (newIgnoreWhitespace)
            {
                case IgnoreWhitespaceKind.Eol:
                    accessor.IgnoreWhitespaceAtEolToolStripMenuItem_Click(null, null);
                    accessor.IgnoreWhitespaceAtEolToolStripMenuItem_Click(null, null);
                    break;
                case IgnoreWhitespaceKind.Change:
                    accessor.IgnoreWhitespaceChangesToolStripMenuItemClick(null, null);
                    accessor.IgnoreWhitespaceChangesToolStripMenuItemClick(null, null);
                    break;
                case IgnoreWhitespaceKind.AllSpace:
                    accessor.IgnoreAllWhitespaceChangesToolStripMenuItem_Click(null, null);
                    accessor.IgnoreAllWhitespaceChangesToolStripMenuItem_Click(null, null);
                    break;
            }

            accessor.IgnoreWhitespace.Should().Be(IgnoreWhitespaceKind.None);

            accessor.IgnoreWhitespaceAtEolButton.Checked.Should().Be(false);
            accessor.IgnoreWhitespaceAtEolMenuItem.Checked.Should().Be(false);

            accessor.IgnoreWhiteSpacesButton.Checked.Should().Be(false);
            accessor.IgnoreWhiteSpacesMenuItem.Checked.Should().Be(false);

            accessor.IgnoreAllWhitespacesButton.Checked.Should().Be(false);
            accessor.IgnoreAllWhitespacesMenuItem.Checked.Should().Be(false);
        }
    }
}
