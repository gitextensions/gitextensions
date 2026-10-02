using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Threading;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using SourceControls = GitUI.Compat.WinFormsControls;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[Category("P8.6i.126")]
public sealed class MenuStripSizingTests
{
    [AvaloniaTest]
    [TestCase(9, "_Commands", "Commands", 81)]
    [TestCase(9, "_Plugins", "Plugins", 58)]
    [TestCase(11, "_Commands", "Commands", 0)]
    [TestCase(11, "_Plugins", "Plugins", 0)]
    public void AutoSize_should_use_native_preferred_text_metrics_not_renderer_padding(int points, string header, string display, int nativeDefaultWidth)
    {
        SourceControls.ToolStripMenuItem item = new() { Header = header, Padding = new Thickness(8, 0) };
        SourceControls.MenuStripEx menu = new() { FontFamily = new FontFamily("Segoe UI"), FontSize = points * 96d / 72, Items = { item } };
        Window window = new() { Width = 640, Height = 120, Content = menu };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            item.FontFamily.Should().Be(menu.FontFamily);
            item.FontSize.Should().Be(menu.FontSize);
            item.Width.Should().Be(GetExpectedWidth(item, display));
            if (OperatingSystem.IsWindows() && nativeDefaultWidth > 0)
            {
                item.Width.Should().Be(nativeDefaultWidth);
            }

            item.Padding.Should().Be(new Thickness(8, 0), "source preferred padding must not overwrite renderer insets");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    [TestCase("_A&B__C", "A&B_C")]
    [TestCase("Ausgewählte _Änderungen übernehmen", "Ausgewählte Änderungen übernehmen")]
    [TestCase("_First\nSecond", "First\nSecond")]
    public void AutoSize_should_measure_translated_mnemonics_literal_ampersands_and_explicit_lines(string header, string display)
    {
        SourceControls.ToolStripMenuItem item = new() { Header = header };
        SourceControls.MenuStripEx menu = new() { Items = { item } };
        Window window = new() { Width = 160, Height = 120, Content = menu };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            item.Width.Should().Be(GetExpectedWidth(item, display), "preferred text size is not wrapped to the host width");
            item.Header.Should().Be(header);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void AutoSize_should_remeasure_inherited_fonts_and_runtime_source_padding()
    {
        SourceControls.ToolStripMenuItem item = new() { Header = "_Commands" };
        SourceControls.MenuStripEx menu = new() { Items = { item } };
        Window window = new() { Width = 640, Height = 120, Content = menu };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            double initialWidth = item.Width;
            menu.FontSize = 11 * 96d / 72;
            menu.FontWeight = FontWeight.Bold;
            menu.FontStyle = FontStyle.Italic;
            Dispatcher.UIThread.RunJobs();
            item.FontSize.Should().Be(menu.FontSize);
            item.FontWeight.Should().Be(menu.FontWeight);
            item.FontStyle.Should().Be(menu.FontStyle);
            item.Width.Should().Be(GetExpectedWidth(item, "Commands"));
            item.Width.Should().BeGreaterThan(initialWidth);

            SourceControls.MenuStripEx.SetNativeItemPadding(item, new Thickness(7, 1, 11, 2));
            Dispatcher.UIThread.RunJobs();
            item.Width.Should().Be(GetExpectedWidth(item, "Commands"));
            SourceControls.MenuStripEx.GetNativeItemPadding(item).Should().Be(new Thickness(7, 1, 11, 2));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void AutoSize_should_preserve_explicit_item_fonts_and_resume_owner_fonts_when_cleared()
    {
        FontFamily authoredFamily = new("monospace");
        SourceControls.ToolStripMenuItem item = new()
        {
            Header = "_Commands",
            FontFamily = authoredFamily,
            FontSize = 20,
            FontStyle = FontStyle.Italic,
            FontWeight = FontWeight.Bold,
        };
        SourceControls.MenuStripEx menu = new() { Items = { item } };
        Window window = new() { Width = 640, Height = 120, Content = menu };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            double authoredWidth = item.Width;
            menu.FontFamily = new FontFamily("Segoe UI");
            menu.FontSize = 11 * 96d / 72;
            menu.FontStyle = FontStyle.Normal;
            menu.FontWeight = FontWeight.Normal;
            Dispatcher.UIThread.RunJobs();
            item.FontFamily.Should().Be(authoredFamily);
            item.FontSize.Should().Be(20);
            item.FontStyle.Should().Be(FontStyle.Italic);
            item.FontWeight.Should().Be(FontWeight.Bold);
            item.Width.Should().Be(authoredWidth);

            item.ClearValue(MenuItem.FontFamilyProperty);
            item.ClearValue(MenuItem.FontSizeProperty);
            item.ClearValue(MenuItem.FontStyleProperty);
            item.ClearValue(MenuItem.FontWeightProperty);
            Dispatcher.UIThread.RunJobs();
            item.FontFamily.Should().Be(menu.FontFamily);
            item.FontSize.Should().Be(menu.FontSize);
            item.FontStyle.Should().Be(menu.FontStyle);
            item.FontWeight.Should().Be(menu.FontWeight);
            item.Width.Should().Be(GetExpectedWidth(item, "Commands"));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void AutoSize_should_remeasure_header_and_image_changes_and_preserve_minimum_width()
    {
        SourceControls.ToolStripMenuItem item = new() { Header = "_Tools", MinWidth = 75 };
        SourceControls.MenuStripEx menu = new() { Items = { item } };
        Window window = new() { Width = 640, Height = 120, Content = menu };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            item.Width.Should().Be(75);
            item.Header = "_A substantially longer translated command";
            Dispatcher.UIThread.RunJobs();
            item.Width.Should().Be(GetExpectedWidth(item, "A substantially longer translated command"));
            double textWidth = item.Width;

            item.Icon = new Image { Width = 16, Height = 16 };
            Dispatcher.UIThread.RunJobs();
            item.Width.Should().Be(textWidth + 16);
            item.Icon = new Image { Width = 24, Height = 16 };
            Dispatcher.UIThread.RunJobs();
            item.Width.Should().Be(textWidth + 16, "ToolStrip's default image scaling keeps its sixteen-pixel image slot");
            item.Icon = null;
            Dispatcher.UIThread.RunJobs();
            item.Width.Should().Be(textWidth);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Explicit_width_should_remain_fixed_before_or_after_an_automatic_measurement()
    {
        SourceControls.ToolStripMenuItem authored = new() { Header = "_Tools", Width = 47 };
        SourceControls.ToolStripMenuItem automatic = new() { Header = "_Commands" };
        SourceControls.MenuStripEx menu = new() { Items = { authored, automatic } };
        Window window = new() { Width = 640, Height = 120, Content = menu };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            automatic.Width = 53;
            authored.Header = "_A much longer translated caption";
            automatic.Header = authored.Header;
            authored.Icon = new Image { Width = 16, Height = 16 };
            automatic.Icon = new Image { Width = 24, Height = 16 };
            menu.FontSize += 4;
            Dispatcher.UIThread.RunJobs();
            authored.Width.Should().Be(47);
            automatic.Width.Should().Be(53);

            automatic.Width = double.NaN;
            Dispatcher.UIThread.RunJobs();
            automatic.Width.Should().Be(GetExpectedWidth(automatic, "A much longer translated caption", imageWidth: 16));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Detached_items_should_release_owned_widths_and_fonts_before_reparenting()
    {
        SourceControls.ToolStripMenuItem automatic = new() { Header = "_Commands" };
        SourceControls.ToolStripMenuItem authored = new() { Header = "_Tools", Width = 47, FontSize = 20 };
        SourceControls.MenuStripEx previous = new() { FontSize = 16, Items = { automatic, authored } };
        SourceControls.MenuStripEx current = new() { FontSize = 18 };
        Window window = new() { Width = 640, Height = 120, Content = previous };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            automatic.Width.Should().NotBe(double.NaN);
            window.Content = null;
            previous.Items.Clear();
            automatic.Width.Should().Be(double.NaN, "a detached strip must release its cached width without another layout");
            authored.Width.Should().Be(47);
            authored.FontSize.Should().Be(20);

            current.Items.Add(automatic);
            current.Items.Add(authored);
            automatic.FontSize.Should().Be(current.FontSize);
            authored.FontSize.Should().Be(20);
            previous.FontSize = 28;
            automatic.FontSize.Should().Be(current.FontSize, "the former owner no longer owns this item");
            window.Content = current;
            Dispatcher.UIThread.RunJobs();
            automatic.Width.Should().Be(GetExpectedWidth(automatic, "Commands"));
            authored.Width.Should().Be(47);

            window.Content = null;
            current.Items.Remove(automatic);
            automatic.Width = 62;
            previous.Items.Add(automatic);
            current.FontSize = 30;
            automatic.FontSize.Should().Be(previous.FontSize);
            automatic.Header = "_A longer translated caption";
            window.Content = previous;
            Dispatcher.UIThread.RunJobs();
            automatic.Width.Should().Be(62, "reparenting must preserve a subsequently authored width");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Top_level_items_moved_into_a_popup_should_release_only_owned_widths_immediately()
    {
        SourceControls.ToolStripMenuItem automatic = new() { Header = "_Commands" };
        SourceControls.ToolStripMenuItem authored = new() { Header = "_Tools", Width = 47 };
        SourceControls.ToolStripMenuItem parent = new() { Header = "_Parent", FontSize = 18 };
        SourceControls.MenuStripEx menu = new() { Items = { automatic, authored, parent } };
        Window window = new() { Width = 640, Height = 120, Content = menu };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            automatic.Width.Should().NotBe(double.NaN);
            menu.Items.Remove(automatic);
            parent.Items.Add(automatic);
            menu.Items.Remove(authored);
            parent.Items.Add(authored);
            automatic.Width.Should().Be(double.NaN, "the top-level cache is not a popup item's authored width");
            automatic.FontSize.Should().Be(parent.FontSize);
            authored.Width.Should().Be(47);
            authored.FontSize.Should().Be(parent.FontSize);
            menu.FontSize = 26;
            automatic.Width.Should().Be(double.NaN);
            authored.Width.Should().Be(47);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Explicit_width_equal_to_the_cached_width_should_stop_automatic_sizing()
    {
        SourceControls.ToolStripMenuItem item = new() { Header = "_Commands" };
        SourceControls.MenuStripEx menu = new() { Items = { item } };
        Window window = new() { Width = 640, Height = 120, Content = menu };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            double fixedWidth = item.Width;
            item.Width = fixedWidth;
            menu.FontSize += 4;
            item.Header = "_A substantially longer translated caption";
            Dispatcher.UIThread.RunJobs();
            item.Width.Should().Be(fixedWidth);

            item.Width = double.NaN;
            Dispatcher.UIThread.RunJobs();
            item.Width.Should().Be(GetExpectedWidth(item, "A substantially longer translated caption"));
            item.Width.Should().BeGreaterThan(fixedWidth);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Popup_items_should_inherit_immediate_owner_fonts_including_dynamic_children()
    {
        SourceControls.ToolStripMenuItem inherited = new() { Header = "_Inherited" };
        SourceControls.ToolStripMenuItem authored = new() { Header = "_Authored", FontSize = 18, Width = 65 };
        SourceControls.ToolStripMenuItem parent = new()
        {
            Header = "_Commands",
            FontSize = 20,
            FontStyle = FontStyle.Italic,
            FontWeight = FontWeight.Bold,
            Items = { inherited, authored },
        };
        SourceControls.MenuStripEx menu = new() { FontSize = 14, Items = { parent } };
        Window window = new() { Width = 640, Height = 120, Content = menu };
        window.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            inherited.FontSize.Should().Be(20);
            inherited.FontStyle.Should().Be(FontStyle.Italic);
            inherited.FontWeight.Should().Be(FontWeight.Bold);
            authored.FontSize.Should().Be(18);
            menu.FontSize = 26;
            Dispatcher.UIThread.RunJobs();
            inherited.FontSize.Should().Be(20, "an authored parent font is the popup's ambient font");

            parent.ClearValue(MenuItem.FontSizeProperty);
            Dispatcher.UIThread.RunJobs();
            inherited.FontSize.Should().Be(26);
            parent.FontFamily = new FontFamily("monospace");
            Dispatcher.UIThread.RunJobs();
            inherited.FontFamily.Should().Be(parent.FontFamily);
            SourceControls.ToolStripMenuItem added = new() { Header = "_Added" };
            parent.Items.Add(added);
            added.FontFamily.Should().Be(parent.FontFamily);
            added.FontSize.Should().Be(parent.FontSize);
            added.FontStyle.Should().Be(parent.FontStyle);
            added.FontWeight.Should().Be(parent.FontWeight);

            SourceControls.ToolStripMenuItem grandchild = new() { Header = "_Grandchild" };
            authored.Items.Add(grandchild);
            grandchild.FontSize.Should().Be(18, "each autogenerated dropdown inherits its immediate owner item");
            authored.FontSize = 22;
            Dispatcher.UIThread.RunJobs();
            grandchild.FontSize.Should().Be(22);
            authored.Width.Should().Be(65);
            Dispatcher.UIThread.RunJobs();
            parent.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            inherited.FontSize.Should().Be(parent.FontSize);
            added.FontSize.Should().Be(parent.FontSize);
            authored.FontSize.Should().Be(22);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public void Browse_menu_items_should_survive_styled_removal_and_reinsertion_after_font_changes()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using FormBrowse form = new();
        SourceControls.MenuStripEx menu = form.mainMenuStrip;
        form.Show();
        try
        {
            Dispatcher.UIThread.RunJobs();
            menu.FontSize = 11 * 96d / 72;
            Dispatcher.UIThread.RunJobs();
            MenuItem item = menu.Items.OfType<MenuItem>().Single(item => item.Name == "commandsToolStripMenuItem");
            item.IsVisible = true;
            Dispatcher.UIThread.RunJobs();
            int index = menu.Items.IndexOf(item);
            for (int iteration = 0; iteration < 2; iteration++)
            {
                menu.Items.Remove(item);
                item.Width.Should().Be(double.NaN);
                menu.Items.Insert(index, item);
                Dispatcher.UIThread.RunJobs();
                item.FontSize.Should().Be(menu.FontSize);
                item.Width.Should().Be(GetExpectedWidth(item, "Commands"));
                item.IsSubMenuOpen = true;
                Dispatcher.UIThread.RunJobs();
                item.Items.OfType<MenuItem>().Should().OnlyContain(child => child.FontSize == item.FontSize);
                item.IsSubMenuOpen = false;
            }
        }
        finally
        {
            form.Close();
        }
    }

    private static double GetExpectedWidth(MenuItem item, string display, double imageWidth = 0)
    {
        string measured = OperatingSystem.IsWindows() ? display.Replace("&", "&&", StringComparison.Ordinal) : display;
        Thickness padding = SourceControls.MenuStripEx.GetNativeItemPadding(item);
        return Math.Ceiling(Math.Max(item.MinWidth, WinFormsTextMeasurer.MeasureTextRenderer(item, measured).Width + imageWidth + 4 + padding.Left + padding.Right));
    }
}
