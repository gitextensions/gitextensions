using GitExtensions.Extensibility.Git;
using GitUI;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class GitModuleExtensionsTests
{
    [TestCase("Resolved 'file' using previous resolution.\n", true, false, false, false, true)]
    [TestCase("Resolved 'file' using previous resolution.\n", false, true, false, false, true)]
    [TestCase("Resolved 'file' using previous resolution.\n", false, false, true, false, true)]
    [TestCase("Resolved 'file' using previous resolution.\n", false, false, false, false, false)]
    [TestCase("Resolved 'file' using previous resolution.\n", true, false, false, true, false)]
    [TestCase("Resolved 'file' using previous resolution.\n", false, true, false, true, false)]
    [TestCase("Resolved 'file' using previous resolution.\n", false, false, true, true, false)]
    [TestCase("Resolved 'file' using previous resolution.\nAborted\n  ", true, false, false, false, false)]
    [TestCase("Resolved 'file' using previous resolution.\nAborted\n  ", false, false, true, false, false)]
    [TestCase("CONFLICT: unresolved file", true, false, false, false, false)]
    [TestCase("", false, false, true, false, false)]
    public void CanContinueAction_should_require_rerere_a_live_operation_and_no_unresolved_or_aborted_state(
        string output, bool merge, bool patch, bool rebase, bool conflicted, bool expected)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.InTheMiddleOfMerge().Returns(merge);
        module.InTheMiddleOfPatch().Returns(patch);
        module.InTheMiddleOfRebase().Returns(rebase);
        module.InTheMiddleOfConflictedMerge().Returns(conflicted);

        module.CanContinueAction(output).Should().Be(expected);
    }
}
