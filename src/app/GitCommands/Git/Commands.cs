using GitCommands.Git.Extensions;
using GitCommands.Git.Tag;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitCommands.Git;

public static partial class Commands
{
    public static IGitCommand CheckoutBranch(
        string branchName,
        bool remote,
        LocalChangesAction localChanges = LocalChangesAction.DontChange,
        CheckoutNewBranchMode newBranchMode = CheckoutNewBranchMode.DontCreate,
        string? newBranchName = null)
    {
        return new GitCommand(accessesRemote: false, changesRepoState: true,
            new GitArgumentBuilder("checkout")
            {
                { localChanges == LocalChangesAction.Merge, "--merge" },
                { localChanges == LocalChangesAction.Reset, "--force" },
                { remote && newBranchMode == CheckoutNewBranchMode.Create, $"-b {newBranchName.Quote()}" },
                { remote && newBranchMode == CheckoutNewBranchMode.Reset, $"-B {newBranchName.Quote()}" },
                branchName.QuoteNE()
            });
    }

    public static IGitCommand CreateTag(GitCreateTagArgs gitCreateTagArgs, string? tagMessageFileName, Func<string, string?> getPathForGitExecution)
    {
        Validate(gitCreateTagArgs, tagMessageFileName);

        return new GitCommand(accessesRemote: false, changesRepoState: true,
            new GitArgumentBuilder("tag")
            {
                    { gitCreateTagArgs.Force, "-f" },
                    GetArgumentForOperation(gitCreateTagArgs),
                    { gitCreateTagArgs.Operation.CanProvideMessage(), $"-F {getPathForGitExecution(tagMessageFileName).Quote()}" },
                    gitCreateTagArgs.TagName.Trim().Quote(),
                    "--",
                    gitCreateTagArgs.ObjectId
            });

        static string? GetArgumentForOperation(GitCreateTagArgs gitCreateTagArgs)
        {
            return gitCreateTagArgs.Operation switch
            {
                /* Lightweight */
                TagOperation.Lightweight => null,
                /* Annotate */
                TagOperation.Annotate => "-a",
                /* Sign with default GPG */
                TagOperation.SignWithDefaultKey => "-s",
                /* Sign with specific GPG */
                TagOperation.SignWithSpecificKey => $"-u {gitCreateTagArgs.SignKeyId}",
                _ => throw new NotSupportedException($"Invalid tag operation: {gitCreateTagArgs.Operation}")
            };
        }

        static void Validate(GitCreateTagArgs gitCreateTagArgs, string tagMessageFileName)
        {
            if (gitCreateTagArgs.ObjectId is null)
            {
                throw new ArgumentException("Revision is required.");
            }

            if (string.IsNullOrWhiteSpace(gitCreateTagArgs.TagName))
            {
                throw new ArgumentException("TagName is required.");
            }

            if (gitCreateTagArgs.Operation.CanProvideMessage() && string.IsNullOrWhiteSpace(tagMessageFileName))
            {
                throw new ArgumentException("TagMessageFileName is required.");
            }

            if (gitCreateTagArgs.Operation == TagOperation.SignWithSpecificKey && string.IsNullOrWhiteSpace(gitCreateTagArgs.SignKeyId))
            {
                throw new ArgumentException("SignKeyId is required.");
            }
        }
    }

    public static IGitCommand DeleteBranch(IReadOnlyCollection<IGitRef> branches, bool force)
    {
        ArgumentNullException.ThrowIfNull(branches);
        if (branches.Count == 0)
        {
            throw new ArgumentException("At least one branch is required.", nameof(branches));
        }

        bool hasRemoteBranch = branches.Any(branch => branch.IsRemote);
        bool hasNonRemoteBranch = branches.Any(branch => !branch.IsRemote);
        return new GitCommand(accessesRemote: false, changesRepoState: true,
            new GitArgumentBuilder("branch")
            {
                    { "--delete" },
                    { force, "--force" },
                    { hasRemoteBranch && hasNonRemoteBranch, "--all" },
                    { hasRemoteBranch && !hasNonRemoteBranch, "--remotes" },
                    branches.Select(branch => branch.Name.Quote())
            });
    }

    public static IGitCommand DeleteRemoteBranches(string remote, IEnumerable<string> branchLocalNames)
    {
        ArgumentNullException.ThrowIfNull(remote);
        ArgumentNullException.ThrowIfNull(branchLocalNames);

        return new GitCommand(accessesRemote: true, changesRepoState: true,
            new GitArgumentBuilder("push")
            {
                    remote,
                    branchLocalNames.Select(branch => $":refs/heads/{branch.Quote()}")
            });
    }
}
