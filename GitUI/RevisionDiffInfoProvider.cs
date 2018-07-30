using System.Collections.Generic;
using GitCommands;
using JetBrains.Annotations;

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
        /// B - second selected row
        /// </summary>
        [ContractAnnotation("revisions:null=>false,extraDiffArgs:null,firstRevision:null,secondRevision:null,error:notnull")]
        [ContractAnnotation("=>false,extraDiffArgs:null,firstRevision:null,secondRevision:null,error:notnull")]
        [ContractAnnotation("=>true,extraDiffArgs:notnull,firstRevision:notnull,secondRevision:notnull,error:null")]
        public static bool TryGet(
            IReadOnlyList<GitRevision> revisions,
            RevisionDiffKind diffKind,
            out string extraDiffArgs,
            out string firstRevision,
            out string secondRevision,
            out string error)
        {
            // NOTE Order in revisions is that first clicked is last in array

            // Detect rename and copy
            extraDiffArgs = "-M -C";

            if (revisions == null)
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

            if (revisions[0] == null || (revisions.Count == 2 && revisions[1] == null))
            {
                error = "Unexpected single null argument to difftool";
                firstRevision = null;
                secondRevision = null;
                return false;
            }

            if (diffKind == RevisionDiffKind.DiffAB)
            {
                firstRevision = revisions.Count == 1
                    ? GetParentRef(revisions[0])
                    : revisions[1].Guid;
                secondRevision = revisions[0].Guid;
            }
            else
            {
                // Second revision is always local
                secondRevision = null;

                if (diffKind == RevisionDiffKind.DiffBLocal)
                {
                    firstRevision = revisions[0].Guid;
                }
                else if (diffKind == RevisionDiffKind.DiffBParentLocal)
                {
                    firstRevision = GetParentRef(revisions[0]);
                }
                else if (revisions.Count == 1)
                {
                    if (diffKind == RevisionDiffKind.DiffALocal)
                    {
                        firstRevision = GetParentRef(revisions[0]);
                    }
                    else if (diffKind == RevisionDiffKind.DiffAParentLocal)
                    {
                        firstRevision = GetParentRef(revisions[0]) + "^";
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
                    firstRevision = revisions[1].Guid;
                }
                else if (diffKind == RevisionDiffKind.DiffAParentLocal)
                {
                    firstRevision = GetParentRef(revisions[1]);
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

            string GetParentRef(GitRevision revision)
            {
                return revision.FirstParentGuid?.ToString() ?? revision.Guid + '^';
            }
        }
    }
}