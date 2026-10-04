using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.ParityCapture;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.ScriptsEngine;
using GitUI.Theming;
using NSubstitute;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// The source Browse is constructed but never shown: its real initialized toolbar/menu
// objects are temporarily hosted in a plain Form, without Browse.OnLoad or command clicks.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class WorkingDirPopupContractTests
{
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private const uint KeyDownMessage = 0x100;
    private const uint KeyUpMessage = 0x101;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static IEnumerable<TestCaseData> Cases()
    {
        foreach (string theme in new[] { "light", "dark", "parity-custom" })
        {
            foreach (int parentPoints in new[] { 9, 11 })
            {
                // Normal Browse leaves the owner's font unassigned. Assigning 11pt is
                // an explicit diagnostic consumer, not a normal-Browse font claim.
                bool[] assignments = parentPoints == 9 ? [false] : [false, true];
                foreach (bool assignedOwnerFont in assignments)
                {
                    foreach (bool rightToLeft in new[] { false, true })
                    {
                        yield return new TestCaseData(theme, parentPoints, assignedOwnerFont, rightToLeft);
                    }
                }
            }
        }
    }

    [TestCaseSource(nameof(Cases))]
    public void Actual_source_WorkingDir_should_record_popup_layout_renderer_and_owned_Escape(
        string theme, int parentPoints, bool assignedOwnerFont, bool rightToLeft)
    {
        // A source Debug assertion must become a captured fault, never a JIT chooser.
        Environment.GetEnvironmentVariable("GITEXTENSIONS_DEBUG_FAIL_FAST").Should().Be("1",
            "set the existing source fail-fast switch before starting this opt-in native probe");
        string workspace = FindWorkspaceRoot();
        string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
        AssertOutsideWorkspace(temporaryRoot, workspace);

        // Guard the restored process baseline as well as the active container. Source
        // FormBrowse registers a SessionEnding save callback; restoring a real-user cache
        // afterward would make that callback unsafe even though this probe never clicks.
        // Reuse the existing first-initialization alias and reject an already-initialized
        // non-temporary baseline before constructing any source FormBrowse instance.
        string bootstrapDirectory = Path.Combine(temporaryRoot, "GitExtensions.WorkingDirPopupBootstrap", Guid.NewGuid().ToString("N"));
        AssertExternalTemporaryRoot(bootstrapDirectory, workspace);
        Directory.CreateDirectory(bootstrapDirectory);
        File.WriteAllText(Path.Combine(bootstrapDirectory, "GitExtensions.settings"),
            "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InvokeExistingHelper("InitializeAppSettingsWithoutRealConfiguration", bootstrapDirectory);
        string baselineSettingsPath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath;
        AssertExternalTemporaryRoot(baselineSettingsPath, workspace);
        AppSettings.ApplicationDataPath.IsValueCreated.Should().BeTrue("the initialized source cache must already have resolved its data root");
        string applicationDataPath = AppSettings.ApplicationDataPath.Value
            ?? throw new InvalidOperationException("The source roaming data root must be isolated.");
        AssertExternalTemporaryRoot(applicationDataPath, workspace);
        string localApplicationDataPath = InitializeLocalCacheWithoutRealConfiguration(bootstrapDirectory, workspace);
        TestContext.Out.WriteLine($"workingDirPopupBootstrap={bootstrapDirectory};baselineSettings={baselineSettingsPath}");
        AssertExternalTemporaryRoot(Path.Combine(temporaryRoot, "GitExtensions.ToolStripOwnerFontProbe"), workspace);
        InvokeExistingHelper("WithIsolatedSettings", (Action<string>)(settingsDirectory =>
        {
            AssertExternalTemporaryRoot(settingsDirectory, workspace);
            string settingsPath = AppSettings.SettingsContainer.SettingsCache.SettingsFilePath;
            AssertContained(settingsPath, settingsDirectory);
            settingsPath.Should().Be(Path.Combine(settingsDirectory, "GitExtensions.settings"));

            // Actual RepositoryStorage uses the active isolated AppSettings container.
            // These empty temporary directories are listing inputs, not valid repositories;
            // no git init, commit, config mutation or repository command is requested here.
            string repositoryDirectory = Path.Combine(settingsDirectory, "repositories");
            string alphaPath = Path.Combine(repositoryDirectory, "alpha_work&tree");
            string betaPath = Path.Combine(repositoryDirectory, "beta&&amp");
            Directory.CreateDirectory(alphaPath);
            Directory.CreateDirectory(betaPath);
            AssertContained(alphaPath, settingsDirectory);
            AssertContained(betaPath, settingsDirectory);
            Repository alpha = new(alphaPath) { Category = "Probe favourites" };
            Repository beta = new(betaPath);
            RepositoryStorage storage = new();
            storage.Save("history", [alpha, beta]);
            storage.Save("history-favourite", [alpha]);
            storage.Load("history").Select(repository => repository.Path).Should().Equal(alphaPath, betaPath);

            string profilePath = Path.Combine(workspace, "eng", "tools", "WinFormsParityCapture",
                "Profiles", "reference.settings.json");
            CaptureSettingsProfile profile = CaptureSettingsProfile.Load(profilePath);
            foreach ((string key, string value) in profile.AppSettings)
            {
                AppSettings.SetString(key, value);
            }

            AppSettings.OwnScripts = string.Empty;
            AppSettings.CheckForUpdates = false;
            AppSettings.TelemetryEnabled = false;
            AppSettings.ShowConEmuTab.Value = false;
            AppSettings.CurrentTranslation = "English";
            using Font parentFont = new(profile.UiFontFamily, parentPoints, FontStyle.Regular, GraphicsUnit.Point);
            AppSettings.Font = parentFont;
            using SourceThemeScope themeScope = new(workspace, settingsDirectory, theme);
            string evidenceDirectory = Path.Combine(settingsDirectory, "WorkingDirPopupProbe");
            Directory.CreateDirectory(evidenceDirectory);
            TestContext.Out.WriteLine($"workingDirPopupDiagnosticEvidence={evidenceDirectory}");
            List<object> stages = [];
            List<object> renderEvents = [];
            List<object> lifetime = [];
            Exception? threadException = null;
            ThreadExceptionEventHandler exceptionHandler = (_, args) => threadException ??= args.Exception;
            Application.ThreadException += exceptionHandler;
            try
            {
                // Reuse the existing source settings/bootstrap boundaries instead of a
                // second parallel startup harness. They replace taskbar jump-list writes,
                // restore ThreadHelper/context and keep the source FormBrowse cold.
                InvokeExistingHelper("WithOriginalBrowse", alphaPath, (Action<FormBrowse>)(browse =>
                {
                    int browseLoads = 0;
                    browse.Load += (_, _) => browseLoads++;
                    browse.Visible.Should().BeFalse();
                    IScriptsManager sourceScripts = browse.UICommands.GetRequiredService<IScriptsManager>();
                    sourceScripts.GetScripts().Should().OnlyContain(script => !script.Enabled,
                        "the real source service must not retain an enabled operator script in this isolated graph");
                    ToolStripEx strip = ReadField<ToolStripEx>(browse, "ToolStripMain");
                    ToolStripSplitButton selector = ReadField<ToolStripSplitButton>(browse, "_NO_TRANSLATE_WorkingDir");
                    ToolStripDropDownItem start = ReadField<ToolStripDropDownItem>(browse, "fileToolStripMenuItem");
                    ToolStripMenuItem sourceFavourite = ReadField<ToolStripMenuItem>(start, "tsmiFavouriteRepositories");
                    ToolStripMenuItem sourceOpen = ReadField<ToolStripMenuItem>(start, "openToolStripMenuItem");
                    ToolStripMenuItem sourceClose = ReadField<ToolStripMenuItem>(browse, "closeToolStripMenuItem");
                    selector.GetType().FullName.Should().Be("GitUI.CommandsDialogs.Menus.WorkingDirectoryToolStripSplitButton");
                    Control sourceParent = strip.Parent ?? throw new InvalidOperationException("Retain the actual source owner.");
                    DockStyle sourceDock = strip.Dock;
                    Rectangle sourceBounds = strip.Bounds;
                    Font sourceOwnerFont = strip.Font;
                    RightToLeft sourceDirection = strip.RightToLeft;
                    using Form host = new()
                    {
                        AutoScaleMode = AutoScaleMode.None,
                        ClientSize = new Size(1400, 360),
                        Font = parentFont,
                        ShowInTaskbar = false,
                        StartPosition = FormStartPosition.CenterScreen,
                        Text = "Actual original WorkingDir popup consumer",
                    };
                    host.Load += (_, _) => host.FixVisualStyle();
                    ToolStripDropDown popup = selector.DropDown;
                    using RendererObserver observer = new(popup, renderEvents);
                    popup.Opening += (_, args) => lifetime.Add(new { stage = observer.Stage, kind = "opening", args.Cancel });
                    popup.Opened += (_, _) => lifetime.Add(new { stage = observer.Stage, kind = "opened" });
                    popup.Closing += (_, args) => lifetime.Add(new { stage = observer.Stage, kind = "closing", reason = args.CloseReason.ToString(), args.Cancel });
                    popup.Closed += (_, args) => lifetime.Add(new { stage = observer.Stage, kind = "closed", reason = args.CloseReason.ToString() });
                    try
                    {
                        strip.RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No;
                        if (assignedOwnerFont)
                        {
                            strip.Font = parentFont;
                        }

                        // This is an explicit plain-owner constraint. It is not the original
                        // multi-strip ToolStripPanel's complete allocation or Browse.OnLoad.
                        strip.Dock = DockStyle.Top;
                        host.Controls.Add(strip);
                        host.Show();
                        host.Activate();
                        Settle(host, popup, browse, () => threadException);
                        stages.Add(new
                        {
                            stage = "owner-activated",
                            host.Visible,
                            host.ContainsFocus,
                            activeForm = Form.ActiveForm?.GetType().FullName,
                            host.Bounds,
                            stripBounds = strip.Bounds,
                            ownerFont = DescribeFont(strip.Font),
                            browseVisible = browse.Visible,
                            browseLoads,
                        });
                        WriteDiagnostic(evidenceDirectory, stages, renderEvents, lifetime);
                        Form.ActiveForm.Should().BeSameAs(host);
                        host.DeviceDpi.Should().Be(96);
                        strip.DeviceDpi.Should().Be(96);
                        observer.Stage = "initial-open";
                        selector.ShowDropDown();
                        observer.Observe(popup.Renderer);
                        Settle(host, popup, browse, () => threadException);
                        popup.Visible.Should().BeTrue();
                        popup.DeviceDpi.Should().Be(96);
                        popup.OwnerItem.Should().BeSameAs(selector);
                        ToolStripTextBox filter = popup.Items.OfType<ToolStripTextBox>().Single();
                        TextBox actualTextBox = filter.TextBox;
                        ToolStripMenuItem favourites = popup.Items.OfType<ToolStripMenuItem>()
                            .Single(item => item.Text == sourceFavourite.Text);
                        ToolStripMenuItem open = popup.Items.OfType<ToolStripMenuItem>()
                            .Single(item => item.Text == sourceOpen.Text);
                        ToolStripMenuItem close = popup.Items.OfType<ToolStripMenuItem>()
                            .Single(item => item.Text == sourceClose.Text);
                        ToolStripMenuItem recentAlpha = popup.Items.OfType<ToolStripMenuItem>()
                            .Single(item => item.Text?.Contains("alpha", StringComparison.OrdinalIgnoreCase) is true);
                        ToolStripMenuItem recentBeta = popup.Items.OfType<ToolStripMenuItem>()
                            .Single(item => item.Text?.Contains("beta", StringComparison.OrdinalIgnoreCase) is true);
                        favourites.Image.Should().BeSameAs(sourceFavourite.Image);
                        open.Image.Should().BeSameAs(sourceOpen.Image);
                        close.Image.Should().BeSameAs(sourceClose.Image);
                        AssertImagePixels(sourceFavourite.Image, GitUI.Properties.Images.Star);
                        AssertImagePixels(sourceClose.Image, GitUI.Properties.Images.DashboardFolderGit);
                        RetainFrame(host, popup, evidenceDirectory, observer.Stage);
                        VerifyActualHostedSlot(popup, filter);
                        int nonHostedMaximum = popup.Items.Cast<ToolStripItem>()
                            .Where(item => item != filter).Max(item => item.Width);
                        filter.Width.Should().Be(nonHostedMaximum - GitExtUtils.GitUI.DpiUtil.Scale(60),
                            "the original FillDropDown authored algorithm sizes the same hosted filter after native item layout");
                        stages.Add(new { stage = observer.Stage, snapshot = Snapshot(host, strip, popup, filter) });
                        RetainFrame(host, popup, evidenceDirectory, observer.Stage);
                        observer.TextCount.Should().BeGreaterThan(0, "record actual native text callbacks, not a zero-event success");
                        observer.ImageCount.Should().BeGreaterThan(0, "the actual source popup paints its cloned images");
                        observer.ArrowCount.Should().BeGreaterThan(0, "the actual populated favourites submenu paints its arrow");
                        observer.BackgroundCount.Should().BeGreaterThan(0, "record the actual native popup surface callback");

                        // Route item-relative pointer movement through the actual popup.
                        // There is no command click or process-external OS cursor mutation.
                        observer.Stage = "owned-close-move";
                        DispatchOwnedPointerMove(popup, close.Bounds);
                        Settle(host, popup, browse, () => threadException);
                        stages.Add(new
                        {
                            stage = observer.Stage,
                            inputMode = "source protected ToolStrip.OnMouseMove; no physical-pointer claim",
                            snapshot = Snapshot(host, strip, popup, filter),
                        });
                        RetainFrame(host, popup, evidenceDirectory, observer.Stage);

                        // Selecting separately records the actual framework state, without
                        // conflating the source API with physical keyboard focus or input.
                        observer.Stage = "close-selected";
                        close.Select();
                        Settle(host, popup, browse, () => threadException);
                        stages.Add(new { stage = observer.Stage, snapshot = Snapshot(host, strip, popup, filter) });
                        RetainFrame(host, popup, evidenceDirectory, observer.Stage);

                        observer.Stage = "close-disabled-diagnostic";
                        bool closeEnabled = close.Enabled;
                        close.Enabled = false;
                        Settle(host, popup, browse, () => threadException);
                        stages.Add(new
                        {
                            stage = observer.Stage,
                            explicitlyDisabledDiagnostic = true,
                            snapshot = Snapshot(host, strip, popup, filter),
                        });
                        RetainFrame(host, popup, evidenceDirectory, observer.Stage);
                        close.Enabled = closeEnabled;

                        // Search uses source ToolStripItem.Text, before mnemonic parsing.
                        // Exercise real TextChanged filtering with literal underscores and ampersands.
                        observer.Stage = "raw-source-filter";
                        foreach ((string query, bool alphaVisible, bool betaVisible) in new[]
                        {
                            ("alpha_work", true, false), ("alpha_work&tree", true, false),
                            ("__", false, false), ("beta&&amp", false, true),
                        })
                        {
                            filter.Text = query;
                            Settle(host, popup, browse, () => threadException);
                            stages.Add(new
                            {
                                stage = observer.Stage,
                                query,
                                rawAlphaText = recentAlpha.Text,
                                rawBetaText = recentBeta.Text,
                                alphaVisible = recentAlpha.Available,
                                betaVisible = recentBeta.Available,
                            });
                            WriteDiagnostic(evidenceDirectory, stages, renderEvents, lifetime);
                            recentAlpha.Available.Should().Be(alphaVisible);
                            recentBeta.Available.Should().Be(betaVisible);
                        }

                        observer.Stage = "filter-focused";
                        bool focusAccepted = actualTextBox.Focus();
                        focusAccepted.Should().BeTrue("owned Escape must target the actually focused native input");
                        actualTextBox.Text = "beta";
                        Settle(host, popup, browse, () => threadException);
                        actualTextBox.Focused.Should().BeTrue();
                        filter.TextBox.Should().BeSameAs(actualTextBox);
                        recentAlpha.Available.Should().BeFalse();
                        recentBeta.Available.Should().BeTrue();
                        favourites.Available.Should().BeTrue();
                        open.Available.Should().BeTrue();
                        close.Available.Should().BeTrue();
                        VerifyActualHostedSlot(popup, filter);
                        stages.Add(new { stage = observer.Stage, focusAccepted, snapshot = Snapshot(host, strip, popup, filter) });
                        RetainFrame(host, popup, evidenceDirectory, observer.Stage);

                        observer.Stage = "owned-Escape";
                        object escapeRoute = DispatchOwnedEscape(actualTextBox);
                        Settle(host, popup, browse, () => threadException);

                        stages.Add(new { stage = observer.Stage, escapeRoute, snapshot = Snapshot(host, strip, popup, filter) });
                        WriteDiagnostic(evidenceDirectory, stages, renderEvents, lifetime);
                        if (popup.Visible)
                        {
                            RetainFrame(host, popup, evidenceDirectory, observer.Stage);
                        }

                        // The completed native consumer matrix establishes this contract,
                        // independently of the candidate's former clear-and-stay-open handler.
                        popup.Visible.Should().BeFalse("the source preprocesses Escape by closing its owned popup");
                        filter.Text.Should().Be("beta", "Escape retains the input until the next source FillDropDown");

                        popup.Close();
                        Settle(host, popup, browse, () => threadException);
                        observer.Stage = "reopened";
                        selector.ShowDropDown();
                        observer.Observe(popup.Renderer);
                        Settle(host, popup, browse, () => threadException);
                        popup.Visible.Should().BeTrue();
                        popup.Items.OfType<ToolStripTextBox>().Single().Should().BeSameAs(filter);
                        filter.TextBox.Should().BeSameAs(actualTextBox);
                        filter.Text.Should().BeEmpty("source FillDropDown clears its retained input only while rebuilding a closed popup");
                        VerifyActualHostedSlot(popup, filter);
                        stages.Add(new { stage = observer.Stage, snapshot = Snapshot(host, strip, popup, filter) });
                        RetainFrame(host, popup, evidenceDirectory, observer.Stage);
                        browseLoads.Should().Be(0);
                        string metadata = JsonSerializer.Serialize(new
                        {
                            status = "native consumer; acceptance requires the corresponding completed passing receipt",
                            theme,
                            parentPoints,
                            assignedOwnerFont,
                            rightToLeft,
                            settingsDirectory,
                            settingsPath,
                            baselineSettingsPath,
                            bootstrapDirectory,
                            applicationDataPath,
                            localApplicationDataPath,
                            avatarCachePath = AppSettings.AvatarImageCachePath,
                            sourceRepositoryDirectory = repositoryDirectory,
                            repositoryInputs = new[] { alphaPath, betaPath },
                            repositoryKind = "isolated empty listing directories, not valid Git repositories; no fixture-requested repository mutations or item actions",
                            profilePath,
                            profileSha256 = HashFile(profilePath),
                            sourceAssembly = typeof(FormBrowse).Assembly.FullName,
                            sourceAssemblySha256 = HashFile(typeof(FormBrowse).Assembly.Location),
                            frameworkAssembly = typeof(ToolStrip).Assembly.FullName,
                            frameworkAssemblySha256 = HashFile(typeof(ToolStrip).Assembly.Location),
                            themeScope.Provenance,
                            themeScope.SourceHashes,
                            originalMenuImages = new
                            {
                                favourite = DescribeImage(sourceFavourite.Image),
                                open = DescribeImage(sourceOpen.Image),
                                close = DescribeImage(sourceClose.Image),
                            },
                            browseLoads,
                            jumpListBoundary = "existing WithOriginalBrowse replaces IWindowsJumpListManager before source construction",
                            scriptsBoundary = "isolated OwnScripts empty; original Browse never shown/loaded; no actions invoked",
                            sourceEnabledScriptCount = sourceScripts.GetScripts().Count(script => script.Enabled),
                            stages,
                            renderEvents,
                            callbackCounts = new { observer.TextCount, observer.ImageCount, observer.ArrowCount, observer.BackgroundCount },
                            lifetime,
                            limitations = "Actual initialized original WorkingDir/Start/Close menus in a plain host, not full loaded Browse. Owned message filtering/preprocessing is not physical keyboard/IME/modifier proof. Empty paths do not prove repository opening/branch-cache behavior. ThemeLoader/ThemeFix source route is exercised, not ThemeModule.Load persistence. No pixel equality, hover input, high-contrast, deactivation, higher-DPI or platform equivalence claim.",
                        }, JsonOptions);
                        File.WriteAllText(Path.Combine(evidenceDirectory, "probe.json"), metadata);
                        TestContext.Out.WriteLine($"workingDirPopupEvidence={evidenceDirectory}");
                    }
                    finally
                    {
                        // Parameterless Close exits native modal menu mode before destroying
                        // the owner. Restore the source-owned strip before the plain host
                        // disposes its controls; the original Browse retains disposal ownership.
                        popup.Close();
                        Application.DoEvents();
                        host.Controls.Remove(strip);
                        sourceParent.Controls.Add(strip);
                        strip.Dock = sourceDock;
                        strip.Bounds = sourceBounds;
                        strip.Font = sourceOwnerFont;
                        strip.RightToLeft = sourceDirection;
                        host.Close();
                        Application.DoEvents();
                    }
                }));
            }
            finally
            {
                WriteDiagnostic(evidenceDirectory, stages, renderEvents, lifetime);
                Application.ThreadException -= exceptionHandler;
            }
        }));
    }

    private static void VerifyActualHostedSlot(ToolStripDropDown popup, ToolStripTextBox filter)
    {
        filter.Owner.Should().BeSameAs(popup);
        filter.GetCurrentParent().Should().BeSameAs(popup);
        filter.TextBox.Parent.Should().BeSameAs(popup);

        // Bounds includes the native TextBox border; PointToScreen(Point.Empty)
        // starts at its client origin and therefore is not the hosted outer origin.
        filter.TextBox.Bounds.Location.Should().Be(filter.Bounds.Location);
        filter.TextBox.Size.Should().Be(filter.Bounds.Size);
    }

    private static object DispatchOwnedEscape(TextBox input)
    {
        nint handle = input.Handle;
        uint scanCode = MapVirtualKey((uint)Keys.Escape, 0);
        nint keyDownBits = (nint)(1u | (scanCode << 16));
        Message message = Message.Create(handle, (int)KeyDownMessage, (nint)Keys.Escape, keyDownBits);
        bool focusedBefore = input.Focused;
        bool filtered = Application.FilterMessage(ref message);
        bool preprocessed = !filtered && input.PreProcessMessage(ref message);
        if (!filtered && !preprocessed)
        {
            SendMessage(handle, KeyDownMessage, (nint)Keys.Escape, keyDownBits);
        }

        SendMessage(handle, KeyUpMessage, (nint)Keys.Escape, (nint)(0xC0000001u | (scanCode << 16)));
        return new
        {
            mode = "Application.FilterMessage then actual TextBox.PreProcessMessage; owned HWND dispatch only if unhandled",
            focusedBefore,
            filtered,
            preprocessed,
            window = handle.ToString(),
            physicalKeyboard = false,
            globalKeyboardStateChanged = false,
        };
    }

    private static void DispatchOwnedPointerMove(ToolStripDropDown popup, Rectangle itemBounds)
    {
        Point position = new(itemBounds.X + (itemBounds.Width / 2), itemBounds.Y + (itemBounds.Height / 2));
        (typeof(ToolStrip).GetMethod("OnMouseMove", PrivateInstance)
            ?? throw new MissingMethodException(typeof(ToolStrip).FullName, "OnMouseMove"))
            .Invoke(popup, [new MouseEventArgs(MouseButtons.None, 0, position.X, position.Y, 0)]);
    }

    private static object Snapshot(Form host, ToolStrip strip, ToolStripDropDown popup, ToolStripTextBox filter) => new
    {
        owner = new { host.Visible, host.ContainsFocus, font = DescribeFont(host.Font), host.DeviceDpi },
        strip = new { strip.Name, strip.Bounds, font = DescribeFont(strip.Font), rightToLeft = strip.RightToLeft.ToString() },
        popup = new
        {
            type = popup.GetType().FullName,
            popup.Visible,
            popup.Bounds,
            popup.ClientRectangle,
            popup.DisplayRectangle,
            popup.Padding,
            popup.Margin,
            preferred = popup.GetPreferredSize(Size.Empty),
            font = DescribeFont(popup.Font),
            rightToLeft = popup.RightToLeft.ToString(),
            renderer = popup.Renderer.GetType().FullName,
            renderMode = popup.RenderMode.ToString(),
            layoutEngine = popup.LayoutEngine.GetType().FullName,
            layoutStyle = popup.LayoutStyle.ToString(),
            popup.ImageScalingSize,
            showImageMargin = (popup as ToolStripDropDownMenu)?.ShowImageMargin,
            showCheckMargin = (popup as ToolStripDropDownMenu)?.ShowCheckMargin,
            popup.AutoSize,
            popup.AutoClose,
            ownerItemName = popup.OwnerItem?.Name,
            foreground = Argb(popup.ForeColor),
            background = Argb(popup.BackColor),
            professionalColors = popup.Renderer is ToolStripProfessionalRenderer professional
                ? new
                {
                background = Argb(professional.ColorTable.ToolStripDropDownBackground),
                border = Argb(professional.ColorTable.MenuBorder),
                imageBegin = Argb(professional.ColorTable.ImageMarginGradientBegin),
                imageMiddle = Argb(professional.ColorTable.ImageMarginGradientMiddle),
                imageEnd = Argb(professional.ColorTable.ImageMarginGradientEnd),
                selected = Argb(professional.ColorTable.MenuItemSelected),
                selectedBorder = Argb(professional.ColorTable.MenuItemBorder),
                professional.ColorTable.UseSystemColors,
                professional.RoundedEdges,
                }
                : null,
            displayedItems = ReadDisplayedItems(popup).Select(item => new
            {
                index = popup.Items.IndexOf(item),
                type = item.GetType().FullName,
                item.Text,
                item.Bounds,
            }).ToArray(),
            items = popup.Items.Cast<ToolStripItem>().Select(item => new
            {
                type = item.GetType().FullName,
                item.Name,
                item.Text,
                item.Available,
                item.Visible,
                item.Enabled,
                item.Selected,
                item.Bounds,
                item.ContentRectangle,
                item.Padding,
                item.Margin,
                item.AutoSize,
                preferred = item.GetPreferredSize(Size.Empty),
                font = DescribeFont(item.Font),
                image = DescribeImage(item.Image),
                imageScaling = item.ImageScaling.ToString(),
                shortcut = (item as ToolStripMenuItem)?.ShortcutKeyDisplayString,
                ownerIsPopup = ReferenceEquals(item.Owner, popup),
                currentParentIsPopup = ReferenceEquals(item.GetCurrentParent(), popup),
            }).ToArray(),
        },
        filter = new
        {
            filter.Text,
            filter.TextBox.Focused,
            filter.TextBox.ContainsFocus,
            filter.TextBox.Bounds,
            filter.TextBox.ClientRectangle,
            font = DescribeFont(filter.TextBox.Font),
            borderStyle = filter.TextBox.BorderStyle.ToString(),
            filter.TextBox.SelectionStart,
            filter.TextBox.SelectionLength,
            currentParent = filter.GetCurrentParent()?.GetType().FullName,
            actualPopupOrigin = popup.PointToClient(filter.TextBox.PointToScreen(Point.Empty)),
        },
    };

    private static void Settle(Form host, ToolStripDropDown popup, FormBrowse browse, Func<Exception?> exception)
    {
        Stopwatch settlement = Stopwatch.StartNew();
        do
        {
            Application.DoEvents();
            Thread.Sleep(1);
        }
        while (settlement.ElapsedMilliseconds < 100);

        host.Refresh();
        host.Update();
        if (popup.Visible)
        {
            popup.PerformLayout();
            popup.Refresh();
            popup.Update();
        }

        Application.DoEvents();
        browse.Visible.Should().BeFalse("the source load route must remain uninvoked");
        Application.OpenForms.Cast<Form>().Where(form => form.Visible)
            .Should().OnlyContain(form => ReferenceEquals(form, host), "no script/error/picker dialog belongs to this consumer probe");
        if (exception() is Exception fault)
        {
            throw new InvalidOperationException("The actual original consumer raised a UI-thread exception.", fault);
        }
    }

    private static void RetainFrame(Form host, ToolStripDropDown popup, string directory, string stage)
    {
        popup.Visible.Should().BeTrue();
        using Bitmap client = new(popup.ClientSize.Width, popup.ClientSize.Height, PixelFormat.Format32bppArgb);
        client.SetResolution(96, 96);
        popup.DrawToBitmap(client, popup.ClientRectangle);
        client.Save(Path.Combine(directory, stage + ".popup-client.png"), ImageFormat.Png);
        using CaptureImageResult capture = ImageCapture.Capture(host, [popup], []);
        capture.Bitmap.Save(Path.Combine(directory, stage + ".owned-frame.png"), ImageFormat.Png);
        File.WriteAllText(Path.Combine(directory, stage + ".capture.json"), JsonSerializer.Serialize(new
        {
            stage,
            method = capture.Method.ToString(),
            capture.ScreenBounds,
            capture.PrimaryScreenBounds,
            popup.Bounds,
            hostDpi = host.DeviceDpi,
            popupDpi = popup.DeviceDpi,
            note = "Unmodified actual native popup client and owned frame, not an expected image or pixel-equivalence assertion.",
        }, JsonOptions));
        capture.Method.Should().Be(CaptureMethod.PrintWindow);
        capture.ScreenBounds.Contains(popup.Bounds).Should().BeTrue();
    }

    private static void WriteDiagnostic(string directory, List<object> stages, List<object> renderEvents, List<object> lifetime)
        => File.WriteAllText(Path.Combine(directory, "diagnostic.json"), JsonSerializer.Serialize(new
        {
            status = "diagnostic, not completed-test evidence",
            stages,
            renderEvents,
            lifetime,
        }, JsonOptions));

    private static T ReadField<T>(object owner, string name)
        => owner.GetType().GetField(name, PrivateInstance)?.GetValue(owner) is T value
            ? value
            : throw new MissingFieldException(owner.GetType().FullName, name);

    private static ToolStripItem[] ReadDisplayedItems(ToolStrip owner)
        => (typeof(ToolStrip).GetProperty("DisplayedItems", PrivateInstance)?.GetValue(owner) as ToolStripItemCollection
            ?? throw new MissingMemberException(typeof(ToolStrip).FullName, "DisplayedItems")).Cast<ToolStripItem>().ToArray();

    private static void InvokeExistingHelper(string name, params object[] arguments)
    {
        MethodInfo method = typeof(ToolStripOwnerOverflowTests).GetMethod(name, PrivateStatic)
            ?? throw new MissingMethodException(typeof(ToolStripOwnerOverflowTests).FullName, name);
        try
        {
            method.Invoke(null, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static string InitializeLocalCacheWithoutRealConfiguration(string bootstrapDirectory, string workspace)
    {
        // Cold RevisionGrid construction initializes AvatarService, whose persistent
        // cache creates Images even though Browse is never shown. The source's readonly
        // LocalApplicationDataPath lazy must resolve under the same portable alias as
        // initial settings, before a source control can touch real-user LocalAppData.
        // Do not replace the readonly lazy or redirect an already-initialized user path.
        if (!AppSettings.LocalApplicationDataPath.IsValueCreated)
        {
            Type settingsType = typeof(AppSettings).Assembly.GetType("GitCommands.Properties.Settings", throwOnError: true)!;
            ApplicationSettingsBase configuration = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ApplicationSettingsBase
                ?? throw new MissingMemberException(settingsType.FullName, "Default");
            _ = configuration["IsPortable"];
            SettingsPropertyValue portable = configuration.PropertyValues["IsPortable"]
                ?? throw new MissingMemberException(settingsType.FullName, "IsPortable");
            object originalPortableValue = portable.PropertyValue;
            bool originalDirty = portable.IsDirty;
            FieldInfo sourcePath = typeof(AppSettings).GetField("_applicationExecutablePath", PrivateStatic)
                ?? throw new MissingFieldException(typeof(AppSettings).FullName, "_applicationExecutablePath");
            object? originalSourcePath = sourcePath.GetValue(null);
            try
            {
                portable.PropertyValue = true;
                sourcePath.SetValue(null, Path.Combine(bootstrapDirectory, "GitExtensions.exe"));
                AppSettings.LocalApplicationDataPath.Value.Should().Be(bootstrapDirectory);
            }
            finally
            {
                sourcePath.SetValue(null, originalSourcePath);
                portable.PropertyValue = originalPortableValue;
                portable.IsDirty = originalDirty;
            }
        }

        string localPath = AppSettings.LocalApplicationDataPath.Value
            ?? throw new InvalidOperationException("The native avatar cache requires an isolated local data root.");
        AssertExternalTemporaryRoot(localPath, workspace);
        return localPath;
    }

    private static string FindWorkspaceRoot()
    {
        for (DirectoryInfo? current = new(TestContext.CurrentContext.TestDirectory); current is not null; current = current.Parent)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "src", "app", "GitUI"))
                && File.Exists(Path.Combine(current.FullName, "GitExtensions.slnx")))
            {
                return current.FullName;
            }
        }

        throw new DirectoryNotFoundException("Locate the source checkout read-only before entering a native fixture.");
    }

    private static void AssertExternalTemporaryRoot(string path, string workspace)
    {
        AssertContained(path, Path.GetTempPath());
        string fullPath = Path.GetFullPath(path);
        AssertOutsideWorkspace(fullPath, workspace);
        for (string? current = fullPath; current is not null; current = Path.GetDirectoryName(current))
        {
            if (Directory.Exists(current) || File.Exists(current))
            {
                File.GetAttributes(current).HasFlag(FileAttributes.ReparsePoint).Should().BeFalse(
                    "temporary outputs must not traverse a pre-existing junction or symbolic link");
            }
        }
    }

    private static void AssertImagePixels(Image? actual, Image expected)
    {
        actual.Should().NotBeNull();
        actual!.Size.Should().Be(expected.Size);
        using Bitmap actualPixels = new(actual);
        using Bitmap expectedPixels = new(expected);
        for (int y = 0; y < expected.Height; y++)
        {
            for (int x = 0; x < expected.Width; x++)
            {
                actualPixels.GetPixel(x, y).ToArgb().Should().Be(expectedPixels.GetPixel(x, y).ToArgb(),
                    "the source resource may return distinct bitmap objects, but the resolved asset pixels must match at {0},{1}", x, y);
            }
        }
    }

    private static void AssertOutsideWorkspace(string path, string workspace)
    {
        string fullPath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        string workspacePath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(workspace));
        string fullWorkspace = Path.TrimEndingDirectorySeparator(Path.GetFullPath(workspace)) + Path.DirectorySeparatorChar;
        fullPath.Equals(workspacePath, StringComparison.OrdinalIgnoreCase).Should().BeFalse();
        fullPath.StartsWith(fullWorkspace, StringComparison.OrdinalIgnoreCase).Should().BeFalse();
    }

    private static void AssertContained(string path, string root)
    {
        string fullRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        Path.GetFullPath(path).StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    private static object DescribeFont(Font font) => new { font.Name, font.OriginalFontName, font.SizeInPoints, style = font.Style.ToString(), font.Height };

    private static string Argb(Color color) => $"#{color.ToArgb():X8}";

    private static string HashFile(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static object? DescribeImage(Image? image)
    {
        if (image is null)
        {
            return null;
        }

        using Bitmap bitmap = new(image);
        using MemoryStream bytes = new();
        bitmap.Save(bytes, ImageFormat.Png);
        return new { image.Width, image.Height, pngSha256 = Convert.ToHexString(SHA256.HashData(bytes.ToArray())) };
    }

    private sealed class RendererObserver : IDisposable
    {
        private readonly ToolStripDropDown _popup;
        private readonly List<object> _events;
        private ToolStripRenderer? _renderer;

        public RendererObserver(ToolStripDropDown popup, List<object> events)
        {
            _popup = popup;
            _events = events;
            Observe(popup.Renderer);
        }

        public string Stage { get; set; } = "constructed";

        public int TextCount { get; private set; }

        public int ImageCount { get; private set; }

        public int ArrowCount { get; private set; }

        public int BackgroundCount { get; private set; }

        public void Observe(ToolStripRenderer renderer)
        {
            if (ReferenceEquals(_renderer, renderer))
            {
                return;
            }

            Dispose();
            _renderer = renderer;
            renderer.RenderItemText += OnText;
            renderer.RenderItemImage += OnImage;
            renderer.RenderArrow += OnArrow;
            renderer.RenderToolStripBackground += OnBackground;
            renderer.RenderImageMargin += OnImageMargin;
            renderer.RenderToolStripBorder += OnBorder;
            renderer.RenderMenuItemBackground += OnItemBackground;
            renderer.RenderSeparator += OnSeparator;
        }

        public void Dispose()
        {
            if (_renderer is null)
            {
                return;
            }

            _renderer.RenderItemText -= OnText;
            _renderer.RenderItemImage -= OnImage;
            _renderer.RenderArrow -= OnArrow;
            _renderer.RenderToolStripBackground -= OnBackground;
            _renderer.RenderImageMargin -= OnImageMargin;
            _renderer.RenderToolStripBorder -= OnBorder;
            _renderer.RenderMenuItemBackground -= OnItemBackground;
            _renderer.RenderSeparator -= OnSeparator;
            _renderer = null;
        }

        private void OnText(object? sender, ToolStripItemTextRenderEventArgs args)
        {
            if (!IsPopupItem(args.Item))
            {
                return;
            }

            TextCount++;
            _events.Add(new { stage = Stage, kind = "text", currentParent = DescribeOwner(args.Item?.GetCurrentParent()), itemOwnerIsPopup = ReferenceEquals(args.Item?.Owner, _popup), itemText = args.Item?.Text, args.Text, args.TextRectangle, flags = args.TextFormat.ToString(), font = args.TextFont is Font font ? DescribeFont(font) : null, color = Argb(args.TextColor) });
        }

        private void OnImage(object? sender, ToolStripItemImageRenderEventArgs args)
        {
            if (!IsPopupItem(args.Item))
            {
                return;
            }

            ImageCount++;
            _events.Add(new { stage = Stage, kind = "image", currentParent = DescribeOwner(args.Item?.GetCurrentParent()), itemOwnerIsPopup = ReferenceEquals(args.Item?.Owner, _popup), args.Item?.Text, args.ImageRectangle, image = DescribeImage(args.Image) });
        }

        private void OnArrow(object? sender, ToolStripArrowRenderEventArgs args)
        {
            if (!IsPopupItem(args.Item))
            {
                return;
            }

            ArrowCount++;
            _events.Add(new { stage = Stage, kind = "arrow", currentParent = DescribeOwner(args.Item?.GetCurrentParent()), itemOwnerIsPopup = ReferenceEquals(args.Item?.Owner, _popup), args.Item?.Text, args.ArrowRectangle, direction = args.Direction.ToString(), color = Argb(args.ArrowColor) });
        }

        private void OnBackground(object? sender, ToolStripRenderEventArgs args)
        {
            if (!ReferenceEquals(args.ToolStrip, _popup))
            {
                return;
            }

            BackgroundCount++;
            AddSurface("background", args);
        }

        private void OnImageMargin(object? sender, ToolStripRenderEventArgs args) => AddSurface("image-margin", args);

        private void OnBorder(object? sender, ToolStripRenderEventArgs args) => AddSurface("border", args);

        private void AddSurface(string kind, ToolStripRenderEventArgs args)
        {
            if (!ReferenceEquals(args.ToolStrip, _popup))
            {
                return;
            }

            _events.Add(new { stage = Stage, kind, owner = DescribeOwner(args.ToolStrip), args.AffectedBounds, background = Argb(args.BackColor) });
        }

        private void OnItemBackground(object? sender, ToolStripItemRenderEventArgs args)
        {
            if (!IsPopupItem(args.Item))
            {
                return;
            }

            _events.Add(new { stage = Stage, kind = "item-background", currentParent = DescribeOwner(args.Item?.GetCurrentParent()), itemOwnerIsPopup = ReferenceEquals(args.Item?.Owner, _popup), args.Item?.Text, args.Item?.Bounds, args.Item?.ContentRectangle, args.Item?.Selected, args.Item?.Enabled });
        }

        private void OnSeparator(object? sender, ToolStripSeparatorRenderEventArgs args)
        {
            if (!IsPopupItem(args.Item))
            {
                return;
            }

            _events.Add(new { stage = Stage, kind = "separator", currentParent = DescribeOwner(args.Item?.GetCurrentParent()), itemOwnerIsPopup = ReferenceEquals(args.Item?.Owner, _popup), args.Vertical, args.Item?.Bounds, args.Item?.ContentRectangle });
        }

        // ManagerRenderMode can share one renderer with the toolbar. Its callbacks
        // are popup evidence only when the actual current parent/surface is this popup.
        private bool IsPopupItem(ToolStripItem? item) => ReferenceEquals(item?.GetCurrentParent(), _popup);

        private object? DescribeOwner(ToolStrip? owner) => owner is null ? null : new
        {
            type = owner.GetType().FullName,
            owner.Name,
            nativeHandle = owner.IsHandleCreated ? owner.Handle.ToString() : null,
            isActualPopup = ReferenceEquals(owner, _popup),
        };
    }

    private sealed class SourceThemeScope : IDisposable
    {
        private readonly SystemColorMode _originalMode = Application.ColorMode;
        private readonly ThemeSettings _originalModuleSettings = ThemeModule.Settings;
        private readonly ThemeSettings _originalFixSettings = ReadSettings(typeof(ThemeFix));
        private readonly ThemeSettings _originalColorSettings = ReadSettings(typeof(ColorHelper));

        public SourceThemeScope(string workspace, string isolationRoot, string themeId)
        {
            string themes = Path.Combine(isolationRoot, "Themes");
            Directory.CreateDirectory(themes);
            string[] files = ["invariant.css", "dark.css", "parity-custom.css"];
            foreach (string file in files)
            {
                string source = file == "parity-custom.css"
                    ? Path.Combine(workspace, "eng", "tools", "WinFormsParityCapture", "Themes", file)
                    : Path.Combine(workspace, "src", "app", "GitUI", "Themes", file);
                string target = Path.Combine(themes, file);
                AssertContained(target, isolationRoot);
                File.Copy(source, target, overwrite: false);
            }

            IThemeCssUrlResolver resolver = Substitute.For<IThemeCssUrlResolver>();
            resolver.ResolveCssUrl(Arg.Any<string>()).Returns(call =>
            {
                string target = Path.Combine(themes, call.Arg<string>());
                AssertContained(target, themes);
                File.Exists(target).Should().BeTrue();
                return target;
            });
            ThemeLoader loader = new(resolver, new ThemeFileReader());
            Theme invariant = loader.LoadTheme(Path.Combine(themes, "invariant.css"), ThemeId.DefaultLight, []);
            Theme selected = themeId == "light" ? invariant
                : loader.LoadTheme(Path.Combine(themes, themeId + ".css"), new ThemeId(themeId, isBuiltin: true), []);
            ThemeSettings settings = new(selected, invariant, [], useSystemVisualStyle: themeId == "light");
            try
            {
                SetModuleSettings(settings);
                ThemeFix.ThemeSettings = settings;
                ColorHelper.ThemeSettings = settings;
                Application.SetColorMode(selected.SystemColorMode);
                Application.DoEvents();
                AppSettings.ThemeId = selected.Id;
                AppSettings.ThemeVariations = [];
                AppSettings.UseSystemVisualStyle = settings.UseSystemVisualStyle;
                SourceHashes = files.ToDictionary(file => file, file => HashFile(Path.Combine(themes, file)));
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public string Provenance => "original ThemeLoader, source CSS copied to guarded temporary paths, ThemeSettings/ThemeFix/ColorHelper; no ThemeModule.Load or user theme path";

        public IReadOnlyDictionary<string, string> SourceHashes { get; }

        public void Dispose()
        {
            SetModuleSettings(_originalModuleSettings);
            ThemeFix.ThemeSettings = _originalFixSettings;
            ColorHelper.ThemeSettings = _originalColorSettings;
            Application.SetColorMode(_originalMode);
            Application.DoEvents();
        }

        private static ThemeSettings ReadSettings(Type owner)
            => owner.GetProperty(nameof(ThemeFix.ThemeSettings), BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ThemeSettings
                ?? throw new MissingMemberException(owner.FullName, nameof(ThemeFix.ThemeSettings));

        private static void SetModuleSettings(ThemeSettings settings)
            => (typeof(ThemeModule).GetProperty(nameof(ThemeModule.Settings), BindingFlags.Public | BindingFlags.Static)?.GetSetMethod(nonPublic: true)
                ?? throw new MissingMemberException(typeof(ThemeModule).FullName, nameof(ThemeModule.Settings))).Invoke(null, [settings]);
    }

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint wordParameter, nint longParameter);

    [DllImport("user32.dll", EntryPoint = "MapVirtualKeyW")]
    private static extern uint MapVirtualKey(uint code, uint mapType);
}
