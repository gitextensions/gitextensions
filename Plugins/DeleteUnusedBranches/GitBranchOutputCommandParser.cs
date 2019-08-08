using System.Collections.Generic;
using System.Linq;

namespace DeleteUnusedBranches
{
    public class GitBranchOutputCommandParser
    {
        public IEnumerable<string> GetBranchNames(string commandOutput)
        {
            if (commandOutput == null)
            {
                yield break;
            }

            foreach (var line in commandOutput.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                var branchName = line.Trim('*', ' ', '\n', '\r');

                if (branchName != "HEAD")
                {
                    yield return branchName;
                }
            }
        }
    }
}
