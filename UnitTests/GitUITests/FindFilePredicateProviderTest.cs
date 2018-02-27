﻿using System;
using FluentAssertions;
using GitUI;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public class FindFilePredicateProviderTest
    {
        private static readonly string patternDefault = "test2";
        private static readonly string workingDirDefault = @"D:\";

        private IFindFilePredicateProvider provider;

        [SetUp]
        public void Init()
        {
            provider = new FindFilePredicateProvider();
        }

        [TestCase(null)]
        public void Get_should_throw_if_pattern_is_null(string pattern)
        {
            Action predicate = () => { provider.Get(pattern, workingDirDefault); };
            predicate.ShouldThrow<ArgumentNullException>();
        }

        [TestCase("")]
        [TestCase(" ")]
        public void Get_should_not_throw_if_pattern_is_empty(string pattern)
        {
            Action predicate = () => { provider.Get(pattern, workingDirDefault); };
            predicate.ShouldNotThrow<ArgumentNullException>();
        }

        [TestCase(null)]
        public void Get_should_throw_if_workingDir_is_null(string workingDir)
        {
            Action predicate = () => { provider.Get(patternDefault, workingDir); };
            predicate.ShouldThrow<ArgumentNullException>();
        }

        [TestCase("")]
        [TestCase(" ")]
        public void Get_should_throw_if_workingDir_is_empty(string workingDir)
        {
            Action predicate = () => { provider.Get(patternDefault, workingDir); };
            predicate.ShouldNotThrow<ArgumentNullException>();
        }

        [TestCase(@"test2/t", "test1/test2/test3")]
        [TestCase(@"\test2\t", "test1/test2/test3")]
        public void Get_should_correct_work_with_slashes_and_backslashes_in_pattern(string pattern, string filePath)
        {
            var predicate = provider.Get(pattern, workingDirDefault);
            Assert.True(predicate(filePath));
        }

        [TestCase(@"\test2\t", "D:/test1/test2/test3/")]
        public void Get_should_not_throw_then_workingDir_lenght_greater_that_pattern_length(string pattern, string workingDir)
        {
            Action predicate = () => { provider.Get(pattern, workingDir); };
            predicate.ShouldNotThrow<ArgumentNullException>();
        }

        [TestCase(@"D:\test1", @"D:/", "test1/test2/test3/")]
        [TestCase(@"D:/test1", @"D:/", "test1/test2/test3/")]
        [TestCase(@"D:\test1", @"D:\", "test1/test2/test3/")]
        [TestCase(@"D:/test1", @"D:\", "test1/test2/test3/")]
        [TestCase(@"D:\test1", @"D:", "test1/test2/test3/")]
        [TestCase(@"D:/test1", @"D:", "test1/test2/test3/")]
        public void Get_should_work_correct_when_workingDir_end_with_slash_or_not(string pattern, string workingDir, string filePath)
        {
            var predicate = provider.Get(pattern, workingDir);
            Assert.True(predicate(filePath));
        }

        [TestCase(@"tEsT2", @"D:/", "Test1/teST2/test3/")]
        [TestCase(@"D:\Test\test1", @"D:/TEST", "teSt1/test2/test3/")]
        public void Get_should_work_with_diferent_cases(string pattern, string workingDir, string filePath)
        {
            var predicate = provider.Get(pattern, workingDir);
            Assert.True(predicate(filePath));
        }

        [TestCase(@"D:/test1", @"D:/", "test1/test2/test3/", ExpectedResult = true)]
        [TestCase(@"D:/test2", @"D:/", "test1/test2/test3/", ExpectedResult = false)]
        [TestCase(@"D:/test/test1", @"D:/test", "test1/test2/test3/", ExpectedResult = true)]
        [TestCase(@"//d/test/test1", @"//d/test\", "test1/test2/test3/", ExpectedResult = true)]
        public bool Get_should_use_startwith_when_pattern_started_with_workingDir(string pattern, string workingDir, string filePath)
        {
            var predicate = provider.Get(pattern, workingDir);
            return predicate(filePath);
        }

        [TestCase(@"test2", @"D:/", "test1/test2/test3/", ExpectedResult = true)]
        [TestCase(@"test1", @"D:/test", "test1/test2/test3/", ExpectedResult = true)]
        public bool Get_should_use_contains_when_pattern_does_not_started_with_workingDir(string pattern, string workingDir, string filePath)
        {
            var predicate = provider.Get(pattern, workingDir);
            return predicate(filePath);
        }

        [TestCase( null )]
        [TestCase( "" )]
        [TestCase( " " )]
        public void Get_should_not_throw_then_filePath_is_null_or_empty( string filePath )
        {
            var predicate = provider.Get(patternDefault, workingDirDefault);

            Action executor = () => { predicate( filePath ); };
            executor.ShouldNotThrow<ArgumentNullException>();
        }
    }
}
