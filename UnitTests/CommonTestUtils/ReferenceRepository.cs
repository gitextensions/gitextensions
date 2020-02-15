﻿using System;
using System.Linq;
using GitCommands;
using GitCommands.Config;
using LibGit2Sharp;
using Remote = LibGit2Sharp.Remote;

namespace CommonTestUtils
{
    public class ReferenceRepository : IDisposable
    {
        private static GitModuleTestHelper _moduleTestHelper;
        private static string _commitHash;

        public ReferenceRepository()
        {
            _moduleTestHelper = new GitModuleTestHelper();

            CreateCommit("A commit message", "A");
        }

        public GitModule Module => _moduleTestHelper.Module;

        public string CommitHash => _commitHash;

        private string Commit(Repository repository, string commitMessage)
        {
            var author = new LibGit2Sharp.Signature("GitUITests", "unittests@gitextensions.com", DateTimeOffset.Now);
            var committer = author;
            var options = new LibGit2Sharp.CommitOptions() { PrettifyMessage = false };
            var commit = repository.Commit(commitMessage, author, committer, options);
            return commit.Id.Sha;
        }

        public void CreateCommit(string commitMessage, string content = null)
        {
            using (var repository = new LibGit2Sharp.Repository(_moduleTestHelper.Module.WorkingDir))
            {
                _moduleTestHelper.CreateRepoFile("A.txt", content ?? commitMessage);
                repository.Index.Add("A.txt");

                _commitHash = Commit(repository, commitMessage);
            }
        }

        public void CheckoutRevision()
        {
            using (var repository = new LibGit2Sharp.Repository(Module.WorkingDir))
            {
                Commands.Checkout(repository, CommitHash, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
            }
        }

        public void CheckoutMaster()
        {
            using (var repository = new LibGit2Sharp.Repository(Module.WorkingDir))
            {
                Commands.Checkout(repository, "master", new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
            }
        }

        public void CreateRemoteForMasterBranch()
        {
            using (var repository = new LibGit2Sharp.Repository(Module.WorkingDir))
            {
                repository.Network.Remotes.Add("origin", "http://useless.url");
                Remote remote = repository.Network.Remotes["origin"];

                var masterBranch = repository.Branches["master"];

                repository.Branches.Update(masterBranch,
                    b => b.Remote = remote.Name,
                    b => b.UpstreamBranch = masterBranch.CanonicalName);
            }
        }

        public void Reset()
        {
            // Undo potential impact from earlier tests
            using (var repository = new LibGit2Sharp.Repository(Module.WorkingDir))
            {
                var options = new LibGit2Sharp.CheckoutOptions();
                repository.Reset(LibGit2Sharp.ResetMode.Hard, (LibGit2Sharp.Commit)repository.Lookup(CommitHash, LibGit2Sharp.ObjectType.Commit), options);
                repository.RemoveUntrackedFiles();

                var remoteNames = repository.Network.Remotes.Select(remote => remote.Name).ToArray();
                foreach (var remoteName in remoteNames)
                {
                    repository.Network.Remotes.Remove(remoteName);
                }

                repository.Config.Set(SettingKeyString.UserName, "author");
                repository.Config.Set(SettingKeyString.UserEmail, "author@mail.com");
            }

            new CommitMessageManager(Module.WorkingDirGitDir, Module.CommitEncoding).ResetCommitMessage();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _moduleTestHelper.Dispose();
        }
    }
}
