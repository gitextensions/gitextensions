using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/MergeConflictHandler.cs. The middle-of-patch and
// middle-of-rebase follow-ups surface as "not ported" until their dialogs arrive.
public static class MergeConflictHandler
{
    public static bool HandleMergeConflicts(IGitUICommands commands, IWin32Window? owner, bool offerCommit = true, bool offerUpdateSubmodules = true)
    {
        if (commands.Module.InTheMiddleOfConflictedMerge())
        {
            if (AppSettings.DontConfirmResolveConflicts || MessageBoxes.ConfirmResolveMergeConflicts(owner))
            {
                SolveMergeConflicts(commands, owner, offerCommit);
            }

            return true;
        }

        if (offerUpdateSubmodules)
        {
            commands.UpdateSubmodules(owner);
        }

        return false;
    }

    private static void SolveMergeConflicts(IGitUICommands commands, IWin32Window? owner, bool offerCommit)
    {
        if (commands.Module.InTheMiddleOfConflictedMerge())
        {
            commands.StartResolveConflictsDialog(owner, offerCommit);
        }

        if (commands.Module.InTheMiddleOfPatch())
        {
            if (MessageBoxes.MiddleOfPatchApply(owner))
            {
                commands.StartApplyPatchDialog(owner);
            }
        }
        else if (commands.Module.InTheMiddleOfRebase())
        {
            if (MessageBoxes.MiddleOfRebase(owner))
            {
                commands.StartTheContinueRebaseDialog(owner);
            }
        }
    }
}
