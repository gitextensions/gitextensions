using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.ParityCapture;
using GitUI;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class MainWindowToolStripLayoutNativeTests
{
    [TestCase(1800, 9, false)]
    [TestCase(923, 9, false)]
    [TestCase(600, 9, false)]
    [TestCase(1800, 11, false)]
    [TestCase(1800, 14, false)]
    [TestCase(1800, 9, true)]
    [TestCase(923, 11, true)]
    [TestCase(600, 14, true)]
    public void Source_row_should_use_actual_owner_preferred_sizes_and_shrink_rightmost_owners_first(int width, int points, bool explicitOwnerFont)
    {
        using Font font = new("Segoe UI", points);
        using Bitmap image = new(16, 16);
        using Form host = new() { ClientSize = new Size(923, 180), Font = font, StartPosition = FormStartPosition.Manual, Location = new Point(16, 16) };
        using ToolStripContainer container = new() { Dock = DockStyle.Fill, Padding = Padding.Empty };
        container.TopToolStripPanel.Padding = new Padding(4, 0, 4, 0);
        host.Controls.Add(container);
        using ToolStripEx main = NewStrip("ToolStripMain");
        using ToolStripEx filters = NewStrip("ToolStripFilters");
        using ToolStripEx scripts = NewStrip("ToolStripScripts");
        main.SuspendLayout();
        filters.SuspendLayout();
        scripts.SuspendLayout();
        scripts.GripStyle = ToolStripGripStyle.Hidden;
        if (explicitOwnerFont)
        {
            main.Font = font;
            filters.Font = font;
        }

        ToolStripEx[] owners = [main, filters, scripts];
        InitializeSourceToolbars(host, container, owners);
        container.SuspendLayout();
        container.TopToolStripPanel.SuspendLayout();
        main.Items.Add(new ToolStripButton { AutoSize = false, Size = new Size(900, 22), Text = "Fixed allocation" });
        main.Items.Add(new ToolStripSplitButton("Avalonia_Port", image));
        filters.Items.Add(new ToolStripButton { AutoSize = false, Size = new Size(500, 22), Text = "Fixed filter allocation" });
        container.TopToolStripPanel.ResumeLayout();
        container.ResumeLayout();
        host.Show();
        Settle(host);
        host.ClientSize = new Size(1800, 180);
        Settle(host);
        AssertSourceRow(host, container, owners, new { stage = "sourceInitializedThenWide", requestedWidth = width, points, explicitOwnerFont });
        host.ClientSize = new Size(width, 180);
        Settle(host);
        AssertSourceRow(host, container, owners, new { stage = "requestedViewport", requestedWidth = width, points, explicitOwnerFont });
        host.DeviceDpi.Should().Be(96);
        scripts.Items.Cast<ToolStripItem>().Should().BeEmpty();
        scripts.GetPreferredSize(Size.Empty).Should().Be(new Size(102, 25), "native DefaultSize100x25 plus the no-overflow two-pixel edge, with no hidden-grip reserve");
        scripts.GripRectangle.Width.Should().Be(0);
        main.Font.SizeInPoints.Should().Be(explicitOwnerFont ? points : 9);
        foreach (int resizedWidth in new[] { 923, 600, 1800 })
        {
            host.ClientSize = new Size(resizedWidth, 180);
            Settle(host);
            AssertSourceRow(host, container, owners, new { stage = "resizeAndRegrow", resizedWidth, points, explicitOwnerFont });
        }
    }

    [TestCase("Avalonia_Port")]
    [TestCase("feature&&docs")]
    [TestCase("Translated &branch_name")]
    public void Source_branch_caption_should_keep_literal_underscores_and_original_ampersand_mnemonics(string caption)
    {
        using Font font = new("Segoe UI", 9);
        using Bitmap image = new(16, 16);
        using Form host = new() { ClientSize = new Size(400, 100), StartPosition = FormStartPosition.Manual, Location = new Point(16, 16) };
        using ToolStripEx strip = NewStrip("ToolStripMain");
        strip.Dock = DockStyle.Top;
        ToolStripSplitButton branch = new(caption, image) { Name = "branchSelect", Font = font };
        strip.Items.Add(branch);
        host.Controls.Add(strip);
        host.Show();
        Settle(host);
        Size measured = TextRenderer.MeasureText(caption, font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        RetainFrame(host, null, [strip], [strip.GetPreferredSize(Size.Empty)], new { caption, measured, branch.Bounds, preferred = branch.GetPreferredSize(Size.Empty) });
        branch.GetPreferredSize(Size.Empty).Width.Should().Be(measured.Width + 32);
        if (caption == "Avalonia_Port")
        {
            branch.Bounds.Width.Should().Be(112);
        }
    }

    [Test]
    public void Source_row_should_reclaim_hidden_owner_width_and_follow_dynamic_script_preferred_size()
    {
        using Form host = new() { ClientSize = new Size(923, 120), StartPosition = FormStartPosition.Manual, Location = new Point(16, 16) };
        using ToolStripContainer container = new() { Dock = DockStyle.Fill };
        container.TopToolStripPanel.Padding = new Padding(4, 0, 4, 0);
        using ToolStripEx main = NewStrip("ToolStripMain");
        using ToolStripEx filters = NewStrip("ToolStripFilters");
        using ToolStripEx scripts = NewStrip("ToolStripScripts");
        main.SuspendLayout();
        filters.SuspendLayout();
        scripts.SuspendLayout();
        scripts.GripStyle = ToolStripGripStyle.Hidden;
        host.Controls.Add(container);
        ToolStripEx[] owners = [main, filters, scripts];
        InitializeSourceToolbars(host, container, owners);
        container.SuspendLayout();
        container.TopToolStripPanel.SuspendLayout();
        main.Items.Add(new ToolStripButton { AutoSize = false, Size = new Size(900, 22) });
        filters.Items.Add(new ToolStripButton { AutoSize = false, Size = new Size(500, 22) });
        container.TopToolStripPanel.ResumeLayout();
        container.ResumeLayout();
        host.Show();
        Settle(host);
        host.ClientSize = new Size(1800, 120);
        Settle(host);
        AssertSourceRow(host, container, owners, new { stage = "beforeHide" });
        filters.Hide();
        Settle(host);
        RetainFrame(host, container, owners, owners.Select(owner => owner.GetPreferredSize(Size.Empty)).ToArray(), new { stage = "afterHide" });
        scripts.Left.Should().Be(main.Right);
        ToolStripButton script = new("Review") { Tag = "userscript" };
        scripts.Items.Add(script);
        scripts.GripStyle = ToolStripGripStyle.Visible;
        Settle(host);
        scripts.Width.Should().Be(script.GetPreferredSize(Size.Empty).Width + script.Margin.Horizontal + scripts.GripRectangle.Width + 2);
        RetainFrame(host, container, [main, filters, scripts], [main.GetPreferredSize(Size.Empty), filters.GetPreferredSize(Size.Empty), scripts.GetPreferredSize(Size.Empty)], new { stage = "hiddenFiltersDynamicScript" });
        scripts.Items.Remove(script);
        script.Dispose();
        scripts.GripStyle = ToolStripGripStyle.Hidden;
        Settle(host);
        scripts.Width.Should().Be(102);
    }

    [TestCase(600)]
    [TestCase(923)]
    public void Direct_narrow_source_authored_locations_should_record_actual_native_row_membership(int width)
    {
        using Form host = new() { ClientSize = new Size(width, 120), StartPosition = FormStartPosition.Manual, Location = new Point(16, 16) };
        using ToolStripContainer container = new() { Dock = DockStyle.Fill };
        container.TopToolStripPanel.Padding = new Padding(4, 0, 4, 0);
        using ToolStripEx main = NewStrip("ToolStripMain");
        using ToolStripEx filters = NewStrip("ToolStripFilters");
        using ToolStripEx scripts = NewStrip("ToolStripScripts");
        main.SuspendLayout();
        filters.SuspendLayout();
        scripts.SuspendLayout();
        scripts.GripStyle = ToolStripGripStyle.Hidden;
        host.Controls.Add(container);
        ToolStripEx[] owners = [main, filters, scripts];
        InitializeSourceToolbars(host, container, owners);
        container.SuspendLayout();
        container.TopToolStripPanel.SuspendLayout();
        main.Items.Add(new ToolStripButton { AutoSize = false, Size = new Size(900, 22) });
        filters.Items.Add(new ToolStripButton { AutoSize = false, Size = new Size(500, 22) });
        container.TopToolStripPanel.ResumeLayout();
        container.ResumeLayout();
        host.Show();
        Settle(host);
        RetainFrame(host, container, owners, owners.Select(owner => owner.GetPreferredSize(Size.Empty)).ToArray(), new { stage = "directNarrowConstruction", width });
        ToolStripPanelRow[] rows = container.TopToolStripPanel.Rows;
        rows.SelectMany(row => row.Controls).Should().BeEquivalentTo(owners);
        foreach (ToolStripPanelRow row in rows)
        {
            row.Controls.Should().NotBeEmpty();
            row.Controls.Select(owner => owner.Top).Distinct().Should().ContainSingle();
            row.Bounds.Height.Should().Be(row.Controls.Max(owner => owner.Height));
        }

        container.TopToolStripPanel.Height.Should().Be(rows.Sum(row => row.Bounds.Height));
        container.ContentPanel.Top.Should().Be(container.TopToolStripPanel.Height);
    }

    private static ToolStripEx NewStrip(string name) => new()
    {
        Name = name,
        Dock = DockStyle.None,
        DrawBorder = false,
        GripEnabled = false,
        GripMargin = Padding.Empty,
        Margin = Padding.Empty,
        Padding = Padding.Empty,
        Location = name switch
        {
            "ToolStripMain" => new Point(3, 0),
            "ToolStripFilters" => new Point(584, 0),
            "ToolStripScripts" => new Point(890, 0),
            _ => Point.Empty,
        },
        LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow,
    };

    private static void InitializeSourceToolbars(Form host, ToolStripContainer container, ToolStripEx[] owners)
    {
        host.SuspendLayout();
        container.SuspendLayout();
        ToolStripPanel top = container.TopToolStripPanel;
        top.SuspendLayout();
        container.Size = host.ClientSize;
        container.BottomToolStripPanelVisible = false;
        container.LeftToolStripPanelVisible = false;
        container.RightToolStripPanelVisible = false;
        owners[0].Size = new Size(479, 25);
        owners[1].Size = new Size(339, 25);
        owners[2].Size = new Size(43, 25);
        top.Controls.Add(owners[0]);
        top.Controls.Add(owners[1]);
        top.Controls.Add(owners[2]);
        owners[0].ResumeLayout(false);
        top.ResumeLayout(false);
        top.PerformLayout();
        container.ResumeLayout(false);
        container.PerformLayout();
        owners[1].ResumeLayout(false);
        owners[1].PerformLayout();
        owners[2].ResumeLayout(false);
        owners[2].PerformLayout();
        host.ResumeLayout(false);
        host.PerformLayout();

        // Match FormBrowse.InitMenusAndToolbars' existing-owner workaround after Designer
        // initialization; fresh zero-origin Controls.Add calls are a different Join route.
        top.Controls.Clear();
        foreach (ToolStripEx owner in owners.Reverse())
        {
            top.Controls.Add(owner);
        }
    }

    private static void AssertSourceRow(Form host, ToolStripContainer container, ToolStripEx[] owners, object inputs)
    {
        Size[] preferred = owners.Select(owner => owner.GetPreferredSize(Size.Empty)).ToArray();
        RetainFrame(host, container, owners, preferred, inputs);
        container.TopToolStripPanel.Rows.Should().ContainSingle("the actual Browse initialization workaround requires all three owners on the first row");
        owners.Select(owner => owner.Top).Should().OnlyContain(top => top == 0);
        int[] expectedWidths = preferred.Select(size => size.Width).ToArray();
        int excess = expectedWidths.Sum() - (host.ClientSize.Width - container.TopToolStripPanel.Padding.Horizontal - container.TopToolStripPanel.RowMargin.Horizontal);
        for (int i = expectedWidths.Length - 1; i >= 0 && excess > 0; i--)
        {
            int freed = Math.Min(excess, Math.Max(0, expectedWidths[i] - 50));
            expectedWidths[i] -= freed;
            excess -= freed;
        }

        owners.Select(owner => owner.Width).Should().Equal(expectedWidths);
        owners[0].Left.Should().Be(container.TopToolStripPanel.Padding.Left + container.TopToolStripPanel.RowMargin.Left);
        owners[1].Left.Should().Be(owners[0].Right);
        owners[2].Left.Should().Be(owners[1].Right);
        container.TopToolStripPanel.Height.Should().Be(preferred.Max(size => size.Height));
        container.ContentPanel.Top.Should().Be(container.TopToolStripPanel.Height);
        container.ContentPanel.Height.Should().Be(container.Height - container.TopToolStripPanel.Height);
    }

    private static void Settle(Form host)
    {
        Application.DoEvents();
        host.PerformLayout();
        host.Refresh();
        Application.DoEvents();
    }

    private static void RetainFrame(Form host, ToolStripContainer? container, ToolStripEx[] owners, Size[] preferred, object inputs)
    {
        string directory = Path.Combine(TestContext.CurrentContext.TestDirectory, "MainWindowToolStripLayoutProbe", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        using CaptureImageResult frame = ImageCapture.Capture(host, [], []);
        frame.Bitmap.Save(Path.Combine(directory, "actual-owner.png"), ImageFormat.Png);
        string json = JsonSerializer.Serialize(new
        {
            inputs,
            host.ClientSize,
            host.DeviceDpi,
            captureMethod = frame.Method.ToString(),
            frame.ScreenBounds,
            rowBounds = container?.TopToolStripPanel.Bounds,
            rowPadding = container?.TopToolStripPanel.Padding,
            rowMargin = container?.TopToolStripPanel.RowMargin,
            rows = container?.TopToolStripPanel.Rows.Select(row => new
            {
                row.Bounds,
                row.Margin,
                controls = row.Controls.Select(control => new { control.Name, control.Bounds }).ToArray(),
            }).ToArray(),
            contentBounds = container?.ContentPanel.Bounds,
            owners = owners.Select((owner, index) => new
            {
                owner.Name,
                owner.Bounds,
                owner.Visible,
                owner.GripStyle,
                owner.GripRectangle,
                preferredSize = preferred[index],
                font = new { owner.Font.Name, owner.Font.SizeInPoints, owner.Font.Style },
                items = owner.Items.Cast<ToolStripItem>().Select(item => new { item.Name, item.Text, item.Bounds, item.Margin, placement = item.Placement.ToString() }).ToArray(),
            }).ToArray(),
            note = "Actual ToolStripEx/ToolStripPanel source algorithms and native100 layout only; this source-shaped subset is not a full FormBrowse or filter behavior/raster parity claim.",
        }, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(directory, "probe.json"), json + Environment.NewLine);
        TestContext.Out.WriteLine($"mainWindowToolStripLayoutEvidence={directory}");
        TestContext.Out.WriteLine(json);
        frame.Method.Should().Be(CaptureMethod.PrintWindow);
    }
}
