using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Git;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI
{
    public class FileStatusDiffCalculator
    {
        private readonly Func<GitModule> _getModule;

        public FileStatusDiffCalculator(Func<GitModule> getModule)
        {
            _getModule = getModule;
        }

        public IReadOnlyList<FileStatusWithDescription> SetDiffs(IReadOnlyList<GitRevision>? revisions, Func<ObjectId, string>? describeRevision, Func<ObjectId, GitRevision?>? getRevision = null)
        {
            var selectedRev = revisions?.FirstOrDefault();
            if (selectedRev is null)
            {
                return Array.Empty<FileStatusWithDescription>();
            }

            GitModule module = GetModule();

            List<FileStatusWithDescription> fileStatusDescs = new();
            if (revisions!.Count == 1)
            {
                if (selectedRev.ParentIds is null || selectedRev.ParentIds.Count == 0)
                {
                    Validates.NotNull(selectedRev.TreeGuid);

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
                        .ParentIds
                        .Take(multipleParents)
                        .Select(parentId =>
                            new FileStatusWithDescription(
                                firstRev: new GitRevision(parentId),
                                secondRev: selectedRev,
                                summary: TranslatedStrings.DiffWithParent + GetDescriptionForRevision(describeRevision, parentId),
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
                            firstRev: new GitRevision(ObjectId.CombinedDiffId), secondRev: selectedRev, summary: TranslatedStrings.CombinedDiff, statuses: conflicts));
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
                summary: TranslatedStrings.DiffWithParent + GetDescriptionForRevision(describeRevision, firstRev.ObjectId),
                statuses: module.GetDiffFilesWithSubmodulesStatus(firstRev.ObjectId, selectedRev.ObjectId, selectedRev.FirstParentId)));

            if (!AppSettings.ShowDiffForAllParents || revisions.Count > maxMultiCompare)
            {
                return fileStatusDescs;
            }

            // Extra information with limited selection
            var allAToB = fileStatusDescs[0].Statuses;

            // Get base commit, add as parent if unique
            Lazy<ObjectId> head = getRevision is not null
                ? new Lazy<ObjectId>(() =>
                {
                    GitRevision? revision = getRevision(ObjectId.IndexId);
                    Validates.NotNull(revision);
                    return revision.FirstParentId!;
                })
                : new Lazy<ObjectId>(() =>
                {
                    var objectId = module.RevParse("HEAD");
                    Validates.NotNull(objectId);
                    return objectId;
                });
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
                            summary: TranslatedStrings.DiffWithParent + GetDescriptionForRevision(describeRevision, rev.ObjectId),
                            statuses: module.GetDiffFilesWithSubmodulesStatus(rev.ObjectId, selectedRev.ObjectId, selectedRev.FirstParentId))));

                return fileStatusDescs;
            }

            // Present common files in BASE->B, BASE->A separately
            // For the following diff:  A->B a,c,d; BASE->B a,b,c; BASE->A a,b,d
            // (the file a has unique changes, b has the same change and c,d is changed in one of the branches)
            // The following groups will be shown: A->B a,c,d; BASE->B a,c; BASE->A a,d; Common BASE b
            var allBaseToB = module.GetDiffFilesWithSubmodulesStatus(baseRevGuid, selectedRev.ObjectId, selectedRev.FirstParentId);
            var allBaseToA = module.GetDiffFilesWithSubmodulesStatus(baseRevGuid, firstRev.ObjectId, firstRev.FirstParentId);

            GitItemStatusNameEqualityComparer comparer = new();
            var commonBaseToAandB = allBaseToB.Intersect(allBaseToA, comparer).Except(allAToB, comparer).ToList();

            GitRevision revBase = new(baseRevGuid);
            fileStatusDescs.Add(new FileStatusWithDescription(
                firstRev: revBase,
                secondRev: selectedRev,
                summary: TranslatedStrings.DiffBaseToB + GetDescriptionForRevision(describeRevision, selectedRev.ObjectId),
                statuses: allBaseToB.Except(commonBaseToAandB, comparer).ToList()));
            fileStatusDescs.Add(new FileStatusWithDescription(
                firstRev: revBase,
                secondRev: firstRev,
                summary: TranslatedStrings.DiffBaseToB + GetDescriptionForRevision(describeRevision, firstRev.ObjectId),
                statuses: allBaseToA.Except(commonBaseToAandB, comparer).ToList()));
            fileStatusDescs.Add(new FileStatusWithDescription(
                firstRev: revBase,
                secondRev: selectedRev,
                summary: TranslatedStrings.DiffCommonBase + GetDescriptionForRevision(describeRevision, baseRevGuid),
                statuses: commonBaseToAandB));

            if (!GitVersion.Current.SupportRangeDiffTool)
            {
                return fileStatusDescs;
            }

            // Add rangeDiff as a separate group (range is not the same as diff with artificial commits)
            List<GitItemStatus> statuses = new() { new GitItemStatus(name: TranslatedStrings.DiffRange) { IsRangeDiff = true } };
            var first = firstRev.ObjectId == firstRevHead ? firstRev : new GitRevision(firstRevHead);
            var selected = selectedRev.ObjectId == selectedRevHead ? selectedRev : new GitRevision(selectedRevHead);
            var (baseToFirstCount, baseToSecondCount) = module.GetCommitRangeDiffCount(first.ObjectId, selected.ObjectId);

            // first and selected has a common merge base and count must be available
            // Only a printout, so no Validates
            var desc = $"{TranslatedStrings.DiffRange} {baseToFirstCount ?? -1}↓ {baseToSecondCount ?? -1}↑";

            FileStatusWithDescription rangeDiff = new(
                firstRev: first,
                secondRev: selected,
                summary: desc,
                statuses: statuses,
                baseA: baseA,
                baseB: baseB);
            fileStatusDescs.Add(rangeDiff);

            return fileStatusDescs;

            static ObjectId GetRevisionOrHead(GitRevision rev, Lazy<ObjectId> head)
                => rev.ObjectId == ObjectId.IndexId
                ? rev.FirstParentId!
                : rev.IsArtificial
                ? head.Value
                : rev.ObjectId;

            static string GetDescriptionForRevision(Func<ObjectId, string>? describeRevision, ObjectId objectId)
                => describeRevision is not null ? describeRevision(objectId) : objectId.ToShortString();
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
