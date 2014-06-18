using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GitUI.AutoCompletion
{
    public interface IAutoCompleteProvider
    {
        Task<List<AutoCompleteWord>> GetAutoCompleteWords (CancellationTokenSource cts);
    }
}