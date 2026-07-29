using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtensions.ParityCapture;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using WinFormsFont = GitExtensions.Shims.WinForms.Font;

namespace GitExtensionsTests;

public sealed partial class ParityScreenshotTests
{
    private const string CapturePlanEnvironmentVariable = "GITEXT_CAPTURE_PARITY_PLAN";
    private const string P02Category = "P0_2";

    [Test]
    [Category(P02Category)]
    public void Capture_plan_should_define_the_shared_acceptance_matrix()
    {
        CapturePlan plan = CapturePlan.Load(GetCapturePlanPath());

        plan.Scales.Should().Equal(100, 125, 150, 200);
        plan.Themes.Select(theme => theme.Id).Should().Equal("light", "dark", "parity-custom");
        CaptureComponentPlan component = plan.Components.Should().ContainSingle().Subject;
        component.TypeName.Should().Be("GitUI.ScriptsEngine.FormFilePrompt");
        component.TextValues.Should().ContainKey("txtFilePath")
            .WhoseValue.Should().Be(@"C:\source\first.txt C:\source\second.txt");
        component.States.Select(state => state.Kind).Should().Equal(
            CaptureStateKind.Normal,
            CaptureStateKind.Focus,
            CaptureStateKind.Hover,
            CaptureStateKind.Pressed,
            CaptureStateKind.Disabled);
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Avalonia_tree_reader_should_emit_canonical_shared_schema()
    {
        Window window = new() { Width = 320, Height = 120 };
        Grid content = new();
        TextBox txtValue = new() { Name = "txtValue", Text = "Parity" };
        content.Children.Add(txtValue);
        window.Content = content;
        window.Show();
        window.SetRenderScaling(1.25);
        Dispatcher.UIThread.RunJobs();

        WriteableBitmap frame = window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The headless window did not render.");
        AvaloniaControlTreeReader reader = new(window, 1.25);
        CaptureDocument document = CreateDocument(
            window,
            reader.ReadPrimary(window, frame.PixelSize),
            frame.PixelSize,
            new CaptureThemePlan { Id = "light", Kind = "builtin", File = "invariant.css", IsBuiltin = true },
            sourceSha256: new string('A', 64),
            scalePercent: 125,
            state: "normal");

        string json = CaptureJson.Serialize(document);
        CaptureDocument roundTripped = CaptureJson.Deserialize(json);

        CaptureJson.Serialize(roundTripped).Should().Be(json);
        roundTripped.Capture.DpiMode.Should().Be(CaptureDpiMode.HeadlessRenderScale);
        roundTripped.Image.CaptureMethod.Should().Be(CaptureMethod.HeadlessSkia);
        Flatten(roundTripped.Surfaces[0].Root)
            .Should().Contain(node => node.FieldName == "txtValue" && node.Text == "Parity");
        window.Close();
    }

    [AvaloniaTest]
    [Category(P02Category)]
    public void Avalonia_state_driver_should_apply_supported_states_and_reject_wrong_targets()
    {
        foreach (CaptureStateKind kind in new[]
                 {
                     CaptureStateKind.Focus,
                     CaptureStateKind.Disabled,
                     CaptureStateKind.Hover,
                     CaptureStateKind.Pressed
                 })
        {
            Window window = new() { Width = 240, Height = 100 };
            Button target = new() { Name = "btnTarget", Content = "Target" };
            window.Content = target;
            window.Show();
            Dispatcher.UIThread.RunJobs();

            CaptureStatePlan state = new() { Id = kind.ToString(), Kind = kind, TargetField = "btnTarget" };
            using (AvaloniaControlStateDriver.Apply(window, state))
            {
                switch (kind)
                {
                    case CaptureStateKind.Focus:
                        target.IsFocused.Should().BeTrue();
                        break;
                    case CaptureStateKind.Disabled:
                        target.IsEnabled.Should().BeFalse();
                        break;
                    case CaptureStateKind.Hover:
                        target.IsPointerOver.Should().BeTrue();
                        break;
                    case CaptureStateKind.Pressed:
                        target.IsPressed.Should().BeTrue();
                        break;
                }

                window.CaptureRenderedFrame().Should().NotBeNull();
            }

            window.Close();
        }

        Window menuWindow = new() { Width = 240, Height = 100 };
        MenuItem parentMenu = new()
        {
            Name = "mnuParent",
            Header = "Parent",
            ItemsSource = new[] { new MenuItem { Header = "Child" } }
        };
        menuWindow.Content = new Menu { ItemsSource = new[] { parentMenu } };
        menuWindow.Show();
        Dispatcher.UIThread.RunJobs();
        CaptureStatePlan menuState = new()
        {
            Id = "menu.open",
            Kind = CaptureStateKind.MenuOpen,
            TargetField = "mnuParent"
        };
        using (AvaloniaControlStateDriver menuDriver = AvaloniaControlStateDriver.Apply(menuWindow, menuState))
        {
            parentMenu.IsSubMenuOpen.Should().BeTrue();
            menuDriver.RequiresExternalSurfaceCapture.Should().BeTrue(
                "a separate popup top level must never be mislabeled as a primary-frame capture");
        }

        menuWindow.Close();

        Window unsupportedWindow = new() { Width = 240, Height = 100 };
        unsupportedWindow.Content = new Button { Name = "btnTarget", Content = "Target" };
        unsupportedWindow.Show();
        CaptureStatePlan unsupportedState = new()
        {
            Id = "checked",
            Kind = CaptureStateKind.Checked,
            TargetField = "btnTarget"
        };

        Action applyUnsupported = () => AvaloniaControlStateDriver.Apply(unsupportedWindow, unsupportedState);

        applyUnsupported.Should().Throw<AvaloniaCaptureStateUnsupportedException>()
            .WithMessage("*ToggleButton*");
        unsupportedWindow.Close();
    }

    private static async Task CaptureParityPlanAsync()
    {
        await Task.Run(RunCaptureWithIsolatedSettings);
    }

    private static void RunCaptureWithIsolatedSettings()
    {
        string settingsDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.ParitySettings-{Guid.NewGuid():N}");
        Directory.CreateDirectory(settingsDirectory);
        AppSettings.TestAccessor accessor = AppSettings.GetTestAccessor();
        Lazy<string?> originalApplicationDataPath = accessor.ApplicationDataPath;
        try
        {
            accessor.ApplicationDataPath = new Lazy<string?>(() => settingsDirectory);
            string settingsPath = AppSettings.SettingsFilePath;
            IsPathContained(settingsPath, settingsDirectory).Should().BeTrue(
                "the capture process must use a temporary settings path");
            using GitExtSettingsCache settingsCache = GitExtSettingsCache.Create(settingsPath);
            DistributedSettings captureSettings = new(
                lowerPriority: null,
                settingsCache,
                SettingLevel.Unknown);
            AppSettings.UsingContainer(captureSettings, () =>
            {
                JoinableTaskContext joinableTaskContext = new();
                joinableTaskContext.Factory.Run(async () =>
                {
                    await Dispatcher.UIThread.InvokeAsync(CaptureParityPlanCoreAsync);
                });
            });
        }
        finally
        {
            accessor.ApplicationDataPath = originalApplicationDataPath;
            TestDirectory.Delete(settingsDirectory);
        }
    }

    private static async Task CaptureParityPlanCoreAsync()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        GitExtensions.Shims.WinForms.ShimHost.MessageBoxHost = new StubMessageBoxHost();

        CapturePlan plan = CapturePlan.Load(GetCapturePlanPath());
        string? viewFilter = Environment.GetEnvironmentVariable(CaptureViewEnvironmentVariable);
        IReadOnlyList<CaptureComponentPlan> components = string.IsNullOrWhiteSpace(viewFilter)
            ? plan.Components
            : plan.Components
                .Where(component => component.TypeName.Contains(viewFilter, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        components.Should().NotBeEmpty($"{CaptureViewEnvironmentVariable} should match at least one capture-plan component");

        Dictionary<string, ViewDescriptor> descriptors = GetViewDescriptors()
            .ToDictionary(descriptor => descriptor.ClassName, StringComparer.Ordinal);
        foreach (CaptureComponentPlan component in components)
        {
            descriptors.Should().ContainKey(component.TypeName);
        }

        string outputDirectory = Path.Combine(GetOutputDirectory(), "avalonia");
        if (Directory.Exists(outputDirectory))
        {
            TestDirectory.Delete(outputDirectory);
        }

        Directory.CreateDirectory(outputDirectory);
        List<CaptureManifestEntry> entries = [];
        using CaptureContext context = new();
        string repositoryRoot = FindRepositoryRoot(Path.Combine(AppContext.BaseDirectory, "GitUI.Avalonia.Tests.dll"));
        IsPathContained(context.WorkingDirectory, repositoryRoot).Should().BeFalse(
            "capture repositories must be throwaway directories outside the working tree");
        CaptureSettingsProfile profile = LoadCaptureSettingsProfile(plan);
        CaptureSettingsSnapshot settingsSnapshot = CaptureSettingsSnapshot.Take(profile);
        SettingsFileSnapshot settingsFileSnapshot = SettingsFileSnapshot.Take(AppSettings.SettingsFilePath);
        try
        {
            ApplyCaptureProfile(profile);
            foreach (CaptureThemePlan theme in plan.Themes)
            {
                ApplyCaptureTheme(theme);
                foreach (int scalePercent in plan.Scales)
                {
                    foreach (CaptureComponentPlan component in components)
                    {
                        ViewDescriptor descriptor = descriptors[component.TypeName];
                        foreach (CaptureStatePlan state in component.States)
                        {
                            entries.Add(await CapturePlannedStateAsync(
                                context,
                                descriptor,
                                component,
                                theme,
                                scalePercent,
                                state,
                                outputDirectory));
                        }
                    }
                }
            }
        }
        finally
        {
            settingsSnapshot.Restore();
            settingsFileSnapshot.AssertUnchanged(AppSettings.SettingsFilePath);
        }

        CaptureSetManifest manifest = new()
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            CreatedAtUtc = DateTime.UtcNow,
            ToolVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0",
            Repository = context.WorkingDirectory,
            Captures = entries
        };
        string manifestPath = Path.Combine(outputDirectory, "manifest.json");
        File.WriteAllText(manifestPath, SerializeManifest(manifest));

        int expectedCount = components.Sum(component => component.States.Count) * plan.Themes.Count * plan.Scales.Count;
        entries.Should().HaveCount(expectedCount);
        entries.Should().NotContain(entry => entry.Status == CaptureStateStatus.Failed);
        entries.Where(entry => entry.Status == CaptureStateStatus.Captured).Should().OnlyContain(
            entry => File.Exists(Path.Combine(outputDirectory, entry.ImageFile!))
                     && File.Exists(Path.Combine(outputDirectory, entry.TreeFile!)));
        await TestContext.Progress.WriteLineAsync(
            $"Captured {entries.Count(entry => entry.Status == CaptureStateStatus.Captured)} Avalonia states "
            + $"and recorded {entries.Count(entry => entry.Status == CaptureStateStatus.Unsupported)} unsupported states in {outputDirectory}");
    }

    private static async Task<CaptureManifestEntry> CapturePlannedStateAsync(
        CaptureContext context,
        ViewDescriptor descriptor,
        CaptureComponentPlan component,
        CaptureThemePlan theme,
        int scalePercent,
        CaptureStatePlan state,
        string outputRoot)
    {
        Control view = CreateView(context, descriptor.ViewType);
        ApplyTextValues(view, component);
        (double width, double height) = GetCaptureSize(descriptor.ViewType);
        bool isWindow = view is Window;
        Window window = view as Window
            ?? new Window
            {
                Title = descriptor.ClassName,
                Content = view,
            };
        window.Width = width;
        window.Height = height;
        window.SizeToContent = SizeToContent.Manual;
        window.RequestedThemeVariant = Application.Current?.RequestedThemeVariant;

        try
        {
            PrepareView(view, context);
            window.Show();
            double renderScale = scalePercent / 100d;
            window.SetRenderScaling(renderScale);
            if (!isWindow)
            {
                await SeedStandaloneControlAsync(view, context);
            }

            await WaitForAsyncViewsAsync(view);
            Dispatcher.UIThread.RunJobs();
            using AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(view, state);
            if (driver.RequiresExternalSurfaceCapture)
            {
                throw new AvaloniaCaptureStateUnsupportedException(
                    "The state opened a separate popup top level that headless Skia cannot compose into the primary frame.");
            }

            WriteableBitmap frame = window.CaptureRenderedFrame()
                ?? throw new AvaloniaCaptureStateUnsupportedException("Headless Skia did not render a frame.");
            EnsureRenderedContent(frame);

            string relativeDirectory = Path.Combine(
                Sanitize(component.TypeName),
                Sanitize(theme.Id),
                scalePercent.ToString(System.Globalization.CultureInfo.InvariantCulture));
            string absoluteDirectory = Path.Combine(outputRoot, relativeDirectory);
            Directory.CreateDirectory(absoluteDirectory);
            string imagePath = Path.Combine(absoluteDirectory, $"{Sanitize(state.Id)}.png");
            string treePath = Path.Combine(absoluteDirectory, $"{Sanitize(state.Id)}.tree.json");
            using (FileStream stream = File.Create(imagePath))
            {
                frame.Save(stream, PngBitmapEncoderOptions.Default);
            }

            AvaloniaControlTreeReader reader = new(view, renderScale);
            CaptureDocument document = CreateDocument(
                view,
                reader.ReadPrimary(view, frame.PixelSize),
                frame.PixelSize,
                theme,
                GetThemeSourceSha256(theme),
                scalePercent,
                state.Id);
            string treeJson = CaptureJson.Serialize(document);
            CaptureJson.Serialize(CaptureJson.Deserialize(treeJson)).Should().Be(treeJson);
            File.WriteAllText(treePath, treeJson);

            return new CaptureManifestEntry
            {
                ComponentType = component.TypeName,
                ThemeId = theme.Id,
                ScalePercent = scalePercent,
                State = state.Id,
                Status = CaptureStateStatus.Captured,
                Note = null,
                DpiMode = CaptureDpiMode.HeadlessRenderScale,
                CaptureMethod = CaptureMethod.HeadlessSkia,
                ImageFile = Path.GetRelativePath(outputRoot, imagePath).Replace('\\', '/'),
                TreeFile = Path.GetRelativePath(outputRoot, treePath).Replace('\\', '/')
            };
        }
        catch (AvaloniaCaptureStateUnsupportedException exception)
        {
            return Unsupported(component.TypeName, theme.Id, scalePercent, state.Id, exception.Message);
        }
        catch (Exception exception)
        {
            return new CaptureManifestEntry
            {
                ComponentType = component.TypeName,
                ThemeId = theme.Id,
                ScalePercent = scalePercent,
                State = state.Id,
                Status = CaptureStateStatus.Failed,
                Note = exception.ToString(),
                DpiMode = CaptureDpiMode.HeadlessRenderScale,
                CaptureMethod = CaptureMethod.Unsupported,
                ImageFile = null,
                TreeFile = null
            };
        }
        finally
        {
            window.Close();
            if (!ReferenceEquals(window, view) && view is IDisposable disposableView)
            {
                disposableView.Dispose();
            }
        }
    }

    private static CaptureDocument CreateDocument(
        Control root,
        CaptureSurface surface,
        PixelSize imageSize,
        CaptureThemePlan theme,
        string sourceSha256,
        int scalePercent,
        string state)
    {
        int dpi = checked(scalePercent * 96 / 100);
        return new CaptureDocument
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            Component = new CaptureComponent
            {
                TypeName = root.GetType().FullName ?? root.GetType().Name,
                AssemblyName = root.GetType().Assembly.GetName().Name ?? "GitUI.Avalonia"
            },
            Capture = new CaptureMetadata
            {
                Framework = "avalonia",
                Theme = new CaptureTheme
                {
                    Id = theme.Id,
                    Kind = theme.Kind,
                    SourceSha256 = sourceSha256
                },
                ScalePercent = scalePercent,
                Dpi = new CaptureDpi { X = dpi, Y = dpi },
                DpiMode = CaptureDpiMode.HeadlessRenderScale,
                State = state,
                StateStatus = CaptureStateStatus.Captured,
                StateNote = null
            },
            Image = new CaptureImage
            {
                WidthPx = imageSize.Width,
                HeightPx = imageSize.Height,
                CaptureMethod = CaptureMethod.HeadlessSkia
            },
            Surfaces = [surface]
        };
    }

    private static void ApplyTextValues(Control root, CaptureComponentPlan component)
    {
        foreach ((string fieldName, string text) in component.TextValues)
        {
            Control? target = root.FindControl<Control>(fieldName);
            if (target is null)
            {
                throw new InvalidDataException($"Text seed field '{fieldName}' was not found on {component.TypeName}.");
            }

            switch (target)
            {
                case TextBox textBox:
                    textBox.Text = text;
                    break;
                case ContentControl contentControl:
                    contentControl.Content = text;
                    break;
                default:
                    throw new InvalidDataException($"Text seed field '{fieldName}' does not expose a supported text boundary.");
            }
        }
    }

    private static IEnumerable<CaptureNode> Flatten(CaptureNode root)
    {
        yield return root;
        foreach (CaptureNode child in root.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    private static CaptureManifestEntry Unsupported(
        string componentType,
        string themeId,
        int scalePercent,
        string state,
        string note) =>
        new()
        {
            ComponentType = componentType,
            ThemeId = themeId,
            ScalePercent = scalePercent,
            State = state,
            Status = CaptureStateStatus.Unsupported,
            Note = note,
            DpiMode = CaptureDpiMode.HeadlessRenderScale,
            CaptureMethod = CaptureMethod.Unsupported,
            ImageFile = null,
            TreeFile = null
        };

    private static string SerializeManifest(CaptureSetManifest manifest)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return JsonSerializer.Serialize(manifest, options) + Environment.NewLine;
    }

    private static void EnsureRenderedContent(WriteableBitmap bitmap)
    {
        using ILockedFramebuffer framebuffer = bitmap.Lock();
        int bytesPerPixel = framebuffer.Format.BitsPerPixel / 8;
        int firstPixel = Marshal.ReadInt32(framebuffer.Address);
        int stepX = Math.Max(1, framebuffer.Size.Width / 64);
        int stepY = Math.Max(1, framebuffer.Size.Height / 64);
        for (int y = 0; y < framebuffer.Size.Height; y += stepY)
        {
            for (int x = 0; x < framebuffer.Size.Width; x += stepX)
            {
                IntPtr address = IntPtr.Add(framebuffer.Address, (y * framebuffer.RowBytes) + (x * bytesPerPixel));
                if (Marshal.ReadInt32(address) != firstPixel)
                {
                    return;
                }
            }
        }

        throw new AvaloniaCaptureStateUnsupportedException("Headless Skia returned a blank image.");
    }

    private static string GetCapturePlanPath()
    {
        string? configuredPath = Environment.GetEnvironmentVariable(CapturePlanEnvironmentVariable);
        return string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(
                FindRepositoryRoot(Path.Combine(AppContext.BaseDirectory, "GitUI.Avalonia.Tests.dll")),
                "eng",
                "avalonia",
                "p0.2-capture-plan.json")
            : Path.GetFullPath(configuredPath);
    }

    private static CaptureSettingsProfile LoadCaptureSettingsProfile(CapturePlan plan)
    {
        string path = Path.Combine(AppContext.BaseDirectory, plan.SettingsProfile);
        CaptureSettingsProfile? profile = JsonSerializer.Deserialize<CaptureSettingsProfile>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return profile ?? throw new InvalidDataException("The capture settings profile is empty.");
    }

    private static void ApplyCaptureProfile(CaptureSettingsProfile profile)
    {
        AppSettings.Font = new WinFormsFont(profile.UiFontFamily, profile.UiFontSizePoints);
        AppSettings.FixedWidthFont = new WinFormsFont(profile.FixedFontFamily, profile.FixedFontSizePoints);
        AppSettings.CommitFont = new WinFormsFont(profile.UiFontFamily, profile.UiFontSizePoints);
        AppSettings.MonospaceFont = new WinFormsFont(profile.FixedFontFamily, profile.FixedFontSizePoints);
        AppSettings.Dictionary = "en-US";
        AppSettings.MarkIllFormedLinesInCommitMsg = true;
        AppSettings.TelemetryEnabled = false;
        AppSettings.CheckForUpdates = false;
        AppSettings.ShowAvailableDiffTools = false;
        foreach ((string key, string value) in profile.AppSettings)
        {
            AppSettings.SetString(key, value);
        }

        AvaloniaFontSettings.ApplyAppSettings();
    }

    private static void ApplyCaptureTheme(CaptureThemePlan theme)
    {
        AppSettings.ThemeId = new ThemeId(theme.Id, theme.IsBuiltin);
        AppSettings.ThemeVariations = [];
        AppSettings.UseSystemVisualStyle = theme.Id.Equals("light", StringComparison.OrdinalIgnoreCase);
        AvaloniaThemeSettings.ApplyAppSettings();
    }

    private static string GetThemeSourceSha256(CaptureThemePlan theme)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Themes", theme.File);
        IsPathContained(path, AppContext.BaseDirectory).Should().BeTrue(
            "capture themes must come from the isolated test runtime, not the user themes directory");
        return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    }

    private static bool IsPathContained(string path, string root)
    {
        string fullPath = Path.GetFullPath(path);
        string fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return fullPath.StartsWith(fullRoot, comparison);
    }

    private static string Sanitize(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(character => invalid.Contains(character) || character is '.' ? '_' : character).ToArray());
    }

    private sealed record CaptureSettingsProfile
    {
        public required string UiFontFamily { get; init; }

        public required float UiFontSizePoints { get; init; }

        public required string FixedFontFamily { get; init; }

        public required float FixedFontSizePoints { get; init; }

        public required IReadOnlyDictionary<string, string> AppSettings { get; init; }
    }

    private sealed record CaptureSettingsSnapshot(
        WinFormsFont Font,
        WinFormsFont FixedWidthFont,
        WinFormsFont CommitFont,
        WinFormsFont MonospaceFont,
        ThemeId ThemeId,
        string[] ThemeVariations,
        bool UseSystemVisualStyle,
        string Dictionary,
        bool MarkIllFormedLines,
        bool? TelemetryEnabled,
        bool CheckForUpdates,
        bool ShowAvailableDiffTools,
        IReadOnlyDictionary<string, string?> ProfileSettings)
    {
        public static CaptureSettingsSnapshot Take(CaptureSettingsProfile profile)
        {
            return new CaptureSettingsSnapshot(
                AppSettings.Font,
                AppSettings.FixedWidthFont,
                AppSettings.CommitFont,
                AppSettings.MonospaceFont,
                AppSettings.ThemeId,
                AppSettings.ThemeVariations,
                AppSettings.UseSystemVisualStyle,
                AppSettings.Dictionary,
                AppSettings.MarkIllFormedLinesInCommitMsg,
                AppSettings.TelemetryEnabled,
                AppSettings.CheckForUpdates,
                AppSettings.ShowAvailableDiffTools,
                profile.AppSettings.Keys.ToDictionary(
                    key => key,
                    key => AppSettings.GetString(key, defaultValue: null),
                    StringComparer.Ordinal));
        }

        public void Restore()
        {
            AppSettings.Font = Font;
            AppSettings.FixedWidthFont = FixedWidthFont;
            AppSettings.CommitFont = CommitFont;
            AppSettings.MonospaceFont = MonospaceFont;
            AppSettings.ThemeId = ThemeId;
            AppSettings.ThemeVariations = ThemeVariations;
            AppSettings.UseSystemVisualStyle = UseSystemVisualStyle;
            AppSettings.Dictionary = Dictionary;
            AppSettings.MarkIllFormedLinesInCommitMsg = MarkIllFormedLines;
            AppSettings.TelemetryEnabled = TelemetryEnabled;
            AppSettings.CheckForUpdates = CheckForUpdates;
            AppSettings.ShowAvailableDiffTools = ShowAvailableDiffTools;
            foreach ((string key, string? value) in ProfileSettings)
            {
                AppSettings.SetString(key, value ?? string.Empty);
            }

            AvaloniaFontSettings.ApplyAppSettings();
            AvaloniaThemeSettings.ApplyAppSettings();
        }
    }

    private sealed record SettingsFileSnapshot(bool Exists, byte[] Content)
    {
        public static SettingsFileSnapshot Take(string path) =>
            File.Exists(path)
                ? new SettingsFileSnapshot(Exists: true, File.ReadAllBytes(path))
                : new SettingsFileSnapshot(Exists: false, []);

        public void AssertUnchanged(string path)
        {
            File.Exists(path).Should().Be(Exists, "capture must not create or remove the real settings file");
            if (Exists)
            {
                File.ReadAllBytes(path).Should().Equal(Content, "capture must not modify the real settings file");
            }
        }
    }
}
