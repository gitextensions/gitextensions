using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class CommitInfoContextMenuTests
{
    private const string Link = "https://example.invalid/source";
    private const uint RightButtonDown = 0x204;
    private const uint RightButtonUp = 0x205;
    private const nuint RightButtonFlag = 2;

    [TestCase(false)]
    [TestCase(true)]
    public void Source_GetCharIndexFromPosition_should_report_nearest_link_at_blank_client_positions(bool trailingPlainText)
    {
        using Font font = new("Segoe UI", 9);
        using Form host = new() { ClientSize = new Size(640, 240), ShowInTaskbar = false };
        using RichTextBox editor = new()
        {
            Font = font,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            WordWrap = false,
            ReadOnly = true,
            Size = new Size(500, 100),
        };
        host.Controls.Add(editor);
        Type extension = typeof(GitUI.CommitInfo.CommitInfoHeader).Assembly.GetType(
            "GitUI.Editor.RichTextBoxExtension.RichTextBoxXhtmlSupportExtension", throwOnError: true)
            ?? throw new InvalidOperationException("The original XHTML extension must be present.");
        MethodInfo loader = extension.GetMethod("SetXHTMLText", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("The original XHTML loader must be present.");
        MethodInfo getLink = extension.GetMethod("GetLink", BindingFlags.Public | BindingFlags.Static, [typeof(RichTextBox), typeof(int)])
            ?? throw new InvalidOperationException("The original link reader must be present.");
        loader.Invoke(null, [editor, $"head <a href='{Link}'>caption</a>{(trailingPlainText ? " tail" : string.Empty)}"]);
        host.Show();
        Application.DoEvents();
        int captionStart = editor.Text.IndexOf("caption", StringComparison.Ordinal);
        Point caption = editor.GetPositionFromCharIndex(captionStart + 3);
        Point[] positions = [editor.GetPositionFromCharIndex(2), caption, new Point(editor.ClientSize.Width - 10, caption.Y + 2), new Point(caption.X, editor.ClientSize.Height - 10)];
        editor.Select(1, 2);
        string?[] links = positions.Select(position =>
        {
            int index = editor.GetCharIndexFromPosition(position);
            string? link = getLink.Invoke(null, [editor, index]) as string;
            TestContext.Out.WriteLine($"trailingPlainText={trailingPlainText} point={position} charIndex={index}/{editor.TextLength} character={(index >= 0 && index < editor.TextLength ? (int)editor.Text[index] : -1)} link={link ?? "<null>"}");
            editor.SelectionStart.Should().Be(1);
            editor.SelectionLength.Should().Be(2);
            return link;
        }).ToArray();
        TestContext.Out.WriteLine($"sourceText={editor.Text.Replace('\r', ' ').Replace('\n', ' ')}");
        links[0].Should().BeNull();
        links[1].Should().Be(Link);
        links[2].Should().Be(trailingPlainText ? null : Link,
            "blank space after the line targets its nearest final character, including the source's hidden linked URI");
        links[3].Should().Be(Link,
            "blank space below a row retains the nearest character at the requested horizontal position");
        host.Close();
    }

    [Test]
    public void Source_context_menu_Opening_should_use_only_the_control_which_opened_the_shared_menu()
    {
        using Form host = new() { ClientSize = new Size(640, 240), ShowInTaskbar = false };
        using RichTextBox first = new() { Location = new Point(0, 0), Size = new Size(200, 70) };
        using RichTextBox second = new() { Location = new Point(0, 80), Size = new Size(200, 70) };
        using Panel panel = new() { Location = new Point(300, 0), Size = new Size(200, 200) };
        using ContextMenuStrip menu = new();
        menu.Items.Add("Copy link");
        Control? openingSource = null;
        menu.Opening += (_, _) => openingSource = menu.SourceControl;
        host.Controls.AddRange([first, second, panel]);
        host.Show();
        Application.DoEvents();
        foreach (Control source in new Control[] { first, second, panel, first })
        {
            openingSource = null;
            menu.Show(source, new Point(10, 10));
            Application.DoEvents();
            openingSource.Should().BeSameAs(source);
            menu.Close();
        }

        host.Close();
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Native_header_right_button_messages_should_preserve_selection_without_link_activation(bool pressInsideSelection)
    {
        using Font font = new("Segoe UI", 9);
        using Form host = new() { ClientSize = new Size(640, 240), ShowInTaskbar = false };
        using RichTextBox editor = new()
        {
            Font = font,
            BorderStyle = BorderStyle.None,
            ScrollBars = RichTextBoxScrollBars.None,
            WordWrap = false,
            ReadOnly = true,
            Size = new Size(500, 100),
        };
        using ContextMenuStrip menu = new();
        menu.Items.Add("Copy link");
        editor.ContextMenuStrip = menu;
        host.Controls.Add(editor);
        Type extension = typeof(GitUI.CommitInfo.CommitInfoHeader).Assembly.GetType(
            "GitUI.Editor.RichTextBoxExtension.RichTextBoxXhtmlSupportExtension", throwOnError: true)
            ?? throw new InvalidOperationException("The original XHTML extension must be present.");
        MethodInfo loader = extension.GetMethod("SetXHTMLText", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("The original XHTML loader must be present.");
        MethodInfo getLink = extension.GetMethod("GetLink", BindingFlags.Public | BindingFlags.Static, [typeof(RichTextBox), typeof(int)])
            ?? throw new InvalidOperationException("The original link reader must be present.");
        loader.Invoke(null, [editor, $"head <a href='{Link}'>caption</a> tail"]);
        int activations = 0;
        editor.LinkClicked += (_, _) => activations++;
        host.Show();
        Application.DoEvents();
        int captionStart = editor.Text.IndexOf("caption", StringComparison.Ordinal);
        Point caption = editor.GetPositionFromCharIndex(captionStart + 3);
        caption.Y += font.Height / 2;
        editor.Focus();
        editor.Select(pressInsideSelection ? captionStart + 1 : 1, pressInsideSelection ? 4 : 2);
        int selectionStart = editor.SelectionStart;
        int selectionLength = editor.SelectionLength;
        Report("before");
        nint coordinates = (caption.Y << 16) | (caption.X & ushort.MaxValue);
        SendMessage(editor.Handle, RightButtonDown, RightButtonFlag, coordinates);
        Application.DoEvents();
        Report("right-down");
        editor.SelectionStart.Should().Be(selectionStart);
        editor.SelectionLength.Should().Be(selectionLength);
        SendMessage(editor.Handle, RightButtonUp, 0, coordinates);
        Application.DoEvents();
        Report("right-up");
        menu.Visible.Should().BeTrue();
        menu.SourceControl.Should().BeSameAs(editor);
        editor.SelectionStart.Should().Be(selectionStart);
        editor.SelectionLength.Should().Be(selectionLength);
        string? contextLink = getLink.Invoke(null, [editor, editor.GetCharIndexFromPosition(caption)]) as string;
        contextLink.Should().Be(Link);
        editor.SelectionStart.Should().Be(selectionStart);
        editor.SelectionLength.Should().Be(selectionLength);
        activations.Should().Be(0, "right-button link notifications must not activate the original LinkClicked event");
        menu.Close();
        Application.DoEvents();
        Report("after-popup");
        editor.SelectionStart.Should().Be(selectionStart);
        editor.SelectionLength.Should().Be(selectionLength);
        host.Close();

        return;

        void Report(string stage)
        {
            string? currentLink = getLink.Invoke(null, [editor, editor.GetCharIndexFromPosition(caption)]) as string;
            TestContext.Out.WriteLine($"stage={stage} insideSelection={pressInsideSelection} point={caption} selection={editor.SelectionStart}/{editor.SelectionLength} selectedText={editor.SelectedText} currentLink={currentLink ?? "<null>"} activations={activations} popupVisible={menu.Visible}");
        }
    }

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nuint firstParameter, nint secondParameter);
}
