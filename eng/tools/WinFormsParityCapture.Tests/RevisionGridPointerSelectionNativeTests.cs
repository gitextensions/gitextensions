using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using AwesomeAssertions;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.ScriptsEngine;
using GitUI.UserControls.RevisionGrid;
using NUnit.Framework;
using SourceCommitInfo = GitUI.CommitInfo.CommitInfo;

namespace WinFormsParityCapture.Tests;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public sealed class RevisionGridPointerSelectionNativeTests
{
    [TestCase(false)]
    [TestCase(true)]
    public void Actual_source_pointer_routes_should_record_current_latest_details_and_selection_order(bool emptyViewport)
    {
        using OriginalBrowseNativeProbe probe = OriginalBrowseNativeProbe.Create("RevisionGridPointerSelectionProbe");
        string repository = probe.Repository;
        if (emptyViewport)
        {
            repository = Path.Combine(probe.BootstrapDirectory, "pointer-empty-repository");
            Directory.CreateDirectory(repository);
            RevisionGridSelectionNativeTests.SeedRepository(repository).Should().HaveCount(3);
        }

        probe.WithOriginalBrowse(new BrowseArguments(), (form, getThreadException, closeAndDrain) =>
            RunPointerStages(form, probe, emptyViewport, getThreadException, closeAndDrain), repository);
    }

    private static void RunPointerStages(FormBrowse form, OriginalBrowseNativeProbe probe, bool emptyViewport,
        Func<Exception?> getThreadException, Action closeAndDrain)
    {
        form.UICommands.GetRequiredService<IScriptsManager>().GetScripts().Should().BeEmpty();
        RevisionGridControl grid = form.RevisionGridControl;
        RevisionDataGridView view = ReadField<RevisionDataGridView>(grid, "_gridView");
        SourceCommitInfo info = ReadField<SourceCommitInfo>(form, "RevisionInfo");
        TabControl tabs = ReadField<TabControl>(form, "CommitInfoTabControl");
        tabs.SelectedTab = tabs.TabPages.Cast<TabPage>().Single(tab => tab.Name == "CommitInfoTabPage");

        // This consumer measures row selection only; opening a popup would change native modal menu mode.
        grid.DisableContextMenu();
        List<object> events = [];
        List<object> snapshots = [];
        List<PointerStep> steps = [];
        int mouseUpCount = 0;
        int mouseUpBeforeInput = 0;
        int stepIndex = -1;
        int[] rows = [];
        ObjectId[] ids = [];
        KeyboardStateScope? keyboard = null;
        Exception? failure = null;
        bool finishing = false;
        Stopwatch elapsed = Stopwatch.StartNew();
        Stopwatch stepElapsed = new();
        using ApplicationContext loopContext = new();
        using System.Windows.Forms.Timer timer = new() { Interval = 50 };
        view.CellMouseDown += (_, e) => RecordEvent("CellMouseDown", e.RowIndex, e.Button);
        view.MouseClick += (_, e) => RecordEvent("MouseClick", view.HitTest(e.X, e.Y).RowIndex, e.Button);
        view.MouseUp += (_, e) =>
        {
            mouseUpCount++;
            RecordEvent("MouseUp", view.HitTest(e.X, e.Y).RowIndex, e.Button);
        };
        view.KeyUp += (_, e) =>
        {
            mouseUpCount++;
            RecordEvent("KeyUp:" + e.KeyData, view.CurrentCell?.RowIndex ?? -1, MouseButtons.None);
        };
        view.SelectionChanged += (_, _) => RecordEvent("SelectionChanged", view.CurrentCell?.RowIndex ?? -1, MouseButtons.None);
        form.Shown += (_, _) =>
        {
            try
            {
                timer.Start();
                NativeTestWindowActivation.Activate(form, waitForPresentation: false);
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

                if (elapsed.Elapsed > TimeSpan.FromSeconds(60))
                {
                    throw new TimeoutException("The actual source queued pointer stages exceeded their bounded owned message loop.");
                }

                if (!IsLoadingComplete(grid))
                {
                    return;
                }

                if (stepIndex < 0)
                {
                    rows = [.. Enumerable.Range(0, view.RowCount)
                        .Where(index => view.GetRevision(index) is { IsArtificial: false }).Take(3)];
                    rows.Should().HaveCount(3);
                    ids = [.. rows.Select(index => view.GetRevision(index)!.ObjectId)];
                    grid.SetSelectedRevision(ids[0]).Should().BeTrue();
                    steps.Add(new("plain-first", 0, MouseButtons.Left, Keys.None, [0]));
                    if (emptyViewport)
                    {
                        steps.Add(new("right-empty", -1, MouseButtons.Right, Keys.None, [0]));
                    }
                    else
                    {
                        steps.Add(new("control-add-third", 2, MouseButtons.Left, Keys.Control, [2, 0]));
                        steps.Add(new("right-latest-selected", 2, MouseButtons.Right, Keys.None, [2, 0]));
                        steps.Add(new("right-other-selected", 0, MouseButtons.Right, Keys.None, [0]));
                        steps.Add(new("control-add-third-again", 2, MouseButtons.Left, Keys.Control, [2, 0]));
                        steps.Add(new("control-remove-third", 2, MouseButtons.Left, Keys.Control, [0]));
                        steps.Add(new("plain-second", 1, MouseButtons.Left, Keys.None, [1]));
                        steps.Add(new("shift-third", 2, MouseButtons.Left, Keys.Shift, [2, 1]));
                        steps.Add(new("control-add-first", 0, MouseButtons.Left, Keys.Control, [0, 2, 1]));
                        steps.Add(new("control-remove-second", 1, MouseButtons.Left, Keys.Control, [0, 2]));
                        steps.Add(new("right-deselected-latest", 1, MouseButtons.Right, Keys.None, [1]));
                        steps.Add(new("control-add-third-for-caret", 2, MouseButtons.Left, Keys.Control, [2, 1]));
                        steps.Add(new("right-second-preserve-current", 1, MouseButtons.Right, Keys.None, [1]));
                        steps.Add(new("up-after-right", 1, MouseButtons.None, Keys.None, [1], Keys.Up));
                    }

                    NativeTestWindowActivation.Activate(form, waitForPresentation: false);
                    form.DeviceDpi.Should().Be(96);
                    stepIndex = 0;
                    QueueInput();
                    return;
                }

                if (mouseUpCount <= mouseUpBeforeInput || stepElapsed.Elapsed < TimeSpan.FromMilliseconds(150)
                    || info.Revision?.ObjectId != grid.GetSelectedRevisionOrDefault()?.ObjectId)
                {
                    return;
                }

                keyboard?.Dispose();
                keyboard = null;
                RecordSnapshot();
                stepIndex++;
                if (stepIndex == steps.Count)
                {
                    Finish();
                }
                else
                {
                    QueueInput();
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
        EventHandler onIdle = null!;
        onIdle = (_, _) =>
        {
            Application.Idle -= onIdle;
            try
            {
                form.Show();
            }
            catch (Exception exception)
            {
                failure = exception;
                Finish();
            }
        };
        Application.Idle += onIdle;
        try
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(30, 30);
            form.ClientSize = new Size(1180, 740);
            Application.Run(loopContext);
        }
        finally
        {
            timer.Stop();
            Application.Idle -= onIdle;
            keyboard?.Dispose();
            File.WriteAllText(Path.Combine(probe.EvidenceDirectory, "pointer-contract.json"), JsonSerializer.Serialize(new
            {
                emptyViewport,
                actualRepository = form.UICommands.Module.WorkingDir,
                probe.Repository,
                probe.ProfilePath,
                form.TopMost,
                sourceRoute = "actual original FormBrowse/RevisionDataGridView; queued owned HWND mouse/key messages through Application.Run, scoped STA keyboard state; source completion accessor is observational only",
                boundary = "selection-only source diagnostic: original DisableContextMenu prevents popup modal state; no source edits, physical input or popup-paint claim; separate three-commit Git CLI fixture for the genuine empty viewport",
                steps,
                snapshots,
                events,
                failure = failure?.ToString(),
            }, new JsonSerializerOptions { WriteIndented = true }));
            TestContext.Out.WriteLine($"revisionGridPointerEvidence={probe.EvidenceDirectory}");
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }

        snapshots.Should().HaveCount(steps.Count);
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
                keyboard?.Dispose();
            }
            catch (Exception exception)
            {
                failure = failure is null ? exception : new AggregateException(failure, exception);
            }
            finally
            {
                keyboard = null;
                try
                {
                    closeAndDrain();
                }
                catch (Exception exception)
                {
                    failure = failure is null ? exception : new AggregateException(failure, exception);
                }
                finally
                {
                    loopContext.ExitThread();
                }
            }
        }

        void QueueInput()
        {
            NativeTestWindowActivation.AssertForeground(form);
            PointerStep step = steps[stepIndex];
            Point point;
            if (step.Row < 0)
            {
                point = new Point(view.ClientSize.Width / 2, view.ClientSize.Height - 30);
                view.HitTest(point.X, point.Y).Type.Should().Be(DataGridViewHitTestType.None,
                    "empty-area evidence must hit the actual source viewport, not a fake or filtered row");
            }
            else
            {
                Rectangle cell = view.GetCellDisplayRectangle(1, rows[step.Row], cutOverflow: true);
                point = new Point(cell.Left + Math.Min(100, cell.Width / 2), cell.Top + (cell.Height / 2));
                view.HitTest(point.X, point.Y).RowIndex.Should().Be(rows[step.Row]);
            }

            view.Focus().Should().BeTrue();
            keyboard = new KeyboardStateScope(step.Modifiers);
            mouseUpBeforeInput = mouseUpCount;
            stepElapsed.Restart();
            if (step.Key != Keys.None)
            {
                nint keyData = (nint)(1 | (MapVirtualKey((uint)step.Key, 0) << 16) | 0x01000000);
                PostMessage(view.Handle, 0x0100, (nint)step.Key, keyData).Should().BeTrue();
                PostMessage(view.Handle, 0x0101, (nint)step.Key, unchecked((nint)((long)keyData | 0xC0000000L))).Should().BeTrue();
                return;
            }

            nint coordinates = (nint)((point.Y << 16) | (point.X & 0xFFFF));
            int modifiers = (step.Modifiers.HasFlag(Keys.Control) ? 8 : 0) | (step.Modifiers.HasFlag(Keys.Shift) ? 4 : 0);
            uint down = step.Button == MouseButtons.Right ? 0x0204u : 0x0201u;
            uint up = step.Button == MouseButtons.Right ? 0x0205u : 0x0202u;
            int button = step.Button == MouseButtons.Right ? 2 : 1;
            PostMessage(view.Handle, 0x0200, (nint)modifiers, coordinates).Should().BeTrue();
            PostMessage(view.Handle, down, (nint)(modifiers | button), coordinates).Should().BeTrue();
            PostMessage(view.Handle, up, (nint)modifiers, coordinates).Should().BeTrue();
        }

        void RecordEvent(string kind, int row, MouseButtons button)
        {
            events.Add(new
            {
                stepIndex,
                kind,
                row,
                button = button.ToString(),
                modifiers = Control.ModifierKeys.ToString(),
                current = view.CurrentCell?.RowIndex,
                latest = form.GetLatestSelectedRevision()?.ObjectId.ToString(),
                selected = grid.GetSelectedRevisions().Select(revision => revision.ObjectId.ToString()).ToArray(),
            });
        }

        void RecordSnapshot()
        {
            NativeTestWindowActivation.AssertForeground(form);
            PointerStep step = steps[stepIndex];
            using CaptureImageResult image = ImageCapture.Capture(form, [], []);
            image.Method.Should().Be(CaptureMethod.PrintWindow);
            image.Bitmap.Save(Path.Combine(probe.EvidenceDirectory, step.Name + ".png"), ImageFormat.Png);
            ObjectId[] selected = [.. grid.GetSelectedRevisions().Select(revision => revision.ObjectId)];
            object snapshot = new
            {
                step.Name,
                stepIndex,
                step.Row,
                modifiers = step.Modifiers.ToString(),
                selected = selected.Select(id => id.ToString()).ToArray(),
                restoration = view.SelectedObjectIds?.Select(id => id.ToString()).ToArray(),
                selectedRows = view.SelectedRows.Cast<DataGridViewRow>().Select(row => row.Index).ToArray(),
                currentRow = view.CurrentCell?.RowIndex,
                current = view.CurrentCell is { } current ? view.GetRevision(current.RowIndex)?.ObjectId.ToString() : null,
                latest = form.GetLatestSelectedRevision()?.ObjectId.ToString(),
                @default = grid.GetSelectedRevisionOrDefault()?.ObjectId.ToString(),
                details = info.Revision?.ObjectId.ToString(),
                captureMethod = image.Method.ToString(),
                image.ScreenBounds,
            };
            snapshots.Add(snapshot);
            TestContext.Out.WriteLine(JsonSerializer.Serialize(snapshot));
            if (step.ExpectedRows is { } expected)
            {
                selected.Should().Equal(expected.Select(index => ids[index]));
                view.SelectedObjectIds.Should().Equal(expected.Reverse().Select(index => ids[index]));
                (grid.GetSelectedRevisionOrDefault()?.ObjectId).Should().Be(ids[expected[0]]);
                (info.Revision?.ObjectId).Should().Be(ids[expected[0]]);
                (form.GetLatestSelectedRevision()?.ObjectId).Should().Be(step.Row < 0 ? null : ids[step.Row]);
                int expectedCurrentIndex = step.Name is "right-other-selected" or "right-second-preserve-current" ? 2 : Math.Max(0, step.Row);
                (view.CurrentCell?.RowIndex).Should().Be(rows[expectedCurrentIndex]);
                if (step.Row < 0)
                {
                    events.Should().NotContain(entry => JsonSerializer.Serialize(entry).Contains("CellMouseDown", StringComparison.Ordinal)
                        && JsonSerializer.Serialize(entry).Contains("\"stepIndex\":" + stepIndex, StringComparison.Ordinal));
                }
            }
        }
    }

    private static bool IsLoadingComplete(RevisionGridControl grid)
    {
        object accessor = typeof(RevisionGridControl).GetMethod("GetTestAccessor", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(grid, null)!;
        return (bool)accessor.GetType().GetProperty("IsDataLoadComplete")!.GetValue(accessor)!;
    }

    private static T ReadField<T>(object owner, string name)
        where T : class
        => (T)(owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)!
            .GetValue(owner) ?? throw new MissingFieldException(owner.GetType().FullName, name));

    private sealed record PointerStep(string Name, int Row, MouseButtons Button, Keys Modifiers, int[]? ExpectedRows, Keys Key = Keys.None);

    private sealed class KeyboardStateScope : IDisposable
    {
        private readonly byte[] _original = new byte[256];

        public KeyboardStateScope(Keys modifiers)
        {
            GetKeyboardState(_original).Should().BeTrue();
            byte[] current = (byte[])_original.Clone();
            foreach (int key in new[] { 0x10, 0x11, 0x12, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 })
            {
                current[key] &= 0x7F;
            }

            if (modifiers.HasFlag(Keys.Control))
            {
                current[0x11] |= 0x80;
                current[0xA2] |= 0x80;
            }

            if (modifiers.HasFlag(Keys.Shift))
            {
                current[0x10] |= 0x80;
                current[0xA0] |= 0x80;
            }

            try
            {
                SetKeyboardState(current).Should().BeTrue();
                Control.ModifierKeys.Should().Be(modifiers);
            }
            catch (Exception exception)
            {
                if (!SetKeyboardState(_original))
                {
                    throw new AggregateException(exception, new InvalidOperationException("The failed keyboard-state setup could not restore its owned STA keyboard bytes."));
                }

                throw;
            }
        }

        public void Dispose() => SetKeyboardState(_original).Should().BeTrue();
    }

    [DllImport("user32.dll", EntryPoint = "PostMessageW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(nint window, uint message, nint wordParameter, nint longParameter);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetKeyboardState(byte[] keys);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetKeyboardState(byte[] keys);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint key, uint mapType);
}
