using System;
using FluentAssertions;
using GitExtUtils.FileLogging;
using NUnit.Framework;

namespace FileLoggingTests
{
    [TestFixture]
    public sealed class LogPathGeneratorTests
    {
        private const string LogPath = "c:\\temp\\gitextensions";

        [Test]
        public void Logpath_is_unmodified()
        {
            var generator = CreateGenerator();
            generator.LogPath.Should().Be(LogPath);
        }

        [Test]
        public void GetFilenamePattern_returns_pattern_to_delete_logfiles()
        {
            var generator = CreateGenerator();
            generator.GetFilenamePattern().Should().Be("mylogname-*-*.log");
        }

        [Test]
        public void GenerateFullPath_returns_a_full_path_of_the_day_and_changes_file_name_on_new_date()
        {
            var generator = CreateGenerator();
            var t1 = new DateTime(2019, 10, 9, 8, 7, 6);
            var t2 = t1.AddDays(1);

            generator.GenerateFullPath(t1).Should().Be(@"c:\temp\gitextensions\mylogname-20191009-666.log");
            generator.GenerateFullPath(t2).Should().Be(@"c:\temp\gitextensions\mylogname-20191010-666.log");
        }

        private static LogPathGenerator CreateGenerator()
        {
            return new LogPathGenerator(LogPath, "mylogname", 666);
        }
    }
}