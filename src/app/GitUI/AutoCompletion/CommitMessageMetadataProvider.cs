namespace GitUI.AutoCompletion;

internal sealed class CommitMessageMetadataProvider : IAutoCompleteProvider
{
    private static readonly string[] keywords = [
        "Co-authored-by: ",
        "Signed-off-by: ",
        "BREAKING CHANGE: ",
        "Reviewed-by: ",
        "Tested-by: ",
        ];

    private static readonly AutoCompleteWord[] _autoCompleteWords = Array.ConvertAll(keywords, k => new AutoCompleteWord(k));

    public Task<IEnumerable<AutoCompleteWord>> GetAutoCompleteWordsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_autoCompleteWords.AsEnumerable());
    }
}
