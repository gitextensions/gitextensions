using GitUI;
using Microsoft.VisualStudio.Threading;

namespace CommonTestUtils;

public partial class GitModuleTestHelper : IDisposable
{
#if CI_BUILD
    static GitModuleTestHelper()
    {
        Console.WriteLine("GitModuleTestHelper: Disabling explicit clean-up for continuous integration test environment");
    }

    private void FileAndForgetCleanUp()
    {
    }

    public static void WaitForCleanUpCompletion()
    {
    }
#else
    static GitModuleTestHelper()
    {
        Console.WriteLine("GitModuleTestHelper: Will perform clean-up in background tasks");

        Epilogue.RegisterAfterSuiteAction(order: 2, WaitForCleanUpCompletion);
    }

    private static TaskManager CleanUpOperations = new(new JoinableTaskContext());

    private static void CleanUp(string path)
    {
        try
        {
            // Directory.Delete seems to intermittently fail, so delete the files first before deleting folders
            foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                if (File.GetAttributes(file).HasFlag(FileAttributes.ReparsePoint))
                {
                    continue;
                }

                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            // Delete tends to fail on the first try, so give it a few tries as a best effort.
            // By this point, all files have been deleted anyway, so this is mainly about removing
            // empty directories.
            for (int tries = 0; tries < 10; ++tries)
            {
                try
                {
                    Directory.Delete(path, true);
                    break;
                }
                catch
                {
                }
            }
        }
        catch
        {
            // do nothing
        }
    }

    private void FileAndForgetCleanUp()
    {
        Epilogue.RegisterAfterTestAction(() => { CleanUpOperations.FileAndForget(() => CleanUp(TemporaryPath)); });
    }

    public static void WaitForCleanUpCompletion()
    {
        CleanUpOperations.JoinPendingOperations();
    }
#endif
}
