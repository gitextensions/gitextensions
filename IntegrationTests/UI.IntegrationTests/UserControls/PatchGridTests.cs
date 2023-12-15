using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.UITests;
using GitUI;
using NSubstitute;

namespace UITests.UserControls
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public sealed class PatchGridTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            ReferenceRepository.ResetRepo(ref _referenceRepository);

            _commands = new GitUICommands(GlobalServiceContainer.CreateDefaultMockServiceContainer(), _referenceRepository.Module);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void RebasePatchesBuiltAccordingToGitRebaseFiles_EnsureRebaseFileFormatWithGit()
        {
            RunPatchGridTest(patchGrid =>
            {
                string content = "line1" + Environment.NewLine;
                string mainFile = "main_file.txt";
                CommitData initialCommit = CreateCommit("main: initial commit", content, mainFile);
                content += "line2" + Environment.NewLine;
                CreateCommit("main: commit2", content, mainFile);
                content += "line3" + Environment.NewLine;
                CommitData tipOfMainBranchCommit = CreateCommit("main: commit3", content, mainFile);

                // Create a branch with contents that will conflict on rebase
                _referenceRepository.CreateBranch("branch2", initialCommit.Hash);
                _referenceRepository.CheckoutBranch("branch2");
                string otherFile = "other_file.txt";
                CommitData commitDone = CreateCommit("branch2: commit that will apply successfully", "other_line1", otherFile);
                CommitData commitApplying = CreateCommit("branch2: commit that will fail to rebase", "a content that will create a conflict", mainFile);
                CommitData commitToDo = CreateCommit("branch2: commit todo", "foobar", otherFile);

                // Real git rebase to ensure git rebase file format has not changed
                ExecutionResult execResult = _referenceRepository.Module.GitExecutable.Execute($"rebase {tipOfMainBranchCommit.Hash}", throwOnErrorExit: false);
                execResult.ExitCode.Should().NotBe(0); // Expecting conflicts!

                IReadOnlyList<PatchFile> patches = patchGrid.GetTestAccessor().GetInteractiveRebasePatchFiles();

                patches.Count.Should().Be(3);
                VerifyPatch(patches[0], "pick", commitDone, isApplied: true);
                VerifyPatch(patches[1], "pick", commitApplying, isApplied: false, isNext: true);
                VerifyPatch(patches[2], "pick", commitToDo, isApplied: false);

                CommitData CreateCommit(string commitMessage, string content, string filename)
                {
                    string hash = _referenceRepository.CreateCommit(commitMessage, content, filename);
                    return new CommitData(hash, commitMessage, $"{hash} {commitMessage}", ObjectId.Parse(hash));
                }

                return Task.CompletedTask;
            });
        }

        [Test]
        public void RebasePatchesBuiltAccordingToGitRebaseFiles()
        {
            RunPatchGridTest(patchGrid =>
            {
                CommitData commitDonePicked = CreateCommit("commit done: pick");
                CommitData commitDoneDelete = CreateCommit("commit done: drop");
                CommitData commitDoneLineRemoved = CreateCommit("commit done: line removed i.e drop");
                CommitData commitApplying = CreateCommit("commit in progress");
                CommitData commitToDoPicked = CreateCommit("commit todo: picked");
                CommitData commitToDoDelete = CreateCommit("commit todo: drop");
                CommitData commitToDoLineRemoved = CreateCommit("commit todo: line removed i.e drop");

                // Carefully crafted git rebase files to test different actions (pick, drop, line moved, line removed)
                Directory.CreateDirectory(Path.Combine(_referenceRepository.Module.WorkingDirGitDir, "rebase-merge"));
                _referenceRepository.CreateRepoFile(".git/rebase-merge/done", @$"drop {commitDoneDelete.RebaseLine}
pick {commitDonePicked.RebaseLine}
pick {commitApplying.RebaseLine}");
                _referenceRepository.CreateRepoFile(".git/rebase-merge/stopped-sha", commitApplying.Hash);
                _referenceRepository.CreateRepoFile(".git/rebase-merge/git-rebase-todo", @$"drop {commitToDoDelete.RebaseLine}
pick {commitToDoPicked.RebaseLine}");

                IReadOnlyList<PatchFile> patches = patchGrid.GetTestAccessor().GetInteractiveRebasePatchFiles();

                patches.Count.Should().Be(5);
                VerifyPatch(patches[0], "drop", commitDoneDelete, isApplied: true); // Line moved up "during interactive rebase"
                VerifyPatch(patches[1], "pick", commitDonePicked, isApplied: true);
                VerifyPatch(patches[2], "pick", commitApplying, isApplied: false, isNext: true);
                VerifyPatch(patches[3], "drop", commitToDoDelete, isApplied: false); // Line moved up "during interactive rebase"
                VerifyPatch(patches[4], "pick", commitToDoPicked, isApplied: false);

                CommitData CreateCommit(string commitMessage)
                {
                    string hash = _referenceRepository.CreateCommit(commitMessage);
                    return new CommitData(hash, commitMessage, $"{hash} {commitMessage}", ObjectId.Parse(hash));
                }

                return Task.CompletedTask;
            });
        }

        private static void VerifyPatch(PatchFile patchFile, string action, CommitData commit, bool isApplied, bool isNext = false)
            => patchFile.Should().BeEquivalentTo(
                new PatchFile
                {
                    Action = action,
                    ObjectId = commit.ObjectId,
                    IsApplied = isApplied,
                    IsNext = isNext,
                    Subject = commit.CommitMessage,
                    Author = ReferenceRepository.AuthorFullIdentity
                },
                options => options.Excluding(x => x.Date));

        private void RunPatchGridTest(Func<PatchGrid, Task> runTestAsync)
        {
            UITest.RunControl(
                createControl: form =>
                {
                    IGitUICommandsSource uiCommandsSource = Substitute.For<IGitUICommandsSource>();
                    uiCommandsSource.UICommands.Returns(x => _commands);

                    form.Size = new(600, 480);

                    return new PatchGrid
                    {
                        Dock = DockStyle.Fill,
                        Parent = form,
                        UICommandsSource = uiCommandsSource,
                    };
                },
                runTestAsync: async patchGrid =>
                {
                    // Wait for pending operations so the Control is loaded completely before testing it
                    await AsyncTestHelper.JoinPendingOperationsAsync(AsyncTestHelper.UnexpectedTimeout);

                    await runTestAsync(patchGrid);
                });
        }
    }

    internal record struct CommitData(string Hash, string CommitMessage, string RebaseLine, ObjectId ObjectId)
    {
    }
}
