using System.Text;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Patches;
using GitUIPluginInterfaces;

namespace GitCommandsTests
{
    [TestFixture]
    public sealed class GitModuleTests
    {
        [Test]
        public void RebasePatchesBuiltAccordingToGitRebaseFiles_EnsureRebaseFileFormatWithGit()
        {
            ReferenceRepository refRepo = new();
            var gitModule = refRepo.Module;
            Console.WriteLine("Repo path: " + gitModule.WorkingDir);

            string content = "line1" + Environment.NewLine;
            string mainFile = "main_file.txt";
            var initialCommit = CreateCommit("main: initial commit", content, mainFile);
            content += "line2" + Environment.NewLine;
            CreateCommit("main: commit2", content, mainFile);
            content += "line3" + Environment.NewLine;
            CreateCommit("main: commit3", content, mainFile);

            // Create a branch with contents that will conflict on rebase
            refRepo.CreateBranch("branch2", initialCommit.Hash);
            refRepo.CheckoutBranch("branch2");
            string otherFile = "other_file.txt";
            var commitDone = CreateCommit("branch2: commit that will apply successfully", "other_line1", otherFile);
            var commitApplying = CreateCommit("branch2: commit that will fail to rebase", "a content that will create a conflict", mainFile);
            var commitToDo = CreateCommit("branch2: commit todo", "foobar", otherFile);

            // Real git rebase to ensure git rebase file format has not changed
            var execResult = gitModule.GitExecutable.Execute($"rebase master", throwOnErrorExit: false);
            execResult.ExitCode.Should().NotBe(0); // Expecting conflicts!

            var patches = gitModule.GetInteractiveRebasePatchFiles();

            patches.Count.Should().Be(3);
            VerifyPatch(patches[0], "pick", commitDone, isApplied: true);
            VerifyPatch(patches[1], "pick", commitApplying, isApplied: false, isNext: true);
            VerifyPatch(patches[2], "pick", commitToDo, isApplied: false);

            CommitData CreateCommit(string commitMessage, string content, string filename)
            {
                string hash = refRepo.CreateCommit(commitMessage, content, filename);
                return new CommitData(hash, commitMessage, $"{hash} {commitMessage}", ObjectId.Parse(hash));
            }
        }

        [Test]
        public void RebasePatchesBuiltAccordingToGitRebaseFiles()
        {
            ReferenceRepository refRepo = new();
            var commitDonePicked = CreateCommit("commit done: pick");
            var commitDoneDelete = CreateCommit("commit done: drop");
            var commitDoneLineRemoved = CreateCommit("commit done: line removed i.e drop");
            var commitApplying = CreateCommit("commit in progress");
            var commitToDoPicked = CreateCommit("commit todo: picked");
            var commitToDoDelete = CreateCommit("commit todo: drop");
            var commitToDoLineRemoved = CreateCommit("commit todo: line removed i.e drop");
            var gitModule = refRepo.Module;

            // Carefully crafted git rebase files to test different actions (pick, drop, line moved, line removed)
            Directory.CreateDirectory(Path.Combine(gitModule.WorkingDirGitDir, "rebase-merge"));
            refRepo.CreateRepoFile(".git/rebase-merge/done", @$"drop {commitDoneDelete.RebaseLine}
pick {commitDonePicked.RebaseLine}
pick {commitApplying.RebaseLine}");
            refRepo.CreateRepoFile(".git/rebase-merge/stopped-sha", commitApplying.Hash);
            refRepo.CreateRepoFile(".git/rebase-merge/git-rebase-todo", @$"drop {commitToDoDelete.RebaseLine}
pick {commitToDoPicked.RebaseLine}");

            var patches = gitModule.GetInteractiveRebasePatchFiles();

            patches.Count.Should().Be(5);
            VerifyPatch(patches[0], "drop", commitDoneDelete, isApplied: true); // Line moved up "during interactive rebase"
            VerifyPatch(patches[1], "pick", commitDonePicked, isApplied: true);
            VerifyPatch(patches[2], "pick", commitApplying, isApplied: false, isNext: true);
            VerifyPatch(patches[3], "drop", commitToDoDelete, isApplied: false); // Line moved up "during interactive rebase"
            VerifyPatch(patches[4], "pick", commitToDoPicked, isApplied: false);

            CommitData CreateCommit(string commitMessage)
            {
                string hash = refRepo.CreateCommit(commitMessage);
                return new CommitData(hash, commitMessage, $"{hash} {commitMessage}", ObjectId.Parse(hash));
            }
        }

        private void VerifyPatch(PatchFile patchFile, string action, CommitData commit, bool isApplied, bool isNext = false)
            => patchFile.Should().BeEquivalentTo(
                new PatchFile { Action = action, ObjectId = commit.ObjectId, IsApplied = isApplied, IsNext = isNext, Subject = commit.CommitMessage, Author = "GitUITests <unittests@gitextensions.com>" },
                options => options.Excluding(x => x.Date));
    }

    internal record struct CommitData(string Hash, string CommitMessage, string RebaseLine, ObjectId ObjectId)
    {
    }
}
