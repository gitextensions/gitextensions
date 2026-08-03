using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using GitExtUtils;
using GitUI;
using GitUI.CommitInfo;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CommitInfoTests
{
    [SetUp]
    public void SetUp()
        => ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

    [AvaloniaTest]
    public void XhtmlTextBlock_should_preserve_text_links_underlines_and_line_breaks()
    {
        XhtmlTextBlock block = new();
        string? activatedUri = null;
        block.LinkClicked += (_, e) => activatedUri = e.LinkUri;

        block.SetXHTMLText("Author: A &amp; B\n<u>tag</u>: <a href='gitext://gototag/v1'>v1</a><br/>done");

        block.GetPlainText().Should().Be($"Author: A & B{Environment.NewLine}tag: v1{Environment.NewLine}done");
        HyperlinkButton link = block.GetVisualDescendants().OfType<HyperlinkButton>().Single();
        link.Content.Should().Be("v1");
        ToolTip.GetTip(link).Should().Be("gitext://gototag/v1");

        link.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        activatedUri.Should().Be("gitext://gototag/v1");
    }

    [AvaloniaTest]
    public void CommitInfo_should_preserve_the_original_named_surfaces()
    {
        CommitInfo control = new();
        CommitInfo.TestAccessor accessor = control.GetTestAccessor();

        accessor.Header.Should().NotBeNull();
        accessor.Avatar.Should().NotBeNull();
        accessor.CommitMessage.Name.Should().Be("rtbxCommitMessage");
        accessor.RevisionInfo.Name.Should().Be("RevisionInfo");
    }

    [AvaloniaTest]
    public void CommitInfoHeader_should_preserve_the_original_named_surfaces()
    {
        CommitInfoHeader header = new();
        CommitInfoHeader.TestAccessor accessor = header.GetTestAccessor();

        accessor.Avatar.Name.Should().Be("avatarControl");
        accessor.RevisionHeader.Name.Should().Be("rtbRevisionHeader");
    }
}
