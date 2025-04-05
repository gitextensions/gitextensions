using FluentAssertions;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using NSubstitute;
using ResourceManager;

namespace GitUI.ScriptsEngine.Tests;

[TestFixture]
public class DistributedScriptsManagerTests
{
    private const int _keyOfExampleScript = 9002;
    private const int _keyOfLocalExampleScript = 9003;

    private IUserScriptsStorage _scriptsStorage;
    private DistributedScriptsManager _managerSingleLevel;
    private DistributedScriptsManager _managerMultiLevel;
    private MockForm _mockForm;

    private IUserScriptsStorage _distributedScriptsStorage;
    private DistributedSettings _localSettings;
    private DistributedSettings _distributedSettings;

    [SetUp]
    public void SetUp()
    {
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        _mockForm = new(commands);

        ScriptInfo script = new()
        {
            HotkeyCommandIdentifier = _keyOfExampleScript,
            Command = "{git}",
            Arguments = "--version",
            AskConfirmation = false,
            RunInBackground = true,
            Name = "Example",
            OnEvent = ScriptEvent.BeforeCommit
        };
        _scriptsStorage = Substitute.For<IUserScriptsStorage>();
        _scriptsStorage.Load(Arg.Any<DistributedSettings>())
            .Returns(x => new List<ScriptInfo>() { script });

        DistributedSettings settings = new(lowerPriority: null, new GitExtSettingsCache("path"), SettingLevel.Global);
        _managerSingleLevel = new DistributedScriptsManager(_scriptsStorage);
        _managerSingleLevel.Initialize(settings);

        _distributedScriptsStorage = Substitute.For<IUserScriptsStorage>();
        _distributedScriptsStorage.Load(Arg.Any<DistributedSettings>())
            .Returns(x => new List<ScriptInfo>() { script },
                     x => []);
        _localSettings = new(lowerPriority: null, new GitExtSettingsCache("path"), SettingLevel.Local);
        _distributedSettings = new(_localSettings, new GitExtSettingsCache("path"), SettingLevel.Global);
        _managerMultiLevel = new DistributedScriptsManager(_distributedScriptsStorage);
        _managerMultiLevel.Initialize(_distributedSettings);
    }

    [Test]
    public void Add_should_add_script()
    {
        ScriptInfo script = new() { HotkeyCommandIdentifier = 1 };
        _managerSingleLevel.Add(script);

        IReadOnlyList<ScriptInfo> scripts = _managerSingleLevel.GetScripts();

        scripts.Count.Should().Be(2); // 1 from the storage + 1 added
        scripts.Should().Contain(script);
    }

    [Test]
    public void GetScript_should_return_script_if_exists()
    {
        ScriptInfo script = _managerSingleLevel.GetScript(_keyOfExampleScript);

        script.Should().NotBeNull();
        script.HotkeyCommandIdentifier.Should().Be(_keyOfExampleScript);
        script.Command.Should().Be("{git}");
        script.Arguments.Should().Be("--version");
    }

    [Test]
    public void Remove_should_remove_script()
    {
        ScriptInfo script = new() { HotkeyCommandIdentifier = _keyOfExampleScript };
        _managerSingleLevel.Remove(script);

        IReadOnlyList<ScriptInfo> scripts = _managerSingleLevel.GetScripts();

        scripts.Should().BeEmpty();
    }

    [Test]
    public void Update_should_update_script()
    {
        _managerSingleLevel.GetScript(_keyOfExampleScript).Should().NotBeNull();

        ScriptInfo script = new()
        {
            HotkeyCommandIdentifier = _keyOfExampleScript,
            AskConfirmation = false,
            RunInBackground = true,
            Name = "Example",

            // changed properties
            Command = "{cmd}",
            Arguments = "version",
            OnEvent = ScriptEvent.BeforeCommit
        };

        _managerSingleLevel.Update(script);

        _managerSingleLevel.GetScript(script.HotkeyCommandIdentifier).Should().Be(script);
    }

    [Test]
    public void RunEventScripts_should_execute_scripts()
    {
        bool result = _managerSingleLevel.RunEventScripts(ScriptEvent.BeforeCommit, _mockForm);

        result.Should().BeTrue();
    }

    [Test]
    public void Save_should_save_scripts()
    {
        _managerSingleLevel.Save();

        DistributedScriptsManager.TestAccessor testAccessor = _managerSingleLevel.GetTestAccessor();
        _scriptsStorage.Received().Save(Arg.Any<DistributedSettings>(), testAccessor.Scripts);
    }

    [Test]
    public void GetScript_should_return_script_if_exists_in_multiLevel()
    {
        ScriptInfo script = new()
        {
            HotkeyCommandIdentifier = _keyOfLocalExampleScript,
            Command = "{cmd}",
            Arguments = "version",
            OnEvent = ScriptEvent.BeforeCommit,
            AskConfirmation = false,
            RunInBackground = true,
            Name = "LowerPriorityScript"
        };
        DistributedScriptsManager.TestAccessor testAccessor = _managerMultiLevel.GetTestAccessor();
        testAccessor.LowerPriority.GetTestAccessor().Scripts.Add(script);

        ScriptInfo? loadedScript = _managerMultiLevel.GetScript(_keyOfLocalExampleScript);

        loadedScript.Should().Be(script);
    }

    [Test]
    public void Add_should_add_script_in_multiLevel()
    {
        ScriptInfo script = new()
        {
            HotkeyCommandIdentifier = _keyOfLocalExampleScript,
            Command = "{cmd}",
            Arguments = "version",
            OnEvent = ScriptEvent.BeforeCommit,
            AskConfirmation = false,
            RunInBackground = true,
            Name = "LowerPriorityScript"
        };

        _managerMultiLevel.Add(script);

        DistributedScriptsManager.TestAccessor testAccessor = _managerMultiLevel.GetTestAccessor();
        testAccessor.Scripts.Count.Should().Be(1);
        testAccessor.LowerPriority.GetTestAccessor().Scripts.Count.Should().Be(1);
        testAccessor.LowerPriority.GetTestAccessor().Scripts[0].Should().Be(script);
    }

    [Test]
    public void Remove_should_remove_script_in_multiLevel()
    {
        ScriptInfo script = new()
        {
            HotkeyCommandIdentifier = _keyOfLocalExampleScript,
            Command = "{cmd}",
            Arguments = "version",
            OnEvent = ScriptEvent.BeforeCommit,
            AskConfirmation = false,
            RunInBackground = true,
            Name = "LowerPriorityScript"
        };
        DistributedScriptsManager.TestAccessor testAccessor = _managerMultiLevel.GetTestAccessor();
        testAccessor.LowerPriority.GetTestAccessor().Scripts.Add(script);

        _managerMultiLevel.Remove(script);

        testAccessor.Scripts.Count.Should().Be(1);
        testAccessor.LowerPriority.GetTestAccessor().Scripts.Count.Should().Be(0);
    }

    [Test]
    public void Update_should_update_script_in_multiLevel()
    {
        ScriptInfo script = new()
        {
            HotkeyCommandIdentifier = _keyOfLocalExampleScript,
            Command = "{cmd}",
            Arguments = "version",
            OnEvent = ScriptEvent.BeforeCommit,
            AskConfirmation = false,
            RunInBackground = true,
            Name = "LowerPriorityScript"
        };
        DistributedScriptsManager.TestAccessor testAccessor = _managerMultiLevel.GetTestAccessor();
        testAccessor.LowerPriority.GetTestAccessor().Scripts.Add(script);

        script.Command = "{git}";

        _managerMultiLevel.Update(script);

        testAccessor.LowerPriority.GetTestAccessor().Scripts.Count.Should().Be(1);
        testAccessor.LowerPriority.GetTestAccessor().Scripts[0].Should().Be(script);
    }

    [Test]
    public void Save_should_save_scripts_in_multiLevel()
    {
        _managerMultiLevel.Save();

        DistributedScriptsManager.TestAccessor accessor = _managerMultiLevel.GetTestAccessor();
        _distributedScriptsStorage.Received(1).Save(
            Arg.Is<DistributedSettings>(s => s.SettingLevel == _distributedSettings.SettingLevel),
            Arg.Is<IReadOnlyList<ScriptInfo>>(l => l.Count == 1));
        _distributedScriptsStorage.Received(1).Save(
            Arg.Is<DistributedSettings>(s => s.SettingLevel == _localSettings.SettingLevel),
            Arg.Is<IReadOnlyList<ScriptInfo>>(l => l.Count == 0));
    }

    private class MockForm : IWin32Window, IGitModuleForm
    {
        public MockForm(IGitUICommands commands)
        {
            UICommands = commands;
        }

        public IntPtr Handle { get; }
        public IGitUICommands UICommands { get; }
    }
}
