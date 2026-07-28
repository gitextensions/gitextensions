using System.ComponentModel.Design;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitCommands.Git.Extensions;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Plugins.ReleaseNotesGenerator;
using GitExtUtils;
using GitUI;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class ReleaseNotesGeneratorPluginTests
{
    private ServiceContainer _serviceContainer = null!;
    private string _workingDirectory = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _serviceContainer = new ServiceContainer();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        System.IO.Abstractions.FileSystem fileSystem = new();
        GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
        RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
        _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
        _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
        _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
        GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

        _workingDirectory = Path.Combine(
            Path.GetTempPath(),
            $"GitExtensions.Avalonia.ReleaseNotesGeneratorTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_workingDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceContainer.Dispose();
        TestDirectory.Delete(_workingDirectory);
    }

    [AvaloniaTest]
    public void Release_notes_generator_form_should_construct_with_original_layout_and_translation_keys()
    {
        using ReleaseNotesGeneratorForm form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        form.Width.Should().Be(614);
        form.Height.Should().Be(547);
        form.MinWidth.Should().Be(614);
        form.MinHeight.Should().Be(454);
        form.FindControl<TextBox>("textBoxResult")!.IsReadOnly.Should().BeTrue();
        form.FindControl<Control>("groupBoxCopy")!.IsEnabled.Should().BeFalse();
        form.FindControl<Button>("buttonGenerate")!.Width.Should().Be(105);
        form.FindControl<Button>("buttonCopyAsHtml")!.Width.Should().Be(125);

        translation.Received(1).AddTranslationItem(
            nameof(ReleaseNotesGeneratorForm), "$this", "Text", "Release Notes Generator");
        translation.Received(1).AddTranslationItem(
            nameof(ReleaseNotesGeneratorForm), "buttonGenerate", "Text", "Generate");
        translation.Received(1).AddTranslationItem(
            nameof(ReleaseNotesGeneratorForm), "groupBoxCopy", "Text", "Copy to clipboard");
        translation.Received(1).AddTranslationItem(
            nameof(ReleaseNotesGeneratorForm),
            "label11",
            "Text",
            "Clipboard will contain HTML code (plain text) and HTML format\r\nwhich can be pasted to programs like MS Word or LibreOffice Writer.");
        translation.DidNotReceive().AddTranslationItem(
            nameof(ReleaseNotesGeneratorForm),
            "_NO_TRANSLATE_textBoxGitLogArguments",
            Arg.Any<string>(),
            Arg.Any<string>());
        translation.DidNotReceive().AddTranslationItem(
            nameof(ReleaseNotesGeneratorForm),
            "_NO_TRANSLATE_textBoxRevTo",
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [AvaloniaTest]
    public void Release_notes_generator_plugin_should_expose_its_embedded_icon()
    {
        ReleaseNotesGeneratorPlugin plugin = new();

        plugin.Id.Should().Be(new Guid("49E7F2D6-AD79-489E-80A4-5CD212AE6DF3"));
        PluginIconProvider.GetIcon(plugin).Should().NotBeNull();
    }

    [AvaloniaTest]
    public void Release_notes_generator_form_should_generate_plain_text_and_encoded_html_from_git_log()
    {
        GitModule module = CreateRepository();
        string fromRevision = module.GitExecutable.GetOutput(new GitArgumentBuilder("rev-parse") { "HEAD" }).Trim();
        File.AppendAllText(Path.Combine(_workingDirectory, "readme.txt"), "second\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "readme.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "Fix <tag> & output".Quote() }).Should().BeTrue();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        GitUIEventArgs eventArgs = new(ownerForm: null, commands);

        using ReleaseNotesGeneratorForm form = new(eventArgs);
        ReleaseNotesGeneratorForm.TestAccessor accessor = form.GetTestAccessor();
        accessor.Generate(fromRevision, "HEAD");

        accessor.RevisionCount.Should().Be("1");
        accessor.Result.Should().Contain("Fix <tag> & output");
        accessor.CopyActionsEnabled.Should().BeTrue();
        accessor.CreateTextTable(new GitLogLineParser().Parse(accessor.Result.Split('\n')), tabs: true)
            .Should().Contain("\tFix <tag> & output");
        accessor.CreateHtmlTableForTesting(new GitLogLineParser().Parse(accessor.Result.Split('\n')))
            .Should().Contain("Fix &lt;tag&gt; &amp; output");
    }

    [Test]
    public void Html_fragment_should_preserve_plain_text_and_platform_rich_html_format()
    {
        const string fragment = "<p>Hallo</p>";
        string richFormat = OperatingSystem.IsWindows()
            ? "HTML Format"
            : OperatingSystem.IsMacOS()
                ? "public.html"
                : "text/html";

        using DataTransfer data = HtmlFragment.CreateClipboardData(fragment);

        data.Formats.Select(format => format.Identifier)
            .Should().Contain(DataFormat.Text.Identifier, richFormat);
        data.TryGetText().Should().Be(fragment);
        DataFormat richDataFormat = data.Formats.Single(format => format.Identifier == richFormat);
        string expectedRichText = OperatingSystem.IsWindows()
            ? HtmlFragment.CreateHtmlFormatClipboardText(fragment)
            : fragment;
        data.Items.Single().TryGetRaw(richDataFormat).Should().Be(expectedRichText);

        if (OperatingSystem.IsWindows())
        {
            HtmlFragment.CreateHtmlFormatClipboardText(fragment).Should().Be(
                "Version:0.9\r\n" +
                "StartHTML:00000097\r\n" +
                "EndHTML:00000177\r\n" +
                "StartFragment:00000131\r\n" +
                "EndFragment:00000143\r\n" +
                "<html><body>\r\n" +
                "<!--StartFragment--><p>Hallo</p><!--EndFragment-->\r\n" +
                "</body></html>");
        }
    }

    private GitModule CreateRepository()
    {
        GitModule module = new(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
        module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", "trunk" }).Should().BeTrue();
        module.SetSetting("user.name", "Avalonia Test");
        module.SetSetting("user.email", "avalonia@example.com");
        File.WriteAllText(Path.Combine(_workingDirectory, "readme.txt"), "initial\n");
        module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "readme.txt" }).Should().BeTrue();
        module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
        return module;
    }
}
