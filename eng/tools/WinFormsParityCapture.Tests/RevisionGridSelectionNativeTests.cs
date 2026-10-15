using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using AwesomeAssertions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.ScriptsEngine;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;
using SourceCommitInfo = GitUI.CommitInfo.CommitInfo;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class RevisionGridSelectionNativeTests
{
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    [TestCase(false)]
    [TestCase(true)]
    public void Actual_source_diff_endpoints_should_preserve_last_selection_details_and_first_current_cell_on_reload(bool reverseEndpoints)
    {
        Environment.GetEnvironmentVariable("GITEXTENSIONS_DEBUG_FAIL_FAST").Should().Be("1");
        string workspace = (string)InvokeHelper(typeof(WorkingDirPopupContractTests), "FindWorkspaceRoot");
        string bootstrapDirectory = Path.Combine(Path.GetTempPath(), "GitExtensions.RevisionSelectionBootstrap", Guid.NewGuid().ToString("N"));
        AssertExternalTemporaryPath(bootstrapDirectory, workspace);
        Directory.CreateDirectory(bootstrapDirectory);
        File.WriteAllText(Path.Combine(bootstrapDirectory, "GitExtensions.settings"),
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InvokeHelper(typeof(ToolStripOwnerOverflowTests), "InitializeAppSettingsWithoutRealConfiguration", bootstrapDirectory);
        AssertExternalTemporaryPath(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath, workspace);
        AssertExternalTemporaryPath(AppSettings.ApplicationDataPath.Value, workspace);
        InvokeHelper(typeof(WorkingDirPopupContractTests), "InitializeLocalCacheWithoutRealConfiguration", bootstrapDirectory, workspace);

        RunWithActualBootstrapSettings(settingsDirectory =>
        {
            AssertExternalTemporaryPath(settingsDirectory, workspace);
            string profilePath = Path.Combine(workspace, "eng", "tools", "WinFormsParityCapture", "Profiles", "reference.settings.json");
            CaptureSettingsProfile profile = CaptureSettingsProfile.Load(profilePath);
            foreach ((string key, string value) in profile.AppSettings)
            {
                AppSettings.SetString(key, value);
            }

            // Keep the original default/reference Ahead/Behind behavior after removing the fixture's cache lock.
            AppSettings.ShowAheadBehindData = true;
            AppSettings.ShowAheadBehindData.Should().BeTrue();

            using StringWriter emptyScripts = new();
            new XmlSerializer(typeof(BindingList<ScriptInfo>)).Serialize(emptyScripts, new BindingList<ScriptInfo>());
            AppSettings.OwnScripts = emptyScripts.ToString();
            AppSettings.CheckForUpdates = false;
            AppSettings.TelemetryEnabled = false;
            AppSettings.ShowConEmuTab.Value = false;
            AppSettings.CurrentTranslation = "English";
            AppSettings.RevisionGraphShowArtificialCommits = true;
            AppSettings.ShowStashes = false;
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            using Font font = new(profile.UiFontFamily, profile.UiFontSizePoints, FontStyle.Regular, GraphicsUnit.Point);
            AppSettings.Font = font;
            Type themeScopeType = typeof(WorkingDirPopupContractTests).GetNestedType("SourceThemeScope", BindingFlags.NonPublic)
                ?? throw new MissingMemberException(typeof(WorkingDirPopupContractTests).FullName, "SourceThemeScope");
            using IDisposable themeScope = (IDisposable)(Activator.CreateInstance(themeScopeType, workspace, settingsDirectory, "light")
                ?? throw new InvalidOperationException("The existing isolated native theme scope must be created."));
            string repository = Path.Combine(settingsDirectory, "repository");
            Directory.CreateDirectory(repository);
            ObjectId[] revisions = SeedRepository(repository);
            ObjectId firstId = reverseEndpoints ? revisions[^1] : revisions[0];
            ObjectId selectedId = reverseEndpoints ? revisions[0] : revisions[^1];
            string evidenceDirectory = Path.Combine(settingsDirectory, "RevisionSelectionProbe");
            Directory.CreateDirectory(evidenceDirectory);
            TestContext.Out.WriteLine($"revisionSelectionEvidence={evidenceDirectory}");
            TestContext.Out.WriteLine($"revisionSelectionBootstrap={bootstrapDirectory}");
            BrowseArguments arguments = new() { FirstId = firstId, SelectedId = selectedId };
            WithOriginalBrowse(repository, arguments, (form, getThreadException, closeAndDrain) =>
            {
                AppSettings.ShowAheadBehindData.Should().BeTrue();
                form.UICommands.GetRequiredService<IScriptsManager>().GetScripts().Should().BeEmpty();
                RevisionGridControl grid = form.RevisionGridControl;

                // Nested diff views reuse these control names; read the exact original main-window fields.
                SourceCommitInfo info = ReadSourceField<SourceCommitInfo>(form, "RevisionInfo");
                TabControl tabs = ReadSourceField<TabControl>(form, "CommitInfoTabControl");
                tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(tab => tab.Name == "CommitInfoTabPage");
                RevisionDataGridView view = ReadSourceField<RevisionDataGridView>(grid, "_gridView");
                using ApplicationContext loopContext = new();
                using System.Windows.Forms.Timer timer = new() { Interval = 50 };
                Stopwatch elapsed = Stopwatch.StartNew();
                Exception? failure = null;
                int stage = 0;
                form.Shown += (_, _) =>
                {
                    try
                    {
                        NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                        timer.Start();
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                        Finish();
                    }
                };
                timer.Tick += (_, _) =>
                {
                    try
                    {
                        if (getThreadException() is Exception exception)
                        {
                            ExceptionDispatchInfo.Capture(exception).Throw();
                        }

                        if (elapsed.Elapsed > TimeSpan.FromSeconds(30))
                        {
                            throw new TimeoutException("The actual original selection stages exceeded their bounded owned message loop.");
                        }

                        if (!IsLoadingComplete(grid) || grid.GetSelectedRevisions().Count != 2 || info.Revision is null)
                        {
                            return;
                        }

                        switch (stage)
                        {
                            case 0:

                                // PrintWindow acquires this source window directly, not the desktop screen.
                                // Stop this timer while activation pumps so it cannot reenter the same stage.
                                timer.Stop();
                                try
                                {
                                    NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                                }
                                finally
                                {
                                    timer.Start();
                                }

                                form.DeviceDpi.Should().Be(96);
                                GetForegroundWindow().Should().Be(form.Handle);
                                Application.OpenForms.Cast<Form>().Where(open => open.Visible).Should().ContainSingle().Which.Should().BeSameAs(form);
                                AssertAndRecord("initial", firstId, selectedId);
                                stage++;
                                grid.PerformRefreshRevisions(forceRefresh: true);
                                break;
                            case 1 when info.Revision.ObjectId == selectedId:
                                AssertAndRecord("reloaded", firstId, selectedId);
                                stage++;
                                grid.SetSelectedRevision(selectedId).Should().BeTrue();
                                grid.SetSelectedRevision(firstId, toggleSelection: true).Should().BeTrue();
                                break;
                            case 2 when info.Revision.ObjectId == firstId:
                                AssertAndRecord("reversed-public-selection", selectedId, firstId);
                                stage++;
                                grid.PerformRefreshRevisions(forceRefresh: true);
                                break;
                            case 3 when info.Revision.ObjectId == firstId:
                                AssertAndRecord("reversed-reloaded", selectedId, firstId);
                                stage++;
                                Finish();
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                        Finish();
                    }
                };
                EventHandler onIdle = null!;
                onIdle = (_, _) =>
                {
                    Application.Idle -= onIdle;
                    try
                    {
                        form.Show();
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                        Finish();
                    }
                };
                Application.Idle += onIdle;
                try
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = new Point(30, 30);
                    form.ClientSize = new Size(1180, 740);
                    Application.Run(loopContext);
                }
                finally
                {
                    timer.Stop();
                    Application.Idle -= onIdle;
                }

                if (failure is not null)
                {
                    ExceptionDispatchInfo.Capture(failure).Throw();
                }

                stage.Should().Be(4);
                return;

                void Finish()
                {
                    timer.Stop();
                    closeAndDrain();
                    loopContext.ExitThread();
                }

                void AssertAndRecord(string stage, ObjectId expectedCurrent, ObjectId expectedDetails)
                {
                    NativeTestWindowActivation.AssertForeground(form);
                    using CaptureImageResult image = ImageCapture.Capture(form, [], []);
                    image.Method.Should().Be(CaptureMethod.PrintWindow,
                        "this actual source selection surface must render directly without screen or synthetic acquisition");
                    object snapshot = new
                    {
                        stage,
                        reverseEndpoints,
                        repository,
                        bootstrapDirectory,
                        settingsDirectory,
                        settingsPath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath,
                        profilePath,
                        profileSha256 = HashFile(profilePath),
                        sourceAssembly = typeof(FormBrowse).Assembly.FullName,
                        sourceAssemblySha256 = HashFile(typeof(FormBrowse).Assembly.Location),
                        frameworkAssembly = typeof(DataGridView).Assembly.FullName,
                        frameworkAssemblySha256 = HashFile(typeof(DataGridView).Assembly.Location),
                        dpiMode = "nativeMonitor",
                        form.DeviceDpi,
                        form.TopMost,
                        captureMethod = image.Method.ToString(),
                        image.ScreenBounds,
                        acquisitionBoundary = "actual PrintWindow(PW_RENDERFULLCONTENT), including original window chrome, with ImageCapture's rendered/nonblank guard; no screen-stability dependency or source TopMost change",
                        sourceSelectedRevisions = grid.GetSelectedRevisions().Select(revision => revision.ObjectId.ToString()).ToArray(),
                        restorationObjectIds = view.SelectedObjectIds?.Select(objectId => objectId.ToString()).ToArray(),
                        nativeSelectedRowIndexes = view.SelectedRows.Cast<DataGridViewRow>().Select(row => row.Index).ToArray(),
                        currentRowIndex = view.CurrentCell?.RowIndex,
                        currentRevision = GetCurrentRevision(view)?.ToString(),
                        latestSelectedRevision = form.GetLatestSelectedRevision()?.ObjectId.ToString(),
                        selectedRevisionOrDefault = grid.GetSelectedRevisionOrDefault()?.ObjectId.ToString(),
                        commitInfoRevision = info.Revision?.ObjectId.ToString(),
                        view.Focused,
                        view.ContainsFocus,
                        AppSettings.ShowAheadBehindData,
                        settingsBoundary = "actual isolated bootstrap AppSettings container, shared by EffectiveSettings global cache; no UsingContainer lock held across the source message loop or joins",
                        sourceRoute = "actual original Browse Application.Run(ApplicationContext without MainForm)/OnLoad loop with timer-separated stages, real RevisionReader, public SetSelectedRevision and PerformRefreshRevisions; original TestAccessor used only to observe completion",
                        scriptsBoundary = "isolated serialized empty BindingList<ScriptInfo>, confirmed by actual source IScriptsManager before Show; repository seeded through Git CLI only",
                        limitations = "Source native100 endpoint/order/current/details consumer only; Ahead/Behind uses its original default but is not an audited axis; no physical modifier-input, full-state, raster-equality, theme-matrix or cross-platform closure claim.",
                    };
                    File.WriteAllText(Path.Combine(evidenceDirectory, stage + ".json"), JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true }));
                    TestContext.Out.WriteLine(JsonSerializer.Serialize(snapshot));
                    image.Bitmap.Save(Path.Combine(evidenceDirectory, stage + ".png"), ImageFormat.Png);
                    grid.GetSelectedRevisions().Select(revision => revision.ObjectId).Should().Equal(expectedDetails, expectedCurrent);
                    view.SelectedObjectIds.Should().Equal(expectedCurrent, expectedDetails);
                    GetCurrentRevision(view).Should().Be(expectedCurrent);
                    form.GetLatestSelectedRevision()?.ObjectId.Should().Be(expectedDetails);
                    grid.GetSelectedRevisionOrDefault()?.ObjectId.Should().Be(expectedDetails);
                    info.Revision?.ObjectId.Should().Be(expectedDetails);
                }
            });
        });
    }

    private static ObjectId? GetCurrentRevision(RevisionDataGridView view)
        => view.CurrentCell is DataGridViewCell cell ? view.GetRevision(cell.RowIndex)?.ObjectId : null;

    private static bool IsLoadingComplete(RevisionGridControl grid)
    {
        object accessor = typeof(RevisionGridControl).GetMethod("GetTestAccessor", PrivateInstance)?.Invoke(grid, null)
            ?? throw new MissingMethodException(typeof(RevisionGridControl).FullName, "GetTestAccessor");
        return (bool)(accessor.GetType().GetProperty("IsDataLoadComplete", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.GetValue(accessor)
            ?? throw new MissingMemberException(accessor.GetType().FullName, "IsDataLoadComplete"));
    }

    private static T ReadSourceField<T>(object owner, string name)
        where T : class
    {
        FieldInfo field = owner.GetType().GetField(name, BindingFlags.Public | PrivateInstance | BindingFlags.DeclaredOnly)
            ?? throw new MissingFieldException(owner.GetType().FullName, name);
        return field.GetValue(owner) as T
            ?? throw new InvalidOperationException($"The original {owner.GetType().FullName}.{name} must be a {typeof(T).FullName}.");
    }

    internal static ObjectId[] SeedRepository(string repository)
    {
        RunGit(repository, "init", "--quiet", "-b", "selection");
        RunGit(repository, "config", "user.name", "Native Selection Probe");
        RunGit(repository, "config", "user.email", "native-selection@example.com");
        RunGit(repository, "config", "commit.gpgsign", "false");
        RunGit(repository, "config", "core.hooksPath", ".native-probe-no-hooks");
        List<ObjectId> revisions = [];
        for (int index = 0; index < 3; index++)
        {
            File.AppendAllText(Path.Combine(repository, "tracked.txt"), $"revision {index}{Environment.NewLine}");
            RunGit(repository, "add", "--", "tracked.txt");
            RunGit(repository, "commit", "--quiet", "-m", $"revision-{index}");
            revisions.Add(ObjectId.Parse(RunGit(repository, "rev-parse", "HEAD").Trim()));
        }

        return [.. revisions];
    }

    private static string RunGit(string repository, params string[] arguments)
    {
        ProcessStartInfo startInfo = new("git")
        {
            WorkingDirectory = repository,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        StringBuilder output = new();
        StringBuilder error = new();
        using Process process = new() { StartInfo = startInfo };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                output.AppendLine(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                error.AppendLine(e.Data);
            }
        };
        process.Start().Should().BeTrue();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        bool exited = process.WaitForExit((int)TimeSpan.FromSeconds(15).TotalMilliseconds);
        if (!exited)
        {
            process.Kill(entireProcessTree: true);
        }

        // The parameterless wait also drains redirected callbacks after this owned CLI process exits.
        process.WaitForExit();
        exited.Should().BeTrue("the isolated fixture Git CLI must exit before the bounded deadline");
        process.ExitCode.Should().Be(0, error.ToString());
        return output.ToString();
    }

    internal static void WithOriginalBrowse(string repository, BrowseArguments arguments, Action<FormBrowse, Func<Exception?>, Action> action)
    {
        FieldInfo managerField = typeof(ThreadHelper).GetField("_taskManager", PrivateStatic)
            ?? throw new MissingFieldException(typeof(ThreadHelper).FullName, "_taskManager");
        object? originalManager = managerField.GetValue(null);
        SynchronizationContext? originalContext = SynchronizationContext.Current;
        ConcurrentQueue<Exception> exceptions = new();
        ThreadExceptionEventHandler onException = (_, args) => exceptions.Enqueue(args.Exception);
        Application.ThreadException += onException;
        Form contextOwner = new();
        _ = contextOwner.Handle;
        WindowsFormsSynchronizationContext nativeContext = SynchronizationContext.Current as WindowsFormsSynchronizationContext
            ?? new WindowsFormsSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(nativeContext);
        JoinableTaskContext context = new();
        (typeof(ThreadHelper).GetProperty(nameof(ThreadHelper.JoinableTaskContext), BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMemberException(typeof(ThreadHelper).FullName, nameof(ThreadHelper.JoinableTaskContext))).SetValue(null, context);
        TaskManager globalManager = managerField.GetValue(null) as TaskManager
            ?? throw new InvalidOperationException("The original global task manager must be initialized.");
        HashSet<TaskManager> managers = new(ReferenceEqualityComparer.Instance) { globalManager };
        FormBrowse? form = null;
        List<Exception> failures = [];
        bool drained = false;
        bool shutdownStarted = false;
        bool messageLoopDuringDrain = false;
        string? synchronizationContextDuringDrain = null;
        string? effectiveGlobalSettingsPath = null;
        ServiceContainer services = new();
        string evidenceDirectory = Path.Combine(Path.GetDirectoryName(repository)
            ?? throw new InvalidOperationException("The owned fixture repository must have a parent."), "RevisionSelectionProbe");
        Directory.CreateDirectory(evidenceDirectory);

        try
        {
            // The source MEF provider is process-owned and allows its user-plugin path to be initialized only once.
            if (ManagedExtensibility.UserPluginsPath is null)
            {
                string pluginsPath = Path.Combine(Path.GetDirectoryName(repository)
                    ?? throw new InvalidOperationException("The owned fixture repository must have a parent."), "UserPlugins");
                Directory.CreateDirectory(pluginsPath);
                ManagedExtensibility.Initialise(userPluginsPath: pluginsPath);
            }

            string actualPluginsPath = ManagedExtensibility.UserPluginsPath
                ?? throw new InvalidOperationException("The native source MEF provider must retain its isolated plugin root.");
            AssertExternalTemporaryPath(actualPluginsPath, (string)InvokeHelper(typeof(WorkingDirPopupContractTests), "FindWorkspaceRoot"));
            InvokeHelper(typeof(WinFormsBootstrap), "RegisterOriginalServices", services);
            if (services.GetService(typeof(IWindowsJumpListManager)) is IWindowsJumpListManager jumpList)
            {
                services.RemoveService(typeof(IWindowsJumpListManager));
                jumpList.Dispose();
            }

            services.AddService<IWindowsJumpListManager>(new CaptureWindowsJumpListManager());
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), repository);
            effectiveGlobalSettingsPath = module.EffectiveSettings.LowerPriority?.LowerPriority?.SettingsCache.SettingsFilePath;
            effectiveGlobalSettingsPath.Should().Be(AppSettings.SettingsFilePath);
            AppSettings.SettingsContainer.SettingsCache.SettingsFilePath.Should().Be(AppSettings.SettingsFilePath);
            form = new FormBrowse(new GitUICommands(services, module), arguments);
            CollectOwnedTaskManagers(form, managers);
            action(form, () => exceptions.TryPeek(out Exception? exception) ? exception : null, CloseAndDrain);
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }
        finally
        {
            CloseAndDrain();

            while (exceptions.TryDequeue(out Exception? exception))
            {
                if (!failures.Contains(exception))
                {
                    failures.Add(exception);
                }
            }

            File.WriteAllText(Path.Combine(evidenceDirectory, "lifetime.json"), JsonSerializer.Serialize(new
            {
                drained,
                messageLoopDuringDrain,
                formDisposed = form?.IsDisposed,
                ownerTaskManagerCount = managers.Count,
                remainingCollections = managers.Select(manager => ReadSourceField<JoinableTaskCollection>(manager, "_joinableTaskCollection").Any()).ToArray(),
                sourceContextStillInstalledBeforeRestore = ReferenceEquals(ThreadHelper.JoinableTaskContext, context),
                installedNativeSynchronizationContext = nativeContext.GetType().FullName,
                synchronizationContextDuringDrain,
                nativeSynchronizationContext = SynchronizationContext.Current?.GetType().FullName,
                receiptPhase = "after Application.Run exits, before source context disposal/restoration",
                AppSettings.ShowAheadBehindData,
                effectiveGlobalSettingsPath,
                actualSettingsPath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath,
                AppSettings.SettingsFilePath,
                settingsBoundary = "actual isolated bootstrap container/global cache with no UsingContainer lock across UI or joins; no Ahead/Behind audit claimed",
                errors = failures.Select(exception => exception.ToString()).ToArray(),
            }, new JsonSerializerOptions { WriteIndented = true }));

            if (drained)
            {
                context.Dispose();
                contextOwner.Dispose();
                managerField.SetValue(null, originalManager);
                SynchronizationContext.SetSynchronizationContext(originalContext);
                Application.ThreadException -= onException;
            }

            // A failed drain retains the live native context, services and collector until testhost exits;
            // restoring null or disposing that context would create the late reporting failure this probe guards.
        }

        if (failures.Count > 0)
        {
            throw new AggregateException("The original native selection probe or its owned lifetime failed.", failures);
        }

        void CloseAndDrain()
        {
            if (shutdownStarted)
            {
                return;
            }

            shutdownStarted = true;
            messageLoopDuringDrain = Application.MessageLoop;

            // Keep native dispatch alive after closing the tested form until every actual owner is drained.
            SynchronizationContext.SetSynchronizationContext(nativeContext);
            synchronizationContextDuringDrain = SynchronizationContext.Current?.GetType().FullName;
            if (form is not null)
            {
                try
                {
                    CollectOwnedTaskManagers(form, managers);
                    form.Close();
                    form.Dispose();
                    form.IsDisposed.Should().BeTrue("the original source form must stop producing work before joining its owners");
                }
                catch (Exception exception)
                {
                    failures.Add(exception);
                }
                finally
                {
                    CollectOwnedTaskManagers(form, managers);
                }
            }

            try
            {
                using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(15));
                JoinOwnedTaskManagers(context, managers, cancellation.Token);
                services.Dispose();

                // Disposing source services may schedule their last callbacks on the global manager.
                JoinOwnedTaskManagers(context, managers, cancellation.Token);
                if (form is not null)
                {
                    form.IsDisposed.Should().BeTrue("a successful drain must not retain a live original form");
                }

                drained = true;
            }
            catch (Exception exception)
            {
                failures.Add(exception);
            }
        }
    }

    private static void RunWithActualBootstrapSettings(Action<string> action)
    {
        string workspace = (string)InvokeHelper(typeof(WorkingDirPopupContractTests), "FindWorkspaceRoot");
        string actualSettingsPath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath;
        AssertExternalTemporaryPath(actualSettingsPath, workspace);
        actualSettingsPath.Should().Be(AppSettings.SettingsFilePath);
        string actualBootstrapDirectory = Path.GetDirectoryName(actualSettingsPath)
            ?? throw new InvalidOperationException("The source bootstrap settings must have an owned parent.");
        string directory = Path.Combine(actualBootstrapDirectory, "RevisionSelection-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        TestContext.Out.WriteLine($"revisionSelectionActualSettingsRoot={actualBootstrapDirectory}");

        // UsingContainer holds the old cache's lock; EffectiveSettings reuses that cache on its workers.
        // Keep the process-owned isolated bootstrap container in place, just as the real capture bootstrap does.
        action(directory);
    }

    private static void CollectOwnedTaskManagers(Control root, HashSet<TaskManager> managers)
    {
        HashSet<Control> visited = new(ReferenceEqualityComparer.Instance);
        Stack<Control> pending = new();
        pending.Push(root);
        while (pending.TryPop(out Control? control))
        {
            if (!visited.Add(control))
            {
                continue;
            }

            for (Type? type = control.GetType(); type is not null; type = type.BaseType)
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | PrivateInstance | BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType == typeof(TaskManager) && field.GetValue(control) is TaskManager manager)
                    {
                        managers.Add(manager);
                    }
                    else if (typeof(Control).IsAssignableFrom(field.FieldType) && field.GetValue(control) is Control child)
                    {
                        pending.Push(child);
                    }
                }
            }

            if (!control.IsDisposed)
            {
                foreach (Control child in control.Controls)
                {
                    pending.Push(child);
                }
            }
        }
    }

    private static void JoinOwnedTaskManagers(JoinableTaskContext context, HashSet<TaskManager> managers, CancellationToken cancellationToken)
    {
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (TaskManager manager in managers)
            {
                JoinableTaskCollection collection = ReadSourceField<JoinableTaskCollection>(manager, "_joinableTaskCollection");

                // The public TaskManager join catches cancellation; direct collection joining must fail on timeout.
                context.Factory.Run(() => collection.JoinTillEmptyAsync(cancellationToken));
            }
        }
        while (managers.Any(manager => ReadSourceField<JoinableTaskCollection>(manager, "_joinableTaskCollection").Any()));

        managers.Should().OnlyContain(manager => !ReadSourceField<JoinableTaskCollection>(manager, "_joinableTaskCollection").Any());
    }

    private static object InvokeHelper(Type owner, string name, params object[] arguments)
    {
        MethodInfo method = owner.GetMethod(name, PrivateStatic)
            ?? throw new MissingMethodException(owner.FullName, name);
        try
        {
            return method.Invoke(null, arguments) ?? new object();
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static void AssertExternalTemporaryPath(string? path, string workspace)
    {
        path.Should().NotBeNull();
        string fullPath = Path.GetFullPath(path ?? throw new InvalidOperationException("A source data path must be resolved."));
        fullPath.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase).Should().BeTrue();
        fullPath.StartsWith(Path.GetFullPath(workspace) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase).Should().BeFalse();
    }

    private static string HashFile(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();
}
