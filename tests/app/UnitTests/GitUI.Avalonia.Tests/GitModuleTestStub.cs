using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using NSubstitute;

namespace GitExtensionsTests;

internal static class GitModuleTestStub
{
    public static void ConfigureEmptyRepositoryReads(IGitModule module)
    {
        module.GetEffectiveSettings().Returns(Substitute.For<SettingsSource>());
        module.GitCommandRunner.RunDetached(CancellationToken.None).ReturnsForAnyArgs(_ =>
        {
            StreamReader output = new(Stream.Null);
            IProcess process = Substitute.For<IProcess>();
            process.StandardOutput.Returns(output);
            process.StandardError.Returns(string.Empty);
            process.WaitForExit().Returns(0);
            process.WaitForExitAsync().Returns(Task.FromResult(0));
            process.WaitForExitAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(0));
            process.When(instance => instance.Dispose()).Do(_ => output.Dispose());
            return process;
        });
    }
}
