using System.Configuration;
using System.Drawing;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Settings;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

// Original helper results and handler IL are observed; no physical keyboard or OS
// clipboard operation is performed or claimed. Temporary settings precede AppSettings.
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class NativeRichTextCopyContractTests
{
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private static readonly Type XhtmlExtension = typeof(GitUI.CommitInfo.CommitInfoHeader).Assembly.GetType(
        "GitUI.Editor.RichTextBoxExtension.RichTextBoxXhtmlSupportExtension", throwOnError: true)
        ?? throw new TypeLoadException("The original XHTML extension must be present.");
    private static readonly MethodInfo Loader = GetExtension("SetXHTMLText", typeof(RichTextBox), typeof(string));
    private static readonly MethodInfo PlainReader = GetExtension("GetPlainText", typeof(RichTextBox));
    private static readonly MethodInfo SelectionReader = GetExtension("GetSelectionPlainText", typeof(RichTextBox));
    private static readonly IReadOnlyDictionary<ushort, OpCode> Opcodes = typeof(OpCodes)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.FieldType == typeof(OpCode))
        .Select(field => (OpCode)(field.GetValue(null) ?? throw new InvalidOperationException("An IL opcode must have a value.")))
        .GroupBy(opcode => unchecked((ushort)opcode.Value))
        .ToDictionary(group => group.Key, group => group.First());

    [TestCase(9, false, false)]
    [TestCase(9, false, true)]
    [TestCase(9, true, false)]
    [TestCase(9, true, true)]
    [TestCase(11, false, false)]
    [TestCase(11, false, true)]
    [TestCase(11, true, false)]
    [TestCase(11, true, true)]
    public void Source_helpers_should_report_the_native_paragraph_and_selection_copy_contract(int points, bool trailingParagraph, bool crlfInput)
    {
        WithIsolatedSettings(() =>
        {
            using Font font = new("Segoe UI", points, FontStyle.Regular, GraphicsUnit.Point);
            using Form host = new()
            {
                AutoScaleMode = AutoScaleMode.None,
                ClientSize = new Size(700, 320),
                ShowInTaskbar = false,
            };
            using RichTextBox editor = new()
            {
                Font = font,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.None,
                WordWrap = false,
                ReadOnly = true,
                Size = new Size(640, 240),
            };
            host.Controls.Add(editor);
            string separator = crlfInput ? "\r\n" : "\n";
            string xhtml = $"Author:\t\t<a href='mailto:copy@example.invalid'>A &amp; B</a>{separator}Commit:\t<a href='gitext://gotorevision/abc123'>abc123</a>{separator}last line"
                + (trailingParagraph ? separator : string.Empty);
            string expected = "Author:\t\tA & B\nCommit:\tabc123\nlast line" + (trailingParagraph ? "\n" : string.Empty);
            Loader.Invoke(null, [editor, xhtml]);
            host.Show();
            try
            {
                SettlePaint(host, editor);
                editor.Select(1, 2);
                string plain = Read(PlainReader, editor);
                Report("full-plain", plain, expected);
                plain.Should().Be(expected,
                    "the original helper reads RichTextBox.SelectedText, whose native paragraph characters are exposed as LF");
                editor.SelectionStart.Should().Be(1);
                editor.SelectionLength.Should().Be(2);
                editor.Text.Should().NotContain("\r", "WinForms exposes RichEdit paragraphs as LF to managed consumers");
                host.DeviceDpi.Should().Be(96);
                editor.DeviceDpi.Should().Be(96);
                font.Name.Should().Be("Segoe UI");

                int firstNewline = editor.Text.IndexOf('\n');
                int author = editor.Text.IndexOf("A & B", StringComparison.Ordinal);
                int finalLine = editor.Text.IndexOf("last line", StringComparison.Ordinal);
                firstNewline.Should().BeGreaterThan(0);
                author.Should().BeGreaterThan(0);
                finalLine.Should().BeGreaterThan(firstNewline);
                (string Name, int Start, int Length, string Expected)[] selections =
                [
                    ("empty", 0, 0, string.Empty),
                    ("newline-only", firstNewline, 1, "\n"),
                    ("newline-and-next-plain-run", firstNewline, "\nCommit:\t".Length, "\nCommit:\t"),
                    ("partial-caption", author + 2, 3, "& B"),
                    ("plain-run", finalLine + 2, 4, "st l"),
                    ("select-all", 0, editor.TextLength, expected),
                ];
                foreach ((string name, int start, int length, string expectedSelection) in selections)
                {
                    editor.Select(start, length);
                    string selected = Read(SelectionReader, editor);
                    Report(name, selected, expectedSelection);
                    selected.Should().Be(expectedSelection);
                    editor.SelectionStart.Should().Be(start, "reading copy text must restore the source selection");
                    editor.SelectionLength.Should().Be(length);
                }

                if (trailingParagraph)
                {
                    editor.Select(editor.TextLength - 1, 1);
                    string selected = Read(SelectionReader, editor);
                    Report("trailing-final-paragraph", selected, "\n");
                    selected.Should().Be("\n");
                    editor.SelectionStart.Should().Be(editor.TextLength - 1);
                    editor.SelectionLength.Should().Be(1);
                }

                // UTF16 surrogate/combining behavior must be observed in the actual
                // helper before portable tests claim that its character loop preserves it.
                Loader.Invoke(null, [editor, "plain ä😀\n<a href='gitext://unicode'>e\u0301 &amp; B</a>\nlast"]);
                SettlePaint(host, editor);
                editor.Select(1, 2);
                string unicodePlain = Read(PlainReader, editor);
                Report("unicode-full-plain-observation", unicodePlain, "plain ä😀\ne\u0301 & B\nlast");
                unicodePlain.Should().NotContain("\r");
                editor.SelectionStart.Should().Be(1);
                editor.SelectionLength.Should().Be(2);

                editor.WordWrap = true;
                editor.Width = 160;
                const string wrappedCaption = "office affinity office affinity";
                Loader.Invoke(null, [editor, $"head <a href='gitext://wrapped-body'>{wrappedCaption}</a> tail\nlast"]);
                SettlePaint(host, editor);
                int wrappedCaptionStart = editor.Text.IndexOf(wrappedCaption, StringComparison.Ordinal);
                wrappedCaptionStart.Should().BeGreaterThan(0);
                editor.GetLineFromCharIndex(editor.TextLength).Should().BeGreaterThan(1,
                    "the source body/refs fixture must exercise real native wrapping, not a header-only no-wrap control");
                editor.Select(wrappedCaptionStart + 1, 4);
                string partialWrappedCaption = Read(SelectionReader, editor);
                Report("wrapped-body-partial-caption", partialWrappedCaption, "ffic");
                partialWrappedCaption.Should().Be("ffic");
                editor.SelectionStart.Should().Be(wrappedCaptionStart + 1);
                editor.SelectionLength.Should().Be(4);
                editor.Select(0, editor.TextLength);
                string wrappedPlain = Read(SelectionReader, editor);
                string expectedWrappedPlain = $"head {wrappedCaption} tail\nlast";
                Report("wrapped-body-select-all", wrappedPlain, expectedWrappedPlain);
                wrappedPlain.Should().Be(expectedWrappedPlain,
                    "visual wrapping must not insert paragraph characters into the source copy text");
            }
            finally
            {
                host.Close();
                Application.DoEvents();
            }

            return;

            void Report(string stage, string actual, string expectedResult)
            {
                TestContext.Out.WriteLine(JsonSerializer.Serialize(new
                {
                    source = "RichTextBoxXhtmlSupportExtension.GetPlainText/GetSelectionPlainText",
                    stage,
                    points,
                    crlfInput,
                    trailingParagraph,
                    dpiMode = "nativeMonitor",
                    captureMethod = "none: text contract, not a visual capture",
                    deviceDpi = editor.DeviceDpi,
                    textLength = editor.TextLength,
                    wordWrap = editor.WordWrap,
                    clientWidth = editor.ClientSize.Width,
                    visualLineCount = editor.GetLineFromCharIndex(editor.TextLength) + 1,
                    nativeManagedText = editor.Text,
                    nativeCodePoints = editor.Text.Select(character => (int)character).ToArray(),
                    selectionStart = editor.SelectionStart,
                    selectionLength = editor.SelectionLength,
                    actual,
                    actualCodePoints = actual.Select(character => (int)character).ToArray(),
                    expected = expectedResult,
                }));
            }
        });
    }

    [TestCase(typeof(GitUI.CommitInfo.CommitInfoHeader), "rtbRevisionHeader_KeyDown", "GetSelectionPlainText")]
    [TestCase(typeof(GitUI.CommitInfo.CommitInfo), "RichTextBox_KeyDown", "GetSelectionPlainText")]
    [TestCase(typeof(GitUI.CommitInfo.CommitInfo), "copyCommitInfoToolStripMenuItem_Click", "GetPlainText")]
    public void Original_copy_handlers_should_retain_their_actual_helper_call_shape_without_touching_the_clipboard(Type owner, string handler, string helper)
    {
        MethodInfo method = owner.GetMethod(handler, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new MissingMethodException(owner.FullName, handler);
        byte[] il = method.GetMethodBody()?.GetILAsByteArray()
            ?? throw new InvalidOperationException("The original copy handler must retain its executable body.");
        string[] calls = GetMethodCalls(method, il).ToArray();
        TestContext.Out.WriteLine(JsonSerializer.Serialize(new
        {
            source = $"{owner.FullName}.{handler}",
            evidenceMode = "actual native assembly IL: handler not invoked, clipboard not exercised",
            ilSha256 = Convert.ToHexString(SHA256.HashData(il)),
            calls,
        }));
        calls.Should().Contain(call => call.EndsWith('.' + helper, StringComparison.Ordinal));
        calls.Should().Contain("GitExtUtils.ClipboardUtil.TrySetText");
        if (handler == "copyCommitInfoToolStripMenuItem_Click")
        {
            calls.Count(call => call == "System.Environment.get_NewLine").Should().Be(2,
                "the source composite deliberately keeps two platform separators between its native LF blocks");
        }
    }

    private static IEnumerable<string> GetMethodCalls(MethodInfo method, byte[] il)
    {
        int position = 0;
        while (position < il.Length)
        {
            ushort value = il[position++];
            if (value == 0xFE)
            {
                value = (ushort)((value << 8) | il[position++]);
            }

            OpCode opcode = Opcodes[value];
            if (opcode.OperandType == OperandType.InlineMethod)
            {
                MethodBase target = method.Module.ResolveMethod(BitConverter.ToInt32(il, position))
                    ?? throw new InvalidOperationException("An original handler method token must resolve.");
                yield return $"{target.DeclaringType?.FullName}.{target.Name}";
            }

            position += opcode.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + (BitConverter.ToInt32(il, position) * 4),
                _ => 4,
            };
        }
    }

    private static MethodInfo GetExtension(string name, params Type[] parameters)
        => XhtmlExtension.GetMethod(name, BindingFlags.Public | BindingFlags.Static, parameters)
            ?? throw new MissingMethodException(XhtmlExtension.FullName, name);

    private static string Read(MethodInfo reader, RichTextBox editor)
        => reader.Invoke(null, [editor]) as string
            ?? throw new InvalidOperationException("The original text helper must return its string result.");

    private static void SettlePaint(Form host, RichTextBox editor)
    {
        Application.DoEvents();
        host.Refresh();
        host.Update();
        editor.Refresh();
        editor.Update();
        Application.DoEvents();
    }

    private static void WithIsolatedSettings(Action action)
    {
        string directory = Path.Combine(Path.GetTempPath(), "GitExtensions.RichTextCopyProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string settingsPath = Path.Combine(directory, "GitExtensions.settings");
        File.WriteAllText(settingsPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><dictionary />");
        InitializeAppSettingsWithoutRealConfiguration(directory);
        using GitExtSettingsCache cache = new(settingsPath, autoSave: false);
        DistributedSettings settings = new(lowerPriority: null, cache, SettingLevel.Unknown);
        TestContext.Out.WriteLine($"isolated original settings retained: {directory}");
        AppSettings.UsingContainer(settings, action);
    }

    private static void InitializeAppSettingsWithoutRealConfiguration(string directory)
    {
        Type settingsType = typeof(AppSettings).Assembly.GetType("GitCommands.Properties.Settings", throwOnError: true)
            ?? throw new TypeLoadException("GitCommands.Properties.Settings");
        ApplicationSettingsBase configuration = settingsType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) as ApplicationSettingsBase
            ?? throw new MissingMemberException(settingsType.FullName, "Default");
        _ = configuration["IsPortable"];
        SettingsPropertyValue portable = configuration.PropertyValues["IsPortable"]
            ?? throw new MissingMemberException(settingsType.FullName, "IsPortable");
        object originalPortableValue = portable.PropertyValue;
        bool originalDirty = portable.IsDirty;
        FieldInfo frameworkPath = typeof(Application).GetField("s_executablePath", PrivateStatic)
            ?? throw new MissingFieldException(typeof(Application).FullName, "s_executablePath");
        object? originalFrameworkPath = frameworkPath.GetValue(null);
        string actualExecutablePath = Application.ExecutablePath;
        string isolatedExecutablePath = Path.Combine(directory, "GitExtensions.exe");
        try
        {
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
}
