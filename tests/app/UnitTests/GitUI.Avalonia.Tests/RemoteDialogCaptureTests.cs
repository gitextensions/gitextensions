using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.ParityCapture;
using GitUI.CommandsDialogs;

namespace GitExtensionsTests;

[TestFixture]
[Category("P8_6i")]
public sealed class RemoteDialogCaptureTests
{
    [AvaloniaTest]
    public void Capture_reader_should_emit_remote_dialog_Designer_layout_contracts()
    {
        AssertFormPull();
        AssertFormPush();
        AssertFormRemotes();
    }

    [AvaloniaTest]
    public void Capture_reader_should_emit_the_visible_text_of_a_non_editable_combo_selection()
    {
        ComboBox comboBox = new()
        {
            Name = "RemoteRepositoryCombo",
            ItemsSource = new[] { string.Empty, "origin" },
            SelectedItem = "origin",
        };
        Window window = new() { Content = comboBox };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            CaptureNode node = new AvaloniaControlTreeReader(window, renderScale: 1)
                .ReadPrimary(window, PixelSize.FromSize(window.ClientSize, 1))
                .Root.Children.Single(child => child.Name == "RemoteRepositoryCombo");

            node.Text.Should().Be("origin");
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertFormPull()
    {
        FormPull form = new();
        try
        {
            IReadOnlyDictionary<string, CaptureNode> nodes = ReadNodes(form);

            nodes["MainLayout"].Dock.Should().Be("Fill");
            nodes["GroupPullFrom"].Anchor.Should().Equal("Top", "Left", "Right");
            nodes["GroupPullFrom"].AutoSize.Should().BeTrue();
            nodes["AddRemote"].Anchor.Should().Equal("Top", "Right");
            nodes["AddRemote"].FlatStyle.Should().Be("Standard");
            nodes["PullFromRemote"].FlatStyle.Should().Be("Standard");
            nodes["PullFromRemote"].Alignment.Should().Be("MiddleLeft");
            nodes["PullFromRemote"].CornerRadiusDip.Should().BeNull();
            nodes["PullFromRemote"].CheckState.Should().BeNull();
        }
        finally
        {
            form.Close();
        }
    }

    private static void AssertFormPush()
    {
        FormPush form = new();
        try
        {
            IReadOnlyDictionary<string, CaptureNode> nodes = ReadNodes(form);

            nodes["groupBox2"].Anchor.Should().Equal("Top", "Left", "Right");
            nodes["BranchGrid"].Dock.Should().Be("Fill");
            nodes["ShowOptions"].AutoSize.Should().BeTrue();
            nodes["Push"].Anchor.Should().Equal("Bottom", "Right");
            nodes["PushToRemote"].FlatStyle.Should().Be("Standard");
            nodes["PushToRemote"].CheckState.Should().BeNull();
        }
        finally
        {
            form.Close();
        }
    }

    private static void AssertFormRemotes()
    {
        FormRemotes form = new();
        try
        {
            IReadOnlyDictionary<string, CaptureNode> nodes = ReadNodes(form);

            nodes["Remotes"].Dock.Should().Be("Fill");
            nodes["gbMgtPanel"].Dock.Should().Be("Top");
            nodes["gbMgtPanel"].AutoSize.Should().BeTrue();
            nodes["label1"].Alignment.Should().Be("MiddleLeft");
            nodes["Remotes"].BorderStyle.Should().Be("Fixed3D");
            nodes["flowLayoutPanelSsh"].Children
                .Select(child => child.FieldName)
                .Should().Equal("TestConnection", "LoadSSHKey");
            nodes["lblHeaderLine2"].Colors.Border.Should().BeNull();
            nodes["RemoteBranches"].Colors.GridLine.Should().Be("#FFE3E3E3");
            nodes["RemoteBranches"].Colors.SelectionBackground.Should().Be("#FF0078D4");
            nodes["RemoteBranches"].Colors.InactiveSelectionForeground.Should().Be("#FFFFFFFF");
            form.FindControl<ListBox>("Remotes")!.Classes.Should().Contain("gitextensions-native-list-items");
            foreach (string buttonName in new[] { "New", "Delete", "btnToggleState", "Save" })
            {
                form.FindControl<Button>(buttonName)!.Classes.Should().Contain("gitextensions-native-dialog-action");
            }
        }
        finally
        {
            form.Close();
        }
    }

    private static IReadOnlyDictionary<string, CaptureNode> ReadNodes(Window form)
    {
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
