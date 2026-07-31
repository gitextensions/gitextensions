using GitCommands;

namespace GitUI.AI;

/// <summary>
/// Creates the configured <see cref="IDiffNoiseClassifier"/> backend.
/// </summary>
public static class DiffNoiseClassifierFactory
{
    public static IDiffNoiseClassifier Create()
        => AppSettings.AiFilterBackend.Value switch
        {
            AiFilterBackend.ClaudeCode => new ClaudeCodeDiffNoiseClassifier(),
            _ => new AnthropicDiffNoiseClassifier()
        };
}
