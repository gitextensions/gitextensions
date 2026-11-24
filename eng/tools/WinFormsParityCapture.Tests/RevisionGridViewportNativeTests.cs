using System.Collections.Frozen;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using AwesomeAssertions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.BuildServerIntegration;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
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
                    if (stage == 0)
                    {
                        RetainColumnPaintProof(probe, control, view, points);
                    }

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

    private static void RetainColumnPaintProof(OriginalBrowseNativeProbe probe, RevisionGridControl control, RevisionDataGridView view, int points)
    {
        Type styleType = typeof(RevisionGridControl).Assembly.GetType("GitUI.UserControls.RevisionGrid.CellStyle", throwOnError: true)
            ?? throw new TypeLoadException("The native grid must retain its actual cell-style contract.");
        using Font bold = new(probe.UiFont, FontStyle.Bold);
        using Font buildFont = new(FontFamily.GenericMonospace, probe.UiFont.Size);
        Font mono = AppSettings.MonospaceFont;
        object style = Activator.CreateInstance(styleType,
            [Brushes.White, Color.Black, Color.Gray, probe.UiFont, bold, mono, FrozenDictionary<string, Color>.Empty])
            ?? throw new InvalidOperationException("The actual native cell-style constructor must remain available.");
        const string longText = "WWWW WWWW WWWW WWWW WWWW WWWW";
        const string authorText = "iiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii";
        GitRevision revision = new(ObjectId.Parse("0123456789abcdef0123456789abcdef01234567"))
        {
            Author = authorText,
            Notes = longText + "\nSecond line must not paint",
            BuildStatus = new BuildInfo { Status = BuildStatus.Unknown, Description = longText },
        };
        int rowHeight = view.Rows[view.FirstDisplayedScrollingRowIndex].Height;
        List<object> records = [];
        int authorInsetPixelDelta = 0;
        foreach ((string providerName, string text, int width, int leftInset, bool ellipsis) in new[]
        {
            ("DateColumnProvider", "October 23, 2026 23:59:59", 130, 0, true),
            ("CommitIdColumnProvider", revision.Guid, 60, 0, false),
            ("NotesColumnProvider", longText, 50, 0, true),
            ("AuthorNameColumnProvider", authorText, 130, 6, true),
            ("AuthorNameColumnProvider", authorText, 131, 6, true),
            ("AuthorNameColumnProvider", authorText, 132, 6, true),
            ("AuthorNameColumnProvider", authorText, 133, 6, true),
            ("BuildStatusColumnProvider", longText, 150, 0, true),
        })
        {
            DataGridViewColumn column = view.Columns.Cast<DataGridViewColumn>()
                .Single(candidate => candidate.Tag?.GetType().Name == providerName);
            object provider = column.Tag
                ?? throw new InvalidOperationException("The native column must retain its actual provider.");
            string paintedText = text;
            object? hashMeasurement = null;
            if (providerName == "CommitIdColumnProvider")
            {
                MethodInfo getLength = provider.GetType().GetMethod("GetCharLengthForColumnWidth", InstanceMembers)
                    ?? throw new MissingMethodException(provider.GetType().FullName, "GetCharLengthForColumnWidth");
                int fullWidthCount = (int)(getLength.Invoke(provider, [width])
                    ?? throw new InvalidOperationException("The source hash measurement must return its character count."));
                int insetWidthCount = (int)(getLength.Invoke(provider, [width - 6])
                    ?? throw new InvalidOperationException("The source diagnostic inset measurement must return its character count."));
                int expectedCount = Enumerable.Range(0, ObjectId.Sha1CharCount + 1)
                    .Where(length => TextRenderer.MeasureText(new string('8', length), mono).Width <= width)
                    .DefaultIfEmpty(0).Max();
                fullWidthCount.Should().Be(expectedCount,
                    "the original determines hash length from its complete 60-pixel column, not width minus a margin");
                fullWidthCount.Should().BeGreaterThan(insetWidthCount,
                    "the actual profile's monospace font must distinguish the full column from the incorrectly reduced width");
                paintedText = revision.ObjectId.ToShortString(fullWidthCount);
                hashMeasurement = new
                {
                    columnWidth = width,
                    fullWidthCount,
                    incorrectlyInsetWidth = width - 6,
                    incorrectlyInsetWidthCount = insetWidthCount,
                    measuredTextWidth = TextRenderer.MeasureText(new string('8', fullWidthCount), mono).Width,
                    nextCharacterWidth = TextRenderer.MeasureText(new string('8', fullWidthCount + 1), mono).Width,
                };
            }

            Rectangle cell = new(11, 7, width, rowHeight);
            Rectangle expectedBounds = new(cell.X + leftInset, cell.Y, cell.Width - leftInset, cell.Height);
            Rectangle oldInsetBounds = new(cell.X + 6, cell.Y, cell.Width - 8, cell.Height);
            Font font = providerName switch
            {
                "CommitIdColumnProvider" => mono,
                "BuildStatusColumnProvider" => buildFont,
                _ => probe.UiFont,
            };
            TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine;
            if (ellipsis)
            {
                flags |= TextFormatFlags.EndEllipsis;
            }

            using Bitmap actual = CreateBitmap();
            using (Graphics graphics = Graphics.FromImage(actual))
            {
                graphics.Clear(Color.White);
                graphics.SetClip(cell);
                DataGridViewCellPaintingEventArgs paint = new(view, graphics, cell, cell, rowIndex: 0, column.Index,
                    DataGridViewElementStates.Visible, paintedText, paintedText, string.Empty,
                    view.DefaultCellStyle, new DataGridViewAdvancedBorderStyle(), DataGridViewPaintParts.ContentForeground);
                bool previousBuildIcon = AppSettings.ShowBuildStatusIconColumn;
                bool previousBuildText = AppSettings.ShowBuildStatusTextColumn;
                try
                {
                    if (providerName == "BuildStatusColumnProvider")
                    {
                        AppSettings.ShowBuildStatusIconColumn = false;
                        AppSettings.ShowBuildStatusTextColumn = true;
                    }

                    (provider.GetType().GetMethod("OnCellPainting", InstanceMembers)
                        ?? throw new MissingMethodException(provider.GetType().FullName, "OnCellPainting"))
                        .Invoke(provider, [paint, revision, rowHeight, style]);
                }
                finally
                {
                    AppSettings.ShowBuildStatusIconColumn = previousBuildIcon;
                    AppSettings.ShowBuildStatusTextColumn = previousBuildText;
                }
            }

            using Bitmap expected = PaintExpected(expectedBounds);
            using Bitmap oldInset = PaintExpected(oldInsetBounds);
            string prefix = $"cell-paint-{providerName}-{width}";
            actual.Save(Path.Combine(probe.EvidenceDirectory, prefix + "-actual.png"), ImageFormat.Png);
            expected.Save(Path.Combine(probe.EvidenceDirectory, prefix + "-expected.png"), ImageFormat.Png);
            oldInset.Save(Path.Combine(probe.EvidenceDirectory, prefix + "-old-inset.png"), ImageFormat.Png);
            int actualDelta = CountPixelDelta(actual, expected);
            int oldInsetDelta = CountPixelDelta(actual, oldInset);
            actualDelta.Should().Be(0,
                "the actual original provider must paint exactly the independently specified native text rectangle");
            if (providerName == "AuthorNameColumnProvider")
            {
                // EndEllipsis can choose identical glyphs for adjacent widths. Probe the
                // neighboring clipping boundaries rather than requiring every two-pixel
                // inset to alter a particular font's truncated string.
                authorInsetPixelDelta += oldInsetDelta;
            }
            else
            {
                oldInsetDelta.Should().BeGreaterThan(0,
                    "the former port's universal six-left/two-right inset must be a distinguishable negative control");
            }

            records.Add(new
            {
                providerName,
                text = paintedText,
                cell,
                expectedBounds,
                oldInsetBounds,
                flags = flags.ToString(),
                font = new { font.Name, font.SizeInPoints, style = font.Style.ToString(), font.Height },
                actualDelta,
                oldInsetDelta,
                hashMeasurement,
                acquisition = "actual original provider OnCellPainting into a 96-DPI GDI bitmap; not a captured window or resized image",
            });

            Bitmap CreateBitmap()
            {
                Bitmap bitmap = new(width + 22, rowHeight + 14);
                bitmap.SetResolution(96, 96);
                return bitmap;
            }

            Bitmap PaintExpected(Rectangle bounds)
            {
                Bitmap bitmap = CreateBitmap();
                using Graphics graphics = Graphics.FromImage(bitmap);
                graphics.Clear(Color.White);
                graphics.SetClip(cell);
                TextRenderer.DrawText(graphics, paintedText, font, bounds, Color.Black, flags);
                return bitmap;
            }
        }

        authorInsetPixelDelta.Should().BeGreaterThan(0,
            "the source author's full remaining width must differ from the old two-pixel right inset at a clipping boundary");
        File.WriteAllText(Path.Combine(probe.EvidenceDirectory, "column-paint.json"),
            JsonSerializer.Serialize(new { points, rowHeight, records, nativeProviderAssembly = control.GetType().Assembly.FullName },
                new JsonSerializerOptions { WriteIndented = true }));
    }

    private static int CountPixelDelta(Bitmap first, Bitmap second)
    {
        int count = 0;
        for (int y = 0; y < first.Height; y++)
        {
            for (int x = 0; x < first.Width; x++)
            {
                if (first.GetPixel(x, y) != second.GetPixel(x, y))
                {
                    count++;
                }
            }
        }

        return count;
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
