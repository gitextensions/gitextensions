using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RevisionGridColumnResizeTests
{
    [AvaloniaTest]
    [TestCase("Message", 35, false)]
    [TestCase("Author Name", 20, false)]
    [TestCase("Message", -10000, false)]
    [TestCase("Message", 35, true)]
    [TestCase("last visible", 20, false)]
    public async Task Loaded_grid_should_apply_original_column_drag_rules(string columnName, double delta, bool loseCapture)
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using AppSettingsTestScope settings = new();
        using ServiceContainer services = new();
        GitExtensions.ServiceContainerRegistry.RegisterServices(services);
        services.RemoveService(typeof(IUpdateCheckService));
        services.AddService<IUpdateCheckService>(Substitute.For<IUpdateCheckService>());
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.ColumnResize-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        FormBrowse? form = null;
        try
        {
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), directory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(Path.Combine(directory, "file.txt"), "content\n");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "file.txt" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            form = new FormBrowse(new GitUICommands(services, module)) { Width = 1400, Height = 800 };
            form.Show();
            using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
            while (form.RevisionGrid.SelectedRevision?.ObjectId != module.GetCurrentCheckout()
                || !form.RevisionGrid.FindControl<TextBlock>("lblLoadingStatus")!.Text!.EndsWith(" revisions", StringComparison.Ordinal))
            {
                await Task.Delay(10, deadline.Token);
            }

            Settle(form);
            Grid row = form.RevisionGrid.GetVisualDescendants().OfType<Grid>()
                .First(grid => grid.Classes.Contains("revision-row") && grid.DataContext is GitRevision revision && revision.ObjectId == module.GetCurrentCheckout());
            IReadOnlyList<ColumnProvider> providers = form.RevisionGrid.ColumnProviders;
            ColumnProvider message = providers.Single(provider => provider.Name == "Message");
            ColumnProvider last = providers.Last(provider => provider.Column.IsVisible && provider.Column.Width.IsAbsolute);
            last.Column.Resizable.Should().BeFalse();
            ColumnProvider selected = columnName == "last visible" ? last : providers.Single(provider => provider.Name == columnName);
            selected.Column.Resizable.Should().Be(columnName != "last visible");
            ListBox grid = form.RevisionGrid.GetTestAccessor().Revisions;
            IPointer? pointer = null;
            grid.AddHandler(InputElement.PointerPressedEvent, (_, e) => pointer = e.Pointer, RoutingStrategies.Tunnel, handledEventsToo: true);
            double[] before = row.ColumnDefinitions.Select(definition => definition.ActualWidth).ToArray();
            double right = before.Take(selected.Index + 1).Sum();
            Point point = row.TranslatePoint(new Point(right - 1, row.Bounds.Height / 2), form)!.Value;
            form.MouseMove(point);
            form.MouseDown(point, MouseButton.Left);
            Settle(form);

            if (columnName == "last visible")
            {
                message.Column.Width.IsStar.Should().BeTrue();
                last.Column.Width.IsAbsolute.Should().BeTrue();
                form.MouseMove(point.WithX(point.X + delta), RawInputModifiers.LeftMouseButton);
                form.MouseUp(point.WithX(point.X + delta), MouseButton.Left);
                Settle(form);
                row.ColumnDefinitions.Select(definition => definition.ActualWidth).Should().Equal(before);
                form.GetVisualDescendants().OfType<Border>().Should().NotContain(border => border.Name == "columnResizePreview");
                return;
            }

            pointer.Should().NotBeNull();
            pointer!.Captured.Should().BeSameAs(grid);
            message.Column.Width.IsAbsolute.Should().BeTrue("the maximized column must release Fill before the temporary Fill is assigned");
            last.Column.Width.IsStar.Should().BeTrue();
            form.GetVisualDescendants().OfType<Border>().Should().Contain(border => border.Name == "columnResizePreview");
            Point moved = point.WithX(point.X + delta);
            form.MouseMove(moved, RawInputModifiers.LeftMouseButton);
            Settle(form);
            selected.Column.Width.Value.Should().BeApproximately(before[selected.Index], 0.01, "the original commits its resize only on release");
            if (loseCapture)
            {
                // Simulate an external capture loss after production has acquired it.
                pointer.Capture(null);
            }

            form.MouseUp(moved, MouseButton.Left);
            Settle(form);

            message.Column.Width.IsStar.Should().BeTrue();
            last.Column.Width.IsAbsolute.Should().BeTrue();
            last.Column.Resizable.Should().BeFalse();
            form.GetVisualDescendants().OfType<Border>().Should().NotContain(border => border.Name == "columnResizePreview");
            double expected = Math.Clamp(before[selected.Index] + (loseCapture ? 0 : delta), selected.Column.MinimumWidth,
                before[selected.Index] + before[last.Index] - last.Column.MinimumWidth);
            row.ColumnDefinitions[selected.Index].ActualWidth.Should().BeApproximately(expected, 1);
            row.ColumnDefinitions[last.Index].ActualWidth.Should().BeApproximately(before[last.Index] - (expected - before[selected.Index]), 1);
            foreach (ColumnProvider other in providers.Where(provider => provider != selected && provider != last))
            {
                row.ColumnDefinitions[other.Index].ActualWidth.Should().BeApproximately(before[other.Index], 1);
            }
        }
        finally
        {
            form?.Close();
            (form as IDisposable)?.Dispose();
            TestDirectory.Delete(directory);
        }
    }

    private static void Settle(Window form)
    {
        Dispatcher.UIThread.RunJobs();
        using var frame = form.CaptureRenderedFrame();
        frame.Should().NotBeNull();
        Dispatcher.UIThread.RunJobs();
    }
}
