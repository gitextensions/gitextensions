using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using SkiaSharp;
using Point = Avalonia.Point;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class BrowseToolbarPopupLayoutTests
{
    private static IEnumerable<TestCaseData> LayoutCases
        => from name in new[] { "branchSelect", "toolStripButtonPull" }
           from rightToLeft in new[] { false, true }
           from points in new[] { 9, 11, 14 }
           select new TestCaseData(name, rightToLeft, points);

    [AvaloniaTest]
    [TestCaseSource(nameof(LayoutCases))]
    public void Browse_selectors_should_use_source_rows_and_padding_in_the_real_popup_before_and_after_reopening(
        string name, bool rightToLeft, int points)
    {
        using PopupFixture fixture = new(name, rightToLeft, points);
        object?[] retained = fixture.Menu.Items.ToArray();
        for (int cycle = 0; cycle < 2; cycle++)
        {
            fixture.Open();
            AssertSourceLayout(fixture);
            if (OperatingSystem.IsWindows())
            {
                Size expectedNativeSize = (name, points) switch
                {
                    ("branchSelect", 9) => new Size(361, 164),
                    ("branchSelect", 11) => new Size(439, 178),
                    ("branchSelect", 14) => new Size(543, 220),
                    ("toolStripButtonPull", 9) => new Size(237, 148),
                    ("toolStripButtonPull", 11) => new Size(281, 160),
                    ("toolStripButtonPull", 14) => new Size(342, 196),
                    _ => throw new InvalidOperationException("The native selector matrix must declare its observed source size."),
                };
                fixture.Presenter.Bounds.Size.Should().Be(expectedNativeSize,
                    "independent native96 Browse probes establish the same captions, explicit shortcuts and owner fonts in both directions");
            }

            fixture.Menu.Items.Should().Equal(retained, "the adapter must retain its supplied source command objects");
            fixture.Menu.Hide();
            fixture.Settle();
        }
    }

    [AvaloniaTest]
    [TestCase("branchSelect", false)]
    [TestCase("branchSelect", true)]
    [TestCase("toolStripButtonPull", false)]
    [TestCase("toolStripButtonPull", true)]
    public void Browse_popup_should_remeasure_actual_caption_shortcut_visibility_and_font_changes_without_losing_owned_rows(
        string name, bool rightToLeft)
    {
        using PopupFixture fixture = new(name, rightToLeft, 9);
        fixture.Open();
        AssertSourceLayout(fixture);
        MenuItem changed = fixture.Menu.Items.OfType<MenuItem>().First();
        string? originalHeader = changed.Header as string;
        changed.Header = "A deliberately wider source _caption with a literal __underscore";
        WinFormsToolStripMenuSizer.SetShortcutDisplayString(changed, "Ctrl+Shift+B, Alt+LButton");
        fixture.Settle();
        AssertSourceLayout(fixture);
        TextBlock shortcutText = changed.GetVisualDescendants().OfType<TextBlock>()
            .Single(part => part.Name == "PART_InputGestureText" && ReferenceEquals(part.TemplatedParent, changed));
        shortcutText.Text.Should().Be("Ctrl+Shift+B, Alt+LButton");
        double wider = fixture.Presenter.Bounds.Width;
        changed.Header = originalHeader;
        WinFormsToolStripMenuSizer.SetShortcutDisplayString(changed, string.Empty);
        fixture.Owner.FontSize = 14 * 96d / 72;
        fixture.Settle();
        AssertSourceLayout(fixture);
        shortcutText.Text.Should().NotBe("Ctrl+Shift+B, Alt+LButton",
            "removing the source override must restore the template's actual executable gesture binding");
        fixture.Owner.FontSize = 12;
        fixture.Settle();
        AssertSourceLayout(fixture);
        fixture.Presenter.Bounds.Width.Should().BeLessThan(wider);
        changed.IsVisible = false;
        fixture.Settle();
        AssertSourceLayout(fixture);
        changed.IsVisible = true;
        fixture.Settle();
        AssertSourceLayout(fixture);
    }

    [AvaloniaTest]
    [TestCase("branchSelect", false)]
    [TestCase("branchSelect", true)]
    [TestCase("toolStripButtonPull", false)]
    [TestCase("toolStripButtonPull", true)]
    public void Browse_popup_should_paint_its_border_over_the_native_canvas_without_reserving_an_extra_content_inset(
        string name, bool dark)
    {
        using PopupFixture fixture = new(name, rightToLeft: false, points: 9, dark: dark);
        fixture.Open();
        AssertSourceLayout(fixture);
        Border chrome = fixture.Presenter.GetVisualDescendants().OfType<Border>().Single(part => part.Name == "LayoutRoot");
        chrome.BorderThickness.Should().Be(new Thickness(1));
        chrome.Padding.Should().Be(default(Thickness));
        MenuItem first = fixture.Menu.Items.OfType<MenuItem>().First(item => item.IsVisible);
        Point firstOrigin = first.TranslatePoint(default, fixture.Presenter)
            ?? throw new InvalidOperationException("The real source row must have a popup-relative origin.");
        firstOrigin.X.Should().Be(0, "the source native border overlays its item canvas");
        firstOrigin.Y.Should().Be(2);
        TopLevel root = TopLevel.GetTopLevel(fixture.Presenter)
            ?? throw new InvalidOperationException("The retained popup must have a rendered root.");
        Point origin = fixture.Presenter.TranslatePoint(default, root)
            ?? throw new InvalidOperationException("The popup frame must have an actual rendered origin.");
        using WriteableBitmap frame = root.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("The actual opened popup must render a frame.");
        using MemoryStream stream = new();
        frame.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        using SKBitmap pixels = SKBitmap.Decode(stream)
            ?? throw new InvalidOperationException("The retained popup frame must decode.");
        Color border = ColorOf(chrome.BorderBrush);
        Color background = ColorOf(fixture.Presenter.Background);
        int x = (int)origin.X;
        int y = (int)origin.Y;
        pixels.GetPixel(x, y).Should().Be(new SKColor(border.R, border.G, border.B, border.A));
        pixels.GetPixel(x + 1, y + 1).Should().Be(new SKColor(background.R, background.G, background.B, background.A));
    }

    [AvaloniaTest]
    [TestCase("branchSelect")]
    [TestCase("toolStripButtonPull")]
    public void Source_shortcut_scope_should_release_only_its_owned_text_value_and_restore_the_live_template_binding(string name)
    {
        using PopupFixture fixture = new(name, rightToLeft: false, points: 9);
        fixture.Open();
        MenuItem command = fixture.Menu.Items.OfType<MenuItem>().First();
        TextBlock text = command.GetVisualDescendants().OfType<TextBlock>()
            .Single(part => part.Name == "PART_InputGestureText" && ReferenceEquals(part.TemplatedParent, command));
        string source = WinFormsToolStripMenuSizer.GetShortcutDisplayString(command)
            ?? throw new InvalidOperationException("The source probe must supply its explicit shortcut caption.");
        text.Text.Should().Be(source);
        text.Text = "Authored template override";
        WinFormsToolStripMenuSizer.ClearToolbarDropDown(fixture.Menu);
        text.Text.Should().Be("Authored template override");
        text.ClearValue(TextBlock.TextProperty);
        string? fallback = text.Text;
        WinFormsToolStripMenuSizer.ConfigureToolbarDropDown(fixture.Menu, fixture.Owner);
        fixture.Menu.Hide();
        fixture.Settle();
        fixture.Open();
        text.Text.Should().Be(source);
        WinFormsToolStripMenuSizer.ClearToolbarDropDown(fixture.Menu);
        text.Text.Should().Be(fallback);
        command.InputGesture = new KeyGesture(Key.F8);
        fixture.Settle();
        text.Text.Should().Contain("F8", "the original template binding must remain active after adapter retirement");
    }

    [AvaloniaTest]
    [TestCase("branchSelect")]
    [TestCase("toolStripButtonPull")]
    public void Source_shortcut_scope_should_follow_actual_template_replacement_and_release_the_detached_part(string name)
    {
        using PopupFixture fixture = new(name, rightToLeft: false, points: 9);
        fixture.Open();
        MenuItem command = fixture.Menu.Items.OfType<MenuItem>().First();
        TextBlock previous = ShortcutPart(command);
        string source = WinFormsToolStripMenuSizer.GetShortcutDisplayString(command)
            ?? throw new InvalidOperationException("The source probe must supply its explicit shortcut caption.");
        TextBlock replacement = new() { Name = "PART_InputGestureText" };
        replacement.SetValue(TextBlock.TextProperty, "Replacement fallback", BindingPriority.Template);
        command.Template = new FuncControlTemplate<MenuItem>((_, _) => replacement);
        command.ApplyTemplate();
        fixture.Settle();
        ShortcutPart(command).Should().BeSameAs(replacement);
        replacement.Text.Should().Be(source);
        previous.TemplatedParent.Should().BeNull();
        previous.Text.Should().NotBe(source);
        command.Template = null;
        command.ApplyTemplate();
        fixture.Settle();
        replacement.TemplatedParent.Should().BeNull();
        replacement.Text.Should().Be("Replacement fallback", "removing a template must release the detached source override even without a new TemplateApplied event");
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public void Source_shortcut_scope_should_not_retain_a_part_replaced_or_retired_synchronously_during_text_publication(bool retireScope)
    {
        using PopupFixture fixture = new("toolStripButtonPull", rightToLeft: false, points: 9);
        fixture.Open();
        MenuItem command = fixture.Menu.Items.OfType<MenuItem>().First();
        string source = WinFormsToolStripMenuSizer.GetShortcutDisplayString(command)
            ?? throw new InvalidOperationException("The source probe must supply its explicit shortcut caption.");
        TextBlock first = new() { Name = "PART_InputGestureText" };
        TextBlock second = new() { Name = "PART_InputGestureText" };
        first.SetValue(TextBlock.TextProperty, "First fallback", BindingPriority.Template);
        second.SetValue(TextBlock.TextProperty, "Second fallback", BindingPriority.Template);
        bool intercepted = false;
        first.PropertyChanged += (_, change) =>
        {
            if (!intercepted && change.Property == TextBlock.TextProperty && first.Text == source)
            {
                intercepted = true;
                if (retireScope)
                {
                    WinFormsToolStripMenuSizer.ClearToolbarDropDown(fixture.Menu);
                }
                else
                {
                    command.Template = new FuncControlTemplate<MenuItem>((_, _) => second);
                    command.ApplyTemplate();
                }
            }
        };
        command.Template = new FuncControlTemplate<MenuItem>((_, _) => first);
        command.ApplyTemplate();
        fixture.Settle();
        intercepted.Should().BeTrue("the actual source text publication must trigger the synchronous lifetime change");
        first.Text.Should().Be("First fallback");
        if (retireScope)
        {
            command.Classes.Should().NotContain("gitextensions-filter-menu");
        }
        else
        {
            ShortcutPart(command).Should().BeSameAs(second);
            first.TemplatedParent.Should().BeNull();
            second.Text.Should().Be(source);
            WinFormsToolStripMenuSizer.ClearToolbarDropDown(fixture.Menu);
            second.Text.Should().Be("Second fallback");
        }
    }

    private static Color ColorOf(IBrush? brush)
        => brush is ISolidColorBrush solid ? solid.Color : throw new InvalidOperationException("The source popup paint must resolve to a solid brush.");

    private static TextBlock ShortcutPart(MenuItem item)
        => item.GetVisualDescendants().OfType<TextBlock>()
            .Single(part => part.Name == "PART_InputGestureText" && ReferenceEquals(part.TemplatedParent, item));

    private static void AssertSourceLayout(PopupFixture fixture)
    {
        MenuFlyout menu = fixture.Menu;
        MenuItem[] commands = menu.Items.OfType<MenuItem>().ToArray();
        double tabWidth = Math.Ceiling(NativeToolStripDropDownLayout.MeasureSourceText(fixture.Owner, "\t").Width);
        int maximumTextWidth = 0;
        int maximumTextHeight = 0;
        foreach (MenuItem command in commands)
        {
            string caption = AvaloniaTranslationUtils.RemoveAvaloniaMnemonics(command.Header as string ?? string.Empty)
                .Replace("&", "&&", StringComparison.Ordinal);
            string shortcut = WinFormsToolStripMenuSizer.GetShortcutDisplayString(command) ?? command.InputGesture?.ToString() ?? string.Empty;
            Size text = NativeToolStripDropDownLayout.MeasureSourceText(command, caption);
            Size gesture = NativeToolStripDropDownLayout.MeasureSourceText(command, shortcut);
            maximumTextWidth = Math.Max(maximumTextWidth, (int)Math.Ceiling(text.Width + tabWidth + gesture.Width));
            maximumTextHeight = Math.Max(maximumTextHeight, (int)Math.Ceiling(Math.Max(text.Height, gesture.Height)));
        }

        // Native Browse probes establish image gutter25, text padding8/9 and arrow10+8;
        // native row height adds text padding2, rounds up to even and adds its final2.
        int expectedWidth = 25 + 8 + maximumTextWidth + 9 + 10 + 8;
        int rowHeight = Math.Max(maximumTextHeight + 2, 16 + 4);
        rowHeight += rowHeight % 2;
        rowHeight += 2;
        int popupWidth = expectedWidth + 1;
        fixture.Presenter.Classes.Should().Contain("gitextensions-filter-menu",
            "Browse selectors must opt into the already source-measured ToolStrip popup algorithm");
        double y = 2;
        foreach (Control control in menu.Items.OfType<Control>())
        {
            if (!control.IsVisible)
            {
                control.IsEffectivelyVisible.Should().BeFalse();
                continue;
            }

            double height = control is Separator ? 6 : rowHeight;
            Point origin = control.TranslatePoint(default, fixture.Presenter)
                ?? throw new InvalidOperationException("The actual popup row must have an arranged origin.");
            origin.Y.Should().Be(y);
            if (control is Separator)
            {
                origin.X.Should().Be(2);
                control.Bounds.Size.Should().Be(new Size(popupWidth - 4, height));
            }
            else
            {
                origin.X.Should().Be(0);
                control.Bounds.Size.Should().Be(new Size(expectedWidth, height));
            }

            y += height;
        }

        fixture.Presenter.Bounds.Size.Should().Be(new Size(popupWidth, y + 2));
        fixture.Menu.Items.OfType<MenuItem>().Should().OnlyContain(item => item.FontSize == fixture.Owner.FontSize);
        foreach (MenuItem command in commands)
        {
            if (WinFormsToolStripMenuSizer.GetShortcutDisplayString(command) is string sourceShortcut)
            {
                TextBlock text = command.GetVisualDescendants().OfType<TextBlock>()
                    .Single(part => part.Name == "PART_InputGestureText" && ReferenceEquals(part.TemplatedParent, command));
                text.Text.Should().Be(sourceShortcut, "the source hotkey caption must be painted, not merely measured");
            }
        }
    }

    private sealed class PopupFixture : IDisposable
    {
        private readonly JoinableTaskContext? _previousContext;
        private readonly JoinableTaskContext _context;

        public PopupFixture(string name, bool rightToLeft, int points, bool dark = false)
        {
            AvaloniaSynchronizationContext.InstallIfNeeded();
            _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
            _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
            ThreadHelper.JoinableTaskContext = _context;
            Form = new FormBrowse();
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(Substitute.For<IGitModule>());
            (typeof(GitModuleForm).GetProperty(nameof(GitModuleForm.UICommands), BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidOperationException("The original-shaped UICommands boundary must remain available."))
                .SetValue(Form, commands);
            Owner = Form.FindControl<IconSplitButton>(name)
                ?? throw new InvalidOperationException("The actual Browse must retain its source-named selector.");
            Menu = Owner.Flyout as MenuFlyout
                ?? throw new InvalidOperationException("The actual source selector must retain its menu.");
            Owner.IsEnabled = true;
            Owner.FontSize = points * 96d / 72;
            if (name == "branchSelect")
            {
                MenuItem checkout = new()
                {
                    Header = "Chec_kout branch...",
                    InputGesture = new KeyGesture(Key.OemPeriod, KeyModifiers.Control),
                };
                WinFormsToolStripMenuSizer.SetShortcutDisplayString(checkout, "Ctrl+OemPeriod");
                Menu.Items.Add(checkout);
                Menu.Items.Add(new Separator());
                foreach (string branch in new[]
                {
                    "Avalonia_Port", "backup/Avalonia_Port-pre-upstream-rebase-20260816",
                    "backup/Avalonia_Port-pre-upstream-rebase-20260830", "backup/Avalonia_Port-pre-upstream-rebase-20261004",
                    "backup/master-pre-Avalonia-merge-20261004", "master",
                })
                {
                    MenuItem item = new() { Header = branch.Replace("_", "__", StringComparison.Ordinal) };
                    item.Classes.Add("gitextensions-menu-no-gesture");
                    Menu.Items.Add(item);
                }
            }
            else
            {
                Form.pullToolStripMenuItem1.InputGesture = new KeyGesture(Key.Down, KeyModifiers.Control);
                WinFormsToolStripMenuSizer.SetShortcutDisplayString(Form.pullToolStripMenuItem1, "Ctrl+Down");
            }

            // Keep the actual source-named button, retained menu and opening handlers, but
            // do not start Browse repository/plugin lifecycles for a layout contract.
            Form.ToolStripMain.Items.Remove(Owner);
            Window = new Window
            {
                Width = 1800,
                Height = 650,
                FlowDirection = rightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
                RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light,
                Content = new Border { Child = Owner, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top },
            };
            Window.Show();
            Settle();
        }

        public FormBrowse Form { get; }

        public Window Window { get; }

        public IconSplitButton Owner { get; }

        public MenuFlyout Menu { get; }

        public MenuFlyoutPresenter Presenter => Menu.Popup.Child as MenuFlyoutPresenter
            ?? throw new InvalidOperationException("The real source popup presenter must be available after opening.");

        public void Open()
        {
            Owner.ShowDropDown();
            Settle();
            Menu.IsOpen.Should().BeTrue();
        }

        public void Settle()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            Menu.Hide();
            Window.Close();
            try
            {
                ((IDisposable)Form).Dispose();
            }
            finally
            {
                ThreadHelper.JoinableTaskContext = _previousContext!;
                _context.Dispose();
            }
        }
    }
}
