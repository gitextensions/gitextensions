using GitCommands;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class PathEqualityComparerTests
    {
        private PathEqualityComparer _comparer;

        [SetUp]
        public void Setup()
        {
            _comparer = new PathEqualityComparer();
        }


        [Platform(Include = "Win")]
        [TestCase("C:\\WORK\\GitExtensions\\", "C:/Work/GitExtensions/")]
        [TestCase("\\\\my-pc\\Work\\GitExtensions\\", "//my-pc/WORK/GitExtensions/")]
        public void Equals(string input, string expected)
        {
            Assert.AreEqual(_comparer.Equals(input, expected), true);
        }

        [Platform(Exclude = "Win")]
        [TestCase("/Work/GitExtensions/", "/Work/GitExtensions/", true)]
        [TestCase("/WORK/GitExtensions/", "/Work/GitExtensions/", false)]
        [TestCase("//my-pc/Work/GitExtensions/", "//my-pc/Work/GitExtensions/", true)]
        public void Equals_Mono(string input, string expected, bool isEqual)
        {
            Assert.AreEqual(_comparer.Equals(input, expected), isEqual);
        }
    }
}
