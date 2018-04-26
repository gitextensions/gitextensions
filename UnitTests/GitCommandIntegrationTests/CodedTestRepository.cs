using System;
using System.IO;

namespace GitCommandIntegrationTests
{
    internal class CodedTestRepository : TestRepository
    {
        // time must be fixed to get deterministic hashes
        private static readonly DateTimeOffset When = new DateTimeOffset(2018, 1, 2, 3, 4, 5, 6, TimeSpan.Zero);

        protected override void CreateContent()
        {
            LibGit2Sharp.Repository.Init(ContentTargetPath);
            using (var repository = new LibGit2Sharp.Repository(ContentTargetPath))
            {
                CreateRepoFile("A.txt", "A");
                repository.Index.Add("A.txt");

                var message = "A commit message";
                var author = new LibGit2Sharp.Signature("GitUITests", "unittests@gitextensions.com", When);
                var committer = author;
                var options = new LibGit2Sharp.CommitOptions();
                repository.Commit(message, author, committer, options);
            }
        }

        private void CreateRepoFile(string filename, string content)
        {
            File.WriteAllText(Path.Combine(ContentTargetPath, filename), content);
        }
    }
}