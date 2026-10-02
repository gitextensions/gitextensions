using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.ParityCapture;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Compat;
using GitUI.Theming;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsControls = GitUI.Compat.WinFormsControls;

namespace GitExtensionsTests;

[TestFixture]
public sealed class DashboardTests
{
    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(false)]
    [TestCase(true)]
    public void Repository_focus_should_select_the_first_tile_through_keyboard_and_capture_routes(bool captureRoute)
    {
        Repository recent = new(@"C:\repos\recent");
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        RepositoryHistorySnapshot snapshot = new(
            [new RepositoryHistoryEntry(recent, "recent", "main", IsFavourite: false, IsAnchored: false)],
            [new RepositoryHistoryEntry(favourite, "favourite", "main", IsFavourite: true, IsAnchored: false)]);
        UserRepositoriesList list = new();
        list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Width = 700, Height = 400, Content = list };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            list.GetTestAccessor().Search.Focus();
            using AvaloniaControlStateDriver? driver = captureRoute
                ? AvaloniaControlStateDriver.Apply(list, new CaptureStatePlan
                {
                    Id = "repository-list.focused", Kind = CaptureStateKind.Focus, TargetField = "listView1",
                })
                : null;
            if (!captureRoute)
            {
                window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            }

            Dispatcher.UIThread.RunJobs();
            ListBox repositories = list.GetTestAccessor().List;
            repositories.IsKeyboardFocusWithin.Should().BeTrue();
            repositories.FocusAdorner.Should().BeNull("the source ListView has BorderStyle.None and owner-drawn selection only");
            repositories.SelectedItem.Should().Be(repositories.Items.OfType<UserRepositoriesList.RepositoryListItem>().First());
            window.KeyPress(Key.Down, RawInputModifiers.None, PhysicalKey.ArrowDown, null);
            Dispatcher.UIThread.RunJobs();
            repositories.SelectedItem.Should().Be(repositories.Items.OfType<UserRepositoriesList.RepositoryListItem>().Last(),
                "group headings are not repository selections");
            window.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, null);
            Dispatcher.UIThread.RunJobs();
            repositories.SelectedItem.Should().Be(repositories.Items.OfType<UserRepositoriesList.RepositoryListItem>().First());
            window.KeyPress(Key.Up, RawInputModifiers.None, PhysicalKey.ArrowUp, null);
            Dispatcher.UIThread.RunJobs();
            list.GetTestAccessor().Search.IsFocused.Should().BeTrue();
            repositories.SelectedItem.Should().Be(repositories.Items.OfType<UserRepositoriesList.RepositoryListItem>().First());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Dashboard_backdrop_should_tile_the_original_image_without_resizing_it()
    {
        Dashboard dashboard = new();
        Window window = new() { Width = 1500, Height = 1000, Content = dashboard };
        try
        {
            window.Show();
            dashboard.RefreshContent();
            window.UpdateLayout();
            Grid layout = dashboard.FindControl<Grid>("tableLayoutPanel1")!;
            layout.Background.Should().BeOfType<Avalonia.Media.ImageBrush>();
            Avalonia.Media.ImageBrush backdrop = (Avalonia.Media.ImageBrush)layout.Background!;
            backdrop.Source.Should().BeSameAs(DashboardTheme.Light.BackgroundImage);
            backdrop.TileMode.Should().Be(Avalonia.Media.TileMode.Tile);
            backdrop.Stretch.Should().Be(Avalonia.Media.Stretch.None);
            backdrop.DestinationRect.Should().Be(new RelativeRect(new Rect(default, DashboardTheme.Light.BackgroundImage.Size), RelativeUnit.Absolute));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Repository_search_should_keep_native_rim_and_underline_while_typing_and_selecting()
    {
        UserRepositoriesList list = new();
        RepositoryHistorySnapshot snapshot = new([], []);
        list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        TextBox other = new();
        Window window = new() { Width = 700, Height = 400, Content = new StackPanel { Children = { list, other } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            TextBox search = list.GetTestAccessor().Search;
            Border rim = search.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "NativeFrame");
            Border underline = search.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "NativeUnderline");
            TextBlock placeholder = search.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Name == "PART_Placeholder");
            other.Focus();
            Dispatcher.UIThread.RunJobs();
            placeholder.IsVisible.Should().BeTrue();
            ((Avalonia.Media.ISolidColorBrush)rim.BorderBrush!).Color.Should().Be(Avalonia.Media.Color.FromRgb(236, 236, 236));
            ((Avalonia.Media.ISolidColorBrush)underline.Background!).Color.Should().Be(Avalonia.Media.Color.FromRgb(131, 131, 131));
            underline.Height.Should().Be(1);
            search.Focus();
            window.KeyTextInput("feature");
            Dispatcher.UIThread.RunJobs();
            search.Text.Should().Be("feature");
            search.CaretIndex.Should().Be(7);
            Control textPresenter = search.GetVisualDescendants().OfType<Control>().Single(control => control.Name == "PART_TextPresenter");
            textPresenter.TranslatePoint(default, search)!.Value.X.Should().Be(2);
            placeholder.IsVisible.Should().BeFalse();
            ((Avalonia.Media.ISolidColorBrush)rim.BorderBrush!).Color.Should().Be(Avalonia.Media.Color.FromRgb(236, 236, 236));
            ((Avalonia.Media.ISolidColorBrush)underline.Background!).Color.Should().Be(Avalonia.Media.Color.FromRgb(0, 103, 192));
            underline.Height.Should().Be(2);
            search.SelectAll();
            (search.SelectionEnd - search.SelectionStart).Should().Be(7);
            window.KeyTextInput("main");
            Dispatcher.UIThread.RunJobs();
            search.Text.Should().Be("main");
            search.ContextFlyout.Should().NotBeNull("native editing operations must remain available through the framework text box");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(true)]
    [TestCase(false)]
    public void Dashboard_should_focus_search_after_the_initial_window_activation(bool refreshBeforeShowing)
    {
        RepositoryHistorySnapshot snapshot = new([], []);
        Dashboard dashboard = new();
        dashboard.Initialize(CreateController(snapshot), CreateHistory(snapshot));
        if (refreshBeforeShowing)
        {
            dashboard.RefreshContent();
        }

        Window window = new() { Width = 686, Height = 400, Content = dashboard };
        try
        {
            window.Show();
            window.Activate();
            Dispatcher.UIThread.RunJobs();
            if (!refreshBeforeShowing)
            {
                dashboard.RefreshContent();
                Dispatcher.UIThread.RunJobs();
            }

            window.UpdateLayout();
            dashboard.GetTestAccessor().Repositories.GetTestAccessor().Search.IsFocused.Should().BeTrue();
            using AvaloniaControlStateDriver driver = AvaloniaControlStateDriver.Apply(dashboard,
                new CaptureStatePlan { Id = "normal", Kind = CaptureStateKind.Normal });
            dashboard.GetTestAccessor().Repositories.GetTestAccessor().Search.IsFocused.Should().BeTrue(
                "normal capture must preserve the product's visible-change focus");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(12)]
    [TestCase(16)]
    [TestCase(24)]
    public void Repository_groups_should_use_native_header_and_inner_tile_bounds(double fontSize)
    {
        Repository recent = new(@"C:\repos\recent");
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        RepositoryHistorySnapshot snapshot = new(
            [new RepositoryHistoryEntry(recent, "recent", "main", IsFavourite: false, IsAnchored: false)],
            [new RepositoryHistoryEntry(favourite, "favourite", "main", IsFavourite: true, IsAnchored: false)]);
        UserRepositoriesList list = new() { FontSize = fontSize };
        list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Width = 700, Height = 500, Content = list };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            ListBoxItem[] containers = [.. list.GetVisualDescendants().OfType<ListBoxItem>()];
            ListBoxItem[] headers = [.. containers.Where(item => item.Content is UserRepositoriesList.RepositoryGroupItem)];
            headers.Should().HaveCount(2);
            foreach (ListBoxItem container in headers)
            {
                Grid header = container.GetVisualDescendants().OfType<Grid>()
                    .Single(grid => grid.Children.OfType<Button>().Any());
                header.Bounds.Height.Should().Be(21);
                TextBlock caption = header.Children.OfType<TextBlock>().Single();
                caption.Bounds.X.Should().Be(10);
                caption.Bounds.Y.Should().Be(2);
            }

            ListBoxItem firstTile = containers.Single(item => item.Content is UserRepositoriesList.RepositoryListItem { IsFavourite: false });
            Grid paintedBounds = firstTile.GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.Classes.Contains("repository-tile-content"));
            paintedBounds.TranslatePoint(default, firstTile)!.Value.Should().Be(new Point(2, 1));
            paintedBounds.Bounds.Width.Should().Be(firstTile.Bounds.Width - 4);
            paintedBounds.Bounds.Height.Should().Be(firstTile.Bounds.Height - 2);
            firstTile.Bounds.Y.Should().Be(headers[0].Bounds.Bottom);
            headers[1].Bounds.Y.Should().Be(firstTile.Bounds.Bottom);
            Grid secondHeader = headers[1].GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.Children.OfType<Button>().Any());
            secondHeader.Bounds.Y.Should().Be(3);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(12)]
    [TestCase(19)]
    [TestCase(27)]
    public void Dashboard_glyph_padding_should_follow_the_current_font_metric_not_a_captured_offset(int fontSize)
    {
        TextBlock text = new() { FontFamily = new Avalonia.Media.FontFamily("Segoe UI"), FontSize = fontSize };
        Thickness padding = WinFormsTextMeasurer.GetTextRendererPadding(text);
        padding.Left.Should().BeGreaterThan(0);
        padding.Right.Should().BeGreaterThanOrEqualTo(padding.Left);
        padding.Left.Should().Be(Math.Ceiling(padding.Left));
        padding.Right.Should().Be(Math.Ceiling(padding.Right));
        padding.Top.Should().Be(0);
        padding.Bottom.Should().Be(0);
        Size padded = WinFormsTextMeasurer.MeasureTextRenderer(text, "Clone repository");
        Size unpadded = WinFormsTextMeasurer.MeasureSize(text, "Clone repository");
        padded.Width.Should().Be(unpadded.Width + padding.Left + padding.Right);
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Dashboard_should_recreate_anonymous_source_links_in_order_and_inherit_the_selected_font()
    {
        RepositoryHistorySnapshot snapshot = new([], []);
        Dashboard dashboard = new();
        dashboard.Initialize(CreateController(snapshot), CreateHistory(snapshot));
        Window window = new() { Width = 686, Height = 550, Content = dashboard };
        try
        {
            window.Show();
            dashboard.RefreshContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Border start = dashboard.FindControl<Border>("flpnlStart")!;
            Border contribute = dashboard.FindControl<Border>("flpnlContribute")!;
            WinFormsControls.LinkLabel[] original = [.. ((StackPanel)start.Child!).Children.OfType<WinFormsControls.LinkLabel>()];
            original.Select(link => link.Text).Take(3).Should().Equal("Create new repository", "Open repository", "Clone repository");
            original.Should().OnlyContain(link => string.IsNullOrEmpty(link.Name));
            original.Select(link => link.TabIndex).Should().Equal(Enumerable.Range(0, original.Length));
            original.Should().OnlyContain(link => link.Padding == new Thickness(24, 3, 3, 3)
                && link.Margin == new Thickness(3, 0, 3, 8)
                && link.FontSize == AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size));
            WinFormsControls.LinkLabel[] contributions = [.. ((StackPanel)contribute.Child!).Children.OfType<WinFormsControls.LinkLabel>()];
            contributions.Select(link => link.Text).Should().Equal("Develop", "Donate", "Translate", "Issues");
            contributions.Select(link => link.TabIndex).Should().Equal(1, 2, 3, 4);

            dashboard.RefreshContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            ((StackPanel)start.Child!).Children.OfType<WinFormsControls.LinkLabel>()
                .Should().NotContain(link => original.Contains(link));
            ((StackPanel)contribute.Child!).Children.OfType<WinFormsControls.LinkLabel>().Should().HaveCount(4);

            CaptureNode root = new AvaloniaControlTreeReader(dashboard, 1)
                .ReadPrimary(dashboard, PixelSize.FromSize(AvaloniaControlTreeReader.GetSourceClientSize(dashboard), 1)).Root;
            CaptureNode[] links = [.. Descendants(root).Where(node => node.Type == "System.Windows.Forms.LinkLabel")];
            links.Should().HaveCount(original.Length + 4);
            links.Where(node => node.FieldName != null || node.Name != null || node.Children.Count != 0
                || node.AutoSize != true || node.Alignment != "MiddleLeft" || node.TabStop != true)
                .Select(node => $"{node.Id}: field={node.FieldName}, name={node.Name}, children={node.Children.Count}, auto={node.AutoSize}, alignment={node.Alignment}, tab={node.TabStop}")
                .Should().BeEmpty();
            links.Select(node => node.Text).Should().Contain("Clone repository");
            links.Select(node => node.FlatStyle).Should().OnlyContain(flatStyle => flatStyle == null,
                "the native capture schema reports FlatStyle only for button controls, not LinkLabel");
            CaptureNode capturedClone = links.Single(node => node.Text == "Clone repository");
            capturedClone.Colors.Foreground.Should().Be("#FF1E1E1E");
            capturedClone.Colors.Background.Should().Be("#FFDBEBF8");
            capturedClone.Colors.DisabledBackground.Should().Be(capturedClone.Colors.Background);
            capturedClone.Colors.Additional["controlForeground"].Should().Be("#FF000000");
            start.Margin.Should().Be(default(Thickness), "native Dock ignores the stored flow-panel margin");
            Descendants(root).Single(node => node.FieldName == "flpnlStart").Margin!.Dip.Left.Should().Be(2);
            Descendants(root).Single(node => node.FieldName == "pbLogo").Margin!.Dip.Left.Should().Be(3);
        }
        finally
        {
            window.Close();
        }

        return;

        static IEnumerable<CaptureNode> Descendants(CaptureNode node)
            => node.Children.SelectMany(child => new[] { child }.Concat(Descendants(child)));
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Dashboard_links_should_keep_chrome_transparent_in_hover_and_pressed_states()
    {
        RepositoryHistorySnapshot snapshot = new([], []);
        Dashboard dashboard = new();
        dashboard.Initialize(CreateController(snapshot), CreateHistory(snapshot));
        Window window = new() { Width = 686, Height = 550, Content = dashboard };
        try
        {
            window.Show();
            dashboard.RefreshContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            WinFormsControls.LinkLabel link = (WinFormsControls.LinkLabel)dashboard.GetTestAccessor().Open;
            Avalonia.Controls.Presenters.ContentPresenter presenter = link.GetVisualDescendants()
                .OfType<Avalonia.Controls.Presenters.ContentPresenter>().Single();
            Point point = link.TranslatePoint(new Point(8, 8), window)!.Value;
            window.MouseMove(point);
            Dispatcher.UIThread.RunJobs();
            link.IsPointerOver.Should().BeTrue();
            ((Avalonia.Media.ISolidColorBrush)link.Foreground!).Color.Should().Be(DashboardTheme.Light.AccentedText);
            ((Avalonia.Media.ISolidColorBrush)presenter.Background!).Color.A.Should().Be(0);
            window.MouseDown(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            ((Avalonia.Media.ISolidColorBrush)link.Foreground!).Color.Should().Be(Avalonia.Media.Colors.Red);
            ((Avalonia.Media.ISolidColorBrush)presenter.Background!).Color.A.Should().Be(0);
            window.MouseUp(new Point(1, 1), MouseButton.Left);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(669)]
    [TestCase(1000)]
    public void Dashboard_should_allocate_source_percentage_columns_in_whole_client_pixels(int width)
    {
        Dashboard dashboard = new();
        Window window = new() { Width = width, Height = 600, Content = dashboard };
        try
        {
            window.Show();
            dashboard.RefreshContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Grid layout = dashboard.FindControl<Grid>("tableLayoutPanel1")!;
            double remaining = layout.Bounds.Width - 213;
            layout.ColumnDefinitions[0].ActualWidth.Should().Be(Math.Floor(remaining * 7.142857 / 100));
            layout.ColumnDefinitions[1].ActualWidth.Should().Be(213);
            layout.ColumnDefinitions[2].ActualWidth.Should().Be(Math.Floor(remaining * 85.71428 / 100));
            dashboard.Margin.Should().Be(default(Thickness));
            CaptureNode tree = new AvaloniaControlTreeReader(dashboard, 1)
                .ReadPrimary(dashboard, new PixelSize(width, 600)).Root;
            tree.Margin!.Dip.Left.Should().Be(0);
            tree.Margin.Dip.Top.Should().Be(0);
            layout.Margin.Should().Be(default(Thickness));
            dashboard.FindControl<Control>("pnlLeft")!.Margin.Should().Be(default(Thickness));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Repository_heading_should_use_native_text_height_and_original_column_defaults()
    {
        UserRepositoriesList control = new();
        Window window = new() { Width = 451, Height = 350, Content = control };
        try
        {
            window.Show();
            window.UpdateLayout();
            TextBlock heading = control.FindControl<TextBlock>("lblRecentRepositories")!;
            heading.Height.Should().Be(Math.Ceiling(WinFormsTextMeasurer.MeasureTextRenderer(heading, heading.Text!).Height));
            control.FindControl<Grid>("tableLayoutPanel1")!.Margin.Should().Be(default(Thickness));
            foreach (string field in new[] { "clmhdrPath", "clmhdrBranch", "clmhdrCategory" })
            {
                control.FindControl<Control>(field)!.Width.Should().Be(60);
            }
        }
        finally
        {
            window.Close();
        }
    }

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Dashboard_logo_should_keep_the_authored_PictureBox_frame_while_zooming_its_image()
    {
        Dashboard dashboard = new();
        dashboard.Resources["GitExtensionsControlForegroundBrush"] = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Magenta);
        dashboard.Resources["GitExtensionsKnownColorControlTextBrush"] = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.White);
        Window window = new() { Width = 686, Height = 600, Content = dashboard };
        try
        {
            window.Show();
            dashboard.RefreshContent();
            window.UpdateLayout();
            WinFormsControls.PictureBox logo = dashboard.FindControl<WinFormsControls.PictureBox>("pbLogo")!;
            ((Avalonia.Media.SolidColorBrush)dashboard.Foreground!).Color.Should().Be(Avalonia.Media.Colors.White);
            logo.Bounds.Size.Should().Be(new Size(185, 44));
            Image image = (Image)logo.Child!;
            logo.SizeMode.Should().Be(WinFormsControls.PictureBoxSizeMode.Zoom);
            image.Stretch.Should().Be(Avalonia.Media.Stretch.Uniform);
            image.Bounds.Width.Should().BeLessThan(logo.Bounds.Width);
            image.Bounds.Height.Should().Be(logo.Bounds.Height);
            logo.SizeMode = WinFormsControls.PictureBoxSizeMode.CenterImage;
            logo.SizeMode.Should().Be(WinFormsControls.PictureBoxSizeMode.CenterImage);
            image.Stretch.Should().Be(Avalonia.Media.Stretch.None);
            logo.SizeMode = WinFormsControls.PictureBoxSizeMode.Normal;
            logo.SizeMode.Should().Be(WinFormsControls.PictureBoxSizeMode.Normal);
            image.HorizontalAlignment.Should().Be(Avalonia.Layout.HorizontalAlignment.Left);
            logo.SizeMode = WinFormsControls.PictureBoxSizeMode.Zoom;
            window.UpdateLayout();
            logo.Bounds.Size.Should().Be(new Size(185, 44));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase(390)]
    [TestCase(540)]
    [TestCase(840)]
    public void Repository_auto_column_should_preserve_the_Designer_preferred_width_and_receive_surplus(int width)
    {
        UserRepositoriesList list = new();
        Window window = new() { Width = width, Height = 350, Content = list };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Grid table = list.FindControl<Grid>("tableLayoutPanel2")!;
            double columnWidth = Math.Max(445, width - 40);
            table.Bounds.Width.Should().Be(width - 40);
            table.ClipToBounds.Should().BeTrue("native child HWNDs cannot paint outside the table's client rectangle");
            table.ColumnDefinitions[0].ActualWidth.Should().Be(columnWidth);
            list.GetTestAccessor().List.Bounds.Width.Should().Be(columnWidth);
            list.GetTestAccessor().Search.Bounds.Width.Should().Be(columnWidth - 6);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Repository_tile_should_paint_the_live_hover_brush_when_hovered_or_selected_without_focus()
    {
        Repository repository = new(@"C:\repos\recent");
        RepositoryHistorySnapshot snapshot = new(
            [new RepositoryHistoryEntry(repository, "recent", "main", IsFavourite: false, IsAnchored: false)], []);
        UserRepositoriesList list = new();
        list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        TextBox otherInput = new();
        Window window = new() { Width = 700, Height = 400, Content = new StackPanel { Children = { list, otherInput } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            ListBoxItem tile = list.GetVisualDescendants().OfType<ListBoxItem>()
                .Single(item => item.Content is UserRepositoriesList.RepositoryListItem);
            Grid paintedBounds = tile.GetVisualDescendants().OfType<Grid>()
                .Single(item => item.Classes.Contains("repository-tile-content"));
            list.HoverColor = Avalonia.Media.Colors.Cyan;
            window.MouseMove(tile.TranslatePoint(new Point(10, 10), window)!.Value);
            Dispatcher.UIThread.RunJobs();
            tile.IsPointerOver.Should().BeTrue();
            ((Avalonia.Media.SolidColorBrush)paintedBounds.Background!).Color.Should().Be(Avalonia.Media.Colors.Cyan);
            window.MouseMove(new Point(699, 399));
            list.GetTestAccessor().List.SelectedItem = tile.Content;
            tile.Focus();
            Dispatcher.UIThread.RunJobs();
            ((Avalonia.Media.SolidColorBrush)paintedBounds.Background!).Color.Should().Be(Avalonia.Media.Colors.Cyan);
            otherInput.Focus();
            Dispatcher.UIThread.RunJobs();
            ((Avalonia.Media.SolidColorBrush)paintedBounds.Background!).Color.Should().Be(Avalonia.Media.Colors.Cyan);
            list.HoverColor = Avalonia.Media.Colors.Gold;
            Dispatcher.UIThread.RunJobs();
            ((Avalonia.Media.SolidColorBrush)paintedBounds.Background!).Color.Should().Be(Avalonia.Media.Colors.Gold);
            tile.FocusAdorner.Should().BeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Dashboard_capture_should_read_live_colors_instead_of_overwriting_them_with_Designer_defaults()
    {
        UserRepositoriesList list = new();
        Window window = new() { Width = 451, Height = 283, Content = list };
        try
        {
            window.Show();
            list.HeaderColor = Avalonia.Media.Color.FromRgb(12, 34, 56);
            list.HeaderBackColor = Avalonia.Media.Color.FromRgb(78, 90, 12);
            list.ForeColor = Avalonia.Media.Color.FromRgb(34, 56, 78);
            list.MainBackColor = Avalonia.Media.Color.FromRgb(90, 12, 34);
            list.SearchBackColor = Avalonia.Media.Color.FromRgb(23, 45, 67);
            window.UpdateLayout();
            CaptureNode root = new AvaloniaControlTreeReader(list, 1)
                .ReadPrimary(list, PixelSize.FromSize(list.Bounds.Size, 1)).Root;
            CaptureNode heading = Nodes(root).Single(node => node.FieldName == "lblRecentRepositories");
            CaptureNode header = Nodes(root).Single(node => node.FieldName == "pnlHeader");
            heading.Colors.Foreground.Should().Be("#FF0C2238");
            header.Colors.Background.Should().Be("#FF4E5A0C");
            CaptureNode search = Nodes(root).Single(node => node.FieldName == "textBoxSearch");
            search.Colors.Background.Should().Be("#FF172D43");
            search.Colors.DisabledBackground.Should().Be("#FF172D43");
            Nodes(root).Single(node => node.FieldName == "listView1").Colors.Foreground.Should().Be("#FF22384E");
            CaptureNode repositoryList = Nodes(root).Single(node => node.FieldName == "listView1");
            repositoryList.ControlKind.Should().Be("list");
            repositoryList.Text.Should().BeEmpty();
            repositoryList.Selected.Should().BeNull();
            foreach (CaptureColumn column in repositoryList.Columns)
            {
                column.Colors.Foreground.Should().Be("#FF22384E");
                column.Colors.Border.Should().BeNull();
                column.Colors.SelectionBackground.Should().NotBeNull();
                column.Colors.SelectionForeground.Should().NotBeNull();
            }

            foreach (string field in new[] { "menuStripRecentMenu", "mnuTop" })
            {
                CaptureNode menu = Nodes(root).Single(node => node.FieldName == field);
                menu.Colors.Foreground.Should().Be("#FF22384E");
                menu.Colors.Background.Should().Be("#FF5A0C22");
                menu.Colors.Border.Should().BeNull("native menu items expose no border-color property");
            }
        }
        finally
        {
            window.Close();
        }

        return;

        static IEnumerable<CaptureNode> Nodes(CaptureNode node)
        {
            yield return node;
            foreach (CaptureNode child in node.Children)
            {
                foreach (CaptureNode descendant in Nodes(child))
                {
                    yield return descendant;
                }
            }
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Dashboard_should_scroll_the_source_minimum_layout_instead_of_clipping_commands()
    {
        RepositoryHistorySnapshot snapshot = new([], []);
        Dashboard dashboard = new();
        dashboard.Initialize(CreateController(snapshot), CreateHistory(snapshot));
        Window window = new() { Width = 686, Height = 358, Content = dashboard };
        try
        {
            window.Show();
            dashboard.RefreshContent();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            Border start = dashboard.FindControl<Border>("flpnlStart")!;
            Border contribute = dashboard.FindControl<Border>("flpnlContribute")!;
            Grid layout = dashboard.FindControl<Grid>("tableLayoutPanel1")!;
            Button clone = ((StackPanel)start.Child!).Children.OfType<Button>().ElementAt(2);
            ScrollViewer scroll = dashboard.GetVisualDescendants().OfType<ScrollViewer>().First();
            start.MinHeight.Should().BeGreaterThan(0);
            contribute.Height.Should().BeGreaterThan(0);
            layout.MinHeight.Should().Be(68 + start.MinHeight + contribute.Height);
            scroll.Extent.Height.Should().BeGreaterThan(scroll.Viewport.Height);
            AvaloniaControlTreeReader.GetSourceClientSize(dashboard).Should().Be(scroll.Viewport);
            CaptureNode captureRoot = new AvaloniaControlTreeReader(dashboard, 1)
                .ReadPrimary(dashboard, PixelSize.FromSize(scroll.Viewport, 1)).Root;
            captureRoot.BoundsDip.Width.Should().Be((decimal)scroll.Viewport.Width);
            captureRoot.ClientSizeDip.Width.Should().Be((decimal)scroll.Viewport.Width);
            clone.Bounds.Height.Should().BeGreaterThan(0);
            clone.TranslatePoint(default, start)!.Value.Y.Should().BeGreaterThan(0);
            Grid content = (Grid)clone.Content!;
            content.Children.OfType<Image>().Single().Margin.Left.Should().Be(-24 + 2);
            content.Children.OfType<TextBlock>().Single().Text.Should().Be("Clone repository");

            // Rehost at a larger client size; the headless window does not resize like a desktop window.
            window.Content = null;
            window.Close();
            window = new Window { Width = 686, Height = layout.MinHeight + 100, Content = dashboard };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            scroll.Extent.Height.Should().BeLessThanOrEqualTo(scroll.Viewport.Height);
            AvaloniaControlTreeReader.GetSourceClientSize(dashboard).Should().Be(scroll.Viewport);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Repository_list_should_focus_its_first_input_and_publish_its_runtime_foreground()
    {
        UserRepositoriesList list = new();
        Button otherInput = new() { Content = "Other input" };
        Window window = new() { Content = new StackPanel { Children = { list, otherInput } } };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            // Focus is forwarded to the first child, so the container itself loses focus.
            list.Focus();
            list.GetTestAccessor().Search.IsFocused.Should().BeTrue();
            list.GetTestAccessor().Search.PlaceholderText.Should().BeNull();
            otherInput.Focus();
            list.GetTestAccessor().Search.PlaceholderText.Should().Be("Search repositories...");
            Avalonia.Media.Color foreground = Avalonia.Media.Color.FromRgb(12, 34, 56);
            list.ForeColor = foreground;
            ((Avalonia.Media.SolidColorBrush)list.Foreground!).Color.Should().Be(foreground);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Repository_list_should_show_shared_recent_favourite_and_branch_data()
    {
        Repository recent = new(@"C:\repos\recent");
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        RepositoryHistorySnapshot snapshot =
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(recent, "recent", "main", IsFavourite: false, IsAnchored: false)],
                [new RepositoryHistoryEntry(favourite, "favourite", "feature", IsFavourite: true, IsAnchored: false)]);
        IUserRepositoriesListController controller = CreateController(snapshot);
        IRepositoryHistoryUIService history = CreateHistory(snapshot);
        UserRepositoriesList list = new();

        list.Initialize(controller, history, () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);

        UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
        accessor.List.Items.OfType<UserRepositoriesList.RepositoryGroupItem>().Select(row => row.Name)
            .Should().Contain("Recent repositories", "Team");
        accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>().Select(row => row.BranchName)
            .Should().Contain("main", "feature");

        accessor.Search.Text = "feature";

        accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>()
            .Should().ContainSingle(row => row.Repository.Repo.Path == favourite.Path);
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Category_header_action_should_remain_enabled_and_hover_color_should_be_applied()
    {
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        RepositoryHistorySnapshot snapshot = new(
            [],
            [new RepositoryHistoryEntry(favourite, "favourite", "main", IsFavourite: true, IsAnchored: false)]);
        UserRepositoriesList list = new();
        list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Content = list };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            Button categoryAction = list.GetVisualDescendants()
                .OfType<Button>()
                .Single(button => button.Classes.Contains("dashboard-group-action"));
            categoryAction.IsEffectivelyEnabled.Should().BeTrue();
            categoryAction.IsVisible.Should().BeFalse();
            Grid groupHeader = (Grid)categoryAction.Parent!;
            groupHeader.Children.OfType<Border>().Single().Bounds.Width.Should().BeGreaterThan(0);
            window.MouseMove(groupHeader.TranslatePoint(new Point(20, 10), window)!.Value);
            Dispatcher.UIThread.RunJobs();
            categoryAction.IsVisible.Should().BeTrue();
            window.MouseMove(categoryAction.TranslatePoint(new Point(categoryAction.Bounds.Width / 2, 10), window)!.Value);
            Dispatcher.UIThread.RunJobs();
            categoryAction.IsVisible.Should().BeTrue("moving onto the task link must not dismiss it");
            window.MouseMove(new Point(window.Bounds.Width - 1, window.Bounds.Height - 1));
            Dispatcher.UIThread.RunJobs();
            categoryAction.IsVisible.Should().BeFalse();

            window.MouseMove(groupHeader.TranslatePoint(new Point(20, 10), window)!.Value);
            Dispatcher.UIThread.RunJobs();
            Point actionPoint = categoryAction.TranslatePoint(new Point(categoryAction.Bounds.Width / 2, 10), window)!.Value;
            window.MouseDown(actionPoint, MouseButton.Left);
            window.MouseUp(actionPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            ContextMenu categoryMenu = list.FindControl<ContextMenu>("contextMenuStripCategory")!;
            categoryMenu.IsOpen.Should().BeTrue("the native group task opens its category menu");
            list.FindControl<MenuItem>("tsmiCategoryRename")!.IsVisible.Should().BeTrue();
            list.FindControl<MenuItem>("tsmiCategoryDelete")!.IsVisible.Should().BeTrue();
            list.FindControl<MenuItem>("tsmiCategoryClear")!.IsVisible.Should().BeFalse();
            categoryMenu.Close();

            Avalonia.Media.Color hoverColor = Avalonia.Media.Color.FromRgb(1, 2, 3);
            list.HoverColor = hoverColor;
            ((Avalonia.Media.SolidColorBrush)list.Resources["DashboardRepositoryHoverBrush"]!).Color
                .Should().Be(hoverColor);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [TestCase("main")]
    [TestCase("")]
    public void Categorized_repository_should_use_the_source_tile_geometry_and_star_overlay(string branchName)
    {
        Repository favourite = new(@"C:\repos\favourite") { Category = "Team" };
        RepositoryHistorySnapshot snapshot = new(
            [],
            [new RepositoryHistoryEntry(favourite, "favourite", branchName, IsFavourite: true, IsAnchored: false)]);
        UserRepositoriesList list = new();
        list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Content = list };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            ListBoxItem container = list.GetVisualDescendants()
                .OfType<ListBoxItem>()
                .Single(item => item.Content is UserRepositoriesList.RepositoryListItem);
            Grid row = container.GetVisualDescendants().OfType<Grid>()
                .Single(grid => grid.Classes.Contains("repository-tile-content"));
            row.MinHeight.Should().Be(48);
            row.Width.Should().Be(((UserRepositoriesList.RepositoryListItem)container.Content!).TileSize.Width - 4);
            row.Margin.Should().Be(new Thickness(2, 1));
            Image[] images = [.. row.Children.OfType<Image>()];

            images.Should().HaveCount(2);
            images.Single(image => ReferenceEquals(image.Source, GitUI.Properties.Images.DashboardFolderGit))
                .Width.Should().Be(GitUI.Properties.Images.DashboardFolderGit.Size.Width);
            images.Single(image => ReferenceEquals(image.Source, GitUI.Properties.Images.Star))
                .Width.Should().Be(16);
            images.Should().ContainSingle(image => ReferenceEquals(image.Source, GitUI.Properties.Images.Star));
            images[0].Source.Should().BeSameAs(GitUI.Properties.Images.Star);
            row.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "favourite").FontWeight
                .Should().Be(Avalonia.Media.FontWeight.Normal);
            // The source binder adds only path and branch subitems, not a category text row.
            row.GetVisualDescendants().OfType<TextBlock>().Should().HaveCount(2);
            row.GetVisualDescendants().OfType<TextBlock>().Should().NotContain(text => text.Text == "Team");
            row.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == branchName)
                .IsVisible.Should().Be(!string.IsNullOrWhiteSpace(branchName));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Appearance_changes_should_repaint_existing_tiles_without_reloading_or_clearing_selection()
    {
        Repository repository = new(@"C:\repos\recent");
        RepositoryHistorySnapshot snapshot = new(
            [new RepositoryHistoryEntry(repository, "recent", "main", IsFavourite: false, IsAnchored: false)], []);
        IUserRepositoriesListController controller = CreateController(snapshot);
        UserRepositoriesList list = new();
        list.Initialize(controller, CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Content = list };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
            accessor.List.SelectedItem = accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>().Single();
            object selected = accessor.List.SelectedItem;
            TextBlock caption = list.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "recent");
            TextBlock branch = list.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == "main");
            controller.ClearReceivedCalls();

            list.ForeColor = Avalonia.Media.Colors.Red;
            list.BranchNameColor = Avalonia.Media.Colors.Green;
            list.FavouriteColor = Avalonia.Media.Colors.Gold;
            list.HoverColor = Avalonia.Media.Colors.Cyan;

            accessor.List.SelectedItem.Should().BeSameAs(selected);
            ((Avalonia.Media.SolidColorBrush)caption.Foreground!).Color.Should().Be(Avalonia.Media.Colors.Red);
            ((Avalonia.Media.SolidColorBrush)branch.Foreground!).Color.Should().Be(Avalonia.Media.Colors.Green);
            controller.DidNotReceive().PreRenderRepositories(Arg.Any<string>());
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    public void Initial_palette_assignments_should_paint_their_declared_defaults()
    {
        UserRepositoriesList list = new();
        list.MainBackColor = list.MainBackColor;
        list.SearchBackColor = list.SearchBackColor;
        list.HeaderColor = list.HeaderColor;
        TextBlock heading = list.FindControl<TextBlock>("lblRecentRepositories")!;

        ((Avalonia.Media.SolidColorBrush)list.Background!).Color.Should().Be(list.MainBackColor);
        ((Avalonia.Media.SolidColorBrush)list.GetTestAccessor().List.Background!).Color.Should().Be(list.MainBackColor);
        ((Avalonia.Media.SolidColorBrush)list.GetTestAccessor().Search.Background!).Color.Should().Be(list.SearchBackColor);
        ((Avalonia.Media.SolidColorBrush)heading.Foreground!).Color.Should().Be(list.HeaderColor);
        heading.FontFamily.Name.Should().Be(AppSettings.Font.Name);
        heading.FontSize.Should().Be(AvaloniaFontSettings.ToDeviceIndependentPixels(AppSettings.Font.Size + 5.5F));
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [NonParallelizable]
    public void Repository_tile_should_measure_its_source_width_when_no_minimum_is_configured()
    {
        int originalMinimum = AppSettings.RecentReposComboMinWidth;
        try
        {
            AppSettings.RecentReposComboMinWidth = 0;
            const string caption = "a deliberately long repository caption used for source-sized dashboard tiles";
            Repository repository = new(@"C:\repos\long");
            RepositoryHistorySnapshot snapshot = new(
                [new RepositoryHistoryEntry(repository, caption, "main", IsFavourite: false, IsAnchored: false)],
                []);
            UserRepositoriesList list = new();
            list.Initialize(CreateController(snapshot), CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
            list.ShowRecentRepositories(reloadData: false);
            Window window = new() { Width = 900, Height = 260, Content = list };

            try
            {
                window.Show();
                Dispatcher.UIThread.RunJobs();
                Grid row = list.GetVisualDescendants()
                    .OfType<ListBoxItem>()
                    .Single(item => item.Content is UserRepositoriesList.RepositoryListItem)
                    .GetVisualDescendants()
                    .OfType<Grid>()
                    .Single(grid => grid.Children.OfType<StackPanel>().Any());
                TextBlock captionProbe = row.GetVisualDescendants().OfType<TextBlock>().Single(text => text.Text == caption);
                double measuredCaption = WinFormsTextMeasurer.MeasureTextRenderer(captionProbe, caption).Width;

                row.Width.Should().Be(Math.Ceiling(measuredCaption + GitUI.Properties.Images.DashboardFolderGit.Size.Width + 50) - 4);
                row.MinHeight.Should().BeGreaterThanOrEqualTo(48);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            AppSettings.RecentReposComboMinWidth = originalMinimum;
        }
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Selecting_a_valid_dashboard_repository_should_raise_the_module_transition()
    {
        string repositoryPath = FindRepositoryRoot();
        Repository repository = new(repositoryPath);
        RepositoryHistorySnapshot snapshot =
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "selected", "main", IsFavourite: false, IsAnchored: false)],
                []);
        IUserRepositoriesListController controller = CreateController(snapshot);
        IRepositoryHistoryUIService history = CreateHistory(snapshot);
        IGitExecutorProvider executorProvider = Substitute.For<IGitExecutorProvider>();
        IGitExecutor executor = Substitute.For<IGitExecutor>();
        executor.WorkingDir.Returns(repositoryPath);
        executor.GetGitDirectory().Returns(Path.Join(repositoryPath, ".git"));
        executorProvider.GetExecutor(repositoryPath).Returns(executor);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IGitExecutorProvider)).Returns(executorProvider);
        UserRepositoriesList list = new();
        GitModuleEventArgs? transition = null;
        list.GitModuleChanged += (_, e) => transition = e;
        list.Initialize(controller, history, () => commands);
        list.ShowRecentRepositories(reloadData: false);
        UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
        accessor.List.SelectedItem = accessor.List.Items
            .OfType<UserRepositoriesList.RepositoryListItem>()
            .Single();

        accessor.OpenSelected();

        transition.Should().NotBeNull();
        Path.TrimEndingDirectorySeparator(transition!.GitModule.WorkingDir)
            .Should().Be(Path.TrimEndingDirectorySeparator(repositoryPath));
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Single_clicking_a_valid_dashboard_repository_should_raise_the_module_transition()
    {
        string repositoryPath = FindRepositoryRoot();
        Repository repository = new(repositoryPath);
        RepositoryHistorySnapshot snapshot =
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "selected", "main", IsFavourite: false, IsAnchored: false)],
                []);
        IUserRepositoriesListController controller = CreateController(snapshot);
        IRepositoryHistoryUIService history = CreateHistory(snapshot);
        IGitExecutorProvider executorProvider = Substitute.For<IGitExecutorProvider>();
        IGitExecutor executor = Substitute.For<IGitExecutor>();
        executor.WorkingDir.Returns(repositoryPath);
        executor.GetGitDirectory().Returns(Path.Join(repositoryPath, ".git"));
        executorProvider.GetExecutor(repositoryPath).Returns(executor);
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.GetService(typeof(IGitExecutorProvider)).Returns(executorProvider);
        UserRepositoriesList list = new();
        GitModuleEventArgs? transition = null;
        list.GitModuleChanged += (_, e) => transition = e;
        list.Initialize(controller, history, () => commands);
        list.ShowRecentRepositories(reloadData: false);
        Window window = new()
        {
            Width = 560,
            Height = 260,
            Content = list,
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ListBoxItem repositoryRow = list.GetVisualDescendants()
                .OfType<ListBoxItem>()
                .Single(item => item.Content is UserRepositoriesList.RepositoryListItem);
            Avalonia.Point clickPoint = Avalonia.VisualExtensions.TranslatePoint(
                repositoryRow,
                new Avalonia.Point(repositoryRow.Bounds.Width / 2, repositoryRow.Bounds.Height / 2),
                window) ?? throw new InvalidOperationException("The repository row position was not available.");

            window.MouseDown(clickPoint, MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(clickPoint, MouseButton.Left, RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();

            transition.Should().NotBeNull("WinForms uses ItemActivation.OneClick for this list");
            Path.TrimEndingDirectorySeparator(transition!.GitModule.WorkingDir)
                .Should().Be(Path.TrimEndingDirectorySeparator(repositoryPath));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Attached_repository_list_should_replace_rows_after_branch_cache_refresh()
    {
        Repository repository = new(@"C:\repos\recent");
        RepositoryHistorySnapshot initial =
            new RepositoryHistorySnapshot(
                [new RepositoryHistoryEntry(repository, "recent", BranchName: null, IsFavourite: false, IsAnchored: false)],
                []);
        RepositoryHistorySnapshot refreshed = new(
            [new RepositoryHistoryEntry(repository, "recent", "main", IsFavourite: false, IsAnchored: false)],
            []);
        IUserRepositoriesListController controller = CreateController(initial);
        IRepositoryHistoryUIService history = CreateHistory(initial);
        UserRepositoriesList list = new();
        list.Initialize(controller, history, () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        Window window = new() { Content = list };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ConfigureController(controller, refreshed);

            history.HistoryChanged += Raise.Event<EventHandler>();
            Dispatcher.UIThread.RunJobs();

            list.GetTestAccessor().List.Items
                .OfType<UserRepositoriesList.RepositoryListItem>()
                .Select(item => item.BranchName)
                .Should().Contain("main");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Dashboard_should_preserve_the_original_translation_strings()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        Dashboard dashboard = new();

        dashboard.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(
            nameof(Dashboard), "_createRepository", "Text", "Create new repository");
        translation.Received(1).AddTranslationItem(
            nameof(Dashboard), "_openRepository", "Text", "Open repository");
        translation.Received(1).AddTranslationItem(
            nameof(Dashboard), "_donate", "Text", "Donate");
        translation.Received(1).AddTranslationItem(
            nameof(Dashboard), "_issues", "Text", "Issues");

        ITranslation repositoryTranslation = Substitute.For<ITranslation>();
        dashboard.GetTestAccessor().Repositories.AddTranslationItems(repositoryTranslation);
        repositoryTranslation.Received(1).AddTranslationItem(
            nameof(UserRepositoriesList),
            "mnuConfigure",
            "Text",
            "Recent repositories &settings");
        repositoryTranslation.Received(1).AddTranslationItem(
            nameof(UserRepositoriesList), "clmhdrPath", "Text", "Path");
        repositoryTranslation.Received(1).AddTranslationItem(
            nameof(UserRepositoriesList), "clmhdrBranch", "Text", "Branch");
        repositoryTranslation.Received(1).AddTranslationItem(
            nameof(UserRepositoriesList), "clmhdrCategory", "Text", "Category");
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Dashboard_repository_settings_should_remain_a_source_owned_menu_item()
    {
        Dashboard dashboard = new();
        RepositoryHistorySnapshot snapshot = new([], []);
        dashboard.Initialize(CreateController(snapshot), CreateHistory(snapshot));

        MenuItem configure = dashboard.GetTestAccessor().Repositories.GetTestAccessor().Configure;

        configure.Header.Should().Be("Recent repositories _settings");
        dashboard.GetVisualDescendants().OfType<Button>()
            .Should().NotContain(button => button.Name == "mnuConfigure");
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Repository_category_menu_should_assign_an_existing_category_through_the_original_controller()
    {
        Repository selected = new(@"C:\repos\selected");
        Repository categorized = new(@"C:\repos\categorized") { Category = "Team" };
        RepositoryHistorySnapshot snapshot = new(
            [new RepositoryHistoryEntry(selected, "selected", "main", IsFavourite: false, IsAnchored: false)],
            [new RepositoryHistoryEntry(categorized, "categorized", "feature", IsFavourite: true, IsAnchored: false)]);
        IUserRepositoriesListController controller = CreateController(snapshot);
        UserRepositoriesList list = new();
        list.Initialize(controller, CreateHistory(snapshot), () => Substitute.For<IGitUICommands>());
        list.ShowRecentRepositories(reloadData: false);
        UserRepositoriesList.TestAccessor accessor = list.GetTestAccessor();
        accessor.List.SelectedItem = accessor.List.Items.OfType<UserRepositoriesList.RepositoryListItem>().First();

        accessor.UpdateContextMenu().Should().BeTrue();
        accessor.OpenCategories();
        accessor.CategoryAdd.IsEnabled.Should().BeTrue();
        MenuItem team = accessor.Categories.Items.OfType<MenuItem>().Single(item => Equals(item.Tag, "Team"));
        team.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(MenuItem.ClickEvent));

        controller.Received(1).AssignCategoryAsync(selected, "Team");
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Category_title_should_disable_unchanged_names_and_accept_a_new_name()
    {
        FormDashboardCategoryTitle form = new(["Team", "Personal"], "Team");
        FormDashboardCategoryTitle.TestAccessor accessor = form.GetTestAccessor();

        accessor.Ok.IsEnabled.Should().BeFalse();
        accessor.CategoryName.Text = "Release";
        Dispatcher.UIThread.RunJobs();
        accessor.Ok.IsEnabled.Should().BeTrue();
        accessor.Ok.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));

        form.Category.Should().Be("Release");
        form.DialogResult.Should().Be(GitExtensions.Shims.WinForms.DialogResult.OK);
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Repository_drop_should_accept_exactly_one_existing_directory()
    {
        string directory = TestContext.CurrentContext.WorkDirectory;

        UserRepositoriesList.CanDropRepositoryDirectory([directory]).Should().BeTrue();
        UserRepositoriesList.CanDropRepositoryDirectory([]).Should().BeFalse();
        UserRepositoriesList.CanDropRepositoryDirectory([directory, directory]).Should().BeFalse();
        UserRepositoriesList.CanDropRepositoryDirectory([Path.Join(directory, "missing")]).Should().BeFalse();
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Dashboard_theme_should_preserve_the_original_light_palette()
    {
        DashboardTheme.Light.LogoBackColor.Should().Be(Avalonia.Media.Color.FromRgb(19, 122, 212));
        DashboardTheme.Light.StartBackColor.Should().Be(Avalonia.Media.Color.FromRgb(219, 235, 248));
        DashboardTheme.Light.ContributeBackColor.Should().Be(Avalonia.Media.Color.FromRgb(230, 241, 250));
        DashboardTheme.Light.SearchBackColor.Should().Be(Avalonia.Media.Color.FromRgb(248, 248, 255));
    }

    [AvaloniaTest]
    [Category("P8.6i.126")]
    [NonParallelizable]
    public void Dashboard_dark_palette_should_resolve_current_system_colors_before_attachment_and_after_theme_change()
    {
        ThemeSettings originalSettings = ThemeModule.Settings;
        ThemeId originalId = AppSettings.ThemeId;
        string[] originalVariations = AppSettings.ThemeVariations;
        bool originalVisualStyle = AppSettings.UseSystemVisualStyle;
        ThemeId themeId = new("dashboard-runtime");
        IThemeRepository repository = Substitute.For<IThemeRepository>();
        repository.GetInvariantTheme().Returns(Theme.CreateDefaultTheme());
        repository.GetTheme(themeId, Arg.Any<IReadOnlyList<string>>()).Returns(
            CreateTheme(System.Drawing.Color.FromArgb(12, 34, 56)),
            CreateTheme(System.Drawing.Color.FromArgb(65, 43, 21)));
        try
        {
            AppSettings.ThemeId = themeId;
            AppSettings.ThemeVariations = [];
            AppSettings.UseSystemVisualStyle = false;
            ThemeModule.TestAccessor.ReloadThemeSettings(repository);
            DashboardTheme dark = DashboardTheme.Dark;
            dark.StartBackColor.Should().Be(Avalonia.Media.Color.FromRgb(12, 34, 56));
            AssertSystemColors(dark);
            Dashboard dashboard = new();
            dashboard.RefreshContent();
            ((Avalonia.Media.SolidColorBrush)dashboard.FindControl<Border>("flpnlStart")!.Background!).Color
                .Should().Be(dark.StartBackColor);

            ThemeModule.TestAccessor.ReloadThemeSettings(repository);
            dark.StartBackColor.Should().Be(Avalonia.Media.Color.FromRgb(65, 43, 21));
            AssertSystemColors(dark);
            dashboard.RefreshContent();
            ((Avalonia.Media.SolidColorBrush)dashboard.FindControl<Border>("flpnlStart")!.Background!).Color
                .Should().Be(dark.StartBackColor);
        }
        finally
        {
            AppSettings.ThemeId = originalId;
            AppSettings.ThemeVariations = originalVariations;
            AppSettings.UseSystemVisualStyle = originalVisualStyle;
            IThemeRepository originalRepository = Substitute.For<IThemeRepository>();
            originalRepository.GetInvariantTheme().Returns(originalSettings.InvariantTheme);
            originalRepository.GetTheme(Arg.Any<ThemeId>(), Arg.Any<IReadOnlyList<string>>()).Returns(originalSettings.Theme);
            ThemeModule.TestAccessor.ReloadThemeSettings(originalRepository);
        }

        return;

        Theme CreateTheme(System.Drawing.Color control)
            => new(
                new Dictionary<AppColor, System.Drawing.Color> { [AppColor.PanelBackground] = System.Drawing.Color.Black },
                new[]
                {
                    System.Drawing.KnownColor.Control, System.Drawing.KnownColor.ControlLight,
                    System.Drawing.KnownColor.ControlDark, System.Drawing.KnownColor.ControlDarkDark,
                    System.Drawing.KnownColor.WindowText, System.Drawing.KnownColor.ControlText, System.Drawing.KnownColor.GrayText,
                }.Select((color, index) => (color, value: System.Drawing.Color.FromArgb(control.R + index, control.G, control.B)))
                    .ToDictionary(pair => pair.color, pair => pair.value),
                themeId);

        static void AssertSystemColors(DashboardTheme dark)
        {
            SearchControl<string> search = new(_ => [], _ => { })
            {
                SearchBoxBorderStyle = GitExtensions.Shims.WinForms.BorderStyle.FixedSingle,
                SearchBoxBorderDefaultColor = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Control)
            };
            TextBox input = search.FindControl<TextBox>("txtSearchBox")!;
            ((Avalonia.Media.SolidColorBrush)((Border)input.Parent!).BorderBrush!).Color
                .Should().Be(dark.StartBackColor);
            (System.Drawing.KnownColor Name, Avalonia.Media.Color Actual)[] colors =
            [
                (System.Drawing.KnownColor.Control, dark.SearchBackColor),
                (System.Drawing.KnownColor.Control, dark.StartBackColor),
                (System.Drawing.KnownColor.ControlLight, dark.ContributeBackColor),
                (System.Drawing.KnownColor.ControlDark, dark.HeaderBackColor),
                (System.Drawing.KnownColor.ControlDarkDark, dark.LogoBackColor),
                (System.Drawing.KnownColor.WindowText, dark.PrimaryText),
                (System.Drawing.KnownColor.ControlText, dark.PrimaryHeadingText),
                (System.Drawing.KnownColor.GrayText, dark.SecondaryHeadingText),
            ];
            foreach ((System.Drawing.KnownColor name, Avalonia.Media.Color actual) in colors)
            {
                actual.Should().Be(AvaloniaThemeResources.ToMediaColor(ThemeModule.Settings.Theme.GetColor(name)));
            }
        }
    }

    [AvaloniaTest]
    [Category("P4.5")]
    public void Dashboard_layout_should_use_designer_authored_96_dpi_metrics()
    {
        Dashboard dashboard = new();
        WinFormsControls.TableLayoutPanel layout = dashboard.FindControl<WinFormsControls.TableLayoutPanel>("tableLayoutPanel1")!;
        WinFormsControls.Panel logo = dashboard.FindControl<WinFormsControls.Panel>("pnlLogo")!;

        layout.ColumnDefinitions.Should().HaveCount(4);
        layout.ColumnDefinitions[0].Width.Value.Should().BeApproximately(7.142857, 0.000001);
        layout.ColumnDefinitions[1].Width.Should().Be(new GridLength(213));
        layout.ColumnDefinitions[2].Width.Value.Should().BeApproximately(85.71428, 0.00001);
        layout.ColumnDefinitions[3].Width.Value.Should().BeApproximately(7.142857, 0.000001);
        logo.Padding.Should().Be(new Avalonia.Thickness(20, 0, 20, 14));
        dashboard.GetTestAccessor().Repositories.HeaderHeight.Should().Be(68);
    }

    [Test]
    [Category("P4.5")]
    public async Task User_repositories_controller_should_assign_remove_and_clear()
    {
        Repository beta = new(@"C:\repos\beta") { Category = "Team" };
        ILocalRepositoryManager manager = Substitute.For<ILocalRepositoryManager>();
        manager.AssignCategoryAsync(beta, "Release").Returns(Task.FromResult<IList<Repository>>([beta]));
        IInvalidRepositoryRemover remover = Substitute.For<IInvalidRepositoryRemover>();
        remover.ShowDeleteInvalidRepositoryDialog(beta.Path).Returns(true);
        IRepositoryCurrentBranchNameCache branchCache = Substitute.For<IRepositoryCurrentBranchNameCache>();
        UserRepositoriesListController controller = new(manager, remover, branchCache);

        await controller.AssignCategoryAsync(beta, "Release");
        controller.RemoveInvalidRepository(beta.Path).Should().BeTrue();
        controller.ClearCache();

        await manager.Received(1).AssignCategoryAsync(beta, "Release");
        remover.Received(1).ShowDeleteInvalidRepositoryDialog(beta.Path);
        branchCache.Received(1).InvalidateAll();
    }

    private static IRepositoryHistoryUIService CreateHistory(RepositoryHistorySnapshot snapshot)
    {
        IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
        history.LoadSnapshot().Returns(snapshot);
        return history;
    }

    private static IUserRepositoriesListController CreateController(RepositoryHistorySnapshot snapshot)
    {
        IUserRepositoriesListController controller = Substitute.For<IUserRepositoriesListController>();
        ConfigureController(controller, snapshot);
        controller.IsValidGitWorkingDir(Arg.Any<string>()).Returns(true);
        return controller;
    }

    private static void ConfigureController(IUserRepositoriesListController controller, RepositoryHistorySnapshot snapshot)
    {
        controller.PreRenderRepositories(Arg.Any<string>()).Returns(call =>
        {
            string filter = call.ArgAt<string>(0);
            IReadOnlyList<RepositoryHistoryEntry> recent = Filter(snapshot.Recent, filter);
            IReadOnlyList<RepositoryHistoryEntry> favourites = Filter(snapshot.Favourites, filter);
            foreach (RepositoryHistoryEntry entry in recent.Concat(favourites))
            {
                controller.GetCurrentBranchName(entry.Repository.Path).Returns(entry.BranchName ?? string.Empty);
            }

            return (CreateRecent(recent), CreateRecent(favourites));
        });

        static IReadOnlyList<RepositoryHistoryEntry> Filter(IReadOnlyList<RepositoryHistoryEntry> entries, string filter)
            => [.. entries.Where(entry => string.IsNullOrWhiteSpace(filter)
                || entry.Caption.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
                || entry.Repository.Path.Contains(filter, StringComparison.CurrentCultureIgnoreCase)
                || (entry.BranchName?.Contains(filter, StringComparison.CurrentCultureIgnoreCase) ?? false))];

        static IReadOnlyList<RecentRepoInfo> CreateRecent(IReadOnlyList<RepositoryHistoryEntry> entries)
            => [.. entries.Select(entry => new RecentRepoInfo(entry.Repository, topRepo: false, entry.IsAnchored)
            {
                Caption = entry.Caption,
            })];
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !Directory.Exists(Path.Join(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("The test checkout root was not found.");
    }
}
