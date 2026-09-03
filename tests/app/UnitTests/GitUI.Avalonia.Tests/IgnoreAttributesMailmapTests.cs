using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.GitIgnoreDialog;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class IgnoreAttributesMailmapTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormAddToGitIgnore_should_construct_and_preview_matches()
    {
        FormAddToGitIgnore form = new();
        FormAddToGitIgnore.TestAccessor accessor = form.GetTestAccessor();

        accessor.FilePattern.Should().NotBeNull();
        accessor.Preview.Should().NotBeNull();

        accessor.UpdatePreview(["a.txt", "b.txt"]);
        accessor.Preview.ItemCount.Should().Be(2);
        accessor.NoMatchPanel.IsVisible.Should().BeFalse();

        accessor.UpdatePreview([]);
        accessor.NoMatchPanel.IsVisible.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormAddToGitIgnore_should_emit_its_translation_keys()
    {
        FormAddToGitIgnore form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "$this", "Text", "Add file(s) to .gitignore");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "AddToIgnore", "Text", "Ignore");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "btnCancel", "Text", "Cancel");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "groupBox1", "Text", "Preview");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "groupFilePattern", "Text", "Enter a file pattern to ignore:");
        translation.Received(1).AddTranslationItem(nameof(FormAddToGitIgnore), "label2", "Text", "No existing files match that pattern.");
    }

    [AvaloniaTest]
    public void FormGitIgnore_should_construct_with_the_original_controls()
    {
        FormGitIgnore form = new();
        FormGitIgnore.TestAccessor accessor = form.GetTestAccessor();

        accessor.Editor.Should().NotBeNull();
        accessor.Save.Should().NotBeNull();
        accessor.AddDefault.Should().NotBeNull();
        accessor.AddPattern.Should().NotBeNull();
        accessor.Cancel.Should().NotBeNull();
    }

    [AvaloniaTest]
    public void FormGitIgnore_should_emit_its_translation_keys()
    {
        FormGitIgnore form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "$this", "Text", "Edit .gitignore");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "Save", "Text", "Save");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "AddDefault", "Text", "Add default ignores");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "AddPattern", "Text", "Add pattern");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "btnCancel", "Text", "Cancel");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "lnkGitIgnorePatterns", "Text", "Example ignore patterns");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "lnkGitIgnoreGenerate", "Text", "Generate a custom ignore file for git");
        translation.Received(1).AddTranslationItem(nameof(FormGitIgnore), "label1", "Text", Arg.Is<string>(text => text.StartsWith("Specify filepatterns")));
    }

    [AvaloniaTest]
    public void FormGitAttributes_should_construct_and_emit_keys()
    {
        FormGitAttributes form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.GetTestAccessor().Editor.Should().NotBeNull();
        form.GetTestAccessor().Save.Should().NotBeNull();

        form.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(nameof(FormGitAttributes), "$this", "Text", "Edit .gitattributes");
        translation.Received(1).AddTranslationItem(nameof(FormGitAttributes), "Save", "Text", "Save");
        translation.Received(1).AddTranslationItem(nameof(FormGitAttributes), "label1", "Text", Arg.Is<string>(text => text.StartsWith("Edit the git attributes")));
    }

    [AvaloniaTest]
    public void FormMailMap_should_construct_and_emit_keys()
    {
        FormMailMap form = new();
        ITranslation translation = Substitute.For<ITranslation>();
        Label help = form.GetTestAccessor().Help;
        translation.TranslateItem(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Func<string?>>())
            .Returns(call => call.ArgAt<Func<string?>>(3)());

        form.GetTestAccessor().Editor.Should().NotBeNull();
        form.GetTestAccessor().Save.Should().NotBeNull();

        form.AddTranslationItems(translation);
        translation.Received(1).AddTranslationItem(nameof(FormMailMap), "$this", "Text", "Edit .mailmap");
        translation.Received(1).AddTranslationItem(nameof(FormMailMap), "Save", "Text", "Save");
        translation.Received(1).AddTranslationItem(
            nameof(FormMailMap),
            "label1",
            "Text",
            Arg.Is<string>(text => text.Contains("henk_westhuis@hotmail.com", StringComparison.Ordinal)));
        help.Content.Should().BeOfType<TextBlock>().Which.Text.Should().Contain("henk_westhuis@hotmail.com");
    }

    [AvaloniaTest]
    public void Ignore_and_editor_dialogs_should_preserve_their_96_dpi_designer_layout_and_tab_order()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.Combine(Path.GetTempPath(), "ge-ignore-layout"));
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        FormAddToGitIgnore add = new() { Width = 599, Height = 341 };
        FormGitIgnore ignore = new(commands, localExclude: false) { Width = 634, Height = 623 };
        FormGitAttributes attributes = new(commands) { Width = 634, Height = 474 };
        FormMailMap mailMap = new(commands) { Width = 634, Height = 474 };

        try
        {
            add.Show();
            ignore.Show();
            attributes.Show();
            mailMap.Show();
            Dispatcher.UIThread.RunJobs();

            Grid addRoot = (Grid)add.Content!;
            addRoot.RowDefinitions.Select(row => row.Height).Should().Equal(
                new GridLength(93),
                GridLength.Star,
                new GridLength(44));
            add.GetTestAccessor().FilePattern.Height.Should().Be(71);
            add.GetTestAccessor().FilePattern.TabIndex.Should().Be(6);
            add.GetTestAccessor().Preview.TabIndex.Should().Be(2);
            add.GetTestAccessor().AddToIgnore.Bounds.Size.Should().Be(new Size(110, 25));
            add.GetTestAccessor().AddToIgnore.TabIndex.Should().Be(7);
            add.GetTestAccessor().Cancel.Bounds.Size.Should().Be(new Size(110, 25));
            add.GetTestAccessor().Cancel.TabIndex.Should().Be(8);
            add.GetTestAccessor().NoMatchPanel.Width.Should().Be(233);
            add.GetTestAccessor().NoMatchPanel.Height.Should().Be(26);

            Grid ignoreSplit = ignore.FindControl<Grid>("splitContainer1")!;
            ignoreSplit.ColumnDefinitions.Select(column => column.Width).Should().Equal(
                GridLength.Star,
                new GridLength(4),
                new GridLength(270));
            ignore.MinWidth.Should().Be(634);
            ignore.MinHeight.Should().Be(459);
            ignore.GetTestAccessor().Editor.TabIndex.Should().Be(0);
            ignore.GetTestAccessor().AddDefault.Bounds.Size.Should().Be(new Size(160, 27));
            ignore.GetTestAccessor().AddDefault.TabIndex.Should().Be(2);
            ignore.GetTestAccessor().AddPattern.Bounds.Size.Should().Be(new Size(160, 27));
            ignore.GetTestAccessor().AddPattern.TabIndex.Should().Be(3);
            ignore.GetTestAccessor().Cancel.Bounds.Size.Should().Be(new Size(75, 27));
            ignore.GetTestAccessor().Cancel.TabIndex.Should().Be(2);
            ignore.GetTestAccessor().Save.Bounds.Size.Should().Be(new Size(160, 27));
            ignore.GetTestAccessor().Save.TabIndex.Should().Be(1);

            AssertEditorDialogLayout(attributes, attributes.GetTestAccessor().Editor, attributes.GetTestAccessor().Save, attributes.GetTestAccessor().Help, 209, 285);
            AssertEditorDialogLayout(mailMap, mailMap.GetTestAccessor().Editor, mailMap.GetTestAccessor().Save, mailMap.GetTestAccessor().Help, 302, 171);
        }
        finally
        {
            add.Close();
            ignore.Close();
            attributes.Close();
            mailMap.Close();
        }
    }

    [TestCase(typeof(FormGitIgnore), "FormGitIgnoreLoad", "FormGitIgnoreFormClosing")]
    [TestCase(typeof(FormGitAttributes), "FormGitAttributesLoad", "FormGitAttributesClosing")]
    [TestCase(typeof(FormMailMap), "FormMailMapLoad", "FormMailMapFormClosing")]
    public void Editor_dialogs_should_retain_the_original_load_and_closing_handler_identities(
        Type type,
        string loadHandler,
        string closingHandler)
    {
        const System.Reflection.BindingFlags Flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;

        type.GetMethod(loadHandler, Flags).Should().NotBeNull();
        type.GetMethod(closingHandler, Flags).Should().NotBeNull();
    }

    [AvaloniaTest]
    public void GitIgnoreModel_should_describe_the_gitignore_file()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.Combine(Path.GetTempPath(), "ge-ignore-model"));

        GitIgnoreModel model = new(module);

        model.FormCaption.Should().Be("Edit .gitignore");
        model.ExcludeFile.Should().EndWith(".gitignore");
        model.SaveFileQuestion.Should().Be("Save changes to .gitignore?");
    }

    [AvaloniaTest]
    public void GitLocalExcludeModel_should_describe_the_local_exclude_file()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.ResolveGitInternalPath("info").Returns(Path.Combine(Path.GetTempPath(), "ge-exclude", ".git", "info"));

        GitLocalExcludeModel model = new(module);

        model.FormCaption.Should().Be("Edit .git/info/exclude");
        model.ExcludeFile.Should().EndWith("exclude");
        model.SaveFileQuestion.Should().Be("Save changes to .git/info/exclude?");
    }

    [AvaloniaTest]
    public void FormMailMap_should_construct_with_ui_commands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.Combine(Path.GetTempPath(), "ge-mailmap"));
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        FormMailMap form = new(commands);

        form.GetTestAccessor().Editor.Should().NotBeNull();

        Dispatcher.UIThread.RunJobs();
    }

    private static void AssertEditorDialogLayout(
        Window form,
        GitUI.Editor.FileViewer editor,
        Button save,
        Label help,
        int helpWidth,
        int helpHeight)
    {
        Grid split = form.FindControl<Grid>("splitContainer1")!;
        split.ColumnDefinitions.Select(column => column.Width).Should().Equal(
            new GridLength(381),
            new GridLength(4),
            GridLength.Star);
        editor.TabIndex.Should().Be(0);
        save.Bounds.Size.Should().Be(new Size(75, 25));
        save.TabIndex.Should().Be(0);
        help.Bounds.Size.Should().Be(new Size(helpWidth, helpHeight));
        help.Bounds.Position.Should().Be(new Point(3, 9));
        help.TabIndex.Should().Be(1);
    }
}
