namespace GitUI.AutoCompletion
{
    public interface IAutoCompleteProvider
    {
        Task<IEnumerable<AutoCompleteWord>> GetAutoCompleteWordsAsync(CancellationToken cancellationToken);
    }
}
