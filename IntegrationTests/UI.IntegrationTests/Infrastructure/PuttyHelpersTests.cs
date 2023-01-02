using System.Runtime.InteropServices;
using FluentAssertions;
using GitCommands;
using GitUI;
using GitUI.Infrastructure;
using GitUI.NBugReports;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;

namespace GitUITests.Infrastructure
{
    [Apartment(ApartmentState.STA)]
    [TestFixture]
    public sealed class PuttyHelpersTests
    {
        private const string Plink = "plink.exe";

        [Test]
        public async Task StartPageantIfConfigured_throw_if_not_UI_thread()
        {
            await TaskScheduler.Default.SwitchTo();

            ((Action)(() => PuttyHelpers.StartPageantIfConfigured(() => null))).Should().Throw<COMException>();
        }

        [Test]
        public Task StartPageantIfConfigured_throw_if_not_Plink()
        {
            return RunTestAsync(
                () => ((Action)(() => PuttyHelpers.StartPageantIfConfigured(() => null)))
                    .Should()
                    .Throw<UserExternalOperationException>()
                    .WithMessage(TranslatedStrings.ErrorSshPuTTYNotConfigured),
                pageant: "foo.exe");
        }

        [Test]
        public Task StartPageantIfConfigured_throw_if_Pageant_not_found()
        {
            return RunTestAsync(
                () => ((Action)(() => PuttyHelpers.StartPageantIfConfigured(() => null)))
                    .Should()
                    .Throw<UserExternalOperationException>()
                    .WithMessage(TranslatedStrings.ErrorFileNotFound)
                    .WithInnerException<FileNotFoundException>(),
                pageant: Plink);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public Task StartPageantIfConfigured_returns_false_if_key_empty(string bogusKey)
        {
            // Copy notepad.exe from %SYSTEM32%\notepad.exe to be our plink.exe
            string notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);
            string plinkfile = Path.Combine(tempPath, Plink);
            File.Copy(notepadPath, plinkfile, overwrite: true);

            try
            {
                return RunTestAsync(
                    () => PuttyHelpers.StartPageantIfConfigured(() => bogusKey).Should().BeFalse(),
                    pageant: plinkfile);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempPath, recursive: true);
                }
                catch
                {
                    // no-op
                }
            }
        }

        [Test]
        public Task StartPageantIfConfigured_throws_if_key_not_empty_but_absent()
        {
            // Copy notepad.exe from %SYSTEM32%\notepad.exe to be our plink.exe
            string notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);
            string plinkfile = Path.Combine(tempPath, Plink);
            File.Copy(notepadPath, plinkfile, overwrite: true);

            try
            {
                return RunTestAsync(
                    () => ((Action)(() => PuttyHelpers.StartPageantIfConfigured(() => "bogus.cer")))
                        .Should()
                        .Throw<UserExternalOperationException>()
                        .WithMessage(TranslatedStrings.ErrorSshKeyNotFound)
                        .WithInnerException<FileNotFoundException>(),
                    pageant: plinkfile);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempPath, recursive: true);
                }
                catch
                {
                    // no-op
                }
            }
        }

        private static async Task RunTestAsync(Action action, string pageant)
        {
            Form form = new();
            await form.SwitchToMainThreadAsync();
            form.Dispose();

            string configuredSshPath = AppSettings.SshPath;
            string configuredPageant = AppSettings.Pageant;

            try
            {
                AppSettings.SshPath = pageant;
                AppSettings.Pageant = pageant;
                action();
            }
            finally
            {
                AppSettings.SshPath = configuredSshPath;
                AppSettings.Pageant = configuredPageant;
            }
        }
    }
}
