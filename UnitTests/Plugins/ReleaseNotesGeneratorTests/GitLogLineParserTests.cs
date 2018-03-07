using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ReleaseNotesGenerator;

namespace ReleaseNotesGeneratorTests
{
    [TestFixture]
    public class GitLogLineParserTests
    {
        private IGitLogLineParser _parser;

        [SetUp]
        public void Setup()
        {
            _parser = new GitLogLineParser();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void Parse_line_should_return_null(string line)
        {
            _parser.Parse(line).Should().BeNull();
        }

        [Test]
        public void Parse_line_should_return_null_non_matching_line()
        {
            string line = "234_42sfsd@asfkmsa askflsak ";
            _parser.Parse(line).Should().BeNull();
        }

        [TestCase("0824@RevisionDiffProvider Release tests", "0824", "RevisionDiffProvider Release tests")]
        [TestCase("0824e058c@RevisionDiffProvider Release tests", "0824e058c", "RevisionDiffProvider Release tests")]
        [TestCase("0824e058c0123@RevisionDiffProvider Release tests", "0824e058c0123", "RevisionDiffProvider Release tests")]
        [TestCase("0824e058c@RevisionDiffProvider@ Release tests", "0824e058c", "RevisionDiffProvider@ Release tests")]
        public void Parse_line_should_parse_correctly(string line, string expectedHash, string expectedMessage)
        {
            var logLine = _parser.Parse(line);

            logLine.Should().NotBeNull();
            logLine.Commit.Should().Be(expectedHash);
            logLine.MessageLines[0].Should().Be(expectedMessage);
        }

        [Test]
        public void Parse_lines_should_return_empty_list_if_null()
        {
            _parser.Parse((string[])null).Should().BeEmpty();
        }

        [Test]
        public void Parse_lines_should_parse_correctly()
        {
            string log = @"7895ec59f@Merge remote-tracking branch 'gitextensions/master' into feature-4157/n4031-compare-arificial-commits
77fc3cb50@Reset to artificial commits require special handling (#4175)* #4031 Reset to artificial commits require special handling

Reset to Unstaged commits does not make sense at all and should be hidden in the GUI
Reset to Staged will require special handling of GitRevision. IndexGuid (Compare to RevisionDiffProvider, options to git-diff)

* Separate ""artificial count"" from normal subject to limit special handling for ArtificialCountEnabled

This will also simplify update of artificial count as Count button
As a bonus, do not list changed files if artificial count is not updated

0824e058c@RevisionDiffProvider Release tests
57ab3ff2d6546@Review comment: RevisionDiffProvider is made into instance class
f95f6c61a@Review comment: simplify GetDiffFiles() unstaged files
ef98f@Review comment: .FirstOrDefault, some simplifications
4aeaa0340@Merge remote-tracking branch 'gitextensions/master' into feature-4157/n4031-compare-arificial-commits
11c8c59ed@Stash name size (#4173)#4120 Size of named stash is too small
c133bf689@Merge pull request #4178 from gerhardol/bugfix/n4168-static-readonlyChange hotkey settings names from const to static readonly
12af4ff7d@Change hotkey settings names from const to static readonlyReview comment in #4168

6761425e0@Merge pull request #4168 from gerhardol/feature/n4031-browsediff-hotkey-deleteBrowseDiff Hotkey support: DEL to delete unstaged files
a96153803@Merge pull request #4124 from Gua-naiko-che/#3907_Choose_branch_orderAllow to choose branch order between alphabetical and by date
e3cf76aa5@Add a setting to choose if branches in the dropdown from Browse Form should be ordered by date or alphabetically.
e524762d9@Merge pull request #4165 from gerhardol/bugfix/n4098-revisiontree-context-menuRevisionFileTree context menu gave exceptions if no items were Selected
414a69768@Merge pull request #4167 from gerhardol/bugfix/n4031-revisiondiff-menu-noselectedBrowse Diff Menu Items should be disabled when no item is SelectedDiff
5e9ac449c@BrowseDiff Hotkey support: DEL to delete unstaged filesPart of #4031
Some reimplementation needed after the split to RevisionDiff
Currently only used to delete files, should be added for at least stage/unstage and reset too.

eb945eff4@Browse Diff Menu Items should be disabled when no item is SelectedDiffFrom #4031The actions did not cause any exceptions, but were confusing to the user, it seemed like they had selected files
Reproduce by right clicking in the open space in a Diff panel with at least one file

a2b77d4b9@FormBrowse Commands in toolbar menu raised exceptions for artificial : (#4166)* FormBrowse Commands in toolbar menu raised exceptions for artificial commits and no revisions at all

Related to #4031 and somewhat to #4098

The irrelevant commands are disabled
There are some existing checks for bareRepositories in InternalInitialize() (where some init code is running...) that al
so could be removed after this (some were missing from that menu). checkoutBranchToolStripMenuItem is a little special t";

            var logLines = _parser.Parse(log.Replace("\r\n", "\n").Split('\n')).ToList();

            logLines.Count.Should().Be(18);

            var line = logLines.SingleOrDefault(l => l.Commit == "77fc3cb50");
            line.Should().NotBeNull();
            line.MessageLines.Count.Should().Be(10);

            line = logLines.SingleOrDefault(l => l.Commit == "57ab3ff2d6546");
            line.Should().NotBeNull();
            line.MessageLines.Count.Should().Be(1);

            line = logLines.SingleOrDefault(l => l.Commit == "ef98f");
            line.Should().NotBeNull();
            line.MessageLines.Count.Should().Be(1);

            line = logLines.SingleOrDefault(l => l.Commit == "eb945eff4");
            line.Should().NotBeNull();
            line.MessageLines.Count.Should().Be(3);
        }
    }
}