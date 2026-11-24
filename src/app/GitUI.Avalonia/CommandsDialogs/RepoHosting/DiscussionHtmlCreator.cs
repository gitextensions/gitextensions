using GitExtensions.Extensibility.Plugins;

namespace GitUI.CommandsDialogs.RepoHosting;

internal static class DiscussionHtmlCreator
{
    // Avalonia renders typed native rows instead of routing HTML through a platform browser control.
    public static IReadOnlyList<DiscussionEntryPresentation> CreateFor(List<IDiscussionEntry>? entries = null)
        => entries?.Select(CreatePresentation).ToArray() ?? [];

    private static DiscussionEntryPresentation CreatePresentation(IDiscussionEntry entry)
    {
        ICommitDiscussionEntry? commitEntry = entry as ICommitDiscussionEntry;
        return new DiscussionEntryPresentation(
            Format(entry.Author),
            entry.Created.ToString(),
            Format(entry.Body),
            commitEntry is null ? null : Format(commitEntry.Sha));
    }

    private static string Format(object? value) => value?.ToString() ?? "[UNKNOWN]";

    // Avalonia inherits the dialog font and resolved colors from its native theme, so the
    // original HTML CSS font-key naming TODO has no portable key to rename here.
    internal sealed record DiscussionEntryPresentation(
        string Author,
        string Created,
        string Body,
        string? Commit);
}
