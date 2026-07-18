namespace GitExtensionsTests;

internal static class TestDirectory
{
    public static void Delete(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        if (!OperatingSystem.IsWindows())
        {
            Directory.Delete(path, recursive: true);
            return;
        }

        const int maximumAttempts = 10;
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (UnauthorizedAccessException) when (attempt < maximumAttempts)
            {
                ClearReadOnlyAttributes(path);
            }
            catch (IOException) when (attempt < maximumAttempts)
            {
            }

            Thread.Sleep(20 * attempt);
        }
    }

    private static void ClearReadOnlyAttributes(string path)
    {
        EnumerationOptions options = new()
        {
            AttributesToSkip = FileAttributes.ReparsePoint,
            RecurseSubdirectories = true,
        };

        foreach (string file in Directory.EnumerateFiles(path, "*", options))
        {
            FileAttributes attributes = File.GetAttributes(file);
            if ((attributes & FileAttributes.ReadOnly) != 0)
            {
                File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
            }
        }
    }
}
