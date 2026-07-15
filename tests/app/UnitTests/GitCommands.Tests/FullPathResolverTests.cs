using GitCommands;

namespace GitCommandsTests;
public class FullPathResolverTests
{
    // The resolver roots and combines paths with the rules of the running platform, so the
    // working directory it is given must be a native one.
    private static readonly string _workingDir = OperatingSystem.IsWindows() ? @"c:\dev\repo" : "/dev/repo";
    private FullPathResolver _resolver = null!;

    private static IEnumerable<string> RootedPaths()
    {
        yield return OperatingSystem.IsWindows() ? @"c:\" : "/";
    }

    private static IEnumerable<string> WorkingDirs()
    {
        if (OperatingSystem.IsWindows())
        {
            yield return @"C:\dev\repo";
            yield return @"C:\dev\repo\";
            yield return @"C:\dev\repo/";
            yield return @"C:\dev\c#\repo\";
        }
        else
        {
            yield return "/dev/repo";
            yield return "/dev/repo/";
            yield return "/dev/c#/repo/";
        }
    }

    [SetUp]
    public void Setup()
    {
        _resolver = new FullPathResolver(() => _workingDir);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Resolve_should_return_null_if_path_null_or_illegal_chars(string? path)
    {
        _resolver.Resolve(path).Should().BeNull();
    }

    [TestCaseSource(nameof(RootedPaths))]
    public void Resolve_should_return_original_path_if_rooted(string path)
    {
        _resolver.Resolve(path).Should().Be(path);
    }

    [Test]
    public void Resolve_should_return_long_full_path()
    {
        string dir = $"folder{Path.DirectorySeparatorChar}";
        string path = string.Concat(Enumerable.Repeat(dir, 10000)) + "filename.txt";
        _resolver.Resolve(path).Should().Be($"{_workingDir}{Path.DirectorySeparatorChar}{path}");
    }

    // The relative paths are written with '/' and converted to the native form here, so one
    // case covers both platforms; the expectation stays an explicit native concatenation.
    [TestCase("file")]
    [TestCase("folder/folder/folder/folder/folder/folder/folder/folder/folder/filename.txt")]
    [TestCase("folder/folder/folder/folder/folder/folder/folder/folder/folder#/filename.txt")]
    [TestCase("folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/folder/filename.txt")]
    public void Resolve_should_return_full_path(string path)
    {
        string nativePath = path.Replace('/', Path.DirectorySeparatorChar);
        _resolver.Resolve(nativePath).Should().Be($"{_workingDir}{Path.DirectorySeparatorChar}{nativePath}");
    }

    [TestCase("drivers/gpu/drm/nouveau/nvkm/subdev/i2c/aux.c")]
    public void Resolve_handles_system_filenames(string path)
    {
        _resolver.Resolve(path).Should().Be($"{_workingDir}{Path.DirectorySeparatorChar}{path.Replace('/', Path.DirectorySeparatorChar)}");
    }

    [TestCaseSource(nameof(WorkingDirs))]
    public void Resolve_combines_paths(string workingDir)
    {
        FullPathResolver resolver = new(() => workingDir);
        resolver.Resolve("file.txt").Should().Be(Path.Combine(workingDir, "file.txt").ToNativePath());
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Resolve_does_not_throw_on_invalid_workingDir(string? workingDir)
    {
        FullPathResolver resolver = new(() => workingDir!);
        resolver.Resolve("file.txt").Should().Be(Path.Combine(Environment.CurrentDirectory, "file.txt").ToNativePath());
    }
}
