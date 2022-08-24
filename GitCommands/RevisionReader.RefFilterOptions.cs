using System;

namespace GitCommands
{
#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row
    [Flags]
    public enum RefFilterOptions
    {
        None                    = 0x000,
        Branches                = 0x001,    // --branches
        Remotes                 = 0x002,    // --remotes
        Tags                    = 0x004,    // --tags
        Stashes                 = 0x008,    //
        All                     = 0x00F,    // --all
        Boundary                = 0x010,    // --boundary
        ShowGitNotes            = 0x020,    // --not --glob=notes --not
        NoMerges                = 0x040,    // --no-merges
        FirstParent             = 0x080,    // --first-parent
        SimplifyByDecoration    = 0x100,    // --simplify-by-decoration
        Reflogs                 = 0x200,    // --reflog
    }
#pragma warning restore SA1025 // Code should not contain multiple whitespace in a row
}
