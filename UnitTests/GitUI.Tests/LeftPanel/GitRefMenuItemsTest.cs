using System.Runtime.CompilerServices;
using GitUI.LeftPanel.ContextMenu;
using GitUI.LeftPanel.Interfaces;
using NSubstitute;
using ResourceManager;

namespace GitUITests.LeftPanel
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class GitRefMenuItemsTest
    {
        private const int expectedMenuItems = 7;
        private const int expectedTotal = expectedMenuItems + 1; // + end separator
        private Queue<ToolStripMenuItem> _factoryQueue = new();
        private IMenuItemFactory _factory = null;
        private TestBranchNode _testNode = new();

        [SetUp]
        public void Setup()
        {
            _factory = Substitute.For<IMenuItemFactory>();
            _factory.CreateMenuItem<ToolStripMenuItem, TestBranchNode>(
                Arg.Do<Action<TestBranchNode>>(arg => _factoryQueue.Peek().Click += (sender, e) => arg(_testNode)),
                Arg.Any<TranslationString>(),
                Arg.Any<TranslationString>(),
                Arg.Any<Bitmap>())
                .Returns(_ => _factoryQueue.Dequeue());

            Enumerable.Range(0, expectedMenuItems).ForEach(_ => _factoryQueue.Enqueue(new ToolStripMenuItem()));
        }

        [Test]
        public void WithInactiveLocalBranch_HasAllMenuItems()
        {
            // Arrange
            var group = CreateGenerator(_factory);
            new LocalBranchMenuItemsStrings().ApplyTo(group.Strings);
            WithRefMenu_HasAllItems(group);
        }

        [Test]
        public void WithRemoteBranch_HasAllMenuItems()
        {
            // Arrange
            var group = CreateGenerator(_factory);
            new RemoteBranchMenuItemsStrings().ApplyTo(group.Strings);
            WithRefMenu_HasAllItems(group);
        }

        [Test]
        public void WithTagNode_HasAllMenuItems()
        {
            // Arrange
            var group = CreateGenerator(_factory);
            new TagMenuItemsStrings().ApplyTo(group.Strings);

            // mock rename to keep the test simple
            group.Strings.Tooltips[MenuItemKey.Rename] = new TranslationString("Rename");
            WithRefMenu_HasAllItems(group);
        }

        private void WithRefMenu_HasAllItems(MenuItemsGenerator<TestBranchNode> group)
        {
            // Act
            var menuItems = group.ToArray();
            Assert.IsEmpty(_factoryQueue);
            Assert.AreEqual(menuItems.Count(), expectedTotal);
            int testIndex = 0;
            AssertItem(menuItems[testIndex++], nameof(TestBranchNode.Checkout));
            AssertItem(menuItems[testIndex++], nameof(TestBranchNode.Merge));
            AssertItem(menuItems[testIndex++], nameof(TestBranchNode.Rebase));
            AssertItem(menuItems[testIndex++], nameof(TestBranchNode.CreateBranch));
            AssertItem(menuItems[testIndex++], nameof(TestBranchNode.Reset));
            Assert.IsInstanceOf<ToolStripSeparator>(menuItems[testIndex++].Item);
            AssertItem(menuItems[testIndex++], nameof(TestBranchNode.Rename));
            AssertItem(menuItems[testIndex++], nameof(TestBranchNode.Delete));
        }

        [Test]
        public void WithCurrentLocalBranch_HavingDisabledItems()
        {
            // Arrange
            LocalBranchMenuItems<TestBranchNode> generator = new(_factory);

            // Act
            const int expectedEnabled = 2; // create branch, rename
            int expectedDisabled = expectedTotal - expectedEnabled;
            var disabledItems = generator.Where(t => !LocalBranchMenuItems<TestBranchNode>.CurrentBranchItemKeys.Contains(t.Key)).ToArray();
            Assert.AreEqual(disabledItems.Count(), expectedDisabled);
            int testIndex = 0;
            AssertItem(disabledItems[testIndex++], nameof(TestBranchNode.Checkout));
            AssertItem(disabledItems[testIndex++], nameof(TestBranchNode.Merge));
            AssertItem(disabledItems[testIndex++], nameof(TestBranchNode.Rebase));
            AssertItem(disabledItems[testIndex++], nameof(TestBranchNode.Reset));
            Assert.IsInstanceOf<ToolStripSeparator>(disabledItems[testIndex++].Item);
            AssertItem(disabledItems[testIndex++], nameof(TestBranchNode.Delete));
        }

        private void AssertItem(ToolStripItemWithKey menuItem, string caption)
        {
            var item = menuItem.Item as ToolStripMenuItem;
            item.PerformClick();
            Assert.AreEqual(caption, _testNode.CallStatck.Pop());
        }

        private static MenuItemsGenerator<TestBranchNode> CreateGenerator(IMenuItemFactory factory)
        {
            return new MenuItemsGenerator<TestBranchNode>(factory);
        }

        // can't use a substitute here because of class constraint on INode
        public class TestBranchNode : INode, IGitRefActions, ICanDelete, ICanRename
        {
            public Stack<string> CallStatck { get; } = new Stack<string>();
            public string FullPath => throw new NotImplementedException();

            public bool Checkout()
            {
                return Trace();
            }

            public bool CreateBranch()
            {
                return Trace();
            }

            public bool Delete()
            {
                return Trace();
            }

            public bool Merge()
            {
                return Trace();
            }

            public bool Rebase()
            {
                return Trace();
            }

            public bool Rename()
            {
                return Trace();
            }

            public bool Reset()
            {
                return Trace();
            }

            private bool Trace([CallerMemberName] string name = "")
            {
                CallStatck.Push(name);
                return true;
            }
        }
    }
}
