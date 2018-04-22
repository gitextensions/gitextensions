using System.Linq;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests
{
    [TestFixture]
    public class LocalGitRefListTest
    {
        [Test]
        public void List_should_sort_branch_names_alphabetically()
        {
            var list = new LocalGitRefList(TestOutput);

            list.OrderedBranches(BranchOrdering.Alphabetically)
                .Select(r => r.Name)
                .Should().BeEquivalentTo(new[] { "feature/spellchecker", "master", "mono" }, options => options.WithStrictOrdering());
        }

        [Test]
        public void List_should_sort_branch_names_by_date()
        {
            var list = new LocalGitRefList(TestOutput);

            list.OrderedBranches(BranchOrdering.ByLastAccessDate)
                .Select(r => r.Name)
                .Should().BeEquivalentTo(new[] { "master", "mono", "feature/spellchecker" }, options => options.WithStrictOrdering());
        }

        [Test]
        public void List_should_sort_tag_names_alphabetically()
        {
            var list = new LocalGitRefList(TestOutput);

            list.OrderedTags(BranchOrdering.Alphabetically)
                .Select(r => r.Name)
                .Should().BeEquivalentTo(new[] { "2.47.3", "master_Net2.0", "v2.49.01" }, options => options.WithStrictOrdering());
        }

        [Test]
        public void List_should_sort_tag_names_by_date()
        {
            var list = new LocalGitRefList(TestOutput);

            list.OrderedTags(BranchOrdering.ByLastAccessDate)
                .Select(r => r.Name)
                .Should().BeEquivalentTo(new[] { "v2.49.01", "2.47.3", "master_Net2.0" }, options => options.WithStrictOrdering());
        }

        private static string TestOutput =>
            "1489307102 +0100 4d8fb6a0d1ab4e7822f5a19121894e2c29f94679 refs/tags/v2.49.01\n" +
            "1292079806 +0100 a275544476cfbb4d996d55a1756d6779d19a25b1 refs/tags/master_Net2.0\n" +
            "1384546330 +0100 591c4059a17b89c19463a58026086f3016fc53a9 refs/tags/2.47.3\n" +
            "1477419020 +0100 5303e7114f1896c639dea0231fac522752cc44a2 refs/heads/mono\n" +
            "1524003998 +1000 68f7aff9912d91b74cbcda14b4e4ce6e0743a013 refs/heads/master\n" +
            "1394044850 +0400 ffeaadd0d0cd55b221ae43b151a2ecad7bfb32eb refs/heads/feature/spellchecker\n";
    }
}