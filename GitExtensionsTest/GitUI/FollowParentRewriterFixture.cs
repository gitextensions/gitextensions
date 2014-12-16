using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GitUI.CommandsDialogs;
using FluentAssertions;
using GitUI.UserControls.RevisionGridClasses;
using GitCommands;

namespace GitExtensionsTest.GitUI
{
    [TestFixture]
    class FollowParentRewriterFixture
    {
        private Func<string,StreamReader> StaticReader(string dataFollow, string dataGraph)
        {
            Func<string, StreamReader> rf = delegate(string args)
            {
                string streamData;
                if (args.Contains("--follow")) 
                {
                    streamData = dataFollow;
                }
                else 
                {
                    streamData = dataGraph;
                }
                streamData.Should().NotBeNull();
                return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(streamData)));
            };
            return rf;
        }

        private static GitRevision MkGitRevision(string commitId, params string[] parentIds)
        {
            GitRevision r = new GitRevision(null, commitId);
            r.ParentGuids = parentIds;
            return r;
        }

        private class GitRevisionExpectation
        {
            private GitRevision[] expected;

            public GitRevisionExpectation(GitRevision[] args)
            {
                expected = args;
            }

            private int calledTimes = 0;

            public void ProcessRevision(GitRevision rev)
            {
                calledTimes.Should().BeLessThan(expected.Count(), "Called too many times");
                var exp = expected[calledTimes];
                (rev != null).Should().Be(exp != null, "rev==null inconsistency");
                if (rev != null)
                {
                    rev.Guid.Should().Be(exp.Guid, "Guid");
                    rev.ParentGuids.ShouldBeEquivalentTo(exp.ParentGuids, "Parents");
                }
                calledTimes++;
            }

            public void CheckCalledEnoughTimes()
            {
                calledTimes.Should().Be(expected.Count(), "Called number of times");
            }

        }

        private void CheckFlush(FollowParentRewriter rw, bool flushAll, GitRevision[] expected)
        {
            GitRevisionExpectation x = new GitRevisionExpectation(expected);
            rw.Flush(flushAll, x.ProcessRevision);
            x.CheckCalledEnoughTimes();
        }

        [Test]
        public void RewriterTestCompleteGraph()
        {
            string dataFollow =
                "dd/aa.txt\n"+
                "\n"+
                "dd/aa.txt\n"+
                "\n"+
                "dd/aa.txt\n"+
                "\n"+
                "aa.txt\n"+
                "\n"+
                "aa.txt\n"+
                "\n"+
                "aa.txt\n"+
                "\n"+
                "aa.txt\n"+
                "\n"+
                "aa.txt\n";
            string dataGraph =
                "bef6694d0b1a58243d817cc3e915952f53269b40 5632365bb31f2da1aae700f47e1a50951754f9ff\n" +
                "5632365bb31f2da1aae700f47e1a50951754f9ff 9eea2112f54967a17ae0f02fb096a4bc41200846 5744afa2ee91a44eb4393220b19aa99a66459a3f\n" +
                "9eea2112f54967a17ae0f02fb096a4bc41200846 2cfff1e08548361ca16c3c05bb4407dc3a190ff7\n" +
                "2cfff1e08548361ca16c3c05bb4407dc3a190ff7 5744afa2ee91a44eb4393220b19aa99a66459a3f\n" +
                "5744afa2ee91a44eb4393220b19aa99a66459a3f e12f41482b55a19a8cf27ba243855f382c277cbb\n" +
                "e12f41482b55a19a8cf27ba243855f382c277cbb 02612100c26a72840edbcd971dd3310b6e3f499e\n" +
                "02612100c26a72840edbcd971dd3310b6e3f499e\n";

            FollowParentRewriter rw = new FollowParentRewriter("dd/aa.txt", StaticReader(dataFollow, dataGraph));
            rw.RewriteNecessary.Should().BeTrue();
            rw.PushRevision(null);
            // 02_e1_57_2c_9e_56_be
            //         \_____/
            CheckFlush(rw, false, new GitRevision[] { null });
            //
            rw.PushRevision(MkGitRevision("bef6694d0b1a58243d817cc3e915952f53269b40", "5632365bb31f2da1aae700f47e1a50951754f9ff"));
            CheckFlush(rw, false, new GitRevision[] { });
            // 
            rw.PushRevision(MkGitRevision("5632365bb31f2da1aae700f47e1a50951754f9ff", "9eea2112f54967a17ae0f02fb096a4bc41200846", "5744afa2ee91a44eb4393220b19aa99a66459a3f"));
            CheckFlush(rw, false, new GitRevision[] { MkGitRevision("bef6694d0b1a58243d817cc3e915952f53269b40", "5632365bb31f2da1aae700f47e1a50951754f9ff") });
            //
            rw.PushRevision(MkGitRevision("9eea2112f54967a17ae0f02fb096a4bc41200846", "2cfff1e08548361ca16c3c05bb4407dc3a190ff7"));
            CheckFlush(rw, false, new GitRevision[] { });
            rw.PushRevision(MkGitRevision("2cfff1e08548361ca16c3c05bb4407dc3a190ff7", "5744afa2ee91a44eb4393220b19aa99a66459a3f"));
            CheckFlush(rw, false, new GitRevision[] { });
            //
            rw.PushRevision(MkGitRevision("5744afa2ee91a44eb4393220b19aa99a66459a3f", "e12f41482b55a19a8cf27ba243855f382c277cbb"));
            CheckFlush(rw, false, new GitRevision[] { 
                MkGitRevision("5632365bb31f2da1aae700f47e1a50951754f9ff", "9eea2112f54967a17ae0f02fb096a4bc41200846", "5744afa2ee91a44eb4393220b19aa99a66459a3f"),
                MkGitRevision("9eea2112f54967a17ae0f02fb096a4bc41200846", "2cfff1e08548361ca16c3c05bb4407dc3a190ff7"),
                MkGitRevision("2cfff1e08548361ca16c3c05bb4407dc3a190ff7", "5744afa2ee91a44eb4393220b19aa99a66459a3f")
            });
            //
            rw.PushRevision(MkGitRevision("e12f41482b55a19a8cf27ba243855f382c277cbb", "02612100c26a72840edbcd971dd3310b6e3f499e"));
            CheckFlush(rw, false, new GitRevision[] { 
                MkGitRevision("5744afa2ee91a44eb4393220b19aa99a66459a3f", "e12f41482b55a19a8cf27ba243855f382c277cbb")
            });
            //
            CheckFlush(rw, true, new GitRevision[] { 
                MkGitRevision("e12f41482b55a19a8cf27ba243855f382c277cbb", "02612100c26a72840edbcd971dd3310b6e3f499e")
            });
        }


        [Test]
        public void RewriterWholePathRemoved()
        {
            string dataFollow =
                "dd/aa.txt\n" +
                "\n" +
                "aa.txt\n";
            string dataGraph =
                "bef6694d0b1a58243d817cc3e915952f53269b40 5632365bb31f2da1aae700f47e1a50951754f9ff\n" +
                "5632365bb31f2da1aae700f47e1a50951754f9ff 9eea2112f54967a17ae0f02fb096a4bc41200846 5744afa2ee91a44eb4393220b19aa99a66459a3f\n" +
                "9eea2112f54967a17ae0f02fb096a4bc41200846 2cfff1e08548361ca16c3c05bb4407dc3a190ff7\n" +
                "2cfff1e08548361ca16c3c05bb4407dc3a190ff7 5744afa2ee91a44eb4393220b19aa99a66459a3f\n" +
                "5744afa2ee91a44eb4393220b19aa99a66459a3f e12f41482b55a19a8cf27ba243855f382c277cbb\n" +
                "e12f41482b55a19a8cf27ba243855f382c277cbb 02612100c26a72840edbcd971dd3310b6e3f499e\n" +
                "02612100c26a72840edbcd971dd3310b6e3f499e\n";

            FollowParentRewriter rw = new FollowParentRewriter("dd/aa.txt", StaticReader(dataFollow, dataGraph));
            rw.RewriteNecessary.Should().BeTrue();
            rw.PushRevision(null);
            // ==_e1_57_==_==_==_be
            //         \=====/
            CheckFlush(rw, false, new GitRevision[] { null });
            //
            rw.PushRevision(MkGitRevision("bef6694d0b1a58243d817cc3e915952f53269b40", "5632365bb31f2da1aae700f47e1a50951754f9ff"));
            CheckFlush(rw, false, new GitRevision[] { });
            // 56..2c skipped
            rw.PushRevision(MkGitRevision("5744afa2ee91a44eb4393220b19aa99a66459a3f", "e12f41482b55a19a8cf27ba243855f382c277cbb"));
            CheckFlush(rw, false, new GitRevision[] { 
                MkGitRevision("bef6694d0b1a58243d817cc3e915952f53269b40", "5744afa2ee91a44eb4393220b19aa99a66459a3f")
            });
            //
            rw.PushRevision(MkGitRevision("e12f41482b55a19a8cf27ba243855f382c277cbb", "02612100c26a72840edbcd971dd3310b6e3f499e"));
            CheckFlush(rw, false, new GitRevision[] { 
                MkGitRevision("5744afa2ee91a44eb4393220b19aa99a66459a3f", "e12f41482b55a19a8cf27ba243855f382c277cbb")
            });
            //
            CheckFlush(rw, true, new GitRevision[] { 
                MkGitRevision("e12f41482b55a19a8cf27ba243855f382c277cbb", "02612100c26a72840edbcd971dd3310b6e3f499e")
            });
        }

        [Test]
        public void RewriterTestBranchPathRemoved()
        {
            string dataFollow =
                "dd/aa.txt\n" +
                "\n" +
                "aa.txt\n";
            string dataGraph =
                "bef6694d0b1a58243d817cc3e915952f53269b40 5632365bb31f2da1aae700f47e1a50951754f9ff\n" +
                "5632365bb31f2da1aae700f47e1a50951754f9ff 9eea2112f54967a17ae0f02fb096a4bc41200846 5744afa2ee91a44eb4393220b19aa99a66459a3f\n" +
                "9eea2112f54967a17ae0f02fb096a4bc41200846 2cfff1e08548361ca16c3c05bb4407dc3a190ff7\n" +
                "2cfff1e08548361ca16c3c05bb4407dc3a190ff7 5744afa2ee91a44eb4393220b19aa99a66459a3f\n" +
                "5744afa2ee91a44eb4393220b19aa99a66459a3f e12f41482b55a19a8cf27ba243855f382c277cbb\n" +
                "e12f41482b55a19a8cf27ba243855f382c277cbb 02612100c26a72840edbcd971dd3310b6e3f499e\n" +
                "02612100c26a72840edbcd971dd3310b6e3f499e\n";

            FollowParentRewriter rw = new FollowParentRewriter("dd/aa.txt", StaticReader(dataFollow, dataGraph));
            rw.RewriteNecessary.Should().BeTrue();
            rw.PushRevision(null);
            // 02_e1_57_==_==_56_be
            //         \_____/     
            CheckFlush(rw, false, new GitRevision[] { null });
            //
            rw.PushRevision(MkGitRevision("bef6694d0b1a58243d817cc3e915952f53269b40", "5632365bb31f2da1aae700f47e1a50951754f9ff"));
            CheckFlush(rw, false, new GitRevision[] { });
            // 
            rw.PushRevision(MkGitRevision("5632365bb31f2da1aae700f47e1a50951754f9ff", "9eea2112f54967a17ae0f02fb096a4bc41200846", "5744afa2ee91a44eb4393220b19aa99a66459a3f"));
            CheckFlush(rw, false, new GitRevision[] { MkGitRevision("bef6694d0b1a58243d817cc3e915952f53269b40", "5632365bb31f2da1aae700f47e1a50951754f9ff") });
            // 9e, 2c skipped
            rw.PushRevision(MkGitRevision("5744afa2ee91a44eb4393220b19aa99a66459a3f", "e12f41482b55a19a8cf27ba243855f382c277cbb"));
            CheckFlush(rw, false, new GitRevision[] { 
                MkGitRevision("5632365bb31f2da1aae700f47e1a50951754f9ff", "5744afa2ee91a44eb4393220b19aa99a66459a3f")
            });
            //
            rw.PushRevision(MkGitRevision("e12f41482b55a19a8cf27ba243855f382c277cbb", "02612100c26a72840edbcd971dd3310b6e3f499e"));
            CheckFlush(rw, false, new GitRevision[] { 
                MkGitRevision("5744afa2ee91a44eb4393220b19aa99a66459a3f", "e12f41482b55a19a8cf27ba243855f382c277cbb")
            });
            //
            CheckFlush(rw, true, new GitRevision[] { 
                MkGitRevision("e12f41482b55a19a8cf27ba243855f382c277cbb", "02612100c26a72840edbcd971dd3310b6e3f499e")
            });
        }

        [Test]
        public void RewriterTestRedundantPath()
        {
            string dataFollow =
                "dd/aa.txt\n" +
                "\n" +
                "aa.txt\n";
            string dataGraph =
                "bef6694d0b1a58243d817cc3e915952f53269b40 5632365bb31f2da1aae700f47e1a50951754f9ff\n" +
                "5632365bb31f2da1aae700f47e1a50951754f9ff 9eea2112f54967a17ae0f02fb096a4bc41200846 5744afa2ee91a44eb4393220b19aa99a66459a3f\n" +
                "9eea2112f54967a17ae0f02fb096a4bc41200846 2cfff1e08548361ca16c3c05bb4407dc3a190ff7\n" +
                "2cfff1e08548361ca16c3c05bb4407dc3a190ff7 5744afa2ee91a44eb4393220b19aa99a66459a3f\n" +
                "5744afa2ee91a44eb4393220b19aa99a66459a3f e12f41482b55a19a8cf27ba243855f382c277cbb\n" +
                "e12f41482b55a19a8cf27ba243855f382c277cbb 02612100c26a72840edbcd971dd3310b6e3f499e\n" +
                "02612100c26a72840edbcd971dd3310b6e3f499e\n";

            FollowParentRewriter rw = new FollowParentRewriter("dd/aa.txt", StaticReader(dataFollow, dataGraph));
            rw.RewriteNecessary.Should().BeTrue();
            rw.PushRevision(null);
            CheckFlush(rw, false, new GitRevision[] { null });
            // 02_e1_57_2c_9e_==_be
            //         \=====/
            rw.PushRevision(MkGitRevision("bef6694d0b1a58243d817cc3e915952f53269b40", "5632365bb31f2da1aae700f47e1a50951754f9ff"));
            CheckFlush(rw, false, new GitRevision[] { });
            // 56 skipped
            rw.PushRevision(MkGitRevision("9eea2112f54967a17ae0f02fb096a4bc41200846", "2cfff1e08548361ca16c3c05bb4407dc3a190ff7"));
            CheckFlush(rw, false, new GitRevision[] {
                MkGitRevision("bef6694d0b1a58243d817cc3e915952f53269b40", "9eea2112f54967a17ae0f02fb096a4bc41200846")
            });
            rw.PushRevision(MkGitRevision("2cfff1e08548361ca16c3c05bb4407dc3a190ff7", "5744afa2ee91a44eb4393220b19aa99a66459a3f"));
            CheckFlush(rw, false, new GitRevision[] {
                MkGitRevision("9eea2112f54967a17ae0f02fb096a4bc41200846", "2cfff1e08548361ca16c3c05bb4407dc3a190ff7"),
            });
            rw.PushRevision(MkGitRevision("5744afa2ee91a44eb4393220b19aa99a66459a3f", "e12f41482b55a19a8cf27ba243855f382c277cbb"));
            CheckFlush(rw, false, new GitRevision[] { 
                MkGitRevision("2cfff1e08548361ca16c3c05bb4407dc3a190ff7", "5744afa2ee91a44eb4393220b19aa99a66459a3f"),
            });
            //
            rw.PushRevision(MkGitRevision("e12f41482b55a19a8cf27ba243855f382c277cbb", "02612100c26a72840edbcd971dd3310b6e3f499e"));
            CheckFlush(rw, false, new GitRevision[] { 
                MkGitRevision("5744afa2ee91a44eb4393220b19aa99a66459a3f", "e12f41482b55a19a8cf27ba243855f382c277cbb")
            });
            //
            CheckFlush(rw, true, new GitRevision[] { 
                MkGitRevision("e12f41482b55a19a8cf27ba243855f382c277cbb", "02612100c26a72840edbcd971dd3310b6e3f499e")
            });
        }

        [Test]
        public void RewriterTestSingleFileName()
        {
            string dataFollow =
                "dd/aa.txt\n" +
                "\n";
            string dataGraph = null;

            FollowParentRewriter rw = new FollowParentRewriter("dd/aa.txt", StaticReader(dataFollow, dataGraph));
            rw.RewriteNecessary.Should().BeFalse();
        }
    }
}
