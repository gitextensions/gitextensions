using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using GitCommands;
using GitCommands.Utils;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class EnvironmentPathsProviderTests
    {
        private string _separator;
        private IEnvironmentAbstraction _environment;
        private IEnvironmentPathsProvider _provider;

        [SetUp]
        public void Setup()
        {
            _separator = EnvUtils.EnvVariableSeparator.ToString();

            _environment = Substitute.For<IEnvironmentAbstraction>();
            _provider = new EnvironmentPathsProvider(_environment);
        }

        [Test]
        public void GetEnvironmentValidPaths()
        {
            string pathVariable = string.Join(_separator, GetValidPaths().Concat(GetInvalidPaths()));
            _environment.GetEnvironmentVariable("PATH").Returns(x => pathVariable);

            var validPaths = _provider.GetEnvironmentValidPaths();

            CollectionAssert.AreEqual(GetValidPaths().ToArray(), validPaths.ToArray());
        }

        [Test]
        public void GetEnvironmentValidPaths_quoted()
        {
            var paths = GetValidPaths().Concat(GetInvalidPaths());
            var quotedPaths = paths.Select(path => path.Quote(" ")).Select(path => path.Quote());
            string pathVariable = string.Join(_separator, quotedPaths);
            _environment.GetEnvironmentVariable("PATH").Returns(pathVariable);

            var validPaths = _provider.GetEnvironmentValidPaths();

            CollectionAssert.AreEqual(GetValidPaths().ToArray(), validPaths.ToArray());
        }

        [Platform(Include = "Win")]
        [TestCase("\\\\my-pc\\Work\\GitExtensions\\", false)]
        [TestCase("C:\\Work\\GitExtensions\\", true)]
        [TestCase("C:\\Work\\", true)]
        [TestCase("C:\\", true)]
        [TestCase("C:", true)]
        [TestCase("", false)]
        [TestCase("\"C:\\Work\\GitExtensions\\", false)]
        public void IsValidPath(string given, bool expected)
        {
            EnvironmentPathsProvider.IsValidPath(given).Should().Be(expected);
        }

        private static IEnumerable<string> GetInvalidPaths()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                yield return @"c::\word";
                yield return "\"c:\\word\t\\\"";
                yield return @".c:\Programs\";
            }
            else
            {
                // I am not able to figure out any invalid (giving exception) path under mono
            }
        }

        private static IEnumerable<string> GetValidPaths()
        {
            if (Path.DirectorySeparatorChar == '\\')
            {
                yield return @"c:\work";
                yield return @"c:\work\";
                yield return @"c:\Program Files(86)\";
                yield return @"c:\Program Files(86)\Git";
            }
            else
            {
                yield return "/etc/init.d/xvfb";
                yield return "/var";
                yield return "/";
            }
        }
    }
}
