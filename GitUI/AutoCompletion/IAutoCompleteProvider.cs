using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GitUI.AutoCompletion
{
    public interface IAutoCompleteProvider
    {
        Task<IEnumerable<AutoCompleteWord>> GetAutoCompleteWords(CancellationTokenSource cts);
    }
}