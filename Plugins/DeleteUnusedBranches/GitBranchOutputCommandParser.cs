namespace GitExtensions.Plugins.DeleteUnusedBranches;

public class GitBranchOutputCommandParser
{
    public IEnumerable<string> GetBranchNames(string commandOutput, bool isRemote)
    {
        if (commandOutput is null)
        {
            yield break;
        }

        foreach (string line in commandOutput.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line) && line.Length > 2))
        {
            // Removing first 2 chars: 1st is '*' for current branch or '+' for a worktree branch followed by a space
            // Symbolic refs are listed like "  origin/HEAD -> origin/master" (see also below), just use the branch name.
            string branchName = line[2..].Trim(' ', '\r').Split(' ')[0];

            if (branchName.StartsWith('(') || branchName.Length == 0)
            {
                continue;
            }

            if (isRemote)
            {
                // Remote HEAD, included from e.g. GitLab as a symbolic link
                string[] remotes = branchName.Split('/');
                if (remotes.Length == 2 && remotes[1] == "HEAD")
                {
                    continue;
                }
            }
            else if (branchName == "HEAD")
            {
                // Local HEAD should normally be in .git/HEAD (rather than .git/refs/heads/HEAD)
                continue;
            }

            yield return branchName;
        }
    }
}
