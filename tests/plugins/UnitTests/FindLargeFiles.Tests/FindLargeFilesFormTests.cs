using GitCommands;
using GitExtensions.Plugins.FindLargeFiles;

namespace DeleteUnusedBranchesTests
{
    [TestFixture]
    public class FindLargeFilesFormTests
    {
        [Test]
        public async Task GenerateCommand_without_deletions_should_return_expected()
        {
            string original = AppSettings.GitCommandValue;
            try
            {
                AppSettings.GitCommandValue = @"C:\Program Files\Git\bin\git.exe";
                await Verifier.Verify(FindLargeFilesForm.GetTestAccessor().GenerateCommand([]));
            }
            finally
            {
                AppSettings.GitCommandValue = original;
            }
        }

        [Test]
        public async Task GenerateCommand_with_deletions_should_return_expected()
        {
            string original = AppSettings.GitCommandValue;
            try
            {
                AppSettings.GitCommandValue = @"C:\Program Files\Git\bin\git.exe";
                SortableObjectsList gitObjects =
                    [
                        new GitObject("sha1", "intune/packages/file.intunewin", 1, "commit") { Delete = true },
                        new GitObject("sha1", "intune/packages/file with spaces.intunewin", 1, "commit") { Delete = true },
                        new GitObject("sha1", "intune/packages/XL Upload/xl-upload.intunewin", 1, "commit") { Delete = true },
                        new GitObject("sha1", "readme.md", 1, "commit"),
                    ];

                await Verifier.Verify(FindLargeFilesForm.GetTestAccessor().GenerateCommand(gitObjects));
            }
            finally
            {
                AppSettings.GitCommandValue = original;
            }
        }
    }
}
