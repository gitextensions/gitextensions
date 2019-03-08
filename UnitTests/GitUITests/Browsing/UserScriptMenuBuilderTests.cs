using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using GitUI.Browsing;
using GitUI.Script;
using Moq;
using NUnit.Framework;

namespace GitUITests.Browsing
{
    [TestFixture]
    internal sealed class UserScriptMenuBuilderTests
    {
        private const string RunScriptMenuItemName = "runScriptToolStripMenuItem";

        private IUserScriptMenuBuilder _userScriptMenuBuilder;

        private readonly Mock<IScriptManager> _scriptManagerMock = new Mock<IScriptManager>();
        private readonly Mock<IScriptRunner> _scriptRunnerMock = new Mock<IScriptRunner>();
        private readonly Mock<ICanRefreshRevisions> _canRefreshRevisionsMock = new Mock<ICanRefreshRevisions>();
        private readonly Mock<ICanLoadSettings> _canLoadSettingsMock = new Mock<ICanLoadSettings>();

        [SetUp]
        public void Init()
        {
            _userScriptMenuBuilder = new UserScriptMenuBuilder(
                _scriptManagerMock.Object,
                _scriptRunnerMock.Object,
                _canRefreshRevisionsMock.Object,
                _canLoadSettingsMock.Object);
        }

        #region Build Tool

        [Test]
        public void Build_tool_with_no_scripts_should_not_delete_it()
        {
            // Arrange
            var tool = new ToolStrip();

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(new BindingList<ScriptInfo>());

            // Act
            _userScriptMenuBuilder.Build(tool);

            // Assert
            Assert.That(tool, Is.Not.Null);
            Assert.That(tool.Enabled, Is.True);
            Assert.That(tool.Visible, Is.True);
        }

        [Test]
        public void Build_tool_with_scripts_should_not_delete_it()
        {
            // Arrange
            var tool = new ToolStrip();

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(Scripts);

            // Act
            _userScriptMenuBuilder.Build(tool);

            // Assert
            Assert.That(tool, Is.Not.Null);
            Assert.That(tool.Enabled, Is.True);
            Assert.That(tool.Visible, Is.True);
        }

        [Test]
        public void Build_tool_with_no_scripts_should_not_remove_exists_items()
        {
            // Arrange
            var tool = new ToolStrip();

            tool.Items.Add(new ToolStripButton());

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(new BindingList<ScriptInfo>());

            // Act
            _userScriptMenuBuilder.Build(tool);

            // Assert
            Assert.That(tool.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void Build_tool_with_scripts_should_not_remove_exists_items()
        {
            // Arrange
            var tool = new ToolStrip();
            var buttonName = Guid.NewGuid().ToString();

            tool.Items.Add(new ToolStripButton { Name = buttonName });

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(Scripts);

            // Act
            _userScriptMenuBuilder.Build(tool);

            // Assert
            var existItemsCount = tool.Items
                .OfType<ToolStripButton>()
                .Count(x => x.Name == buttonName);

            Assert.That(existItemsCount, Is.EqualTo(1));
        }

        [Test]
        public void Build_tool_should_work()
        {
            // Arrange
            var tool = new ToolStrip();

            tool.Items.Add(new ToolStripButton());

            var scripts = Scripts();

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(scripts);

            // Act
            _userScriptMenuBuilder.Build(tool);

            // Assert
            const int existItemsCount = 1;
            const int separatorsCount = 1;
            var newItemsCount = scripts
                .Where(x => x.Enabled)
                .Count(x => x.OnEvent == ScriptEvent.ShowInUserMenuBar);

            Assert.That(tool.Enabled, Is.True);
            Assert.That(tool.Visible, Is.True);
            Assert.That(tool.Items.Count, Is.EqualTo(existItemsCount + separatorsCount + newItemsCount));
        }

        #endregion Build Tool

        #region Build Context Menu

        [Test]
        public void Build_contextMenu_with_no_scripts_should_not_delete_it()
        {
            // Arrange
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add(new ToolStripMenuItem { Name = RunScriptMenuItemName });

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(new BindingList<ScriptInfo>());

            // Act
            _userScriptMenuBuilder.Build(contextMenu);

            // Assert
            Assert.That(contextMenu, Is.Not.Null);
            Assert.That(contextMenu.Enabled, Is.True);
        }

        [Test]
        public void Build_contextMenu_with_scripts_should_not_delete_it()
        {
            // Arrange
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add(new ToolStripMenuItem { Name = RunScriptMenuItemName });

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(Scripts);

            // Act
            _userScriptMenuBuilder.Build(contextMenu);

            // Assert
            Assert.That(contextMenu, Is.Not.Null);
            Assert.That(contextMenu.Enabled, Is.True);
        }

        [Test]
        public void Build_contextMenu_with_scripts_should_not_delete_runScriptMenuItem()
        {
            // Arrange
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add(new ToolStripMenuItem { Name = RunScriptMenuItemName });

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(Scripts);

            // Act
            _userScriptMenuBuilder.Build(contextMenu);

            // Assert
            var existItemsCount = contextMenu.Items
                .OfType<ToolStripMenuItem>()
                .Count(x => x.Name == RunScriptMenuItemName);

            Assert.That(existItemsCount, Is.EqualTo(1));
        }

        [Test]
        public void Build_contextMenu_with_no_scripts_should_not_remove_exists_items()
        {
            // Arrange
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add(new ToolStripMenuItem { Name = RunScriptMenuItemName });

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(new BindingList<ScriptInfo>());

            // Act
            _userScriptMenuBuilder.Build(contextMenu);

            // Assert
            Assert.That(contextMenu.Items.Count, Is.EqualTo(1));
        }

        [Test]
        public void Build_contextMenu_with_scripts_should_not_remove_exists_items()
        {
            // Arrange
            var contextMenu = new ContextMenuStrip();

            contextMenu.Items.Add(new ToolStripMenuItem { Name = RunScriptMenuItemName });

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(Scripts);

            // Act
            _userScriptMenuBuilder.Build(contextMenu);

            // Assert
            var existItemsCount = contextMenu.Items
                .OfType<ToolStripMenuItem>()
                .Count(x => x.Name == RunScriptMenuItemName);

            Assert.That(existItemsCount, Is.EqualTo(1));
        }

        [Test]
        public void Build_contextMenu_with_only_addToRevisionGridContextMenu_scripts_should_invisible_runScriptMenuItem()
        {
            // Arrange
            var contextMenu = new ContextMenuStrip();
            var runScriptMenuItem = new ToolStripMenuItem { Name = RunScriptMenuItemName };

            contextMenu.Items.Add(runScriptMenuItem);

            var scripts = Scripts()
                .Where(x => x.AddToRevisionGridContextMenu)
                .ToList();

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(new BindingList<ScriptInfo>(scripts));

            // Act
            _userScriptMenuBuilder.Build(contextMenu);

            // Assert
            Assert.That(runScriptMenuItem.Enabled, Is.True);
            Assert.That(runScriptMenuItem.Visible, Is.False);
        }

        [Test]
        public void Build_contextMenu_should_work()
        {
            // Arrange
            var contextMenu = new ContextMenuStrip();
            var runScriptMenuItem = new ToolStripMenuItem { Name = RunScriptMenuItemName };

            contextMenu.Items.Add(runScriptMenuItem);

            var scripts = Scripts();

            _scriptManagerMock.Setup(x => x.GetScripts())
                .Returns(scripts);

            // Act
            _userScriptMenuBuilder.Build(contextMenu);

            // Assert
            const int existItemsCount = 1;
            const int separatorsCount = 1;
            var newItemsCount = scripts
                .Where(x => x.Enabled)
                .Count(x => x.AddToRevisionGridContextMenu);

            var newItemsInRunScriptMenuCount = scripts
                .Where(x => x.Enabled)
                .Count(x => !x.AddToRevisionGridContextMenu);

            Assert.That(contextMenu.Enabled, Is.True);
            Assert.That(contextMenu.Items.Count, Is.EqualTo(existItemsCount + separatorsCount + newItemsCount));
            Assert.That(runScriptMenuItem.Enabled, Is.True);
            Assert.That(runScriptMenuItem.DropDown.Items.Count, Is.EqualTo(newItemsInRunScriptMenuCount));
        }

        #endregion Build Context Menu

        private static BindingList<ScriptInfo> Scripts()
        {
            var scripts = new BindingList<ScriptInfo>();

            foreach (var enabled in new[] { false, true })
            {
                foreach (var command in new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() })
                {
                    foreach (var addToContextmenu in new[] { false, true })
                    {
                        foreach (ScriptEvent onEvent in Enum.GetValues(typeof(ScriptEvent)))
                        {
                            foreach (var icon in new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() })
                            {
                                scripts.Add(new ScriptInfo
                                {
                                    Name = Guid.NewGuid().ToString(),
                                    Enabled = enabled,
                                    Command = command,
                                    AddToRevisionGridContextMenu = addToContextmenu,
                                    OnEvent = onEvent,
                                    Icon = icon
                                });
                            }
                        }
                    }
                }
            }

            return scripts;
        }
    }
}
