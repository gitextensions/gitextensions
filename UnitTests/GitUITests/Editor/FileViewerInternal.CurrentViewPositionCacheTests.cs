using System.Threading;
using FluentAssertions;
using GitUI.Editor;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Editor
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public class CurrentViewPositionCacheTests
    {
        private IGitModule _module;
        private FileViewerInternal _fileViewerInternal;
        private FileViewerInternal.CurrentViewPositionCache _viewPositionCache;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _fileViewerInternal = new FileViewerInternal(() => _module);

            _viewPositionCache = new FileViewerInternal.CurrentViewPositionCache(_fileViewerInternal);
        }

        [TestCase(null)]
        [TestCase("a")]
        public void Capture_should_not_change_capture_if_less_then_two_lines(string text)
        {
            var test = _viewPositionCache.GetTestAccessor();

            var existingViewPosition = new FileViewerInternal.ViewPosition
            {
                FirstLine = "first line",
                FirstVisibleLine = 24,
                TotalNumberOfLines = 35
            };
            test.ViewPosition = existingViewPosition;

            test.TextEditor.ShowLineNumbers = true;
            test.TextEditor.Text = text;

            _viewPositionCache.Capture();

            test.ViewPosition.Should().Be(existingViewPosition);
        }

        [Test]
        public void Capture_should_capture_current_position_if_ShowLineNumbers_true()
        {
            var test = _viewPositionCache.GetTestAccessor();
            test.TextEditor.ShowLineNumbers = true;
            test.TextEditor.Text = "a\r\nb\r\nc\r\n";

            _viewPositionCache.Capture();

            Assert.Inconclusive("TODO:");

            // test.ViewPosition.ActiveLineNum.Should().Be(0);
        }

        [Test]
        public void Capture_should_capture_current_position_and_calculate_active_line_if_ShowLineNumbers_false()
        {
            var test = _viewPositionCache.GetTestAccessor();
            test.TextEditor.ShowLineNumbers = false;
            test.TextEditor.Text = "a\r\nb\r\nc\r\n";

            _viewPositionCache.Capture();

            Assert.Inconclusive("TODO:");
        }
    }
}