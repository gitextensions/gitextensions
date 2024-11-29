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
    /// <returns>A <see cref="MemoryStream"/> with the file contents or <see langword="null"/> on error.</returns>
    public static MemoryStream? GetFileStream(string blob, IGitCommandRunner gitCommandRunner)
    {
        try
        {
            MemoryStream stream = CatFile(blob, gitCommandRunner);
            return MightBeLfsPointer(stream)
                ? ApplyLfsSmudge(stream, gitCommandRunner)
                : stream;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
            return null;
        }
    }

    private static MemoryStream CatFile(string blob, IGitCommandRunner gitCommandRunner)
    {
        GitArgumentBuilder args = new("cat-file")
        {
                "blob",
                blob
        };
        using IProcess process = gitCommandRunner.RunDetached(CancellationToken.None, args, redirectOutput: true);

        MemoryStream stream = new();
        process.StandardOutput.BaseStream.CopyTo(stream);
        stream.Position = 0;
        return stream;
    }

    private static MemoryStream ApplyLfsSmudge(MemoryStream lfsPointerStream, IGitCommandRunner gitCommandRunner)
    {
        try
        {
            GitArgumentBuilder args = new("lfs") { "smudge" };
            using IProcess process = gitCommandRunner.RunDetached(CancellationToken.None, args, redirectInput: true, redirectOutput: true, throwOnErrorExit: false);

            lfsPointerStream.CopyTo(process.StandardInput.BaseStream);
            process.StandardInput.Close();
            lfsPointerStream.Position = 0;

            MemoryStream smudgedStream = new();
            process.StandardOutput.BaseStream.CopyTo(smudgedStream);
            smudgedStream.Position = 0;

            string errorOutput = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(errorOutput))
            {
                Trace.WriteLine(errorOutput);
                return lfsPointerStream;
            }

            if (process.WaitForExit() != ExecutionResult.Success)
            {
                return lfsPointerStream;
            }

            lfsPointerStream.Dispose();

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

        char[] buffer = new char[lfsVersionPrefix.Length];
        using (StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, buffer.Length, leaveOpen: true))
        {
            reader.Read(buffer);
        }

        stream.Position = 0;

        return lfsVersionPrefix.AsSpan().Equals(buffer, StringComparison.Ordinal);
    }
}
