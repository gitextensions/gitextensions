using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.CommitDialog;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class FormCommitFixture
    {
        [Test, RequiresThread(ApartmentState.STA), Explicit]
        public void DoesNotCrashWithTextBoxWrappedLines()
        {
            AppSettings.CommitValidationMaxCntCharsPerLine = 80;
            AppSettings.CommitValidationAutoWrap = true;
            var gitDir = TestContext.CurrentContext.TestDirectory;
            var crasherMessage =
@"Title

shouldWrapInTextBoxWithWordWrabSetToTrueAndWontHardWrapForLackOfWhiteSpacesThisCouldBeALongPathOrAnUrlmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm
shouldWrapInTextBoxWithWordWrabSetToTrueAndWontHardWrapForLackOfWhiteSpacesThisCouldBeALongPathOrAnUrlmmmmmmmmmmmmmmmmmmmmmmmmmmmmmmm
thrashed1
thrashed2
this one is more than 80 chars, and will hard-wrap, thrashing the previous two lines
now this line is out of range
";
            File.WriteAllText(Path.Combine(gitDir, "COMMITMESSAGE"), crasherMessage);
            using (var commitForm = new FormCommit(new GitUICommands(gitDir)))
            {
                Exception thrown = null;
                Application.ThreadException += (_, ex) => thrown = ex.Exception;
                commitForm.Should().NotBeNull();
                Task.Delay(500).ContinueWith(_ => commitForm.Close());
                commitForm.ShowDialog();
                thrown.Should().BeNull();
            }
        }

        public class CommitMessageUpdateTestCase
        {
            public bool AutoWrap { get; set; } = true;
            public int FirstLine { get; set; } = 10;
            public int OtherLines { get; set; } = 15;
            public bool SecondLineEmpty { get; set; } = true;
            public bool Indent { get; set; } = true;
            public string[] InitialLines { get; set; } = new string[0];
            public string[] Changes { get; set; } = new string[0];
            public int CursorLine { get; set; } = 0;
            public int CursorColumn { get; set; } = 0;
            public FormattedCommitMessage.MessageUpdate ExpectedUpdate { get; set; } = FormattedCommitMessage.MessageUpdate.NoChange;
            public string[] ExpectedFormattedLines { get; set; }

            private string _description;
            public CommitMessageUpdateTestCase(string description)
            {
                _description = description;
            }

            public override string ToString()
            {
                return _description;
            }
        }

        private static FormattedCommitMessage.MessageUpdate FullChange = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(0, 0);
        public static CommitMessageUpdateTestCase[] CommitMessageUpdates = new[]
        {
            new CommitMessageUpdateTestCase("Empty"),
            new CommitMessageUpdateTestCase("Only correct title") { Changes = new[] { "Title" } },
            new CommitMessageUpdateTestCase("Correct title with comment before") { Changes = new[] { "# a comment", "Title" } },
            new CommitMessageUpdateTestCase("Correct title with comment after") { Changes = new[] { "Title", "# a comment" } },
            new CommitMessageUpdateTestCase("Correct title and body with comment 1") { Changes = new[] { "# a comment", "Title", "", "body" } },
            new CommitMessageUpdateTestCase("Correct title and body with comment 2") { Changes = new[] { "Title", "# a comment", "", "body" } },
            new CommitMessageUpdateTestCase("Correct title and body with comment 3") { Changes = new[] { "Title", "", "# a comment", "body" } },
            new CommitMessageUpdateTestCase("Only too long title")
            {
                Changes = new[] { "Title which is too long" },
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateColorUpdate(new[] { new FormattedCommitMessage.ColoredLine(0, 10, 13) })
            },
            new CommitMessageUpdateTestCase("Missing second line with indent")
            {
                Changes = new[] { "Title", "not empty" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", " - not empty" }
            },
            new CommitMessageUpdateTestCase("Missing second line with indent that makes the line too long")
            {
                Changes = new[] { "Title", "not empty line" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", " - not empty", "   line" }
            },
            new CommitMessageUpdateTestCase("Missing second line among 3 with bullet")
            {
                Changes = new[] { "Title", "not empty", "other line" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", " - not empty", "other line" }
            },
            new CommitMessageUpdateTestCase("Missing second line without bullet")
            {
                Indent = false,
                Changes = new[] { "Title", "not empty" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "not empty" }
            },
            new CommitMessageUpdateTestCase("Line too long without wrapping")
            {
                AutoWrap = false,
                Changes = new[] { "Title", "", "a line which is too long" },
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateColorUpdate(new[] { new FormattedCommitMessage.ColoredLine(2, 15, 9) })
            },
            new CommitMessageUpdateTestCase("Last line too long")
            {
                Changes = new[] { "Title", "", "a line which is too long" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "a line which is", "too long" }
            },
            new CommitMessageUpdateTestCase("Last line too long with indent")
            {
                Changes = new[] { "Title", "", "  a line which is too long" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "  a line which", "  is too long" }
            },
            new CommitMessageUpdateTestCase("Line too long that can wrap to next line")
            {
                Changes = new[] { "Title", "", "a line which is too long", "next" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "a line which is", "too long next" }
            },
            new CommitMessageUpdateTestCase("Line too long that can wrap to next line with indent")
            {
                Changes = new[] { "Title", "", "  a line which is long", "  next" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "  a line which", "  is long next" }
            },
            new CommitMessageUpdateTestCase("Line too long with indent and comment following")
            {
                Changes = new[] { "Title", "", "  a line which is too long", "# a comment" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "  a line which", "  is too long", "# a comment" }
            },
            new CommitMessageUpdateTestCase("Line too long with indent and different indent following 1")
            {
                Changes = new[] { "Title", "", "  a line which is too long", "no indent" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "  a line which", "  is too long", "no indent" }
            },
            new CommitMessageUpdateTestCase("Line too long with indent and different indent following 2")
            {
                Changes = new[] { "Title", "", "  a line which is too long", "    big indent" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "  a line which", "  is too long", "    big indent" }
            },
            new CommitMessageUpdateTestCase("Line too long with empty line following")
            {
                Changes = new[] { "Title", "", "a line which is too long", "", "other line" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "a line which is", "too long", "", "other line" }
            },
            new CommitMessageUpdateTestCase("Lines too short")
            {
                OtherLines = 20,
                Changes = new[] { "Title", "", "a", "short", "line", "other line" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "a short line other", "line" }
            },
            new CommitMessageUpdateTestCase("Lines too short would fill without space")
            {
                Changes = new[] { "Title", "", "1234567890", "12345", "", "other line" },
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.NoChange
            },
            new CommitMessageUpdateTestCase("Lines too short with indent")
            {
                OtherLines = 20,
                Changes = new[] { "Title", "", "  a", "  short", "  line", "  other line" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "  a short line other", "  line" }
            },
            new CommitMessageUpdateTestCase("Lines too short with empty line")
            {
                OtherLines = 20,
                Changes = new[] { "Title", "", "a", "short", "line", "", "other line" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "a short line", "", "other line" }
            },
            new CommitMessageUpdateTestCase("Lines too short with indent and different indent following")
            {
                OtherLines = 20,
                Changes = new[] { "Title", "", "  a", "  short", "  line", "other line" },
                ExpectedUpdate = FullChange,
                ExpectedFormattedLines = new[] { "Title", "", "  a short line", "other line" }
            },
            new CommitMessageUpdateTestCase("Space at cursor not removed if not at limit")
            {
                InitialLines = new[] { "Title", "", "a short line" },
                Changes = new[] { "Title", "", "a short line " },
                CursorLine = 2, CursorColumn = 13,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.NoChange
            },
            new CommitMessageUpdateTestCase("Space at cursor at limit on last line replaced with new line")
            {
                InitialLines = new[] { "Title", "", "a short line ab" },
                Changes = new[] { "Title", "", "a short line ab " },
                CursorLine = 2, CursorColumn = 16,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(3, 0),
                ExpectedFormattedLines = new[] { "Title", "", "a short line ab", "" }
            },
            new CommitMessageUpdateTestCase("Space at cursor at limit on last line replaced with new line with ident")
            {
                InitialLines = new[] { "Title", "", "   a short line" },
                Changes = new[] { "Title", "", "   a short line " },
                CursorLine = 2, CursorColumn = 16,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(3, 3),
                ExpectedFormattedLines = new[] { "Title", "", "   a short line", "   " }
            },
            new CommitMessageUpdateTestCase("Space at cursor at limit with available next line moves cursor")
            {
                InitialLines = new[] { "Title", "", " a short line a", " next" },
                Changes = new[] { "Title", "", " a short line a ", " next" },
                CursorLine = 2, CursorColumn = 16,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(3, 1),
                ExpectedFormattedLines = new[] { "Title", "", " a short line a", " next" }
            },
            new CommitMessageUpdateTestCase("Space at cursor at limit with unavailable next line inserts line")
            {
                InitialLines = new[] { "Title", "", " a short line a", "next" },
                Changes = new[] { "Title", "", " a short line a ", "next" },
                CursorLine = 2, CursorColumn = 16,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(3, 1),
                ExpectedFormattedLines = new[] { "Title", "", " a short line a", " ", "next" }
            },
            new CommitMessageUpdateTestCase("Cursor after auto wrap")
            {
                InitialLines = new[] { "Title", "", "a short line ab" },
                Changes = new[] { "Title", "", "a short line abc" },
                CursorLine = 2, CursorColumn = 16,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(3, 3),
                ExpectedFormattedLines = new[] { "Title", "", "a short line", "abc" }
            },
            new CommitMessageUpdateTestCase("Cursor after auto wrap with line after")
            {
                InitialLines = new[] { "Title", "", "a short line ab", "next" },
                Changes = new[] { "Title", "", "a short line abc", "next" },
                CursorLine = 2, CursorColumn = 16,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(3, 3),
                ExpectedFormattedLines = new[] { "Title", "", "a short line", "abc next" }
            },
            new CommitMessageUpdateTestCase("Cursor after auto join next line")
            {
                InitialLines = new[] { "Title", "", "a short", "line" },
                Changes = new[] { "Title", "", "a short ", "line" },
                CursorLine = 2, CursorColumn = 8,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(2, 8),
                ExpectedFormattedLines = new[] { "Title", "", "a short line" }
            },
            new CommitMessageUpdateTestCase("Cursor after auto join previous line")
            {
                InitialLines = new[] { "Title", "", "a short", "lin" },
                Changes = new[] { "Title", "", "a short", "line" },
                CursorLine = 3, CursorColumn = 4,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(2, 12),
                ExpectedFormattedLines = new[] { "Title", "", "a short line" }
            },
            new CommitMessageUpdateTestCase("Cursor after auto join previous line with cursor before break")
            {
                InitialLines = new[] { "Title", "", "a short", "lne with more" },
                Changes = new[] { "Title", "", "a short", "line with more" },
                CursorLine = 3, CursorColumn = 2,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(2, 10),
                ExpectedFormattedLines = new[] { "Title", "", "a short line", "with more" }
            },
            new CommitMessageUpdateTestCase("Cursor after auto join previous line with cursor after break")
            {
                InitialLines = new[] { "Title", "", "a short", "line with mre" },
                Changes = new[] { "Title", "", "a short", "line with more" },
                CursorLine = 3, CursorColumn = 12,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(3, 7),
                ExpectedFormattedLines = new[] { "Title", "", "a short line", "with more" }
            },
            new CommitMessageUpdateTestCase("Cursor after auto join previous line with indent")
            {
                InitialLines = new[] { "Title", "", "  a short", "  " },
                Changes = new[] { "Title", "", "  a short", "  l" },
                CursorLine = 3, CursorColumn = 3,
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.CreateFullUpdate(2, 11),
                ExpectedFormattedLines = new[] { "Title", "", "  a short l" }
            },
            new CommitMessageUpdateTestCase("No join at bullet")
            {
                Changes = new[] { "Title", "", "a short", "- line" },
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.NoChange
            },
            new CommitMessageUpdateTestCase("No join at bullet with indent")
            {
                Changes = new[] { "Title", "", "  a short", "  - line" },
                ExpectedUpdate = FormattedCommitMessage.MessageUpdate.NoChange
            },
        };

        [TestCaseSource("CommitMessageUpdates")]
        public void FormatsCommitMessagesProperly(CommitMessageUpdateTestCase testCase)
        {
            AppSettings.CommitValidationMaxCntCharsFirstLine = testCase.FirstLine;
            AppSettings.CommitValidationMaxCntCharsPerLine = testCase.OtherLines;
            AppSettings.CommitValidationSecondLineMustBeEmpty = testCase.SecondLineEmpty;
            AppSettings.CommitValidationIndentAfterFirstLine = testCase.Indent;

            var formattedCommitMessage = new FormattedCommitMessage("#");
            // ensure verbatim for "arrange" (as this could really happen if settings are changed during edition)
            AppSettings.CommitValidationAutoWrap = false;
            formattedCommitMessage.UpdateLines(testCase.InitialLines, 0, 0); 

            AppSettings.CommitValidationAutoWrap = testCase.AutoWrap;
            var update = formattedCommitMessage.UpdateLines(testCase.Changes, testCase.CursorLine, testCase.CursorColumn);
            update.ShouldBeEquivalentTo(testCase.ExpectedUpdate);
            if (update.NeedFullUpdate)
                formattedCommitMessage.FormattedLines.Should().Equal(testCase.ExpectedFormattedLines);
        }
    }
}
