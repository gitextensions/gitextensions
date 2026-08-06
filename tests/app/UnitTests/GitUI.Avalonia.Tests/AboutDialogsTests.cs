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
    }

    [AvaloniaTest]
    public void FormContributors_should_construct_with_three_tabs()
    {
        FormContributors form = new();

        TabControl tabs = (TabControl)form.Content!;
        tabs.ItemCount.Should().Be(3);
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
