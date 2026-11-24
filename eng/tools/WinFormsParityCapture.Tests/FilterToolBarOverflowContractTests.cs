using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using GitExtensions.ParityCapture;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// Exercises the actual original consumer in the standalone native test assembly.
// This standalone consumer needs no repository, Bind, Git command,
// real settings, CSS-theme root or clipboard. Native color-mode cases are not CSS-theme proof.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class FilterToolBarOverflowContractTests
{
    private const string FilterTypeName = "GitUI.UserControls.FilterToolBar";
    private const string BranchComboName = "tscboBranchFilter";
    private const string RevisionComboName = "tstxtRevisionFilter";
    private const string SettingsFileName = "GitExtensions.settings";
    private const string PortableSettingName = "IsPortable";
    private const uint GetFontMessage = 0x31;
    private const int WideWidth = 800;
    private const int DesignerWidth = 339;
    private const int NarrowWidth = 50;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly string[] SourceItemNames =
    [
        "tsbtnAdvancedFilter", "tsbShowReflog", "tssbtnShowBranches", "toolStripLabel1",
        BranchComboName, "tsddbtnBranchFilter", "toolStripSeparator19", "tslblRevisionFilter",
        RevisionComboName, "tsddbtnRevisionFilter", "tsmiShowOnlyFirstParent",
    ];

    // Eight explicit consumer cases; every case follows the same width transaction. Two
    // assigned-owner cases sample native dark mode without multiplying an unrelated matrix.
    [TestCase(9, false, false, false)]
    [TestCase(9, false, true, false)]
    [TestCase(11, false, false, false)]
    [TestCase(11, false, true, false)]
    [TestCase(9, true, false, false)]
    [TestCase(9, true, true, true)]
    [TestCase(11, true, false, true)]
    [TestCase(11, true, true, false)]
    public void FilterToolBar_should_keep_actual_items_combo_hosts_and_ambient_font_through_overflow_and_regrowth(
        int parentPoints, bool assignedOwnerFont, bool rightToLeft, bool dark)
    {
        WithIsolatedSettings(settingsDirectory =>
        {
            SystemColorMode originalMode = Application.ColorMode;
            Exception? threadException = null;
            ThreadExceptionEventHandler onThreadException = (_, args) => threadException ??= args.Exception;
            Application.ThreadException += onThreadException;
            try
            {
                Application.SetColorMode(dark ? SystemColorMode.Dark : SystemColorMode.Classic);
                using Font parentFont = new("Segoe UI", parentPoints, FontStyle.Regular, GraphicsUnit.Point);
                AppSettings.Font = parentFont;
                AppSettings.RevisionFilterDropdowns = ["retained revision history"];
                using Form host = new()
                {
                    AutoScaleMode = AutoScaleMode.None,
                    ClientSize = new Size(920, 240),
                    Font = parentFont,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.CenterScreen,
                    Text = "Actual original FilterToolBar consumer",
                };
                using ToolStripEx strip = CreateOriginalFilter();
                using NativePopupLifetime lifetime = new(host);
                strip.GetType().FullName.Should().Be(FilterTypeName);
                strip.Items.Cast<ToolStripItem>().Select(item => item.Name).Should().Equal(SourceItemNames);
                ToolStripItem[] sourceItems = strip.Items.Cast<ToolStripItem>().ToArray();
                sourceItems.Should().OnlyContain(item => item.Overflow == ToolStripItemOverflow.AsNeeded);
                ToolStripComboBox[] comboHosts = sourceItems.OfType<ToolStripComboBox>().ToArray();
                comboHosts.Select(item => item.Name).Should().Equal(BranchComboName, RevisionComboName);
                ComboBox[] controls = comboHosts.Select(item => item.ComboBox).ToArray();
                comboHosts.Should().OnlyContain(item => !item.AutoSize && item.Width == 100);
                comboHosts.Should().OnlyContain(item => item.FlatStyle == FlatStyle.System);
                comboHosts[1].Items.Cast<string>().Should().Contain("retained revision history");
                object constructed = Snapshot(strip);
                if (assignedOwnerFont)
                {
                    strip.Font = parentFont;
                }

                strip.RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No;
                strip.Location = new Point(8, 8);
                host.Controls.Add(strip);
                _ = host.Handle;
                _ = strip.Handle;
                Font menuFont = ReadNativeMenuFont();
                strip.Font.Should().Be(assignedOwnerFont ? parentFont : menuFont,
                    "an unassigned ToolStrip owner retains the source menu default, independently of its Form font");

                // ToolStripComboBox constructs its own control with the menu font. Do not
                // assert that assigning the owner font implicitly assigns the hosted font.
                controls.Should().OnlyContain(control => control.Font.Equals(menuFont));
                int preferredHeight = strip.GetPreferredSize(Size.Empty).Height;
                strip.AutoSize = false;
                strip.Size = new Size(WideWidth, preferredHeight);
                host.Show();
                host.Activate();
                host.Focus();
                Settle(host, strip);
                Form.ActiveForm.Should().BeSameAs(host);
                host.ContainsFocus.Should().BeTrue();
                host.DeviceDpi.Should().Be(96);
                strip.DeviceDpi.Should().Be(96);
                foreach (ToolStripComboBox combo in comboHosts)
                {
                    combo.Text = combo.Name == BranchComboName ? "retained branch" : "retained revision";
                    combo.SelectionStart = 2;
                    combo.SelectionLength = 3;
                }

                string[] texts = controls.Select(control => control.Text).ToArray();
                string evidenceDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                    "FilterOverflowProbe", Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(evidenceDirectory);
                List<object> stages = [];
                List<string> closeReasons = [];
                ToolStripDropDown? observedPopup = null;
                foreach (int width in new[] { WideWidth, DesignerWidth, NarrowWidth, WideWidth })
                {
                    strip.Width = width;
                    strip.PerformLayout();
                    Settle(host, strip);
                    stages.Add(new { stage = $"closed-{width}", snapshot = Snapshot(strip) });
                    WriteProgress(evidenceDirectory, stages);
                    sourceItems.Should().OnlyContain(item => ReferenceEquals(item.Owner, strip));
                    strip.Items.Cast<ToolStripItem>().Should().Equal(sourceItems);
                    controls.Select(control => control.Text).Should().Equal(texts);
                    ToolStripItem[] overflow = sourceItems.Where(item => item.Placement == ToolStripItemPlacement.Overflow).ToArray();
                    ToolStripItem[] main = sourceItems.Where(item => item.Placement == ToolStripItemPlacement.Main).ToArray();
                    sourceItems.Should().OnlyContain(item => item.Placement != ToolStripItemPlacement.None);
                    strip.OverflowButton.Visible.Should().Be(overflow.Length > 0);
                    foreach (ToolStripItem item in main)
                    {
                        item.GetCurrentParent().Should().BeSameAs(strip);
                        strip.ClientRectangle.Contains(item.Bounds).Should().BeTrue(
                            "the native source only places complete items on the main strip");
                    }

                    if (width == WideWidth)
                    {
                        overflow.Should().BeEmpty();
                        foreach (ToolStripComboBox combo in comboHosts)
                        {
                            combo.ComboBox.Parent.Should().BeSameAs(strip);
                            combo.Margin.Should().Be(new Padding(1, 0, 1, 0));
                        }

                        continue;
                    }

                    overflow.Should().NotBeEmpty();
                    ToolStripDropDown popup = strip.OverflowButton.DropDown;
                    lifetime.Popup = popup;
                    if (!ReferenceEquals(observedPopup, popup))
                    {
                        popup.Closed += (_, args) => closeReasons.Add(args.CloseReason.ToString());
                        observedPopup = popup;
                    }

                    stages.Add(new { stage = $"before-open-{width}", snapshot = Snapshot(strip) });
                    strip.OverflowButton.ShowDropDown();
                    Settle(host, strip, popup);
                    stages.Add(new { stage = $"opened-{width}", snapshot = Snapshot(strip), popup = Snapshot(popup) });
                    WriteProgress(evidenceDirectory, stages);
                    popup.Visible.Should().BeTrue();
                    popup.DeviceDpi.Should().Be(96);
                    popup.OwnerItem.Should().BeSameAs(strip.OverflowButton);
                    popup.Items.IsReadOnly.Should().BeTrue();

                    // ToolStripOverflow does not display source separators, even when
                    // their owner placement is Overflow. Their logical ownership is unchanged.
                    ToolStripItem[] displayed = overflow.Where(item => item is not ToolStripSeparator).ToArray();
                    ReadDisplayedItems(popup).Should().Equal(displayed);
                    FlowLayoutSettings flow = popup.LayoutSettings as FlowLayoutSettings
                        ?? throw new InvalidOperationException("The real source overflow must retain its native flow layout.");
                    flow.FlowDirection.Should().Be(FlowDirection.LeftToRight);
                    flow.WrapContents.Should().BeTrue();
                    foreach (ToolStripItem item in displayed)
                    {
                        item.Owner.Should().BeSameAs(strip);
                        item.GetCurrentParent().Should().BeSameAs(popup);
                        item.Bounds.Width.Should().BeGreaterThan(0);
                        item.Bounds.Height.Should().BeGreaterThan(0);
                        if (item is ToolStripComboBox combo)
                        {
                            combo.ComboBox.Parent.Should().BeSameAs(popup);
                            combo.Margin.Should().Be(new Padding(2));
                            popup.ClientRectangle.Contains(combo.ComboBox.Bounds).Should().BeTrue();
                        }
                    }

                    for (int index = 0; index < comboHosts.Length; index++)
                    {
                        comboHosts[index].ComboBox.Should().BeSameAs(controls[index]);
                    }

                    RetainFrame(host, popup, evidenceDirectory, $"opened-{width}");
                    popup.Close();
                    Settle(host, strip);
                    popup.Visible.Should().BeFalse();
                    stages.Add(new { stage = $"after-close-{width}", snapshot = Snapshot(strip) });
                    WriteProgress(evidenceDirectory, stages);
                    controls.Select(control => control.Text).Should().Equal(texts);
                    if (threadException is not null)
                    {
                        throw new InvalidOperationException("The actual original consumer raised a UI-thread exception.", threadException);
                    }
                }

                MethodInfo setFocus = strip.GetType().GetMethod("SetFocus", BindingFlags.Instance | BindingFlags.Public)
                    ?? throw new InvalidOperationException("The actual original filter must retain its public focus route.");
                foreach (int width in new[] { WideWidth, DesignerWidth, NarrowWidth })
                {
                    strip.Width = width;
                    Settle(host, strip);
                    host.ActiveControl = null;
                    host.Focus();
                    Settle(host, strip);
                    stages.Add(new { stage = $"focus-before-{width}", snapshot = Snapshot(strip) });
                    setFocus.Invoke(strip, null);
                    Settle(host, strip);
                    stages.Add(new { stage = $"focus-first-{width}", snapshot = Snapshot(strip) });
                    setFocus.Invoke(strip, null);
                    Settle(host, strip);
                    stages.Add(new { stage = $"focus-second-{width}", snapshot = Snapshot(strip) });
                    WriteProgress(evidenceDirectory, stages);
                    strip.OverflowButton.DropDown.Close();
                    Settle(host, strip);
                }

                strip.Width = WideWidth;
                Settle(host, strip);
                RetainFrame(host, null, evidenceDirectory, "regrown");
                object metadata = new
                {
                    status = "native consumer observations only; draft is acceptance evidence only after an actual successful run",
                    parentPoints,
                    assignedOwnerFont,
                    rightToLeft,
                    dark,
                    settingsDirectory,
                    sourceAssembly = strip.GetType().Assembly.FullName,
                    sourceAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(strip.GetType().Assembly.Location))),
                    frameworkAssembly = typeof(ToolStrip).Assembly.FullName,
                    frameworkAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(ToolStrip).Assembly.Location))),
                    constructed,
                    preferredHeight,
                    closeReasons,
                    stages,
                    scope = "actual standalone original FilterToolBar; explicit owner constraints, not actual FormBrowse ToolStripPanel allocation",
                    limitations = "No CSS custom theme, physical pointer/keyboard, popup-list editing, group toggles, translated caption, pixel equality or higher-DPI claim. Selection/HFONT/height are recorded, not inferred from authored bounds or a blank raster.",
                };
                string json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Path.Combine(evidenceDirectory, "probe.json"), json);
                TestContext.Out.WriteLine($"filterOverflowEvidence={evidenceDirectory}");
                TestContext.Out.WriteLine(json);
            }
            finally
            {
                Application.ThreadException -= onThreadException;
                Application.SetColorMode(originalMode);
                Application.DoEvents();
            }
        });
    }

    private static ToolStripEx CreateOriginalFilter()
    {
        Type type = typeof(ToolStripEx).Assembly.GetType(FilterTypeName, throwOnError: true)
            ?? throw new TypeLoadException(FilterTypeName);
        return Activator.CreateInstance(type) as ToolStripEx
            ?? throw new InvalidOperationException("Construct the real internal source FilterToolBar, not a source-shaped surrogate.");
    }

    private static Font ReadNativeMenuFont()
        => typeof(ToolStripManager).GetProperty("DefaultFont", PrivateStatic | BindingFlags.Public)?.GetValue(null) as Font
            ?? throw new MissingMemberException(typeof(ToolStripManager).FullName, "DefaultFont");

    private static ToolStripItem[] ReadDisplayedItems(ToolStrip popup)
        => (typeof(ToolStrip).GetProperty("DisplayedItems", PrivateInstance)?.GetValue(popup) as ToolStripItemCollection
            ?? throw new MissingMemberException(typeof(ToolStrip).FullName, "DisplayedItems")).Cast<ToolStripItem>().ToArray();

    private static void Settle(Form host, ToolStrip strip, ToolStripDropDown? popup = null)
    {
        Stopwatch settlement = Stopwatch.StartNew();
        do
        {
            Application.DoEvents();
            Thread.Sleep(1);
        }
        while (settlement.ElapsedMilliseconds < 100);

        host.Refresh();
        strip.Refresh();
        host.Update();
        strip.Update();
        if (popup is { Visible: true })
        {
            popup.Refresh();
            popup.Update();
        }

        Application.DoEvents();
    }

    private static object Snapshot(ToolStrip strip) => new
    {
        type = strip.GetType().FullName,
        strip.Name,
        strip.Size,
        strip.ClientSize,
        strip.DisplayRectangle,
        strip.AutoSize,
        strip.Padding,
        strip.GripStyle,
        strip.GripMargin,
        gripEnabled = (strip as ToolStripEx)?.GripEnabled,
        strip.Visible,
        parentType = strip.Parent?.GetType().FullName,
        renderer = strip.Renderer.GetType().FullName,
        font = DescribeFont(strip.Font),
        preferred = strip.GetPreferredSize(Size.Empty),
        nativeMode = Application.ColorMode.ToString(),
        overflow = strip is ToolStripDropDown ? null : new { strip.OverflowButton.Bounds, strip.OverflowButton.Available, strip.OverflowButton.Visible },
        items = (strip is ToolStripDropDown ? ReadDisplayedItems(strip) : strip.Items.Cast<ToolStripItem>()).Select(item => new
        {
            item.Name,
            type = item.GetType().FullName,
            item.Available,
            item.Visible,
            item.Enabled,
            item.AutoSize,
            item.Bounds,
            item.Margin,
            item.Padding,
            item.IsOnDropDown,
            item.Placement,
            item.Text,
            preferred = item.GetPreferredSize(Size.Empty),
            owner = item.Owner?.Name,
            currentParent = item.GetCurrentParent()?.GetType().FullName,
            font = DescribeFont(item.Font),
            combo = item is ToolStripComboBox combo ? DescribeCombo(combo.ComboBox) : null,
        }).ToArray(),
    };

    private static object DescribeCombo(ComboBox combo) => new
    {
        parent = combo.Parent?.GetType().FullName,
        combo.Bounds,
        combo.ClientRectangle,
        combo.PreferredHeight,
        preferred = combo.GetPreferredSize(Size.Empty),
        combo.SelectionStart,
        combo.SelectionLength,
        combo.SelectedText,
        combo.DropDownWidth,
        combo.DropDownStyle,
        combo.FlatStyle,
        combo.Text,
        combo.Visible,
        combo.Focused,
        combo.ContainsFocus,
        combo.IsHandleCreated,
        font = DescribeFont(combo.Font),
        nativeFont = combo.IsHandleCreated ? ReadNativeFont(combo) : null,
    };

    private static object ReadNativeFont(ComboBox combo)
    {
        nint fontHandle = SendMessage(combo.Handle, GetFontMessage, 0, 0);
        fontHandle.Should().NotBe(0, "the real hosted ComboBox must expose its selected native HFONT");
        using Font nativeFont = Font.FromHfont(fontHandle);
        return new { hwnd = combo.Handle.ToString(), hfont = fontHandle.ToString(), font = DescribeFont(nativeFont) };
    }

    private static object DescribeFont(Font font)
        => new { font.Name, font.OriginalFontName, font.SizeInPoints, style = font.Style.ToString(), font.Height };

    private static void WriteProgress(string directory, IReadOnlyList<object> stages)
        => File.WriteAllText(Path.Combine(directory, "progress.json"), JsonSerializer.Serialize(new
        {
            status = "diagnostic, not completed-test evidence",
            stages,
        }, new JsonSerializerOptions { WriteIndented = true }));

    private static void RetainFrame(Form host, ToolStripDropDown? popup, string directory, string stage)
    {
        using CaptureImageResult capture = ImageCapture.Capture(host, popup is { Visible: true } ? [popup] : [], []);
        capture.Bitmap.Save(Path.Combine(directory, stage + ".png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(directory, stage + ".capture.json"), JsonSerializer.Serialize(new
        {
            stage,
            method = capture.Method.ToString(),
            capture.ScreenBounds,
            capture.PrimaryScreenBounds,
            hostDpi = host.DeviceDpi,
            popupDpi = popup?.DeviceDpi,
            dpiMode = "nativeMonitor",
            popupVisible = popup?.Visible,
            popupBounds = popup?.Bounds,
            note = "Unmodified real owner and owned popup retained independently of direct layout assertions; no renderer-pixel/blank-raster inference.",
        }, new JsonSerializerOptions { WriteIndented = true }));
        capture.Method.Should().Be(CaptureMethod.PrintWindow);
        if (popup is not null)
        {
            popup.Visible.Should().BeTrue();
            capture.ScreenBounds.Contains(popup.Bounds).Should().BeTrue();
        }
    }

    private static void WithIsolatedSettings(Action<string> action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.FilterOverflowProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string settingsPath = Path.Combine(directory, SettingsFileName);
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(directory);
        using GitExtSettingsCache cache = new(settingsPath, autoSave: false);
        DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
        TestContext.Out.WriteLine($"isolated original settings retained: {directory}");
        AppSettings.UsingContainer(settings, () => action(directory));
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
            // AppSettings selects its first cache before its public container-isolation
            // boundary exists. Match the existing native fixtures' temporary path alias.
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

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint wordParameter, nint longParameter);

    private sealed class NativePopupLifetime(Form host) : IDisposable
    {
        public ToolStripDropDown? Popup { get; set; }

        public void Dispose()
        {
            if (Popup is { IsDisposed: false } popup)
            {
                // The reason-taking overload alone does not exit native modal menu mode.
                popup.Close();
                Application.DoEvents();
            }

            host.Close();
            Application.DoEvents();
        }
    }
}
