using System;
using FluentAssertions;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests
{
    public class LocalGitRefListItemTest
    {
        [TestFixture]
        public class LineParsing
        {
            [Test]
            public void Item_should_parse_time_from_first_column()
            {
                var item = CreateItem();

                item.Date.ToUniversalTime().Should().Be(new DateTime(2008, 12, 15, 19, 45, 51));
            }

            [Test]
            public void Item_should_parse_object_name_from_third_column()
            {
                var item = CreateItem();

                item.Ref.Guid.Should().Be("943d230ba465d86c3ad2cd00f7e8c508d144d9a5");
            }

            [Test]
            public void Item_should_parse_object_name_from_fourth_column()
            {
                var item = CreateItem();

                item.Ref.CompleteName.Should().Be("refs/tags/0.90");
            }
        }

        [TestFixture]
        public class IntegrationWithGitRef
        {
            [Test]
            [TestCase("refs/heads/trunk", true)]
            [TestCase("/refs/heads/trunk", false)]
            [TestCase("refs/tags/0.90", false)]
            [TestCase("refs/remotes/origin/master", false)]
            public void Item_should_recognize_branches(string refName, bool isBranch)
            {
                var item = CreateItem(refName);

                item.Ref.IsHead.Should().Be(isBranch);
            }

            [Test]
            [TestCase("refs/heads/trunk", "trunk")]
            [TestCase("refs/heads/feature/magic", "feature/magic")]
            public void Item_should_return_friendly_names_for_branches(string refName, string expectedName)
            {
                var item = CreateItem(refName);

                item.Ref.Name.Should().Be(expectedName);
            }

            [Test]
            [TestCase("refs/heads/trunk", false)]
            [TestCase("refs/tags/0.90", true)]
            [TestCase("!refs/tags/0.90", false)]
            [TestCase("refs/remotes/origin/master", false)]
            public void Item_should_recognize_tags(string refName, bool isTag)
            {
                var item = CreateItem(refName);

                item.Ref.IsTag.Should().Be(isTag);
            }

            [Test]
            [TestCase("refs/tags/0.90", "0.90")]
            public void Item_should_return_friendly_names_for_tags(string refName, string expectedName)
            {
                var item = CreateItem(refName);

                item.Ref.Name.Should().Be(expectedName);
            }
        }

        private static LocalGitRefList.Item CreateItem(string refName = "refs/tags/0.90")
        {
            return new LocalGitRefList.Item(@"1229370351 +0100 943d230ba465d86c3ad2cd00f7e8c508d144d9a5 " + refName);
        }
    }
}