using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;

namespace GitExtensionsTests;

[TestFixture]
[Category("P8_6i")]
public sealed class RepositoryWorkflowCaptureTests
{
    [AvaloniaTest]
    public void Capture_reader_should_emit_source_system_colors_for_patch_and_merge_inputs()
    {
        AvaloniaThemeSettings.ApplyAppSettings();
        FormMergeBranch merge = new() { RequestedThemeVariant = ThemeVariant.Light };
        FormRebase rebase = new() { RequestedThemeVariant = ThemeVariant.Light };
        try
        {
            merge.Measure(new Size(783, 424));
            merge.Arrange(new Rect(0, 0, 783, 424));
            rebase.Measure(new Size(1034, 461));
            rebase.Arrange(new Rect(0, 0, 1034, 461));
            Dispatcher.UIThread.RunJobs();

            IReadOnlyDictionary<string, CaptureNode> mergeNodes = ReadNodes(merge);
            mergeNodes["nbMessages"].Colors.Background.Should().Be("#FFFFFFFF");
            mergeNodes["nbMessages"].Colors.DisabledBackground.Should().Be("#FFFFFFFF");
            mergeNodes["mergeMessage"].Colors.Background.Should().Be("#FFFFFFFF");
            mergeNodes["mergeMessage"].Colors.DisabledBackground.Should().Be("#FFFFFFFF");

            IReadOnlyDictionary<string, CaptureNode> patchNodes = ReadNodes(rebase);
            patchNodes["Patches"].Colors.Background.Should().Be("#FFE3E3E3");
            patchNodes["Patches"].Colors.SelectionBackground.Should().Be("#FF0078D7");

            CaptureNode[] sourceLocalPanels = Flatten(ReadSurface(rebase).Root)
                .Where(node => node.FieldName is null
                    && node.Name is "PanelCurrentBranch" or "flowLayoutPanel1" or "flowLayoutPanel2")
                .ToArray();
            sourceLocalPanels.Should().HaveCount(3);
            sourceLocalPanels.Should().OnlyContain(node => node.Anchor.SequenceEqual(new[] { "Top", "Left" }));
            sourceLocalPanels.Should().OnlyContain(node => node.Dock == "Fill");
            sourceLocalPanels.Should().OnlyContain(node => node.AutoSize == true);
            sourceLocalPanels.Should().OnlyContain(node => node.BorderStyle == "None");
        }
        finally
        {
            rebase.Close();
            merge.Close();
        }
    }

    [AvaloniaTest]
    public void Capture_reader_should_preserve_source_workflow_container_and_focus_order()
    {
        FormArchive archive = new();
        FormCherryPick cherryPick = new();
        try
        {
            archive.Measure(new Size(594, 571));
            archive.Arrange(new Rect(0, 0, 594, 571));
            cherryPick.Measure(new Size(614, double.PositiveInfinity));
            cherryPick.Arrange(new Rect(0, 0, 614, cherryPick.DesiredSize.Height));
            Dispatcher.UIThread.RunJobs();

            CaptureNode archiveRoot = ReadSurface(archive).Root;
            CaptureNode archiveFilters = FindNode(archiveRoot, "groupBox2");
            string[] archiveFilterChildren = archiveFilters.Children
                .Select(node => node.FieldName)
                .Where(name => name is not null)
                .Cast<string>()
                .ToArray();
            Array.IndexOf(archiveFilterChildren, "checkboxRevisionFilter")
                .Should().BeLessThan(Array.IndexOf(archiveFilterChildren, "checkBoxPathFilter"));

            CaptureNode cherryRoot = ReadSurface(cherryPick).Root;
            cherryRoot.Children.Select(node => node.TabIndex).Should().Equal(0, 1);
            cherryRoot.Children.Should().OnlyContain(node => node.AutoSize == true);
            CaptureNode mainLayout = FindNode(cherryRoot, "tlPnlMain");
            FindNode(mainLayout, "lvParentsList").Should().BeSameAs(
                mainLayout.Children.Single(node => node.FieldName == "lvParentsList"));
        }
        finally
        {
            cherryPick.Close();
            archive.Close();
        }
    }

    [AvaloniaTest]
    public void Normal_capture_should_restore_the_source_default_focus()
    {
        FormArchive form = new();
        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();

            using AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(
                form,
                new CaptureStatePlan { Id = "normal", Kind = CaptureStateKind.Normal });

            form.FindControl<Button>("buttonArchiveRevision")!.IsFocused.Should().BeTrue();
            form.FindControl<CheckBox>("checkBoxPathFilter")!.IsFocused.Should().BeFalse();
        }
        finally
        {
            form.Close();
        }
    }

    private static IReadOnlyDictionary<string, CaptureNode> ReadNodes(Window window)
    {
        CaptureSurface surface = ReadSurface(window);
        return Flatten(surface.Root)
            .Where(node => node.FieldName is not null || node.Name is not null)
            .GroupBy(node => node.FieldName ?? node.Name!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    private static CaptureSurface ReadSurface(Window window)
        => new AvaloniaControlTreeReader(window, renderScale: 1)
            .ReadPrimary(window, PixelSize.FromSize(window.ClientSize, 1));

    private static CaptureNode FindNode(CaptureNode root, string fieldName)
        => Flatten(root).Single(node => node.FieldName == fieldName);

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
