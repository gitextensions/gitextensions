using System.Collections.Generic;
using GitCommands;

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
        public static string Get(IReadOnlyList<GitRevision> revisions, RevisionDiffKind diffKind,
            out string extraDiffArgs, out string firstRevision, out string secondRevision)
        {
            // Note: Order in revisions is that first clicked is last in array
            string error = "";

            // Detect rename and copy
            extraDiffArgs = "-M -C";

            if (revisions == null)
            {
                error = "Unexpected null revision argument to difftool";
                firstRevision = null;
                secondRevision = null;
            }
            else if (revisions.Count == 0 || revisions.Count > 2)
            {
                error = "Unexpected number of arguments to difftool: " + revisions.Count;
                firstRevision = null;
                secondRevision = null;
            }
            else if (revisions[0] == null || (revisions.Count > 1 && revisions[1] == null))
            {
                error = "Unexpected single null argument to difftool";
                firstRevision = null;
                secondRevision = null;
            }
            else if (diffKind == RevisionDiffKind.DiffAB)
            {
                if (revisions.Count == 1)
                {
                    firstRevision = revisions[0].FirstParentGuid ?? revisions[0].Guid + '^';
                }
                else
                {
                    firstRevision = revisions[1].Guid;
                }

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
                    firstRevision = revisions[0].FirstParentGuid ?? revisions[0].Guid + '^';
                }
                else
                {
                    firstRevision = revisions[0].Guid;
                    if (revisions.Count == 1)
                    {
                        if (diffKind == RevisionDiffKind.DiffALocal)
                        {
                            firstRevision = revisions[0].FirstParentGuid ?? revisions[0].Guid + '^';
                        }
                        else if (diffKind == RevisionDiffKind.DiffAParentLocal)
                        {
                            firstRevision = (revisions[0].FirstParentGuid ?? revisions[0].Guid + '^') + "^";
                        }
                        else
                        {
                            error = "Unexpected arg to difftool with one revision: " + diffKind;
                        }
                    }
                    else
                    {
                        if (diffKind == RevisionDiffKind.DiffALocal)
                        {
                            firstRevision = revisions[1].Guid;
                        }
                        else if (diffKind == RevisionDiffKind.DiffAParentLocal)
                        {
                            firstRevision = revisions[1].FirstParentGuid ?? revisions[1].Guid + '^';
                        }
                        else
                        {
                            error = "Unexpected arg to difftool with two revisions: " + diffKind;
                        }
                    }
                }
            }

            return error;
        }
    }
}