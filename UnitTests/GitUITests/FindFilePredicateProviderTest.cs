using GitUI;
using NUnit.Framework;

namespace GitUITests
{
    [TestFixture]
    public class FindFilePredicateProviderTest
    {
        private IFindFilePredicateProvider provider;

        [SetUp]
        public void Init()
        {
            provider = new FindFilePredicateProvider();
        }

        [TestCase(@"Code", @"D:\", "TestCode", ExpectedResult = true)]
        [TestCase(@"Code", @"D:\", "TestCode/File1", ExpectedResult = true)]
        [TestCase(@"code", @"D:\", "TestCode/File1", ExpectedResult = true)]
        [TestCase(@"/Code", @"D:\", "TestCode/File1", ExpectedResult = false)]
        [TestCase(@"Code/", @"D:\", "TestCode/File1", ExpectedResult = true)]
        [TestCase(@"Code\", @"D:\", "TestCode/File1", ExpectedResult = true)]
        [TestCase(@"D:/Code\", @"D:\", "TestCode/File1", ExpectedResult = false)]
        [TestCase(@"D:/Test/", @"D:\", "TestCode/File1", ExpectedResult = false)]
        [TestCase(@"D:\Test", @"D:\", "TestCode/File1", ExpectedResult = true)]
        [TestCase(@"//d/git\Test", @"//d/git", "TestCode/File1", ExpectedResult = true)]
        [TestCase(@"c/dir1/dir2", @"F:\", "src/dir1/dir2/dir3", ExpectedResult = true)]
        [TestCase(@"c\dir1/dir2", @"F:\", "src/dir1/dir2/dir3", ExpectedResult = true)]
        [TestCase(@"c/dir1\dir2", @"F:\", "src/dir1/dir2/dir3", ExpectedResult = true)]
        public bool UsageScenario(string pattern, string workingDir, string fileName)
        {
            var predicate = provider.Get(pattern, workingDir);
            return predicate.Invoke(fileName);
        }
    }
}
