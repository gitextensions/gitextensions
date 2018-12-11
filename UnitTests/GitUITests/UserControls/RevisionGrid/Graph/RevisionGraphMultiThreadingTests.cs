using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitCommands;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid
{
    // This is a brute force, worst case,  thread safety test. What is does:
    // - Load revisions from git in a random order. No topo-order. To ensure the biggest possible threading issues.
    // - Build revision graph in another thread. This will almost impossible, since the order of the revisions constantly changes.
    // - Keep rendering all the stuff that is or isn't available. More threading horror.
    [TestFixture]
    public class RevisionGraphMultiThreadingTests
    {
        // Number of times to repeat the cycle. Each cycle, the number of revisions increases.
        // It is useful to repeat the threads, because it will make sure all threads are started at the same time.
        // Otherwise, the revision loading is long finished and the rendering is still running
        // Advice: set this number to 50 for thread safety test
        private const int _numberOfRepeats = 10;

        // Increase this number to create a larger test set.
        // Advice: set this number to 1000 for thread safety test
        private const int _numberOfRevisionsAddedPerRun = 500;
        private readonly Random _random = new Random();

        private RevisionGraph _revisionGraph;

        [SetUp]
        public void Setup()
        {
            _revisionGraph = new RevisionGraph();

            GitRevision revision = new GitRevision(ObjectId.Random());

            // Mark the first revision as the current checkout
            _revisionGraph.Add(revision, RevisionNodeFlags.CheckedOut);
        }

        [Test]
        public void ShouldReorderInTopoOrder()
        {
            for (int i = 0; i < _numberOfRepeats; i++)
            {
                // Simulate thread that loads revisions from git
                var loadRevisionsTask = new Task(() => LoadRandomRevisions());

                // Simulate thread that caches the rows in the background
                var buildCacheTask = new Task(() => BuildCache());

                // Simulate thread that renders
                var renderTask = new Task(() => Render());

                loadRevisionsTask.Start();
                buildCacheTask.Start();
                renderTask.Start();

                Task.WaitAll(loadRevisionsTask, buildCacheTask, renderTask);

                // One last 'cache to', in case the loading of the revisions was finished after building the cache (unlikely)
                _revisionGraph.CacheTo(_revisionGraph.Count, _revisionGraph.Count);

                // Validate topo order
                Assert.IsTrue(_revisionGraph.GetTestAccessor().ValidateTopoOrder());
            }
        }

        private void LoadRandomRevisions()
        {
            List<GitRevision> randomRevisions = new List<GitRevision>();

            for (int i = 0; i < _numberOfRevisionsAddedPerRun; i++)
            {
                GitRevision revision = new GitRevision(ObjectId.Random());
                if (randomRevisions.Count > 1)
                {
                    var randomRevision1 = randomRevisions[_random.Next(randomRevisions.Count - 1)];
                    var randomRevision2 = randomRevisions[_random.Next(randomRevisions.Count - 1)];

                    revision.ParentIds = new ObjectId[] { randomRevision1.ObjectId, randomRevision2.ObjectId };
                }

                _revisionGraph.Add(revision, RevisionNodeFlags.None);

                randomRevisions.Add(revision);
            }

            Assert.IsTrue(_revisionGraph.GetTestAccessor().ValidateTopoOrder(), "Revisions not reordered to topo order");
        }

        private void BuildCache()
        {
            for (int i = 0; i < _numberOfRevisionsAddedPerRun; i++)
            {
                // Cache in chunks
                _revisionGraph.CacheTo(_revisionGraph.GetCachedCount() + 30, _revisionGraph.GetCachedCount() + 10);
            }
        }

        private void Render()
        {
            for (int i = 0; i < _numberOfRevisionsAddedPerRun / 10; i++)
            {
                var pageStart = _random.Next(_revisionGraph.Count);

                for (int j = pageStart; j < pageStart + 4; j++)
                {
                    // Simulate render commit message
                    _revisionGraph.GetNodeForRow(j);

                    // Simulate render graph
                    _revisionGraph.GetSegmentsForRow(j)?.GetLaneCount();
                }
            }
        }
    }
}