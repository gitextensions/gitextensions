namespace CommonTestUtils;

public class GitModuleTestSnapshot
{
    private readonly string _repositoryName;
    private readonly List<string> _folders = [];
    private readonly List<(string FilePath, byte[] Content)> _files = [];

    public GitModuleTestSnapshot(string repositoryName, string path)
    {
        _repositoryName = repositoryName;

        DirectoryInfo sourcePath = new(path);

        PopulateFrom(sourcePath, relativePath: "");
    }

    public GitModuleTestHelper Clone()
    {
        return CloneTo(FileSystemUtility.GetTemporaryPath());
    }

    public GitModuleTestHelper CloneTo(string path)
    {
        foreach (string folderPath in _folders)
        {
            Directory.CreateDirectory(Path.Combine(path, folderPath));
        }

        foreach ((string filePath, byte[] content) in _files)
        {
            File.WriteAllBytes(Path.Combine(path, filePath), content);
        }

        return new GitModuleTestHelper(_repositoryName, path, useExisting: true);
    }

    private void PopulateFrom(DirectoryInfo sourcePath, string relativePath)
    {
        foreach (FileSystemInfo entry in sourcePath.EnumerateFileSystemInfos())
        {
            string entryPath = Path.Combine(relativePath, entry.Name);

            if (entry is DirectoryInfo subPath)
            {
                _folders.Add(entryPath);

                PopulateFrom(subPath, entryPath);
            }
            else
            {
                _files.Add((entryPath, File.ReadAllBytes(entry.FullName)));
            }
        }
    }
}
