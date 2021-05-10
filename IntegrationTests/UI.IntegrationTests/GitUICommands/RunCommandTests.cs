using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitExtensions.UITests;
using GitUI;
using GitUI.CommandsDialogs;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUITests.GitUICommandsTests
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public sealed class RunCommandTests
    {
        // Created once for the fixture
        private ReferenceRepository _referenceRepository;

        // Created once for each test
        private GitUICommands _commands;

        [SetUp]
        public void SetUp()
        {
            if (_referenceRepository is null)
            {
                _referenceRepository = new ReferenceRepository();

                string cmdPath = (Environment.GetEnvironmentVariable("COMSPEC") ?? "C:/WINDOWS/system32/cmd.exe").ToPosixPath().QuoteNE();
                _referenceRepository.Module.GitExecutable.RunCommand($"config --local difftool.cmd.path {cmdPath}").Should().BeTrue();
                _referenceRepository.Module.GitExecutable.RunCommand($"config --local mergetool.cmd.path {cmdPath}").Should().BeTrue();
                _referenceRepository.Module.GitExecutable.RunCommand("config --local diff.guitool cmd").Should().BeTrue();
                _referenceRepository.Module.GitExecutable.RunCommand("config --local merge.guitool cmd").Should().BeTrue();

                AppSettings.UseConsoleEmulatorForCommands = false;
                AppSettings.CloseProcessDialog = true;
            }
            else
            {
                _referenceRepository.Reset();
            }

            _commands = new GitUICommands(_referenceRepository.Module);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            AppSettings.SetDocumentationBaseUrl("master");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _referenceRepository.Dispose();
        }

        [Test]
        public void RunCommandBasedOnArgument_should_throw_on_null_args()
        {
            ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(null)))
                .Should().Throw<NullReferenceException>();
        }

        [Test]
        public void RunCommandBasedOnArgument_should_throw_on_empty_args()
        {
            ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(new string[] { })))
                .Should().Throw<ArgumentOutOfRangeException>();

            ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(new string[] { "ge.exe" })))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void RunCommandBasedOnArgument_about()
        {
            UserEnvironmentInformation.Initialise("1234567", isDirty: true);
            RunCommandBasedOnArgument<FormAbout>(new string[] { "ge.exe", "about" });
        }

        [TestCase("add")]
        [TestCase("addfiles")]
        public void RunCommandBasedOnArgument_add(string command)
            => RunCommandBasedOnArgument<FormAddFiles>(new string[] { "ge.exe", command });

        [TestCase("apply")]
        [TestCase("applypatch")]
        public void RunCommandBasedOnArgument_apply(string command)
            => RunCommandBasedOnArgument<FormApplyPatch>(new string[] { "ge.exe", command });

        [Test]
        public void RunCommandBasedOnArgument_blame_throws()
        {
            ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(new string[] { "ge.exe", "blame" })))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void RunCommandBasedOnArgument_blame()
            => RunCommandBasedOnArgument<FormBlame>(new string[] { "ge.exe", "blame", "filename" });

        [Test]
        public void RunCommandBasedOnArgument_blame_line()
            => RunCommandBasedOnArgument<FormBlame>(new string[] { "ge.exe", "blame", "filename", "42" });

        [Test]
        public void RunCommandBasedOnArgument_branch()
        {
            _referenceRepository.CheckoutMaster();
            RunCommandBasedOnArgument<FormCreateBranch>(new string[] { "ge.exe", "branch" }, runTest: form =>
            {
                SetText(form, "BranchNameTextBox", "branchname");
                ClickButton(form, "Ok");
            });
        }

        [Test]
        public void RunCommandBasedOnArgument_browse()
        {
            var selected = ObjectId.Random();
            var first = ObjectId.Random();
            var otherIgnored = ObjectId.Random();
            RunCommandBasedOnArgument<FormBrowse>(new string[] { "ge.exe", "browse", $"-commit={selected},{first},,{otherIgnored}" });
        }

        [TestCase("checkout")]
        [TestCase("checkoutbranch")]
        public void RunCommandBasedOnArgument_checkout(string command)
            => RunCommandBasedOnArgument<FormCheckoutBranch>(new string[] { "ge.exe", command }, expectedResult: false);

        [Test]
        public void RunCommandBasedOnArgument_checkoutrevision()
            => RunCommandBasedOnArgument<FormCheckoutRevision>(new string[] { "ge.exe", "checkoutrevision" }, expectedResult: false);

        [Test]
        public void RunCommandBasedOnArgument_cherry()
            => RunCommandBasedOnArgument<FormCherryPick>(new string[] { "ge.exe", "cherry" }, expectedResult: false);

        [Test]
        public void RunCommandBasedOnArgument_cleanup()
            => RunCommandBasedOnArgument<FormCleanupRepository>(new string[] { "ge.exe", "cleanup" });

        [Test]
        public void RunCommandBasedOnArgument_clone()
            => RunCommandBasedOnArgument<FormClone>(new string[] { "ge.exe", "clone" });

        [Test]
        public void RunCommandBasedOnArgument_commit()
            => RunCommandBasedOnArgument<FormCommit>(new string[] { "ge.exe", "commit" });

        [Test]
        public void RunCommandBasedOnArgument_difftool_throws()
        {
            ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(new string[] { "ge.exe", "difftool" })))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void RunCommandBasedOnArgument_difftool()
            => _commands.GetTestAccessor().RunCommandBasedOnArgument(new string[] { "ge.exe", "difftool", "filename" }).Should().BeTrue();

        [Test]
        public void RunCommandBasedOnArgument_history_throws(
            [Values("blamehistory", "filehistory")] string command)
        {
            ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(new string[] { "ge.exe", command })))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void RunCommandBasedOnArgument_history(
            [Values("blamehistory", "filehistory")] string command,
            [Values(false, true)] bool commit,
            [Values(false, true)] bool filter)
        {
            var expectedTab = command.Contains("blame") ? "Blame" : "Diff";

            System.Collections.Generic.List<string> args = new() { "ge.exe", command, "filename" };
            if (commit)
            {
                args.Add(_referenceRepository.CommitHash);
                if (filter)
                {
                    args.Add("--filter-by-revision");
                }
            }

            RunCommandBasedOnArgument<FormFileHistory>(args.ToArray(),
                runTest: form => form.FindDescendantOfType<FullBleedTabControl>(_ => true).SelectedTab.Text.Should().Be(expectedTab));
        }

        [Test]
        public void RunCommandBasedOnArgument_history_returns_false(
            [Values("blamehistory", "filehistory")] string command,
            [Values(0, 1, 2, 3)] int invalidVariant)
        {
            const bool ignored = false;
            (bool commitValid, bool filter, bool filterValid)[] invalidVariants =
            {
                (false, false, ignored),
                (false, true, false),
                (false, true, true),
                (true, true, false)
            };
            (bool commitValid, bool filter, bool filterValid) = invalidVariants[invalidVariant];

            System.Collections.Generic.List<string> args = new() { "ge.exe", command, "filename" };
            args.Add(commitValid ? _referenceRepository.CommitHash : "no-commit");
            if (filter)
            {
                args.Add(filterValid ? "--filter-by-revision" : "invalid");
            }

            _commands.GetTestAccessor().RunCommandBasedOnArgument(args.ToArray()).Should().BeFalse();
        }

        [Test]
        public void RunCommandBasedOnArgument_fileeditor_throws()
        {
            ((Action)(() => _commands.GetTestAccessor().RunCommandBasedOnArgument(new string[] { "ge.exe", "fileeditor" })))
                .Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void RunCommandBasedOnArgument_fileeditor()
            => RunCommandBasedOnArgument<FormEditor>(new string[] { "ge.exe", "fileeditor", "filename" });

        [Test]
        public void RunCommandBasedOnArgument_formatpatch()
            => RunCommandBasedOnArgument<FormFormatPatch>(new string[] { "ge.exe", "formatpatch" });

        [Test]
        public void RunCommandBasedOnArgument_gitignore()
            => RunCommandBasedOnArgument<FormGitIgnore>(new string[] { "ge.exe", "gitignore" });

        [Test]
        public void RunCommandBasedOnArgument_init()
            => RunCommandBasedOnArgument<FormInit>(new string[] { "ge.exe", "init" });

        [Test]
        public void RunCommandBasedOnArgument_merge()
            => RunCommandBasedOnArgument<FormMergeBranch>(new string[] { "ge.exe", "merge" });

        [TestCase("mergeconflicts")]
        [TestCase("mergetool")]
        public void RunCommandBasedOnArgument_mergeconflicts(string command)
            => RunCommandBasedOnArgument<FormResolveConflicts>(new string[] { "ge.exe", command });

        [Test]
        public void RunCommandBasedOnArgument_openrepo()
          => RunCommandBasedOnArgument<FormBrowse>(new string[] { "ge.exe", "openrepo" });

        [Test]
        public void RunCommandBasedOnArgument_pull()
            => RunCommandBasedOnArgument<FormPull>(new string[] { "ge.exe", "pull" }, expectedResult: false);

        [Test]
        public void RunCommandBasedOnArgument_push()
            => RunCommandBasedOnArgument<FormPush>(new string[] { "ge.exe", "push" }, expectedResult: false);

        [Test]
        public void RunCommandBasedOnArgument_rebase()
            => RunCommandBasedOnArgument<FormRebase>(new string[] { "ge.exe", "rebase" });

        [Test]
        public void RunCommandBasedOnArgument_remotes()
            => RunCommandBasedOnArgument<FormRemotes>(new string[] { "ge.exe", "remotes" });

        [TestCase("revert")]
        [TestCase("reset")]
        public void RunCommandBasedOnArgument_reset(string command)
            => RunCommandBasedOnArgument<FormResetChanges>(new string[] { "ge.exe", command }, expectedResult: false);

        [Test]
        public void RunCommandBasedOnArgument_searchfile()
            => RunCommandBasedOnArgument<SearchWindow<string>>(new string[] { "ge.exe", "searchfile" }, expectedResult: false);

        [Ignore("FormSettings throws unhandled exceptions when opened this way, works from real command line")]
        [Test]
        public void RunCommandBasedOnArgument_settings()
            => RunCommandBasedOnArgument<FormSettings>(new string[] { "ge.exe", "settings" });

        [Test]
        public void RunCommandBasedOnArgument_stash()
            => RunCommandBasedOnArgument<FormStash>(new string[] { "ge.exe", "stash" });

        [Test]
        public void RunCommandBasedOnArgument_synchronize()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                while (true)
                {
                    await UITest.WaitForIdleAsync();
                    var formPull = Application.OpenForms.OfType<FormPull>().FirstOrDefault();
                    if (formPull is not null)
                    {
                        formPull.Close();
                        break;
                    }
                }

                while (true)
                {
                    await UITest.WaitForIdleAsync();
                    var formPush = Application.OpenForms.OfType<FormPush>().FirstOrDefault();
                    if (formPush is not null)
                    {
                        formPush.Close();
                        break;
                    }
                }
            });

            RunCommandBasedOnArgument<FormCommit>(new string[] { "ge.exe", "synchronize" }, expectedResult: false);
        }

        [Test]
        public void RunCommandBasedOnArgument_tag()
            => RunCommandBasedOnArgument<FormCreateTag>(new string[] { "ge.exe", "tag" }, expectedResult: false);

        [Test]
        public void RunCommandBasedOnArgument_viewdiff()
            => RunCommandBasedOnArgument<FormLog>(new string[] { "ge.exe", "viewdiff" }, expectedResult: false);

        [TestCase("git://")]
        [TestCase("http://")]
        [TestCase("https://")]
        [TestCase("github-windows://openRepo/")]
        [TestCase("github-mac://openRepo/")]
        public void RunCommandBasedOnArgument_Url(string url)
            => RunCommandBasedOnArgument<FormClone>(new string[] { "ge.exe", url });

        [TestCase("")]
        [TestCase(" ")]
        [TestCase("help")]
        [TestCase("nonsense")]
        public void RunCommandBasedOnArgument_unsupported(string command)
            => RunCommandBasedOnArgument<FormCommandlineHelp>(new string[] { "ge.exe", command });

        private void RunCommandBasedOnArgument<TForm>(string[] args, bool expectedResult = true, Action<TForm> runTest = null) where TForm : Form
        {
            UITest.RunForm<TForm>(
                showForm: () => _commands.GetTestAccessor().RunCommandBasedOnArgument(args).Should().Be(expectedResult),
                runTestAsync: form =>
                {
                    runTest?.Invoke(form);

                    return Task.CompletedTask;
                });
        }

        private static void ClickButton(Form form, string buttonName)
            => form.FindDescendantOfType<Button>(button => button.Name == buttonName).PerformClick();

        private static void SetText(Form form, string controlName, string value)
            => form.FindDescendantOfType<Control>(control => control.Name == controlName).Text = value;
    }
}
