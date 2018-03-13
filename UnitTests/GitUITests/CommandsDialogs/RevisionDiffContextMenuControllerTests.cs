using FluentAssertions;
using GitCommands;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class RevisionDiffContextMenuControllerTests
    {
        private FileStatusListContextMenuController _revisionDiffContextMenuController;

        [SetUp]
        public void Setup()
        {
            _revisionDiffContextMenuController = new FileStatusListContextMenuController();
        }

        public void BrowseDiff_SuppressDiffToLocalWhenNoSelectedRevision()
        {
            var selectionInfo = new ContextMenuDiffToolInfo();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_SuppressDiffToLocalWhenNoLocalExists()
        {
            var rev = new GitRevision("1234567890");
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev, localExists: false);
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_ShowContextDiffToolForUnstaged()
        {
            var rev = new GitRevision(GitRevision.UnstagedGuid);
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev);
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeTrue();
        }

        [Test]
        public void BrowseDiff_ShowContextDiffToolForUnstagedParent()
        {
            var rev = new GitRevision("1234567890");
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev, selectedItemParentRevs: new string[] { GitRevision.UnstagedGuid });
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeTrue();
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void BrowseDiff_ShowContextDiffToolForDeletedAndNew(bool d, bool n)
        {
            var rev = new GitRevision("1234567890");
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev, allAreDeleted: d, allAreNew: n);
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().Be(!d);
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().Be(!n);
        }
    }
}