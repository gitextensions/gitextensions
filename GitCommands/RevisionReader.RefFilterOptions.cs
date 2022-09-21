using System;

namespace GitCommands
{
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
    [Flags]
    public enum RefFilterOptions
    {
        None                    = 0x000,    // Git default, current branch
        Branches                = 0x001,    // --branches=<filter>

        // Unused
        // Remotes              = 0x002,    // --remotes
        // Tags                 = 0x004,    // --tags
        All                     = 0x007,    // --all (default is HEAD)
        Reflog                  = 0x200,    // --reflog

        // Exclude some refs from other than reflog
        NoStash                 = 0x008,    // --exclude=refs/stash
        NoGitNotes              = 0x010,    // --not --glob=notes --not

        // Other revision filters
        NoMerges                = 0x020,    // --no-merges
        FirstParent             = 0x040,    // --first-parent
        Boundary                = 0x100,    // --boundary

        // history simplification
        SimplifyByDecoration = 0x080,       // --simplify-by-decoration
    }
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row
}
