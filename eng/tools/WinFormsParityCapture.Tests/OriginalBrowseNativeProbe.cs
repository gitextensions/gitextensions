using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Serialization;
using AwesomeAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.ScriptsEngine;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Shares isolated source inputs and the existing live-loop lifetime across actual Browse probes.
/// </summary>
internal sealed class OriginalBrowseNativeProbe : IDisposable
{
    private static readonly List<OriginalBrowseNativeProbe> _retainedScopes = [];
    private readonly Font[] _fonts;
    private readonly WorkingDirPopupContractTests.SourceThemeScope _theme;
    private int _lifetimeReceiptCount;
    private bool _retainSourceResources;
    private bool _disposed;

    private OriginalBrowseNativeProbe(string repository, string bootstrapDirectory, string evidenceDirectory,
        string profilePath, Font[] fonts, WorkingDirPopupContractTests.SourceThemeScope theme)
    {
        Repository = repository;
        BootstrapDirectory = bootstrapDirectory;
        EvidenceDirectory = evidenceDirectory;
        ProfilePath = profilePath;
        _fonts = fonts;
        _theme = theme;
    }

    /// <summary>
    ///  Gets the explicitly supplied external repository, which the probe does not modify.
    /// </summary>
    internal string Repository { get; }

    /// <summary>
    ///  Gets the owned initial-bootstrap directory, not necessarily the reused process cache root.
    /// </summary>
    internal string BootstrapDirectory { get; }

    /// <summary>
    ///  Gets this case's unique retained output directory.
    /// </summary>
    internal string EvidenceDirectory { get; }

    /// <summary>
    ///  Gets the exact shared reference profile applied before source construction.
    /// </summary>
    internal string ProfilePath { get; }

    /// <summary>
    ///  Gets the requested settings font retained until the source owners have drained.
    /// </summary>
    internal Font UiFont => _fonts[0];

    /// <summary>
    ///  Gets the hash of the actual source light-theme input copied into isolation.
    /// </summary>
    internal string ThemeSha256 => _theme.SourceHashes["invariant.css"];

    /// <summary>
    ///  Initializes only guarded temporary configuration and the supplied read-only fixture.
    /// </summary>
    /// <param name="probeName">A single directory name identifying the consumer.</param>
    /// <param name="points">The shared source UI-font setting in points.</param>
    internal static OriginalBrowseNativeProbe Create(string probeName, int points = 9)
    {
        Environment.GetEnvironmentVariable("GITEXTENSIONS_DEBUG_FAIL_FAST").Should().Be("1");
        probeName.IndexOfAny(Path.GetInvalidFileNameChars()).Should().Be(-1);
        probeName.Should().NotBeNullOrWhiteSpace();
        string workspace = WorkingDirPopupContractTests.FindWorkspaceRoot();
        string repository = Environment.GetEnvironmentVariable("GITEXT_TOOLBAR_PROBE_REPOSITORY")
            ?? throw new InvalidOperationException("Supply the explicit external fixture through GITEXT_TOOLBAR_PROBE_REPOSITORY before running actual Browse probes.");
        repository = Path.TrimEndingDirectorySeparator(Path.GetFullPath(repository));
        AssertExternalTemporaryPath(repository, workspace);
        Directory.Exists(repository).Should().BeTrue();
        Directory.Exists(Path.Combine(repository, ".git")).Should().BeTrue(
            "the actual source fixture must be a standalone external checkout, not a linked worktree reaching the owner's repository");

        string bootstrapDirectory = Path.Combine(Path.GetTempPath(), "GitExtensions.ActualBrowseNativeBootstrap", Guid.NewGuid().ToString("N"));
        AssertExternalTemporaryPath(bootstrapDirectory, workspace);
        Directory.CreateDirectory(bootstrapDirectory);
        File.WriteAllText(Path.Combine(bootstrapDirectory, "GitExtensions.settings"),
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        ToolStripOwnerOverflowTests.InitializeAppSettingsWithoutRealConfiguration(bootstrapDirectory);
        AssertExternalTemporaryPath(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath, workspace);
        string applicationData = AppSettings.ApplicationDataPath.Value
            ?? throw new InvalidOperationException("The source bootstrap must resolve an isolated application-data root.");
        AssertExternalTemporaryPath(applicationData, workspace);
        string localData = WorkingDirPopupContractTests.InitializeLocalCacheWithoutRealConfiguration(bootstrapDirectory, workspace);
        string positionPath = ResetIsolatedBrowsePosition(localData, workspace);
        AppSettings.SettingsContainer.SettingsCache.SettingsFilePath.Should().Be(AppSettings.SettingsFilePath,
            "EffectiveSettings and the source global cache must share the isolated bootstrap; never hold UsingContainer's lock through source UI work");

        string evidenceDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, probeName, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(evidenceDirectory);
        string profilePath = Path.Combine(workspace, "eng", "tools", "WinFormsParityCapture", "Profiles", "reference.settings.json");
        CaptureSettingsProfile profile = CaptureSettingsProfile.Load(profilePath);
        foreach ((string key, string value) in profile.AppSettings)
        {
            AppSettings.SetString(key, value);
        }

        Font[] fonts =
        [
            new(profile.UiFontFamily, points, FontStyle.Regular, GraphicsUnit.Point),
            new(profile.FixedFontFamily, profile.FixedFontSizePoints, FontStyle.Regular, GraphicsUnit.Point),
            new(profile.UiFontFamily, points, FontStyle.Regular, GraphicsUnit.Point),
            new(profile.FixedFontFamily, profile.FixedFontSizePoints, FontStyle.Regular, GraphicsUnit.Point),
        ];
        AppSettings.Font = fonts[0];
        AppSettings.FixedWidthFont = fonts[1];
        AppSettings.CommitFont = fonts[2];
        AppSettings.MonospaceFont = fonts[3];
        AppSettings.ShowAheadBehindData = true;
        AppSettings.CheckForUpdates = false;
        AppSettings.TelemetryEnabled = false;
        AppSettings.ShowAvailableDiffTools = false;
        AppSettings.ShowConEmuTab.Value = false;
        AppSettings.Translation = "English";
        AppSettings.CurrentTranslation = "English";
        AppSettings.RevisionGraphShowArtificialCommits = true;
        AppSettings.ShowStashes = false;
        AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
        AppSettings.ShorteningRecentRepoPathStrategy = ShorteningRecentRepoPathStrategy.None;
        AppSettings.RecentReposComboMinWidth = 0;
        using StringWriter emptyScripts = new();
        new XmlSerializer(typeof(BindingList<ScriptInfo>)).Serialize(emptyScripts, new BindingList<ScriptInfo>());
        string serializedScripts = emptyScripts.ToString();
        AppSettings.OwnScripts = serializedScripts;

        // Seed every case explicitly, as both capture workers do; never inherit a prior probe's history.
        Repository history = new(repository + Path.DirectorySeparatorChar) { Category = "Development" };
        RepositoryStorage storage = new();
        storage.Save("history", [history]);
        storage.Save("history-favourite", [history]);
        string themeRoot = Path.Combine(bootstrapDirectory, probeName);
        WorkingDirPopupContractTests.SourceThemeScope theme = new(workspace, themeRoot, "light");
        OriginalBrowseNativeProbe probe = new(repository, bootstrapDirectory, evidenceDirectory, profilePath, fonts, theme);
        File.WriteAllText(Path.Combine(evidenceDirectory, "inputs.json"), JsonSerializer.Serialize(new
        {
            probeName,
            points,
            repository,
            bootstrapDirectory,
            localData,
            positionPath,
            actualSettingsPath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath,
            AppSettings.SettingsFilePath,
            profilePath,
            profileSha256 = HashFile(profilePath),
            theme.Provenance,
            theme.SourceHashes,
            uiFont = new { fonts[0].Name, fonts[0].SizeInPoints, style = fonts[0].Style.ToString() },
            AppSettings.ShowAheadBehindData,
            AppSettings.RecentReposComboMinWidth,
            shortening = AppSettings.ShorteningRecentRepoPathStrategy.ToString(),
            scriptsSha256 = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(serializedScripts))),
            history = storage.Load("history"),
            favourites = storage.Load("history-favourite"),
            settingsBoundary = "existing guarded portable bootstrap and local-cache aliases; actual global cache retained without UsingContainer lock; existing source theme scope",
            repositoryBoundary = "caller-supplied standalone checkout under Windows Temp; actual source reads only; no fixture Git writes or source-control substitution",
        }, new JsonSerializerOptions { WriteIndented = true }));
        TestContext.Out.WriteLine($"originalBrowseProbeEvidence={evidenceDirectory};bootstrap={bootstrapDirectory};repository={repository}");
        return probe;
    }

    /// <summary>
    ///  Calls the unchanged live-loop owner lifetime and preserves its fresh receipt per case.
    /// </summary>
    /// <param name="arguments">The real source constructor arguments.</param>
    /// <param name="action">The consumer, which must close and drain while its native loop is alive.</param>
    /// <param name="repository">An optional separately owned tiny fixture for a distinct input scenario.</param>
    internal void WithOriginalBrowse(BrowseArguments arguments, Action<FormBrowse, Func<Exception?>, Action> action, string? repository = null)
    {
        string actualRepository = repository is null ? Repository : Path.TrimEndingDirectorySeparator(Path.GetFullPath(repository));
        AssertExternalTemporaryPath(actualRepository, WorkingDirPopupContractTests.FindWorkspaceRoot());
        Directory.Exists(Path.Combine(actualRepository, ".git")).Should().BeTrue();
        DateTime started = DateTime.UtcNow;
        string lifetimePath = Path.Combine(Path.GetDirectoryName(actualRepository)
            ?? throw new InvalidOperationException("The external repository must have an owned parent."), "RevisionSelectionProbe", "lifetime.json");
        bool previouslyRetained = _retainSourceResources;

        // Only a fresh successful drain proves that theme pumping and font disposal are safe.
        _retainSourceResources = true;
        try
        {
            RevisionGridSelectionNativeTests.WithOriginalBrowse(actualRepository, arguments, action);
        }
        finally
        {
            if (File.Exists(lifetimePath) && File.GetLastWriteTimeUtc(lifetimePath) >= started)
            {
                using JsonDocument receipt = JsonDocument.Parse(File.ReadAllText(lifetimePath));
                _retainSourceResources = previouslyRetained || !receipt.RootElement.GetProperty("drained").GetBoolean();
                _lifetimeReceiptCount++;
                string receiptName = _lifetimeReceiptCount == 1 ? "lifetime.json" : $"lifetime-{_lifetimeReceiptCount}.json";
                File.Copy(lifetimePath, Path.Combine(EvidenceDirectory, receiptName), overwrite: false);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_retainSourceResources)
        {
            // The existing source lifetime deliberately retains its context/services on failed drain.
            // Keep these dependent resources strongly rooted until testhost exit, without pumping a theme reset.
            _retainedScopes.Add(this);
            TestContext.Out.WriteLine($"originalBrowseResourcesRetainedForFailedOrUnprovenDrain={EvidenceDirectory}");
            return;
        }

        _theme.Dispose();
        foreach (Font font in _fonts)
        {
            font.Dispose();
        }
    }

    private static void AssertExternalTemporaryPath(string path, string workspace)
    {
        string fullPath = Path.GetFullPath(path);
        string temporaryRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(Path.GetTempPath())) + Path.DirectorySeparatorChar;
        fullPath.StartsWith(temporaryRoot, StringComparison.OrdinalIgnoreCase).Should().BeTrue();
        string checkoutRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(workspace)) + Path.DirectorySeparatorChar;
        fullPath.StartsWith(checkoutRoot, StringComparison.OrdinalIgnoreCase).Should().BeFalse();
        for (string? current = fullPath; current is not null; current = Path.GetDirectoryName(current))
        {
            if (Directory.Exists(current) || File.Exists(current))
            {
                File.GetAttributes(current).HasFlag(FileAttributes.ReparsePoint).Should().BeFalse();
            }
        }
    }

    private static string HashFile(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static string ResetIsolatedBrowsePosition(string localData, string workspace)
    {
        const BindingFlags staticMembers = BindingFlags.Static | BindingFlags.NonPublic;
        string path = typeof(WindowPositionList).GetField("ConfigFilePath", staticMembers)?.GetValue(null) as string
            ?? throw new MissingFieldException(typeof(WindowPositionList).FullName, "ConfigFilePath");
        AssertExternalTemporaryPath(path, workspace);
        Path.GetFullPath(path).Should().Be(Path.Combine(localData, "WindowPositions.xml"));
        WindowPositionList list = WindowPositionList.Load()
            ?? throw new InvalidOperationException("The isolated source position list must load.");
        list.WindowPositions.RemoveAll(position => position.Name == nameof(FormBrowse));
        list.Save();
        Type manager = typeof(FormBrowse).Assembly.GetType("GitUI.WindowPositionManager", throwOnError: true)
            ?? throw new TypeLoadException("GitUI.WindowPositionManager");

        // A new input case has fresh saved-window state even when the source process cache survived a prior case.
        (manager.GetField("_windowPositionList", staticMembers)
            ?? throw new MissingFieldException(manager.FullName, "_windowPositionList")).SetValue(null, list);
        return path;
    }
}
