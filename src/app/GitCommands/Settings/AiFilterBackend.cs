namespace GitCommands;

/// <summary>
/// Selects how the diff AI filter talks to Claude.
/// </summary>
public enum AiFilterBackend
{
    /// <summary>Call the Anthropic Messages API directly using an API key (Console billing).</summary>
    AnthropicApi = 0,

    /// <summary>
    /// Drive the locally installed Claude Code in headless mode, reusing its authentication
    /// (works with a Claude Pro/Max subscription). This is the same mechanism the Claude Agent SDK uses.
    /// </summary>
    ClaudeCode = 1,
}
