using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text.Json;
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
using NUnit.Framework;
using SourceCommitInfo = GitUI.CommitInfo.CommitInfo;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Records the actual original Browse's toolbar rafting and preferred sizes across load and resize history.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class FormBrowseToolStripOwnerNativeTests
{
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags StaticMembers = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [TestCase(9, 923)]
    [TestCase(11, 923)]
    [TestCase(14, 923)]
    [TestCase(9, 600)]
    [TestCase(11, 600)]
    [TestCase(14, 600)]
    public void Actual_source_Browse_should_record_toolbar_owners_after_restore_resize_regrowth_and_visibility(int points, int restoredWidth)
    {
        using OriginalBrowseNativeProbe probe = OriginalBrowseNativeProbe.Create("FormBrowseToolStripOwnerProbe", points);
        probe.WithOriginalBrowse(new BrowseArguments(), (form, getThreadException, closeAndDrain) =>
        {
            ToolStripContainer container = ReadField<ToolStripContainer>(form, "toolPanel");
            ToolStrip[] owners = [ReadField<ToolStrip>(form, "ToolStripMain"), ReadField<ToolStrip>(form, "ToolStripFilters"), ReadField<ToolStrip>(form, "ToolStripScripts")];
            ToolStripSplitButton working = ReadField<ToolStripSplitButton>(form, "_NO_TRANSLATE_WorkingDir");
            SourceCommitInfo info = ReadField<SourceCommitInfo>(form, "RevisionInfo");
            RevisionGridControl grid = form.RevisionGridControl;
            IReadOnlyDictionary<object, ControlTreeReader.FontBaseline> baselines = ControlTreeReader.CaptureFontBaselines(form);
            form.UICommands.GetRequiredService<IScriptsManager>().GetScripts().Should().BeEmpty();
            form.Font.SizeInPoints.Should().Be(points);
            form.ClientSize.Width.Should().Be(923, "the actual source Designer initializes the main window before saved-size restoration");
            HasExplicitFont(owners[0]).Should().BeFalse("normal Browse assigns the Form font, not the main ToolStrip's independent font");
            WriteJson(probe, "constructor", DescribeLayout(form, container, owners));

            _ = form.Handle;
            form.DeviceDpi.Should().Be(96);
            int clientHeight = form.ClientSize.Height;
            WindowPosition persisted = SeedActualSavedPosition(form, restoredWidth, clientHeight);
            WriteJson(probe, "persisted-position", new { persisted, expectedClientSize = new Size(restoredWidth, clientHeight), boundary = "actual isolated WindowPositionList file/cache; original OnLoad/RestorePosition applies the saved outer rectangle" });
            List<(string Name, Action Apply)> stages =
            [
                ("loaded-restored", () => { }),
                ("resize-923", () => Resize(923)),
                ("resize-1277", () => Resize(1277)),
                ("shrink-600", () => Resize(600)),
                ("regrow-1277", () => Resize(1277)),
                ("hide-filters", () => owners[1].Hide()),
                ("restore-filters", () => owners[1].Show()),
                ("hide-all", () => Array.ForEach(owners, owner => owner.Hide())),
                ("restore-all", () => Array.ForEach(owners, owner => owner.Show())),
                ("visibility-regrow-923", () => Resize(923)),
                ("explicit-owner-font-1277", () =>
                {
                    foreach (ToolStrip owner in owners)
                    {
                        owner.Font = probe.UiFont;
                    }

                    Resize(1277);
                }),
                ("explicit-owner-font-923", () => Resize(923)),
                ("explicit-owner-font-600", () => Resize(600)),
                ("explicit-owner-font-regrow-1277", () => Resize(1277)),
            ];
            using ApplicationContext loop = new();
            using System.Windows.Forms.Timer timer = new() { Interval = 50 };
            Stopwatch elapsed = Stopwatch.StartNew();
            Exception? failure = null;
            int stage = 0;
            int stableObservations = 0;
            string? previousLayout = null;
            bool prepared = false;
            bool finishing = false;
            string? loadedCaption = null;
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
                // Activation, source joins and ImageCapture pump native messages. Keep this stage non-reentrant.
                timer.Stop();
                try
                {
                    if (getThreadException() is Exception exception)
                    {
                        ExceptionDispatchInfo.Capture(exception).Throw();
                    }

                    if (elapsed.Elapsed > TimeSpan.FromSeconds(30))
                    {
                        throw new TimeoutException($"The original Browse toolbar probe did not finish stage {stage} within its owned native loop.");
                    }

                    if (!IsLoadingComplete(grid) || info.Revision is null || working.Text == "WorkingDir")
                    {
                        return;
                    }

                    if (!prepared)
                    {
                        stages[stage].Apply();
                        form.PerformLayout();
                        container.PerformLayout();
                        container.TopToolStripPanel.PerformLayout();
                        foreach (ToolStrip owner in owners)
                        {
                            owner.PerformLayout();
                        }

                        prepared = true;
                        previousLayout = null;
                        stableObservations = 0;
                        return;
                    }

                    object layout = DescribeLayout(form, container, owners);
                    string serializedLayout = JsonSerializer.Serialize(layout);
                    stableObservations = serializedLayout == previousLayout ? stableObservations + 1 : 1;
                    previousLayout = serializedLayout;
                    if (stableObservations < 3)
                    {
                        return;
                    }

                    NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                    if (stage == 0)
                    {
                        form.ClientSize.Should().Be(new Size(restoredWidth, clientHeight));
                        HasExplicitFont(owners[0]).Should().BeFalse();
                        loadedCaption = working.Text;
                    }

                    working.Text.Should().Be(loadedCaption, "this same repository, history and shortening setting must supply the caption at every resize");
                    if (stage >= 10)
                    {
                        owners.Should().OnlyContain(owner => HasExplicitFont(owner));
                        owners.Select(owner => owner.Font.SizeInPoints).Should().OnlyContain(size => size == points);
                    }

                    RetainFrame(probe, form, container, owners, baselines, stages[stage].Name, stage, points, restoredWidth);
                    stage++;
                    prepared = false;
                    if (stage == stages.Count)
                    {
                        Finish();
                    }
                }
                catch (Exception exception)
                {
                    failure = exception;
                    try
                    {
                        WriteJson(probe, "failure", new { stage, elapsedMilliseconds = elapsed.ElapsedMilliseconds, exception = exception.ToString() });
                    }
                    finally
                    {
                        Finish();
                    }
                }
                finally
                {
                    if (!finishing)
                    {
                        timer.Start();
                    }
                }
            };
            Application.Idle += OnIdle;
            try
            {
                Application.Run(loop);
            }
            finally
            {
                timer.Stop();
                Application.Idle -= OnIdle;
            }

            if (failure is not null)
            {
                ExceptionDispatchInfo.Capture(failure).Throw();
            }

            stage.Should().Be(stages.Count);
            TestContext.Out.WriteLine($"actualBrowseToolbarStages={stage};points={points};restoredWidth={restoredWidth};evidence={probe.EvidenceDirectory}");
            return;

            void Finish()
            {
                if (finishing)
                {
                    return;
                }

                finishing = true;
                timer.Stop();
                try
                {
                    closeAndDrain();
                }
                finally
                {
                    loop.ExitThread();
                }
            }

            void OnIdle(object? sender, EventArgs args)
            {
                Application.Idle -= OnIdle;
                try
                {
                    form.Show();
                }
                catch (Exception exception)
                {
                    failure = exception;
                    Finish();
                }
            }

            void Resize(int width)
            {
                form.ClientSize = new Size(width, clientHeight);
            }
        });
    }

    private static object DescribeLayout(FormBrowse form, ToolStripContainer container, ToolStrip[] owners)
        => new
        {
            form.Bounds,
            form.ClientSize,
            form.DeviceDpi,
            form.TopMost,
            formFont = DescribeFont(form.Font),
            containerBounds = container.Bounds,
            contentBounds = container.ContentPanel.Bounds,
            topPanel = new { container.TopToolStripPanel.Bounds, container.TopToolStripPanel.DisplayRectangle, container.TopToolStripPanel.Padding, container.TopToolStripPanel.RowMargin },
            rows = container.TopToolStripPanel.Rows.Select((row, index) => new
            {
                index,
                row.Bounds,
                row.DisplayRectangle,
                row.Margin,
                row.Padding,
                owners = row.Controls.Select(control => control.Name).ToArray(),
                ownerMinimumSizes = row.Controls.OfType<ToolStrip>().Select(owner => new { owner.Name, size = InvokeInstance<Size>(row, "GetMinimumSize", owner) }).ToArray(),
            }).ToArray(),
            owners = owners.Select(owner => DescribeOwner(container.TopToolStripPanel, owner)).ToArray(),
        };

    private static object DescribeOwner(ToolStripPanel panel, ToolStrip owner)
    {
        ToolStripPanelRow? row = panel.Rows.SingleOrDefault(candidate => candidate.Controls.Contains(owner));
        object cell = ReadProperty<object>(owner, "ToolStripPanelCell");
        ToolStripItem[] displayed = ReadProperty<ToolStripItemCollection>(owner, "DisplayedItems").Cast<ToolStripItem>().ToArray();
        return new
        {
            owner.Name,
            owner.Bounds,
            owner.ClientSize,
            owner.DisplayRectangle,
            owner.Visible,
            owner.AutoSize,
            owner.MinimumSize,
            owner.MaximumSize,
            preferredNatural = owner.GetPreferredSize(Size.Empty),
            preferredConstrained = owner.GetPreferredSize(panel.DisplayRectangle.Size),
            rowIndex = row is null ? (int?)null : Array.IndexOf(panel.Rows, row),
            rowMinimumSize = row is null ? (Size?)null : InvokeInstance<Size>(row, "GetMinimumSize", owner),
            owner.Padding,
            owner.Margin,
            gripStyle = owner.GripStyle.ToString(),
            owner.GripRectangle,
            font = DescribeFont(owner.Font),
            explicitFont = HasExplicitFont(owner),
            defaultMenuFont = DescribeFont(ReadStaticProperty<Font>(typeof(ToolStripManager), "DefaultFont")),
            cell = new { bounds = ReadProperty<Rectangle>(cell, "Bounds"), cachedBounds = ReadProperty<Rectangle>(cell, "CachedBounds"), margin = ReadProperty<Padding>(cell, "Margin"), maximumSize = ReadProperty<Size>(cell, "MaximumSize") },
            displayedItems = displayed.Select(item => item.Name).ToArray(),
            overflowButton = new { owner.OverflowButton.Bounds, owner.OverflowButton.Visible, owner.OverflowButton.Available, placement = owner.OverflowButton.Placement.ToString(), preferred = owner.OverflowButton.GetPreferredSize(Size.Empty) },
            items = owner.Items.Cast<ToolStripItem>().Select(item => new
            {
                item.Name,
                item.Text,
                item.Bounds,
                item.AutoSize,
                item.Available,
                item.Visible,
                item.Enabled,
                item.Margin,
                item.Padding,
                placement = item.Placement.ToString(),
                overflow = item.Overflow.ToString(),
                displayStyle = item.DisplayStyle.ToString(),
                font = DescribeFont(item.Font),
                imageSize = item.Image?.Size,
                preferredNatural = item.GetPreferredSize(Size.Empty),
                preferredConstrained = item.GetPreferredSize(owner.DisplayRectangle.Size),
                hostControl = item is ToolStripControlHost host ? new { host.Control.Bounds, host.Control.Margin, host.Control.MinimumSize, font = DescribeFont(host.Control.Font) } : null,
            }).ToArray(),
        };
    }

    private static object DescribeFont(Font font) => new { font.Name, font.SizeInPoints, style = font.Style.ToString(), font.Height };

    private static bool HasExplicitFont(Control control) => InvokeInstance<bool>(control, "IsFontSet");

    private static T InvokeInstance<T>(object owner, string name, params object[] arguments)
    {
        MethodInfo method = owner.GetType().GetMethod(name, InstanceMembers)
            ?? throw new MissingMethodException(owner.GetType().FullName, name);
        return method.Invoke(owner, arguments) is T result ? result : throw new InvalidOperationException($"Source {name} must return {typeof(T).FullName}.");
    }

    private static bool IsLoadingComplete(RevisionGridControl grid)
    {
        object accessor = InvokeInstance<object>(grid, "GetTestAccessor");
        return ReadProperty<bool>(accessor, "IsDataLoadComplete");
    }

    private static T ReadField<T>(object owner, string name)
        where T : class
        => owner.GetType().GetField(name, InstanceMembers)?.GetValue(owner) as T
            ?? throw new MissingFieldException(owner.GetType().FullName, name);

    private static T ReadProperty<T>(object owner, string name)
        => owner.GetType().GetProperty(name, InstanceMembers)?.GetValue(owner) is T value
            ? value : throw new MissingMemberException(owner.GetType().FullName, name);

    private static T ReadStaticProperty<T>(Type owner, string name)
        => owner.GetProperty(name, StaticMembers)?.GetValue(null) is T value
            ? value : throw new MissingMemberException(owner.FullName, name);

    private static void RetainFrame(OriginalBrowseNativeProbe probe, FormBrowse form, ToolStripContainer container, ToolStrip[] owners,
        IReadOnlyDictionary<object, ControlTreeReader.FontBaseline> baselines, string stage, int ordinal, int points, int restoredWidth)
    {
        NativeTestWindowActivation.AssertForeground(form);
        form.DeviceDpi.Should().Be(96);
        using CaptureImageResult image = ImageCapture.Capture(form, [], []);
        image.Method.Should().Be(CaptureMethod.PrintWindow, "actual source Browse is acquired directly; no screen occlusion, scaling or TopMost substitution");
        string name = $"{ordinal:D2}-{stage}";
        image.Bitmap.Save(Path.Combine(probe.EvidenceDirectory, name + ".png"), ImageFormat.Png);
        ControlTreeReader reader = new(form, form.DeviceDpi, baselines);
        CaptureDocument document = new()
        {
            SchemaVersion = CaptureDocument.CurrentSchemaVersion,
            Component = new CaptureComponent { TypeName = typeof(FormBrowse).FullName ?? throw new InvalidOperationException("The source Browse type must have a name."), AssemblyName = typeof(FormBrowse).Assembly.GetName().Name ?? "GitUI" },
            Capture = new CaptureMetadata
            {
                Framework = "winforms",
                Theme = new CaptureTheme { Id = "light", Kind = "builtin", SourceSha256 = probe.ThemeSha256 },
                ScalePercent = 100,
                Dpi = new CaptureDpi { X = form.DeviceDpi, Y = form.DeviceDpi },
                DpiMode = CaptureDpiMode.NativeMonitor,
                State = stage,
                StateStatus = CaptureStateStatus.Captured,
                StateNote = "Actual source toolbar-owner diagnostic; resize history and explicit owner-font phases are inputs, not full parity closure.",
            },
            Image = new CaptureImage { WidthPx = image.Bitmap.Width, HeightPx = image.Bitmap.Height, CaptureMethod = image.Method, Acquisitions = image.Acquisitions },
            Surfaces = [reader.ReadPrimary(form, image.PrimaryScreenBounds)],
        };
        File.WriteAllText(Path.Combine(probe.EvidenceDirectory, name + ".tree.json"), CaptureJson.Serialize(document));
        WriteJson(probe, name, new
        {
            stage,
            ordinal,
            points,
            restoredWidth,
            assignedOwnerFont = ordinal >= 10,
            probe.Repository,
            probe.ProfilePath,
            profileSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(probe.ProfilePath))),
            sourceAssembly = typeof(FormBrowse).Assembly.FullName,
            sourceAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(FormBrowse).Assembly.Location))),
            frameworkAssembly = typeof(ToolStrip).Assembly.FullName,
            layout = DescribeLayout(form, container, owners),
            AppSettings.ShowAheadBehindData,
            AppSettings.RecentReposComboMinWidth,
            shortening = AppSettings.ShorteningRecentRepoPathStrategy.ToString(),
            image.ScreenBounds,
            captureMethod = image.Method.ToString(),
            scope = "actual original FormBrowse constructor/OnLoad/RestorePosition and runtime source controls; single same-input case retains resize, visibility and font history; no guessed wrap or width cap",
        });
    }

    private static WindowPosition SeedActualSavedPosition(FormBrowse form, int width, int height)
    {
        Type managerType = typeof(FormBrowse).Assembly.GetType("GitUI.WindowPositionManager", throwOnError: true)
            ?? throw new TypeLoadException("GitUI.WindowPositionManager");
        FieldInfo cache = managerType.GetField("_windowPositionList", StaticMembers)
            ?? throw new MissingFieldException(managerType.FullName, "_windowPositionList");
        WindowPositionList list = WindowPositionList.Load()
            ?? throw new InvalidOperationException("The isolated source position list must load.");
        string path = typeof(WindowPositionList).GetField("ConfigFilePath", StaticMembers)?.GetValue(null) as string
            ?? throw new MissingFieldException(typeof(WindowPositionList).FullName, "ConfigFilePath");
        string localData = AppSettings.LocalApplicationDataPath.Value
            ?? throw new InvalidOperationException("The original position file must have an isolated local data directory.");
        string localRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(localData)) + Path.DirectorySeparatorChar;
        Path.GetFullPath(path).StartsWith(localRoot, StringComparison.OrdinalIgnoreCase).Should().BeTrue();
        Size nativeChrome = form.Size - form.ClientSize;
        WindowPosition position = new(new Rectangle(new Point(30, 30), new Size(width + nativeChrome.Width, height + nativeChrome.Height)), form.DeviceDpi, FormWindowState.Normal, nameof(FormBrowse));
        list.AddOrUpdate(position);
        list.Save();

        // Match a fresh worker's settings read even when prior source tests populated the process cache.
        cache.SetValue(null, WindowPositionList.Load());
        return position;
    }

    private static void WriteJson(OriginalBrowseNativeProbe probe, string name, object snapshot)
        => File.WriteAllText(Path.Combine(probe.EvidenceDirectory, name + ".json"), JsonSerializer.Serialize(snapshot, JsonOptions));
}
