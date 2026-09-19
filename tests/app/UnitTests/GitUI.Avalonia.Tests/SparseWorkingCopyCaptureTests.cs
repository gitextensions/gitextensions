using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using GitUI.CommandsDialogs;
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
