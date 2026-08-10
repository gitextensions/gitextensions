using System.Diagnostics;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.Gource;
using GitUI.Compat;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class GourcePluginTests
{
    [TestCase(false, true)]
    [TestCase(true, false)]
    public void IsLaunchAvailable_should_reject_host_executable_in_Flatpak(bool isFlatpak, bool expected)
    {
        GourceStart.IsLaunchAvailable(isFlatpak).Should().Be(expected);
    }

    [AvaloniaTest]
    public void Gource_form_should_construct_with_original_layout_and_translation_keys()
    {
        using GourceStart form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(718);
        form.Height.Should().Be(165);
        form.FindControl<Grid>("tableLayoutPanel1")!.Height.Should().Be(99);
        form.FindControl<Button>("button1")!.Width.Should().Be(94);
        form.FindControl<TextBox>("Arguments").Should().NotBeNull();
        form.FindControl<TextBox>("GourcePath").Should().NotBeNull();
        form.FindControl<TextBox>("WorkingDir").Should().NotBeNull();

        AssertTranslation(translation, "$this", "Gource");
        AssertTranslation(translation, "ArgumentsLabel", "Arguments");
        AssertTranslation(translation, "GourceBrowse", "Browse");
        AssertTranslation(translation, "WorkingDirBrowse", "Browse");
        AssertTranslation(translation, "button1", "Start");
        AssertTranslation(translation, "label1", "Path to Gource");
        AssertTranslation(translation, "label2", "Repository");
        AssertTranslation(translation, "linkLabel1", "Gource project");
        AssertTranslation(translation, "linkLabel2", "Gource command line");

        static void AssertTranslation(ITranslation translation, string item, string text)
        {
            translation.Received(1).AddTranslationItem(nameof(GourceStart), item, "Text", text);
        }
    }

    [AvaloniaTest]
    public void Gource_plugin_should_expose_its_icon_and_native_string_settings()
    {
        GourcePlugin plugin = new();
        ISetting[] settings = plugin.GetSettings().ToArray();

        plugin.Id.Should().Be(new Guid("F0A6A769-6DCC-4452-9A43-343347015EEC"));
        PluginIconProvider.GetIcon(plugin).Should().NotBeNull();
        settings.Select(setting => setting.Name).Should().Equal("Path to Gource", "Arguments");
        settings.Should().AllBeOfType<StringSetting>();
        settings.Select(PluginSettingControlFactory.Create)
            .Select(binding => binding.Control)
            .Should().AllBeOfType<TextBox>();
    }

    [Test]
    public void Gource_process_should_launch_without_a_shell_or_executable_quoting()
    {
        ProcessStartInfo startInfo = GourceStart.CreateProcessStartInfo(
            "/opt/Gource App/gource",
            "--hide filenames --seconds-per-day 0.5",
            "/tmp/repository path");

        startInfo.FileName.Should().Be("/opt/Gource App/gource");
        startInfo.Arguments.Should().Be("--hide filenames --seconds-per-day 0.5");
        startInfo.WorkingDirectory.Should().Be("/tmp/repository path");
        startInfo.UseShellExecute.Should().BeFalse();
    }

    [Test]
    public void Gource_plugin_should_offer_the_windows_archive_only_on_Windows()
    {
        GourcePlugin.ShouldOfferAutomaticDownload.Should().Be(OperatingSystem.IsWindows());
    }

    [Test]
    public void Avalonia_build_graph_should_include_every_bundled_plugin_project()
    {
        string repositoryRoot = FindRepositoryRoot();
        string[] expected = Directory.GetFiles(
                Path.Join(repositoryRoot, "src", "plugins"),
                "*.csproj",
                SearchOption.AllDirectories)
            .Select(path => NormalizeRelativePath(repositoryRoot, path))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        XDocument solution = XDocument.Load(Path.Join(repositoryRoot, "GitExtensions.Avalonia.slnx"));
        string[] solutionProjects = solution.Descendants("Project")
            .Select(project => project.Attribute("Path")?.Value)
            .Where(path => path?.StartsWith("src/plugins/", StringComparison.OrdinalIgnoreCase) == true)
            .Select(path => path!)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        solutionProjects.Should().Equal(expected);

        string entryProjectPath = Path.Join(
            repositoryRoot,
            "src",
            "app",
            "GitExtensions.Avalonia",
            "GitExtensions.Avalonia.csproj");
        string entryProjectDirectory = Path.GetDirectoryName(entryProjectPath)!;
        XDocument entryProject = XDocument.Load(entryProjectPath);
        string[] buildOnlyPluginProjects = entryProject.Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")?.Value)
            .Where(path => path?.Contains("plugins", StringComparison.OrdinalIgnoreCase) == true)
            .Select(path => ResolveProjectReference(repositoryRoot, entryProjectDirectory, path!))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        string[] expectedBuildOnlyProjects = expected
            .Where(path => !path.EndsWith("GitUIPluginInterfaces.csproj", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        buildOnlyPluginProjects.Should().Equal(expectedBuildOnlyProjects);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !Directory.Exists(Path.Join(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The test checkout root was not found.");
    }

    private static string NormalizeRelativePath(string repositoryRoot, string path)
        => Path.GetRelativePath(repositoryRoot, path).Replace('\\', '/');

    private static string ResolveProjectReference(string repositoryRoot, string projectDirectory, string include)
    {
        string nativeInclude = include
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        return NormalizeRelativePath(repositoryRoot, Path.GetFullPath(Path.Combine(projectDirectory, nativeInclude)));
    }
}
