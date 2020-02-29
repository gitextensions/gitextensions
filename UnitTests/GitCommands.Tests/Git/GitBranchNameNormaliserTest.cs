using FluentAssertions;
using GitCommands.Git;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public sealed class GitBranchNameNormaliserTest
    {
        private GitBranchNameNormaliser _gitBranchNameNormaliser;
        private GitBranchNameOptions _gitBranchNameOptions;

        [SetUp]
        public void Setup()
        {
            _gitBranchNameNormaliser = new GitBranchNameNormaliser();
            _gitBranchNameOptions = new GitBranchNameOptions("_");
        }

        [TestCase("[BUG-1234] some long description located [//test/*/dir?/.lock]{3,} and foo:bar\\can...", "_", "_BUG-1234]_some_long_description_located__/test/_/dir_/_lock]{3,}_and_foo_bar_can_")]
        [TestCase("[BUG-1234] some long description located [//test/*/dir?/.lock]{3,} and foo:bar\\can...", "-", "-BUG-1234]-some-long-description-located--/test/-/dir-/-lock]{3,}-and-foo-bar-can-")]
        [TestCase("[BUG-1234] some long description located [//test/*/dir?/.lock]{3,} and foo:bar\\can...", "", "BUG-1234]somelongdescriptionlocated/test/dir/lock]{3,}andfoobarcan")]
        public void Normalise(string input, string replacementToken, string expected)
        {
            _gitBranchNameOptions = new GitBranchNameOptions(replacementToken);
            _gitBranchNameNormaliser.Normalise(input, _gitBranchNameOptions).Should().Be(expected);
        }

        // They can include slash / for hierarchical (directory) grouping,
        // but no slash-separated component can begin with a dot . or end with the sequence .lock.
        [TestCase(".test", "_test")]
        [TestCase("hierarchy/.test", "hierarchy/_test")]
        [TestCase("hierarchy/.test/foo", "hierarchy/_test/foo")]
        [TestCase(".lock", "_lock")]
        [TestCase("pad.lock", "pad_lock")]
        [TestCase("pad.lock.lock", "pad.lock_lock")]
        [TestCase("hierarchy/sher.lock", "hierarchy/sher_lock")]
        [TestCase("hierarchy/sher.lok", "hierarchy/sher.lok")]
        [TestCase("hierarchy/sher.lock/foo", "hierarchy/sher_lock/foo")]
        [TestCase("hierarchy/sher.lok/foo", "hierarchy/sher.lok/foo")]
        public void Normalise_rule01(string input, string expected)
        {
            _gitBranchNameNormaliser.Rule01(input, _gitBranchNameOptions).Should().Be(expected);
        }

        // They cannot have two consecutive dots .. anywhere.
        [TestCase("..test", "_test")]
        [TestCase("...test", "_test")]
        [TestCase("hierarchy/..test", "hierarchy/_test")]
        [TestCase("hierarchy/...test", "hierarchy/_test")]
        [TestCase("hierarchy/..test/foo", "hierarchy/_test/foo")]
        [TestCase("hierarchy/...test/foo", "hierarchy/_test/foo")]
        [TestCase("..lock", "_lock")]
        [TestCase("pad..lock", "pad_lock")]
        [TestCase("pad...lock", "pad_lock")]
        [TestCase("hierarchy/sher..lock", "hierarchy/sher_lock")]
        [TestCase("padlock...", "padlock_")]
        [TestCase("hierarchy/sher...lock", "hierarchy/sher_lock")]
        [TestCase("hierarchy/sher..lock/foo", "hierarchy/sher_lock/foo")]
        [TestCase("hierarchy/sher...lock/foo", "hierarchy/sher_lock/foo")]
        [TestCase("hierarchy/sher..lok/foo", "hierarchy/sher_lok/foo")]
        [TestCase("hier.....archy/sher...lok/fo..o", "hier_archy/sher_lok/fo_o")]
        public void Normalise_rule03(string input, string expected)
        {
            _gitBranchNameNormaliser.Rule03(input, _gitBranchNameOptions).Should().Be(expected);
        }

        // Branch name cannot have ASCII control characters (i.e. bytes whose values are lower than \040, or \127 'DEL'),
        // space, tilde '~', caret '^', or colon ':' anywhere.
        [TestCase("test:test", "test_test")]
        [TestCase("test test", "test_test")]
        [TestCase("test^test", "test_test")]
        [TestCase("test~test", "test_test")]
        [TestCase("hier archy:sher~lok/fo^o", "hier_archy_sher_lok/fo_o")]
        [TestCase("błąd", "błąd")]
        [TestCase("привет, ё-маё!", "привет,_ё-маё!")]
        [TestCase("Pokémon 195", "Pokémon_195")]
        [TestCase("Anhörung`!@#$%", "Anhörung`!@#$%")]
        public void Normalise_rule04(string input, string expected)
        {
            _gitBranchNameNormaliser.Rule04(input, _gitBranchNameOptions).Should().Be(expected);
        }

        // Branch name cannot have question-mark '?', asterisk '*', or open bracket '[' anywhere.
        [TestCase("test?", "test_")]
        [TestCase("?test", "_test")]
        [TestCase("test???test", "test___test")]
        [TestCase("test*", "test_")]
        [TestCase("*test", "_test")]
        [TestCase("test***test", "test___test")]
        [TestCase("test[foo]", "test_foo]")]
        [TestCase("[test]", "_test]")]
        [TestCase("testing?[*]*test", "testing___]_test")]
        public void Normalise_rule05(string input, string expected)
        {
            _gitBranchNameNormaliser.Rule05(input, _gitBranchNameOptions).Should().Be(expected);
        }

        // Branch name begin or end with a slash '/' or contain multiple consecutive slashes.
        [TestCase("test/", "test")]
        [TestCase("/test", "test")]
        [TestCase("/test/", "test")]
        [TestCase("/test/test/", "test/test")]
        [TestCase("///test///test///", "test/test")]
        public void Normalise_rule06(string input, string expected)
        {
            _gitBranchNameNormaliser.Rule06(input).Should().Be(expected);
        }

        // Branch name end with a dot '.'.
        [TestCase("test.", "test_")]
        [TestCase("test..", "test_")]
        [TestCase("test...", "test_")]
        public void Normalise_rule07(string input, string expected)
        {
            _gitBranchNameNormaliser.Rule07(input, _gitBranchNameOptions).Should().Be(expected);
        }

        // Branch name cannot contain a sequence '@{'.
        [TestCase("@{", "_")]
        [TestCase("@{test}", "_test}")]
        [TestCase("test@foo", "test@foo")]
        [TestCase("test@{bla}", "test_bla}")]
        public void Normalise_rule08(string input, string expected)
        {
            _gitBranchNameNormaliser.Rule08(input, _gitBranchNameOptions).Should().Be(expected);
        }

        // Branch name cannot contain a '\'.
        [TestCase(@"test\foo\\bar\", "test_foo_bar_")]
        public void Normalise_rule09(string input, string expected)
        {
            _gitBranchNameNormaliser.Rule09(input, _gitBranchNameOptions).Should().Be(expected);
        }
    }
}
