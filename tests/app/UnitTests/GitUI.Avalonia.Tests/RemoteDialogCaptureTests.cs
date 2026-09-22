using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;

namespace GitExtensionsTests;

[TestFixture]
[Category("P8_6i")]
public sealed class RemoteDialogCaptureTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void Capture_reader_should_emit_remote_dialog_Designer_layout_contracts()
    {
        AvaloniaThemeSettings.ApplyAppSettings();
        AssertFormPull();
        AssertFormPush();
        AssertFormRemotes();
        AssertFormDeleteRemoteBranch();
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

            form.UseLayoutRounding.Should().BeFalse();
            nodes["MainPanel"].Padding.Dip.Should().Be(new CaptureThicknessF
            {
                Left = 9,
                Top = 9,
                Right = 9,
                Bottom = 9,
            });
            nodes["MainLayout"].Dock.Should().Be("Fill");
            nodes["GroupPullFrom"].Anchor.Should().Equal("Top", "Left", "Right");
            nodes["GroupPullFrom"].AutoSize.Should().BeTrue();
            nodes["AddRemote"].Anchor.Should().Equal("Top", "Right");
            nodes["AddRemote"].FlatStyle.Should().Be("Standard");
            nodes["PullFromRemote"].FlatStyle.Should().Be("Standard");
            nodes["PullFromRemote"].Alignment.Should().Be("MiddleLeft");
            nodes["PullFromRemote"].CornerRadiusDip.Should().BeNull();
            nodes["PullFromRemote"].CheckState.Should().BeNull();
            nodes["MainLayout"].TabStop.Should().BeFalse();
            nodes["PanelRightInner"].TabStop.Should().BeFalse();
            nodes["PanelMergeOptions"].TabStop.Should().BeFalse();
            nodes["PanelTagOptions"].TabStop.Should().BeFalse();
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

            form.UseLayoutRounding.Should().BeFalse();
            nodes["groupBox2"].Anchor.Should().Equal("Top", "Left", "Right");
            nodes["BranchGrid"].Dock.Should().Be("Fill");
            nodes["ShowOptions"].AutoSize.Should().BeTrue();
            nodes["Push"].Anchor.Should().Equal("Bottom", "Right");
            nodes["PushToRemote"].FlatStyle.Should().Be("Standard");
            nodes["PushToRemote"].CheckState.Should().BeNull();
            nodes["tableLayoutPanel1"].TabStop.Should().BeFalse();
            nodes["BranchGrid"].ReadOnly.Should().BeFalse();
            nodes["BranchGrid"].Colors.Background.Should().Be("#FFA0A0A0");
            nodes["BranchGrid"].Colors.SelectionBackground.Should().Be("#FF0078D7");
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
            form.GetTestAccessor().RemoteColor.Color = Color.FromArgb(255, 123, 63, 178);
            IReadOnlyDictionary<string, CaptureNode> nodes = ReadNodes(form);

            form.UseLayoutRounding.Should().BeFalse();
            nodes["Remotes"].Dock.Should().Be("Fill");
            nodes["gbMgtPanel"].Dock.Should().Be("Top");
            nodes["gbMgtPanel"].AutoSize.Should().BeTrue();
            nodes["label1"].Alignment.Should().Be("MiddleLeft");
            nodes["Remotes"].BorderStyle.Should().Be("Fixed3D");
            nodes["Remotes"].ControlKind.Should().Be("list");
            nodes["Remotes"].Text.Should().BeEmpty();
            nodes["Remotes"].Selected.Should().BeNull();
            nodes["tabControl1"].Focused.Should().BeFalse();
            nodes["flpnlRemoteManagement"].TabStop.Should().BeFalse();
            nodes["flowLayoutPanel2"].TabStop.Should().BeFalse();
            nodes["tblpnlMgtDetails"].TabStop.Should().BeFalse();
            nodes["tableLayoutPanel2"].TabStop.Should().BeFalse();
            nodes["flowLayoutPanelSsh"].Children
                .Select(child => child.FieldName)
                .Should().Equal("TestConnection", "LoadSSHKey");
            nodes["lblHeaderLine2"].Colors.Border.Should().BeNull();
            nodes["RemoteBranches"].Colors.GridLine.Should().Be("#FFE3E3E3");
            nodes["RemoteBranches"].Colors.SelectionBackground.Should().Be("#FF0078D7");
            nodes["RemoteBranches"].Colors.InactiveSelectionForeground.Should().Be("#FFFFFFFF");
            nodes["btnRemoteColor"].Colors.Background.Should().Be("#FF7B3FB2");
            nodes["btnRemoteColor"].Colors.DisabledBackground.Should().Be("#FF7B3FB2");
            nodes["RemoteBranches"].ReadOnly.Should().BeTrue();

            CaptureNode scaledRoot = ReadSurface(form, renderScale: 1.25).Root;
            scaledRoot.Margin.Px.Should().Be(new CaptureThickness { Left = 3, Top = 3, Right = 3, Bottom = 3 });
            scaledRoot.Margin.Dip.Should().Be(new CaptureThicknessF { Left = 2.4m, Top = 2.4m, Right = 2.4m, Bottom = 2.4m });
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

    private static void AssertFormDeleteRemoteBranch()
    {
        FormDeleteRemoteBranch form = new();
        try
        {
            IReadOnlyDictionary<string, CaptureNode> nodes = ReadNodes(form);

            form.UseLayoutRounding.Should().BeFalse();
            nodes["MainPanel"].AutoSize.Should().BeTrue();
            nodes["MainPanel"].Dock.Should().Be("Fill");
            nodes["ControlsPanel"].AutoSize.Should().BeTrue();
            nodes["ControlsPanel"].Dock.Should().Be("Bottom");
            nodes["Branches"].BorderStyle.Should().Be("None");
            nodes["Branches"].TabStop.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    private static IReadOnlyDictionary<string, CaptureNode> ReadNodes(Window form)
    {
        Dispatcher.UIThread.RunJobs();
        CaptureSurface surface = ReadSurface(form, renderScale: 1);
        return Flatten(surface.Root)
            .Where(node => node.FieldName is not null || node.Name is not null)
            .GroupBy(node => node.FieldName ?? node.Name!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
    }

    private static CaptureSurface ReadSurface(Window form, double renderScale)
        => new AvaloniaControlTreeReader(form, renderScale)
            .ReadPrimary(form, PixelSize.FromSize(form.ClientSize, renderScale));

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
