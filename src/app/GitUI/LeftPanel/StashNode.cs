using System.Diagnostics;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Properties;

namespace GitUI.LeftPanel
{
    [DebuggerDisplay("(Tag) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
    internal sealed class StashNode : BaseRevisionNode
    {
        public StashNode(Tree tree, in ObjectId? objectId, string reflogSelector, string subject, bool visible)
            : base(tree, reflogSelector.RemovePrefix("refs/"), visible)
        {
            ObjectId = objectId;
            DisplayName = $"{reflogSelector.RemovePrefix(GitRefName.RefsStashPrefix)}: {subject}";
            ReflogSelector = reflogSelector;
        }

        public string DisplayName { get; }
        public string ReflogSelector { get; }

        internal override void OnSelected()
        {
            if (Tree.IgnoreSelectionChangedEvent)
            {
                return;
            }

            base.OnSelected();
            SelectRevision();
        }

        internal override void OnDoubleClick()
        {
            OpenStash(TreeViewNode.TreeView);
        }

        internal bool OpenStash(IWin32Window owner)
        {
            return UICommands.StartStashDialog(owner, manageStashes: true, ReflogSelector);
        }

        public void ApplyStash(IWin32Window owner)
        {
            UICommands.StashApply(owner, ReflogSelector);
        }

        public void PopStash(IWin32Window owner)
        {
            UICommands.StashPop(owner, ReflogSelector);
        }

        public void DropStash(IWin32Window owner)
        {
            using (new WaitCursorScope())
            {
                TaskDialogButton result;
                if (AppSettings.DontConfirmStashDrop)
                {
                    result = TaskDialogButton.Yes;
                }
                else
                {
                    TaskDialogPage page = new()
                    {
                        Text = TranslatedStrings.AreYouSure,
                        Caption = TranslatedStrings.StashDropConfirmTitle,
                        Heading = TranslatedStrings.CannotBeUndone,
                        Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
                        Icon = TaskDialogIcon.Information,
                        Verification = new TaskDialogVerificationCheckBox
                        {
                            Text = TranslatedStrings.DontShowAgain
                        },
                        SizeToContent = true
                    };

                    result = TaskDialog.ShowDialog(owner, page);

                    if (page.Verification.Checked)
                    {
                        AppSettings.DontConfirmStashDrop = true;
                    }
                }

                if (result == TaskDialogButton.Yes)
                {
                    UICommands.StashDrop(owner, ReflogSelector);
                }
            }
        }

        public override void ApplyStyle()
        {
            base.ApplyStyle();

            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                Visible
                    ? nameof(Images.Stash)
                    : nameof(Images.EyeClosed);
        }

        protected override string DisplayText()
        {
            return DisplayName;
        }
    }
}
