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
public sealed class NativeTreeKeyboardModifierContractTests
{
    [TestCase(9, 90)]
    [TestCase(9, 160)]
    [TestCase(11, 90)]
    [TestCase(11, 160)]
    public void Source_tree_should_record_modifier_scroll_backspace_dynamic_caret_and_prefix_contracts(float points, int viewportHeight)
    {
        string directory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TreeKeyboardModifierProbe", Guid.NewGuid().ToString("N"));
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
        branches.Nodes.Add("wide-" + new string('W', 80));
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
        DateTime decoratorTime = new(2026, 10, 3, 12, 0, 0);
        NativeTreeViewExplorerNavigationDecorator navigation = new(tree, () => decoratorTime);
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

        // These new contracts are observations, not assumed native formulas.
        // All input is queued to this fixture's HWND and uses its real message-loop
        // preprocessing; no physical desktop/IDE input or elapsed-time injection.
        foreach (Keys key in new[] { Keys.Home, Keys.End, Keys.PageUp, Keys.PageDown, Keys.Left, Keys.Right, Keys.Up, Keys.Down })
        {
            currentStep = $"ctrl-{key}-setup";
            tree.SelectedNode = tags.Nodes[15];
            tree.TopNode = tags.Nodes[10];
            ScrollReport horizontal = ReadScroll(tree, 0);
            horizontal.QuerySucceeded.Should().BeTrue("the source long label supplies an actual horizontal-scroll range");
            int horizontalMaximum = Math.Max(horizontal.Minimum, horizontal.Maximum - (int)horizontal.Page + 1);
            int horizontalMiddle = (horizontal.Minimum + horizontalMaximum) / 2;
            SendMessage(tree.Handle, 0x114, (nint)(4 | (horizontalMiddle << 16)), 0); // WM_HSCROLL/SB_THUMBPOSITION.
            Record("sourceControlSetup", null);
            ScrollReport beforeHorizontal = ReadScroll(tree, 0);
            ScrollReport beforeVertical = ReadScroll(tree, 1);
            int activationStart = events.Count;
            RunQueuedKey($"ctrl-{key}", key, control: true);
            tree.SelectedNode.Should().BeSameAs(tags.Nodes[15]);
            events.Skip(activationStart).Should().NotContain(entry => entry.Kind == "explorerAfterSelect");
            ScrollReport afterHorizontal = ReadScroll(tree, 0);
            ScrollReport afterVertical = ReadScroll(tree, 1);
            int verticalMaximum = Math.Max(beforeVertical.Minimum, beforeVertical.Maximum - (int)beforeVertical.Page + 1);
            int expectedVertical = key switch
            {
                Keys.Home => beforeVertical.Minimum,
                Keys.End => verticalMaximum,
                Keys.PageUp => beforeVertical.Position - Math.Max(1, (int)beforeVertical.Page - 1),
                Keys.PageDown => beforeVertical.Position + Math.Max(1, (int)beforeVertical.Page - 1),
                Keys.Up => beforeVertical.Position - 1,
                Keys.Down => beforeVertical.Position + 1,
                _ => beforeVertical.Position,
            };
            afterVertical.Position.Should().Be(Math.Clamp(expectedVertical, beforeVertical.Minimum, verticalMaximum));
            int expectedHorizontal = beforeHorizontal.Position + (key == Keys.Left ? -5 : key == Keys.Right ? 5 : 0);
            afterHorizontal.Position.Should().Be(Math.Clamp(expectedHorizontal, beforeHorizontal.Minimum, horizontalMaximum));
        }

        currentStep = "backspace-leaf-setup";
        tree.SelectedNode = nestedLeaf;
        Record("sourceControlSetup", null);
        RunQueuedKey("backspace-from-leaf", Keys.Back);
        tree.SelectedNode.Should().BeSameAs(nested);
        RunQueuedKey("backspace-repeat", Keys.Back);
        tree.SelectedNode.Should().BeSameAs(feature);
        currentStep = "backspace-root-setup";
        tree.SelectedNode = branches;
        Record("sourceControlSetup", null);
        RunQueuedKey("backspace-from-root", Keys.Back);
        tree.SelectedNode.Should().BeSameAs(branches);

        tree.SelectedNode = main;
        RunQueuedKey("shift-down", Keys.Down, shift: true);
        tree.SelectedNode.Should().BeSameAs(maintenance);
        RunQueuedKey("shift-home", Keys.Home, shift: true);
        tree.SelectedNode.Should().BeSameAs(branches);
        RunQueuedKey("shift-end", Keys.End, shift: true);
        tree.SelectedNode.Should().BeSameAs(stashes);

        RunQueuedKey("mixed-case-prefix-reset", Keys.Home);
        RunQueuedCharacter("mixed-case-prefix-uppercase-M", 'M');
        RunQueuedCharacter("mixed-case-prefix-lowercase-m", 'm');
        tree.SelectedNode.Should().BeSameAs(main);
        RunQueuedCharacter("mixed-case-prefix-next-a", 'a');
        tree.SelectedNode.Should().BeSameAs(main);
        ReadIncrementalSearch(tree, out _).Should().Be("Mma");
        RunQueuedKey("accented-prefix-reset", Keys.Home);
        RunQueuedCharacter("accented-prefix-m", 'm');
        RunQueuedCharacter("accented-prefix-é", 'é');
        tree.SelectedNode!.Text.Should().Be("mémoire");
        RunQueuedKey("live-caption-prefix-reset", Keys.Home);
        main.Text = "master";
        RunQueuedCharacter("live-caption-prefix-m", 'm');
        RunQueuedCharacter("live-caption-prefix-a", 'a');
        RunQueuedCharacter("live-caption-prefix-s", 's');
        tree.SelectedNode.Should().BeSameAs(main);

        currentStep = "collapse-selected-descendant-setup";
        tree.SelectedNode = nestedLeaf;
        Record("sourceControlSetup", null);
        currentStep = "collapse-selected-descendant";
        feature.Collapse();
        Application.DoEvents();
        Record("actualSourceNodeCollapse", null);
        tree.SelectedNode.Should().BeSameAs(feature);
        currentStep = "restore-selected-descendant-setup";
        feature.Expand();
        tree.SelectedNode = nestedLeaf;
        Record("sourceControlSetup", null);
        currentStep = "remove-selected-descendant";
        nestedLeaf.Remove();
        Application.DoEvents();
        Record("actualSourceNodeRemoval", null);
        tree.SelectedNode.Should().BeSameAs(nested);
        currentStep = "remove-selected-root-setup";
        tree.SelectedNode = main;
        Record("sourceControlSetup", null);
        currentStep = "remove-selected-root";
        branches.Remove();
        Application.DoEvents();
        Record("actualSourceRootRemoval", null);
        tree.SelectedNode.Should().BeSameAs(tags);

        currentStep = "remove-selected-leaf-with-siblings-setup";
        tree.SelectedNode = tags.Nodes[15];
        Record("sourceControlSetup", null);
        currentStep = "remove-selected-leaf-with-siblings";
        tags.Nodes[15].Remove();
        Application.DoEvents();
        Record("actualSourceSiblingLeafRemoval", null);
        tree.SelectedNode!.Text.Should().Be("tag-16");
        currentStep = "remove-selected-last-leaf-setup";
        TreeNode lastLeaf = tags.Nodes[^1];
        TreeNode precedingLeaf = tags.Nodes[^2];
        tree.SelectedNode = lastLeaf;
        Record("sourceControlSetup", null);
        currentStep = "remove-selected-last-leaf";
        lastLeaf.Remove();
        Application.DoEvents();
        Record("actualSourceLastSiblingLeafRemoval", null);
        tree.SelectedNode.Should().BeSameAs(precedingLeaf);
        currentStep = "remove-selected-last-root-setup";
        tree.SelectedNode = stashes;
        Record("sourceControlSetup", null);
        currentStep = "remove-selected-last-root";
        stashes.Remove();
        Application.DoEvents();
        Record("actualSourceLastRootRemoval", null);
        tree.SelectedNode.Should().BeSameAs(tags);
        currentStep = "remove-only-root-setup";
        tree.SelectedNode = tags;
        Record("sourceControlSetup", null);
        currentStep = "remove-only-root";
        tags.Remove();
        Application.DoEvents();
        Record("actualSourceOnlyRootRemoval", null);
        tree.SelectedNode.Should().BeNull();

        WriteReport();
        TestContext.Out.WriteLine($"inputMode=queuedKeyDownWithScopedThreadKeyboardState dpiMode=nativeMonitor deviceDpi={tree.DeviceDpi} fontPoints={points} viewportHeight={viewportHeight} output={path}");

        void RunQueuedKey(string name, Keys key, bool control = false, bool shift = false)
        {
            currentStep = name;
            decoratorTime = decoratorTime.AddMilliseconds(600);
            GetFocus().Should().Be(tree.Handle);
            using KeyboardStateScope keyboard = new(control, shift);
            uint scanCode = MapVirtualKey((uint)key, 0);
            int extended = key is Keys.Home or Keys.End or Keys.PageUp or Keys.PageDown or Keys.Left or Keys.Right or Keys.Up or Keys.Down ? 1 << 24 : 0;
            nint keyDownData = (nint)(1 | (int)(scanCode << 16) | extended);
            int eventStart = events.Count;
            PostMessage(tree.Handle, 0x100, (nint)key, keyDownData).Should().BeTrue();
            PostMessage(tree.Handle, 0x101, (nint)key, unchecked((nint)((long)keyDownData | 0xC0000000L))).Should().BeTrue();
            Application.DoEvents();
            Record("queuedKeyDownWithScopedThreadKeyboardState", "actualApplicationMessageLoop");
            (GetKeyState(0x11) < 0).Should().Be(control);
            (GetKeyState(0x10) < 0).Should().Be(shift);
            events.Skip(eventStart).Should().Contain(entry => entry.Kind == "PreviewKeyDown",
                "the real queued key must pass the source preprocessing boundary before its effects are interpreted");
            foreach (InputEvent preview in events.Skip(eventStart).Where(entry => entry.Kind == "PreviewKeyDown"))
            {
                preview.KeyOrAction.Contains("Control", StringComparison.Ordinal).Should().Be(control);
                preview.KeyOrAction.Contains("Shift", StringComparison.Ordinal).Should().Be(shift);
            }

            if (!control)
            {
                bool expectedActivation = (key is Keys.Home or Keys.End) || (key == Keys.Back && name != "backspace-from-root");
                events.Skip(eventStart).Count(entry => entry.Kind == "explorerAfterSelect").Should().Be(expectedActivation ? 1 : 0,
                    "the public source decorator clock isolates its business suppression interval from the independently queued native input");
            }
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
                GetKeyState(0x11), GetKeyState(0x10), GetFocus().ToInt64(), nativeElapsedMilliseconds, GetMessageTime(), Environment.TickCount64,
                visibleNodes, events.Count));

            // Retain completed observations before each source assertion, even if
            // a later assumption is rejected by the first actual native run.
            WriteReport();
        }

        void WriteReport()
        {
            KeyboardReport report = new("originalNativeTreeViewAndExplorerDecorator", "queuedKeyDownWithScopedThreadKeyboardState",
                "nativeMonitor", tree.DeviceDpi, font.Name, font.SizeInPoints, font.Style.ToString(), viewportHeight,
                tree.ItemHeight, images.ImageSize, tree.ClientRectangle, Application.ColorMode.ToString(), Application.IsDarkModeEnabled,
                GetDoubleClickTime(), "sourcePublicDecoratorInjectedStepClockOnlyNativeQueuedInputAndPrefixTimingRemainReal",
                "messageDrivenOnlyPhysicalKeyboardAndImeNotVerified", steps, events);
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

        public KeyboardStateScope(bool control, bool shift = false)
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

            if (shift)
            {
                current[0x10] |= 0x80;
                current[0xA0] |= 0x80;
            }

            SetKeyboardState(current).Should().BeTrue();
            (GetKeyState(0x11) < 0).Should().Be(control);
            (GetKeyState(0x10) < 0).Should().Be(shift);
        }

        public void Dispose() => SetKeyboardState(_original).Should().BeTrue();
    }

    private sealed record KeyboardReport(string SourceBoundary, string InputMode, string DpiMode, int DeviceDpi,
        string ResolvedFontFamily, float FontSizeInPoints, string FontStyle, int RequestedViewportHeight, int ItemHeight, Size ImageSize,
        Rectangle ClientRectangle, string ColorMode, bool DarkModeEnabled, uint NativeDoubleClickMilliseconds, string DecoratorActivationClockMode, string UnsupportedState,
        List<StepReport> Steps, List<InputEvent> Events);

    private sealed record StepReport(string Step, string Route, string? Preprocessing, string? Caret, string? TopNode,
        bool Focused, ScrollReport HorizontalScroll, ScrollReport VerticalScroll, bool IncrementalSearchExists,
        string IncrementalSearch, short ControlKeyState, short ShiftKeyState, long NativeFocusHandle, int? NativeElapsedMilliseconds,
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
