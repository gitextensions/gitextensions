using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.ParityCapture;
using GitUI.CommandsDialogs;

namespace GitExtensionsTests;

[TestFixture]
[Category("P8_6i")]
public sealed class BranchDialogCaptureTests
{
    [AvaloniaTest]
    public void Capture_reader_should_emit_branch_dialog_runtime_layout_contracts()
    {
        FormCreateBranch create = new();
        FormCheckoutBranch checkout = new();
        FormDeleteBranch delete = new();
        FormRenameBranch rename = new();
        try
        {
            IReadOnlyDictionary<string, CaptureNode> createNodes = ReadNodes(create);
            createNodes["tableLayout"].Margin.Dip.Left.Should().Be(0);
            createNodes["tableLayout"].Dock.Should().Be("Fill");
            createNodes["tableLayout"].AutoSize.Should().BeTrue();

            IReadOnlyDictionary<string, CaptureNode> checkoutNodes = ReadNodes(checkout);
            checkoutNodes["tlpnlBranches"].Anchor.Should().Equal("Top", "Left", "Right");
            checkoutNodes["tlpnlBranches"].AutoSize.Should().BeTrue();
            checkoutNodes["horLine"].ClientSizeDip.Should().Be(new CaptureSizeF { Width = 690, Height = 0 });
            checkoutNodes["lbChanges"].Text.Should().Be("=");

            IReadOnlyDictionary<string, CaptureNode> deleteNodes = ReadNodes(delete);
            deleteNodes["tlpnlMain"].Margin.Dip.Left.Should().Be(0);
            deleteNodes["tlpnlMain"].Dock.Should().Be("Fill");
            deleteNodes["tlpnlMain"].AutoSize.Should().BeTrue();

            IReadOnlyDictionary<string, CaptureNode> renameNodes = ReadNodes(rename);
            renameNodes["label1"].BoundsDip.Should().Be(
                new CaptureRectangleF { X = 3, Y = 10, Width = 63, Height = 21 });
            renameNodes["Ok"].BoundsDip.Should().Be(
                new CaptureRectangleF { X = 422, Y = 7, Width = 59, Height = 28 });
        }
        finally
        {
            create.Close();
            checkout.Close();
            delete.Close();
            rename.Close();
        }
    }

    private static IReadOnlyDictionary<string, CaptureNode> ReadNodes(Window form)
    {
        Size availableSize = new(
            double.IsNaN(form.Width) ? double.PositiveInfinity : form.Width,
            double.IsNaN(form.Height) ? double.PositiveInfinity : form.Height);
        form.Measure(availableSize);
        Size arrangedSize = new(
            double.IsNaN(form.Width) ? form.DesiredSize.Width : form.Width,
            double.IsNaN(form.Height) ? form.DesiredSize.Height : form.Height);
        form.Arrange(new Rect(arrangedSize));
        Dispatcher.UIThread.RunJobs();
        CaptureSurface surface = new AvaloniaControlTreeReader(form, renderScale: 1)
            .ReadPrimary(form, PixelSize.FromSize(form.ClientSize, 1));
        return Flatten(surface.Root)
            .Where(node => node.FieldName is not null || node.Name is not null)
            .GroupBy(node => node.FieldName ?? node.Name!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    private static IEnumerable<CaptureNode> Flatten(CaptureNode root)
    {
        yield return root;
        foreach (CaptureNode child in root.Children)
        {
            foreach (CaptureNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }
}
