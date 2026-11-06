using CommonTestUtils;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;

namespace GitCommandsTests.Git;

public sealed class SessionRefsTests
{
    private const string _refsFilters = "--exclude=refs/agents/** --exclude=refs/*/checkpoints/** --exclude=refs/copilot/**";

    [TestCase("", new string[0])]
    [TestCase("--all --exclude=refs/agents/**  --exclude=\"refs/*/checkpoints/**\" --exclude=", new[] { "refs/agents/**", "refs/*/checkpoints/**" })]
    public void GetPatterns_should_return_the_patterns_of_the_exclude_options(string refsFilters, string[] expected)
    {
        SessionRefs.GetPatterns(refsFilters).Should().Equal(expected);
    }

    [Test]
    public void GetRefs_should_not_list_any_ref_if_there_is_no_pattern()
    {
        using ReferenceRepository repository = new();

        SessionRefs.GetRefs(repository.Module.GitExecutable, refsFilters: "").Should().BeEmpty();
    }

    [Test]
    public void GetDeleteRefArgs_should_delete_the_full_refname()
    {
        SessionRefs.GetDeleteRefArgs("refs/agents/a/1").ToString().Should().Be("update-ref -d \"refs/agents/a/1\"");
    }

    [Test]
    public void GetDeleteRefArgs_should_delete_only_the_refs_matching_the_filters()
    {
        using ReferenceRepository repository = new();
        IExecutable git = repository.Module.GitExecutable;
        string[] sessionRefs = ["refs/agents/a/1", "refs/copilot/b", "refs/claude/checkpoints/c/2"];
        foreach (string gitRef in sessionRefs.Append("refs/keep/agents/d"))
        {
            git.RunCommand($"update-ref {gitRef} {repository.CommitHash}").Should().BeTrue();
        }

        IReadOnlyList<string> found = SessionRefs.GetRefs(git, _refsFilters);
        found.Should().BeEquivalentTo(sessionRefs);

        foreach (string gitRef in found)
        {
            git.Execute(SessionRefs.GetDeleteRefArgs(gitRef)).ExitedSuccessfully.Should().BeTrue();
        }

        SessionRefs.GetRefs(git, _refsFilters).Should().BeEmpty();
        git.GetOutput("for-each-ref --format=%(refname)").Should().Contain("refs/keep/agents/d").And.Contain("refs/heads/");
    }
}
