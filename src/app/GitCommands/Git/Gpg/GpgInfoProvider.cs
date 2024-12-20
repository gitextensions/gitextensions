using GitUI;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitCommands.Git.Gpg;

/// <summary>
/// Provides methods to load GPG information for a given Git revision.
/// </summary>
public interface IGpgInfoProvider
{
    /// <summary>
    /// Asynchronously loads GPG information for the specified Git revision.
    /// </summary>
    /// <param name="revision">The Git revision to load GPG information for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the GPG information, or <c>null</c> if no information is available.</returns>
    Task<GpgInfo?> LoadGpgInfoAsync(GitRevision? revision);
}

public class GpgInfoProvider(IGitGpgController gitGpgController) : IGpgInfoProvider
{
    private readonly IGitGpgController _gitGpgController = gitGpgController;

    public async Task<GpgInfo?> LoadGpgInfoAsync(GitRevision? revision)
    {
        if (!AppSettings.ShowGpgInformation.Value || revision?.ObjectId is null)
        {
            return null;
        }

        await TaskScheduler.Default;
        Task<CommitStatus> getCommitSignature = _gitGpgController.GetRevisionCommitSignatureStatusAsync(revision);
        Task<TagStatus> getTagSignature = _gitGpgController.GetRevisionTagSignatureStatusAsync(revision);
        await Task.WhenAll(getCommitSignature, getTagSignature);

        CommitStatus commitStatus = getCommitSignature.CompletedResult();
        TagStatus tagStatus = getTagSignature.CompletedResult();

        // Absence of Commit sign and Tag sign
        if (commitStatus == CommitStatus.NoSignature && tagStatus == TagStatus.NoTag)
        {
            return null;
        }

        return new GpgInfo(commitStatus,
                           _gitGpgController.GetCommitVerificationMessage(revision),
                           tagStatus,
                           _gitGpgController.GetTagVerifyMessage(revision));
    }
}
