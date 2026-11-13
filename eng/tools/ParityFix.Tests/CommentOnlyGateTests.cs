using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityFix.Tests;

// parity-scaffolding: Exercises the Windows gate's real proof boundary without running product builds in a tool test.
[TestFixture]
[Platform("Win", Reason = "The verification gate is a Windows PowerShell entry point.")]
public sealed class CommentOnlyGateTests
{
    [Test]
    public async Task CommentOnly_should_select_proof_build_and_inventory_without_any_test_or_linux_leg_Async()
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        File.AppendAllText(Path.Combine(fixture.Root, "Widget.cs"), "\n// Restored rationale.");

        (int exitCode, string output, string error) = await RunAsync("-File", Path.Combine(fixture.Root, "eng", "avalonia", "gate.ps1"), "-CommentOnly", "-PlanOnly");

        exitCode.Should().Be(0, error);
        string[] running = output.Split('\n').Select(line => Regex.Match(line, @"^  (\S+)\s+run\s*$"))
            .Where(match => match.Success).Select(match => match.Groups[1].Value).ToArray();
        running.Should().Equal("comment-proof", "winforms", "avalonia-build", "inventory");
        foreach (string leg in new[] { "focused", "construction", "tool-tests", "inventory-tests", "linux", "linux-tool-tests" })
        {
            output.Should().MatchRegex($@"(?m)^  {leg}\s+skip\s+\(comment-only:");
        }
    }

    [TestCase("-Only", "inventory")]
    [TestCase("-Level", "Edit")]
    [TestCase("-Level", "Exit")]
    [TestCase("-Filter", "FullyQualifiedName~Example")]
    [TestCase("-ToolTestProject", "eng/tools/Example.Tests/Example.Tests.csproj")]
    public async Task CommentOnly_should_reject_options_that_bypass_its_required_legs_Async(string option, string value)
    {
        (int exitCode, string output, string error) = await RunAsync("-File", FindGate(), "-CommentOnly", "-PlanOnly", option, value);
        exitCode.Should().Be(1);
        error.Should().Contain("-CommentOnly requires Batch");
        output.Should().BeEmpty();
    }

    [TestCase("// Restored rationale.\nclass Widget { int Value() => 1; }", true)]
    [TestCase("class Widget { int Value() => 2; }", false)]
    [TestCase("#nullable disable\nclass Widget { int Value() => 1; }", false)]
    public async Task CommentOnly_should_require_the_real_repository_proof_before_build_and_inventory_Async(string working, bool expected)
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        File.WriteAllText(Path.Combine(fixture.Root, "Widget.cs"), working);
        string harness = Path.Combine(fixture.Root, "harness.ps1");
        File.WriteAllText(harness, """
            param($Gate, $ProofDll, $Root)
            $ErrorActionPreference = 'Stop'
            $ast = [Management.Automation.Language.Parser]::ParseFile($Gate, [ref]$null, [ref]$null)
            $functions = $ast.FindAll({ param($node)
                $node -is [Management.Automation.Language.FunctionDefinitionAst] -and
                    $node.Name -in 'Invoke-Dotnet', 'Assert-CommentOnlyProof', 'Get-CommentOnlyLegs'
            }, $true)
            foreach ($function in $functions) { Invoke-Expression $function.Extent.Text }
            Set-Location -LiteralPath $Root
            Assert-CommentOnlyProof (Join-Path $Root 'proof.log') (Join-Path $Root 'proof.json')
            @(Get-CommentOnlyLegs) | ConvertTo-Json
            """);

        (int exitCode, string output, string error) = await RunAsync("-File", harness, FindGate(), typeof(WorktreeProof).Assembly.Location, fixture.Root);

        exitCode.Should().Be(expected ? 0 : 1, error);
        using JsonDocument proof = JsonDocument.Parse(File.ReadAllText(Path.Combine(fixture.Root, "proof.json")));
        proof.RootElement.GetProperty("Passed").GetBoolean().Should().Be(expected);
        if (expected)
        {
            using JsonDocument plan = JsonDocument.Parse(output);
            JsonElement[] legs = plan.RootElement.EnumerateArray().ToArray();
            legs[0].GetProperty("Name").GetString().Should().Be("comment-proof");
            legs.Where(leg => !leg.TryGetProperty("Skip", out JsonElement skip) || skip.ValueKind == JsonValueKind.Null)
                .Select(leg => leg.GetProperty("Name").GetString()).Should().Equal("comment-proof", "winforms", "avalonia-build", "inventory");
            legs.Where(leg => leg.TryGetProperty("Skip", out JsonElement skip) && skip.ValueKind == JsonValueKind.String)
                .Select(leg => leg.GetProperty("Name").GetString()).Should().BeEquivalentTo(
                    "focused", "construction", "tool-tests", "inventory-tests", "linux", "linux-tool-tests");
        }
        else
        {
            output.Should().BeEmpty("the proof must refuse before any following leg is selected");
            error.Should().Contain("comment-only proof failed");
        }
    }

    [TestCase(false, true, true)]
    [TestCase(true, true, false)]
    [TestCase(false, false, false)]
    public async Task Verify_should_accept_comment_only_receipts_only_with_proof_for_the_exact_tree_Async(bool changeTree, bool proved, bool expected)
    {
        using Repository fixture = new();
        await fixture.InitializeAsync();
        string tree = (await WorktreeProof.GitAsync(fixture.Root, "write-tree")).Trim();
        string directory = Path.Combine(fixture.Root, "eng", "avalonia", "parity-evidence", "gates", "test");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "receipt.json"), JsonSerializer.Serialize(new
        {
            id = "test", status = "passed", level = "Batch", partial = false, commentOnly = true, tree,
            legs = proved ? new[] { new { name = "comment-proof", status = "passed" } } : Array.Empty<object>()
        }));
        if (changeTree)
        {
            File.AppendAllText(Path.Combine(fixture.Root, "Widget.cs"), "\n// Another comment.");
        }

        (int exitCode, string output, string error) = await RunAsync("-File", Path.Combine(fixture.Root, "eng", "avalonia", "gate.ps1"), "-Verify");

        exitCode.Should().Be(expected ? 0 : 1, error);
        output.Should().Contain(expected ? "verified:" : "NOT VERIFIED:");
    }

    private static async Task<(int ExitCode, string Output, string Error)> RunAsync(params string[] arguments)
    {
        ProcessStartInfo start = new("pwsh") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("-NoProfile");
        foreach (string argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(start) ?? throw new InvalidOperationException("PowerShell did not start.");
        Task<string> output = process.StandardOutput.ReadToEndAsync();
        Task<string> error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, await output, await error);
    }

    private static string FindGate()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null)
        {
            string path = Path.Combine(directory.FullName, "eng", "avalonia", "gate.ps1");
            if (File.Exists(path))
            {
                return path;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find the gate script.");
    }

    private sealed class Repository : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "CommentGate-" + Guid.NewGuid().ToString("N"));

        public async Task InitializeAsync()
        {
            string gateDirectory = Path.Combine(Root, "eng", "avalonia");
            Directory.CreateDirectory(gateDirectory);
            File.Copy(FindGate(), Path.Combine(gateDirectory, "gate.ps1"));
            await WorktreeProof.GitAsync(Root, "init", "-q");
            await WorktreeProof.GitAsync(Root, "config", "user.name", "nikola.begovic");
            await WorktreeProof.GitAsync(Root, "config", "user.email", "begota98@gmail.com");
            File.WriteAllText(Path.Combine(Root, ".git", "info", "exclude"), "harness.ps1\nproof.*\neng/avalonia/parity-evidence/\n");
            File.WriteAllText(Path.Combine(Root, "Widget.cs"), "class Widget { int Value() => 1; }");
            await WorktreeProof.GitAsync(Root, "add", ".");
            await WorktreeProof.GitAsync(Root, "commit", "-qm", "test: establish comment gate fixture");
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
