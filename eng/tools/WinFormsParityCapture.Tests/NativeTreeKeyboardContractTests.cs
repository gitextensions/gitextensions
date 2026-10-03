using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using GitUI.UserControls;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class NativeTreeKeyboardContractTests
{
    [TestCase(9, 90)]
    [TestCase(9, 160)]
    [TestCase(11, 90)]
    [TestCase(11, 160)]
    public void Source_tree_keyboard_should_record_caret_activation_prefix_and_scroll_contracts(float points, int viewportHeight)
    {
        string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TreeKeyboardProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "keyboard.json");
        using Font font = new("Segoe UI", points);
        using Bitmap blankIcon = new(16, 18);
        using ImageList images = new() { ImageSize = blankIcon.Size, ColorDepth = ColorDepth.Depth32Bit };
        images.Images.Add(blankIcon);
        using Form window = new()
        {
            AutoScaleMode = AutoScaleMode.None,
            ClientSize = new Size(260, viewportHeight),
            ShowInTaskbar = false
        };
        using NativeTreeView tree = new()
        {
            Dock = DockStyle.Fill,
            Font = font,
            BorderStyle = BorderStyle.None,
            ImageList = images,
            FullRowSelect = true,
            PathSeparator = "/",
            ShowNodeToolTips = true
        };
        using Button other = new() { Bounds = new Rectangle(205, viewportHeight - 23, 50, 23), Text = "Other" };
        TreeNode branches = tree.Nodes.Add("Branches");
        TreeNode alpha = branches.Nodes.Add("alpha");
        TreeNode feature = branches.Nodes.Add("feature");
        TreeNode nested = feature.Nodes.Add("nested");
        TreeNode nestedLeaf = nested.Nodes.Add("deep-leaf");
        TreeNode main = branches.Nodes.Add("main");
        TreeNode maintenance = branches.Nodes.Add("maintenance");
        branches.Nodes.Add("mémoire");
        TreeNode tags = tree.Nodes.Add("Tags");
        for (int index = 0; index < 30; index++)
        {
            tags.Nodes.Add($"tag-{index:00}");
        }

        TreeNode stashes = tree.Nodes.Add("Stashes");
        stashes.Nodes.Add("stash-one");
        branches.Expand();
        feature.Expand();
        nested.Expand();
        tags.Expand();
        NativeTreeViewExplorerNavigationDecorator navigation = new(tree);
        List<InputEvent> events = [];
        List<StepReport> steps = [];
        string currentStep = "setup";
        tree.PreviewKeyDown += (_, e) => events.Add(new InputEvent(currentStep, "PreviewKeyDown", e.KeyData.ToString(), tree.SelectedNode?.FullPath));
        tree.KeyDown += (_, e) => events.Add(new InputEvent(currentStep, "KeyDown", e.KeyData.ToString(), tree.SelectedNode?.FullPath));
        tree.KeyPress += (_, e) => events.Add(new InputEvent(currentStep, "KeyPress", $"U+{(int)e.KeyChar:X4}", tree.SelectedNode?.FullPath));
        tree.AfterSelect += (_, e) => events.Add(new InputEvent(currentStep, "nativeAfterSelect", e.Action.ToString(), e.Node?.FullPath));
        navigation.AfterSelect += (_, e) => events.Add(new InputEvent(currentStep, "explorerAfterSelect", e.Action.ToString(), e.Node?.FullPath));
        window.Controls.Add(tree);
        window.Controls.Add(other);
        window.Show();
        window.Activate();
        tree.Focus().Should().BeTrue();

        // Native activation/focus and client painting can remain queued after
        // Show. Settle real messages before recording item bounds or key input.
        long paintingSettledAt = Environment.TickCount64 + 100;
        while (Environment.TickCount64 < paintingSettledAt)
        {
            Application.DoEvents();
        }

        window.Refresh();
        tree.Refresh();
        tree.DeviceDpi.Should().Be(96);
        window.DeviceDpi.Should().Be(96);
        tree.Focused.Should().BeTrue();
        GetFocus().Should().Be(tree.Handle);
        tree.SelectedNode = alpha;
        events.Clear();

        // Each native route is fed through the real Application-loop preprocessing
        // boundary before dispatch. WM_KEYDOWN alone omits PreviewKeyDown and gives
        // a false result for the original explorer decorator's arrow suppression.
        RunKey("home", Keys.Home);
        tree.SelectedNode.Should().BeSameAs(branches);
        RunKey("end", Keys.End);
        tree.SelectedNode.Should().BeSameAs(stashes, "End chooses the last visible item, not a collapsed descendant");
        RunKey("page-up", Keys.PageUp);
        tree.SelectedNode.Should().NotBeSameAs(stashes);
        TreeNode? pageUp = tree.SelectedNode;
        RunKey("page-down", Keys.PageDown);
        tree.SelectedNode.Should().NotBeSameAs(pageUp);

        currentStep = "set-nested-leaf";
        tree.SelectedNode = nestedLeaf;
        nestedLeaf.EnsureVerticallyVisible();
        Record("setup", null);
        RunKey("left-from-leaf", Keys.Left);
        tree.SelectedNode.Should().BeSameAs(nested);
        RunKey("right-from-expanded-parent", Keys.Right);
        tree.SelectedNode.Should().BeSameAs(nestedLeaf);
        RunKey("up-to-parent", Keys.Up);
        tree.SelectedNode.Should().BeSameAs(nested);
        RunKey("down-to-first-child", Keys.Down);
        tree.SelectedNode.Should().BeSameAs(nestedLeaf);

        currentStep = "set-middle-for-ctrl-scroll";
        tree.SelectedNode = tags.Nodes[10];
        tree.SelectedNode.EnsureVerticallyVisible();
        Record("setup", null);
        TreeNode? caretBeforeCtrl = tree.SelectedNode;
        int scrollBeforeCtrl = ReadScroll(tree, 1).Position;
        RunKey("ctrl-down", Keys.Down, control: true);
        tree.SelectedNode.Should().BeSameAs(caretBeforeCtrl, "native Ctrl+Down scrolls without moving the caret");
        int scrollAfterCtrl = ReadScroll(tree, 1).Position;
        scrollAfterCtrl.Should().BeGreaterThan(scrollBeforeCtrl);
        RunKey("ctrl-up", Keys.Up, control: true);
        tree.SelectedNode.Should().BeSameAs(caretBeforeCtrl);
        ReadScroll(tree, 1).Position.Should().BeLessThan(scrollAfterCtrl);

        currentStep = "set-middle-for-page-sequence";
        tree.SelectedNode = tags.Nodes[15];
        tree.TopNode = tags.Nodes[13];
        Record("setup", null);
        RunKey("middle-page-up", Keys.PageUp);
        RunKey("middle-page-up-repeat", Keys.PageUp);
        RunKey("middle-page-down", Keys.PageDown);
        RunKey("middle-page-down-repeat", Keys.PageDown);

        currentStep = "set-alpha-for-arrow-activation";
        tree.SelectedNode = alpha;
        int navigationBeforeArrow = CountExplorerEvents();
        RunKey("arrow-down-suppressed", Keys.Down);
        CountExplorerEvents().Should().Be(navigationBeforeArrow);
        TreeNode? caretBeforeActivation = tree.SelectedNode;
        RunKey("enter-activates-caret", Keys.Enter);
        tree.SelectedNode.Should().BeSameAs(caretBeforeActivation);
        CountExplorerEvents().Should().BeGreaterThan(navigationBeforeArrow);
        int navigationBeforeSpace = CountExplorerEvents();
        RunKey("space-activates-caret", Keys.Space);
        tree.SelectedNode.Should().BeSameAs(caretBeforeActivation);
        CountExplorerEvents().Should().BeGreaterThan(navigationBeforeSpace);

        currentStep = "set-alpha-for-prefix";
        tree.SelectedNode = alpha;
        alpha.EnsureVerticallyVisible();
        Record("setup", null);
        foreach ((string name, char character) in new[]
        {
            ("prefix-m", 'm'), ("prefix-repeated-m", 'm'), ("prefix-ma", 'a'), ("prefix-unmatched", 'z'),
            ("prefix-unicode", 'é')
        })
        {
            RunCharacter(name, character);
            if (name == "prefix-m")
            {
                tree.SelectedNode.Should().BeSameAs(main);
                ReadIncrementalSearch(tree, out _).Should().Be("m");
            }
            else if (name == "prefix-repeated-m")
            {
                tree.SelectedNode.Should().BeSameAs(maintenance);
                ReadIncrementalSearch(tree, out _).Should().Be("mm");
            }
            else
            {
                tree.SelectedNode.Should().BeSameAs(maintenance, "a nonmatching accumulated prefix leaves the native caret unchanged");
            }
        }

        RunKey("prefix-reset-home", Keys.Home);
        RunCharacter("contiguous-prefix-m", 'm');
        RunCharacter("contiguous-prefix-ma", 'a');
        RunCharacter("contiguous-prefix-maa", 'a');
        currentStep = "prefix-focus-lost";
        other.Focus().Should().BeTrue();
        Application.DoEvents();
        GetFocus().Should().Be(other.Handle);
        Record("nativeFocusChange", null);
        currentStep = "prefix-focus-returned";
        tree.Focus().Should().BeTrue();
        Application.DoEvents();
        GetFocus().Should().Be(tree.Handle);
        Record("nativeFocusChange", null);
        RunCharacter("prefix-after-focus-m", 'm');

        int doubleClickMilliseconds = checked((int)GetDoubleClickTime());
        foreach (int delay in new[]
        {
            Math.Max(0, doubleClickMilliseconds - 50), doubleClickMilliseconds + 50,
            (doubleClickMilliseconds * 2) - 50, (doubleClickMilliseconds * 2) + 50
        }.Distinct())
        {
            RunKey($"timed-prefix-{delay}-reset-home", Keys.Home);
            RunCharacter($"timed-prefix-{delay}-m", 'm');
            currentStep = $"timed-prefix-{delay}-elapsed";
            long startedAt = Environment.TickCount64;
            long settledAt = startedAt + delay;
            while (Environment.TickCount64 < settledAt)
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }

            Record("nativeElapsedMessagePump", null, checked((int)(Environment.TickCount64 - startedAt)));
            RunCharacter($"timed-prefix-{delay}-a", 'a');
        }

        foreach (int delay in new[]
        {
            Math.Max(0, doubleClickMilliseconds - 50), doubleClickMilliseconds + 50,
            (doubleClickMilliseconds * 2) - 50, (doubleClickMilliseconds * 2) + 50
        }.Distinct())
        {
            RunKey($"queued-prefix-{delay}-reset-home", Keys.Home);
            RunQueuedCharacter($"queued-prefix-{delay}-m", 'm');
            currentStep = $"queued-prefix-{delay}-elapsed";
            long startedAt = Environment.TickCount64;
            long settledAt = startedAt + delay;
            while (Environment.TickCount64 < settledAt)
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }

            Record("nativeElapsedMessagePump", null, checked((int)(Environment.TickCount64 - startedAt)));
            RunQueuedCharacter($"queued-prefix-{delay}-a", 'a');
        }

        // The additional reset/page/timeout observations remain diagnostic until
        // an actual native run establishes those remaining control contracts.
        // No synthetic elapsed-time injection claims to alter the OS search timer.
        WriteReport();
        TestContext.Out.WriteLine($"inputMode=applicationLoopPreprocessThenWindowMessage dpiMode=nativeMonitor deviceDpi={tree.DeviceDpi} fontPoints={points} viewportHeight={viewportHeight} output={path}");

        void RunKey(string name, Keys key, bool control = false)
        {
            currentStep = name;
            GetFocus().Should().Be(tree.Handle);
            using KeyboardStateScope keyboard = new(control);
            uint scanCode = MapVirtualKey((uint)key, 0);
            int extended = key is Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown or Keys.Left or Keys.Right or Keys.Up or Keys.Down ? 1 << 24 : 0;
            nint keyDownData = (nint)(1 | (int)(scanCode << 16) | extended);
            PreProcessControlState preprocessing = Dispatch(0x100, (nint)key, keyDownData);
            SendMessage(tree.Handle, 0x101, (nint)key, unchecked((nint)((long)keyDownData | 0xC0000000L)));
            Application.DoEvents();
            Record("key", preprocessing.ToString());
        }

        void RunCharacter(string name, char character)
        {
            currentStep = name;
            GetFocus().Should().Be(tree.Handle);
            using KeyboardStateScope keyboard = new(false);
            PreProcessControlState preprocessing = Dispatch(0x102, character, 1);
            Application.DoEvents();
            Record("character", preprocessing.ToString());
        }

        void RunQueuedCharacter(string name, char character)
        {
            currentStep = name;
            GetFocus().Should().Be(tree.Handle);
            using KeyboardStateScope keyboard = new(false);

            // A synchronous SendMessage does not publish a new message-queue
            // timestamp. The native incremental-search timer must also be probed
            // through genuinely queued input, with both clocks retained separately.
            PostMessage(tree.Handle, 0x102, character, 1).Should().BeTrue();
            Application.DoEvents();
            Record("queuedCharacter", "actualApplicationDoEventsMessageLoop");
        }

        PreProcessControlState Dispatch(int messageId, nint parameter, nint data)
        {
            Message message = Message.Create(tree.Handle, messageId, parameter, data);
            PreProcessControlState state = tree.PreProcessControlMessage(ref message);
            if (state != PreProcessControlState.MessageProcessed)
            {
                SendMessage(message.HWnd, (uint)message.Msg, message.WParam, message.LParam);
            }

            return state;
        }

        int CountExplorerEvents() => events.Count(entry => entry.Kind == "explorerAfterSelect");

        void Record(string route, string? preprocessing, int? nativeElapsedMilliseconds = null)
        {
            ScrollReport horizontal = ReadScroll(tree, 0);
            ScrollReport vertical = ReadScroll(tree, 1);
            List<NodeReport> visibleNodes = [];
            foreach (TreeNode node in EnumerateExpandedNodes(tree.Nodes))
            {
                visibleNodes.Add(new NodeReport(node.FullPath, node.Text, node.IsExpanded, node.IsVisible, node.Bounds));
            }

            string incrementalSearch = ReadIncrementalSearch(tree, out bool searchExists);
            steps.Add(new StepReport(currentStep, route, preprocessing, tree.SelectedNode?.FullPath,
                tree.TopNode?.FullPath, tree.Focused, horizontal, vertical, searchExists, incrementalSearch,
                GetKeyState(0x11), GetFocus().ToInt64(), nativeElapsedMilliseconds, GetMessageTime(), Environment.TickCount64,
                visibleNodes, events.Count));

            // Retain completed observations before each source assertion, even if
            // a later assumption is rejected by the first actual native run.
            WriteReport();
        }

        void WriteReport()
        {
            KeyboardReport report = new("originalNativeTreeViewAndExplorerDecorator", "applicationLoopPreprocessThenWindowMessage",
                "nativeMonitor", tree.DeviceDpi, font.Name, font.SizeInPoints, font.Style.ToString(), viewportHeight,
                tree.ItemHeight, images.ImageSize, tree.ClientRectangle, Application.ColorMode.ToString(), Application.IsDarkModeEnabled,
                GetDoubleClickTime(), "messageDrivenOnlyPhysicalKeyboardAndImeNotVerified", steps, events);
            File.WriteAllText(path, JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

    private static IEnumerable<TreeNode> EnumerateExpandedNodes(TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            yield return node;
            if (node.IsExpanded)
            {
                foreach (TreeNode child in EnumerateExpandedNodes(node.Nodes))
                {
                    yield return child;
                }
            }
        }
    }

    private static ScrollReport ReadScroll(NativeTreeView tree, int bar)
    {
        ScrollInformation information = new() { Size = (uint)Marshal.SizeOf<ScrollInformation>(), Mask = 0x17 };

        // A missing horizontal scrollbar is a real state, not a failed vertical
        // measurement. Preserve the query result rather than assuming a position.
        bool succeeded = GetScrollInfo(tree.Handle, bar, ref information);
        if (bar == 1)
        {
            succeeded.Should().BeTrue();
        }

        return new ScrollReport(succeeded, information.Minimum, information.Maximum,
            information.Page, information.Position, information.TrackPosition);
    }

    private static string ReadIncrementalSearch(NativeTreeView tree, out bool searchExists)
    {
        StringBuilder buffer = new(1024);
        searchExists = SendSearchMessage(tree.Handle, 0x1140, 0, buffer) != 0; // TVM_GETISEARCHSTRINGW.
        return buffer.ToString();
    }

    private sealed class KeyboardStateScope : IDisposable
    {
        private readonly byte[] _original = new byte[256];

        public KeyboardStateScope(bool control)
        {
            GetKeyboardState(_original).Should().BeTrue();
            byte[] current = (byte[])_original.Clone();
            foreach (int key in new[] { 0x10, 0x11, 0x12, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 })
            {
                current[key] &= 0x7F;
            }

            if (control)
            {
                current[0x11] |= 0x80;
                current[0xA2] |= 0x80;
            }

            SetKeyboardState(current).Should().BeTrue();
            (GetKeyState(0x11) < 0).Should().Be(control);
        }

        public void Dispose() => SetKeyboardState(_original).Should().BeTrue();
    }

    private sealed record KeyboardReport(string SourceBoundary, string InputMode, string DpiMode, int DeviceDpi,
        string ResolvedFontFamily, float FontSizeInPoints, string FontStyle, int RequestedViewportHeight, int ItemHeight, Size ImageSize,
        Rectangle ClientRectangle, string ColorMode, bool DarkModeEnabled, uint NativeDoubleClickMilliseconds, string UnsupportedState,
        List<StepReport> Steps, List<InputEvent> Events);

    private sealed record StepReport(string Step, string Route, string? Preprocessing, string? Caret, string? TopNode,
        bool Focused, ScrollReport HorizontalScroll, ScrollReport VerticalScroll, bool IncrementalSearchExists,
        string IncrementalSearch, short ControlKeyState, long NativeFocusHandle, int? NativeElapsedMilliseconds,
        int NativeMessageTime, long NativeTickCount64, List<NodeReport> ExpandedVisibleOrder, int EventCount);

    private sealed record NodeReport(string FullPath, string Caption, bool Expanded, bool WithinViewport, Rectangle TextBounds);

    private sealed record InputEvent(string Step, string Kind, string KeyOrAction, string? Caret);

    private sealed record ScrollReport(bool QuerySucceeded, int Minimum, int Maximum, uint Page, int Position, int TrackPosition);

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint parameter, nint data);

    [DllImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(nint window, uint message, nint parameter, nint data);

    [DllImport("user32.dll")]
    private static extern int GetMessageTime();

    [DllImport("user32.dll", EntryPoint = "SendMessageW", CharSet = CharSet.Unicode)]
    private static extern nint SendSearchMessage(nint window, uint message, nint parameter, StringBuilder text);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetScrollInfo(nint window, int bar, ref ScrollInformation information);

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

    [DllImport("user32.dll")]
    private static extern uint GetDoubleClickTime();

    [DllImport("user32.dll", EntryPoint = "MapVirtualKeyW")]
    private static extern uint MapVirtualKey(uint code, uint mapType);

    [StructLayout(LayoutKind.Sequential)]
    private struct ScrollInformation
    {
        public uint Size;
        public uint Mask;
        public int Minimum;
        public int Maximum;
        public uint Page;
        public int Position;
        public int TrackPosition;
    }
}
