using System;

namespace GitCommands
{
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
    [Flags]
    public enum RefFilterOptions
    {
        None                    = 0x000,
        Branches                = 0x001,    // --branches=<filter>

        // Unused
        // Remotes              = 0x002,    // --remotes
        // Tags                 = 0x004,    // --tags
        All                     = 0x007,    // --all (default is HEAD)

        // Exclude some refs from --all
        NoStash                 = 0x008,    // --exclude=refs/stash
        NoGitNotes              = 0x010,    // --not --glob=notes --not

        NoMerges                = 0x020,    // --no-merges
        FirstParent             = 0x040,    // --first-parent
        SimplifyByDecoration    = 0x080,    // --simplify-by-decoration
        Boundary                = 0x100,    // --boundary
        Reflogs                 = 0x200,    // --reflog
    }
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row
}
