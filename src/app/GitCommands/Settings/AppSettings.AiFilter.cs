using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;

namespace GitCommands;

public static partial class AppSettings
{
    private static readonly SettingsPath AiFilterSettingsPath = new AppSettingsPath("AIFilter");

    /// <summary>
    /// The HTTP endpoint of the Anthropic Messages API used to classify diffs.
    /// </summary>
    public static ISetting<string> AiFilterEndpoint { get; } = Setting.Create(AiFilterSettingsPath, nameof(AiFilterEndpoint), "https://api.anthropic.com/v1/messages");

    /// <summary>
    /// The Anthropic model used to classify diffs.
    /// </summary>
    public static ISetting<string> AiFilterModel { get; } = Setting.Create(AiFilterSettingsPath, nameof(AiFilterModel), "claude-sonnet-5");

    /// <summary>
    /// The Anthropic API key. Stored in the settings file; leave empty to use the ANTHROPIC_API_KEY environment variable instead.
    /// </summary>
    public static ISetting<string> AiFilterApiKey { get; } = Setting.Create(AiFilterSettingsPath, nameof(AiFilterApiKey), "");

    /// <summary>
    /// The value sent in the <c>anthropic-version</c> HTTP header.
    /// </summary>
    public static ISetting<string> AiFilterAnthropicVersion { get; } = Setting.Create(AiFilterSettingsPath, nameof(AiFilterAnthropicVersion), "2023-06-01");

    /// <summary>
    /// Per-file diff is truncated to this many characters before being sent to the AI, to bound cost and latency.
    /// </summary>
    public static ISetting<int> AiFilterMaxDiffCharsPerFile { get; } = Setting.Create(AiFilterSettingsPath, nameof(AiFilterMaxDiffCharsPerFile), 20000);

    /// <summary>Hide files whose changes are only added/removed/reordered imports (e.g. C# <c>using</c> directives).</summary>
    public static ISetting<bool> AiFilterImports { get; } = Setting.Create(AiFilterSettingsPath, nameof(AiFilterImports), true);

    /// <summary>Hide files that only changed because a renamed symbol was updated at its call sites (not the declaration).</summary>
    public static ISetting<bool> AiFilterCallerSiteRenames { get; } = Setting.Create(AiFilterSettingsPath, nameof(AiFilterCallerSiteRenames), true);

    /// <summary>Hide files whose only changes convert methods between synchronous and asynchronous form in .NET projects.</summary>
    public static ISetting<bool> AiFilterSyncToAsync { get; } = Setting.Create(AiFilterSettingsPath, nameof(AiFilterSyncToAsync), true);

    /// <summary>Hide files whose only changes are style/formatting (e.g. whitespace) in .NET projects.</summary>
    public static ISetting<bool> AiFilterStyleOnly { get; } = Setting.Create(AiFilterSettingsPath, nameof(AiFilterStyleOnly), true);
}
