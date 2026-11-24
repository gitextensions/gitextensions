using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Avalonia.Headless.NUnit;
using GitExtensions.Extensibility;
using GitUI;
using GitUI.CommandsDialogs;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class VisualStudioDiscoveryTests
{
    [AvaloniaTest]
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public async Task Init_should_discover_the_latest_supported_edition_including_previews(bool installerExists, bool queryAvailability)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using AppSettingsTestScope settings = new();
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.VisualStudioDiscovery-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        string installer = Path.Combine(directory, "vswhere.exe");
        string developmentEnvironment = Path.Combine(directory, "Preview", "devenv.exe");
        if (installerExists)
        {
            File.WriteAllText(installer, string.Empty);
        }

        // Re-run the real static initializer without changing the application's cached discovery state.
        AssemblyLoadContext context = new($"VisualStudioDiscovery-{Guid.NewGuid():N}", isCollectible: true);
        context.Resolving += (_, name) => AssemblyLoadContext.Default.Assemblies
            .FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name));
        try
        {
            Assembly assembly = context.LoadFromAssemblyPath(typeof(FormBrowse).Assembly.Location);
            Type boundary = assembly.GetType("GitUI.Compat.VisualStudioDiscovery", throwOnError: true)!;
            TaskCompletionSource locationRead = new(TaskCreationOptions.RunContinuationsAsynchronously);
            boundary.GetField("VsWherePath", BindingFlags.Static | BindingFlags.NonPublic)!
                .SetValue(null, (Func<string>)(() =>
                {
                    locationRead.TrySetResult();
                    return installer;
                }));
            IExecutable executable = Substitute.For<IExecutable>();
            IProcess process = Substitute.For<IProcess>();
            using StreamReader output = new(new MemoryStream(Encoding.UTF8.GetBytes(developmentEnvironment + Environment.NewLine)));
            process.StandardOutput.Returns(output);
            process.WaitForExitAsync().Returns(Task.FromResult(0));
            process.When(instance => instance.Dispose()).Do(_ => output.Dispose());
            TaskCompletionSource<ArgumentString> invocation = new(TaskCreationOptions.RunContinuationsAsynchronously);
            executable.Start(Arg.Any<ArgumentString>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(),
                Arg.Any<Encoding?>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(call =>
                {
                    invocation.TrySetResult(call.Arg<ArgumentString>());
                    return process;
                });
            int factories = 0;
            string? executablePath = null;
            boundary.GetField("CreateExecutable", BindingFlags.Static | BindingFlags.NonPublic)!
                .SetValue(null, (Func<string, IExecutable>)(path =>
                {
                    Interlocked.Increment(ref factories);
                    executablePath = path;
                    return executable;
                }));
            Type integration = assembly.GetType("GitUI.VisualStudioIntegration", throwOnError: true)!;
            PropertyInfo installed = integration.GetProperty("IsVisualStudioInstalled", BindingFlags.Static | BindingFlags.Public)!;
            if (queryAvailability)
            {
                _ = installed.GetValue(null);
            }
            else
            {
                integration.GetMethod("Init", BindingFlags.Static | BindingFlags.Public)!.Invoke(null, null);
            }

            await locationRead.Task.WaitAsync(TimeSpan.FromSeconds(15));
            if (installerExists)
            {
                ArgumentString arguments = await invocation.Task.WaitAsync(TimeSpan.FromSeconds(15));
                arguments.ToString().Should().Be("-prerelease -latest -requires Microsoft.VisualStudio.Product.Enterprise -requires Microsoft.VisualStudio.Product.Professional -requires Microsoft.VisualStudio.Product.Community -requiresAny -property productPath");
                using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
                while (!(bool)installed.GetValue(null)!)
                {
                    await Task.Delay(10, deadline.Token);
                }

                factories.Should().Be(1);
                executablePath.Should().Be(installer);
                integration.GetField("_devEnvPath", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)
                    .Should().Be(developmentEnvironment + Environment.NewLine);
                executable.Received(1).Start(arguments, createWindow: false, redirectInput: false,
                    redirectOutput: true, Arg.Any<Encoding?>(), useShellExecute: false, throwOnErrorExit: true);
            }
            else
            {
                ((bool)installed.GetValue(null)!).Should().BeFalse();
                factories.Should().Be(0);
                executable.DidNotReceiveWithAnyArgs().Start();
            }
        }
        finally
        {
            context.Unload();
            TestDirectory.Delete(directory);
        }
    }
}
