using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GitUIPluginInterfaces;

namespace GitUI
{
    public static class RevisionDiffInfoProvider
    {
        /// <summary>
        /// One row selected:
        /// B - Selected row
        /// A - B's parent
        ///
        /// Two rows selected:
        /// A - first selected row
        /// B - second selected row.
        /// </summary>
        public static bool TryGet(
            IReadOnlyList<GitRevision?>? revisions,
            RevisionDiffKind diffKind,
            [NotNullWhen(returnValue: true)] out string? firstRevision,
            out string? secondRevision,
            [NotNullWhen(returnValue: false)] out string? error)
        {
            // NOTE Order in revisions is that first clicked is last in array

            if (revisions is null)
            {
                error = "Unexpected null revision argument to difftool";
                firstRevision = null;
                secondRevision = null;
                return false;
            }

            if (revisions.Count == 0 || revisions.Count > 2)
            {
                error = "Unexpected number of arguments to difftool: " + revisions.Count;
                firstRevision = null;
                secondRevision = null;
                return false;
            }

            var revision0 = revisions[0];

            if (revision0 is null)
            {
                error = "Unexpected single null argument to difftool";
                firstRevision = null;
                secondRevision = null;
                return false;
            }

            if (revisions.Count == 2 && revisions[1] is null && diffKind == RevisionDiffKind.DiffBLocal)
            {
                error = "Unexpected second null argument to difftool for DiffB";
                firstRevision = null;
                secondRevision = null;
                return false;
            }

            if (diffKind == RevisionDiffKind.DiffAB)
            {
                // If revisions[1]?.Guid is null, the "commit before the initial" is used as firstRev
                firstRevision = revisions.Count == 1
                    ? GetParentRef(revision0)
                    : revisions[1]?.Guid ?? "--root";
                secondRevision = revision0.Guid;
            }
            else
            {
                // Second revision is always local
                secondRevision = null;

                if (diffKind == RevisionDiffKind.DiffBLocal)
                {
                    firstRevision = revision0.Guid;
                }
                else if (revisions.Count == 1)
                {
                    if (diffKind == RevisionDiffKind.DiffALocal)
                    {
                        firstRevision = GetParentRef(revision0);
                    }
                    else
                    {
                        firstRevision = null;
                        error = "Unexpected arg to difftool with one revision: " + diffKind;
                        return false;
                    }
                }
                else if (diffKind == RevisionDiffKind.DiffALocal)
                {
                    firstRevision = revisions[1]?.Guid ?? "--root";
                }
                else
                {
                    firstRevision = null;
                    error = "Unexpected arg to difftool with two revisions: " + diffKind;
                    return false;
                }
            }

            error = null;
            return true;

            static string GetParentRef(GitRevision revision)
            {
                return revision.FirstParentId?.ToString() ?? revision.Guid + '^';
            }
        }
    }
}
