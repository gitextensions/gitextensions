using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.AboutBoxDialog;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.Menus;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class AboutDialogsTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        UserEnvironmentInformation.Initialise("9999999999999999999999999999999999abcdef", isDirty: true);
    }

    [Test]
    public void Resources_twin_should_expose_the_original_string_resources()
    {
        GitUI.Properties.Resources.Team.Should().Contain(",");
        GitUI.Properties.Resources.Coders.Should().NotBeNullOrWhiteSpace();
        GitUI.Properties.Resources.Translators.Should().NotBeNullOrWhiteSpace();
        GitUI.Properties.Resources.Designers.Should().NotBeNullOrWhiteSpace();
        GitUI.Properties.Resources.ChangeLog.Should().Contain("#");
    }

    [AvaloniaTest]
    public void FormChangeLog_should_construct_and_load_the_changelog()
    {
        FormChangeLog form = new();
        form.Title.Should().Be("Change log");

        ITranslation translation = Substitute.For<ITranslation>();
        form.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(nameof(FormChangeLog), "$this", "Text", "Change log");

        TextBox changeLog = form.FindControl<TextBox>("ChangeLog")!;
        changeLog.IsReadOnly.Should().BeTrue();
        changeLog.TextWrapping.Should().Be(Avalonia.Media.TextWrapping.NoWrap);
        changeLog.Text.Should().Be(GitUI.Properties.Resources.ChangeLog);
    }

    [AvaloniaTest]
    public void FormContributors_should_construct_with_three_tabs()
    {
        FormContributors form = new();

        TabControl tabs = (TabControl)form.Content!;
        tabs.ItemCount.Should().Be(3);

        TabItem[] pages = tabs.Items.Cast<TabItem>().ToArray();
        pages.Select(page => page.Header).Should().Equal("Developers", "Translators", "Designers");
        pages.Should().OnlyContain(page => page.Classes.Contains("gitextensions-dialog-tab"));
        TextBox[] textBoxes = pages.Select(page => (TextBox)page.Content!).ToArray();
        textBoxes.Should().OnlyContain(textBox => textBox.IsReadOnly && !textBox.IsTabStop);
        textBoxes.Should().OnlyContain(textBox => textBox.BorderThickness == new Thickness(0));
        textBoxes.Should().OnlyContain(textBox => textBox.VerticalContentAlignment == Avalonia.Layout.VerticalAlignment.Top);
        textBoxes[0].Text.Should().Contain("Team:").And.Contain("Contributors:");
    }

    [AvaloniaTest]
    public void FormContributors_should_match_native_96_dpi_client_geometry()
    {
        FormContributors form = new();

        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();

            form.ClientSize.Should().Be(new Size(624, 442));
            TabControl tabs = (TabControl)form.Content!;
            tabs.Bounds.Should().Be(new Rect(0, 0, 624, 442));
            tabs.Items.Cast<TabItem>().First().Bounds.Height.Should().Be(27);
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormAbout_should_construct_and_emit_its_translation_keys()
    {
        FormAbout form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormAbout), "label1", "Text", "This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY of FITNESS FOR A PARTICULAR PURPOSE.");
        translation.Received(1).AddTranslationItem(nameof(FormAbout), "label2", "Text", "Git Extensions is open source. Get involved!");
        translation.Received(1).AddTranslationItem(nameof(FormAbout), "linkLabelIcons", "Text", "Some icons by Yusuke Kamiyamane (CCA3)");
        translation.Received(1).AddTranslationItem(nameof(FormAbout), "_copyTooltip", "Text", "Copy environment info");

        Dispatcher.UIThread.RunJobs();
        form.Close();
    }

    [AvaloniaTest]
    public void FormAbout_should_match_native_96_dpi_client_geometry()
    {
        FormAbout form = new();

        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();

            form.ClientSize.Should().Be(new Size(601, 318));
            form.FindControl<StackPanel>("flowLayoutPanel1")!.Bounds.Should().Be(new Rect(0, 0, 157, 318));
            form.FindControl<Image>("logoPictureBox")!.Bounds.Should().Be(new Rect(12, 12, 128, 128));
            form.FindControl<TextBlock>("label2")!.Bounds.Should().Be(new Rect(12, 156, 133, 30));
            form.FindControl<Border>("pictureDonate")!.Bounds.Should().Be(new Rect(6, 202, 145, 32));
            form.FindControl<HyperlinkButton>("_NO_TRANSLATE_labelProductName")!.Bounds.Height.Should().Be(19);
            form.FindControl<TextBlock>("_NO_TRANSLATE_labelProductDescription")!.Bounds.Height.Should().Be(13);
            form.FindControl<HyperlinkButton>("_NO_TRANSLATE_ThanksTo")!.Bounds.Height.Should().Be(13);
            form.FindControl<HyperlinkButton>("linkLabelIcons")!.Bounds.Height.Should().Be(13);
            form.FindControl<EnvironmentInfo>("environmentInfo")!.Bounds.Should().Be(new Rect(0, 81, 420, 139));
            form.FindControl<TextBlock>("label1")!.Bounds.Should().Be(new Rect(0, 228, 420, 54));
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void Help_menu_should_expose_the_about_changelog_donate_items_under_the_FormBrowse_category()
    {
        HelpToolStripMenuItem menu = new();
        ITranslation translation = Substitute.For<ITranslation>();

        menu.AddControlTranslationItems(translation);

        translation.Received(1).AddTranslationItem("FormBrowse", "changelogToolStripMenuItem", "Text", "&Changelog");
        translation.Received(1).AddTranslationItem("FormBrowse", "donateToolStripMenuItem", "Text", "&Donate");
        translation.Received(1).AddTranslationItem("FormBrowse", "aboutToolStripMenuItem", "Text", "&About");
    }
}
