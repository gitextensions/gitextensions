using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class GitModuleTest
    {
        GitModule _gitModule;
        [SetUp]
        public void SetUp()
        {
            _gitModule = new GitModule(null);
        }

        [TestCase(@"  ""author <author@mail.com>""  ", @"commit --author=""author <author@mail.com>"" -F ""COMMITMESSAGE""")]
        [TestCase(@"""author <author@mail.com>""", @"commit --author=""author <author@mail.com>"" -F ""COMMITMESSAGE""")]
        [TestCase(@"author <author@mail.com>", @"commit --author=""author <author@mail.com>"" -F ""COMMITMESSAGE""")]
        public void CommitCmdShouldTrimAuthor(string input, string expected)
        {
            var actual = _gitModule.CommitCmd(false, author: input);
            StringAssert.AreEqualIgnoringCase(expected, actual);
        }
    }
}
