using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class RichEditLinkSelectionTests
{
    private const string Caption = "office affinity";
    private const string Uri = "gitext://author";
    private const uint MouseMove = 0x200;
    private const uint LeftButtonDown = 0x201;
    private const uint LeftButtonUp = 0x202;
    private const uint RightButtonDown = 0x204;
    private const uint RightButtonUp = 0x205;
    private const uint NativeLeftButtonDown = 0x2;
    private const uint NativeLeftButtonUp = 0x4;
    private const uint GetOptions = 0x44E;

    [Test]
    [TestCase(0, 3)]
    [TestCase(1, 5)]
    [TestCase(7, 11)]
    [TestCase(11, 15)]
    public void Source_XHTML_links_should_copy_partial_caption_characters_without_hidden_URIs(int start, int end)
    {
        using Font font = new("Segoe UI", 9);
        using RichTextBox editor = CreateEditor(font);
        LoadXhtml(editor, $"head <a href='{Uri}'>{Caption}</a> tail");
        int captionStart = editor.Text.IndexOf(Caption, StringComparison.Ordinal);
        editor.Select(captionStart + start, end - start);
        GetSelectionPlainText(editor).Should().Be(Caption[start..end]);
        editor.SelectionStart.Should().Be(captionStart + start);
        editor.SelectionLength.Should().Be(end - start);
        editor.Select(3, editor.Text.IndexOf(" tail", StringComparison.Ordinal) + 2 - 3);
        GetSelectionPlainText(editor).Should().Be($"d {Caption} t");
    }

    [Test]
    [TestCase(0)]
    [TestCase(4)]
    public void Source_XHTML_link_click_should_report_the_native_press_route(int modifiers)
    {
        using Font font = new("Segoe UI", 9);
        using RichTextBox editor = CreateEditor(font);
        using Form host = new() { ShowInTaskbar = false, ClientSize = editor.Size };
        host.Controls.Add(editor);
        LoadXhtml(editor, $"head <a href='{Uri}'>{Caption}</a> tail");
        host.Show();
        Application.DoEvents();
        int captionStart = editor.Text.IndexOf(Caption, StringComparison.Ordinal);
        Point target = editor.GetPositionFromCharIndex(captionStart + 2);
        target.Offset(1, font.Height / 2);
        int clicks = 0;
        editor.LinkClicked += (_, e) =>
        {
            clicks++;
            TestContext.Progress.WriteLine($"linkEvent modifiers={modifiers} clicks={clicks} start={e.LinkStart} length={e.LinkLength} selection={editor.SelectionStart}/{editor.SelectionLength}");
        };
        try
        {
            editor.Select(0, 0);
            SendMessage(editor.Handle, MouseMove, 0, PackPoint(target));
            SendMessage(editor.Handle, LeftButtonDown, 1 | modifiers, PackPoint(target));
            TestContext.Progress.WriteLine($"afterLeftDown modifiers={modifiers} clicks={clicks} selection={editor.SelectionStart}/{editor.SelectionLength}");
            clicks.Should().Be(1, "RichTextBox raises its native link event on the left-button-down notification");
            SendMessage(editor.Handle, LeftButtonUp, modifiers, PackPoint(target));
            TestContext.Progress.WriteLine($"afterLeftUp modifiers={modifiers} clicks={clicks} selection={editor.SelectionStart}/{editor.SelectionLength}");
            clicks.Should().Be(1);
            SendMessage(editor.Handle, RightButtonDown, 2, PackPoint(target));
            SendMessage(editor.Handle, RightButtonUp, 0, PackPoint(target));
            TestContext.Progress.WriteLine($"afterRightClick modifiers={modifiers} clicks={clicks} selection={editor.SelectionStart}/{editor.SelectionLength}");
            clicks.Should().Be(1, "right-clicking a source link is not link activation");
        }
        finally
        {
            host.Close();
        }
    }

    [Test]
    [TestCase(false, false, false, "head office ")]
    [TestCase(true, false, false, "head office affinity")]
    [TestCase(false, true, false, "office affinity tail")]
    [TestCase(true, true, false, "office affinity tail")]
    [TestCase(false, false, true, "a")]
    [TestCase(true, false, true, "a")]
    public void Source_XHTML_caption_should_preserve_native_drag_selection_started_in_plain_text(bool useLink, bool backwards, bool withinWord, string expected)
    {
        using Font font = new("Segoe UI", 9);
        using RichTextBox editor = CreateEditor(font);
        using Form host = new() { ShowInTaskbar = false, ClientSize = editor.Size };
        host.Controls.Add(editor);
        editor.CreateControl();
        nint initialOptions = SendMessage(editor.Handle, GetOptions, 0, 0);
        LoadXhtml(editor, useLink ? $"head <a href='{Uri}'>{Caption}</a> tail" : $"head {Caption} tail");
        TestContext.Progress.WriteLine($"nativeDrag useLink={useLink} autoWordSelection={editor.AutoWordSelection} optionsBefore={initialOptions:X} optionsAfter={SendMessage(editor.Handle, GetOptions, 0, 0):X}");
        host.Show();
        Application.DoEvents();
        int clicks = 0;
        editor.LinkClicked += (_, _) => clicks++;
        int captionStart = editor.Text.IndexOf(Caption, StringComparison.Ordinal);
        int startIndex = backwards ? editor.Text.IndexOf("tail", StringComparison.Ordinal) + 1 : 2;
        int endIndex = withinWord ? 3 : captionStart + 3;
        Point start = editor.GetPositionFromCharIndex(startIndex);
        Point end = editor.GetPositionFromCharIndex(endIndex);
        start.Offset(1, font.Height / 2);
        end.Offset(1, font.Height / 2);
        Point previousPosition = Cursor.Position;
        nint previousForeground = GetForegroundWindow();
        string diagnosticsDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "HeaderProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(diagnosticsDirectory);
        try
        {
            ForegroundOwnedWindow(host);
            editor.Focus().Should().BeTrue();
            PumpNativeInput();
            TestContext.Progress.WriteLine($"dragBefore focused={editor.Focused} foreground={GetForegroundWindow():X} host={host.Handle:X} screenStart={editor.PointToScreen(start)} screenEnd={editor.PointToScreen(end)}");
            SaveStage("before");
            Cursor.Position = editor.PointToScreen(start);
            PumpNativeInput();
            MouseEvent(NativeLeftButtonDown, 0, 0, 0, 0);
            PumpNativeInput();
            TestContext.Progress.WriteLine($"dragDown cursor={Cursor.Position} capture={GetCapture():X} selection={editor.SelectionStart}/{editor.SelectionLength}");
            SaveStage("down");
            Cursor.Position = editor.PointToScreen(end);
            PumpNativeInput();
            TestContext.Progress.WriteLine($"dragMove cursor={Cursor.Position} capture={GetCapture():X} selection={editor.SelectionStart}/{editor.SelectionLength}");
            SaveStage("move");
            MouseEvent(NativeLeftButtonUp, 0, 0, 0, 0);
            PumpNativeInput();
            SaveStage("up");
            TestContext.Progress.WriteLine($"drag clicks={clicks} selection={editor.SelectionStart}/{editor.SelectionLength} plain={GetSelectionPlainText(editor)}");
            clicks.Should().Be(0);
            GetSelectionPlainText(editor).Should().Be(expected);
        }
        finally
        {
            MouseEvent(NativeLeftButtonUp, 0, 0, 0, 0);
            ReleaseCapture();
            Cursor.Position = previousPosition;
            host.Close();
            if (previousForeground != 0)
            {
                SetForegroundWindow(previousForeground);
            }
        }

        return;

        void SaveStage(string stage)
        {
            using CaptureImageResult capture = ImageCapture.Capture(host, [], []);
            string path = Path.Combine(diagnosticsDirectory, $"native-drag-{stage}.png");
            capture.Bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            TestContext.Progress.WriteLine($"nativeDragEvidence={path}");
        }
    }

    [Test]
    public void Source_XHTML_caption_should_expose_each_character_inside_common_ligature_sequences()
    {
        using Font font = new("Segoe UI", 9);
        using RichTextBox editor = CreateEditor(font);
        LoadXhtml(editor, $"head <a href='{Uri}'>{Caption}</a> tail");
        int start = editor.Text.IndexOf(Caption, StringComparison.Ordinal);
        int[] positions = Enumerable.Range(0, Caption.Length + 1)
            .Select(index => editor.GetPositionFromCharIndex(start + index).X).ToArray();
        TestContext.Progress.WriteLine($"captionPositions={string.Join(',', positions)}");
        for (int index = 1; index < positions.Length; index++)
        {
            positions[index].Should().BeGreaterThan(positions[index - 1],
                "source caption characters remain individual caret/copy positions inside office and affinity");
        }
    }

    private static RichTextBox CreateEditor(Font font)
        => new()
        {
            Font = font,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            WordWrap = false,
            ReadOnly = true,
            Size = new Size(500, 100),
        };

    private static Type GetExtension()
        => typeof(GitUI.CommitInfo.CommitInfoHeader).Assembly.GetType("GitUI.Editor.RichTextBoxExtension.RichTextBoxXhtmlSupportExtension", throwOnError: true)
            ?? throw new InvalidOperationException("The source RichEdit XHTML loader must be present.");

    private static string GetSelectionPlainText(RichTextBox editor)
    {
        MethodInfo reader = GetExtension().GetMethod(nameof(GetSelectionPlainText), BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("The source RichEdit selection reader must be present.");
        return reader.Invoke(null, [editor]) as string
            ?? throw new InvalidOperationException("The source RichEdit selection must be readable.");
    }

    private static void LoadXhtml(RichTextBox editor, string xhtml)
    {
        editor.CreateControl();
        MethodInfo loader = GetExtension().GetMethod("SetXHTMLText", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("The source RichEdit XHTML loader must be present.");
        loader.Invoke(null, [editor, xhtml]);
        Application.DoEvents();
    }

    private static nint PackPoint(Point point) => (nint)((point.Y << 16) | (point.X & 0xffff));

    private static void PumpNativeInput()
    {
        // Injected physical input is queued by Windows, not synchronously dispatched by
        // mouse_event. Pump the owned STA window before inspecting or releasing a press.
        long until = Environment.TickCount64 + 100;
        while (Environment.TickCount64 < until)
        {
            Application.DoEvents();
        }
    }

    private static void ForegroundOwnedWindow(Form host)
    {
        uint currentThread = GetCurrentThreadId();
        uint foregroundThread = GetWindowThreadProcessId(GetForegroundWindow(), out _);
        bool attached = foregroundThread != 0 && foregroundThread != currentThread;
        if (attached)
        {
            AttachThreadInput(currentThread, foregroundThread, true).Should().BeTrue();
        }

        try
        {
            BringWindowToTop(host.Handle).Should().BeTrue();
            SetForegroundWindow(host.Handle).Should().BeTrue();
            host.Activate();
            PumpNativeInput();
            GetForegroundWindow().Should().Be(host.Handle,
                "physical mouse input must reach the disposable source HWND, not the previously active IDE");
        }
        finally
        {
            if (attached)
            {
                AttachThreadInput(currentThread, foregroundThread, false).Should().BeTrue();
            }
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern nint SendMessage(nint window, uint message, nint wordParameter, nint longParameter);

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern nint GetCapture();

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint window);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint window, out uint processId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint sourceThread, uint targetThread, bool attach);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(nint window);

    [DllImport("user32.dll", EntryPoint = "mouse_event")]
    private static extern void MouseEvent(uint flags, uint x, uint y, uint data, nuint extraInfo);
}
