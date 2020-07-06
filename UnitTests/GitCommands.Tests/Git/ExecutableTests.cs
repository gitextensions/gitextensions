using FluentAssertions;
using GitCommands;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitCommandsTests.Git
{
    [TestFixture]
    public sealed class ExecutableTests
    {
        private const string _invalidExe = "invalid.exe";

        private ExternalOperationException _externalOperationException;

        private void RecordExecutableException(ExternalOperationException ex, ExternalOperationExceptionFactory.Handling exceptionHandling)
            => _externalOperationException = ex as ExternalOperationException;

        [SetUp]
        public void SetUp()
        {
            _externalOperationException = null;
            ExternalOperationExceptionFactory.Default.OnException += RecordExecutableException;
        }

        [TearDown]
        public void TearDown()
        {
            ExternalOperationExceptionFactory.Default.OnException -= RecordExecutableException;
        }

        [Test]
        public void StartNonexisting()
        {
            IExecutable executable = new Executable(_invalidExe);

            ExternalOperationException ex = Assert.Throws<ExternalOperationException>(() => executable.Start());

            ex.Command.Should().StartWith(_invalidExe);
            ex.Should().BeSameAs(_externalOperationException);
        }

        [Test]
        public void GetOutputNonexisting()
        {
            IExecutable executable = new Executable(_invalidExe);

            ExternalOperationException ex = Assert.Throws<ExternalOperationException>(() => executable.GetOutput(""));

            ex.Command.Should().StartWith(_invalidExe);
            ex.Should().BeSameAs(_externalOperationException);
        }

        [Test]
        public void StartExistingInInvalidDir()
        {
            string executableName = "cmd.exe";
            string workingDirectory = @"C:\nonexistent-dir";
            IExecutable executable = new Executable(executableName, workingDirectory);

            ExternalOperationException ex = Assert.Throws<ExternalOperationException>(() => executable.Start());

            ex.Command.Should().StartWith(executableName);
            ex.WorkingDirectory.Should().Be(workingDirectory);
            ex.Should().BeSameAs(_externalOperationException);
        }
    }
}
