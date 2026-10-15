using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.Extensibility;
using GitUI;
using GitUI.CommandsDialogs;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

/// <summary>
///  Measures the original Browse selectors after their real opening handlers populate the menus.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class BrowseToolbarPopupLayoutNativeTests
{
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly string[] OwnerNames = ["branchSelect", "toolStripButtonPull"];
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [TestCase(9, false)]
    [TestCase(9, true)]
    [TestCase(11, false)]
    [TestCase(11, true)]
    [TestCase(14, false)]
    [TestCase(14, true)]
    public void Actual_source_Browse_selectors_should_use_native_menu_frame_and_populated_item_metrics(int points, bool rightToLeft)
    {
        using OriginalBrowseNativeProbe probe = OriginalBrowseNativeProbe.Create("BrowseToolbarPopupLayoutProbe", points);
        Point originalCursor = NativeMethods.GetCursorPosition();
        try
        {
            probe.WithOriginalBrowse(new BrowseArguments(), (form, getThreadException, closeAndDrain) =>
            {
                ToolStrip strip = ReadField<ToolStrip>(form, "ToolStripMain");
                strip.Font = probe.UiFont;
                strip.RightToLeft = rightToLeft ? RightToLeft.Yes : RightToLeft.No;
                ToolStripDropDownItem[] owners = OwnerNames.Select(name => ReadField<ToolStripDropDownItem>(form, name)).ToArray();
                using ApplicationContext loop = new();
                using System.Windows.Forms.Timer timer = new() { Interval = 80 };
                Stopwatch elapsed = Stopwatch.StartNew();
                Exception? failure = null;
                int stage = 0;
                bool finishing = false;
                EventHandler onIdle = null!;
                onIdle = (_, _) =>
                {
                    Application.Idle -= onIdle;
                    try
                    {
                        form.Show();
                        NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                        timer.Start();
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                        Finish();
                    }
                };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    try
                    {
                        if (getThreadException() is Exception exception)
                        {
                            ExceptionDispatchInfo.Capture(exception).Throw();
                        }

                        if (elapsed.Elapsed > TimeSpan.FromSeconds(30))
                        {
                            throw new TimeoutException("The source toolbar popup probe exceeded its bounded owned message loop.");
                        }

                        if (!IsLoadingComplete(form.RevisionGridControl))
                        {
                            return;
                        }

                        ToolStripDropDownItem owner = owners[stage / 3];
                        switch (stage % 3)
                        {
                            case 0:
                                NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                                NativeMethods.SetCursorPosition(form.PointToScreen(new Point(form.ClientSize.Width - 20, form.ClientSize.Height - 20)));
                                form.DeviceDpi.Should().Be(96);
                                owner.Available.Should().BeTrue();
                                owner.Enabled.Should().BeTrue();
                                owner.ShowDropDown();
                                break;
                            case 1:
                                Record(owner);
                                owner.DropDown.Close();
                                break;
                            default:
                                owner.DropDown.Visible.Should().BeFalse();
                                break;
                        }

                        if (++stage == owners.Length * 3)
                        {
                            Finish();
                        }
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                        Finish();
                    }
                    finally
                    {
                        if (!finishing)
                        {
                            timer.Start();
                        }
                    }
                };
                Application.Idle += onIdle;
                try
                {
                    form.StartPosition = FormStartPosition.Manual;
                    form.Location = new Point(30, 30);
                    form.ClientSize = new Size(1800, 950);
                    Application.Run(loop);
                }
                finally
                {
                    timer.Stop();
                    Application.Idle -= onIdle;
                    File.WriteAllText(Path.Combine(probe.EvidenceDirectory, "result.json"), JsonSerializer.Serialize(new
                    {
                        points,
                        rightToLeft,
                        completedStages = stage,
                        failure = failure?.ToString(),
                        scope = "actual source FormBrowse, original Branch/Pull opening handlers and native96 message loop; explicit owner font; no command invocation",
                    }, JsonOptions));
                }

                if (failure is not null)
                {
                    ExceptionDispatchInfo.Capture(failure).Throw();
                }

                stage.Should().Be(owners.Length * 3);
                return;

                void Finish()
                {
                    if (finishing)
                    {
                        return;
                    }

                    finishing = true;
                    timer.Stop();
                    foreach (ToolStripDropDownItem owner in owners)
                    {
                        owner.DropDown.Close();
                    }

                    closeAndDrain();
                    loop.ExitThread();
                }

                void Record(ToolStripDropDownItem owner)
                {
                    ToolStripDropDownMenu popup = owner.DropDown as ToolStripDropDownMenu
                        ?? throw new InvalidOperationException("The source selector must retain its native menu.");
                    popup.Visible.Should().BeTrue();
                    popup.DeviceDpi.Should().Be(96);
                    popup.Font.SizeInPoints.Should().Be(points);
                    NativeTestWindowActivation.AssertForeground(form);
                    ToolStripItem[] visible = popup.Items.Cast<ToolStripItem>().Where(item => item.Available).ToArray();
                    visible.Should().NotBeEmpty();
                    Size maximum = ReadProperty<Size>(popup, "MaxItemSize");
                    int trim = ReadProperty<int>(popup, "PaddingToTrim");
                    Rectangle textRectangle = ReadProperty<Rectangle>(popup, "TextRectangle");
                    Size tab = TextRenderer.MeasureText("\t", popup.Font);
                    int expectedItemHeight = points switch
                    {
                        9 => 22,
                        11 => 24,
                        14 => 30,
                        _ => throw new InvalidOperationException("The source font matrix must declare its native row metric."),
                    };
                    ToolStripMenuItem[] commands = popup.Items.OfType<ToolStripMenuItem>().ToArray();
                    commands.Should().OnlyContain(item => item.ShortcutKeys == Keys.None || !string.IsNullOrEmpty(item.ShortcutKeyDisplayString),
                        "this probe measures explicit source shortcut captions rather than assuming a framework key-name conversion");
                    int textWidth = commands.Max(item => TextRenderer.MeasureText(item.Text, item.Font).Width
                        + tab.Width + TextRenderer.MeasureText(item.ShortcutKeyDisplayString, item.Font).Width);
                    object description = new
                    {
                        owner = owner.Name,
                        points,
                        rightToLeft,
                        popup.Bounds,
                        popup.ClientSize,
                        popup.DisplayRectangle,
                        popup.Padding,
                        popup.Margin,
                        maximumItemSize = maximum,
                        paddingToTrim = trim,
                        textRectangle,
                        imageRectangle = ReadProperty<Rectangle>(popup, "ImageRectangle"),
                        arrowRectangle = ReadProperty<Rectangle>(popup, "ArrowRectangle"),
                        measuredTab = tab,
                        renderer = popup.Renderer.GetType().FullName,
                        background = $"#{popup.BackColor.ToArgb():X8}",
                        foreground = $"#{popup.ForeColor.ToArgb():X8}",
                        items = popup.Items.Cast<ToolStripItem>().Select(item => new
                        {
                            item.Name,
                            item.Text,
                            item.Available,
                            item.Bounds,
                            item.Margin,
                            preferred = item.GetPreferredSize(Size.Empty),
                            caption = TextRenderer.MeasureText(item.Text, item.Font),
                            shortcut = item is ToolStripMenuItem menuItem ? menuItem.ShortcutKeyDisplayString : null,
                            item.Font.SizeInPoints,
                            foreground = $"#{item.ForeColor.ToArgb():X8}",
                        }).ToArray(),
                    };
                    string prefix = Path.Combine(probe.EvidenceDirectory,
                        owner.Name ?? throw new InvalidOperationException("The source selector must retain its field name."));
                    File.WriteAllText(prefix + ".json", JsonSerializer.Serialize(description, JsonOptions));
                    using CaptureImageResult frame = ImageCapture.Capture(form, [popup], []);
                    frame.Bitmap.Save(prefix + ".png", ImageFormat.Png);
                    ControlTreeReader reader = new(form, form.DeviceDpi);
                    File.WriteAllText(prefix + ".tree.json", JsonSerializer.Serialize(new
                    {
                        method = frame.Method.ToString(),
                        frame.ScreenBounds,
                        frame.PrimaryScreenBounds,
                        frame.Acquisitions,
                        primary = reader.ReadPrimary(form, frame.ScreenBounds),
                        popup = reader.ReadPopup(popup, 0, frame.ScreenBounds.Location),
                    }, JsonOptions));
                    popup.Width.Should().Be(maximum.Width - trim + popup.Padding.Horizontal);
                    popup.Height.Should().Be(visible.Sum(item => item.GetPreferredSize(Size.Empty).Height + item.Margin.Vertical) + popup.Padding.Vertical);
                    visible.OfType<ToolStripMenuItem>().Should().OnlyContain(item => item.Bounds.Size == item.GetPreferredSize(Size.Empty));
                    popup.Padding.Should().Be(rightToLeft ? new Padding(1, 2, 34, 2) : new Padding(33, 2, 1, 2));
                    trim.Should().Be(rightToLeft ? 34 : 33);
                    maximum.Width.Should().Be(25 + 8 + textWidth + 9 + 10 + 8,
                        "the source reserves image gutter, text/shortcut and arrow padding independently of popup border paint");
                    textRectangle.Width.Should().Be(textWidth);
                    textRectangle.X.Should().Be(rightToLeft ? 27 : 33);
                    popup.Width.Should().Be(maximum.Width + 1);
                    visible.OfType<ToolStripMenuItem>().Should().OnlyContain(item =>
                        item.Bounds.X == 0 && item.Bounds.Width == maximum.Width && item.Bounds.Height == expectedItemHeight);
                    visible.First().Bounds.Y.Should().Be(2);
                    visible.OfType<ToolStripSeparator>().Should().OnlyContain(separator =>
                        separator.Bounds.X == 2 && separator.Width == popup.Width - 4 && separator.Height == 6);
                }
            });
        }
        finally
        {
            NativeMethods.SetCursorPosition(originalCursor);
        }
    }

    private static T ReadField<T>(object owner, string name)
        where T : class
        => owner.GetType().GetField(name, InstanceMembers)?.GetValue(owner) as T
            ?? throw new MissingFieldException(owner.GetType().FullName, name);

    private static T ReadProperty<T>(object owner, string name)
        => (T)(owner.GetType().GetProperty(name, InstanceMembers)?.GetValue(owner)
            ?? throw new MissingMemberException(owner.GetType().FullName, name));

    private static bool IsLoadingComplete(RevisionGridControl grid)
    {
        object accessor = typeof(RevisionGridControl).GetMethod("GetTestAccessor", InstanceMembers)?.Invoke(grid, null)
            ?? throw new MissingMethodException(typeof(RevisionGridControl).FullName, "GetTestAccessor");
        return (bool)(accessor.GetType().GetProperty("IsDataLoadComplete", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.GetValue(accessor)
            ?? throw new MissingMemberException(accessor.GetType().FullName, "IsDataLoadComplete"));
    }
}
