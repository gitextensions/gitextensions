using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public class GitStashTests
    {
        [TestCase("stash@{0}: Very descriptive message", 0, "stash@{0}", "Very descriptive message")]
        [TestCase("stash@{1}: On 4263_View_Stash_AOORE: Testing things", 1, "stash@{1}", "On 4263_View_Stash_AOORE: Testing things")]
        [TestCase("stash@{2}: WIP on 4263_View_Stash_AOORE: a8348732f Merge pull request #4259 from RussKie/fix_4258_NRE_in_FormPull", 2, "stash@{2}", "WIP on 4263_View_Stash_AOORE: a8348732f Merge pull request #4259 from RussKie/fix_4258_NRE_in_FormPull")]
        [TestCase("stash@{3}: WIP on master: Test", 3, "stash@{3}", "WIP on master: Test")]
        public void Can_parse_stash_names(string rawStash, int index, string name, string message)
        {
            var stash = new GitStash(rawStash, index);

            stash.Index.Should().Be(index);
            stash.Message.Should().Be(message);
            stash.Name.Should().Be(name);
        }
    }
}

