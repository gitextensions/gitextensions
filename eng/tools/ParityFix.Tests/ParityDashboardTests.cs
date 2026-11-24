using System.Diagnostics;
using System.Text.Json;
using AwesomeAssertions;
using NUnit.Framework;

namespace GitExtensions.ParityFix.Tests;

// parity-scaffolding: Exercises the real Windows gate dashboard with isolated inventory, ledger and history files.
[TestFixture]
[Platform("Win", Reason = "The dashboard wrapper belongs to the Windows PowerShell verification gate.")]
public sealed class ParityDashboardTests
{
    [TestCase("previous", "change since aaaaaaaaa: -5", 3)]
    [TestCase("intervening", "change since ccccccccc: -2", 4)]
    [TestCase("same-only", "", 2)]
    [TestCase("new", "change since bbbbbbbbb: 0", 3)]
    [TestCase("ledger", "change since aaaaaaaaa: -5", 4)]
    [TestCase("accepted", "change since aaaaaaaaa: -5", 4)]
    public async Task Dashboard_should_compare_the_last_different_commit_and_deduplicate_history_Async(string scenario, string expected, int expectedRows)
    {
        string root = Path.Combine(Path.GetTempPath(), "ParityDashboard-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "types"));
        try
        {
            string a = new('a', 40);
            string b = new('b', 40);
            string c = new('c', 40);
            object[] rows = scenario switch
            {
                "intervening" => [new { commit = a, actionable = 100 }, new { commit = b, actionable = 95 }, new { commit = c, actionable = 97 }, new { commit = b, actionable = 95 }],
                "same-only" => [new { commit = b, actionable = 95 }, new { commit = b, actionable = 95 }],
                "new" => [new { commit = a, actionable = 100 }, new { commit = b, actionable = 95 }],
                _ => [new { commit = a, actionable = 100 }, new { commit = b, actionable = 95 }, new { commit = b, actionable = 95 }]
            };
            File.WriteAllLines(Path.Combine(root, "history.jsonl"), rows.Select((row, index) =>
            {
                JsonElement value = JsonSerializer.SerializeToElement(row);
                return JsonSerializer.Serialize(new
                {
                    commit = value.GetProperty("commit").GetString(),
                    actionable = value.GetProperty("actionable").GetInt32(),
                    productActionable = value.GetProperty("actionable").GetInt32() - 20,
                    testOnlyActionable = 20,
                    mechanical = value.GetProperty("actionable").GetInt32(),
                    behavior = 0,
                    structural = 0,
                    dependent = 0,
                    acceptedComments = 0,
                    acceptedAdaptedMinimal = 0,
                    complete = 0,
                    allButMacOs = scenario == "ledger" && index == rows.Length - 1 ? 1 : 0
                });
            }));
            File.WriteAllText(Path.Combine(root, "inventory.json"), JsonSerializer.Serialize(new
            {
                analyzedCommit = scenario == "new" ? c : b,
                summary = new { findingCount = 95, productFindingCount = 75, testOnlyFindingCount = 20, mappingCount = 1, analyzedTypeCount = 1 }
            }));
            File.WriteAllText(Path.Combine(root, "types", "Fixture.json"), JsonSerializer.Serialize(new
            {
                typeName = "Fixture",
                dependentFindings = Array.Empty<object>(),
                acceptedFrameworkDeviations = scenario == "accepted"
                    ? new[]
                    {
                        new { category = "comments", reason = "codeAbsent" },
                        new { category = "comments", reason = "codeMoved" },
                        new { category = "comments", reason = "codeMoved" },
                        new { category = "comments", reason = "adaptedMinimal" },
                        new { category = "comments", reason = "generatedDesigner" },
                        new { category = "members", reason = "codeAbsent" }
                    }
                    : Array.Empty<object>(),
                findings = Enumerable.Range(0, 95).Select(index => new { code = "comment.missing", path = index.ToString(), isTestOnly = index < 20 })
            }));
            File.WriteAllText(Path.Combine(root, "ownership.json"), JsonSerializer.Serialize(new
            {
                types = new[] { new { typeName = "Fixture", owner = "progress132" } }
            }));
            File.WriteAllText(Path.Combine(root, "ledger.json"), "{\"axisOrder\":[\"structural\"],\"components\":[]}");

            for (int attempt = 0; attempt < 2; attempt++)
            {
                ProcessStartInfo start = new("pwsh") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
                string script = FindScript();
                foreach (string argument in new[]
                         {
                             "-NoProfile", "-File", script, "-Inventory", Path.Combine(root, "inventory.json"),
                             "-Ownership", Path.Combine(root, "ownership.json"), "-LedgerPath", Path.Combine(root, "ledger.json"),
                             "-History", Path.Combine(root, "history.jsonl"), "-Output", Path.Combine(root, "dashboard.md")
                         })
                {
                    start.ArgumentList.Add(argument);
                }

                using Process process = Process.Start(start) ?? throw new InvalidOperationException("PowerShell did not start.");
                Task<string> output = process.StandardOutput.ReadToEndAsync();
                Task<string> error = process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();
                string diagnostics = await error;
                await output;
                process.ExitCode.Should().Be(0, diagnostics);
                string dashboard = await File.ReadAllTextAsync(Path.Combine(root, "dashboard.md"));
                dashboard.Should().Contain("Components complete: 0 of 0.");
                dashboard.Should().Contain("| 132 Shared core and compat | 95 | 0 | 0 | 95 | 75 | 20 |");
                dashboard.Should().Contain("| **Total** | **95** | **0** | **0** | **95** | **75** | **20** |");
                dashboard.Should().Contain(scenario == "accepted"
                    ? "Accepted handwritten comment findings: 4** (codeAbsent 1, codeMoved 2, adaptedMinimal 1); accepted deviations, not closed."
                    : "Accepted handwritten comment findings: 0**");
                if (expected.Length == 0)
                {
                    dashboard.Should().NotContain("change since");
                }
                else
                {
                    dashboard.Should().Contain(expected);
                }

                File.ReadAllLines(Path.Combine(root, "history.jsonl")).Should().HaveCount(expectedRows);
                using JsonDocument last = JsonDocument.Parse(File.ReadAllLines(Path.Combine(root, "history.jsonl"))[^1]);
                last.RootElement.GetProperty("productActionable").GetInt32().Should().Be(75);
                last.RootElement.GetProperty("testOnlyActionable").GetInt32().Should().Be(20);
                last.RootElement.GetProperty("acceptedComments").GetInt32().Should().Be(scenario == "accepted" ? 4 : 0);
                last.RootElement.GetProperty("acceptedAdaptedMinimal").GetInt32().Should().Be(scenario == "accepted" ? 1 : 0);
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static string FindScript()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null)
        {
            string path = Path.Combine(directory.FullName, "eng", "avalonia", "parity-dashboard.ps1");
            if (File.Exists(path))
            {
                return path;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find the dashboard script.");
    }
}
