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
///  Measures System-rendered working-directory surfaces independently of their raw BackColor.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class WorkingDirectoryPopupPaintNativeTests
{
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    [Test]
    public void Actual_source_WorkingDir_System_popup_and_favourite_submenu_should_paint_native_Menu_and_ControlDark()
    {
        using OriginalBrowseNativeProbe probe = OriginalBrowseNativeProbe.Create("WorkingDirectoryPopupPaintProbe");
        Point originalCursor = NativeMethods.GetCursorPosition();
        try
        {
            probe.WithOriginalBrowse(new BrowseArguments(), (form, getThreadException, closeAndDrain) =>
            {
                ToolStripDropDownItem owner = ReadField<ToolStripDropDownItem>(form, "_NO_TRANSLATE_WorkingDir");
                ToolStripMenuItem? favourite = null;
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
                            throw new TimeoutException("The actual source working-directory paint probe exceeded its bounded owned loop.");
                        }

                        if (!IsLoadingComplete(form.RevisionGridControl))
                        {
                            return;
                        }

                        switch (stage)
                        {
                            case 0:
                                NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                                NativeMethods.SetCursorPosition(form.PointToScreen(new Point(form.ClientSize.Width - 20, form.ClientSize.Height - 20)));
                                owner.ShowDropDown();
                                break;
                            case 1:
                                Record("parent", owner.DropDown, [owner.DropDown]);
                                favourite = owner.DropDown.Items.OfType<ToolStripMenuItem>().Single(item => item.HasDropDownItems);
                                favourite.ShowDropDown();
                                break;
                            default:
                                ToolStripDropDown child = favourite?.DropDown
                                    ?? throw new InvalidOperationException("The populated source favourite row must retain its native child popup.");
                                Record("favourite", child, [owner.DropDown, child]);
                                Finish();
                                break;
                        }

                        stage++;
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
                        completedStages = stage,
                        failure = failure?.ToString(),
                        scope = "actual source FormBrowse working-directory and populated favourite child, native96 light System renderer, unscaled complete-window acquisition; no command invocation",
                    }, new JsonSerializerOptions { WriteIndented = true }));
                }

                if (failure is not null)
                {
                    ExceptionDispatchInfo.Capture(failure).Throw();
                }

                stage.Should().Be(3);
                return;

                void Finish()
                {
                    if (finishing)
                    {
                        return;
                    }

                    finishing = true;
                    timer.Stop();
                    favourite?.DropDown.Close();
                    owner.DropDown.Close();
                    closeAndDrain();
                    loop.ExitThread();
                }

                void Record(string name, ToolStripDropDown popup, ToolStripDropDown[] popups)
                {
                    form.DeviceDpi.Should().Be(96);
                    popup.DeviceDpi.Should().Be(96);
                    popup.Visible.Should().BeTrue();
                    popup.Renderer.Should().BeAssignableTo<ToolStripSystemRenderer>();
                    NativeTestWindowActivation.AssertForeground(form);
                    using CaptureImageResult frame = ImageCapture.Capture(form, popups, []);
                    frame.Bitmap.Save(Path.Combine(probe.EvidenceDirectory, name + ".png"), ImageFormat.Png);
                    Rectangle bounds = NativeMethods.GetWindowRectangle(popup.Handle);
                    int x = bounds.X - frame.ScreenBounds.X;
                    int y = bounds.Y - frame.ScreenBounds.Y;
                    Color border = frame.Bitmap.GetPixel(x, y);
                    Color background = frame.Bitmap.GetPixel(x + bounds.Width - 2, y + bounds.Height - 2);
                    List<object> separatorEvidence = [];
                    foreach (ToolStripSeparator separator in popup.Items.OfType<ToolStripSeparator>())
                    {
                        using Bitmap expected = ToolStripSeparatorPaintTests.RenderSeparator(
                            popup.Renderer, separator, SystemColors.Menu, vertical: false);
                        for (int row = 0; row < separator.Height; row++)
                        {
                            for (int column = 0; column < separator.Width; column++)
                            {
                                frame.Bitmap.GetPixel(x + separator.Bounds.X + column, y + separator.Bounds.Y + row).ToArgb()
                                    .Should().Be(expected.GetPixel(column, row).ToArgb(),
                                        $"the actual source popup separator must paint its real renderer at {column},{row}");
                            }
                        }

                        separatorEvidence.Add(new { separator.Bounds, allPixelsMatchNativeRenderer = true });
                    }

                    File.WriteAllText(Path.Combine(probe.EvidenceDirectory, name + ".json"), JsonSerializer.Serialize(new
                    {
                        acquisition = frame.Method.ToString(),
                        frame.ScreenBounds,
                        bounds,
                        renderer = popup.Renderer.GetType().FullName,
                        rawBackground = $"#{popup.BackColor.ToArgb():X8}",
                        paintedBackground = $"#{background.ToArgb():X8}",
                        paintedBorder = $"#{border.ToArgb():X8}",
                        menu = $"#{SystemColors.Menu.ToArgb():X8}",
                        controlDark = $"#{SystemColors.ControlDark.ToArgb():X8}",
                        separators = separatorEvidence,
                    }, new JsonSerializerOptions { WriteIndented = true }));
                    background.ToArgb().Should().Be(SystemColors.Menu.ToArgb());
                    border.ToArgb().Should().Be(SystemColors.ControlDark.ToArgb());
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

    private static bool IsLoadingComplete(RevisionGridControl grid)
    {
        object accessor = typeof(RevisionGridControl).GetMethod("GetTestAccessor", InstanceMembers)?.Invoke(grid, null)
            ?? throw new MissingMethodException(typeof(RevisionGridControl).FullName, "GetTestAccessor");
        return (bool)(accessor.GetType().GetProperty("IsDataLoadComplete", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.GetValue(accessor)
            ?? throw new MissingMemberException(accessor.GetType().FullName, "IsDataLoadComplete"));
    }
}
