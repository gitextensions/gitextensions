using System.Diagnostics;
using System.Text;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;

namespace GitCommands.Git;

internal static class GitFileStreamGetter
{
    /// <summary>
    ///  Gets the plain file contents from Git.
    ///  If the file starts with the version string of a LFS pointer, the LFS Smudge filter is applied.
    /// </summary>
    /// <param name="blob">The Git file identifier.</param>
    /// <param name="gitCommandRunner">The wrapper for the Git executable, which handles file encoding.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="MemoryStream"/> with the file contents or <see langword="null"/> on error.</returns>
    public static async Task<MemoryStream?> GetFileStreamAsync(string blob, IGitCommandRunner gitCommandRunner, CancellationToken cancellationToken)
    {
        try
        {
            MemoryStream stream = await CatFileAsync(blob, gitCommandRunner, cancellationToken);
            return MightBeLfsPointer(stream)
                ? await ApplyLfsSmudgeAsync(stream, gitCommandRunner, cancellationToken)
                : stream;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
            return null;
        }
    }

    private static async Task<MemoryStream> CatFileAsync(string blob, IGitCommandRunner gitCommandRunner, CancellationToken cancellationToken)
    {
        GitArgumentBuilder args = new("cat-file")
        {
                "blob",
                blob
        };
        using IProcess process = gitCommandRunner.RunDetached(CancellationToken.None, args, redirectOutput: true);

        MemoryStream stream = new();
        await process.StandardOutput.BaseStream.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;
        return stream;
    }

    private static async Task<MemoryStream> ApplyLfsSmudgeAsync(MemoryStream lfsPointerStream, IGitCommandRunner gitCommandRunner, CancellationToken cancellationToken)
    {
        try
        {
            GitArgumentBuilder args = new("lfs") { "smudge" };
            using IProcess process = gitCommandRunner.RunDetached(CancellationToken.None, args, redirectInput: true, redirectOutput: true, throwOnErrorExit: false);

            await lfsPointerStream.CopyToAsync(process.StandardInput.BaseStream, cancellationToken);
            process.StandardInput.Close();
            lfsPointerStream.Position = 0;

            MemoryStream smudgedStream = new();
            await process.StandardOutput.BaseStream.CopyToAsync(smudgedStream, cancellationToken);
            smudgedStream.Position = 0;

            int exitCode = await process.WaitForExitAsync(cancellationToken);

            string errorOutput = process.StandardError;
            if (!string.IsNullOrWhiteSpace(errorOutput))
            {
                Trace.WriteLine(errorOutput);
                return lfsPointerStream;
            }

            if (exitCode != ExecutionResult.Success)
            {
                return lfsPointerStream;
            }

            await lfsPointerStream.DisposeAsync();

            return smudgedStream;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
            return lfsPointerStream;
        }
    }

    private static bool MightBeLfsPointer(MemoryStream stream)
    {
        const string lfsVersionPrefix = "version https://git-lfs.github.com/spec/v";

        if (stream.Length < lfsVersionPrefix.Length)
        {
            return false;
        }

        string prefix = Encoding.UTF8.GetString(stream.GetBuffer(), 0, lfsVersionPrefix.Length);
        return prefix == lfsVersionPrefix;
    }
}
