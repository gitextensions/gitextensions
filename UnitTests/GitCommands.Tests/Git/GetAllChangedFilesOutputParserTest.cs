using System.IO;
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
