using System.ComponentModel.Design;
using System.Reflection;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Theming;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace WinFormsParityCapture;

internal sealed class WinFormsBootstrap : IDisposable
{
    private readonly ServiceContainer _serviceContainer;
    private readonly ThreadExceptionEventHandler _threadExceptionHandler;
    private Exception? _threadException;

    private WinFormsBootstrap(ServiceContainer serviceContainer, GitUICommands commands, ThreadExceptionEventHandler threadExceptionHandler)
    {
        _serviceContainer = serviceContainer;
        _threadExceptionHandler = threadExceptionHandler;
        Commands = commands;
    }

    public GitUICommands Commands { get; }

    public static WinFormsBootstrap Create(
        string repositoryPath,
        CaptureSettingsProfile profile,
        CaptureThemePlan theme,
        string isolationRoot)
    {
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            throw new ThreadStateException("WinForms capture requires an STA worker thread.");
        }

        Exception? earlyThreadException = null;
        WinFormsBootstrap? bootstrap = null;
        ThreadExceptionEventHandler threadExceptionHandler = (_, args) =>
        {
            if (bootstrap is null)
            {
                earlyThreadException ??= args.Exception;
            }
            else
            {
                bootstrap._threadException ??= args.Exception;
            }
        };
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += threadExceptionHandler;
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Control.CheckForIllegalCrossThreadCalls = true;

        string settingsPath = AppSettings.SettingsFilePath;
        EnsurePathIsContained(settingsPath, isolationRoot, "settings");

        string themePath = Path.Combine(AppContext.BaseDirectory, "Themes", theme.File);
        EnsurePathIsContained(themePath, isolationRoot, "theme");
        if (!File.Exists(themePath))
        {
            throw new FileNotFoundException("The isolated theme file is missing.", themePath);
        }

        ApplyProfile(profile, theme);
        SetDocumentationBaseUrl();
        AppSettings.LoadSettings();

        using (new Form())
        {
            SetJoinableTaskContext();
        }

        _ = GitModule.SystemEncoding;
        _ = new Executable("git").GetOutput("--version");
        ThemeModule.Load();

        string userPluginsPath = Path.Combine(isolationRoot, AppSettings.UserPluginsDirectoryName);
        EnsurePathIsContained(userPluginsPath, isolationRoot, "user plugins");
        Directory.CreateDirectory(userPluginsPath);
        ManagedExtensibility.Initialise(userPluginsPath: userPluginsPath);

        ServiceContainer serviceContainer = new();
        RegisterOriginalServices(serviceContainer);
        GitModule module = new(
            serviceContainer.GetRequiredService<IGitExecutorProvider>(),
            repositoryPath);
        GitUICommands commands = new(serviceContainer, module);
        bootstrap = new WinFormsBootstrap(serviceContainer, commands, threadExceptionHandler)
        {
            _threadException = earlyThreadException
        };
        return bootstrap;
    }

    public void Dispose()
    {
        Application.ThreadException -= _threadExceptionHandler;
        _serviceContainer.Dispose();
    }

    public void ThrowIfThreadException()
    {
        if (_threadException is not null)
        {
            throw new InvalidOperationException("The captured WinForms UI raised a UI-thread exception.", _threadException);
        }
    }

    private static void ApplyProfile(CaptureSettingsProfile profile, CaptureThemePlan theme)
    {
        AppSettings.Font = new Font(profile.UiFontFamily, profile.UiFontSizePoints, FontStyle.Regular, GraphicsUnit.Point);
        AppSettings.FixedWidthFont = new Font(profile.FixedFontFamily, profile.FixedFontSizePoints, FontStyle.Regular, GraphicsUnit.Point);
        AppSettings.CommitFont = new Font(profile.UiFontFamily, profile.UiFontSizePoints, FontStyle.Regular, GraphicsUnit.Point);
        AppSettings.MonospaceFont = new Font(profile.FixedFontFamily, profile.FixedFontSizePoints, FontStyle.Regular, GraphicsUnit.Point);
        AppSettings.ThemeId = new ThemeId(theme.Id, theme.IsBuiltin);
        AppSettings.ThemeVariations = [];
        AppSettings.UseSystemVisualStyle = theme.Id.Equals("light", StringComparison.OrdinalIgnoreCase);
        AppSettings.TelemetryEnabled = false;
        AppSettings.CheckForUpdates = false;
        AppSettings.ShowAvailableDiffTools = false;
        AppSettings.ShowConEmuTab.Value = false;
        AppSettings.Translation = "English";
        AppSettings.CurrentTranslation = "English";

        foreach ((string key, string value) in profile.AppSettings)
        {
            AppSettings.SetString(key, value);
        }
    }

    private static void EnsurePathIsContained(string path, string root, string description)
    {
        string fullPath = Path.GetFullPath(path);
        string fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"The {description} path '{fullPath}' escapes the isolated worker root '{fullRoot}'.");
        }
    }

    private static void RegisterOriginalServices(ServiceContainer serviceContainer)
    {
        Assembly entryAssembly = Assembly.Load("GitExtensions");
        Type registryType = entryAssembly.GetType("GitExtensions.ServiceContainerRegistry", throwOnError: true)!;
        MethodInfo method = registryType.GetMethod(
            "RegisterServices",
            BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMethodException(registryType.FullName, "RegisterServices");
        method.Invoke(obj: null, [serviceContainer]);
    }

    private static void SetDocumentationBaseUrl()
    {
        MethodInfo method = typeof(AppSettings).GetMethod(
            "SetDocumentationBaseUrl",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(typeof(AppSettings).FullName, "SetDocumentationBaseUrl");
        method.Invoke(obj: null, [AppSettings.ProductVersion]);
    }

    private static void SetJoinableTaskContext()
    {
        PropertyInfo property = typeof(GitUI.ThreadHelper).GetProperty(
            "JoinableTaskContext",
            BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMemberException(typeof(GitUI.ThreadHelper).FullName, "JoinableTaskContext");
        property.SetValue(obj: null, new JoinableTaskContext());
    }
}
