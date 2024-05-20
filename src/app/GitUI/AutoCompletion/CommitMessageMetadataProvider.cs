namespace GitUI.AutoCompletion;

internal class CommitMessageMetadataProvider : IAutoCompleteProvider
{
    private static readonly string[] keywords = [
        "Co-authored-by: ",
        "Signed-off-by: ",
        "BREAKING CHANGE: ",
        "Reviewed-by: ",
        "Tested-by: ",
        ];

    private static readonly AutoCompleteWord[] _autoCompleteWords = keywords.Select(k => new AutoCompleteWord(k)).ToArray();

    public Task<IEnumerable<AutoCompleteWord>> GetAutoCompleteWordsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_autoCompleteWords.AsEnumerable());
    }
}
