using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using GitCommands;
using GitCommands.Config;
using GitCommands.DiffMergeTools;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitCommandsTests.DiffMergeTools
{
    [TestFixture]
    public class DiffMergeToolConfigurationManagerTests
    {
        private const string DiffToolName = "customDiffTool";
        private const string MergeToolName = "customMergeTool";
        private const string MergeToolNameNoGui = "customMergeToolNoGui";
        private IConfigValueStore _fileSettings;
        private DiffMergeToolConfigurationManager _configurationManager;

        private const string DiffToolKey = "diff.guitool";
        private const string MergeToolKey = "merge.guitool";
        private const string MergeToolNoGuiKey = "merge.tool";

        [SetUp]
        public void Setup()
        {
            _fileSettings = Substitute.For<IConfigValueStore>();

            _configurationManager = new DiffMergeToolConfigurationManager(() => _fileSettings);

            _fileSettings.GetValue(SettingKeyString.DiffToolKey).Returns(DiffToolName);
            _fileSettings.GetValue(SettingKeyString.MergeToolKey).Returns(MergeToolName);
            _fileSettings.GetValue(SettingKeyString.MergeToolNoGuiKey).Returns(MergeToolNameNoGui);
        }

        [Test]
        public void Check_that_application_toolkey_is_same_as_test_constant()
        {
            // Test cases requires constants, test that they are equal
            SettingKeyString.DiffToolKey.Should().Be(DiffToolKey);
            SettingKeyString.MergeToolKey.Should().Be(MergeToolKey);
            SettingKeyString.MergeToolNoGuiKey.Should().Be(MergeToolNoGuiKey);
        }

        [Test]
        public void ConfiguredDiffTool_should_return_null_if_config_file_unset()
        {
            new DiffMergeToolConfigurationManager(() => null).ConfiguredDiffTool.Should().BeNull();
        }

        [Test]
        public void ConfiguredDiffTool_should_return_value_in_config_file()
        {
            _configurationManager.ConfiguredDiffTool.Should().Be(DiffToolName);
        }

        [Test]
        public void ConfiguredMergeTool_should_return_null_if_config_file_unset()
        {
            new DiffMergeToolConfigurationManager(() => null).ConfiguredMergeTool.Should().BeNull();
        }

        [Test]
        public void ConfiguredMergeTool_should_return_value_in_config_file()
        {
            _configurationManager.ConfiguredMergeTool.Should().Be(GitVersion.Current.SupportGuiMergeTool ? MergeToolName : MergeToolNameNoGui);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void ConfigureDiffMergeTool_should_return_if_tool_unset(string toolName)
        {
            using (new NoAssertContext())
            {
                _configurationManager.ConfigureDiffMergeTool(toolName, DiffMergeToolType.Diff, "", "");
            }

            _fileSettings.DidNotReceive().SetValue(Arg.Any<string>(), Arg.Any<string>());
            _fileSettings.DidNotReceive().SetPathValue(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public void ConfigureDiffMergeTool_should_return_if_file_unset()
        {
            _fileSettings = null;

            _configurationManager.ConfigureDiffMergeTool("bla", DiffMergeToolType.Diff, "", "");
        }

        [TestCase("bla", DiffMergeToolType.Diff, @"c:\some\path\to the tool\MyTool.exe", "--wait --diff \"$LOCAL\" \"$REMOTE\"", DiffToolKey, "difftool.bla.path", "difftool.bla.cmd")]
        [TestCase("bla", DiffMergeToolType.Merge, @"c:\some\path\to the tool\MyTool.exe", "--wait \"$MERGED\"", MergeToolKey, "mergetool.bla.path", "mergetool.bla.cmd")]
        public void ConfigureDiffMergeTool_should_configure_tool(string toolName, DiffMergeToolType toolType, string toolPath, string toolCommand,
            string expectedValueKey, string expectedPathKey, string expectedCommandKey)
        {
            _configurationManager.ConfigureDiffMergeTool(toolName, toolType, toolPath, toolCommand);

            _fileSettings.Received(1).SetValue(Arg.Any<string>(), Arg.Any<string>());
            _fileSettings.Received(1).SetValue(expectedValueKey, "bla");

            _fileSettings.Received(2).SetPathValue(Arg.Any<string>(), Arg.Any<string>());
            _fileSettings.Received(1).SetPathValue(expectedPathKey, toolPath);
            _fileSettings.Received(1).SetPathValue(expectedCommandKey, toolCommand);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void GetToolCommand_should_return_empty_if_toolName_unset(string toolName)
        {
            _fileSettings.GetValue(Arg.Any<string>()).Returns("");

            _configurationManager.GetToolCommand(toolName, DiffMergeToolType.Merge).Should().BeEmpty();
            _fileSettings.Received(0).GetValue(Arg.Any<string>());
        }

        [TestCase("bla", DiffMergeToolType.Diff, "difftool.bla.cmd")]
        [TestCase("bla", DiffMergeToolType.Merge, "mergetool.bla.cmd")]
        public void GetToolCommand_should_return_command_for_toolName_if_set(string toolName, DiffMergeToolType toolType, string expectedValueKey)
        {
            var command = "--wait --diff \"$LOCAL\" \"$REMOTE\"";
            _fileSettings.GetValue(expectedValueKey).Returns(command);

            _configurationManager.GetToolCommand(toolName, toolType).Should().Be(command);
            _fileSettings.Received(1).GetValue(Arg.Any<string>());
            _fileSettings.Received(1).GetValue(expectedValueKey);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void LoadDiffMergeToolConfig_should_throw_if_toolName_unset(string toolName)
        {
            ((Action)(() => _configurationManager.LoadDiffMergeToolConfig(toolName, ""))).Should().Throw<ArgumentException>();
        }

        [Test]
        public void LoadDiffMergeToolConfig_should_create_tool_config_with_userSuppliedPath_if_tool_unregistered()
        {
            var config = _configurationManager.LoadDiffMergeToolConfig("bla", @"c:\some\path\to the tool\bla.exe");

            config.Should().NotBeNull();
            config.ExeFileName.Should().Be("bla.exe");
            config.Path.Should().Be("c:/some/path/to the tool/bla.exe");
            config.DiffCommand.Should().BeEmpty();
            config.MergeCommand.Should().BeEmpty();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void LoadDiffMergeToolConfig_should_create_tool_config_if_tool_unregistered(string userSuppliedPath)
        {
            var config = _configurationManager.LoadDiffMergeToolConfig("bla", userSuppliedPath);

            config.Should().NotBeNull();
            config.ExeFileName.Should().Be("bla.exe");
            config.Path.Should().Be(string.Empty);
            config.DiffCommand.Should().BeEmpty();
            config.MergeCommand.Should().BeEmpty();
        }

        [Test]
        public void LoadDiffMergeToolConfig_should_create_tool_config_if_tool_unregistered_but_exists_path()
        {
            var config = _configurationManager.LoadDiffMergeToolConfig("notepad", null);

            config.Should().NotBeNull();
            config.ExeFileName.Should().Be("notepad.exe");
            config.Path.Length.Should().BeGreaterThan(0); // e.g. C:/WINDOWS/system32/notepad.exe
            config.DiffCommand.Should().BeEmpty();
            config.MergeCommand.Should().BeEmpty();
        }

        [Test]
        public void LoadDiffMergeToolConfig_should_load_tool_config_if_tool_registered()
        {
            var tool = new SemanticMerge();

            var toolPath = @"c:\some\path\to the tool\MyTool.exe";
            _fileSettings.GetValue($"mergetool.{tool.Name}.path").Returns(toolPath);
            _configurationManager = new DiffMergeToolConfigurationManager(() => _fileSettings, FindFileInFolders);

            var config = _configurationManager.LoadDiffMergeToolConfig(tool.Name, null);

            config.Should().NotBeNull();
            config.ExeFileName.Should().Be(tool.ExeFileName);
            config.Path.Should().Be(toolPath.ToPosixPath());
            config.DiffCommand.Should().Be(tool.DiffCommand);
            config.MergeCommand.Should().Be(tool.MergeCommand);

            return;

            string FindFileInFolders(string fileName, IEnumerable<string> folders)
            {
                folders.Should().BeEquivalentTo(new[] { toolPath }.Union(tool.SearchPaths));
                return toolPath;
            }
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void GetToolPath_should_return_empty_if_toolName_unset(string toolName)
        {
            _fileSettings.GetValue(Arg.Any<string>()).Returns("");

            _configurationManager.GetToolPath(toolName, DiffMergeToolType.Merge).Should().BeEmpty();
            _fileSettings.Received(0).GetValue(Arg.Any<string>());
        }

        [TestCase("bla", DiffMergeToolType.Diff, "difftool.bla.path")]
        [TestCase("bla", DiffMergeToolType.Merge, "mergetool.bla.path")]
        public void GetToolPath_should_return_command_for_toolName_if_set(string toolName, DiffMergeToolType toolType, string expectedValueKey)
        {
            var toolPath = @"c:\some\path\to the tool\MyTool.exe";
            _fileSettings.GetValue(expectedValueKey).Returns(toolPath);

            _configurationManager.GetToolPath(toolName, toolType).Should().Be(toolPath);
            _fileSettings.Received(1).GetValue(Arg.Any<string>());
            _fileSettings.Received(1).GetValue(expectedValueKey);
        }

        [TestCase(DiffMergeToolType.Diff, DiffToolKey, "difftool")]
        [TestCase(DiffMergeToolType.Merge, MergeToolKey, "mergetool")]
        public void GetInfo(DiffMergeToolType toolType, string expectedToolKey, string expectedPrefix)
        {
            var info = _configurationManager.GetTestAccessor().GetInfo(toolType);

            info.toolKey.Should().Be(expectedToolKey);
            info.prefix.Should().Be(expectedPrefix);
        }

        [Test]
        public void GetToolSetting_should_return_empty_if_toolName_is_empty()
        {
            _fileSettings.GetValue(Arg.Any<string>()).Returns(string.Empty);
            var settings = _configurationManager.GetTestAccessor().GetToolSetting(string.Empty, DiffMergeToolType.Diff, "");

            settings.Should().BeEmpty();
            _fileSettings.Received(1).GetValue(Arg.Any<string>());
            _fileSettings.Received(1).GetValue(SettingKeyString.DiffToolKey);
        }

        [TestCase(DiffMergeToolType.Diff, "difftool")]
        [TestCase(DiffMergeToolType.Merge, "mergetool")]
        public void GetToolSetting_should_load_setting_for_requested_toolName(DiffMergeToolType toolType, string expectedPrefix)
        {
            var settings = _configurationManager.GetTestAccessor().GetToolSetting(DiffToolName, toolType, "customSuffix");

            settings.Should().BeEmpty();
            _fileSettings.GetValue($"{expectedPrefix}.{DiffToolName}.customSuffix");
        }
    }
}
