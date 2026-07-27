using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.ProxySwitcher;
using GitExtUtils;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class ProxySwitcherPluginTests
{
    private ServiceContainer _serviceContainer = null!;
    private string? _originalGlobalConfig;
    private string _globalConfigPath = null!;
    private string _workingDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        string testId = Guid.NewGuid().ToString("N");
        _workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.ProxySwitcherTests-{testId}");
        _globalConfigPath = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.ProxySwitcherTests-{testId}.gitconfig");
        Directory.CreateDirectory(_workingDirectory);

        _originalGlobalConfig = Environment.GetEnvironmentVariable("GIT_CONFIG_GLOBAL");
        Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", _globalConfigPath);
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", _originalGlobalConfig);
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
        File.Delete(_globalConfigPath);
    }

    [AvaloniaTest]
    public void Proxy_switcher_form_should_construct_with_original_layout_and_translation_keys()
    {
        using ProxySwitcherForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(341);
        form.Height.Should().Be(106);
        form.MinWidth.Should().Be(341);
        form.MinHeight.Should().Be(106);
        form.FindControl<TextBox>("LocalHttpProxy_TextBox")!.IsReadOnly.Should().BeTrue();
        form.FindControl<TextBox>("GlobalHttpProxy_TextBox")!.IsReadOnly.Should().BeTrue();
        form.FindControl<Button>("SetProxy_Button")!.Margin.Should().Be(new Thickness(3));
        form.FindControl<Button>("UnsetProxy_Button")!.Margin.Should().Be(new Thickness(3));

        translation.Received(1).AddTranslationItem(
            nameof(ProxySwitcherForm), "$this", "Text", "Proxy Switcher");
        translation.Received(1).AddTranslationItem(
            nameof(ProxySwitcherForm), "ApplyGlobally_CheckBox", "Text", "Apply globally");
        translation.Received(1).AddTranslationItem(
            nameof(ProxySwitcherForm), "GlobalHttpProxy_Label", "Text", "Global http.proxy:");
        translation.Received(1).AddTranslationItem(
            nameof(ProxySwitcherForm), "LocalHttpProxy_Label", "Text", "Local http.proxy:");
        translation.Received(1).AddTranslationItem(
            nameof(ProxySwitcherForm), "SetProxy_Button", "Text", "Set proxy");
        translation.Received(1).AddTranslationItem(
            nameof(ProxySwitcherForm), "UnsetProxy_Button", "Text", "Unset proxy");
        translation.Received(1).AddTranslationItem(
            nameof(ProxySwitcherForm),
            "_pleaseSetProxy",
            "Text",
            "There is no proxy configured. Please set the proxy host in the plugin settings.");
    }

    [AvaloniaTest]
    public void Proxy_switcher_plugin_should_expose_its_embedded_icon()
    {
        ProxySwitcherPlugin plugin = new();

        plugin.Id.Should().Be(new Guid("C2A1C7A4-D519-4BD1-859B-6CE7DB9325FB"));
        PluginIconProvider.GetIcon(plugin).Should().NotBeNull();
    }

    [AvaloniaTest]
    public void Proxy_switcher_form_should_set_mask_and_unset_local_and_global_git_proxy()
    {
        GitModule module = CreateRepository();
        ProxySwitcherPlugin plugin = new();
        DictionarySettingsSource settings = new();
        plugin.Username[settings] = "alice";
        plugin.Password[settings] = "secret";
        plugin.HttpProxy[settings] = "proxy.example";
        plugin.HttpProxyPort[settings] = "8081";

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        GitUIEventArgs eventArgs = new(ownerForm: null, commands);

        using ProxySwitcherForm form = new(plugin, settings, eventArgs);
        ProxySwitcherForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.BuildHttpProxy().Should().Be("\"alice:secret@proxy.example:8081\"");

        accessor.ApplyGlobally.IsChecked = false;
        accessor.SetProxy();

        module.GetEffectiveSetting("http.proxy").Should().Be("alice:secret@proxy.example:8081");
        accessor.LocalProxy.Should().Be("alice:****@proxy.example:8081");
        accessor.GlobalProxy.Should().BeEmpty();
        accessor.ApplyGlobally.IsChecked.Should().BeFalse();

        accessor.ApplyGlobally.IsChecked = true;
        accessor.SetProxy();

        File.ReadAllText(_globalConfigPath).Should().Contain("proxy = alice:secret@proxy.example:8081");
        accessor.LocalProxy.Should().Be("alice:****@proxy.example:8081");
        accessor.GlobalProxy.Should().Be("alice:****@proxy.example:8081");
        accessor.ApplyGlobally.IsChecked.Should().BeTrue();

        accessor.UnsetProxy();

        File.ReadAllText(_globalConfigPath).Should().NotContain("proxy =");
        accessor.LocalProxy.Should().Be("alice:****@proxy.example:8081");
        accessor.GlobalProxy.Should().BeEmpty();
        accessor.ApplyGlobally.IsChecked.Should().BeFalse();

        accessor.UnsetProxy();

        module.GetEffectiveSetting("http.proxy").Should().BeEmpty();
        accessor.LocalProxy.Should().BeEmpty();
        accessor.GlobalProxy.Should().BeEmpty();
        accessor.ApplyGlobally.IsChecked.Should().BeTrue();
    }

    private GitModule CreateRepository()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
        return module;
    }

    private sealed class DictionarySettingsSource : SettingsSource
    {
        private readonly Dictionary<string, string?> _values = [];

        public override SettingLevel SettingLevel => SettingLevel.Local;

        public override string? GetValue(string name)
            => _values.GetValueOrDefault(name);

        public override void SetValue(string name, string? value)
            => _values[name] = value;
    }
}
