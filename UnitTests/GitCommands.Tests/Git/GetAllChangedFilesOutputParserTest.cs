using ApprovalTests;
using ApprovalTests.Namers;
using GitCommands;
using GitCommands.Git;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GitCommandsTests.Git.Commands
{
    [TestFixture]
    public class GetAllChangedFilesOutputParserTest
    {
        [Test]
        [TestCase("status modified files", "#Header\03 unknown info\01 .M S..U 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c subm1\01 .M SCM. 160000 160000 160000 6bd3b036fc5718a51a0d27cde134c7019798c3ce 6bd3b036fc5718a51a0d27cde134c7019798c3ce subm2\0\r\nwarning: LF will be replaced by CRLF in adfs.h.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in dir.c.\nThe file will have its original line endings in your working directory.")]
        [TestCase("status ignored files", "1 .M N... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\0? untracked_file\0")]
        [TestCase("status staged files", "1 M. N... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\01 MM N... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\0")]
        [TestCase("status untracked files", "1 .M S... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c subm1\0! ignored_file\0")]
        [TestCase("status with spaces", "#Header\03 unknown info\01 .M N... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c  no trim space0 \01 .M SCM. 160000 160000 160000 6bd3b036fc5718a51a0d27cde134c7019798c3ce 6bd3b036fc5718a51a0d27cde134c7019798c3ce  no trim space1 \0\r\nwarning: LF will be replaced by CRLF in adfs.h.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in dir.c.\nThe file will have its original line endings in your working directory.")]
        [TestCase("both modified", "u UU N... 100644 100644 100644 100644 628b3ed138239cbf0cfd993deab272ef606ac34a bf42fd75847fdc76d4f4a2b89d92a1b289b2e4ea f7f9ab7c975ab68e9bd8bda93b8bb717987947ea GitUI/CommandsDialogs/FormFormatPatch.Designer.cs")]
        [TestCase("both added", "u AA N... 000000 100644 100644 100644 0000000000000000000000000000000000000000 61780798228d17af2d34fce4cfbdf35556832472 78981922613b2afb6025042ff6bd878ac1994e85 t.t")]
        [TestCase("both deleted", "u DD N... 100644 000000 000000 000000 3c70853f1ed9b82635f0763dd1373c3101e79d5d 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 GitUI/CommandsDialogs/FormFormatPatch.cs")]
        [TestCase("added by us", "u AU N... 000000 100644 000000 100644 0000000000000000000000000000000000000000 3c70853f1ed9b82635f0763dd1373c3101e79d5d 0000000000000000000000000000000000000000 a2")]
        [TestCase("added by them", "u UA N... 000000 000000 100644 100644 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 3c70853f1ed9b82635f0763dd1373c3101e79d5d b2")]
        [TestCase("deleted by them", "u UD N... 100644 100644 000000 100644 f7f9ab7c975ab68e9bd8bda93b8bb717987947ea bf42fd75847fdc76d4f4a2b89d92a1b289b2e4ea 0000000000000000000000000000000000000000 GitUI/CommandsDialogs/FormFormatPatch.Designer.cs")]
        [TestCase("deleted by us", "u DU N... 100644 000000 100644 100644 dc7a0a8364df7cb022b3e29a48ecd84992af4613 0000000000000000000000000000000000000000 bf42fd75847fdc76d4f4a2b89d92a1b289b2e4ea GitUI/CommandsDialogs/FormFormatPatch.Designer.cs")]

        // Note that it is not expected that fatal: is mixed with proper info, but parsing should handle
        [TestCase("fatal error",
    "? unknown info\0fatal: bad config line 1 in file F:/dev/gc/gitextensions/.git/modules/GitExtensionsDoc/config\nfatal: 'git status --porcelain=2' failed in submodule GitExtensionsDoc\n1 MM N... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\0")]
        public void TestGetStatusChangedFilesFromString(string testName, string statusString)
        {
            // TODO produce a valid working directory
            GitModule module = new(Path.GetTempPath());
            GetAllChangedFilesOutputParser getAllChangedFilesOutputParser = new(() => module);

            using (ApprovalResults.ForScenario(testName.Replace(' ', '_')))
            {
                // git status --porcelain=2 --untracked-files=no -z
                var statuses = getAllChangedFilesOutputParser.Parse(statusString);
                Approvals.VerifyJson(JsonConvert.SerializeObject(statuses));
            }
        }
    }
}
