using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using GitCommands;

namespace GitUI.AI;

/// <summary>
/// Classifies diffs by driving the locally installed Claude Code in headless mode
/// (<c>claude -p --output-format json</c>). This reuses Claude Code's own authentication,
/// so it works with a Claude Pro/Max subscription without a separate API key.
/// It is the same mechanism the Claude Agent SDK uses to run under a Claude plan.
/// </summary>
public sealed class ClaudeCodeDiffNoiseClassifier : IDiffNoiseClassifier
{
    private readonly string _executable;
    private readonly string _model;
    private readonly int _maxCharsPerFile;

    public ClaudeCodeDiffNoiseClassifier()
    {
        string configured = AppSettings.AiFilterClaudeCodeExecutable.Value;
        _executable = string.IsNullOrWhiteSpace(configured) ? "claude" : configured.Trim();
        _model = AppSettings.AiFilterModel.Value;
        _maxCharsPerFile = Math.Max(1000, AppSettings.AiFilterMaxDiffCharsPerFile.Value);
    }

    public async Task<IReadOnlyList<DiffNoiseClassification>> ClassifyAsync(
        IReadOnlyList<DiffFileContent> files,
        DiffNoiseFilterOptions options,
        CancellationToken cancellationToken)
    {
        if (!options.AnyEnabled || files.Count == 0)
        {
            return [];
        }

        List<DiffNoiseClassification> results = [];
        foreach (List<DiffFileContent> batch in DiffNoisePrompt.CreateBatches(files, _maxCharsPerFile))
        {
            cancellationToken.ThrowIfCancellationRequested();
            string userPrompt = DiffNoisePrompt.BuildUserPrompt(batch, options, _maxCharsPerFile);
            string result = await RunClaudeAsync(userPrompt, cancellationToken);
            results.AddRange(DiffNoisePrompt.ParseClassifications(result, batch));
        }

        return results;
    }

    private async Task<string> RunClaudeAsync(string userPrompt, CancellationToken cancellationToken)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = _executable,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardInputEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,

            // Run in a neutral directory so Claude Code has no repository files to act on.
            WorkingDirectory = GetNeutralWorkingDirectory()
        };
        startInfo.ArgumentList.Add("-p");
        startInfo.ArgumentList.Add("--output-format");
        startInfo.ArgumentList.Add("json");
        startInfo.ArgumentList.Add("--no-session-persistence");
        startInfo.ArgumentList.Add("--model");
        startInfo.ArgumentList.Add(_model);
        startInfo.ArgumentList.Add("--system-prompt");
        startInfo.ArgumentList.Add(DiffNoisePrompt.SystemPrompt);

        using Process process = new() { StartInfo = startInfo };

        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            throw new DiffNoiseClassifierException(
                $"Could not launch Claude Code ('{_executable}'). Ensure Claude Code is installed and logged in " +
                $"(run 'claude' once and '/login'), or set the executable path in Settings > Diff AI filter. Details: {ex.Message}",
                ex);
        }

        try
        {
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.StandardInput.WriteAsync(userPrompt.AsMemory(), cancellationToken);
            process.StandardInput.Close();

            await process.WaitForExitAsync(cancellationToken);

            string stdout = await stdoutTask;
            string stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                string message = !string.IsNullOrWhiteSpace(stderr) ? stderr : stdout;
                throw new DiffNoiseClassifierException($"Claude Code exited with code {process.ExitCode}: {Truncate(message, 500)}");
            }

            return ExtractResult(stdout);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }
    }

    private static string ExtractResult(string stdout)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(stdout);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("is_error", out JsonElement isError) && isError.ValueKind == JsonValueKind.True)
            {
                string? errorText = root.TryGetProperty("result", out JsonElement err) ? err.GetString() : null;
                throw new DiffNoiseClassifierException($"Claude Code reported an error: {errorText ?? "unknown error"}");
            }

            if (root.TryGetProperty("result", out JsonElement result) && result.ValueKind == JsonValueKind.String)
            {
                return result.GetString() ?? "";
            }
        }
        catch (JsonException ex)
        {
            throw new DiffNoiseClassifierException($"Could not parse Claude Code output: {ex.Message}", ex);
        }

        throw new DiffNoiseClassifierException("Claude Code output did not contain a result.");
    }

    private static string GetNeutralWorkingDirectory()
    {
        string dir = Path.Combine(Path.GetTempPath(), "GitExtensions", "AiFilter");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception)
        {
            // Best effort - the process may have already exited.
        }
    }

    private static string Truncate(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength] + "…";
}
