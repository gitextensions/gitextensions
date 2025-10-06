using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json;

namespace GitCommandsTests.Git.Commands;

[TestFixture]
public class GetAllChangedFilesOutputParserTest
{
    [Test]
    [TestCase("status_modified_files", "#Header\03 unknown info\01 .M S..U 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c subm1\01 .M SCM. 160000 160000 160000 6bd3b036fc5718a51a0d27cde134c7019798c3ce 6bd3b036fc5718a51a0d27cde134c7019798c3ce subm2\0\r\nwarning: LF will be replaced by CRLF in adfs.h.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in dir.c.\nThe file will have its original line endings in your working directory.")]
    [TestCase("status_ignored_files", "1 .M N... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\0? untracked_file\0")]
    [TestCase("status_staged_files", "1 M. N... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\01 MM N... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c adfs.h\0")]
    [TestCase("status_untracked_files", "1 .M S... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c subm1\0! ignored_file\0")]
    [TestCase("status_with_spaces", "#Header\03 unknown info\01 .M N... 160000 160000 160000 cbca134e29be13b35f21ca4553ba04f796324b1c cbca134e29be13b35f21ca4553ba04f796324b1c  no trim space0 \01 .M SCM. 160000 160000 160000 6bd3b036fc5718a51a0d27cde134c7019798c3ce 6bd3b036fc5718a51a0d27cde134c7019798c3ce  no trim space1 \0\r\nwarning: LF will be replaced by CRLF in adfs.h.\nThe file will have its original line endings in your working directory.\nwarning: LF will be replaced by CRLF in dir.c.\nThe file will have its original line endings in your working directory.")]
    [TestCase("both_modified", "u UU N... 100644 100644 100644 100644 628b3ed138239cbf0cfd993deab272ef606ac34a bf42fd75847fdc76d4f4a2b89d92a1b289b2e4ea f7f9ab7c975ab68e9bd8bda93b8bb717987947ea GitUI/CommandsDialogs/FormFormatPatch.Designer.cs")]
    [TestCase("both_added", "u AA N... 000000 100644 100644 100644 0000000000000000000000000000000000000000 61780798228d17af2d34fce4cfbdf35556832472 78981922613b2afb6025042ff6bd878ac1994e85 t.t")]
    [TestCase("both_deleted", "u DD N... 100644 000000 000000 000000 3c70853f1ed9b82635f0763dd1373c3101e79d5d 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 GitUI/CommandsDialogs/FormFormatPatch.cs")]
    [TestCase("added_by_us", "u AU N... 000000 100644 000000 100644 0000000000000000000000000000000000000000 3c70853f1ed9b82635f0763dd1373c3101e79d5d 0000000000000000000000000000000000000000 a2")]
    [TestCase("added_by_them", "u UA N... 000000 000000 100644 100644 0000000000000000000000000000000000000000 0000000000000000000000000000000000000000 3c70853f1ed9b82635f0763dd1373c3101e79d5d b2")]
    [TestCase("deleted_by_them", "u UD N... 100644 100644 000000 100644 f7f9ab7c975ab68e9bd8bda93b8bb717987947ea bf42fd75847fdc76d4f4a2b89d92a1b289b2e4ea 0000000000000000000000000000000000000000 GitUI/CommandsDialogs/FormFormatPatch.Designer.cs")]
    [TestCase("deleted_by_us", "u DU N... 100644 000000 100644 100644 dc7a0a8364df7cb022b3e29a48ecd84992af4613 0000000000000000000000000000000000000000 bf42fd75847fdc76d4f4a2b89d92a1b289b2e4ea GitUI/CommandsDialogs/FormFormatPatch.Designer.cs")]
    public async Task TestGetStatusChangedFilesFromString(string testName, string statusString)
    {
        // TODO produce a valid working directory
        GitModule module = new(Path.GetTempPath());
        GetAllChangedFilesOutputParser getAllChangedFilesOutputParser = new(() => module);

        // git status --porcelain=2 --untracked-files=no -z
        IReadOnlyList<GitItemStatus> statuses = getAllChangedFilesOutputParser.Parse(statusString);
        await Verifier.VerifyJson(JsonConvert.SerializeObject(statuses))
            .UseParameters(testName);
    }

    [Test]
    public async Task TestGetDefaultStatus()
    {
        GitItemStatus item = GitItemStatus.GetDefaultStatus("filename.txt");

        await Verifier.VerifyJson(JsonConvert.SerializeObject(item));
    }

    private const string _rawinfo = ":100644 100644 96b438fc ffe29e27 ";

    [Test]
    [TestCase("Ignore_unmerged_in_conflict_if_revision_is_work_tree", StagedStatus.WorkTree,
        $"{_rawinfo}M\0testfile.txt\0{_rawinfo}U\0testfile.txt\0")]
    [TestCase("Include_unmerged_in_conflict_if_revision_is_index", StagedStatus.Index,
        $"{_rawinfo}M\0testfile.txt\0{_rawinfo}U\0testfile2.txt\0")]
    [TestCase("Check_that_the_staged_status_is_None_if_not_IndexWorkTree1", StagedStatus.None,
        $"{_rawinfo}M\0testfile.txt\0{_rawinfo}U\0testfile2.txt\0")]
    [TestCase("Check_that_the_staged_status_is_None_if_not_IndexWorkTree2", StagedStatus.None,
        $"{_rawinfo}M\0testfile.txt\0{_rawinfo}U\0testfile2.txt\0")]

    [TestCase("Check_that_spaces_are_not_trimmed_in_file_names", StagedStatus.None,
        $"{_rawinfo}M\0 no trim space0 \0{_rawinfo}U\0 no trim space1 \0{_rawinfo}A\0 no trim space2 \0")]
    [TestCase("Rename_with_spaces", StagedStatus.None,
        $"{_rawinfo}R100\0CONTRIBUTING.md\0 CONTRIBUTI NG.md\0{_rawinfo}C70\0apa.md\0 apa.md\0{_rawinfo}A\0 co decov.yml\0{_rawinfo}D\0CODE_OF_CONDUCT.md\0")]
#if !DEBUG && false
    // This test is for documentation, but as the throw is in a called function, it will not test cleanly
    // Check that the staged status is None if not Index/WorkTree
    // Assertion in Debug, throws in Release
    [TestCase("Check_that_the_staged_status_is_None_if_not_IndexWorkTree3", StagedStatus.None,
        "M\0testfile.txt\0U\0testfile2.txt\0")]
#endif
    public async Task GetDiffChangedFilesFromString(string testName, StagedStatus stagedStatus, string statusString)
    {
        // TODO produce a valid working directory
        GitModule module = new(Path.GetTempPath());
        // git diff --find-renames --find-copies -z --raw
        List<GitItemStatus> statuses = module.GetTestAccessor().GetDiffChangedFilesFromString(statusString, stagedStatus);

        await Verifier.VerifyJson(JsonConvert.SerializeObject(statuses))
            .UseParameters(testName);
    }
}
