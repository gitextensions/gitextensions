using System.IO;
using System.Linq;
using System.Text;
using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class GitModuleTest
    {
        private GitModule _gitModule;
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

        [TestCase(false, false, @"", false, false, false, @"", @"commit")]
        [TestCase(true, false, @"", false, false, false, @"", @"commit --amend")]
        [TestCase(false, true, @"", false, false, false, @"", @"commit --signoff")]
        [TestCase(false, false, @"", true, false, false, @"", @"commit -F ""COMMITMESSAGE""")]
        [TestCase(false, false, @"", false, true, false, @"", @"commit --no-verify")]
        [TestCase(false, false, @"", false, false, false, @"12345678", @"commit")]
        [TestCase(false, false, @"", false, false, true, @"", @"commit -S")]
        [TestCase(false, false, @"", false, false, true, @"      ", @"commit -S")]
        [TestCase(false, false, @"", false, false, true, null, @"commit -S")]
        [TestCase(false, false, @"", false, false, true, @"12345678", @"commit -S12345678")]
        [TestCase(true, true, @"", true, true, true, @"12345678", @"commit --amend --no-verify --signoff -S12345678 -F ""COMMITMESSAGE""")]
        public void CommitCmdTests(bool amend, bool signOff, string author, bool useExplicitCommitMessage, bool noVerify, bool gpgSign, string gpgKeyId, string expected)
        {
            var actual = _gitModule.CommitCmd(amend, signOff, author, useExplicitCommitMessage, noVerify, gpgSign, gpgKeyId);
            StringAssert.AreEqualIgnoringCase(expected, actual);
        }

        [Test]
        public void ParseGitBlame()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestData/README.blame");

            var result = _gitModule.ParseGitBlame(File.ReadAllText(path), Encoding.UTF8);

            Assert.AreEqual(80, result.Lines.Count);

            Assert.AreEqual("957ff3ce9193fec3bd2578378e71676841804935", result.Lines[0].Commit.ObjectId);
            Assert.AreEqual("# Git Extensions", result.Lines[0].Text);

            Assert.AreEqual(1, result.Lines[0].OriginLineNumber);
            Assert.AreEqual(1, result.Lines[0].FinalLineNumber);

            Assert.AreSame(result.Lines[0].Commit, result.Lines[1].Commit);
            Assert.AreSame(result.Lines[0].Commit, result.Lines[6].Commit);

            Assert.AreEqual("e3268019c66da7534414e9562ececdee5d455b1b", result.Lines.Last().Commit.ObjectId);
            Assert.AreEqual("", result.Lines.Last().Text);
        }

        [Test]
        public void UnescapeOctalCodePoints_handles_octal_codes()
        {
            Assert.AreEqual("", GitModule.UnescapeOctalCodePoints(null));
            Assert.AreEqual("", GitModule.UnescapeOctalCodePoints(""));
            Assert.AreEqual(" ", GitModule.UnescapeOctalCodePoints(" "));
            Assert.AreEqual("Hello, World!", GitModule.UnescapeOctalCodePoints("Hello, World!"));

            // escaped octal code points (Korean Hangul in this case)
            Assert.AreEqual("두다.txt", GitModule.UnescapeOctalCodePoints(@"\353\221\220\353\213\244.txt"));

            // 777 is an invalid byte, which is omitted from the output
            Assert.AreEqual("Invalid byte .txt", GitModule.UnescapeOctalCodePoints(@"Invalid byte \777.txt"));

            // If nothing was escaped in the original string, the same string instance is returned
            var s = "Hello, World!".ToUpper();
            Assert.AreSame(s, GitModule.UnescapeOctalCodePoints(s));
        }
    }
}
