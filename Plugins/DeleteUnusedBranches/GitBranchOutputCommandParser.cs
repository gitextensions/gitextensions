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

            foreach (string line in commandOutput.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                string branchName = line.Trim('*', ' ', '\n', '\r');

                if (branchName != "HEAD")
                {
                    yield return branchName;
                }
            }
        }
    }
}
