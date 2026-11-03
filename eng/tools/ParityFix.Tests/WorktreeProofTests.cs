using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityFix.Tests;

// parity-scaffolding: Uses throwaway repositories to prove the entire staged and unstaged twin delta.
[TestFixture]
public sealed class WorktreeProofTests
{
    [TestCase("// Reason.\nclass Widget { int Value() => 1; }", true)]
    [TestCase("class Widget { int Value() => 2; }", false)]
    [TestCase("#nullable disable\nclass Widget { int Value() => 1; }", false)]
    [TestCase("class Widget { int Value() => ", false)]
    public void AreCommentOnlyEquivalent_should_reject_code_directive_and_parse_changes(string twin, bool expected)
    {
        WorktreeProof.AreCommentOnlyEquivalent("class Widget { int Value() => 1; }", twin).Should().Be(expected);
    }

    [Test]
    public void AreCommentOnlyEquivalent_should_normalize_git_line_endings_without_changing_directives()
    {
        const string original = "#nullable enable\nclass Widget { int Value() => 1; }\n";
        WorktreeProof.AreCommentOnlyEquivalent(original, "// Rationale.\r\n" + original.Replace("\n", "\r\n", StringComparison.Ordinal)).Should().BeTrue();
    }

    [Test]
    public async Task ProveAsync_should_include_staged_and_unstaged_edits_and_ignore_files_outside_the_twin_Async()
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        File.WriteAllText(fixture.FilePath, "// Staged.\nclass Widget { int Value() => 1; }");
        await WorktreeProof.GitAsync(fixture.Root, "add", ".");
        File.AppendAllText(fixture.FilePath, "\n// Unstaged.");
        File.WriteAllText(Path.Combine(fixture.Root, "outside.cs"), "This is not C#.");

        ProofReport proof = await WorktreeProof.ProveAsync(fixture.Root, "twin");

        proof.Passed.Should().BeTrue();
        proof.Files.Should().ContainSingle().Which.Path.Should().Be("twin/Widget space.cs");
        proof.Head.Should().Be((await WorktreeProof.GitAsync(fixture.Root, "rev-parse", "HEAD")).Trim());
    }

    [TestCase("code")]
    [TestCase("directive")]
    [TestCase("delete")]
    [TestCase("add")]
    [TestCase("markup")]
    [TestCase("order")]
    public async Task ProveAsync_should_fail_for_every_unproved_twin_change_Async(string change)
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        switch (change)
        {
            case "code":
                File.WriteAllText(fixture.FilePath, "class Widget { int Value() => 2; }");
                break;
            case "directive":
                File.WriteAllText(fixture.FilePath, "#nullable disable\nclass Widget { int Value() => 1; }");
                break;
            case "delete":
                File.Delete(fixture.FilePath);
                break;
            case "add":
                File.WriteAllText(Path.Combine(fixture.Root, "twin", "New.cs"), "class New { }");
                break;
            case "markup":
                File.WriteAllText(Path.Combine(fixture.Root, "twin", "New.axaml"), "<Window />");
                break;
            case "order":
                File.WriteAllText(fixture.FilePath, "class Widget { void Extra() { } int Value() => 1; }");
                await WorktreeProof.GitAsync(fixture.Root, "add", ".");
                await WorktreeProof.GitAsync(fixture.Root, "commit", "-qm", "test: establish member order");
                File.WriteAllText(fixture.FilePath, "class Widget { int Value() => 1; void Extra() { } }");
                break;
        }

        ProofReport proof = await WorktreeProof.ProveAsync(fixture.Root, "twin");

        proof.Passed.Should().BeFalse();
        proof.Files.Should().ContainSingle(file => !file.Passed);
    }

    [Test]
    public async Task ProveAsync_should_report_zero_changed_files_explicitly_Async()
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        ProofReport proof = await WorktreeProof.ProveAsync(fixture.Root, "twin");
        proof.Files.Should().BeEmpty();
        proof.Passed.Should().BeTrue();
    }

    private sealed class Repository : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "ParityProof-" + Guid.NewGuid().ToString("N"));

        public string FilePath => Path.Combine(Root, "twin", "Widget space.cs");

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(Path.Combine(Root, "twin"));
            await WorktreeProof.GitAsync(Root, "init", "-q");
            await WorktreeProof.GitAsync(Root, "config", "user.name", "nikola.begovic");
            await WorktreeProof.GitAsync(Root, "config", "user.email", "begota98@gmail.com");
            File.WriteAllText(FilePath, "class Widget { int Value() => 1; }");
            await WorktreeProof.GitAsync(Root, "add", ".");
            await WorktreeProof.GitAsync(Root, "commit", "-qm", "test: establish comment proof fixture");
        }

        public void Dispose()
        {
            foreach (string file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
            }

            Directory.Delete(Root, recursive: true);
        }
    }
}
