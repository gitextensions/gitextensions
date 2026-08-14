using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI;
using ResourceManager;

namespace WinFormsParityCapture;

internal static class CaptureRunner
{
    private static readonly JsonSerializerOptions ManifestJsonOptions = CreateJsonOptions();

    public static async Task<int> CaptureAsync(CaptureOptions options)
    {
        EnsureWindows();
        string planPath = RequireExistingFile(options.PlanPath, "--plan");
        string repositoryPath = RequireExistingDirectory(options.RepositoryPath, "--repository");
        string outputPath = RequireValue(options.OutputPath, "--output");
        EnsureRepositoryIsOutsideWorkingTree(repositoryPath);

        CapturePlan plan = CapturePlan.Load(planPath);
        IReadOnlyList<CaptureComponentPlan> components = SelectComponents(plan, options.Components);
        IReadOnlyList<CaptureThemePlan> themes = SelectThemes(plan, options.Themes);
        IReadOnlyList<int> scales = SelectScales(plan, options.Scales);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        IReadOnlyList<CaptureMonitor> monitors = NativeMethods.GetMonitors();

        Directory.CreateDirectory(outputPath);
        string isolationRoot = Path.Combine(Path.GetTempPath(), "GitExtensions.WinFormsParityCapture", Guid.NewGuid().ToString("N"));
        string runtimeRoot = Path.Combine(isolationRoot, "runtime");
        Directory.CreateDirectory(runtimeRoot);

        List<CaptureManifestEntry> entries = [];
        try
        {
            CopyRuntime(AppContext.BaseDirectory, runtimeRoot);
            string isolatedPlanPath = Path.Combine(runtimeRoot, Path.GetFileName(planPath));
            if (!File.Exists(isolatedPlanPath))
            {
                File.Copy(planPath, isolatedPlanPath);
            }

            foreach (CaptureComponentPlan component in components)
            {
                foreach (CaptureThemePlan theme in themes)
                {
                    foreach (int scale in scales)
                    {
                        int targetDpi = checked(scale * 96 / 100);
                        CaptureMonitor? nativeMonitor = monitors.FirstOrDefault(monitor => monitor.DpiX == targetDpi && monitor.DpiY == targetDpi);
                        CaptureDpiMode? dpiMode;
                        CaptureMonitor? hostMonitor;
                        if (nativeMonitor is { DpiX: > 0 })
                        {
                            dpiMode = CaptureDpiMode.NativeMonitor;
                            hostMonitor = nativeMonitor;
                        }
                        else if (scale == 100)
                        {
                            dpiMode = null;
                            hostMonitor = null;
                        }
                        else
                        {
                            dpiMode = CaptureDpiMode.DpiChangeMessage;
                            hostMonitor = monitors.FirstOrDefault(monitor => monitor.DpiX == 96) is { DpiX: > 0 } baseline
                                ? baseline
                                : monitors.FirstOrDefault();
                        }

                        if (dpiMode is null || hostMonitor is null || hostMonitor.Value.DpiX == 0)
                        {
                            entries.AddRange(component.States.Select(state => Unsupported(
                                component.TypeName,
                                theme.Id,
                                scale,
                                state.Id,
                                "A native 96-DPI monitor is required for the 100% baseline.")));
                            continue;
                        }

                        foreach (CaptureStatePlan state in component.States)
                        {
                            string stateWorkerResultPath = Path.Combine(
                                isolationRoot,
                                $"worker-{Sanitize(component.TypeName)}-{Sanitize(theme.Id)}-{scale}-{Sanitize(state.Id)}.json");
                            List<string> workerArguments =
                            [
                                "--worker",
                                "--plan", isolatedPlanPath,
                                "--repository", repositoryPath,
                                "--output", Path.GetFullPath(outputPath),
                                "--component", component.TypeName,
                                "--theme", theme.Id,
                                "--scale", scale.ToString(CultureInfo.InvariantCulture),
                                "--state", state.Id,
                                "--monitor", hostMonitor.Value.ToString(),
                                "--dpi-mode", dpiMode.Value.ToString(),
                                "--worker-result", stateWorkerResultPath
                            ];

                            int exitCode = await RunWorkerAsync(runtimeRoot, workerArguments);
                            if (!File.Exists(stateWorkerResultPath))
                            {
                                throw new InvalidOperationException(
                                    $"Capture worker failed for {component.TypeName}, {theme.Id}, {scale}%, {state.Id} with exit code {exitCode}.");
                            }

                            CaptureWorkerResult? workerResult = JsonSerializer.Deserialize<CaptureWorkerResult>(
                                File.ReadAllText(stateWorkerResultPath),
                                ManifestJsonOptions);
                            entries.AddRange(workerResult?.Captures
                                ?? throw new InvalidDataException($"Worker result '{stateWorkerResultPath}' is empty."));
                        }
                    }
                }
            }

            CaptureSetManifest manifest = new()
            {
                SchemaVersion = 1,
                CreatedAtUtc = DateTime.UtcNow,
                ToolVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0",
                Repository = Path.GetFullPath(repositoryPath),
                Captures = entries
            };
            string manifestPath = Path.Combine(outputPath, "manifest.json");
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, ManifestJsonOptions) + Environment.NewLine);
            Console.WriteLine($"Capture manifest written to {manifestPath}");
            return entries.Any(entry => entry.Status == CaptureStateStatus.Failed) ? 1 : 0;
        }
        finally
        {
            DeleteIsolationRoot(isolationRoot);
        }
    }

    public static int CaptureWorker(CaptureOptions options)
    {
        EnsureWindows();
        string planPath = RequireExistingFile(options.PlanPath, "--plan");
        string repositoryPath = RequireExistingDirectory(options.RepositoryPath, "--repository");
        string outputPath = RequireValue(options.OutputPath, "--output");
        string componentType = RequireValue(options.ComponentType, "--component");
        string themeId = RequireValue(options.ThemeId, "--theme");
        int scale = options.ScalePercent ?? throw new ArgumentException("--scale is required.");
        CaptureMonitor monitor = options.Monitor ?? throw new ArgumentException("--monitor is required.");
        CaptureDpiMode dpiMode = Enum.Parse<CaptureDpiMode>(RequireValue(options.DpiMode, "--dpi-mode"));
        string workerResultPath = RequireValue(options.WorkerResultPath, "--worker-result");
        string stateId = RequireValue(options.StateId, "--state");

        CapturePlan plan = CapturePlan.Load(planPath);
        CaptureComponentPlan component = plan.Components.Single(item => item.TypeName == componentType);
        CaptureThemePlan theme = plan.Themes.Single(item => item.Id.Equals(themeId, StringComparison.OrdinalIgnoreCase));
        string profilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(planPath)!, plan.SettingsProfile));
        CaptureSettingsProfile profile = CaptureSettingsProfile.Load(profilePath);
        string isolationRoot = Path.GetFullPath(AppContext.BaseDirectory);
        Directory.CreateDirectory(outputPath);

        List<CaptureManifestEntry> entries = [];
        CaptureStatePlan state = component.States.Single(item => item.Id == stateId);
        try
        {
            using WinFormsBootstrap bootstrap = WinFormsBootstrap.Create(repositoryPath, profile, theme, isolationRoot);
            using Control root = ComponentFactory.Create(component, bootstrap.Commands);
            PrepareControl(root, bootstrap.Commands, component, monitor, scale, dpiMode);
            PumpUntilReady(root);
            int actualDpi = root.DeviceDpi;
            entries.Add(actualDpi != scale * 96 / 100
                ? Unsupported(componentType, theme.Id, scale, state.Id, $"The WinForms DPI-change path reported {actualDpi} DPI instead of {scale * 96 / 100} DPI.")
                : CaptureState(bootstrap, root, componentType, theme, scale, dpiMode, state, outputPath));
            Application.DoEvents();
            bootstrap.ThrowIfThreadException();
            ComponentFactory.CleanupBeforeDispose(root);
        }
        catch (CaptureStateUnsupportedException exception)
        {
            entries.Add(Unsupported(componentType, theme.Id, scale, state.Id, exception.Message));
        }
        catch (Exception exception)
        {
            entries.Add(Failed(componentType, theme.Id, scale, state.Id, dpiMode, exception));
        }

        CaptureWorkerResult result = new() { Captures = entries };
        File.WriteAllText(workerResultPath, JsonSerializer.Serialize(result, ManifestJsonOptions) + Environment.NewLine);
        return entries.Any(entry => entry.Status == CaptureStateStatus.Failed) ? 1 : 0;
    }

    public static int Validate(CaptureOptions options)
    {
        string manifestPath = RequireExistingFile(options.ManifestPath, "--manifest");
        string manifestDirectory = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        CaptureSetManifest? manifest = JsonSerializer.Deserialize<CaptureSetManifest>(
            File.ReadAllText(manifestPath),
            ManifestJsonOptions);
        if (manifest is null || manifest.SchemaVersion != 1)
        {
            throw new InvalidDataException("The capture manifest is empty or has an unsupported schema version.");
        }

        int capturedCount = 0;
        int unsupportedCount = 0;
        HashSet<string> captureKeys = new(StringComparer.Ordinal);
        foreach (CaptureManifestEntry entry in manifest.Captures)
        {
            string captureKey = $"{entry.ComponentType}\n{entry.ThemeId}\n{entry.ScalePercent}\n{entry.State}";
            if (!captureKeys.Add(captureKey))
            {
                throw new InvalidDataException($"Manifest contains a duplicate capture for {entry.ComponentType}/{entry.ThemeId}/{entry.ScalePercent}/{entry.State}.");
            }

            if (entry.Status == CaptureStateStatus.Failed)
            {
                throw new InvalidDataException($"Capture failed for {entry.ComponentType}/{entry.State}: {entry.Note}");
            }

            if (entry.Status != CaptureStateStatus.Captured)
            {
                unsupportedCount++;
                if (entry.CaptureMethod != CaptureMethod.Unsupported || string.IsNullOrWhiteSpace(entry.Note))
                {
                    throw new InvalidDataException("Unsupported states must name the reason and use captureMethod=unsupported.");
                }

                if (entry.DpiMode is not null || entry.ImageFile is not null || entry.TreeFile is not null)
                {
                    throw new InvalidDataException("Unsupported states must not claim DPI acquisition or artifact paths.");
                }

                continue;
            }

            if (entry.DpiMode is null || entry.CaptureMethod == CaptureMethod.Unsupported)
            {
                throw new InvalidDataException("Captured states must name both dpiMode and captureMethod.");
            }

            string imagePath = ResolveArtifactPath(
                manifestDirectory,
                entry.ImageFile ?? throw new InvalidDataException("A captured state has no image."));
            string treePath = ResolveArtifactPath(
                manifestDirectory,
                entry.TreeFile ?? throw new InvalidDataException("A captured state has no tree."));
            if (!File.Exists(imagePath) || !File.Exists(treePath))
            {
                throw new FileNotFoundException($"Capture artifacts are missing for {entry.ComponentType}/{entry.State}.");
            }

            string json = File.ReadAllText(treePath);
            CaptureDocument document = CaptureJson.Deserialize(json);
            if (document.Component.TypeName != entry.ComponentType
                || document.Capture.Theme.Id != entry.ThemeId
                || document.Capture.ScalePercent != entry.ScalePercent
                || document.Capture.State != entry.State
                || document.Capture.DpiMode != entry.DpiMode
                || document.Image.CaptureMethod != entry.CaptureMethod)
            {
                throw new InvalidDataException($"Tree metadata does not match its manifest entry for {entry.ComponentType}/{entry.State}.");
            }

            using Image image = Image.FromFile(imagePath);
            if (image.Width != document.Image.WidthPx || image.Height != document.Image.HeightPx)
            {
                throw new InvalidDataException($"Image dimensions do not match the tree for {entry.ComponentType}/{entry.State}.");
            }

            if (options.RoundTrip)
            {
                string canonical = CaptureJson.Serialize(document);
                if (!string.Equals(json, canonical, StringComparison.Ordinal))
                {
                    throw new InvalidDataException($"Tree '{treePath}' does not round-trip byte-identically.");
                }
            }

            capturedCount++;
        }

        Console.WriteLine($"Validated {capturedCount} captured states; {unsupportedCount} states explicitly unsupported.");
        return 0;
    }

    private static string ResolveArtifactPath(string manifestDirectory, string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidDataException($"Manifest artifact path '{relativePath}' must be relative.");
        }

        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(manifestDirectory)) + Path.DirectorySeparatorChar;
        string path = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Manifest artifact path '{relativePath}' escapes the capture directory.");
        }

        return path;
    }

    private static CaptureManifestEntry CaptureState(
        WinFormsBootstrap bootstrap,
        Control root,
        string componentType,
        CaptureThemePlan theme,
        int scale,
        CaptureDpiMode dpiMode,
        CaptureStatePlan state,
        string outputRoot)
    {
        try
        {
            bootstrap.ThrowIfThreadException();
            using ControlStateDriver driver = ApplyVerifiedCaptureState(root, bootstrap.Commands, state);
            bootstrap.ThrowIfThreadException();
            using CaptureImageResult image = ImageCapture.Capture(root, driver.Popups);
            string relativeDirectory = Path.Combine(Sanitize(componentType), Sanitize(theme.Id), scale.ToString(CultureInfo.InvariantCulture));
            string absoluteDirectory = Path.Combine(outputRoot, relativeDirectory);
            Directory.CreateDirectory(absoluteDirectory);
            string imageFileName = $"{Sanitize(state.Id)}.png";
            string treeFileName = $"{Sanitize(state.Id)}.tree.json";
            string imagePath = Path.Combine(absoluteDirectory, imageFileName);
            string treePath = Path.Combine(absoluteDirectory, treeFileName);
            image.Bitmap.Save(imagePath);

            int dpi = root.DeviceDpi;
            ControlTreeReader reader = new(root, dpi);
            List<CaptureSurface> surfaces =
            [
                reader.ReadPrimary(root, NativeMethods.GetWindowRectangle(root.FindForm()?.Handle ?? root.Handle))
            ];
            surfaces.AddRange(driver.Popups.Select((popup, index) => reader.ReadPopup(popup, index)));

            string themePath = Path.Combine(AppContext.BaseDirectory, "Themes", theme.File);
            string relativeImagePath = Path.GetRelativePath(outputRoot, imagePath).Replace('\\', '/');
            CaptureDocument document = new()
            {
                SchemaVersion = CaptureDocument.CurrentSchemaVersion,
                Component = new CaptureComponent
                {
                    TypeName = componentType,
                    AssemblyName = root.GetType().Assembly.GetName().Name ?? "GitUI"
                },
                Capture = new CaptureMetadata
                {
                    Framework = "winforms",
                    Theme = new CaptureTheme
                    {
                        Id = theme.Id,
                        Kind = theme.Kind,
                        SourceSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(themePath)))
                    },
                    ScalePercent = scale,
                    Dpi = new CaptureDpi { X = dpi, Y = dpi },
                    DpiMode = dpiMode,
                    State = state.Id,
                    StateStatus = CaptureStateStatus.Captured,
                    StateNote = null
                },
                Image = new CaptureImage
                {
                    WidthPx = image.Bitmap.Width,
                    HeightPx = image.Bitmap.Height,
                    CaptureMethod = image.Method
                },
                Surfaces = surfaces
            };
            File.WriteAllText(treePath, CaptureJson.Serialize(document));
            bootstrap.ThrowIfThreadException();

            return new CaptureManifestEntry
            {
                ComponentType = componentType,
                ThemeId = theme.Id,
                ScalePercent = scale,
                State = state.Id,
                Status = CaptureStateStatus.Captured,
                Note = null,
                DpiMode = dpiMode,
                CaptureMethod = image.Method,
                ImageFile = relativeImagePath,
                TreeFile = Path.GetRelativePath(outputRoot, treePath).Replace('\\', '/')
            };
        }
        catch (CaptureStateUnsupportedException exception)
        {
            return Unsupported(componentType, theme.Id, scale, state.Id, exception.Message);
        }
        catch (Exception exception)
        {
            return new CaptureManifestEntry
            {
                ComponentType = componentType,
                ThemeId = theme.Id,
                ScalePercent = scale,
                State = state.Id,
                Status = CaptureStateStatus.Failed,
                Note = exception.ToString(),
                DpiMode = dpiMode,
                CaptureMethod = CaptureMethod.Unsupported,
                ImageFile = null,
                TreeFile = null
            };
        }
    }

    // parity-scaffolding: The original revision grid can publish a new selection between
    // preparation and popup opening; retry only that detected race, never an unsupported state.
    private static ControlStateDriver ApplyVerifiedCaptureState(
        Control root,
        IGitUICommands commands,
        CaptureStatePlan state)
    {
        const int maximumAttempts = 3;
        CaptureStateNotReadyException? lastException = null;
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            ComponentFactory.PrepareCaptureState(root, commands);
            ControlStateDriver? driver = null;
            try
            {
                driver = ControlStateDriver.Apply(root, state);
                ComponentFactory.VerifyCaptureState(root, commands, state);
                return driver;
            }
            catch (CaptureStateNotReadyException ex)
            {
                driver?.Dispose();
                lastException = ex;
            }
            catch
            {
                driver?.Dispose();
                throw;
            }
        }

        throw lastException!;
    }

    private static void CopyRuntime(string sourceRoot, string destinationRoot)
    {
        foreach (string directory in Directory.EnumerateDirectories(sourceRoot, "*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(sourceRoot, directory);
            Directory.CreateDirectory(Path.Combine(destinationRoot, relative));
        }

        foreach (string file in Directory.EnumerateFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            if (Path.GetFileName(file).Equals("GitExtensions.settings", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string relative = Path.GetRelativePath(sourceRoot, file);
            string destination = Path.Combine(destinationRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }

        // parity-scaffolding: AppSettings derives its portable settings location from a
        // recognized Git Extensions app host. The isolated alias still starts this tool's
        // apphost payload, while keeping every settings read and write inside the worker root.
        string captureHost = Path.Combine(sourceRoot, $"{Assembly.GetExecutingAssembly().GetName().Name}.exe");
        if (!File.Exists(captureHost))
        {
            throw new FileNotFoundException("The capture worker apphost is missing.", captureHost);
        }

        File.Copy(captureHost, Path.Combine(destinationRoot, "GitExtensions.exe"), overwrite: true);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }

    private static void DeleteIsolationRoot(string isolationRoot)
    {
        string fullRoot = Path.GetFullPath(isolationRoot);
        string expectedParent = Path.TrimEndingDirectorySeparator(Path.Combine(Path.GetTempPath(), "GitExtensions.WinFormsParityCapture"))
            + Path.DirectorySeparatorChar;
        if (!fullRoot.StartsWith(expectedParent, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Refusing to remove unexpected isolation path '{fullRoot}'.");
        }

        if (Directory.Exists(fullRoot))
        {
            Directory.Delete(fullRoot, recursive: true);
        }
    }

    private static void EnsureRepositoryIsOutsideWorkingTree(string repositoryPath)
    {
        string? workingTree = TryGetWorkingTree();
        if (workingTree is null)
        {
            return;
        }

        string repository = Path.TrimEndingDirectorySeparator(Path.GetFullPath(repositoryPath));
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(workingTree));
        if (repository.Equals(root, StringComparison.OrdinalIgnoreCase)
            || repository.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The capture repository must be a throwaway repository outside this working tree.");
        }
    }

    private static void EnsureWindows()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("WinFormsParityCapture runs only on Windows.");
        }
    }

    private static void PumpUntilReady(Control root)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(2);
        do
        {
            Application.DoEvents();
            Thread.Sleep(25);
        }
        while (DateTime.UtcNow < deadline);

        Application.DoEvents();

        // parity-scaffolding: Auto-sized descendants can invalidate their standalone host
        // after the first layout pass; settle both levels before measuring or rendering.
        root.FindForm()?.PerformLayout();
        root.PerformLayout();
        root.FindForm()?.PerformLayout();
        root.PerformLayout();
        Application.DoEvents();
    }

    private static void PrepareControl(
        Control root,
        IGitUICommands commands,
        CaptureComponentPlan component,
        CaptureMonitor monitor,
        int scale,
        CaptureDpiMode dpiMode)
    {
        int targetDpi = checked(scale * 96 / 100);
        if (root is Form form)
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(monitor.X + 16, monitor.Y + 16);
            form.ShowInTaskbar = false;
            form.Show();
        }
        else
        {
            // parity-scaffolding: GitModuleControl resolves its real command source through
            // the parent form, so standalone component captures need the same runtime shape.
            CaptureHostForm host = new(commands)
            {
                AutoScaleMode = AutoScaleMode.Dpi,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(monitor.X + 16, monitor.Y + 16),
                ShowInTaskbar = false,
                TopMost = true,
                ClientSize = root.Size
            };
            host.Controls.Add(root);
            host.Show();
            host.Activate();
        }

        ComponentFactory.PrepareAfterHandle(root, commands, component);
        Application.DoEvents();
        int currentDpi = NativeMethods.GetWindowDpi(root.FindForm()?.Handle ?? root.Handle);
        if (dpiMode == CaptureDpiMode.NativeMonitor)
        {
            if (currentDpi != targetDpi)
            {
                throw new CaptureStateUnsupportedException($"Native monitor reported {currentDpi} DPI instead of {targetDpi} DPI.");
            }

            return;
        }

        Rectangle currentBounds = NativeMethods.GetWindowRectangle(root.FindForm()?.Handle ?? root.Handle);
        double factor = (double)targetDpi / currentDpi;
        Rectangle suggestedBounds = new(
            currentBounds.X,
            currentBounds.Y,
            Math.Max(1, (int)Math.Round(currentBounds.Width * factor)),
            Math.Max(1, (int)Math.Round(currentBounds.Height * factor)));
        NativeMethods.SendDpiChanged(root.FindForm()?.Handle ?? root.Handle, targetDpi, suggestedBounds);
        Application.DoEvents();
    }

    private sealed class CaptureHostForm(IGitUICommands commands) : Form, IGitUICommandsSource, IGitModuleForm
    {
        public event EventHandler<GitUICommandsChangedEventArgs> UICommandsChanged
        {
            add { }
            remove { }
        }

        public IGitUICommands UICommands { get; } = commands;
    }

    private static string RequireExistingDirectory(string? value, string option)
    {
        string path = RequireValue(value, option);
        return Directory.Exists(path)
            ? Path.GetFullPath(path)
            : throw new DirectoryNotFoundException($"{option} directory '{path}' does not exist.");
    }

    private static string RequireExistingFile(string? value, string option)
    {
        string path = RequireValue(value, option);
        return File.Exists(path)
            ? Path.GetFullPath(path)
            : throw new FileNotFoundException($"{option} file '{path}' does not exist.", path);
    }

    private static string RequireValue(string? value, string option) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{option} is required.")
            : value;

    private static async Task<int> RunWorkerAsync(string runtimeRoot, IReadOnlyList<string> arguments)
    {
        ProcessStartInfo startInfo = new(Path.Combine(runtimeRoot, "GitExtensions.exe"));

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        startInfo.WorkingDirectory = runtimeRoot;
        startInfo.UseShellExecute = false;
        startInfo.Environment["GITEXTENSIONS_DEBUG_FAIL_FAST"] = "1";
        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The capture worker could not be started.");
        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    private static string Sanitize(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(character => invalid.Contains(character) || character is '.' or ':' ? '_' : character));
    }

    private static IReadOnlyList<CaptureComponentPlan> SelectComponents(CapturePlan plan, IReadOnlySet<string> selection)
    {
        IReadOnlyList<CaptureComponentPlan> result = selection.Count == 0
            ? plan.Components
            : plan.Components.Where(component => selection.Contains(component.TypeName)).ToArray();
        if (result.Count == 0)
        {
            throw new ArgumentException("No requested components exist in the capture plan.");
        }

        return result;
    }

    private static IReadOnlyList<int> SelectScales(CapturePlan plan, IReadOnlySet<int> selection)
    {
        IReadOnlyList<int> result = selection.Count == 0
            ? plan.Scales
            : plan.Scales.Where(selection.Contains).ToArray();
        if (result.Count == 0 || result.Any(scale => scale is not (100 or 125 or 150 or 200)))
        {
            throw new ArgumentException("Capture scales must be selected from 100, 125, 150, and 200.");
        }

        return result;
    }

    private static IReadOnlyList<CaptureThemePlan> SelectThemes(CapturePlan plan, IReadOnlySet<string> selection)
    {
        IReadOnlyList<CaptureThemePlan> result = selection.Count == 0
            ? plan.Themes
            : plan.Themes.Where(theme => selection.Contains(theme.Id)).ToArray();
        if (result.Count == 0)
        {
            throw new ArgumentException("No requested themes exist in the capture plan.");
        }

        return result;
    }

    private static string? TryGetWorkingTree()
    {
        ProcessStartInfo startInfo = new("git")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Environment.CurrentDirectory
        };
        startInfo.ArgumentList.Add("rev-parse");
        startInfo.ArgumentList.Add("--show-toplevel");
        using Process? process = Process.Start(startInfo);
        if (process is null)
        {
            return null;
        }

        string output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        return process.ExitCode == 0 ? output : null;
    }

    private static CaptureManifestEntry Unsupported(
        string componentType,
        string themeId,
        int scale,
        string state,
        string note) =>
        new()
        {
            ComponentType = componentType,
            ThemeId = themeId,
            ScalePercent = scale,
            State = state,
            Status = CaptureStateStatus.Unsupported,
            Note = note,
            DpiMode = null,
            CaptureMethod = CaptureMethod.Unsupported,
            ImageFile = null,
            TreeFile = null
        };

    private static CaptureManifestEntry Failed(
        string componentType,
        string themeId,
        int scale,
        string state,
        CaptureDpiMode dpiMode,
        Exception exception) =>
        new()
        {
            ComponentType = componentType,
            ThemeId = themeId,
            ScalePercent = scale,
            State = state,
            Status = CaptureStateStatus.Failed,
            Note = exception.ToString(),
            DpiMode = dpiMode,
            CaptureMethod = CaptureMethod.Unsupported,
            ImageFile = null,
            TreeFile = null
        };
}
