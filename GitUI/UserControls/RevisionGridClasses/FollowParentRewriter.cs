using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitCommands;
using System.IO;

namespace GitUI.UserControls.RevisionGridClasses
{
    /// Parent rewriter supplementing similar funcionality in git
    /// Created as a substitute to rewrite parents while following file renames
    /// The general idea is as follows:
    /// * A single _fileName is processed
    /// * EnsureHistoryLoaded() checkes whether there were any renames in file's history. 
    ///   If there were no renames, no further processing is needed.
    /// * If there were renames, LoadParents() creates an in-memory graph of all commits leading up to 
    ///   interesting commits (i.e. commits modifying any of the current or historical file names)
    /// * The RevisionGrid fetches revisions while following renames and passes them to PushRevision() 
    ///   These revisions are called "Seen" revisions. They are a subset of revisions processed during 
    ///   the LoadParents() phase.
    /// * Each time a revision is queued, the _historyGraph is passes trhough to reach to rev's ancestors to 
    ///   find another descendent Seen revision. That revision's parent is rewritten to point to currently 
    ///   processed revision (UpdateDescendants()).
    /// * When all parents of a revision point to a Seen revision, than it is safe to pass it further with
    ///   rewritten parents Flush(false)
    /// * Redundant parents are removed to prevent excessive lane count in graph  
    /// * A final Flush(true) passes on any pending revisions even if they weren't completely rewritten
    public class FollowParentRewriter
    {
        private class CommitData
        {
            // original parents
            internal List<string> parents = new List<string>();
            // original children
            internal List<string> children = new List<string>();
            // rewritten parents (Empty == no rewriting necessary)
            internal List<string> rewrittenParents = new List<string>();
            // null  == mentioned as parent only
            // !null == received GitRevision data (==seen)
            internal GitRevision rev = null;
            // true == all ancestors were seen
            internal bool allAncectorsSeen = false;
            
            internal void AddParent(string parentId)
            {
                if (!parents.Contains(parentId))
                {
                    parents.Add(parentId);
                }
            }

            internal void AddChild(string childId)
            {
                if (!children.Contains(childId))
                {
                    children.Add(childId);
                }
            }

            internal bool WasSeen()
            {
                return rev != null;
            }

            internal bool RewritingNeeded()
            {
                return rewrittenParents.Any();
            }

            internal string[] ParentsAfterRewriting()
            {
                if (rewrittenParents.Any())
                {
                    return rewrittenParents.ToArray();
                }
                else if (WasSeen()) 
                {
                    return rev.ParentGuids;
                }
                else 
                {
                    return parents.ToArray();
                }
            }
        }

        private Func<string, StreamReader> _gitExecFunc;
        private string _fileName;
        public FollowParentRewriter(String fileName, Func<string, StreamReader> gitExecFunc)
        {
            _fileName = fileName;
            _gitExecFunc = gitExecFunc;
        }

        private CommitData ProvideCommitData(string commitId)
        {
            if (!_historyGraph.ContainsKey(commitId))
            {
                CommitData dta = new CommitData();
                _historyGraph.Add(commitId, dta);
                return dta;
            }
            else
            {
                return _historyGraph[commitId];
            }
        }

        /// <summary>
        /// Previous names of analysed file
        /// </summary>
        private HashSet<string> _previousNames = new HashSet<string>();

        private void AddNameToPreviousNames(string line)
        {
            if (!_previousNames.Contains(line))
            {
                _previousNames.Add(line);
            }
        }

        // Fetches previous names of _fileName, stores in _previousNames
        private void LoadPreviousNames()
        {
            string arg = "log " + GitCommandHelpers.FindRenamesAndCopiesOpts() + " --follow --name-only --format=\"%n\"";
            if (AppSettings.FullHistoryInFileHistory)
            {
                arg += " --full-history";
            }
            arg += " -- \"" + _fileName + "\"";
            using (StreamReader gitResult = _gitExecFunc(arg))
            {
                string line;
                while ((line = gitResult.ReadLine()) != null)
                {
                    if (line.IsNotNullOrWhitespace())
                    {
                        AddNameToPreviousNames(line);
                    }
                }
            }
        }

        /// <summary>
        /// Commits and their immediate relatives
        /// commitId => CommitData
        /// </summary>
        private Dictionary<string, CommitData> _historyGraph = new Dictionary<string, CommitData>();

        // line contains commitid followed by one or more parentIds
        private void AddCommitToParentsGraph(string line)
        {
            char[] space = new char[] { ' ' };
            string[] commitAndParents = line.Split(space, 2);
            string commitId = commitAndParents[0];
            CommitData ownData = ProvideCommitData(commitId);
            if ((commitAndParents.Length > 1) &&
                commitAndParents[1].IsNotNullOrWhitespace())
            {
                string[] parents = commitAndParents[1].Split(space);
                foreach (string parentId in parents)
                {
                    ownData.AddParent(parentId);
                    CommitData parentData = ProvideCommitData(parentId);
                    parentData.AddChild(commitId);
                }
            }
        }

        private void LoadParents()
        {
            string arg = "log " + GitCommandHelpers.FindRenamesAndCopiesOpts() + " --simplify-merges --parents --boundary --not --glob=notes --not --all --format=\"%H %P\"";
            if (AppSettings.OrderRevisionByDate)
            {
                arg += " --date-order";
            }
            else
            {
                arg += " --topo-order";
            }
            if (AppSettings.FullHistoryInFileHistory)
            {
                arg += " --full-history --simplify-by-decoration ";
            }
            if (AppSettings.MaxRevisionGraphCommits > 0)
            {
                arg += string.Format(" --max-count=\"{0}\"", (int)AppSettings.MaxRevisionGraphCommits);
            }
            arg += " -- \"" + _previousNames.Join("\" \"") + "\"";

            using (StreamReader gitResult = _gitExecFunc(arg))
            {
                string line;
                while ((line = gitResult.ReadLine()) != null)
                {
                    AddCommitToParentsGraph(line);
                }
            }
        }

        private bool _historyLoaded = false;
        private void EnsureHistoryLoaded()
        {
            if (!_historyLoaded) {
                LoadPreviousNames();
                _historyLoaded = true;
                if (RewriteNecessary)
                {
                    LoadParents();
                }
            }
        }

        /// <summary>
        /// Returns whether the rewrite process is needed (i.e. the file had more than one name throughout its history)
        /// </summary>
        /// <returns></returns>
        public bool RewriteNecessary {
            get {
                EnsureHistoryLoaded();
                return _previousNames.Count() > 1;
            }
        }

        /// <summary>
        /// Finds nearest seen descendents of rev and changes their parent pointers to rev
        /// </summary>
        /// <param name="rev"></param>
        private void UpdateDescendants(GitRevision rev)
        {
            // child, parent
            Queue<Tuple<string, string>> toCheck = new Queue<Tuple<string, string>>();
            // pairs (child, parent) already passed through toCheck
            HashSet<Tuple<string, string>> toCheckDuplicates = new HashSet<Tuple<string, string>>();
            ProvideCommitData(rev.Guid).children.ForEach((g) => {
                Tuple<string, string> t = new Tuple<string, string>(g, rev.Guid);
                toCheck.Enqueue(t);
                toCheckDuplicates.Add(t);
            });
            while (toCheck.Any())
            {
                Tuple<string,string> ct = toCheck.Dequeue();
                string checkChild = ct.Item1;
                string checkParent = ct.Item2;

                CommitData cdta = ProvideCommitData(checkChild);
                bool allAncestorsSeen = true;
                // cdta was seen and was reached from seen revision (rev)
                // Each of its parents pointing "in the direction" of rev should be replaced with rev
                foreach (string parentId in cdta.parents)
                {
                    if ((parentId == checkParent) && !cdta.rewrittenParents.Contains(rev.Guid))
                    {
                        cdta.rewrittenParents.Add(rev.Guid);
                    }
                    // At the same time we check whether all other parents of cdta are seen or reachable
                    else if (allAncestorsSeen)
                    {
                        CommitData parentDta = ProvideCommitData(parentId);
                        if (!parentDta.WasSeen() && !parentDta.allAncectorsSeen)
                        {
                            allAncestorsSeen = false;
                        }
                    }
                }
                if (allAncestorsSeen)
                {
                    cdta.allAncectorsSeen = true;
                }
                if (!cdta.WasSeen())
                {
                    // If cdta is an unseen revision, then we must continue along its descendants to find 
                    // a seen revision
                    cdta.children.ForEach((g) =>
                    {
                        Tuple<string, string> t = new Tuple<string, string>(g, checkChild);
                        if (!toCheckDuplicates.Contains(t))
                        {
                            toCheck.Enqueue(t);
                            toCheckDuplicates.Add(t);
                        }
                    });
                }
            }
        }

        private void UpdateHistoryGraph(GitRevision rev)
        {
            CommitData dta = ProvideCommitData(rev.Guid);
            dta.rev = rev;
        }

        private Queue<GitRevision> _revisionQueue = new Queue<GitRevision>();

        /// <summary>
        /// Receives revision data, stores it for receiving by Flush
        /// </summary>
        /// <param name="rev"></param>
        public void PushRevision(GitRevision rev)
        {
            _revisionQueue.Enqueue(rev);
            if ((rev != null) && RewriteNecessary)
            {
                UpdateHistoryGraph(rev);
                UpdateDescendants(rev);
            }
        }

        // RemoveRedundantParents helper, returns whether a commit is reachable through rewritten parents
        // checking range limited to Seen or allAncestorsSeen commits
        private bool RemoveRedundantParentsIsReachable(string target, string source)
        {
            // (source) connections to check for reachability
            Queue<string> sourcesToCheck = new Queue<string>();
            HashSet<string> checkedSources = new HashSet<string>();
            sourcesToCheck.Enqueue(source);
            while (sourcesToCheck.Any())
            {
                string currentSource = sourcesToCheck.Dequeue();
                if (checkedSources.Contains(currentSource))
                {
                    continue;
                }
                checkedSources.Add(currentSource);
                if (currentSource == target)
                {
                    return true;
                }
                var dta = ProvideCommitData(currentSource);
                if (!dta.WasSeen() && !dta.allAncectorsSeen)
                {
                    // interested only in Seen || allAncectorsSeen commits
                    continue;
                }
                foreach (string p in dta.ParentsAfterRewriting())
                {
                    sourcesToCheck.Enqueue(p);
                }
            }
            return false;
        }

        /// Removes parent paths to allAncectorsSeen commits that can be pruned without affecting reachability
        private string[] RemoveRedundantParents(string[] parentsToCheck, string[] parentsToKeep)
        {
            if (parentsToCheck.Length == 1)
            {
                return parentsToCheck;
            }
                           // parentsToKeep should not to be considered for removal
            var toRemove = from removalCandidateParent in parentsToCheck.Except(parentsToKeep)
                           from reachabilityCheckedParent in parentsToCheck
                           // parents to check different from parents to removal
                           where (removalCandidateParent != reachabilityCheckedParent)
                           // and reachable
                           && RemoveRedundantParentsIsReachable(removalCandidateParent, reachabilityCheckedParent)
                           // should be excluded from result
                           select removalCandidateParent;
            return parentsToCheck.Except(toRemove).ToArray();
        }

        private void DequeueAndProcessRevision(Action<GitRevision> processRevision)
        {
            GitRevision rev = _revisionQueue.Dequeue();
            if ((rev != null) && RewriteNecessary)
            {
                var cdta = ProvideCommitData(rev.Guid);
                if (cdta.RewritingNeeded())
                {
                    var rewrittenParentGuids = cdta.ParentsAfterRewriting();
                    if (cdta.allAncectorsSeen)
                    {
                        rewrittenParentGuids = RemoveRedundantParents(rewrittenParentGuids, rev.ParentGuids);
                    }
                    rev.ParentGuids = rewrittenParentGuids;
                }
            }
            processRevision(rev);
        }

        /// <summary>
        /// Calls supplied function with any revisions ready to be processed
        /// If flushAll==True then forces processing of all pending revisions without rewriting them
        /// </summary>
        public void Flush(bool flushAll, Action<GitRevision> processRevision)
        {
            if (!flushAll && RewriteNecessary)
            {
                while (_revisionQueue.Any())
                {
                    GitRevision r = _revisionQueue.Peek();
                    if ((r == null)  || ProvideCommitData(r.Guid).allAncectorsSeen)
                    {
                        DequeueAndProcessRevision(processRevision);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                while (_revisionQueue.Any())
                {
                    DequeueAndProcessRevision(processRevision);
                }
            }
        }
    }
}
