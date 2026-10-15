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
using GitUI.UserControls.RevisionGrid;
using NUnit.Framework;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class RevisionGridViewportNativeTests
{
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    [TestCase(9)]
    [TestCase(11)]
    [TestCase(14)]
    public void Actual_source_revision_grid_should_reserve_scrollbars_and_clamp_its_fill_column_before_horizontal_scrolling(int points)
    {
        using OriginalBrowseNativeProbe probe = OriginalBrowseNativeProbe.Create("RevisionGridViewportProbe", points);
        probe.WithOriginalBrowse(new BrowseArguments(), (form, getThreadException, closeAndDrain) =>
        {
            RevisionGridControl control = form.RevisionGridControl;
            RevisionDataGridView view = ReadField<RevisionDataGridView>(control, "_gridView");
            DataGridViewColumn fill = ReadField<DataGridViewColumn>(control, "_maximizedColumn");
            HScrollBar horizontal = view.Controls.OfType<HScrollBar>().Single();
            VScrollBar vertical = view.Controls.OfType<VScrollBar>().Single();
            List<object> snapshots = [];
            List<(string Name, Action Apply)> stages =
            [
                ("wide", () => Resize(1800)),
                ("narrow", () => Resize(600)),
                ("narrow-scrolled-right", () => view.HorizontalScrollingOffset = VisibleWidth() - view.DisplayRectangle.Width),
                ("wide-regrown", () => Resize(1800)),
            ];
            using ApplicationContext loop = new();
            using System.Windows.Forms.Timer timer = new() { Interval = 50 };
            Stopwatch elapsed = Stopwatch.StartNew();
            Exception? failure = null;
            int stage = 0;
            int stableObservations = 0;
            string? previousLayout = null;
            bool prepared = false;
            bool finishing = false;
            form.Shown += (_, _) => timer.Start();
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                try
                {
                    if (getThreadException() is Exception exception)
                    {
                        ExceptionDispatchInfo.Capture(exception).Throw();
                    }

                    if (elapsed.Elapsed > TimeSpan.FromSeconds(45))
                    {
                        throw new TimeoutException($"The original grid viewport probe exceeded its bounded loop at stage {stage}.");
                    }

                    if (!IsLoadingComplete(control) || view.RowCount < 40)
                    {
                        return;
                    }

                    if (!prepared)
                    {
                        stages[stage].Apply();
                        form.PerformLayout();
                        view.PerformLayout();
                        prepared = true;
                        stableObservations = 0;
                        previousLayout = null;
                        return;
                    }

                    object snapshot = new
                    {
                        stage = stages[stage].Name,
                        form.ClientSize,
                        view.Bounds,
                        view.DisplayRectangle,
                        view.HorizontalScrollingOffset,
                        horizontal = new { horizontal.Bounds, horizontal.Visible, horizontal.Minimum, horizontal.Maximum, horizontal.LargeChange },
                        vertical = new { vertical.Bounds, vertical.Visible, vertical.Minimum, vertical.Maximum, vertical.LargeChange },
                        columns = view.Columns.Cast<DataGridViewColumn>().Select(column => new
                        {
                            column.Index,
                            column.Width,
                            column.MinimumWidth,
                            column.Visible,
                            autoSizeMode = column.AutoSizeMode.ToString(),
                        }).ToArray(),
                    };
                    string serialized = JsonSerializer.Serialize(snapshot);
                    stableObservations = serialized == previousLayout ? stableObservations + 1 : 1;
                    previousLayout = serialized;
                    if (stableObservations < 3)
                    {
                        return;
                    }

                    form.DeviceDpi.Should().Be(96);
                    view.ScrollBars.Should().Be(ScrollBars.Both);
                    vertical.Visible.Should().BeTrue();
                    vertical.Width.Should().Be(17);
                    view.DisplayRectangle.Width.Should().Be(view.ClientSize.Width - vertical.Width);
                    fill.MinimumWidth.Should().Be(25);
                    bool shouldScroll = stage is 1 or 2;
                    horizontal.Visible.Should().Be(shouldScroll);
                    view.DisplayRectangle.Height.Should().Be(view.ClientSize.Height - (shouldScroll ? horizontal.Height : 0));
                    int imposedWidth = view.Columns.Cast<DataGridViewColumn>()
                        .Where(column => column.Visible && column != fill).Sum(column => column.Width);
                    fill.Width.Should().Be(Math.Max(fill.MinimumWidth, view.DisplayRectangle.Width - imposedWidth));
                    if (shouldScroll)
                    {
                        horizontal.Height.Should().Be(17);
                        fill.Width.Should().Be(fill.MinimumWidth);
                    }

                    if (stage == 2)
                    {
                        view.HorizontalScrollingOffset.Should().BeGreaterThan(0);
                        DataGridViewColumn last = view.Columns.Cast<DataGridViewColumn>()
                            .Where(column => column.Visible).MaxBy(column => column.DisplayIndex)
                            ?? throw new InvalidOperationException("The original grid must retain a visible last column.");
                        view.GetCellDisplayRectangle(last.Index, view.FirstDisplayedScrollingRowIndex, cutOverflow: false)
                            .Right.Should().Be(view.DisplayRectangle.Right);
                    }

                    snapshots.Add(snapshot);
                    using CaptureImageResult image = ImageCapture.Capture(form, [], []);
                    image.Method.Should().Be(GitExtensions.ParityCapture.CaptureMethod.PrintWindow);
                    image.Bitmap.Save(Path.Combine(probe.EvidenceDirectory, $"{stage:D2}-{stages[stage].Name}.png"), ImageFormat.Png);
                    stage++;
                    prepared = false;
                    if (stage == stages.Count)
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
            Application.Idle += OnIdle;
            try
            {
                Application.Run(loop);
            }
            finally
            {
                timer.Stop();
                Application.Idle -= OnIdle;
                File.WriteAllText(Path.Combine(probe.EvidenceDirectory, "viewport.json"),
                    JsonSerializer.Serialize(new { points, stage, snapshots, failure = failure?.ToString() },
                        new JsonSerializerOptions { WriteIndented = true }));
            }

            if (failure is not null)
            {
                ExceptionDispatchInfo.Capture(failure).Throw();
            }

            stage.Should().Be(stages.Count);
            return;

            void Finish()
            {
                if (finishing)
                {
                    return;
                }

                finishing = true;
                timer.Stop();
                try
                {
                    closeAndDrain();
                }
                finally
                {
                    loop.ExitThread();
                }
            }

            void OnIdle(object? sender, EventArgs args)
            {
                Application.Idle -= OnIdle;
                try
                {
                    form.Show();
                }
                catch (Exception exception)
                {
                    failure = exception;
                    Finish();
                }
            }

            void Resize(int width) => form.ClientSize = new Size(width, 573);

            int VisibleWidth() => view.Columns.Cast<DataGridViewColumn>().Where(column => column.Visible).Sum(column => column.Width);
        });
    }

    private static bool IsLoadingComplete(RevisionGridControl grid)
    {
        object accessor = grid.GetType().GetMethod("GetTestAccessor", InstanceMembers)?.Invoke(grid, [])
            ?? throw new MissingMethodException(grid.GetType().FullName, "GetTestAccessor");
        return accessor.GetType().GetProperty("IsDataLoadComplete", InstanceMembers)?.GetValue(accessor) is true;
    }

    private static T ReadField<T>(object owner, string name)
        where T : class
        => owner.GetType().GetField(name, InstanceMembers)?.GetValue(owner) as T
            ?? throw new MissingFieldException(owner.GetType().FullName, name);
}
