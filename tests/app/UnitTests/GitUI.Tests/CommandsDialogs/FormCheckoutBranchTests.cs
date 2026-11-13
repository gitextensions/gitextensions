using GitUI.CommandsDialogs;

namespace GitUITests.CommandsDialogs;

public sealed class FormCheckoutBranchTests
{
    [TestCase("", new[] { "main" }, "", TestName = "Empty input returns empty name")]
    [TestCase("   ", new[] { "main" }, "", TestName = "Whitespace input is treated as empty")]
    [TestCase("main", new[] { "main", "dev" }, "main", TestName = "Name is kept when it exists in the new list")]
    [TestCase("origin/feature", new[] { "main", "feature" }, "feature", TestName = "Remote prefix is stripped when the local branch exists")]
    [TestCase("origin/feature/login", new[] { "main", "feature/login" }, "feature/login", TestName = "Remote prefix is stripped for nested branch names")]
    [TestCase("feature", new[] { "origin/feature", "upstream/dev" }, "origin/feature", TestName = "Remote branch is found for a typed local name")]
    [TestCase("feature/login", new[] { "origin/feature/login" }, "origin/feature/login", TestName = "Remote branch is found for a typed nested local name")]
    [TestCase("feature", new[] { "upstream/feature", "origin/feature" }, "upstream/feature", TestName = "First matching remote wins")]
    [TestCase("demo/2026-11-11", new[] { "main", "dev" }, "demo/2026-11-11", TestName = "Unknown name is kept as free text")]
    [TestCase("origin/demo/2026-11-11", new[] { "main", "dev" }, "origin/demo/2026-11-11", TestName = "Unknown remote name is kept as free text")]
    [TestCase("feature", new[] { "origin/feature2" }, "feature", TestName = "Partial suffix does not match")]
    [TestCase("feature", new string[0], "feature", TestName = "Name is kept when the new list is empty")]
    [TestCase("Feature", new[] { "origin/feature" }, "Feature", TestName = "Matching is case sensitive")]
    [TestCase("origin/feature/login", new[] { "login" }, "origin/feature/login", TestName = "Unrelated shorter local branch is not matched")]
    [TestCase("login", new[] { "origin/feature/login" }, "login", TestName = "Unrelated deeper remote branch is not matched")]
    [TestCase("origin/a/b", new[] { "b", "a/b" }, "a/b", TestName = "Strip removes exactly one segment and prefers the full remainder")]
    [TestCase("feature/login/x", new[] { "origin/feature/login/x" }, "origin/feature/login/x", TestName = "Remote prefix is added for deeply nested branch names")]
    public void TranslateBranchName(string previousBranchName, string[] branchNamesInNewMode, string expectedResult)
    {
        FormCheckoutBranch.TranslateBranchName(previousBranchName, branchNamesInNewMode).Should().Be(expectedResult);
    }
}
