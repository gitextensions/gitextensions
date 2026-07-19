using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/MergeConflictHandler.cs. Merge and rebase recovery use
// their same-named dialogs; middle-of-patch recovery remains unavailable until the
// apply-patch workflow is ported.
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
