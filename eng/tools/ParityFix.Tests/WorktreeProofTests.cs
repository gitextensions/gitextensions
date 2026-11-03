using System.Text.Json.Nodes;
using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityFix.Tests;

// parity-scaffolding: Uses throwaway repositories to prove the entire staged and unstaged repository delta.
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
    public void AreCommentOnlyEquivalent_should_reject_changes_to_disabled_code()
    {
        const string original = "class Widget {\n#if DEBUG\nint Value() => 1;\n#endif\n}";
        WorktreeProof.AreCommentOnlyEquivalent(original, original.Replace("=> 1", "=> 2", StringComparison.Ordinal)).Should().BeFalse();
    }

    [TestCase("1", "2", false)]
    [TestCase("// Reason.", "// Changed reason.", false)]
    [TestCase("1", "1", true)]
    public void AreCommentOnlyEquivalent_should_compare_nested_disabled_text(string before, string after, bool expected)
    {
        const string code = "#if RELEASE\n#if DEBUG\nVALUE\n#endif\n#endif\nclass Widget { }\n";
        WorktreeProof.AreCommentOnlyEquivalent(code.Replace("VALUE", before, StringComparison.Ordinal),
            ("// Active comment.\n" + code.Replace("VALUE", after, StringComparison.Ordinal)).Replace("\n", "\r\n", StringComparison.Ordinal))
            .Should().Be(expected);
    }

    [Test]
    public void AreCommentOnlyEquivalent_should_normalize_git_line_endings_without_changing_directives()
    {
        const string original = "#nullable enable\nclass Widget { int Value() => 1; }\n";
        WorktreeProof.AreCommentOnlyEquivalent(original, "// Rationale.\r\n" + original.Replace("\n", "\r\n", StringComparison.Ordinal)).Should().BeTrue();
    }

    [Test]
    public async Task ProveAsync_should_include_staged_and_unstaged_edits_across_the_repository_Async()
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        File.WriteAllText(fixture.FilePath, "// Staged.\nclass Widget { int Value() => 1; }");
        await WorktreeProof.GitAsync(fixture.Root, "add", ".");
        File.AppendAllText(fixture.FilePath, "\n// Unstaged.");
        File.AppendAllText(Path.Combine(fixture.Root, "outside.cs"), "\n// Outside the twin.");

        ProofReport proof = await WorktreeProof.ProveAsync(Path.Combine(fixture.Root, "twin"));

        proof.Passed.Should().BeTrue();
        proof.Files.Select(file => file.Path).Should().Equal("outside.cs", "twin/Widget space.cs");
        proof.Head.Should().Be((await WorktreeProof.GitAsync(fixture.Root, "rev-parse", "HEAD")).Trim());
    }

    [TestCase("code")]
    [TestCase("directive")]
    [TestCase("delete")]
    [TestCase("add")]
    [TestCase("markup")]
    [TestCase("order")]
    [TestCase("outside-code")]
    [TestCase("outside-json")]
    [TestCase("mode")]
    [TestCase("adaptations")]
    public async Task ProveAsync_should_fail_for_every_unproved_repository_change_Async(string change)
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
            case "outside-code":
                File.WriteAllText(Path.Combine(fixture.Root, "outside.cs"), "class Outside { int Value() => 2; }");
                break;
            case "outside-json":
                File.WriteAllText(Path.Combine(fixture.Root, "portmap.json"), "{}");
                break;
            case "adaptations":
                Directory.CreateDirectory(Path.Combine(fixture.Root, "eng", "avalonia"));
                File.WriteAllText(Path.Combine(fixture.Root, "eng", "avalonia", "framework-adaptations.json"), "{}");
                break;
            case "mode":
                await WorktreeProof.GitAsync(fixture.Root, "update-index", "--chmod=+x", "twin/Widget space.cs");
                break;
            case "order":
                File.WriteAllText(fixture.FilePath, "class Widget { void Extra() { } int Value() => 1; }");
                await WorktreeProof.GitAsync(fixture.Root, "add", ".");
                await WorktreeProof.GitAsync(fixture.Root, "commit", "-qm", "test: establish member order");
                File.WriteAllText(fixture.FilePath, "class Widget { int Value() => 1; void Extra() { } }");
                break;
        }

        ProofReport proof = await WorktreeProof.ProveAsync(fixture.Root);

        proof.Passed.Should().BeFalse();
        proof.Files.Should().ContainSingle(file => !file.Passed);
    }

    [TestCase("missing", true, null)]
    [TestCase("moved", true, null)]
    [TestCase("format", true, null)]
    [TestCase("member", false, "not an absent or moved comment pin")]
    [TestCase("event", false, "not an absent or moved comment pin")]
    [TestCase("edited", false, "Edited existing framework adaptation 'Widget/member.extra/member/field:value'")]
    [TestCase("removed", false, "Removed existing framework adaptation 'Widget/member.extra/member/field:value'")]
    [TestCase("malformed", false, "Invalid framework-adaptation manifest")]
    [TestCase("schema", false, "property 'schemaVersion' changed")]
    [TestCase("root", false, "property 'unreviewed' was added")]
    [TestCase("duplicate", false, "duplicate framework-adaptation identity")]
    [TestCase("duplicateProperty", false, "Duplicate JSON property 'schemaVersion'")]
    [TestCase("incomplete", false, "must pin an exact handwritten comment")]
    public async Task ProveAsync_should_prove_only_additive_exact_comment_pins_Async(string change, bool expected, string? diagnostic)
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        string directory = Path.Combine(fixture.Root, "eng", "avalonia");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "framework-adaptations.json");
        const string baseline = """
            { "schemaVersion": 4, "deviations": [
              { "typeName": "Widget", "code": "member.extra", "path": "member/field:value",
                "twinPart": "Widget.cs", "twinAccessibility": "private", "twinSignature": "int value", "rationale": "Framework state." }
            ] }
            """;
        File.WriteAllText(path, baseline);
        await WorktreeProof.GitAsync(fixture.Root, "add", ".");
        await WorktreeProof.GitAsync(fixture.Root, "commit", "-qm", "test: establish adaptation manifest");
        JsonObject root = JsonNode.Parse(baseline)!.AsObject();
        JsonArray entries = root["deviations"]!.AsArray();
        JsonObject pin = new()
        {
            ["typeName"] = "Widget", ["category"] = "comments", ["code"] = "comment.missing",
            ["path"] = "comment/Widget.cs/method:Before()/body/0", ["originalPart"] = "Widget.cs",
            ["originalValue"] = "Reason.", ["reason"] = "codeAbsent", ["rationale"] = "Reviewed the whole type."
        };
        entries.Add(pin);
        switch (change)
        {
            case "moved":
                pin["code"] = "comment.drifted";
                pin["reason"] = "codeMoved";
                pin["twinValue"] = "Reason.";
                pin["twinPart"] = "Widget.cs";
                pin["targetMember"] = "method:After()";
                break;
            case "member":
                pin["code"] = "member.missing";
                pin["path"] = "member/method:InitializeComponent";
                break;
            case "event": pin["code"] = "event.wiring.missing"; break;
            case "edited": entries[0]!["rationale"] = "Changed state."; break;
            case "removed": entries.RemoveAt(0); break;
            case "schema": root["schemaVersion"] = 5; break;
            case "root": root["unreviewed"] = true; break;
            case "duplicate": entries.Add(pin.DeepClone()); break;
            case "incomplete": pin.Remove("originalPart"); break;
            case "format": entries.Clear(); entries.Add(JsonNode.Parse(baseline)!["deviations"]![0]!.DeepClone()); break;
        }

        string working = root.ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        if (change == "malformed")
        {
            working = "{ broken";
        }
        else if (change == "duplicateProperty")
        {
            working = working.Replace("\"schemaVersion\": 4", "\"schemaVersion\": 4, \"schemaVersion\": 4", StringComparison.Ordinal);
        }

        File.WriteAllText(path, working);
        File.AppendAllText(fixture.FilePath, "\n// Comment accompanying pins.");

        ProofReport proof = await WorktreeProof.ProveAsync(fixture.Root);

        proof.Passed.Should().Be(expected);
        FileProof manifest = proof.Files.Single(file => file.Path == "eng/avalonia/framework-adaptations.json");
        manifest.Passed.Should().Be(expected);
        proof.ExemptPaths.Should().BeEmpty();
        proof.Files.Single(file => file.Path == "twin/Widget space.cs").Passed.Should().BeTrue();
        if (diagnostic is not null)
        {
            manifest.Error.Should().Contain(diagnostic);
        }
    }

    [Test]
    public async Task ProveAsync_should_exempt_only_the_two_exact_metadata_paths_Async()
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        string metadata = Path.Combine(fixture.Root, "eng", "avalonia");
        Directory.CreateDirectory(metadata);
        File.WriteAllText(Path.Combine(metadata, "portmap.json"), "{}");
        File.WriteAllText(Path.Combine(metadata, "parity-ledger.json"), "{}");
        await WorktreeProof.GitAsync(fixture.Root, "add", ".");

        ProofReport proof = await WorktreeProof.ProveAsync(fixture.Root);

        proof.Passed.Should().BeTrue();
        proof.Files.Should().BeEmpty();
        proof.ExemptPaths.Should().Equal("eng/avalonia/parity-ledger.json", "eng/avalonia/portmap.json");
    }

    [Test]
    public async Task ProveAsync_should_report_zero_changed_files_explicitly_Async()
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        ProofReport proof = await WorktreeProof.ProveAsync(fixture.Root);
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
            await WorktreeProof.GitAsync(Root, "config", "core.filemode", "false");
            File.WriteAllText(FilePath, "class Widget { int Value() => 1; }");
            File.WriteAllText(Path.Combine(Root, "outside.cs"), "class Outside { int Value() => 1; }");
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
