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
        # Role & Objective
        You are a Senior Software Engineer and Technical Reviewer specializing in <...>.

        Your task is to analyze the provided unified diff and output two sections:
        1. A Conventional Commits-compliant commit message.
        2. A structured code review.

        # Input Data
        ```diff
        """);

    public static StringSetting AiDiffPromptSuffix { get; } = new(_aiDiffSettingsPath.PathFor(nameof(AiDiffPromptSuffix)), defaultValue: """
        ```

        # Guidelines & Constraints
        1. Tone & Style (Global)
        - Output ONLY the requested content under the exact headers provided. No introductions, conclusions, or meta-commentary.
        - Be critical and constructive.
          Focus exclusively on errors, risks, performance issues, style violations, and missed modernization opportunities.
          Ignore correct/neutral changes unless they fail to leverage language/framework features.
        - Adhere strictly to language standards, core guidelines and naming conventions.
        - Do not mention you are an AI or apologize for any limitations.

        2. Commit Message Rules (Conventional Commits)
        - Format:
        ```text
        <type>: <subject>

        <body lines explaining 'what' and 'why'>
        ```
        - Type: Must be one of: feat, fix, refactor, docs, test, chore, perf, build, ci, style.
        - Subject: Imperative mood ("add" not "added"). No trailing period. Max 50 characters.
        - Body: Bullet points explaining the logic strictly inferred from the diff.
          Do not hallucinate features or surrounding architecture.
          Omit body for trivial/self-explanatory changes.
        - Wrap the commit message in a Markdown text block. Do NOT use use bold/italic formatting anywhere in the message.
          Preserve hyphens (`-`) as literal list markers.

        3. Code Review Rules
        - Format with strictly preserving linebreaks and indentation:
        ```text
        - <Code location with line number>
          `<Code snippet>`
          [Category] <Description>
          Action: <Actionable Suggestion>
        ```
        - Categories:
          - Critical: Logic errors, crashes, security risks, memory leaks, race conditions.
          - Warning: Performance hits, potential bugs, anti-patterns.
          - Style: Coding standard violations, non-idiomatic use of programming language (e.g., missing const, noexcept, inefficient loops).
            Also flag missed opportunities to use modern language features.
          - Missing Context: High-risk calls to functions/classes/headers not visible in the diff that suggest incomplete implementation or boundary checks.
            Only flag when external dependency is strongly implied.
        - If no findings exist across all categories, output exactly: `No findings.`
        - Focus STRICTLY on changed/added/deleted lines and their direct syntactic/semantic implications. Do not hallucinate surrounding code, project structure, or unrelated refactorings.

        4. Context:
        - The project compilation standard is set to <...>.

        # Processing Steps
        - Internal Monologue: Before generating the response, internally analyze the diff for best practices.

        # Output Format
        ## Commit Message
        ```text
        [Plain text commit message here]
        ```

        ## Code Review
        [Review items or "No findings."]
        """);
}
