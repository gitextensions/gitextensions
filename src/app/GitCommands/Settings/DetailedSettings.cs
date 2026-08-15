using GitExtensions.Extensibility.Settings;

namespace GitCommands.Settings;

public static class DetailedSettings
{
    private static readonly SettingsPath _settingsPath = new(parent: null, "Detailed");
    private static readonly SettingsPath _aiSettingsPath = new(parent: null, _settingsPath.PathFor("AI"));
    private static readonly SettingsPath _aiDiffSettingsPath = new(parent: null, _aiSettingsPath.PathFor("Diff"));

    public static BoolSetting GetRemoteBranchesDirectlyFromRemote { get; } = new(_settingsPath.PathFor(nameof(GetRemoteBranchesDirectlyFromRemote)), defaultValue: false);
    public static BoolSetting AddMergeLogMessages { get; } = new(_settingsPath.PathFor(nameof(AddMergeLogMessages)), defaultValue: false);
    public static NumberSetting<int> MergeLogMessagesCount { get; } = new(_settingsPath.PathFor(nameof(MergeLogMessagesCount)), defaultValue: 20);

    public static StringSetting AiDiffPromptPrefix { get; } = new(_aiDiffSettingsPath.PathFor(nameof(AiDiffPromptPrefix)), defaultValue: """
        You are a senior software engineer specializing in ...

        Here is a diff of a code change:
        ```diff
        """);

    public static StringSetting AiDiffPromptSuffix { get; } = new(_aiDiffSettingsPath.PathFor(nameof(AiDiffPromptSuffix)), defaultValue: """
        ```

        Your task is to perform a review of the code changes and to write a concise and clear commit message that describes the change and its purpose.
        The commit message should be structured as follows:

        1. Subject line: imperative mood, at most 50 characters, no trailing period.
            Use a Conventional Commits prefix when it fits the change, one of:
            feat, fix, refactor, docs, test, chore, perf, build, ci
            (example: "fix: prevent crash when staging an empty file").
        2. Then exactly one blank line.
        3. Body: explain WHAT changed and, above all, WHY it changed.
            Use "- " bullet points when there are several distinct changes.

        Guidelines:
        - Infer the intent from the diff; never invent changes that aren't there.
        - For a small, self-explanatory change, a subject line alone is fine.
        - Be concise and specific; avoid filler like "updated some code".

        Output ONLY the raw commit message text: no markdown, no code fences, no
        surrounding quotes, and no commentary before or after it.
        """);
}
