using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using GitCommands;
using GitUI.Blame;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUITests.UserControls
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class BlameControlTests
    {
        private BlameControl.TestAccessor _sut;
        private GitBlameLine _gitBlameLine;

        [SetUp]
        public void SetUp()
        {
            var blameCommit1 = new GitBlameCommit(
                ObjectId.Random(),
                "author1",
                "author1@mail.fake",
                new DateTime(2010, 3, 22, 12, 01, 02),
                "authorTimeZone",
                "committer1",
                "committer1@authormail.com",
                new DateTime(2010, 3, 22, 13, 01, 02),
                "committerTimeZone",
                "test summary commit1",
                "fileName.txt");

            var blameCommit2 = new GitBlameCommit(
                ObjectId.Random(),
                "author2",
                "author2@mail.fake",
                new DateTime(2011, 3, 22, 12, 01, 02),
                "authorTimeZone",
                "committer2",
                "committer2@authormail.com",
                new DateTime(2011, 3, 22, 13, 01, 02),
                "committerTimeZone",
                "test summary commit2",
                "fileName.txt");

            _gitBlameLine = new GitBlameLine(blameCommit1, 1, 1, "line1");

            _sut = new BlameControl().GetTestAccessor();
            _sut.Blame = new GitBlame(new GitBlameLine[]
            {
                _gitBlameLine,
                new GitBlameLine(blameCommit1, 2, 2, "line2"),
                new GitBlameLine(blameCommit2, 3, 3, "line3"),
                new GitBlameLine(blameCommit2, 4, 4, "line4"),
            });
        }

        [TestCase(true, true, true, true, "author1 - 3/22/2010 - fileName.txt")]
        [TestCase(true, true, true, false, "3/22/2010 - author1 - fileName.txt")]
        [TestCase(false, true, true, false, "author1 - fileName.txt")]
        [TestCase(true, false, true, false, "3/22/2010 - fileName.txt")]
        [TestCase(true, true, false, false, "3/22/2010 - author1")]
        public void BuildAuthorLine_When_FilePath_IsDifferent(bool showAuthorDate, bool showAuthor, bool showFilePath, bool displayAuthorFirst, string expectedResult)
        {
            var line = new StringBuilder();

            _sut.BuildAuthorLine(_gitBlameLine, line, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, "fileName_different.txt", showAuthor, showAuthorDate, showFilePath, displayAuthorFirst);

            line.ToString().Should().StartWith(expectedResult);
        }

        [Test]
        public void BuildAuthorLine_When_FilePath_Is_Identic()
        {
            var line = new StringBuilder();

            _sut.BuildAuthorLine(_gitBlameLine, line, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, "fileName.txt", true, true, true, false);

            line.ToString().Should().StartWith("3/22/2010 - author1");
        }

        [Test]
        public void BuildBlameContents_WithDateAndTime()
        {
            var originalValue = AppSettings.BlameShowAuthorTime;

            try
            {
                AppSettings.BlameShowAuthorTime = true;

                var (gutter, content) = _sut.BuildBlameContents("fileName.txt");

                content.Should().Be($"line1{Environment.NewLine}line2{Environment.NewLine}line3{Environment.NewLine}line4{Environment.NewLine}");
                var gutterLines = gutter.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                gutterLines[0].TrimEnd().Should().Be("3/22/2010 12:01 PM - author1");
                gutterLines[1].Should().NotBeNullOrEmpty().And.BeNullOrWhiteSpace();
                gutterLines[2].TrimEnd().Should().Be("3/22/2011 12:01 PM - author2");
                gutterLines[3].Should().NotBeNullOrEmpty().And.BeNullOrWhiteSpace();

                // All the lines have the same length
                gutterLines.Select(l => l.Length).Should().AllBeEquivalentTo(gutterLines[0].Length);
            }
            finally
            {
                AppSettings.BlameShowAuthorTime = originalValue;
            }
        }

        [Test]
        public void BuildBlameContents_WithDateButNotTime()
        {
            var originalValue = AppSettings.BlameShowAuthorTime;

            try
            {
                // Given
                AppSettings.BlameShowAuthorTime = false;

                // When
                var (gutter, content) = _sut.BuildBlameContents("fileName.txt");

                // Then
                content.Should().Be($"line1{Environment.NewLine}line2{Environment.NewLine}line3{Environment.NewLine}line4{Environment.NewLine}");
                var gutterLines = gutter.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                gutterLines[0].TrimEnd().Should().Be($"3/22/2010 - author1");
                gutterLines[1].Should().NotBeNullOrEmpty().And.BeNullOrWhiteSpace();
                gutterLines[2].TrimEnd().Should().Be($"3/22/2011 - author2");
                gutterLines[3].Should().NotBeNullOrEmpty().And.BeNullOrWhiteSpace();

                // All the lines have the same length
                gutterLines.Select(l => l.Length).Should().AllBeEquivalentTo(gutterLines[0].Length);
            }
            finally
            {
                AppSettings.BlameShowAuthorTime = originalValue;
            }
        }
    }
}
