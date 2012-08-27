using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands.Git
{
    public class GitCheckoutBranchCmd : GitCommand
    {

        public enum LocalChanges
        {
            DontChange,
            Merge,
            Reset
        }

        public enum NewBranch
        {
            DontCreate,
            Create,
            Reset
        }

        public string BranchName { get; set; }
        public string NewBranchName { get; set; }
        public bool Remote { get; set; }
        public LocalChanges LocalChangesAction { get; set; }
        public NewBranch NewBranchAction { get; set; }

        public GitCheckoutBranchCmd(string branchName, bool remote)
        {
            BranchName = branchName;
            Remote = remote;
        }

        public override string GitComandName()
        {
            return "checkout";
        }

        public override IEnumerable<string> CollectArguments()
        {
            if (LocalChangesAction == LocalChanges.Merge)
                yield return "--merge";
            else if (LocalChangesAction == LocalChanges.Reset)
                yield return "--force";

            if (Remote)
            { 
                if (NewBranchAction == NewBranch.Create)
                    yield return "-b " + NewBranchName.Quote();
                else if (NewBranchAction == NewBranch.Reset)
                    yield return "-B " + NewBranchName.Quote();
            }

            yield return BranchName.Quote();
        }

        public override bool AccessesRemote()
        {
            return false;
        }

        public void SetLocalChangesFromSettings(Settings.LocalChanges changes)
        {
            if (changes == Settings.LocalChanges.Merge)
                LocalChangesAction = LocalChanges.Merge;
            else if (changes == Settings.LocalChanges.Reset)
                LocalChangesAction = LocalChanges.Reset;
            else
                LocalChangesAction = LocalChanges.DontChange;
        }

    }
}
