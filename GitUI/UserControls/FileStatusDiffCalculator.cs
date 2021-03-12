using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;

namespace GitUI
{
    public class FileStatusDiffCalculator
    {
        private readonly Func<GitModule> _getModule;

        public FileStatusDiffCalculator(Func<GitModule> getModule)
        {
            _getModule = getModule;
        }

        public IReadOnlyList<FileStatusWithDescription> SetDiffs(in IReadOnlyList<GitRevision> revisions, Func<ObjectId, string> describeRevision,
            Func<ObjectId, GitRevision> getRevision = null)
        {
            var selectedRev = revisions?.FirstOrDefault();
            if (selectedRev is null)
            {
                return Array.Empty<FileStatusWithDescription>();
            }

            GitModule module = GetModule();

            var fileStatusDescs = new List<FileStatusWithDescription>();
            if (revisions.Count == 1)
            {
                if (selectedRev.ParentIds is null || selectedRev.ParentIds.Count == 0)
                {
                    // No parent for the initial commit
                    fileStatusDescs.Add(new FileStatusWithDescription(
                        firstRev: null,
                        secondRev: selectedRev,
                        summary: GetDescriptionForRevision(describeRevision, selectedRev.ObjectId),
                        statuses: module.GetTreeFiles(selectedRev.TreeGuid, full: true)));
                }
                else
                {
                    // Get the parents for the selected revision
                    var multipleParents = AppSettings.ShowDiffForAllParents ? selectedRev.ParentIds.Count : 1;
                    fileStatusDescs.AddRange(selectedRev
                        .ParentIds?
                        .Take(multipleParents)
                        .Select(parentId =>
                            new FileStatusWithDescription(
                                firstRev: new GitRevision(parentId),
                                secondRev: selectedRev,
                                summary: Strings.DiffWithParent + GetDescriptionForRevision(describeRevision, parentId),
                                statuses: module.GetDiffFilesWithSubmodulesStatus(parentId, selectedRev.ObjectId, selectedRev.FirstParentId))));
                }

                // Show combined (merge conflicts) when a single merge commit is selected
                var isMergeCommit = fileStatusDescs.Count > 1;
                if (AppSettings.ShowDiffForAllParents && isMergeCommit)
                {
                    var conflicts = module.GetCombinedDiffFileList(selectedRev.Guid);
                    if (conflicts.Count != 0)
                    {
                        // Create an artificial commit
                        fileStatusDescs.Add(new FileStatusWithDescription(
                            firstRev: new GitRevision(ObjectId.CombinedDiffId), secondRev: selectedRev, summary: Strings.CombinedDiff, statuses: conflicts));
                    }
                }

                return fileStatusDescs;
            }

            // With more than 4, only first -> selected is interesting
            // Show multi compare if 2-4 are selected
            const int maxMultiCompare = 4;

            // With 4 selected, assume that ranges are selected: baseA..headA baseB..headB
            // the first item is therefore the second selected
            var firstRev = AppSettings.ShowDiffForAllParents && revisions.Count == maxMultiCompare
                ? revisions[2]
                : revisions.Last();

            fileStatusDescs.Add(new FileStatusWithDescription(
                firstRev: firstRev,
                secondRev: selectedRev,
                summary: Strings.DiffWithParent + GetDescriptionForRevision(describeRevision, firstRev.ObjectId),
                statuses: module.GetDiffFilesWithSubmodulesStatus(firstRev.ObjectId, selectedRev.ObjectId, selectedRev.FirstParentId)));

            if (!AppSettings.ShowDiffForAllParents || revisions.Count > maxMultiCompare)
            {
                return fileStatusDescs;
            }

            // Extra information with limited selection
            var allAToB = fileStatusDescs[0].Statuses;

            // Get base commit, add as parent if unique
            Lazy<ObjectId> head = getRevision is not null
                ? new Lazy<ObjectId>(() => getRevision(ObjectId.IndexId).FirstParentId)
                : new Lazy<ObjectId>(() => module.RevParse("HEAD"));
            var firstRevHead = GetRevisionOrHead(firstRev, head);
            var selectedRevHead = GetRevisionOrHead(selectedRev, head);
            var baseRevGuid = module.GetMergeBase(firstRevHead, selectedRevHead);

            // Four selected, to check if two ranges are selected
            var baseA = (revisions.Count != 4 || baseRevGuid is null)
                ? null
                : module.GetMergeBase(GetRevisionOrHead(revisions[3], head), firstRevHead);
            var baseB = baseA is null || baseA != revisions[3].ObjectId
                ? null
                : module.GetMergeBase(GetRevisionOrHead(revisions[1], head), selectedRevHead);
            if (baseB != revisions[1].ObjectId)
            {
                baseB = null;
            }

            // Check for separate branches (note that artificial commits both have HEAD as BASE)
            if (baseRevGuid is null

                // For two check that the selections are in separate branches
                || (revisions.Count == 2 && (baseRevGuid == firstRevHead
                    || baseRevGuid == selectedRevHead))

                // For three, show multi-diff if not base is selected
                || (revisions.Count == 3 && baseRevGuid != revisions[1].ObjectId)

                // For four, two ranges must be selected
                || (revisions.Count == 4 && (baseA is null || baseB is null)))
            {
                // No variant of range diff, show multi diff
                fileStatusDescs.AddRange(
                    revisions
                        .Where(rev => rev != firstRev && rev != selectedRev)
                        .Select(rev => new FileStatusWithDescription(
                            firstRev: rev,
                            secondRev: selectedRev,
                            summary: Strings.DiffWithParent + GetDescriptionForRevision(describeRevision, rev.ObjectId),
                            statuses: module.GetDiffFilesWithSubmodulesStatus(rev.ObjectId, selectedRev.ObjectId, selectedRev.FirstParentId))));

                return fileStatusDescs;
            }

            // Present common files in BASE->B, BASE->A separately
            // For the following diff:  A->B a,c,d; BASE->B a,b,c; BASE->A a,b,d
            // (the file a has unique changes, b has the same change and c,d is changed in one of the branches)
            // The following groups will be shown: A->B a,c,d; BASE->B a,c; BASE->A a,d; Common BASE b
            var allBaseToB = module.GetDiffFilesWithSubmodulesStatus(baseRevGuid, selectedRev.ObjectId, selectedRev.FirstParentId);
            var allBaseToA = module.GetDiffFilesWithSubmodulesStatus(baseRevGuid, firstRev.ObjectId, firstRev.FirstParentId);

            var comparer = new GitItemStatusNameEqualityComparer();
            var commonBaseToAandB = allBaseToB.Intersect(allBaseToA, comparer).Except(allAToB, comparer).ToList();

            var revBase = new GitRevision(baseRevGuid);
            fileStatusDescs.Add(new FileStatusWithDescription(
                firstRev: revBase,
                secondRev: selectedRev,
                summary: Strings.DiffBaseToB + GetDescriptionForRevision(describeRevision, selectedRev.ObjectId),
                statuses: allBaseToB.Except(commonBaseToAandB, comparer).ToList()));
            fileStatusDescs.Add(new FileStatusWithDescription(
                firstRev: revBase,
                secondRev: firstRev,
                summary: Strings.DiffBaseToB + GetDescriptionForRevision(describeRevision, firstRev.ObjectId),
                statuses: allBaseToA.Except(commonBaseToAandB, comparer).ToList()));
            fileStatusDescs.Add(new FileStatusWithDescription(
                firstRev: revBase,
                secondRev: selectedRev,
                summary: Strings.DiffCommonBase + GetDescriptionForRevision(describeRevision, baseRevGuid),
                statuses: commonBaseToAandB));

            // Add rangeDiff as a separate group (range is not the same as diff with artificial commits)
            var statuses = new List<GitItemStatus> { new GitItemStatus { Name = Strings.DiffRange, IsRangeDiff = true } };
            var first = firstRev.ObjectId == firstRevHead ? firstRev : new GitRevision(firstRevHead);
            var selected = selectedRev.ObjectId == selectedRevHead ? selectedRev : new GitRevision(selectedRevHead);
            var (baseToFirstCount, baseToSecondCount) = module.GetCommitRangeDiffCount(first.ObjectId, selected.ObjectId);
            const int rangeDiffCommitLimit = 100;
            var desc = $"{Strings.DiffRange} {baseToFirstCount ?? rangeDiffCommitLimit}↓ {baseToSecondCount ?? rangeDiffCommitLimit}↑";

            var rangeDiff = new FileStatusWithDescription(
                firstRev: first,
                secondRev: selected,
                summary: desc,
                statuses: statuses,
                baseA: baseA,
                baseB: baseB);
            fileStatusDescs.Add(rangeDiff);

            // Git range-diff has cubic runtime complexity and can be slow and memory consuming, so just skip if diff is large
            // to avoid that GE seem to hang when selecting the range diff
            int count = (baseA is null || baseB is null
                ? baseToFirstCount + baseToSecondCount
                : module.GetCommitCount(firstRevHead.ToString(), baseA.ToString(), cache: true)
                + module.GetCommitCount(selectedRevHead.ToString(), baseB.ToString(), cache: true))
                ?? rangeDiffCommitLimit;
            if (!GitVersion.Current.SupportRangeDiffTool || count >= rangeDiffCommitLimit)
            {
                var range = baseA is null || baseB is null
                    ? $"{first.ObjectId}...{selected.ObjectId}"
                    : $"{baseA}..{first.ObjectId} {baseB}..{selected.ObjectId}";
                statuses[0].IsStatusOnly = true;

                // Message is not translated, considered as an error message
                statuses[0].ErrorMessage =
                    $"# The symmetric difference from {first.ObjectId.ToShortString()} to {selected.ObjectId.ToShortString()} is {count} >= {rangeDiffCommitLimit}\n"
                    + "# Git range-diff may take a long time and Git Extensions seem to hang during execution, why the command is not executed.\n"
                    + "# You can still run the command in a Git terminal.\n"
                    + "# Remove '--no-patch' to see changes to files too.\n"
                    + $"git range-diff {range} --no-patch";
            }

            return fileStatusDescs;

            static ObjectId GetRevisionOrHead(GitRevision rev, Lazy<ObjectId> head)
                => rev.ObjectId == ObjectId.IndexId
                ? rev.FirstParentId!
                : rev.IsArtificial
                ? head.Value
                : rev.ObjectId;

            static string GetDescriptionForRevision(Func<ObjectId, string> describeRevision, ObjectId objectId)
                => describeRevision is not null ? describeRevision(objectId) : objectId?.ToShortString();
        }

        private GitModule GetModule()
        {
            var module = _getModule();

            if (module is null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(GitModule)}");
            }

            return module;
        }
    }
}
