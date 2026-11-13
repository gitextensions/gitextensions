using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// The original selector is initialized once, using the established temporary-settings
// bootstrap. Its native rows and dispatch are real; history callbacks only observe.
// RepositoryHistoryUIService.OpenRepo has no injectable LaunchBrowse boundary, so these
// observations do not prove process launch, repository validation, or module replacement.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class WorkingDirPopupInputContractTests
{
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const uint MouseMoveMessage = 0x200;
    private const uint LeftButtonDownMessage = 0x201;
    private const uint LeftButtonUpMessage = 0x202;
    private const uint KeyDownMessage = 0x100;
    private const uint KeyUpMessage = 0x101;
    private const uint SystemCharacterMessage = 0x106;
    private const int ModifierAltContextBit = 1 << 29;
    private const string SelectorTypeName = "GitUI.CommandsDialogs.Menus.WorkingDirectoryToolStripSplitButton";
    private const string StartTypeName = "GitUI.CommandsDialogs.Menus.StartToolStripMenuItem";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [TestCase(Keys.None, Keys.None)]
    [TestCase(Keys.Control, Keys.Control)]
    [TestCase(Keys.None, Keys.Control)]
    [TestCase(Keys.Control, Keys.None)]
    [TestCase(Keys.Control, Keys.Control | Keys.Shift)]
    [TestCase(Keys.Control | Keys.Shift, Keys.Control)]
    [TestCase(Keys.Control, Keys.Control | Keys.Alt)]
    [TestCase(Keys.Control | Keys.Alt, Keys.Control)]
    [TestCase(Keys.Control | Keys.Shift, Keys.Control | Keys.Alt)]
    [TestCase(Keys.Control | Keys.Alt, Keys.Control | Keys.Shift)]
    public void Actual_source_WorkingDir_repository_Click_should_observe_release_time_thread_modifiers(
        Keys downModifiers, Keys upModifiers)
    {
        WithFixture([new RowDefinition("recent", "1: &Jasper_worktree")], fixture =>
        {
            using KeyboardStateScope keyboard = new();
            keyboard.Set(Keys.None);
            fixture.Open();
            ToolStripMenuItem row = fixture.Row("recent");
            Point position = Center(row.Bounds);
            fixture.Popup.GetItemAt(position).Should().BeSameAs(row);
            keyboard.Set(downModifiers);
            fixture.DispatchPointer(MouseMoveMessage, position, downModifiers, leftButton: false);
            keyboard.Set(downModifiers, leftButton: true);
            fixture.DispatchPointer(LeftButtonDownMessage, position, downModifiers, leftButton: true);
            fixture.Record("after-owned-WM_LBUTTONDOWN", new { downModifiers = downModifiers.ToString() });
            fixture.Clicks.Should().BeEmpty("a native repository leaf activates on release, not on the press observation");
            fixture.Popup.Visible.Should().BeTrue();

            keyboard.Set(upModifiers);
            fixture.DispatchPointer(LeftButtonUpMessage, position, upModifiers, leftButton: false);
            fixture.Record("after-owned-WM_LBUTTONUP", new { upModifiers = upModifiers.ToString() });
            fixture.Clicks.Should().ContainSingle();
            ClickObservation click = fixture.Clicks.Single();
            click.Row.Should().Be("recent");
            click.Modifiers.Should().Be((int)upModifiers,
                "the real native Click callback reads current Control.ModifierKeys, not a mouse-down snapshot");
            click.Control.Should().Be((upModifiers & Keys.Control) != 0);
            click.Shift.Should().Be((upModifiers & Keys.Shift) != 0);
            click.Alt.Should().Be((upModifiers & Keys.Alt) != 0);
            click.SourceOpenRepoPredicateWouldLaunch.Should().Be(upModifiers == Keys.Control);
            fixture.AssertSinglePopulation();
            fixture.Complete();
        });
    }

    [TestCase(Keys.None)]
    [TestCase(Keys.Control)]
    [TestCase(Keys.Shift)]
    [TestCase(Keys.Control | Keys.Shift)]
    [TestCase(Keys.Control | Keys.Alt)]
    public void Actual_source_WorkingDir_selected_repository_Enter_should_record_native_activation_and_live_modifiers(Keys modifiers)
    {
        WithFixture([new RowDefinition("recent", "1: &Jasper_worktree")], fixture =>
        {
            using KeyboardStateScope keyboard = new();
            keyboard.Set(Keys.None);
            fixture.Open();
            ToolStripMenuItem row = fixture.Row("recent");

            // Selecting this owned native row establishes the activation target; it
            // does not claim arrow navigation from the editable hosted filter.
            row.Select();
            row.Selected.Should().BeTrue();
            keyboard.Set(modifiers);
            fixture.DispatchDialogKey(fixture.Popup, Keys.Enter);
            fixture.Record("after-owned-Enter", new { modifiers = modifiers.ToString() });
            fixture.Clicks.Count.Should().BeLessThanOrEqualTo(1);
            if (modifiers == Keys.None)
            {
                fixture.Clicks.Should().ContainSingle("the native selected leaf's ordinary Enter route activates it");
            }
            else
            {
                fixture.Clicks.Should().BeEmpty("the actual native selected leaf does not treat modified Enter as plain Enter");
                fixture.Popup.Visible.Should().BeTrue();
                row.Selected.Should().BeTrue();
            }

            // Native ToolStripItem.ProcessDialogKey compares the full keyData to
            // Keys.Enter. These owned-message assertions corroborate that source
            // contract; they do not claim physical keyboard input or process routing.
            foreach (ClickObservation click in fixture.Clicks)
            {
                click.Row.Should().Be("recent");
                click.Modifiers.Should().Be((int)modifiers);
                click.SourceOpenRepoPredicateWouldLaunch.Should().Be(modifiers == Keys.Control);
            }

            fixture.AssertSinglePopulation();
            fixture.Complete();
        });
    }

    [TestCase("1: &Jasper &Quasar", 'j')]
    [TestCase("1: &Jasper &Quasar", 'q')]
    [TestCase("1: &Jasper &Quasar", 'Q')]
    [TestCase("1: Jasper&&Quasar", 'q')]
    [TestCase("1: Jasper&&&Quasar", 'q')]
    [TestCase("1: Jasper_Quasar", 'q')]
    [TestCase("1: Jasper_Quasar", '_')]
    [TestCase("1: &Jasper_Quasar", 'j')]
    [TestCase("1: &Jasper_Quasar", 'q')]
    [TestCase("1&0: Jasper_worktree", '0')]
    public void Actual_source_WorkingDir_should_dispatch_genuine_markers_without_treating_literal_underscores_as_mnemonics(
        string caption, char character)
    {
        WithFixture([new RowDefinition("recent", caption)], fixture =>
        {
            using KeyboardStateScope keyboard = new();
            keyboard.Set(Keys.None);
            fixture.Open();
            ToolStripMenuItem row = fixture.Row("recent");
            bool sourceMatch = Control.IsMnemonic(character, row.Text);
            fixture.AssertNoFixedMnemonicCollision(character);
            fixture.Record("native-mnemonic-eligibility", new
            {
                caption,
                character = character.ToString(),
                runtimeControlIsMnemonic = sourceMatch,
                sourceBoundary = "installed System.Windows.Forms.Control.IsMnemonic, not a duplicated caption parser",
            });
            keyboard.Set(Keys.Alt);
            fixture.DispatchMnemonic(fixture.Popup, character);
            fixture.Record("after-owned-queued-WM_SYSCHAR", new { character = character.ToString() });
            fixture.Clicks.Count.Should().Be(sourceMatch ? 1 : 0,
                "this controlled numeric-leading caption has no implicit first-character match or competing explicit mnemonic");
            if (sourceMatch)
            {
                fixture.Clicks.Single().Row.Should().Be("recent");
                fixture.Clicks.Single().Modifiers.Should().Be((int)Keys.Alt);
            }

            fixture.AssertSinglePopulation();
            fixture.Complete();
        });
    }

    [Test]
    public void Actual_source_WorkingDir_duplicate_mnemonics_should_cycle_native_selection_without_clicking()
    {
        WithFixture(
            [new RowDefinition("first", "1: &Jasper_worktree"), new RowDefinition("second", "2: &Juniper_worktree")],
            fixture =>
            {
                using KeyboardStateScope keyboard = new();
                keyboard.Set(Keys.None);
                fixture.Open();
                fixture.AssertNoFixedMnemonicCollision('j');
                fixture.Row("first").Select();
                keyboard.Set(Keys.Alt);
                List<string?> selected = [];
                for (int step = 0; step < 4; step++)
                {
                    fixture.DispatchMnemonic(fixture.Popup, 'j');
                    selected.Add(fixture.SelectedRow());
                    fixture.Record("duplicate-native-mnemonic", new { step, selectedRow = selected[^1] });
                    fixture.Clicks.Should().BeEmpty("the native two-match route selects instead of activating either repository");
                    fixture.Popup.Visible.Should().BeTrue();
                }

                selected.Should().Equal("second", "first", "second", "first");
                fixture.AssertSinglePopulation();
                fixture.Complete();
            });
    }

    [TestCase("Zed&Quasar&Jasper", 'z')]
    [TestCase("Zed&&Quasar", 'z')]
    [TestCase("Zed&Quasar", 'z')]
    public void Actual_source_WorkingDir_should_record_raw_first_character_fallback_for_mixed_mnemonic_captions(string caption, char character)
    {
        WithFixture([new RowDefinition("recent", caption)], fixture =>
        {
            using KeyboardStateScope keyboard = new();
            keyboard.Set(Keys.None);
            fixture.Open();
            fixture.AssertNoFixedMnemonicCollision(character);
            fixture.Record("raw-caption-pseudo-mnemonic-eligibility", new
            {
                caption,
                character = character.ToString(),
                runtimeControlIsMnemonic = Control.IsMnemonic(character, caption),
                rawFirstCharacter = caption[0].ToString(),
            });
            keyboard.Set(Keys.Alt);
            fixture.DispatchMnemonic(fixture.Popup, character);
            fixture.Record("after-native-raw-caption-fallback");

            // WinForms' private ContainsMnemonic eligibility is not equivalent to
            // "there is any genuine marker". Retain the installed runtime's second
            // pass outcome instead of duplicating that private helper as an oracle.
            fixture.Clicks.Count.Should().BeLessThanOrEqualTo(1);
            foreach (ClickObservation click in fixture.Clicks)
            {
                click.Row.Should().Be("recent");
            }

            fixture.AssertSinglePopulation();
            fixture.Complete();
        });
    }

    [Test]
    public void Actual_source_WorkingDir_explicit_mnemonic_should_precede_a_competing_raw_caption_fallback()
    {
        WithFixture(
            [new RowDefinition("implicit", "Zed&Quasar&Jasper"), new RowDefinition("explicit", "2: &Zelda_worktree")],
            fixture =>
            {
                using KeyboardStateScope keyboard = new();
                keyboard.Set(Keys.None);
                fixture.Open();
                fixture.AssertNoFixedMnemonicCollision('z');
                keyboard.Set(Keys.Alt);
                fixture.DispatchMnemonic(fixture.Popup, 'z');
                fixture.Record("after-native-explicit-before-raw-first-character");
                fixture.Clicks.Should().ContainSingle();
                fixture.Clicks.Single().Row.Should().Be("explicit");
                fixture.AssertSinglePopulation();
                fixture.Complete();
            });
    }

    [Test]
    public void Actual_source_WorkingDir_mnemonics_should_exclude_hidden_and_disabled_repository_rows()
    {
        WithFixture(
            [new RowDefinition("hidden", "1: &Jasper_hidden", Visible: false),
                new RowDefinition("disabled", "2: &Juniper_disabled", Enabled: false),
                new RowDefinition("active", "3: &Jade_visible")],
            fixture =>
            {
                using KeyboardStateScope keyboard = new();
                keyboard.Set(Keys.None);
                fixture.Open();
                fixture.Row("hidden").Visible.Should().BeFalse();
                fixture.Row("disabled").Enabled.Should().BeFalse();
                fixture.AssertNoFixedMnemonicCollision('j');
                keyboard.Set(Keys.Alt);
                fixture.DispatchMnemonic(fixture.Popup, 'j');
                fixture.Record("after-hidden-disabled-native-mnemonic");
                fixture.Clicks.Should().ContainSingle();
                fixture.Clicks.Single().Row.Should().Be("active");
                fixture.AssertSinglePopulation();
                fixture.Complete();
            });
    }

    [Test]
    public void Actual_source_WorkingDir_filter_hidden_rows_should_not_participate_in_native_mnemonic_dispatch()
    {
        WithFixture(
            [new RowDefinition("filtered", "1: &Jasper_hidden"), new RowDefinition("active", "2: &Jade_keep")],
            fixture =>
            {
                using KeyboardStateScope keyboard = new();
                keyboard.Set(Keys.None);
                fixture.Open();
                fixture.Filter.Text = "keep";
                fixture.Record("source-filter-textchanged", new { fixture.Filter.Text });
                fixture.Row("filtered").Visible.Should().BeFalse();
                fixture.Row("active").Visible.Should().BeTrue();
                fixture.Filter.GetCurrentParent().Should().BeSameAs(fixture.Popup);
                fixture.AssertNoFixedMnemonicCollision('j');
                keyboard.Set(Keys.Alt);
                fixture.DispatchMnemonic(fixture.Popup, 'j');
                fixture.Record("after-source-filtered-native-mnemonic");
                fixture.Clicks.Should().ContainSingle();
                fixture.Clicks.Single().Row.Should().Be("active");
                fixture.AssertSinglePopulation();
                fixture.Complete();
            });
    }

    [Test]
    public void Actual_source_WorkingDir_mnemonic_should_open_an_owned_repository_submenu_before_activating_its_child()
    {
        WithFixture(
            [new RowDefinition("parent", "1: &Jasper_group", Children: [new RowDefinition("child", "1: &Quasar_worktree")])],
            fixture =>
            {
                using KeyboardStateScope keyboard = new();
                keyboard.Set(Keys.None);
                fixture.Open();
                ToolStripMenuItem parent = fixture.Row("parent");
                fixture.AssertNoFixedMnemonicCollision('j');
                keyboard.Set(Keys.Alt);
                fixture.DispatchMnemonic(fixture.Popup, 'j');
                fixture.Record("after-native-parent-mnemonic");
                parent.DropDown.Visible.Should().BeTrue();
                parent.DropDown.OwnerItem.Should().BeSameAs(parent);
                fixture.Popup.Visible.Should().BeTrue();
                fixture.Clicks.Should().BeEmpty("opening a native row's submenu is not a repository leaf click");
                ToolStripMenuItem child = fixture.Row("child");
                child.GetCurrentParent().Should().BeSameAs(parent.DropDown);
                fixture.DispatchMnemonic(parent.DropDown, 'q');
                fixture.Record("after-native-child-mnemonic");
                fixture.Clicks.Should().ContainSingle();
                fixture.Clicks.Single().Row.Should().Be("child");
                fixture.AssertSinglePopulation();
                fixture.Complete();
            });
    }

    private static Point Center(Rectangle bounds) => new(bounds.X + (bounds.Width / 2), bounds.Y + (bounds.Height / 2));

    private static void WithFixture(RowDefinition[] rows, Action<SourceFixture> action)
    {
        Environment.GetEnvironmentVariable("GITEXTENSIONS_DEBUG_FAIL_FAST").Should().Be("1",
            "source Debug assertions must be capturable faults, not interactive JIT choosers");
        InvokeExistingHelper("WithIsolatedSettings", (Action<string>)(settingsDirectory =>
        {
            string isolatedSettings = Path.GetFullPath(AppSettings.SettingsContainer.SettingsCache.SettingsFilePath);
            isolatedSettings.Should().Be(Path.Combine(settingsDirectory, "GitExtensions.settings"));
            AppSettings.CurrentTranslation = "English";
            AppSettings.OwnScripts = string.Empty;
            AppSettings.CheckForUpdates = false;
            AppSettings.TelemetryEnabled = false;
            using SourceFixture fixture = new(rows, settingsDirectory);
            action(fixture);
        }));
    }

    private static void InvokeExistingHelper(string name, params object[] arguments)
    {
        MethodInfo method = typeof(ToolStripOwnerOverflowTests).GetMethod(name, PrivateStatic)
            ?? throw new MissingMethodException(typeof(ToolStripOwnerOverflowTests).FullName, name);
        Invoke(method, null, arguments);
    }

    private static object? Invoke(MethodInfo method, object? owner, params object?[] arguments)
    {
        try
        {
            return method.Invoke(owner, arguments);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static T CreateSource<T>(string name)
        where T : class
        => Activator.CreateInstance(typeof(FormBrowse).Assembly.GetType(name, throwOnError: true)
            ?? throw new TypeLoadException(name)) as T ?? throw new InvalidOperationException(name);

    private sealed record RowDefinition(string Name, string Caption, bool Visible = true, bool Enabled = true, RowDefinition[]? Children = null);

    private sealed record ClickObservation(string Step, string Row, int Modifiers, string ModifierNames,
        bool Control, bool Shift, bool Alt, bool SourceOpenRepoPredicateWouldLaunch, string SourceActionBoundary);

    private sealed class SourceFixture : IDisposable
    {
        private readonly Form _host;
        private readonly ToolStripEx _strip;
        private readonly ToolStripSplitButton _selector;
        private readonly ToolStripMenuItem _start;
        private readonly ToolStripMenuItem _close;
        private readonly IRepositoryHistoryUIService _history;
        private readonly Dictionary<string, ToolStripMenuItem> _rows = new(StringComparer.Ordinal);
        private readonly List<object> _steps = [];
        private readonly List<object> _events = [];
        private readonly List<object> _lifetime = [];
        private readonly List<object> _diagnosticFrames = [];
        private readonly List<NativeInputReceipt> _nativeInput = [];
        private readonly List<NativeInputPostReceipt> _nativeInputPosts = [];
        private readonly string _settingsDirectory;
        private readonly string _directory;
        private readonly ThreadExceptionEventHandler _threadExceptionHandler;
        private readonly NativeInputObserver _nativeInputObserver;
        private Exception? _threadException;
        private string _step = "constructed";
        private string _status = "diagnostic; test not completed";

        public SourceFixture(RowDefinition[] definitions, string settingsDirectory)
        {
            _settingsDirectory = settingsDirectory;
            _directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "WorkingDirPopupInputProbe", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            TestContext.Out.WriteLine($"workingDirPopupInputEvidence={_directory}");
            _host = new()
            {
                AutoScaleMode = AutoScaleMode.None,
                ClientSize = new Size(800, 460),
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Original WorkingDir owned input contract",
            };
            _strip = new() { Dock = DockStyle.Top, GripEnabled = false, ClickThrough = true };
            _selector = CreateSource<ToolStripSplitButton>(SelectorTypeName);
            _start = CreateSource<ToolStripMenuItem>(StartTypeName);
            _close = new("Close fixture boundary");
            _close.Click += (_, _) => throw new InvalidOperationException("No source fixed action may execute in this input probe.");
            _history = Substitute.For<IRepositoryHistoryUIService>();
            _history.When(service => service.PopulateFavouriteRepositoriesMenu(Arg.Any<ToolStripDropDownItem>()))
                .Do(call => call.Arg<ToolStripDropDownItem>().DropDownItems.Clear());
            _history.When(service => service.PopulateRecentRepositoriesMenu(Arg.Any<ToolStripDropDownItem>()))
                .Do(call =>
                {
                    ToolStripDropDownItem container = call.Arg<ToolStripDropDownItem>();
                    foreach (RowDefinition definition in definitions)
                    {
                        container.DropDownItems.Add(CreateRow(definition));
                    }
                });
            Func<IGitUICommands> commands = () => throw new InvalidOperationException("No source command/service graph may execute.");
            MethodInfo initialize = _selector.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public)
                ?? throw new MissingMethodException(SelectorTypeName, "Initialize");

            // Reinitializing Browse's existing selector would install a second set of
            // native handlers. This fresh actual source selector is initialized once;
            // a source Browse is not shown, loaded, or needed by the safe service seam.
            Invoke(initialize, _selector, commands, _history, _start, _close);
            _strip.Items.Add(_selector);
            _host.Controls.Add(_strip);
            Popup = _selector.DropDown;
            Popup.Opening += (_, args) => ObserveLifetime("opening", args.Cancel.ToString());
            Popup.Opened += (_, _) => ObserveLifetime("opened", null);
            Popup.Closing += (_, args) => ObserveLifetime("closing", args.CloseReason.ToString());
            Popup.Closed += (_, args) => ObserveLifetime("closed", args.CloseReason.ToString());
            _threadExceptionHandler = (_, args) => _threadException ??= args.Exception;
            Application.ThreadException += _threadExceptionHandler;
            _nativeInputObserver = new(ObserveNativeInput);
            Application.AddMessageFilter(_nativeInputObserver);
            WriteReport();
        }

        public ToolStripDropDown Popup { get; }

        public ToolStripTextBox Filter => Popup.Items.OfType<ToolStripTextBox>().Single();

        public List<ClickObservation> Clicks { get; } = [];

        public ToolStripMenuItem Row(string name) => _rows[name];

        public string? SelectedRow() => _rows.Values.SingleOrDefault(row => row.Selected)?.Name;

        public void Open()
        {
            _host.Show();
            _host.Activate();
            Settle();
            _selector.ShowDropDown();
            Settle();
            Record("opened-initialized-original-selector");
            Popup.Visible.Should().BeTrue();
            Popup.OwnerItem.Should().BeSameAs(_selector);
            Filter.TextBox.Parent.Should().BeSameAs(Popup);
            _host.DeviceDpi.Should().Be(96);
            Popup.DeviceDpi.Should().Be(96);
            AssertSinglePopulation();
            CaptureDiagnosticFrames("opened-original-selector");
        }

        public void DispatchPointer(uint message, Point position, Keys modifiers, bool leftButton)
        {
            AssertOwned(Popup);
            uint mouseFlags = (leftButton ? 1u : 0u)
                | ((modifiers & Keys.Shift) != 0 ? 4u : 0u)
                | ((modifiers & Keys.Control) != 0 ? 8u : 0u);
            nint coordinates = (nint)((uint)(ushort)position.X | ((uint)(ushort)position.Y << 16));
            Record("before-owned-pointer-message", new { message, position, mouseFlags });
            SendMessage(Popup.Handle, message, (nint)mouseFlags, coordinates);
            CheckThreadException();
            Record("after-owned-pointer-message", new { message, position, mouseFlags });
        }

        public void DispatchDialogKey(ToolStripDropDown target, Keys key)
        {
            AssertOwned(target);
            uint scanCode = MapVirtualKey((uint)key, 0);
            nint downBits = (nint)(1u | (scanCode << 16));
            DispatchPreprocessedMessage(target, KeyDownMessage, (nint)key, downBits);
            SendMessage(target.Handle, KeyUpMessage, (nint)key, (nint)(0xC0000001u | (scanCode << 16)));
            CheckThreadException();
        }

        public void DispatchMnemonic(ToolStripDropDown target, char character)
        {
            AssertOwned(target);
            int receiptStart = _nativeInput.Count;
            int postStart = _nativeInputPosts.Count;
            nint targetHandle = target.Handle;
            Record("before-owned-queued-mnemonic", new
            {
                character = character.ToString(),
                targetHandle = targetHandle.ToInt64(),
                outsideMessageLoop = !Application.MessageLoop,
            });

            // ToolStripDropDown.CanProcessMnemonic deliberately rejects calls outside
            // Application.MessageLoop. A synchronous PreProcessMessage after DoEvents
            // returned therefore could not prove an open native menu's input contract.
            // Queue only this owned HWND; the real loop performs its own filtering and
            // PreProcessControlMessage while the saved thread modifier state is live.
            bool posted = PostMessage(targetHandle, SystemCharacterMessage, character, (nint)(1 | ModifierAltContextBit));
            _nativeInputPosts.Add(new(targetHandle.ToInt64(), SystemCharacterMessage, character.ToString(),
                (int)Control.ModifierKeys, posted));
            WriteReport();
            posted.Should().BeTrue();
            Application.DoEvents();
            CheckThreadException();
            NativeInputReceipt[] receipts = _nativeInput.Skip(receiptStart).ToArray();
            Record("after-owned-queued-mnemonic", new
            {
                character = character.ToString(),
                ownedPostMessageCalls = _nativeInputPosts.Skip(postStart).ToArray(),
                actualNativeLoopReceipts = receipts,
                mode = "PostMessage to owned native popup then actual Application.DoEvents preprocessing; no direct PerformClick/ProcessMnemonic",
            });
            _nativeInputPosts.Skip(postStart).Should().ContainSingle("the fixture queues exactly one owned message per mnemonic step");
            receipts.Should().NotBeEmpty("the native application filter must observe the genuinely queued owned character");
            receipts.Should().OnlyContain(receipt => receipt.ApplicationMessageLoop
                && receipt.Window == targetHandle.ToInt64()
                && receipt.Message == SystemCharacterMessage
                && receipt.Character == character.ToString()
                && receipt.Modifiers == (int)Keys.Alt,
                "every real filter pass must preserve the owned target, character, loop and live Alt state; a filter pass is not an enqueue");
            Control.ModifierKeys.Should().Be(Keys.Alt);
            CaptureDiagnosticFrames("after-owned-queued-mnemonic");
        }

        public void AssertNoFixedMnemonicCollision(char character)
        {
            Popup.Items.OfType<ToolStripMenuItem>().Where(item => item.Name is null || !_rows.ContainsKey(item.Name))
                .Should().NotContain(item => Control.IsMnemonic(character, item.Text),
                    "fixed Open/Close/Configure actions are outside the observation seam and must never be activated");
        }

        public void AssertSinglePopulation()
        {
            _history.Received(1).PopulateFavouriteRepositoriesMenu(Arg.Any<ToolStripDropDownItem>());
            _history.Received(1).PopulateRecentRepositoriesMenu(_selector);
            _history.DidNotReceive().TriggerBranchNameCacheUpdate(Arg.Any<bool>());
        }

        public void Record(string stage, object? detail = null)
        {
            _step = stage;
            _steps.Add(new
            {
                stage,
                detail,
                actualModifiers = (int)Control.ModifierKeys,
                actualModifierNames = Control.ModifierKeys.ToString(),
                control = GetKeyState((int)Keys.ControlKey),
                shift = GetKeyState((int)Keys.ShiftKey),
                alt = GetKeyState((int)Keys.Menu),
                leftButton = GetKeyState((int)Keys.LButton),
                applicationMessageLoop = Application.MessageLoop,
                popupOwnerPressed = Popup.OwnerItem?.Pressed,
                popupContainsFocus = Popup.ContainsFocus,
                nativeFocus = GetFocus().ToInt64(),
                popupVisible = Popup.Visible,
                popupBounds = Popup.Bounds,
                hostedTextBoxFocused = Popup.Items.OfType<ToolStripTextBox>().SingleOrDefault()?.TextBox.Focused,
                selectedRows = _rows.Values.Where(row => row.Selected).Select(row => row.Name).ToArray(),
                rows = _rows.Values.Select(row => new
                {
                    row.Name,
                    row.Text,
                    row.Visible,
                    row.Available,
                    row.Enabled,
                    row.Selected,
                    row.Pressed,
                    row.Bounds,
                    parentType = row.GetCurrentParent()?.GetType().FullName,
                    ownerType = row.Owner?.GetType().FullName,
                    submenuVisible = row.HasDropDownItems && row.DropDown.Visible,
                }).ToArray(),
                clickCount = Clicks.Count,
            });
            WriteReport();
        }

        public void Complete()
        {
            CheckThreadException();
            _status = "completed native owned-message assertions; observation-only history callbacks";
            Record("completed");
        }

        public void Dispose()
        {
            try
            {
                if (_status.StartsWith("diagnostic", StringComparison.Ordinal))
                {
                    CaptureDiagnosticFrames("failure-before-native-teardown");
                }

                WriteReport();
                Popup.Close();
                Application.DoEvents();
                _host.Close();
                Application.DoEvents();
            }
            finally
            {
                Application.ThreadException -= _threadExceptionHandler;
                Application.RemoveMessageFilter(_nativeInputObserver);
                _host.Dispose();
                _start.Dispose();
                _close.Dispose();
            }
        }

        private ToolStripMenuItem CreateRow(RowDefinition definition)
        {
            ToolStripMenuItem row = new(definition.Caption)
            {
                Name = definition.Name,
                Available = definition.Visible,
                Enabled = definition.Enabled,
            };
            _rows.Add(row.Name, row);
            row.MouseDown += (_, args) => ObserveRowEvent(row, "mouseDown", args.Button.ToString());
            row.MouseUp += (_, args) => ObserveRowEvent(row, "mouseUp", args.Button.ToString());
            if (definition.Children is { Length: > 0 } children)
            {
                foreach (RowDefinition child in children)
                {
                    row.DropDownItems.Add(CreateRow(child));
                }

                row.DropDown.Opened += (_, _) => ObserveRowEvent(row, "submenuOpened", null);
                row.DropDown.Closed += (_, args) => ObserveRowEvent(row, "submenuClosed", args.CloseReason.ToString());
            }
            else
            {
                row.Click += (_, _) =>
                {
                    Keys modifiers = Control.ModifierKeys;
                    Clicks.Add(new(_step, row.Name, (int)modifiers, modifiers.ToString(),
                        GetKeyState((int)Keys.ControlKey) < 0, GetKeyState((int)Keys.ShiftKey) < 0, GetKeyState((int)Keys.Menu) < 0,
                        modifiers == Keys.Control,
                        "source-inspected OpenRepo predicate only; neither original service action nor LaunchBrowse executed"));
                    ObserveRowEvent(row, "observationOnlyClick", null);
                };
            }

            return row;
        }

        private void AssertOwned(ToolStripDropDown target)
        {
            (ReferenceEquals(target, Popup) || _rows.Values.Any(row => row.HasDropDownItems && ReferenceEquals(row.DropDown, target)))
                .Should().BeTrue("only a dropdown constructed inside this controlled source fixture may receive input");
            target.Visible.Should().BeTrue();
            target.IsHandleCreated.Should().BeTrue();
        }

        private void DispatchPreprocessedMessage(ToolStripDropDown target, uint messageId, nint parameter, nint bits)
        {
            Message message = Message.Create(target.Handle, (int)messageId, parameter, bits);
            Record("before-owned-key-preprocessing", new { messageId, parameter = parameter.ToInt64() });
            bool filtered = Application.FilterMessage(ref message);
            bool preprocessed = !filtered && target.PreProcessMessage(ref message);
            if (!filtered && !preprocessed)
            {
                SendMessage(target.Handle, messageId, parameter, bits);
            }

            Record("after-owned-key-preprocessing", new
            {
                messageId,
                filtered,
                preprocessed,
                target = target.GetType().FullName,
                targetHandle = target.Handle.ToInt64(),
                mode = "actual Application.FilterMessage then owned ToolStripDropDown.PreProcessMessage; SendMessage only if unhandled",
            });
        }

        private void ObserveRowEvent(ToolStripMenuItem row, string kind, string? detail)
        {
            _events.Add(new { step = _step, kind, detail, row = row.Name, modifiers = (int)Control.ModifierKeys, row.Visible, row.Enabled, row.Selected });
            WriteReport();
        }

        private void ObserveLifetime(string kind, string? reason)
        {
            _lifetime.Add(new { step = _step, kind, reason, actualModifiers = (int)Control.ModifierKeys });
            WriteReport();
        }

        private void ObserveNativeInput(Message message)
        {
            bool owned = (Popup.IsHandleCreated && message.HWnd == Popup.Handle)
                || _rows.Values.Any(row => row.HasDropDownItems && row.DropDown.IsHandleCreated && message.HWnd == row.DropDown.Handle);
            if (owned && message.Msg == SystemCharacterMessage)
            {
                _nativeInput.Add(new(message.HWnd.ToInt64(), (uint)message.Msg, ((char)message.WParam).ToString(),
                    Application.MessageLoop, (int)Control.ModifierKeys, GetFocus().ToInt64(),
                    new StackTrace(skipFrames: 1, fNeedFileInfo: false).GetFrames().Take(12)
                        .Select(frame => frame.GetMethod()).OfType<MethodBase>()
                        .Select(method => method.DeclaringType?.FullName + "." + method.Name).ToArray()));
                WriteReport();
            }
        }

        private void CaptureDiagnosticFrames(string stage)
        {
            List<(string Role, Control Window)> surfaces = [("owner", _host)];
            if (Popup.Visible)
            {
                surfaces.Add(("selector-popup", Popup));
            }

            foreach (ToolStripMenuItem row in _rows.Values.Where(row => row.HasDropDownItems && row.DropDown.Visible))
            {
                surfaces.Add(("submenu-" + row.Name, row.DropDown));
            }

            foreach ((string role, Control window) in surfaces)
            {
                if (!window.Visible || !window.IsHandleCreated)
                {
                    continue;
                }

                Rectangle requested = window is Form form ? form.Bounds : ((ToolStripDropDown)window).Bounds;
                Rectangle screenBounds = Rectangle.Intersect(requested, SystemInformation.VirtualScreen);
                string fileName = $"diagnostic-{_diagnosticFrames.Count:D3}-{stage}-{role}.png";
                try
                {
                    if (screenBounds.Width > 0 && screenBounds.Height > 0)
                    {
                        // These bounded live desktop samples are diagnostic only.
                        // They may contain occlusion and are neither accepted canonical
                        // acquisition nor proof of unobscured popup/chrome/raster parity.
                        using Bitmap frame = new(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);
                        using Graphics graphics = Graphics.FromImage(frame);
                        graphics.CopyFromScreen(screenBounds.Location, Point.Empty, screenBounds.Size, CopyPixelOperation.SourceCopy);
                        frame.Save(Path.Combine(_directory, fileName), ImageFormat.Png);
                    }

                    _diagnosticFrames.Add(new
                    {
                        stage,
                        role,
                        diagnosticOnly = true,
                        method = "rawScreenGrab",
                        fileName,
                        requestedBounds = requested,
                        capturedBounds = screenBounds,
                        requestedEntirelyOnScreen = requested == screenBounds,
                    });
                }
                catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or ExternalException or ArgumentException)
                {
                    // A diagnostic acquisition error must not replace the input
                    // assertion whose real message/callback receipts are retained.
                    _diagnosticFrames.Add(new
                    {
                        stage,
                        role,
                        diagnosticOnly = true,
                        method = "rawScreenGrab",
                        requestedBounds = requested,
                        capturedBounds = screenBounds,
                        error = exception.ToString(),
                    });
                }
            }

            WriteReport();
        }

        private void Settle()
        {
            Stopwatch settlement = Stopwatch.StartNew();
            do
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }
            while (settlement.ElapsedMilliseconds < 100);

            CheckThreadException();
            Application.OpenForms.Cast<Form>().Where(form => form.Visible).Should().OnlyContain(form => ReferenceEquals(form, _host),
                "the safe input probe has no picker, source Browse.OnLoad, script, or process-result dialog");
        }

        private void CheckThreadException()
        {
            if (_threadException is Exception exception)
            {
                throw new InvalidOperationException("The actual source owned input route raised a UI-thread exception.", exception);
            }
        }

        private void WriteReport()
        {
            object report = new
            {
                status = _status,
                test = TestContext.CurrentContext.Test.FullName,
                sourceSelector = _selector.GetType().FullName,
                sourceAssembly = typeof(FormBrowse).Assembly.FullName,
                runtimeWinFormsAssembly = typeof(Control).Assembly.FullName,
                sourceInitializationCount = 1,
                inputMode = "owned native HWND messages and real framework preprocessing with saved/restored thread keyboard state",
                physicalOperatingSystemInput = false,
                processesLaunched = false,
                scriptsExecuted = false,
                activeAppSettingsContainerIsTemporary = true,
                repositoryCommandsOrMutations = false,
                clipboardAccessed = false,
                browseConstructedOrLoaded = false,
                historyService = "injected observation-only IRepositoryHistoryUIService; no original OpenRepo action executed",
                sourceRoutingCorroboration = "GitUI/RepositoryHistoryUIService.OpenRepo uses Control.ModifierKeys != Keys.Control for current-instance routing",
                settingsDirectory = _settingsDirectory,
                hostDpi = _host.DeviceDpi,
                popupDpi = Popup.DeviceDpi,
                unsupported = "physical mouse/keyboard, IME/AltGr, full cold/loaded Browse, original history action/process routing, persistence, all platforms/scales",
                steps = _steps,
                events = _events,
                lifetime = _lifetime,
                clicks = Clicks,
                nativeInput = _nativeInput,
                nativeInputPosts = _nativeInputPosts,
                diagnosticFrames = _diagnosticFrames,
            };
            File.WriteAllText(Path.Combine(_directory, "probe.json"), JsonSerializer.Serialize(report, JsonOptions) + Environment.NewLine);
        }
    }

    private sealed record NativeInputReceipt(long Window, uint Message, string Character, bool ApplicationMessageLoop, int Modifiers, long FocusWindow,
        string[] FilterCallStackMethods);

    private sealed record NativeInputPostReceipt(long Window, uint Message, string Character, int Modifiers, bool Posted);

    private sealed class NativeInputObserver(Action<Message> observe) : IMessageFilter
    {
        public bool PreFilterMessage(ref Message message)
        {
            observe(message);
            return false;
        }
    }

    private sealed class KeyboardStateScope : IDisposable
    {
        private readonly byte[] _original = new byte[256];

        public KeyboardStateScope()
        {
            GetKeyboardState(_original).Should().BeTrue();
        }

        public void Set(Keys modifiers, bool leftButton = false)
        {
            (modifiers & ~Keys.Modifiers).Should().Be(Keys.None);
            byte[] current = (byte[])_original.Clone();
            foreach (int key in new[] { 0x01, 0x10, 0x11, 0x12, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 })
            {
                current[key] &= 0x7F;
            }

            if ((modifiers & Keys.Control) != 0)
            {
                current[0x11] |= 0x80;
                current[0xA2] |= 0x80;
            }

            if ((modifiers & Keys.Shift) != 0)
            {
                current[0x10] |= 0x80;
                current[0xA0] |= 0x80;
            }

            if ((modifiers & Keys.Alt) != 0)
            {
                current[0x12] |= 0x80;
                current[0xA4] |= 0x80;
            }

            if (leftButton)
            {
                current[0x01] |= 0x80;
            }

            SetKeyboardState(current).Should().BeTrue();
            Control.ModifierKeys.Should().Be(modifiers);
            (GetKeyState((int)Keys.LButton) < 0).Should().Be(leftButton);
        }

        public void Dispose()
        {
            SetKeyboardState(_original).Should().BeTrue();
            byte[] restored = new byte[256];
            GetKeyboardState(restored).Should().BeTrue();
            restored.Should().Equal(_original, "the controlled probe must restore the native calling thread's complete keyboard state");
        }
    }

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint parameter, nint data);

    [DllImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(nint window, uint message, nint parameter, nint data);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetKeyboardState([Out] byte[] keys);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetKeyboardState(byte[] keys);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int key);

    [DllImport("user32.dll")]
    private static extern nint GetFocus();

    [DllImport("user32.dll", EntryPoint = "MapVirtualKeyW")]
    private static extern uint MapVirtualKey(uint code, uint mapType);
}
