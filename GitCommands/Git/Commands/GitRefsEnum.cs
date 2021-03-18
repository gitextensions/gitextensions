using System.Diagnostics;
using GitUIPluginInterfaces;

namespace GitCommands.Git.Commands
{
    /// <summary>
    /// Enums requestable in GitRefs() (multiple names can be appended)
    /// Compare to <see ref="GitRefType"/> for actual values of parsed GitRefs
    /// </summary>
    public enum GetRefsEnum
    {
        None = 0,

        Branches = 1 << 0,
        Remotes = 1 << 1,
        Tags = 1 << 2,

        // All refs, including those abve but also (at least) stash, notes and bisect
        All = 1 << 3
    }
}
