using System.ComponentModel.Design;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtensions.ParityCapture;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using Microsoft.VisualStudio.Threading;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// Consumer-only font/overflow probes, not a
// claim that this source-shaped subset reproduces the full FormBrowse ToolStripPanel.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class ToolStripOwnerOverflowTests
{
    private const string ParentFontSwitch = "System.Windows.Forms.ApplyParentFontToMenus";
    private const string PortableSettingName = "IsPortable";
    private const string SettingsFileName = "GitExtensions.settings";
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    private static IEnumerable<TestCaseData> FontCases()
    {
        foreach (int points in new[] { 9, 11, 18, 22 })
        {
            foreach (bool explicitlyAssignedOwnerFont in new[] { false, true })
            {
                foreach (bool rightToLeft in new[] { false, true })
                {
                    yield return new TestCaseData(points, explicitlyAssignedOwnerFont, rightToLeft);
                }
            }
        }
    }

    private static IEnumerable<TestCaseData> OverflowCases()
    {
        foreach (int points in new[] { 9, 22 })
        {
            foreach (bool rightToLeft in new[] { false, true })
            {
                foreach (int ownerWidth in new[] { 180, 320, 480 })
                {
                    foreach (int workingDirectoryFixedWidth in new[] { 0, 280 })
                    {
                        yield return new TestCaseData(points, rightToLeft, ownerWidth, workingDirectoryFixedWidth)
                            .SetCategory(workingDirectoryFixedWidth > 0 ? "NativeFixedOverflow" : "NativeAutoOverflow")
                            .SetCategory($"NativeOverflow_{points}_{rightToLeft}_{ownerWidth}_{workingDirectoryFixedWidth}");
                    }
                }
            }
        }
    }

    [TestCaseSource(nameof(FontCases))]
    public void Source_owner_height_should_record_effective_ambient_font_and_distinguish_auto_and_fixed_items(
        int points, bool explicitlyAssignedOwnerFont, bool rightToLeft)
    {
        using Font requestedFont = new("Segoe UI", points, FontStyle.Regular, GraphicsUnit.Point);
        using Bitmap image = SourceImage();
        using Form host = CreateHost(requestedFont);
        using ToolStripEx strip = CreateStrip(image, rightToLeft);
        if (explicitlyAssignedOwnerFont)
        {
            strip.Font = requestedFont;
        }

        host.Controls.Add(strip);
        _ = host.Handle;
        _ = strip.Handle;
        strip.DeviceDpi.Should().Be(96);
        strip.PerformLayout();
        int expectedHeight = strip.Items.Cast<ToolStripItem>()
            .Where(item => item.Available && item.Overflow != ToolStripItemOverflow.Always)
            .Select(item => (item.AutoSize ? item.GetPreferredSize(Size.Empty).Height : item.Height) + item.Margin.Vertical)
            .Prepend(25 - strip.Padding.Vertical).Max() + strip.Padding.Vertical;
        strip.GetPreferredSize(Size.Empty).Height.Should().Be(expectedHeight,
            "native ToolStrip.GetPreferredSizeHorizontal composes source defaults, effective font and actual item margins");
        if (explicitlyAssignedOwnerFont)
        {
            strip.Font.Should().BeSameAs(requestedFont);
        }

        object before = Snapshot(strip);
        strip.Size = strip.GetPreferredSize(Size.Empty);
        strip.PerformLayout();
        foreach (ToolStripItem item in strip.Items.Cast<ToolStripItem>().Where(item => item.Placement == ToolStripItemPlacement.Main))
        {
            if (item.AutoSize)
            {
                item.Height.Should().Be(Math.Max(0, strip.DisplayRectangle.Height - item.Margin.Vertical));
            }
            else
            {
                item.Height.Should().Be(22, "the explicit fixed consumer item must not be stretched with autosized items");
            }
        }

        WriteEvidence(strip, "owner-height", new
        {
            requestedParentFont = DescribeFont(requestedFont),
            explicitlyAssignedOwnerFont,
            expectedHeight,
            before,
            after = Snapshot(strip),
            scope = "original ToolStripEx consumer; no actual FormBrowse initialization or multiple-toolbar panel claim",
        });
    }

    [TestCaseSource(nameof(OverflowCases))]
    public void Source_overflow_should_reparent_the_same_items_wrap_them_and_restore_main_placement_after_growth(
        int points, bool rightToLeft, int ownerWidth, int workingDirectoryFixedWidth)
    {
        using Font font = new("Segoe UI", points, FontStyle.Regular, GraphicsUnit.Point);
        using Bitmap image = SourceImage();
        using Form host = CreateHost(font);
        using ToolStripEx strip = CreateStrip(image, rightToLeft);
        using NativePopupLifetime popupLifetime = new(host);
        strip.Font = font;
        ToolStripItem workingDirectory = strip.Items["_NO_TRANSLATE_WorkingDir"]
            ?? throw new InvalidOperationException("The source working-directory consumer is required.");
        if (workingDirectoryFixedWidth > 0)
        {
            workingDirectory.AutoSize = false;
            workingDirectory.Size = new Size(workingDirectoryFixedWidth, 22);
        }

        host.Controls.Add(strip);
        _ = host.Handle;
        _ = strip.Handle;
        strip.DeviceDpi.Should().Be(96);
        int preferredHeight = strip.GetPreferredSize(Size.Empty).Height;

        // Explicit owner constraint isolates the genuine split-stack layout. This is
        // not an assertion about ToolStripPanel's constraint allocation among three strips.
        strip.AutoSize = false;
        strip.Size = new Size(ownerWidth, preferredHeight);
        host.Show();
        host.Activate();
        host.Focus();
        Stopwatch activation = Stopwatch.StartNew();
        do
        {
            Application.DoEvents();
            Thread.Sleep(10);
        }
        while (activation.ElapsedMilliseconds < 100);

        Form.ActiveForm.Should().BeSameAs(host, "the genuine popup must not be opened ahead of queued native owner activation");
        host.ContainsFocus.Should().BeTrue();
        strip.PerformLayout();
        ToolStripItem[] sourceItems = strip.Items.Cast<ToolStripItem>().Where(item => item.Available).ToArray();
        sourceItems.Should().OnlyContain(item => item.Placement != ToolStripItemPlacement.None,
            "these default AsNeeded source consumers must remain accessible as a main item or an actual overflow item");
        ToolStripItem[] overflowItems = sourceItems.Where(item => item.Placement == ToolStripItemPlacement.Overflow).ToArray();
        overflowItems.Should().NotBeEmpty();
        strip.OverflowButton.Visible.Should().BeTrue();
        strip.OverflowButton.DropDown.Items.IsReadOnly.Should().BeTrue();
        object beforeOpen = Snapshot(strip);
        ToolStripDropDown popup = strip.OverflowButton.DropDown;
        popupLifetime.Popup = popup;
        List<string> lifetime = [];
        popup.Opening += (_, args) => lifetime.Add($"opening:cancel={args.Cancel};visible={popup.Visible};size={popup.Size}");
        popup.Opened += (_, _) => lifetime.Add($"opened:visible={popup.Visible};size={popup.Size}");
        popup.Closing += (_, args) => lifetime.Add($"closing:reason={args.CloseReason};cancel={args.Cancel};size={popup.Size}");
        popup.Closed += (_, args) => lifetime.Add($"closed:reason={args.CloseReason};size={popup.Size}");
        strip.OverflowButton.ShowDropDown();
        bool visibleImmediately = popup.Visible;
        Application.DoEvents();
        WriteEvidence(strip, "overflow-open-diagnostic", new
        {
            points,
            rightToLeft,
            ownerWidth,
            workingDirectoryFixedWidth,
            host.Visible,
            host.ContainsFocus,
            hostBounds = DescribeRectangle(host.Bounds),
            hostClient = DescribeRectangle(host.ClientRectangle),
            visibleImmediately,
            popupVisibleAfterEvents = popup.Visible,
            popupBounds = DescribeRectangle(popup.Bounds),
            popupPreferred = DescribeSize(popup.GetPreferredSize(Size.Empty)),
            popupDisplay = DescribeRectangle(popup.DisplayRectangle),
            popup.ClientSize,
            popup.HasChildren,
            popup.Padding,
            strip.OverflowButton.HasDropDownItems,
            displayed = ReadDisplayedItems(popup).Select(DescribeItem).ToArray(),
            beforeOpen,
            afterOpen = Snapshot(strip),
            lifetime,
        });
        if (!popup.Visible)
        {
            string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "OwnerOverflowProbe", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            using CaptureImageResult diagnostic = ImageCapture.Capture(host, [], []);
            diagnostic.Bitmap.Save(Path.Combine(directory, "actual-host-after-failed-popup.png"), ImageFormat.Png);
            File.WriteAllText(Path.Combine(directory, "capture.json"), JsonSerializer.Serialize(new
            {
                method = diagnostic.Method.ToString(),
                diagnostic.ScreenBounds,
                note = "Actual owner window after source ShowDropDown failed; no absent popup was fabricated.",
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"failedOverflowHostEvidence={directory}");
        }
        else
        {
            WritePopupEvidence(host, popup, new { points, rightToLeft, ownerWidth, workingDirectoryFixedWidth });
        }

        popup.Visible.Should().BeTrue();
        FlowLayoutSettings flow = popup.LayoutSettings as FlowLayoutSettings
            ?? throw new InvalidOperationException("The native overflow must expose its actual flow layout.");
        flow.FlowDirection.Should().Be(FlowDirection.LeftToRight);
        flow.WrapContents.Should().BeTrue();
        ToolStripItem[] displayed = ReadDisplayedItems(popup);
        displayed.Should().Equal(overflowItems, "overflow presents the same original item references in source order, not cloned menu commands");
        foreach (ToolStripItem item in overflowItems)
        {
            item.Owner.Should().BeSameAs(strip);
            item.GetCurrentParent().Should().BeSameAs(popup);
        }

        if (workingDirectoryFixedWidth > 0)
        {
            workingDirectory.Width.Should().Be(workingDirectoryFixedWidth,
                "source placement moves an item to overflow; it does not squeeze the configured fixed-width source item");
        }

        object opened = Snapshot(strip);
        object popupMetadata = new
        {
            type = popup.GetType().FullName,
            preferred = DescribeSize(popup.GetPreferredSize(Size.Empty)),
            size = DescribeSize(popup.Size),
            displayRectangle = DescribeRectangle(popup.DisplayRectangle),
            flowDirection = flow.FlowDirection.ToString(),
            flow.WrapContents,
            displayed = displayed.Select(DescribeItem).ToArray(),
            sourcePreferredConstraint = 200,
            inputMode = "sourceShowDropDownApi; no physical-pointer or keyboard claim",
        };

        // The reason-taking overload only hides the popup. Parameterless Close also
        // exits ModalMenuFilter when the final dropdown leaves its queue; otherwise
        // the next fixture's owner is compared against this soon-to-be-destroyed HWND.
        popup.Close();
        Application.DoEvents();
        popup.Visible.Should().BeFalse();
        strip.Width = strip.GetPreferredSize(Size.Empty).Width + 20;
        strip.PerformLayout();
        sourceItems.Should().OnlyContain(item => item.Placement == ToolStripItemPlacement.Main);
        strip.OverflowButton.Visible.Should().BeFalse();
        sourceItems.Select(item => item.GetCurrentParent()).Should().OnlyContain(parent => ReferenceEquals(parent, strip));
        WriteEvidence(strip, "owner-overflow", new
        {
            points,
            rightToLeft,
            ownerWidth,
            workingDirectoryFixedWidth,
            beforeOpen,
            opened,
            popup = popupMetadata,
            popupVisibleAfterClose = popup.Visible,
            closingLifetime = lifetime,
            restored = Snapshot(strip),
            scope = "explicit constrained source ToolStripEx consumer, not full FormBrowse ToolStripPanel allocation",
        });
    }

    [TestCase(9)]
    [TestCase(11)]
    [TestCase(18)]
    [TestCase(22)]
    public void Cold_FormBrowse_initialization_should_record_actual_ambient_toolbar_font_without_assigning_the_owner(int points)
    {
        WithIsolatedSettings(settingsDirectory =>
        {
            using Font requested = new("Segoe UI", points, FontStyle.Regular, GraphicsUnit.Point);
            AppSettings.Font = requested;
            AppSettings.Translation = "English";
            AppSettings.CurrentTranslation = "English";
            AppSettings.CheckForUpdates = false;
            AppSettings.TelemetryEnabled = false;
            AppSettings.ShowConEmuTab.Value = false;
            WithOriginalBrowse(settingsDirectory, form =>
            {
                ToolStripEx strip = form.Controls.Find("ToolStripMain", searchAllChildren: true).Single() as ToolStripEx
                    ?? throw new InvalidOperationException("The original Browse must expose its actual named main strip.");
                form.Font.Name.Should().Be(requested.Name);
                form.Font.SizeInPoints.Should().Be(points);
                bool explicitOwnerFont = HasExplicitFont(strip);
                explicitOwnerFont.Should().BeFalse("the original initializer assigns the form's Font, not its ToolStrip owner's font");
                bool effectiveParentFontSwitch = AppContext.TryGetSwitch(ParentFontSwitch, out bool enabled) && enabled;
                Font independentExpectedFont = effectiveParentFontSwitch ? form.Font : GetNativeMenuFont();
                strip.Font.Should().Be(independentExpectedFont,
                    "ToolStrip.Font chooses the menu default unless its own font or the actual compatibility switch opts into inheritance");
                strip.DeviceDpi.Should().Be(96);
                strip.PerformLayout();
                foreach (ToolStripItem item in strip.Items.Cast<ToolStripItem>())
                {
                    item.Font.Should().Be(strip.Font, "these source main-strip fields do not assign their own font");
                }

                WriteEvidence(strip, "cold-FormBrowse-constructor", new
                {
                    requestedSettingsFont = DescribeFont(requested),
                    actualFormFont = DescribeFont(form.Font),
                    explicitOwnerFont,
                    effectiveParentFontSwitch,
                    snapshot = Snapshot(strip),
                    settingsDirectory,
                    actualSettingsFilePath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath,
                    initializationRoute = "original FormBrowse constructor including InitializeComplete and InitMenusAndToolbars; no Show/OnLoad",
                    repositoryPath = settingsDirectory,
                    scope = "actual cold FormBrowse ambient-font policy; no explicit strip/item font and no repository load",
                });
                Path.GetFullPath(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath).Should()
                    .Be(Path.Combine(settingsDirectory, SettingsFileName));
            });
        });
    }

    private static bool HasExplicitFont(Control control)
    {
        MethodInfo method = typeof(Control).GetMethod("IsFontSet", PrivateInstance)
            ?? throw new MissingMethodException(typeof(Control).FullName, "IsFontSet");
        return (bool)(method.Invoke(control, null)
            ?? throw new InvalidOperationException("The native explicit-font state must be readable."));
    }

    private static Font GetNativeMenuFont()
    {
        PropertyInfo property = typeof(ToolStripManager).GetProperty("DefaultFont", PrivateStatic | BindingFlags.Public)
            ?? throw new MissingMemberException(typeof(ToolStripManager).FullName, "DefaultFont");
        return property.GetValue(null) as Font
            ?? throw new InvalidOperationException("The actual framework menu-default font must be available.");
    }

    private static object DescribeGrip(ToolStrip strip)
    {
        PropertyInfo property = typeof(ToolStrip).GetProperty("Grip", PrivateInstance)
            ?? throw new MissingMemberException(typeof(ToolStrip).FullName, "Grip");
        ToolStripItem grip = property.GetValue(strip) as ToolStripItem
            ?? throw new InvalidOperationException("The native grip item must exist.");
        return new
        {
            style = strip.GripStyle.ToString(),
            gripEnabled = grip.Enabled,
            margin = strip.GripMargin.ToString(),
            bounds = DescribeRectangle(grip.Bounds),
            preferred = DescribeSize(grip.GetPreferredSize(Size.Empty)),
            grip.Enabled,
            grip.Available,
            grip.Visible,
            gripThickness = grip.GetType().GetProperty("GripThickness", PrivateInstance | BindingFlags.Public)?.GetValue(grip),
        };
    }

    private static void WithOriginalBrowse(string repositoryPath, Action<FormBrowse> action)
    {
        FieldInfo managerField = typeof(ThreadHelper).GetField("_taskManager", PrivateStatic)
            ?? throw new MissingFieldException(typeof(ThreadHelper).FullName, "_taskManager");
        object? originalManager = managerField.GetValue(null);
        SynchronizationContext? originalSynchronizationContext = SynchronizationContext.Current;
        using JoinableTaskContext context = new();
        PropertyInfo contextProperty = typeof(ThreadHelper).GetProperty(nameof(ThreadHelper.JoinableTaskContext), BindingFlags.Public | BindingFlags.Static)
            ?? throw new MissingMemberException(typeof(ThreadHelper).FullName, nameof(ThreadHelper.JoinableTaskContext));
        contextProperty.SetValue(null, context);
        using ServiceContainer services = new();
        MethodInfo register = typeof(WinFormsBootstrap).GetMethod("RegisterOriginalServices", PrivateStatic)
            ?? throw new MissingMethodException(typeof(WinFormsBootstrap).FullName, "RegisterOriginalServices");
        register.Invoke(null, [services]);
        if (services.GetService(typeof(IWindowsJumpListManager)) is IWindowsJumpListManager jumpList)
        {
            services.RemoveService(typeof(IWindowsJumpListManager));
            jumpList.Dispose();
        }

        services.AddService<IWindowsJumpListManager>(new CaptureWindowsJumpListManager());
        GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), repositoryPath);
        GitUICommands commands = new(services, module);
        try
        {
            using FormBrowse form = new(commands, new BrowseArguments());
            _ = form.Handle;
            action(form);
        }
        finally
        {
            managerField.SetValue(null, originalManager);
            SynchronizationContext.SetSynchronizationContext(originalSynchronizationContext);
        }
    }

    private static Form CreateHost(Font parentFont)
        => new()
        {
            AutoScaleMode = AutoScaleMode.None,
            ClientSize = new Size(1800, 240),
            Font = parentFont,
            ShowInTaskbar = false,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(80, 80),
        };

    private static ToolStripEx CreateStrip(Image image, bool rightToLeft)
    {
        ToolStripEx strip = new()
        {
            Name = "ToolStripMain",
            AutoSize = true,
            ClickThrough = true,
            Dock = DockStyle.None,
            DrawBorder = false,
            GripEnabled = false,
            GripStyle = ToolStripGripStyle.Visible,
            GripMargin = Padding.Empty,
            Padding = Padding.Empty,
            LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow,
            ImageScalingSize = new Size(16, 16),
            RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No,
            Location = new Point(3, 0),
        };
        Type workingType = typeof(ToolStripEx).Assembly.GetType("GitUI.CommandsDialogs.Menus.WorkingDirectoryToolStripSplitButton", throwOnError: true)!;
        ToolStripItem working = (ToolStripItem)(Activator.CreateInstance(workingType)
            ?? throw new InvalidOperationException("The original working-directory item must be constructible."));
        working.Name = "_NO_TRANSLATE_WorkingDir";
        working.Text = "A long repository path that must remain reachable through native overflow";
        strip.Items.AddRange(new ToolStripItem[]
        {
            new ToolStripButton { Name = "RefreshButton", Image = image, DisplayStyle = ToolStripItemDisplayStyle.Image },
            new ToolStripSeparator { Name = "toolStripSeparator0" },
            new ToolStripSplitButton { Name = "menuCommitInfoPosition", Image = image, DisplayStyle = ToolStripItemDisplayStyle.Image },
            new ToolStripSplitButton { Name = "toolStripButtonLevelUp", Image = image, DisplayStyle = ToolStripItemDisplayStyle.Image },
            working,
            new ToolStripSplitButton { Name = "branchSelect", Image = image, Text = "Branch" },
            new ToolStripButton { Name = "toolStripButtonCommit", Image = image, Text = "Commit (10)" },
            new ToolStripButton { Name = "EditSettings", Image = image, DisplayStyle = ToolStripItemDisplayStyle.Image },
            new ToolStripButton { Name = "sourceFixedHeightConsumer", Image = image, AutoSize = false, Size = new Size(23, 22) },
        });
        return strip;
    }

    private static Bitmap SourceImage()
    {
        Bitmap image = new(16, 16, PixelFormat.Format32bppArgb);
        image.SetResolution(96, 96);
        using Graphics graphics = Graphics.FromImage(image);
        graphics.Clear(Color.Magenta);
        return image;
    }

    private static ToolStripItem[] ReadDisplayedItems(ToolStrip strip)
    {
        PropertyInfo property = typeof(ToolStrip).GetProperty("DisplayedItems", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The native overflow displayed-item property is required.");
        ToolStripItemCollection items = property.GetValue(strip) as ToolStripItemCollection
            ?? throw new InvalidOperationException("The native overflow must retain its original displayed-item collection.");
        return items.Cast<ToolStripItem>().ToArray();
    }

    private static object Snapshot(ToolStrip strip)
        => new
        {
            ownerFont = DescribeFont(strip.Font),
            parentFont = strip.Parent is null ? null : DescribeFont(strip.Parent.Font),
            explicitParentFontSwitch = AppContext.TryGetSwitch(ParentFontSwitch, out bool parentFontEnabled),
            parentFontEnabled,
            defaultMenuFont = DescribeFont(GetNativeMenuFont()),
            preferred = DescribeSize(strip.GetPreferredSize(Size.Empty)),
            size = DescribeSize(strip.Size),
            displayRectangle = DescribeRectangle(strip.DisplayRectangle),
            strip.AutoSize,
            grip = DescribeGrip(strip),
            padding = strip.Padding.ToString(),
            overflowButton = DescribeItem(strip.OverflowButton),
            items = strip.Items.Cast<ToolStripItem>().Select(DescribeItem).ToArray(),
        };

    private static object DescribeItem(ToolStripItem item)
        => new
        {
            item.Name,
            type = item.GetType().FullName,
            item.Available,
            item.AutoSize,
            preferred = DescribeSize(item.GetPreferredSize(Size.Empty)),
            bounds = DescribeRectangle(item.Bounds),
            margin = item.Margin.ToString(),
            placement = item.Placement.ToString(),
            overflow = item.Overflow.ToString(),
            itemOwner = item.Owner?.Name,
            actualParent = item.GetCurrentParent()?.GetType().FullName,
            font = DescribeFont(item.Font),
            item.Text,
        };

    private static object DescribeFont(Font font)
        => new { font.Name, font.OriginalFontName, font.SizeInPoints, style = font.Style.ToString(), font.Height };

    private static object DescribeSize(Size size) => new { size.Width, size.Height };

    private static object DescribeRectangle(Rectangle rectangle) => new { rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height };

    private static void WriteEvidence(ToolStrip strip, string role, object source)
    {
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "OwnerOverflowProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        using Bitmap frame = new(strip.Width, strip.Height, PixelFormat.Format32bppArgb);
        frame.SetResolution(96, 96);
        strip.DrawToBitmap(frame, strip.ClientRectangle);
        frame.Save(Path.Combine(directory, "source-owner-client.png"), ImageFormat.Png);
        object metadata = new
        {
            role,
            dpiMode = "nativeMonitor",
            deviceDpi = strip.DeviceDpi,
            captureMethod = "drawToBitmapClientOnly",
            applicationColorMode = Application.ColorMode.ToString(),
            source,
            note = "Layout and actual item ownership proof only; no popup chrome, physical hover, complete main owner or raster identity claim.",
        };
        File.WriteAllText(Path.Combine(directory, "probe.json"), JsonSerializer.Serialize(metadata,
            new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        TestContext.Out.WriteLine($"ownerOverflowProbeEvidence={directory}");
        TestContext.Out.WriteLine(JsonSerializer.Serialize(metadata));
    }

    private static void WritePopupEvidence(Form host, ToolStripDropDown popup, object source)
    {
        // ShowDropDown queues actual client/popup painting. Keep the real message loop
        // active before asking PrintWindow to render owned surfaces independently.
        Stopwatch painting = Stopwatch.StartNew();
        do
        {
            Application.DoEvents();
            Thread.Sleep(1);
        }
        while (painting.ElapsedMilliseconds < 100);

        host.Refresh();
        popup.Refresh();
        host.Update();
        popup.Update();
        bool popupCaptured = popup.Visible;
        using CaptureImageResult capture = ImageCapture.Capture(host, popupCaptured ? [popup] : [], []);
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "OwnerOverflowProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        capture.Bitmap.Save(Path.Combine(directory, popupCaptured ? "actual-owned-overflow.png" : "actual-host-without-popup.png"), ImageFormat.Png);
        object metadata = new
        {
            role = "owned-overflow-popup",
            dpiMode = "nativeMonitor",
            deviceDpi = host.DeviceDpi,
            popupDeviceDpi = popup.DeviceDpi,
            captureMethod = capture.Method.ToString(),
            capture.ScreenBounds,
            capture.PrimaryScreenBounds,
            hostBounds = DescribeRectangle(host.Bounds),
            popupBounds = DescribeRectangle(popup.Bounds),
            popupCaptured,
            popupVisible = popup.Visible,
            paintingSettlementMilliseconds = painting.ElapsedMilliseconds,
            source,
            note = popupCaptured
                ? "Actual native owner and owned popup including window chrome; no physical input or full Browse claim."
                : "Unsupported popup evidence: the popup closed during real paint settlement; only the actual owner is captured.",
        };
        File.WriteAllText(Path.Combine(directory, "popup.json"), JsonSerializer.Serialize(metadata,
            new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine);
        TestContext.Out.WriteLine($"ownerOverflowPopupEvidence={directory}");
        TestContext.Out.WriteLine(JsonSerializer.Serialize(metadata));
        popupCaptured.Should().BeTrue();
        popup.Visible.Should().BeTrue();
        host.DeviceDpi.Should().Be(96);
        popup.DeviceDpi.Should().Be(96);
        capture.Method.Should().Be(CaptureMethod.PrintWindow);
        capture.ScreenBounds.Contains(popup.Bounds).Should().BeTrue();
    }

    private static void WithIsolatedSettings(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.ToolStripOwnerFontProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string settingsPath = Path.Combine(directory, SettingsFileName);
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(directory);
        using GitExtSettingsCache cache = new(settingsPath, autoSave: false);
        DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
        AppSettings.UsingContainer(settings, () => action(directory));

        // Retain the isolated settings beside the reported directory until the test host
        // exits: a first AppSettings initialization owns a live FileSystemWatcher/cache.
        // The tranche cleanup removes these explicit temporary roots after preserving JSON.
        TestContext.Out.WriteLine($"isolated original settings retained: {directory}");
    }

    private static void InitializeAppSettingsWithoutRealConfiguration(string directory)
    {
        Type settingsType = typeof(AppSettings).Assembly.GetType("GitCommands.Properties.Settings", throwOnError: true)
            ?? throw new TypeLoadException("GitCommands.Properties.Settings");
        ApplicationSettingsBase configuration = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ApplicationSettingsBase
            ?? throw new MissingMemberException(settingsType.FullName, "Default");
        _ = configuration[PortableSettingName];
        SettingsPropertyValue portable = configuration.PropertyValues[PortableSettingName]
            ?? throw new MissingMemberException(settingsType.FullName, PortableSettingName);
        object originalPortableValue = portable.PropertyValue;
        bool originalDirty = portable.IsDirty;
        FieldInfo frameworkPath = typeof(Application).GetField("s_executablePath", PrivateStatic)
            ?? throw new MissingFieldException(typeof(Application).FullName, "s_executablePath");
        object? originalFrameworkPath = frameworkPath.GetValue(null);
        string actualExecutablePath = Application.ExecutablePath;
        string isolatedExecutablePath = Path.Combine(directory, "GitExtensions.exe");
        try
        {
            // AppSettings' static constructor selects its initial cache before any public
            // isolation boundary is available. This test-only alias keeps that first read
            // and its migrations in the seeded temporary path, without editing product code.
            portable.PropertyValue = true;
            frameworkPath.SetValue(null, isolatedExecutablePath);
            _ = AppSettings.SettingsContainer;
            FieldInfo sourcePath = typeof(AppSettings).GetField("_applicationExecutablePath", PrivateStatic)
                ?? throw new MissingFieldException(typeof(AppSettings).FullName, "_applicationExecutablePath");
            if (Equals(sourcePath.GetValue(null), isolatedExecutablePath))
            {
                sourcePath.SetValue(null, actualExecutablePath);
            }
        }
        finally
        {
            frameworkPath.SetValue(null, originalFrameworkPath);
            portable.PropertyValue = originalPortableValue;
            portable.IsDirty = originalDirty;
        }
    }

    private sealed class NativePopupLifetime(Form host) : IDisposable
    {
        public ToolStripDropDown? Popup { get; set; }

        public void Dispose()
        {
            // Cleanup must also run after a failed assertion. Let the framework finish
            // normal menu-mode teardown before destroying the native owner and pumping
            // its queued activation/close messages; never reset static menu state.
            if (Popup is { IsDisposed: false } popup)
            {
                popup.Close();
                Application.DoEvents();
            }

            host.Close();
            Application.DoEvents();
        }
    }
}
