using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Compat;
using GitUI.Theming;

namespace GitExtensionsTests;

[TestFixture]
public sealed partial class ThemeCapabilityTests
{
    [TestCase(false, true, 0xC8, 0xFF, 0xC8)]
    [TestCase(false, false, 0xFF, 0xC8, 0xC8)]
    [TestCase(true, true, 0x00, 0x95, 0x00)]
    [TestCase(true, false, 0x95, 0x00, 0x00)]
    public void Filter_background_should_match_the_original_resolved_color(
        bool isDark,
        bool isValid,
        byte red,
        byte green,
        byte blue)
    {
        AvaloniaThemeResources.ResolveFilterBackground(isDark, isValid)
            .Should().Be(System.Drawing.Color.FromArgb(red, green, blue));
    }

    [GeneratedRegex(@"\bSystemColors\.(?<name>[A-Za-z_][A-Za-z0-9_]*)\b")]
    private static partial Regex SystemColorRegex();

    [GeneratedRegex(@"\.(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\{\s*color:\s*#(?<hex>[0-9A-Fa-f]{6})")]
    private static partial Regex CssColorRegex();

    [GeneratedRegex(@"#[0-9A-Fa-f]{6}\b")]
    private static partial Regex HexColorRegex();

    [Test]
    public void App_color_resource_mapping_should_be_explicit_and_exhaustive()
    {
        AvaloniaThemeResources.MappedAppColors.Should().Equal(Enum.GetValues<AppColor>());
        AvaloniaThemeResources.MappedAppColors.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void System_color_resource_mapping_should_match_the_WinForms_source_inventory()
    {
        string repositoryRoot = FindRepositoryRoot();
        string originalRoot = Path.Combine(repositoryRoot, "src", "app", "GitUI");
        System.Drawing.KnownColor[] consumedColors = Directory
            .EnumerateFiles(originalRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsBuildOutput(path))
            .SelectMany(path => SystemColorRegex().Matches(File.ReadAllText(path)).Cast<Match>())
            .Select(match => Enum.Parse<System.Drawing.KnownColor>(match.Groups["name"].Value))
            .Distinct()
            .OrderBy(name => name.ToString(), StringComparer.Ordinal)
            .ToArray();

        AvaloniaThemeResources.MappedSystemColors
            .OrderBy(name => name.ToString(), StringComparer.Ordinal)
            .Should()
            .Equal(consumedColors);
        AvaloniaThemeResources.MappedSystemColors.Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void Every_shipped_theme_should_be_deployed_discoverable_and_loadable()
    {
        string repositoryRoot = FindRepositoryRoot();
        string sourceThemesDirectory = Path.Combine(repositoryRoot, "src", "app", "GitUI", "Themes");
        ThemePathProvider pathProvider = new();
        ThemeRepository repository = CreateRepository(pathProvider);
        string[] sourceFiles = Directory
            .EnumerateFiles(sourceThemesDirectory, "*.css", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray()!;
        string[] deployedFiles = Directory
            .EnumerateFiles(pathProvider.AppThemesDirectory, "*.css", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .Order(StringComparer.Ordinal)
            .ToArray()!;
        ThemeId[] expectedIds = sourceFiles
            .Select(Path.GetFileNameWithoutExtension)
            .Select(name => new ThemeId(name!, isBuiltin: true))
            .Distinct()
            .OrderBy(id => id.Name, StringComparer.Ordinal)
            .ToArray();
        ThemeId[] actualIds = repository
            .GetThemeIds()
            .Where(id => id.IsBuiltin)
            .OrderBy(id => id.Name, StringComparer.Ordinal)
            .ToArray();

        deployedFiles.Should().Contain(sourceFiles);
        actualIds.Should().Contain(expectedIds);
        foreach (ThemeId themeId in expectedIds)
        {
            Theme theme = repository.GetTheme(themeId, ThemeVariations.None);
            theme.Id.Should().Be(themeId);
            foreach (AppColor appColor in AvaloniaThemeResources.MappedAppColors)
            {
                System.Drawing.Color resolved = theme.GetColor(appColor);
                if (resolved.IsEmpty)
                {
                    resolved = repository.GetInvariantTheme().GetColor(appColor);
                }

                if (appColor == AppColor.GraphBranch8)
                {
                    resolved.IsEmpty.Should().BeTrue();
                }
                else
                {
                    resolved.IsEmpty.Should().BeFalse($"{themeId} should resolve {appColor}");
                }
            }
        }
    }

    [Test]
    public void User_and_portable_theme_paths_should_retain_the_original_contract()
    {
        string temporaryRoot = CreateTemporaryDirectory();
        AppSettings.TestAccessor settingsAccessor = AppSettings.GetTestAccessor();
        Lazy<string?> originalApplicationDataPath = settingsAccessor.ApplicationDataPath;
        try
        {
            string userDirectory = Path.Combine(temporaryRoot, "user");
            settingsAccessor.ApplicationDataPath = new Lazy<string?>(() => userDirectory);
            ThemePathProvider userPathProvider = new();
            Directory.CreateDirectory(userPathProvider.UserThemesDirectory!);
            ThemeId userThemeId = new("p1-3-user");
            File.WriteAllText(
                userPathProvider.GetThemePath(userThemeId),
                "@import url(\"invariant.css\");" + Environment.NewLine
                    + ".PanelBackground { color: #abcdef; }" + Environment.NewLine
                    + ".Branch { color: #123456; }");
            ThemeRepository userRepository = CreateRepository(userPathProvider);

            userRepository.GetThemeIds().Should().Contain(userThemeId);
            Theme userTheme = userRepository.GetTheme(userThemeId, ThemeVariations.None);
            userTheme.GetColor(AppColor.PanelBackground).ToArgb().Should().Be(
                System.Drawing.ColorTranslator.FromHtml("#abcdef").ToArgb());
            userTheme.GetColor(AppColor.Branch).ToArgb().Should().Be(
                System.Drawing.ColorTranslator.FromHtml("#123456").ToArgb());

            string applicationDirectory = Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);
            settingsAccessor.ApplicationDataPath = new Lazy<string?>(() => applicationDirectory);
            ThemePathProvider portablePathProvider = new();
            ThemeRepository portableRepository = CreateRepository(portablePathProvider);

            portablePathProvider.UserThemesDirectory.Should().BeNull();
            portableRepository.GetInvariantTheme().Id.Should().Be(ThemeId.DefaultLight);
            portablePathProvider.Invoking(provider => provider.GetThemePath(userThemeId))
                .Should()
                .Throw<InvalidOperationException>();
        }
        finally
        {
            settingsAccessor.ApplicationDataPath = originalApplicationDataPath;
            Directory.Delete(temporaryRoot, recursive: true);
        }
    }

    [AvaloniaTest]
    public void Runtime_theme_switch_should_repaint_an_existing_window()
    {
        Application application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application was not created.");
        ThemeId originalThemeId = AppSettings.ThemeId;
        string[] originalVariations = AppSettings.ThemeVariations;
        bool originalUseSystemVisualStyle = AppSettings.UseSystemVisualStyle;
        Window window = new()
        {
            Width = 320,
            Height = 160,
        };
        try
        {
            AppSettings.ThemeVariations = ThemeVariations.None;
            AppSettings.ThemeId = ThemeId.DefaultLight;
            AvaloniaThemeSettings.ApplyAppSettings();
            window.Show();
            Dispatcher.UIThread.RunJobs();
            GetBrushColor(window.Background).Should().Be(Color.Parse("#F0F0F0"));

            AppSettings.ThemeId = ThemeId.DefaultDark;
            AvaloniaThemeSettings.ApplyAppSettings();
            Dispatcher.UIThread.RunJobs();

            window.IsVisible.Should().BeTrue();
            GetBrushColor(window.Background).Should().Be(Color.Parse("#202020"));
        }
        finally
        {
            window.Close();
            AppSettings.ThemeId = originalThemeId;
            AppSettings.ThemeVariations = originalVariations;
            AppSettings.UseSystemVisualStyle = originalUseSystemVisualStyle;
            AvaloniaThemeSettings.ApplyAppSettings();
        }
    }

    [AvaloniaTest]
    public void Colors_page_should_apply_repaint_persist_and_reset_a_user_theme()
    {
        Application application = Application.Current
            ?? throw new InvalidOperationException("The Avalonia application was not created.");
        string temporaryRoot = CreateTemporaryDirectory();
        try
        {
            string settingsPath = Path.Combine(temporaryRoot, "test.settings");
            using GitExtSettingsCache settingsCache = GitExtSettingsCache.Create(settingsPath);
            DistributedSettings isolatedSettings = new(
                lowerPriority: null,
                settingsCache,
                SettingLevel.Unknown);
            AppSettings.UsingContainer(isolatedSettings, Run);
        }
        finally
        {
            Directory.Delete(temporaryRoot, recursive: true);
        }

        return;

        void Run()
        {
            AppSettings.TestAccessor settingsAccessor = AppSettings.GetTestAccessor();
            Lazy<string?> originalApplicationDataPath = settingsAccessor.ApplicationDataPath;
            ThemeId originalThemeId = AppSettings.ThemeId;
            string[] originalVariations = AppSettings.ThemeVariations;
            bool originalUseSystemVisualStyle = AppSettings.UseSystemVisualStyle;
            ThemeVariant? originalVariant = application.RequestedThemeVariant;
            Window? window = null;
            try
            {
                string userDirectory = Path.Combine(temporaryRoot, "user");
                settingsAccessor.ApplicationDataPath = new Lazy<string?>(() => userDirectory);
                ThemePathProvider pathProvider = new();
                Directory.CreateDirectory(pathProvider.UserThemesDirectory!);
                ThemeId userThemeId = new("p1-3-live");
                File.WriteAllText(
                    pathProvider.GetThemePath(userThemeId),
                    "@import url(\"invariant.css\");" + Environment.NewLine
                        + ".PanelBackground { color: #abcdef; }" + Environment.NewLine
                        + ".Branch { color: #123456; }");
                ThemeRepository repository = CreateRepository(pathProvider);

                AppSettings.ThemeId = ThemeId.DefaultLight;
                AppSettings.ThemeVariations = ThemeVariations.None;
                ThemeModule.TestAccessor.ReloadThemeSettings(repository);
                application.RequestedThemeVariant = ThemeVariant.Light;
                AvaloniaThemeResources.Apply(application, ThemeModule.Settings);

                ColorsSettingsPage page = new();
                page.LoadSettings();
                window = new Window
                {
                    Width = 900,
                    Height = 560,
                    RequestedThemeVariant = ThemeVariant.Light,
                    Content = page,
                };
                window.Show();
                Dispatcher.UIThread.RunJobs();
                GetBrushColor(window.Background).Should().Be(Color.Parse("#F0F0F0"));

                page.SelectedThemeId = userThemeId;
                page.GetTestAccessor().RestartNeeded.IsVisible.Should().BeTrue();
                page.SaveSettings();
                AppSettings.ThemeId.Should().Be(userThemeId);

                ThemeModule.TestAccessor.ReloadThemeSettings(repository);
                AvaloniaThemeResources.Apply(application, ThemeModule.Settings);
                Dispatcher.UIThread.RunJobs();

                GetBrushColor(window.Background).Should().Be(Color.Parse("#F0F0F0"));
                GetResourceBrushColor(application, AppColor.Branch).Should().Be(Color.Parse("#123456"));
                AssertEveryMappedColorIsPublished(application, ThemeVariant.Light);

                AppSettings.SettingsContainer.SettingsCache.Save();
                AppSettings.ThemeId = ThemeId.DefaultLight;
                AppSettings.SettingsContainer.SettingsCache.Load();
                AppSettings.ThemeId.Should().Be(userThemeId);
                ThemeModule.TestAccessor.ReloadThemeSettings(repository);
                AvaloniaThemeResources.Apply(application, ThemeModule.Settings);
                GetResourceBrushColor(application, AppColor.Branch).Should().Be(Color.Parse("#123456"));

                page.PopulateThemeMenu(Enumerable.Repeat(ThemeId.WindowsAppColorModeId, 1).Concat(repository.GetThemeIds()));
                page.SelectedThemeId = ThemeId.DefaultLight;
                page.SaveSettings();
                ThemeModule.TestAccessor.ReloadThemeSettings(repository);
                AvaloniaThemeResources.Apply(application, ThemeModule.Settings);
                Dispatcher.UIThread.RunJobs();

                GetBrushColor(window.Background).Should().Be(Color.Parse("#F0F0F0"));
                foreach (AppColor appColor in AvaloniaThemeResources.MappedAppColors)
                {
                    System.Drawing.Color expected = AppColorDefaults.GetBy(appColor);
                    string key = AvaloniaThemeResources.AppColorPrefix + appColor + "Brush";
                    if (expected.IsEmpty)
                    {
                        application.TryGetResource(key, ThemeVariant.Light, out _).Should().BeFalse();
                    }
                    else
                    {
                        GetResourceBrushColor(application, appColor).Should().Be(
                            Color.FromArgb(expected.A, expected.R, expected.G, expected.B));
                    }
                }
            }
            finally
            {
                window?.Close();
                settingsAccessor.ApplicationDataPath = originalApplicationDataPath;
                AppSettings.ThemeId = originalThemeId;
                AppSettings.ThemeVariations = originalVariations;
                AppSettings.UseSystemVisualStyle = originalUseSystemVisualStyle;
                AppSettings.SettingsContainer.SettingsCache.Save();
                AvaloniaThemeSettings.ApplyAppSettings();
                application.RequestedThemeVariant = originalVariant;
            }
        }
    }

    [Test]
    public void Configurable_color_literals_should_not_bypass_the_theme_boundary()
    {
        string repositoryRoot = FindRepositoryRoot();
        string invariantThemePath = Path.Combine(repositoryRoot, "src", "app", "GitUI", "Themes", "invariant.css");
        HashSet<string> configurableHexValues = CssColorRegex()
            .Matches(File.ReadAllText(invariantThemePath))
            .Cast<Match>()
            .Where(match => Enum.TryParse(match.Groups["name"].Value, out AppColor _))
            .Select(match => "#" + match.Groups["hex"].Value.ToUpperInvariant())
            .Where(hex => hex is not "#000000" and not "#FFFFFF")
            .ToHashSet(StringComparer.Ordinal);
        string twinRoot = Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia");
        string stylesFallback = Path.Combine(twinRoot, "Styles", "GitExtensionsVisualParity.axaml");
        string resourceBoundary = Path.Combine(twinRoot, "Compat", "AvaloniaThemeResources.cs");
        List<string> findings = [];

        foreach (string path in Directory.EnumerateFiles(twinRoot, "*", SearchOption.AllDirectories)
                     .Where(path => Path.GetExtension(path) is ".cs" or ".axaml")
                     .Where(path => !IsBuildOutput(path))
                     .Where(path => !string.Equals(path, stylesFallback, StringComparison.OrdinalIgnoreCase))
                     .Where(path => !string.Equals(path, resourceBoundary, StringComparison.OrdinalIgnoreCase)))
        {
            string[] lines = File.ReadAllLines(path);
            for (int index = 0; index < lines.Length; index++)
            {
                foreach (Match match in HexColorRegex().Matches(lines[index]))
                {
                    string hex = match.Value.ToUpperInvariant();
                    if (configurableHexValues.Contains(hex))
                    {
                        findings.Add($"{Path.GetRelativePath(repositoryRoot, path)}:{index + 1}: {hex}");
                    }
                }
            }
        }

        findings.Should().BeEmpty(
            "configurable colors must resolve through AppColor/AvaloniaThemeResources; "
                + "the shared style file is the documented designer/pre-startup fallback");
    }

    private static ThemeRepository CreateRepository(IThemePathProvider pathProvider)
        => new(
            new ThemePersistence(new ThemeLoader(new ThemeCssUrlResolver(pathProvider), new ThemeFileReader())),
            pathProvider);

    private static void AssertEveryMappedColorIsPublished(Application application, ThemeVariant themeVariant)
    {
        foreach (AppColor appColor in AvaloniaThemeResources.MappedAppColors)
        {
            System.Drawing.Color expected = AvaloniaThemeResources.ResolveAppColor(ThemeModule.Settings, appColor);
            string key = AvaloniaThemeResources.AppColorPrefix + appColor + "Brush";
            if (expected.IsEmpty)
            {
                application.TryGetResource(key, themeVariant, out _).Should().BeFalse();
            }
            else
            {
                GetResourceBrushColor(application, appColor).Should().Be(
                    Color.FromArgb(expected.A, expected.R, expected.G, expected.B));
            }
        }

        foreach (System.Drawing.KnownColor knownColor in AvaloniaThemeResources.MappedSystemColors)
        {
            string key = AvaloniaThemeResources.KnownColorPrefix + knownColor + "Brush";
            application.TryGetResource(key, themeVariant, out object? resource).Should().BeTrue();
            resource.Should().BeOfType<SolidColorBrush>();
        }
    }

    private static Color GetResourceBrushColor(Application application, AppColor appColor)
    {
        string key = AvaloniaThemeResources.AppColorPrefix + appColor + "Brush";
        application.TryGetResource(key, ThemeVariant.Light, out object? resource).Should().BeTrue();
        return resource.Should().BeOfType<SolidColorBrush>().Which.Color;
    }

    private static Color GetBrushColor(IBrush? brush)
        => brush.Should().BeAssignableTo<ISolidColorBrush>().Which.Color;

    private static bool IsBuildOutput(string path)
        => path.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj");

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "GitExtensions.P1.3", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string FindRepositoryRoot([CallerFilePath] string startPath = "")
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
