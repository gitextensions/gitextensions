using System.Collections;
using System.ComponentModel.Design;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils;
using GitUI;
using GitUI.BuildServerIntegration;
using GitUI.CommandsDialogs;
using GitUI.HelperDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using GitUIPluginInterfaces.BuildServerIntegration;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class LegacyBuildServerCredentialTests
{
    private const string LegacyConfiguration = "[Credentials]\nBuildServerCredentialsType=UsernameAndPassword\nUsername=legacy-user\nPassword=legacy-password\nBearerToken=legacy-token\n";

    private JoinableTaskContext? _previousContext;

    [SetUp]
    public void SetUp() => _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;

    [TearDown]
    public void TearDown() => ThreadHelper.JoinableTaskContext = _previousContext!;

    [AvaloniaTest]
    [TestCase("stored")]
    [TestCase("edit")]
    [TestCase("cancel")]
    [TestCase("crypto")]
    [TestCase("ambiguous")]
    [TestCase("current")]
    [TestCase("unsupported")]
    [TestCase("empty")]
    public void Real_adapter_and_watcher_should_read_legacy_credentials_without_rewriting_them(string scenario)
    {
        using AppSettingsTestScope settings = new();
        using CredentialCacheScope credentialsCache = new();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.LegacyCredentials-{Guid.NewGuid():N}");
        string repository = Path.Combine(directory, "repository");
        string legacyRoot = Path.Combine(directory, "IsolatedStorage");
        string helperStore = Path.Combine(directory, "helper-store");
        string helperLog = Path.Combine(directory, "helper-log");
        Directory.CreateDirectory(repository);
        using ServiceContainer services = CreateServices();
        GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), repository);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
        string helper = Path.Combine(directory, "credential-helper.sh");
        string helperScript = $"printf '%s\\n' \"$1\" >> {ShellQuote(helperLog)}\ncase \"$1\" in\nget) if test -f {ShellQuote(helperStore)}; then cat {ShellQuote(helperStore)}; fi ;;\nstore) cat > {ShellQuote(helperStore)} ;;\n*) exit 2 ;;\nesac\n";
        File.WriteAllText(helper, helperScript, new UTF8Encoding(false));
        module.GitExecutable.RunCommand(new GitArgumentBuilder("config") { "--local", "--add", "credential.helper", "\"\"" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("config") { "--local", "--add", "credential.helper", $"!sh {ShellQuote(helper)}".Quote() }).Should().BeTrue();
        SettingsSource adapterSettings = Substitute.For<SettingsSource>();
        adapterSettings.GetValue("BuildServerUrl").Returns("https://legacy-ci.example.test");
        adapterSettings.GetValue("ProjectName").Returns("owned-project");
        adapterSettings.GetValue("IgnoreBuildBranch").Returns(string.Empty);
        using IBuildServerAdapter oldAdapter = CreateAdapter();
        IBuildServerWatcher oldCredentialIO = Substitute.For<IBuildServerWatcher>();
        oldCredentialIO.GetBuildServerCredentials(Arg.Any<IBuildServerAdapter>(), true)
            .Returns(new BuildServerCredentials { BuildServerCredentialsType = BuildServerCredentialsType.Guest });
        oldCredentialIO.ReplaceVariables(Arg.Any<string>()).Returns(call => call.Arg<string>());
        oldAdapter.Initialize(oldCredentialIO, adapterSettings, () => throw new AssertionException("Unexpected settings UI."));
        string key = oldAdapter.UniqueKey;
        Dictionary<string, byte[]> payloads = [];
        List<string> legacyFiles = [];
        if (scenario != "empty")
        {
            byte[] protectedData = [1, 7, 11];
            payloads[Convert.ToHexString(protectedData)] = EncodeOriginalConfiguration(LegacyConfiguration);
            legacyFiles.Add(WriteOriginalStore(legacyRoot, key, protectedData));
            if (scenario == "ambiguous")
            {
                byte[] other = [2, 7, 11];
                payloads[Convert.ToHexString(other)] = EncodeOriginalConfiguration(LegacyConfiguration.Replace("legacy-password", "different-password"));
                legacyFiles.Add(WriteOriginalStore(legacyRoot, key, other));
            }
        }

        Dictionary<string, byte[]> before = legacyFiles.ToDictionary(path => path, File.ReadAllBytes);
        Dictionary<string, DateTime> writeTimes = legacyFiles.ToDictionary(path => path, File.GetLastWriteTimeUtc);
        int rootReads = 0;
        int decryptions = 0;
        using IDisposable legacyIO = LegacyBuildServerCredentialStore.TestAccessor.UseExternalIO(
            () =>
            {
                rootReads++;
                return legacyRoot;
            },
            encrypted =>
            {
                decryptions++;
                if (scenario == "crypto")
                {
                    throw new CryptographicException("Owned unreadable legacy fixture.");
                }

                return payloads[Convert.ToHexString(encrypted)];
            },
            () => scenario != "unsupported");
        FormBrowse? browse = null;
        FormBuildServerCredentials? prompt = null;
        Exception? failure = null;
        int prompts = 0;
        bool editing = false;
        DispatcherTimer deadline = new() { Interval = TimeSpan.FromSeconds(20) };
        deadline.Tick += (_, _) =>
        {
            deadline.Stop();
            failure = new AssertionException("The owned credentials prompt did not finish.");
            prompt?.Close();
        };
        using IDisposable observer = Window.WindowOpenedEvent.AddClassHandler<FormBuildServerCredentials>((form, _) =>
        {
            prompt = form;
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    prompts++;
                    bool hasLegacy = scenario is "edit" or "cancel";
                    (editing || (!hasLegacy && scenario is not "stored" and not "current")).Should().BeTrue("stored credentials must reach the real adapter without a prompt");
                    form.FindControl<TextBox>("textBoxUserName")!.Text.Should().Be(hasLegacy ? "legacy-user" : null);
                    form.FindControl<TextBox>("textBoxPassword")!.Text.Should().Be(hasLegacy ? "legacy-password" : null);
                    form.FindControl<TextBox>("textBoxBearerToken")!.Text.Should().Be(hasLegacy ? "legacy-token" : null);
                    if (scenario == "edit")
                    {
                        form.FindControl<TextBox>("textBoxPassword")!.Text = "edited-password";
                        form.FindControl<Button>("buttonOK")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    else
                    {
                        form.FindControl<Button>("buttonCancel")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                }
                catch (Exception exception)
                {
                    failure = exception;
                    form.Close();
                }
            });
        });
        try
        {
            if (scenario == "current")
            {
                new BuildServerCredentialStore(() => repository).Save(key, new BuildServerCredentials
                {
                    BuildServerCredentialsType = BuildServerCredentialsType.UsernameAndPassword,
                    Username = "current-user",
                    Password = "current-password",
                });
            }

            browse = new FormBrowse(new GitUICommands(services, module), new BrowseArguments());
            RevisionGridControl grid = browse.FindControl<RevisionGridControl>("RevisionGrid")!;
            FieldInfo field = typeof(RevisionGridControl).GetField("_buildServerWatcher", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new AssertionException("The real grid no longer owns its watcher.");
            BuildServerWatcher watcher = (BuildServerWatcher)(field.GetValue(grid)
                ?? throw new AssertionException("The real grid did not construct its watcher."));
            using IBuildServerAdapter adapter = CreateAdapter();
            deadline.Start();
            adapter.Initialize(watcher, adapterSettings, () => throw new AssertionException("Unexpected settings UI."));
            failure.Should().BeNull();
            if (scenario is "stored" or "current" or "edit" or "cancel")
            {
                prompts.Should().Be(0);
                editing = scenario is "edit" or "cancel";
                IBuildServerCredentials? actual = watcher.GetBuildServerCredentials(adapter, !editing);
                failure.Should().BeNull();
                if (scenario == "cancel")
                {
                    actual.Should().BeNull();
                }
                else
                {
                    actual.Should().NotBeNull();
                    actual!.Username.Should().Be(scenario == "current" ? "current-user" : "legacy-user");
                    actual.Password.Should().Be(scenario == "current" ? "current-password" : scenario == "edit" ? "edited-password" : "legacy-password");
                    actual.BuildServerCredentialsType.Should().Be(BuildServerCredentialsType.UsernameAndPassword);
                }
            }

            deadline.Stop();
            prompts.Should().Be(scenario is "stored" or "current" ? 0 : 1);
            string[] operations = File.ReadAllLines(helperLog);
            operations.Count(operation => operation == "store").Should().Be(scenario is "edit" or "current" ? 1 : 0);
            if (scenario is "unsupported" or "current")
            {
                rootReads.Should().Be(0);
                decryptions.Should().Be(0);
            }
            else if (scenario == "empty")
            {
                rootReads.Should().BeGreaterThan(0);
                Directory.Exists(legacyRoot).Should().BeFalse("reading an old store must not create one");
            }
            else
            {
                decryptions.Should().BeGreaterThan(0);
            }

            foreach ((string path, byte[] bytes) in before)
            {
                File.ReadAllBytes(path).Should().Equal(bytes, "legacy credentials stay read-only, including explicit edits");
                File.GetLastWriteTimeUtc(path).Should().Be(writeTimes[path]);
            }
        }
        finally
        {
            deadline.Stop();
            prompt?.Close();
            browse?.Close();
            TestDirectory.Delete(directory);
        }
    }

    [TestCase("bearertoken", true, BuildServerCredentialsType.BearerToken)]
    [TestCase("UsernameAndPassword", true, BuildServerCredentialsType.UsernameAndPassword)]
    [TestCase("unknown", false, BuildServerCredentialsType.Guest)]
    [TestCase("42", false, (BuildServerCredentialsType)42)]
    [TestCase("", false, BuildServerCredentialsType.UsernameAndPassword)]
    [TestCase("", true, BuildServerCredentialsType.Guest)]
    public void Legacy_format_should_preserve_original_type_and_guest_parsing(string type, bool guest, BuildServerCredentialsType expected)
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.LegacyFormat-{Guid.NewGuid():N}");
        const string key = "owned-format-fixture";
        byte[] protectedData = [17, 19, 23];
        string configuration = $"[Credentials]\nBuildServerCredentialsType={type}\nUseGuestAccess={guest.ToString().ToLowerInvariant()}\nUsername=owned-user\nPassword=owned-password\nBearerToken=owned-token\n";
        try
        {
            string path = WriteOriginalStore(root, key, protectedData);
            using IDisposable externalIO = LegacyBuildServerCredentialStore.TestAccessor.UseExternalIO(
                () => root, bytes =>
                {
                    bytes.Should().Equal(protectedData);
                    return EncodeOriginalConfiguration(configuration);
                }, () => true);
            BuildServerCredentials? credentials = LegacyBuildServerCredentialStore.Get(key);
            credentials.Should().NotBeNull();
            credentials!.BuildServerCredentialsType.Should().Be(expected);
            credentials.Username.Should().Be("owned-user");
            credentials.Password.Should().Be("owned-password");
            credentials.BearerToken.Should().Be("owned-token");
            File.ReadAllBytes(path).Should().Equal(protectedData);
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Multiple_old_stores_should_be_used_only_when_their_credential_values_agree(bool same)
    {
        string root = Path.Combine(Path.GetTempPath(), $"GitExtensions.LegacyAgreement-{Guid.NewGuid():N}");
        const string key = "owned-agreement-fixture";
        try
        {
            WriteOriginalStore(root, key, [1]);
            WriteOriginalStore(root, key, [2]);
            using IDisposable externalIO = LegacyBuildServerCredentialStore.TestAccessor.UseExternalIO(
                () => root, bytes => EncodeOriginalConfiguration(
                    !same && bytes[0] == 2 ? LegacyConfiguration.Replace("legacy-password", "different-password") : LegacyConfiguration),
                () => true);
            BuildServerCredentials? credentials = LegacyBuildServerCredentialStore.Get(key);
            if (same)
            {
                credentials.Should().NotBeNull();
                credentials!.Password.Should().Be("legacy-password");
            }
            else
            {
                credentials.Should().BeNull("neither directory order nor timestamp selects an account");
            }
        }
        finally
        {
            TestDirectory.Delete(root);
        }
    }

    private static byte[] EncodeOriginalConfiguration(string configuration)
    {
        using MemoryStream stream = new();
        using (StreamWriter writer = new(stream, Encoding.UTF8))
        {
            writer.Write(configuration);
        }

        return stream.ToArray();
    }

    private static IBuildServerAdapter CreateAdapter()
        => ManagedExtensibility.GetExports<IBuildServerAdapter, IBuildServerTypeMetadata>()
            .Single(export => export.Metadata.BuildServerType == "Jenkins").Value;

    private static string WriteOriginalStore(string root, string key, byte[] protectedData)
    {
        FieldInfo cache = typeof(IsolatedStorageFile).Assembly.GetType("System.IO.IsolatedStorage.Helper")
            ?.GetField("s_userRootDirectory", BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new AssertionException("The framework root cache changed; do not write a real user store.");
        object? previous = cache.GetValue(null);
        string ownedRandomRoot = Path.Combine(root, Path.GetRandomFileName(), Path.GetRandomFileName());
        try
        {
            cache.SetValue(null, ownedRandomRoot);
            using IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForDomain();
            string actualRoot = (string)(typeof(IsolatedStorageFile).GetProperty("RootDirectory", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(store)
                ?? throw new AssertionException("Cannot prove the framework-owned fixture path."));
            Path.GetFullPath(actualRoot).Should().StartWith(Path.GetFullPath(ownedRandomRoot) + Path.DirectorySeparatorChar);
            string fileName = string.Format("BuildServer-{0}.options", Convert.ToBase64String(Encoding.UTF8.GetBytes(key)));
            using IsolatedStorageFileStream stream = new(fileName, FileMode.Create, FileAccess.Write, FileShare.None, store);
            stream.Write(protectedData);
            return Path.Combine(actualRoot, fileName);
        }
        finally
        {
            cache.SetValue(null, previous);
        }
    }

    private static ServiceContainer CreateServices()
    {
        ServiceContainer services = new();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(services);
        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver resolver = new(fileSystem);
        RepositoryDescriptionProvider descriptions = new(resolver);
        services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        services.AddService<IGitDirectoryResolver>(resolver);
        services.AddService<IRepositoryDescriptionProvider>(descriptions);
        services.AddService<IAppTitleGenerator>(new AppTitleGenerator(descriptions));
        services.AddService<ILinkFactory>(new LinkFactory());
        GitCommands.ServiceContainerRegistry.RegisterServices(services);
        GitUI.ServiceContainerRegistry.RegisterServices(services);
        return services;
    }

    private static string ShellQuote(string value) => "'" + (OperatingSystem.IsWindows() ? value.Replace('\\', '/') : value).Replace("'", "'\\''") + "'";

    private sealed class CredentialCacheScope : IDisposable
    {
        private readonly IDictionary _cache;
        private readonly List<DictionaryEntry> _previous = [];

        internal CredentialCacheScope()
        {
            _cache = (IDictionary)(typeof(CredentialsSetting).Assembly
                .GetType("GitExtensions.Extensibility.Settings.CredentialsManager")
                ?.GetProperty("Credentials", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null)
                ?? throw new AssertionException("Cannot isolate the pending credential cache."));
            IDictionaryEnumerator enumerator = _cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                _previous.Add(enumerator.Entry);
            }

            _cache.Clear();
        }

        public void Dispose()
        {
            _cache.Clear();
            foreach (DictionaryEntry entry in _previous)
            {
                _cache.Add(entry.Key, entry.Value);
            }
        }
    }
}
