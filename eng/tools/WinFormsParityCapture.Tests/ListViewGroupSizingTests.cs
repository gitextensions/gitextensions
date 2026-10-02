using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using AwesomeAssertions;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
public sealed class ListViewGroupSizingTests
{
    private const uint GetGroupRectMessage = 0x1000 + 98;

    [Test]
    [TestCase(9)]
    [TestCase(12)]
    [TestCase(18)]
    public void Repository_groups_should_expose_their_native_header_and_label_bounds(int sizeInPoints)
    {
        Application.EnableVisualStyles();
        using Font font = new("Segoe UI", sizeInPoints);
        using ImageList images = new() { ImageSize = new Size(32, 32) };
        using Bitmap image = new(32, 32);
        images.Images.Add(image);
        using Form window = new() { ClientSize = new Size(445, 400), ShowInTaskbar = false, AutoScaleMode = AutoScaleMode.None };
        using GitUI.UserControls.NativeListView list = new()
        {
            Font = font, Dock = DockStyle.Fill, View = View.Tile, BorderStyle = BorderStyle.None,
            TileSize = new Size(350, 50), LargeImageList = images, OwnerDraw = true,
            HeaderStyle = ColumnHeaderStyle.None, UseCompatibleStateImageBehavior = false,
        };
        ListViewGroup recent = new("Recent repositories") { CollapsedState = ListViewGroupCollapsedState.Expanded, TaskLink = "Actions" };
        ListViewGroup development = new("Development") { CollapsedState = ListViewGroupCollapsedState.Expanded, TaskLink = "Actions" };
        list.Groups.AddRange([recent, development]);
        list.Items.Add(new ListViewItem("recent", recent) { ImageIndex = 0 });
        list.Items.Add(new ListViewItem("favourite", development) { ImageIndex = 0 });
        window.Controls.Add(list);
        window.Show();
        Application.DoEvents();
        window.DeviceDpi.Should().Be(96, "these are native 100% defaults, not a scaled bitmap");
        foreach (ListViewGroup group in list.Groups)
        {
            int id = (int)(typeof(ListViewGroup).GetProperty("ID", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(group) ?? throw new InvalidOperationException("The native group ID is unavailable."));
            NativeRect header = new() { Top = 1 };
            NativeRect label = new() { Top = 2 };
            SendMessage(list.Handle, GetGroupRectMessage, id, ref header).Should().NotBe(0);
            SendMessage(list.Handle, GetGroupRectMessage, id, ref label).Should().NotBe(0);
            ListViewItem item = group.Items[0];
            TestContext.Progress.WriteLine($"font={sizeInPoints} height={font.Height} group={group.Header} header={header.Left},{header.Top},{header.Right},{header.Bottom} label={label.Left},{label.Top},{label.Right},{label.Bottom} item={item.Bounds}");
            header.Bottom.Should().BeLessThanOrEqualTo(item.Bounds.Top);
            label.Right.Should().BeGreaterThan(label.Left);
            (header.Bottom - header.Top).Should().Be(21);
            label.Left.Should().Be(10);
            (label.Top - header.Top).Should().Be(2);
            item.Bounds.Should().Be(new Rectangle(2, header.Bottom + 1, list.TileSize.Width - 4, list.TileSize.Height - 2));
        }
    }

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    private static extern nint SendMessage(nint window, uint message, nint groupId, ref NativeRect bounds);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
