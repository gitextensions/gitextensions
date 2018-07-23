using System;
using FluentAssertions;
using GitCommands;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class CommitDataManagerTest
    {
        private CommitDataManager _commitDataManager;
        private IGitModule _module;
        private Func<IGitModule> _getModule;

        [SetUp]
        public void Setup()
        {
            _module = Substitute.For<IGitModule>();
            _module.ReEncodeStringFromLossless(Arg.Any<string>()).Returns(x => x[0]);
            _module.ReEncodeCommitMessage(Arg.Any<string>(), Arg.Any<string>()).Returns(x => x[0]);

            _getModule = () => _module;
            _commitDataManager = new CommitDataManager(_getModule);
        }

        [Test]
        public void CreateFromFormattedData()
        {
            var body = "\tI made a really neat change.\n\n" +
                       "Notes (p4notes):\n" +
                       "\tP4@547123";

            var rawData = "37da2014bc5128ca084543423e410d81df838845\n" +
                          "a13ae23be4d207c7af2818fd7cc2caa2d0a63e47\n" +
                          "ad1ccc2ecc00865d61e74b703a260d17b4db1216 a8d564d3bb8c65e88e40239937cb48dda57f01b8 1711f4a6522f86c3e4e404a66a78f4586d25d89d\n" +
                          "John Doe (Acme Inc) <John.Doe@test.com>\n" +
                          "1508676972\n" +
                          "John Doe <John.Doe@test.com>\n" +
                          "1508676972\n" +
                          "\n" +
                          body;

            var data = _commitDataManager.CreateFromFormattedData(rawData);

            data.Should().NotBeNull();
            data.Author.Should().Be("John Doe (Acme Inc) <John.Doe@test.com>");
            data.Committer.Should().Be("John Doe <John.Doe@test.com>");
            data.ObjectId.Should().Be(ObjectId.Parse("37da2014bc5128ca084543423e410d81df838845"));
            data.ParentGuids.Should().BeEquivalentTo(ObjectId.Parse("ad1ccc2ecc00865d61e74b703a260d17b4db1216"), ObjectId.Parse("a8d564d3bb8c65e88e40239937cb48dda57f01b8"), ObjectId.Parse("1711f4a6522f86c3e4e404a66a78f4586d25d89d"));
            data.TreeGuid.Should().Be(ObjectId.Parse("a13ae23be4d207c7af2818fd7cc2caa2d0a63e47"));

            data.AuthorDate.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss").Should().Be("2017-10-22T12:56:12");
            data.CommitDate.UtcDateTime.ToString("yyyy-MM-ddTHH:mm:ss").Should().Be("2017-10-22T12:56:12");

            // split the text into lines because of the different newline characters for Windows and Linux
            // TODO: it looks like a bug, data.Body has an extra trailing newline
            data.Body.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).Should()
                .BeEquivalentTo((body + Environment.NewLine).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None));

            // TODO: this is a bad API design, a collection should not be null
            data.ChildIds.Should().BeNull();
        }
    }
}