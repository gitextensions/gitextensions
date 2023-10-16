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
        private ReferenceRepository _refRepo;
        private GitModule _gitModule;

        [SetUp]
        public void SetUp()
        {
            _refRepo = new();
            _gitModule = _refRepo.Module;
            Console.WriteLine("Repo path: " + _gitModule.WorkingDir);
        }

        [Test]
        public void GetCommitCount_Should_manage_ambiguous_argument()
        {
            const string ambiguousName = "script";
            CommitData initialCommit = CreateCommitWithAmbiguousFolder("commit1");
            CreateCommitWithAmbiguousFolder("commit2");
            CommitData tipOfMainBranchCommit = CreateCommitWithAmbiguousFolder("commit3");

            // Create a branch with same name than a folder inside repo
            _refRepo.CreateBranch(ambiguousName, tipOfMainBranchCommit.Hash);

            int? exitCode = _gitModule.GetCommitCount(initialCommit.Hash, ambiguousName);

            exitCode.Should().Be(0);

            CommitData CreateCommitWithAmbiguousFolder(string commitMessage)
            {
                string hash = _refRepo.CreateCommitRelative(ambiguousName, "main_file.txt", commitMessage, commitMessage);
                return new CommitData(hash, commitMessage, $"{hash} {commitMessage}", ObjectId.Parse(hash));
            }
        }

        [Test]
        public void RebasePatchesBuiltAccordingToGitRebaseFiles_EnsureRebaseFileFormatWithGit()
        {
            string content = "line1" + Environment.NewLine;
            string mainFile = "main_file.txt";
            CommitData initialCommit = CreateCommit("main: initial commit", content, mainFile);
            content += "line2" + Environment.NewLine;
            CreateCommit("main: commit2", content, mainFile);
            content += "line3" + Environment.NewLine;
            CommitData tipOfMainBranchCommit = CreateCommit("main: commit3", content, mainFile);

            // Create a branch with contents that will conflict on rebase
            _refRepo.CreateBranch("branch2", initialCommit.Hash);
            _refRepo.CheckoutBranch("branch2");
            string otherFile = "other_file.txt";
            CommitData commitDone = CreateCommit("branch2: commit that will apply successfully", "other_line1", otherFile);
            CommitData commitApplying = CreateCommit("branch2: commit that will fail to rebase", "a content that will create a conflict", mainFile);
            CommitData commitToDo = CreateCommit("branch2: commit todo", "foobar", otherFile);

            // Real git rebase to ensure git rebase file format has not changed
            ExecutionResult execResult = _gitModule.GitExecutable.Execute($"rebase {tipOfMainBranchCommit.Hash}", throwOnErrorExit: false);
            execResult.ExitCode.Should().NotBe(0); // Expecting conflicts!

            IReadOnlyList<PatchFile> patches = _gitModule.GetInteractiveRebasePatchFiles();

            patches.Count.Should().Be(3);
            VerifyPatch(patches[0], "pick", commitDone, isApplied: true);
            VerifyPatch(patches[1], "pick", commitApplying, isApplied: false, isNext: true);
            VerifyPatch(patches[2], "pick", commitToDo, isApplied: false);

            CommitData CreateCommit(string commitMessage, string content, string filename)
            {
                string hash = _refRepo.CreateCommit(commitMessage, content, filename);
                return new CommitData(hash, commitMessage, $"{hash} {commitMessage}", ObjectId.Parse(hash));
            }
        }

        [Test]
        public void RebasePatchesBuiltAccordingToGitRebaseFiles()
        {
            CommitData commitDonePicked = CreateCommit("commit done: pick");
            CommitData commitDoneDelete = CreateCommit("commit done: drop");
            CommitData commitDoneLineRemoved = CreateCommit("commit done: line removed i.e drop");
            CommitData commitApplying = CreateCommit("commit in progress");
            CommitData commitToDoPicked = CreateCommit("commit todo: picked");
            CommitData commitToDoDelete = CreateCommit("commit todo: drop");
            CommitData commitToDoLineRemoved = CreateCommit("commit todo: line removed i.e drop");

            // Carefully crafted git rebase files to test different actions (pick, drop, line moved, line removed)
            Directory.CreateDirectory(Path.Combine(_gitModule.WorkingDirGitDir, "rebase-merge"));
            _refRepo.CreateRepoFile(".git/rebase-merge/done", @$"drop {commitDoneDelete.RebaseLine}
pick {commitDonePicked.RebaseLine}
pick {commitApplying.RebaseLine}");
            _refRepo.CreateRepoFile(".git/rebase-merge/stopped-sha", commitApplying.Hash);
            _refRepo.CreateRepoFile(".git/rebase-merge/git-rebase-todo", @$"drop {commitToDoDelete.RebaseLine}
pick {commitToDoPicked.RebaseLine}");

            IReadOnlyList<PatchFile> patches = _gitModule.GetInteractiveRebasePatchFiles();

            patches.Count.Should().Be(5);
            VerifyPatch(patches[0], "drop", commitDoneDelete, isApplied: true); // Line moved up "during interactive rebase"
            VerifyPatch(patches[1], "pick", commitDonePicked, isApplied: true);
            VerifyPatch(patches[2], "pick", commitApplying, isApplied: false, isNext: true);
            VerifyPatch(patches[3], "drop", commitToDoDelete, isApplied: false); // Line moved up "during interactive rebase"
            VerifyPatch(patches[4], "pick", commitToDoPicked, isApplied: false);

            CommitData CreateCommit(string commitMessage)
            {
                string hash = _refRepo.CreateCommit(commitMessage);
                return new CommitData(hash, commitMessage, $"{hash} {commitMessage}", ObjectId.Parse(hash));
            }
        }

        private void VerifyPatch(PatchFile patchFile, string action, CommitData commit, bool isApplied, bool isNext = false)
            => patchFile.Should().BeEquivalentTo(
                new PatchFile { Action = action, ObjectId = commit.ObjectId, IsApplied = isApplied, IsNext = isNext, Subject = commit.CommitMessage, Author = ReferenceRepository.AuthorFullIdentity },
                options => options.Excluding(x => x.Date));
    }

    internal record struct CommitData(string Hash, string CommitMessage, string RebaseLine, ObjectId ObjectId)
    {
    }
}
