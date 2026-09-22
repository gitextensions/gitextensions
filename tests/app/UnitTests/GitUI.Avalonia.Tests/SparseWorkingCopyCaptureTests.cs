using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using AwesomeAssertions;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI.CommandsDialogs;
using GitUI.Editor;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitExtensionsTests;

[TestFixture]
public sealed class SparseWorkingCopyCaptureTests
{
    [AvaloniaTest]
    [Category("P8_6i")]
    public void Capture_reader_should_not_identify_default_button_storage_as_source_fields()
    {
        using FormSparseWorkingCopy form = new();
        Button save = new() { Content = "Save" };
        Button cancel = new() { Content = "Cancel" };
        form.Content = new StackPanel { Children = { save, cancel } };
        form.AcceptButton = save;
        form.CancelButton = cancel;
        form.Show();
        try
        {
            CaptureNode root = new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new Avalonia.PixelSize(300, 200)).Root;

            Flatten(root).Should().NotContain(node => node.FieldName == "_acceptButton" || node.FieldName == "_cancelButton");
            Flatten(root).Count(node => node.ControlKind == "button").Should().Be(2);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8_6i")]
    public void Capture_reader_should_preserve_the_FileViewer_readonly_contract()
    {
        FileViewer viewer = new() { IsReadOnly = false };
        Window window = new() { Width = 320, Height = 200, Content = viewer };
        window.Show();
        try
        {
            CaptureNode root = new AvaloniaControlTreeReader(window, renderScale: 1)
                .ReadPrimary(window, new Avalonia.PixelSize(320, 200)).Root;

            Flatten(root).Single(node => node.Type == "GitUI.Editor.FileViewer").ReadOnly.Should().BeFalse();
            Flatten(root).Single(node => node.FieldName == "internalFileViewer").ReadOnly.Should().BeFalse();
            Flatten(root).Single(node => node.FieldName == "TextEditor").ReadOnly.Should().BeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8_6i")]
    public void Capture_reader_should_emit_the_code_built_sparse_link_as_one_source_control()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.Combine(Path.GetTempPath(), "ge-sparse-capture"));
        module.ResolveGitInternalPath("info").Returns(Path.Combine(Path.GetTempPath(), "ge-sparse-capture", ".git", "info"));
        module.GetEffectiveSetting(FormSparseWorkingCopyViewModel.SettingCoreSparseCheckout).Returns((string?)null);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        using FormSparseWorkingCopy form = new(commands);
        form.Show();
        try
        {
            CaptureNode root = new AvaloniaControlTreeReader(form, renderScale: 1)
                .ReadPrimary(form, new Avalonia.PixelSize(784, 561)).Root;

            CaptureNode link = Flatten(root).Single(node => node.Type == "System.Windows.Forms.LinkLabel");
            link.Text.Should().Be("Git Sparse feature is currently enabled. Disable for this repository");
            link.Children.Should().BeEmpty();
        }
        finally
        {
            form.Close();
        }
    }

    private static IEnumerable<CaptureNode> Flatten(CaptureNode node)
    {
        yield return node;
        foreach (CaptureNode child in node.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }
}
