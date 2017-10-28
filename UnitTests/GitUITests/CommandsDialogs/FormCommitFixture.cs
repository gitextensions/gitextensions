using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class FormCommitFixture
    {
        [Test, RequiresThread(ApartmentState.STA)]
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
    }
}
