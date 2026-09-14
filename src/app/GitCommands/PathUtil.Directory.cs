using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GitCommands;

public static partial class PathUtil
{
    private const string Kernel32LibraryName = "kernel32.dll";

    /// <summary>
    ///  Compares directory paths after resolving junctions, mount points and symbolic links.
    /// </summary>
    /// <remarks>
    ///  Falls back to normalized paths when a directory cannot be opened.
    /// </remarks>
    public static bool AreSameDirectory(string path1, string path2)
    {
        string fullPath1 = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path1));
        string fullPath2 = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path2));

        return string.Equals(fullPath1, fullPath2, _pathComparison)
            || string.Equals(GetFinalDirectoryPath(fullPath1), GetFinalDirectoryPath(fullPath2), _pathComparison);
    }

    private static unsafe string GetFinalDirectoryPath(string path)
    {
        const uint fileFlagBackupSemantics = 0x02000000;
        const int initialBufferSize = 512;

        using SafeFileHandle handle = CreateFileW(
            path, 0, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open, fileFlagBackupSemantics, IntPtr.Zero);
        if (handle.IsInvalid)
        {
            return path;
        }

        char[] buffer = new char[initialBufferSize];
        while (true)
        {
            uint length;
            fixed (char* bufferPointer = buffer)
            {
                length = GetFinalPathNameByHandleW(handle, bufferPointer, (uint)buffer.Length, 0);
            }

            if (length == 0)
            {
                return path;
            }

            if (length < buffer.Length)
            {
                return Path.TrimEndingDirectorySeparator(new string(buffer, 0, (int)length));
            }

            buffer = new char[checked((int)length + 1)];
        }
    }

    [LibraryImport(Kernel32LibraryName, StringMarshalling = StringMarshalling.Utf16)]
    private static partial SafeFileHandle CreateFileW(
        string fileName, uint desiredAccess, FileShare shareMode, IntPtr securityAttributes,
        FileMode creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

    [LibraryImport(Kernel32LibraryName)]
    private static unsafe partial uint GetFinalPathNameByHandleW(SafeFileHandle file, char* path, uint pathLength, uint flags);
}
