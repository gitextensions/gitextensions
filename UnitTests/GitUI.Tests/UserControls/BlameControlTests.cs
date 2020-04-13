using System;
using System.Collections.Generic;
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
        private BlameControl _blameControl;
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

            _blameControl = new BlameControl();
            _blameControl.GetTestAccessor().Blame = new GitBlame(new GitBlameLine[]
            {
                _gitBlameLine,
                new GitBlameLine(blameCommit1, 2, 2, "line2"),
                new GitBlameLine(blameCommit2, 3, 3, "line3"),
                new GitBlameLine(blameCommit2, 4, 4, "line4"),
            });
        }

        [TearDown]
        public void TearDown()
        {
            _blameControl.Dispose();
        }

        [TestCase(true, true, true, true, "author1 - 3/22/2010 - fileName.txt")]
        [TestCase(true, true, true, false, "3/22/2010 - author1 - fileName.txt")]
        [TestCase(false, true, true, false, "author1 - fileName.txt")]
        [TestCase(true, false, true, false, "3/22/2010 - fileName.txt")]
        [TestCase(true, true, false, false, "3/22/2010 - author1")]
        public void BuildAuthorLine_When_FilePath_IsDifferent(bool showAuthorDate, bool showAuthor, bool showFilePath, bool displayAuthorFirst, string expectedResult)
        {
            var line = new StringBuilder();

            _blameControl.GetTestAccessor().BuildAuthorLine(_gitBlameLine, line, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
                "fileName_different.txt", showAuthor, showAuthorDate, showFilePath, displayAuthorFirst);

            line.ToString().Should().StartWith(expectedResult);
        }

        [Test]
        public void BuildAuthorLine_When_FilePath_Is_Identic()
        {
            var line = new StringBuilder();

            _blameControl.GetTestAccessor().BuildAuthorLine(_gitBlameLine, line, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
                "fileName.txt", true, true, true, false);

            line.ToString().Should().StartWith("3/22/2010 - author1");
        }

        [Test]
        public void BuildBlameContents_WithDateAndTime()
        {
            var originalValue = AppSettings.BlameShowAuthorTime;

            try
            {
                AppSettings.BlameShowAuthorTime = true;

                var (gutter, content, _) = _blameControl.GetTestAccessor().BuildBlameContents("fileName.txt");

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
                var (gutter, content, _) = _blameControl.GetTestAccessor().BuildBlameContents("fileName.txt");

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

        private IEnumerable<GitBlameLine> CreateBlameLine(params DateTime[] lineDates)
        {
            for (var index = 0; index < lineDates.Length; index++)
            {
                DateTime lineDate = lineDates[index];
                yield return new GitBlameLine(
                    new GitBlameCommit(null, "Author1", "@Author1", lineDate, string.Empty,
                        "Commiter", "@Committer", lineDate, string.Empty, "Summary1", "file"),
                    index + 1, index + 1, "text");
            }
        }

        [Test]
        public void CalculateBlameGutterData_When_date_is_older_than_artificial_old_boundary_Then_it_defines_first_age_bucket_and_so_falls_into_it()
        {
            // Given
            var sut = _blameControl.GetTestAccessor();
            IReadOnlyList<GitBlameLine> blameLines = CreateBlameLine(sut.ArtificialOldBoundary.AddDays(-1)).ToList();

            // When
            var blameEntries = sut.CalculateBlameGutterData(blameLines);

            // Then
            blameEntries.Should().HaveCount(1);
            blameEntries[0].AgeBucketIndex.Should().Be(0);
        }

        [Test]
        public void CalculateBlameGutterData_When_date_is_newer_than_artificial_old_boundary_Then_it_falls_in_a_later_age_bucket()
        {
            // Given
            IReadOnlyList<GitBlameLine> blameLines = CreateBlameLine(DateTime.Now.AddMonths(-18)).ToList();

            // When
            var blameEntries = _blameControl.GetTestAccessor().CalculateBlameGutterData(blameLines);

            // Then
            blameEntries.Should().HaveCount(1);
            blameEntries[0].AgeBucketIndex.Should().Be(3);
        }

        [Test]
        public void CalculateBlameGutterData_When_date_is_DateMin_Then_it_falls_in_a_first_age_bucket()
        {
            // Given
            IReadOnlyList<GitBlameLine> blameLines = CreateBlameLine(DateTime.Now.AddMonths(-18), DateTime.MinValue).ToList();

            // When
            var blameEntries = _blameControl.GetTestAccessor().CalculateBlameGutterData(blameLines);

            // Then
            blameEntries.Should().HaveCount(2);
            blameEntries[1].AgeBucketIndex.Should().Be(0);
        }

        [Test]
        public void CalculateBlameGutterData_When_all_dates_are_DateMin_Then_they_falls_in_a_first_age_bucket()
        {
            // Given
            IReadOnlyList<GitBlameLine> blameLines = CreateBlameLine(DateTime.MinValue, DateTime.MinValue).ToList();

            // When
            var blameEntries = _blameControl.GetTestAccessor().CalculateBlameGutterData(blameLines);

            // Then
            blameEntries.Should().HaveCount(2);
            blameEntries[0].AgeBucketIndex.Should().Be(0);
            blameEntries[1].AgeBucketIndex.Should().Be(0);
        }

        [Test]
        public void CalculateBlameGutterData_When_dates_just_after_age_bucket_limit_Then_One_date_in_each_age_bucket()
        {
            // Given
            var now = DateTime.Now;
            var marginError = TimeSpan.FromMinutes(10); // Due to the DateTime.Now value which is slightly different
            IReadOnlyList<GitBlameLine> blameLines = CreateBlameLine(
                now.AddDays(-7 * 365), // Because there are 7 age buckets (corresponding to the different colors)
                now.AddDays(-6 * 365).Add(marginError),
                now.AddDays(-5 * 365).Add(marginError),
                now.AddDays(-4 * 365).Add(marginError),
                now.AddDays(-3 * 365).Add(marginError),
                now.AddDays(-2 * 365).Add(marginError),
                now.AddDays(-1 * 365).Add(marginError),
                now.Add(-marginError)).ToList();

            // When
            var blameEntries = _blameControl.GetTestAccessor().CalculateBlameGutterData(blameLines);

            // Then
            blameEntries.Should().HaveCount(8);
            blameEntries[0].AgeBucketIndex.Should().Be(0);
            blameEntries[1].AgeBucketIndex.Should().Be(1);
            blameEntries[2].AgeBucketIndex.Should().Be(2);
            blameEntries[3].AgeBucketIndex.Should().Be(3);
            blameEntries[4].AgeBucketIndex.Should().Be(4);
            blameEntries[5].AgeBucketIndex.Should().Be(5);
            blameEntries[6].AgeBucketIndex.Should().Be(6);
            blameEntries[7].AgeBucketIndex.Should().Be(6);
        }
    }
}
