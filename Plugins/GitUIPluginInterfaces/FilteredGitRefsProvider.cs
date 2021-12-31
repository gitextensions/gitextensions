using System;
using System.Collections.Generic;
using System.Linq;
using GitExtUtils;

namespace GitUIPluginInterfaces
{
    /// <inherit/>
    public sealed class FilteredGitRefsProvider : IFilteredGitRefsProvider
    {
        public FilteredGitRefsProvider(IGitModule module)
        {
            _refs = new(() => module.GetRefs(RefsFilter.NoFilter));
        }

        private readonly Lazy<IReadOnlyList<IGitRef>> _refs;

        /// <inherit/>
        public IReadOnlyList<IGitRef> GetRefs(RefsFilter filter)
        {
            if (filter == RefsFilter.NoFilter)
            {
                return _refs.Value;
            }

            return _refs.Value.Where(r =>
                ((filter & RefsFilter.Tags) != 0 && r.IsTag)
                || ((filter & RefsFilter.Remotes) != 0 && r.IsRemote)
                || ((filter & RefsFilter.Heads) != 0 && r.IsHead)).ToList();
        }
    }
}
