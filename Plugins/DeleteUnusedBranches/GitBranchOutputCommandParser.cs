namespace GitExtensions.Plugins.DeleteUnusedBranches
{
    public class GitBranchOutputCommandParser
    {
        public IEnumerable<string> GetBranchNames(string commandOutput)
        {
            if (commandOutput is null)
            {
                yield break;
            }

            foreach (string line in commandOutput.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line) && line.Length > 2))
            {
                // Removing first 2 chars: 1st is '*' for current branch or '+' for a worktree branch followed by a space
                string branchName = line[2..].Trim(' ', '\n', '\r');

                if (branchName != "HEAD" && !branchName.StartsWith("(") && branchName.Length != 0)
                {
                    yield return branchName;
                }
            }
        }
    }
}
