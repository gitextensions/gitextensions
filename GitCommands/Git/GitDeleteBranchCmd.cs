using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GitCommands
{
    public class GitDeleteBranchCmd : GitCommand
    {
        private List<string> branchList = new List<string>();

        public bool Force { get; set; }
        public bool HasRemoteBranch { get; set; }
        public bool HasNonRemoteBranch { get; set; }

        public override string GitComandName()
        {
            return "branch";
        }

        public override void CollectArguments(List<string> argumentsList)
        {
            if (Force)
                argumentsList.Add("-D");
            else
                argumentsList.Add("-d");

            if (HasRemoteBranch)
                if (HasNonRemoteBranch)
                    argumentsList.Add("-a");
                else
                    argumentsList.Add("-r");

            foreach (string branch in branchList)
                argumentsList.Add("\""+branch+"\"");
        }

        public override bool AccessesRemote()
        {
            return false;
        }

        public void AddBranch(string branchName, bool isRemote) 
        {
            branchList.Add(branchName);
            if (isRemote)
                HasRemoteBranch = true;
            else
                HasNonRemoteBranch = true;
        }
    }
}
