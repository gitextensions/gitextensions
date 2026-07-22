using System.Diagnostics;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;

using ResourceManager;

namespace GitUI.LeftPanel;

[DebuggerDisplay("(Stash) ReflogSelector = {ReflogSelector}, Hash = {ObjectId}")]
internal sealed class StashNode
{
    private readonly StashTree _tree;

    public StashNode(StashTree tree, in ObjectId objectId, string reflogSelector, string subject)
    {
        _tree = tree;
        ObjectId = objectId;
        DisplayName = $"{reflogSelector.RemovePrefix(GitRefName.RefsStashPrefix)}: {subject}";
        ReflogSelector = reflogSelector;
    }

    public string DisplayName { get; }

    public string ReflogSelector { get; }

    public ObjectId ObjectId { get; }

    internal bool OpenStash(IWin32Window owner)
    {
        return _tree.UICommands.StartStashDialog(owner, manageStashes: true, ReflogSelector);
    }

    public void ApplyStash(IWin32Window owner)
    {
        _tree.UICommands.StashApply(owner, ReflogSelector);
    }

    public void PopStash(IWin32Window owner)
    {
        _tree.UICommands.StashPop(owner, ReflogSelector);
    }

    public void DropStash(IWin32Window owner)
    {
        using (WaitCursorScope.Enter())
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
                        Text = TranslatedStrings.DontShowAgain,
                    },
                    SizeToContent = true,
                };

                result = TaskDialog.ShowDialog(owner, page);

                if (page.Verification.Checked)
                {
                    AppSettings.DontConfirmStashDrop = true;
                }
            }

            if (result == TaskDialogButton.Yes)
            {
                _tree.UICommands.StashDrop(owner, ReflogSelector);
            }
        }
    }
}
