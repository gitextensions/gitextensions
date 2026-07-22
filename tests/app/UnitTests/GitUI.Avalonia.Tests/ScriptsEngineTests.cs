using System.ComponentModel;
using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Hotkey;
using GitUI.ScriptsEngine;
using GitUI.UserControls.RevisionGrid;
using NSubstitute;
using ResourceManager;
using ResourceManager.Hotkey;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class ScriptsEngineTests
{
    [Test]
    public void Service_registry_should_share_one_manager_with_the_runner()
    {
        using ServiceContainer services = new();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(services);

        GitUI.ServiceContainerRegistry.RegisterServices(services);

        services.GetRequiredService<IScriptsManager>()
            .Should().BeSameAs(services.GetRequiredService<IScriptsRunner>());
        services.GetRequiredService<ISimplePromptCreator>().Should().BeOfType<SimplePromptCreator>();
        services.GetRequiredService<IFilePromptCreator>().Should().BeOfType<FilePromptCreator>();
    }

    [Test]
    public void Scripts_manager_should_preserve_the_original_xml_contract()
    {
        string? originalScripts = AppSettings.OwnScripts;
        try
        {
            AppSettings.OwnScripts = string.Empty;
            ScriptsManager manager = new();

            manager.GetScripts().Should().Contain(script => script.HotkeyCommandIdentifier == 9005
                && script.OnEvent == ScriptEvent.ShowInUserMenuBar
                && script.Command == "bash");
            manager.SerializeIntoXml().Should().Contain("<ScriptInfo>").And.Contain("<HotkeyCommandIdentifier>9005</HotkeyCommandIdentifier>");
        }
        finally
        {
            AppSettings.OwnScripts = originalScripts;
        }
    }

    [AvaloniaTest]
    public void Script_info_should_resolve_packaged_icons_and_discard_invalid_names()
    {
        ScriptInfo packaged = new() { Name = "Edit", Icon = "EditFile" };
        ScriptInfo invalid = new() { Name = "Missing", Icon = "NotAnImageResource" };

        packaged.GetIcon().Should().NotBeNull();
        invalid.GetIcon().Should().BeNull();
        invalid.Icon.Should().BeNull();
    }

    [AvaloniaTest]
    public void Simple_prompt_should_keep_the_default_and_return_edited_input()
    {
        SimplePrompt prompt = new("Script input", "Branch", "main");
        TextBox input = prompt.FindControl<TextBox>("txtUserInput")!;
        input.Text.Should().Be("main");
        prompt.FindControl<TextBlock>("labelInput")!.Text.Should().Be("Branch:");

        input.Text = "feature";
        prompt.FindControl<Button>("btnOk")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        prompt.UserInput.Should().Be("feature");
        prompt.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
    }

    [AvaloniaTest]
    public void File_prompt_should_cancel_an_empty_value()
    {
        FormFilePrompt prompt = new();

        prompt.FindControl<Button>("btnOk")!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        prompt.UserInput.Should().BeEmpty();
        prompt.DialogResult.Should().Be(WinFormsShims.DialogResult.Cancel);
    }

    [AvaloniaTest]
    public void File_prompt_should_retain_the_original_translation_keys()
    {
        FormFilePrompt prompt = new();
        ITranslation translation = NSubstitute.Substitute.For<ITranslation>();

        prompt.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormFilePrompt), "$this", "Text", "Select script files");
        translation.Received(1).AddTranslationItem(nameof(FormFilePrompt), "btnBrowse", "Text", "Browse...");
        translation.Received(1).AddTranslationItem(nameof(FormFilePrompt), "btnOk", "Text", "&OK");
        translation.Received(1).AddTranslationItem(nameof(FormFilePrompt), "lblSelectFiles", "Text", "Select file(s)");
    }

    [AvaloniaTest]
    public void Quick_string_selector_should_sort_and_preselect_the_first_value()
    {
        FormQuickStringSelector selector = new();

        selector.Init(["zeta", "alpha"]);

        selector.SelectedString.Should().Be("alpha");
        selector.FindControl<ListBox>("lbxRefs")!.Height.Should().Be(48);
    }

    [Test]
    public void PowerShell_launcher_should_select_the_native_executable_name()
    {
        PowerShellHelper.GetExecutableName().Should().Be(OperatingSystem.IsWindows() ? "powershell.exe" : "pwsh");
    }

    [Test]
    public void Script_options_provider_should_supply_selected_paths_and_caret_position()
    {
        ScriptOptionsProvider provider = new(
            () => ["src/file one.cs", "README.md"],
            () => 17,
            () => 4);

        provider.GetValues("SelectedRelativePaths").Should().Equal(
            "src/file one.cs".EscapeForCommandLine(),
            "README.md".EscapeForCommandLine());
        provider.GetValues("LineNumber").Should().Equal("17");
        provider.GetValues("ColumnNumber").Should().Equal("4");
    }

    [AvaloniaTest]
    public void User_script_context_menu_should_keep_direct_and_nested_scripts_separate()
    {
        ScriptInfo nested = new() { Name = "Nested", HotkeyCommandIdentifier = 9001 };
        ScriptInfo direct = new() { Name = "Direct", HotkeyCommandIdentifier = 9002, AddToRevisionGridContextMenu = true };
        ScriptInfo disabled = new() { Name = "Disabled", HotkeyCommandIdentifier = 9003, Enabled = false };
        IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
        scriptsManager.GetScripts().Returns(new BindingList<ScriptInfo>([nested, direct, disabled]));
        IHotkeySettingsLoader hotkeyLoader = Substitute.For<IHotkeySettingsLoader>();
        hotkeyLoader.LoadHotkeys(FormSettings.HotkeySettingsName).Returns(
            [new HotkeyCommand(9002, direct.GetDisplayName()) { KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.D }]);
        using ServiceContainer services = new();
        services.AddService(typeof(IScriptsManager), scriptsManager);
        services.AddService(typeof(IHotkeySettingsLoader), hotkeyLoader);
        MenuItem host = new() { Header = "Run script" };
        MenuItem tail = new() { Header = "Tail" };
        ContextMenu menu = new() { Items = { host, tail } };
        List<int> invoked = [];

        bool added = menu.AddUserScripts(host, Invoke, script => script.AddToRevisionGridContextMenu, services);

        added.Should().BeTrue();
        menu.Items.OfType<MenuItem>().Select(item => item.Header).Should().Equal("Run script", "Direct", "Tail");
        host.Items.OfType<MenuItem>().Select(item => item.Header).Should().Equal("Nested");
        MenuItem directItem = menu.Items.OfType<MenuItem>().ElementAt(1);
        directItem.InputGesture.Should().BeEquivalentTo(new KeyGesture(Key.D, KeyModifiers.Control));
        directItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        invoked.Should().Equal(9002);

        bool Invoke(int id)
        {
            invoked.Add(id);
            return true;
        }
    }

    [AvaloniaTest]
    public void Git_module_form_should_dispatch_script_hotkey_commands_through_the_shared_runner()
    {
        const int scriptId = 9123;
        ScriptInfo script = new() { HotkeyCommandIdentifier = scriptId, Name = "Test script", Command = "git" };
        IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
        IScriptsRunner scriptsRunner = Substitute.For<IScriptsRunner>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IScriptsManager)).Returns(scriptsManager);
        commands.GetService(typeof(IScriptsRunner)).Returns(scriptsRunner);
        scriptsManager.GetScript(scriptId).Returns(script);
        scriptsRunner.RunScript(script, Arg.Any<IWin32Window>(), commands, Arg.Any<IScriptOptionsProvider>()).Returns(true);
        TestGitModuleForm form = new(commands);

        form.Dispatch(scriptId).Should().BeTrue();

        scriptsRunner.Received(1).RunScript(script, form, commands, Arg.Is<IScriptOptionsProvider>(provider => provider == ScriptOptionsProviderBase.Default));
    }

    [AvaloniaTest]
    public void Git_module_control_should_dispatch_scripts_with_its_nearest_option_provider()
    {
        const int scriptId = 9125;
        ScriptInfo script = new() { HotkeyCommandIdentifier = scriptId, Name = "Control script", Command = "git" };
        IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
        IScriptsRunner scriptsRunner = Substitute.For<IScriptsRunner>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        source.UICommands.Returns(commands);
        commands.GetService(typeof(IScriptsManager)).Returns(scriptsManager);
        commands.GetService(typeof(IScriptsRunner)).Returns(scriptsRunner);
        scriptsManager.GetScript(scriptId).Returns(script);
        scriptsRunner.RunScript(script, Arg.Any<IWin32Window>(), commands, Arg.Any<IScriptOptionsProvider>()).Returns(true);
        ScriptOptionsProvider provider = new(() => ["selected.txt"], () => 3, () => 8);
        TestGitModuleControl control = new(provider) { UICommandsSource = source };

        control.Dispatch(scriptId).Should().BeTrue();

        scriptsRunner.Received(1).RunScript(script, control, commands, provider);
    }

    [AvaloniaTest]
    public void Git_module_form_should_dispatch_configured_user_script_hotkeys()
    {
        const int scriptId = 9124;
        ScriptInfo script = new() { HotkeyCommandIdentifier = scriptId, Name = "Hotkey script", Command = "git" };
        IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
        IScriptsRunner scriptsRunner = Substitute.For<IScriptsRunner>();
        IHotkeySettingsLoader hotkeyLoader = Substitute.For<IHotkeySettingsLoader>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IScriptsManager)).Returns(scriptsManager);
        commands.GetService(typeof(IScriptsRunner)).Returns(scriptsRunner);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(hotkeyLoader);
        scriptsManager.GetScript(scriptId).Returns(script);
        hotkeyLoader.LoadHotkeys(FormSettings.HotkeySettingsName).Returns(
            [new HotkeyCommand(scriptId, script.GetDisplayName()) { KeyData = WinFormsShims.Keys.Control | WinFormsShims.Keys.D7 }]);
        scriptsRunner.RunScript(script, Arg.Any<IWin32Window>(), commands, Arg.Any<IScriptOptionsProvider>()).Returns(true);
        TestGitModuleForm form = new(commands);

        form.ProcessHotkey(WinFormsShims.Keys.Control | WinFormsShims.Keys.D7).Should().BeTrue();

        scriptsRunner.Received(1).RunScript(script, form, commands, ScriptOptionsProviderBase.Default);
    }

    private sealed class TestGitModuleForm : GitUI.GitModuleForm
    {
        public TestGitModuleForm(IGitUICommands commands)
            : base(commands, enablePositionRestore: false)
        {
            HotkeysEnabled = true;
        }

        public bool Dispatch(int command)
            => ExecuteCommand(command);
    }

    private sealed class TestGitModuleControl : GitUI.GitModuleControl
    {
        private readonly IScriptOptionsProvider _provider;

        public TestGitModuleControl(IScriptOptionsProvider provider)
        {
            _provider = provider;
        }

        public bool Dispatch(int command)
            => ExecuteCommand(command);

        protected override IScriptOptionsProvider GetScriptOptionsProvider()
            => _provider;
    }
}
