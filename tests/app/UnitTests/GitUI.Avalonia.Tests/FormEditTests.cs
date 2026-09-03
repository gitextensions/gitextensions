using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.HelperDialogs;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class FormEditTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormEdit_should_display_editable_text_and_preserve_the_read_only_boundary()
    {
        IGitModule module = Substitute.For<IGitModule>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);

        using FormEdit form = new(commands, "first line\nsecond line", "recovered.txt");

        form.GetTestAccessor().Viewer.GetText().Should().Be("first line\nsecond line");
        form.IsReadOnly.Should().BeFalse();

        form.IsReadOnly = true;

        form.IsReadOnly.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormEdit_should_use_the_existing_translation_key_once()
    {
        using FormEdit form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormEdit), "$this", "Text", "View");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Should().ContainSingle();
    }
}
