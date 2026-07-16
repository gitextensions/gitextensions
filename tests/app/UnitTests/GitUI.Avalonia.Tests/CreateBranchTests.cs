using System.ComponentModel.Design;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CreateBranchTests
{
    private static readonly ObjectId RevisionId = ObjectId.Parse("0123456789012345678901234567890123456789");

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void FormCreateBranch_should_construct_with_commit_chooser()
    {
        FormCreateBranch form = new();

        form.FindControl<CommitSummaryUserControl>("commitSummaryUserControl1").Should().NotBeNull();
        CommitPickerSmallControl picker = form.FindControl<CommitPickerSmallControl>("commitPicker")
            ?? throw new InvalidOperationException("Commit picker was not created.");
        Button pickCommit = picker.FindControl<Button>("buttonPickCommit")
            ?? throw new InvalidOperationException("Pick-commit button was not created.");
        pickCommit.IsVisible.Should().BeTrue();
    }

    [AvaloniaTest]
    public void FormCreateBranch_should_use_existing_translation_keys_once()
    {
        FormCreateBranch form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);
        form.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCreateBranch), "$this", "Text", "Create branch");
        translation.Received(1).AddTranslationItem(nameof(FormCreateBranch), "chkCheckoutAfterCreate", "Text", "Checkout &after create");
        translation.Received(1).AddTranslationItem(nameof(FormCreateBranch), "chkClearOrphan", "toolTip", "Remove files from the working directory and from the index");
        translation.Received(1).AddTranslationItem(nameof(FormCreateBranch), "chkCreateOrphan", "Text", "Create or&phan");
        translation.Received(1).AddTranslationItem(nameof(FormCreateBranch), "cmdOk", "Text", "&Create branch");
        translation.Received(1).AddTranslationItem(nameof(FormCreateBranch), "grpOrphan", "Text", "Orphan");
        translation.Received(1).AddTranslationItem(nameof(FormCreateBranch), "label1", "Text", "&Branch name");
        translation.Received(1).AddTranslationItem(nameof(FormCreateBranch), "lblCreateBranch", "Text", "Create b&ranch at this revision");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void CommitSummaryUserControl_should_use_existing_translation_keys_once()
    {
        CommitSummaryUserControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();

        control.AddTranslationItems(translation);
        control.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(CommitSummaryUserControl), "_noRevision", "Text", "No revision");
        translation.Received(1).AddTranslationItem(nameof(CommitSummaryUserControl), "_notAvailable", "Text", "n/a");
        translation.Received(1).AddTranslationItem(nameof(CommitSummaryUserControl), "labelAuthorCaption", "Text", "Author:");
        translation.Received(1).AddTranslationItem(nameof(CommitSummaryUserControl), "labelBranchesCaption", "Text", "Branch(es):");
        translation.Received(1).AddTranslationItem(nameof(CommitSummaryUserControl), "labelDateCaption", "Text", "Commit date:");
        translation.Received(1).AddTranslationItem(nameof(CommitSummaryUserControl), "labelTagsCaption", "Text", "Tag(s):");

        string[] emittedKeys = translation.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ITranslation.AddTranslationItem))
            .Select(call => string.Join('.', call.GetArguments().Take(3)))
            .ToArray();
        emittedKeys.Distinct(StringComparer.Ordinal).Count().Should().Be(emittedKeys.Length);
    }

    [AvaloniaTest]
    public void CreateClick_should_run_checkout_branch_command()
    {
        (IGitUICommands commands, IGitModule module) = CreateCommands();
        ArgumentString? executedArguments = null;
        commands.StartGitCommandProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Do<ArgumentString>(arguments => executedArguments = arguments))
            .Returns(true);

        FormCreateBranch form = new(commands, RevisionId, newBranchNamePrefix: "feature");
        TextBox branchName = form.FindControl<TextBox>("BranchNameTextBox")
            ?? throw new InvalidOperationException("Branch name field was not created.");
        CheckBox checkout = form.FindControl<CheckBox>("chkCheckoutAfterCreate")
            ?? throw new InvalidOperationException("Checkout option was not created.");
        Button create = form.FindControl<Button>("cmdOk")
            ?? throw new InvalidOperationException("Create button was not created.");

        branchName.Text = "feature/new";
        checkout.IsChecked = true;
        create.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        executedArguments.Should().NotBeNull();
        executedArguments!.Value.ToString().Should().Be($"checkout -b \"feature/new\" {RevisionId}");
        form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
        module.Received(1).CheckBranchFormat("feature/new");
    }

    [AvaloniaTest]
    public void CreateClick_should_run_orphan_branch_command_for_empty_repository()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.GetCurrentCheckout().Returns(default(ObjectId));
        module.IsBareRepository().Returns(false);
        module.CheckBranchFormat(Arg.Any<string>()).Returns(true);

        IGitBranchNameNormaliser normaliser = Substitute.For<IGitBranchNameNormaliser>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(IGitBranchNameNormaliser)).Returns(normaliser);
        ArgumentString? executedArguments = null;
        commands.StartGitCommandProcessDialog(
                Arg.Any<WinFormsShims.IWin32Window>(),
                Arg.Do<ArgumentString>(arguments => executedArguments = arguments))
            .Returns(true);

        FormCreateBranch form = new(commands, default);
        TextBox branchName = form.FindControl<TextBox>("BranchNameTextBox")
            ?? throw new InvalidOperationException("Branch name field was not created.");
        Button create = form.FindControl<Button>("cmdOk")
            ?? throw new InvalidOperationException("Create button was not created.");

        branchName.Text = "new-root";
        create.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        executedArguments.Should().NotBeNull();
        executedArguments!.Value.ToString().Should().Be("checkout --orphan new-root");
        form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
    }

    [AvaloniaTest]
    public void CreateClick_should_create_branch_in_real_repository()
    {
        string workingDirectory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.CreateBranch-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workingDirectory);
        try
        {
            using ServiceContainer services = new();
            GitExtUtils.ServiceContainerRegistry.RegisterServices(services);
            System.IO.Abstractions.FileSystem fileSystem = new();
            GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
            services.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
            services.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
            GitCommands.ServiceContainerRegistry.RegisterServices(services);

            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), workingDirectory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(Path.Combine(workingDirectory, "tracked.txt"), "content");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "tracked.txt" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            ObjectId revisionId = module.GetCurrentCheckout();

            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.GetService(Arg.Any<Type>()).Returns(call => services.GetService(call.Arg<Type>()));
            commands.StartGitCommandProcessDialog(Arg.Any<WinFormsShims.IWin32Window>(), Arg.Any<ArgumentString>())
                .Returns(call => module.GitExecutable.RunCommand(call.ArgAt<ArgumentString>(1)));

            FormCreateBranch form = new(commands, revisionId);
            TextBox branchName = form.FindControl<TextBox>("BranchNameTextBox")
                ?? throw new InvalidOperationException("Branch name field was not created.");
            CheckBox checkout = form.FindControl<CheckBox>("chkCheckoutAfterCreate")
                ?? throw new InvalidOperationException("Checkout option was not created.");
            Button create = form.FindControl<Button>("cmdOk")
                ?? throw new InvalidOperationException("Create button was not created.");

            branchName.Text = "avalonia-smoke";
            checkout.IsChecked = false;
            create.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            form.DialogResult.Should().Be(WinFormsShims.DialogResult.OK);
            module.GetRefs(RefsFilter.Heads).Select(gitRef => gitRef.Name).Should().Contain("avalonia-smoke");
        }
        finally
        {
            Directory.Delete(workingDirectory, recursive: true);
        }
    }

    [Test]
    public void StartCreateBranchDialog_should_reject_bare_repository()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.IsBareRepository().Returns(true);
        GitUICommands commands = new(Substitute.For<IServiceProvider>(), module);

        commands.StartCreateBranchDialog(objectId: RevisionId).Should().BeFalse();
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands()
    {
        GitRevision revision = new(RevisionId)
        {
            Author = "A. U. Thor",
            CommitUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Subject = "Initial revision",
        };
        IGitModule module = Substitute.For<IGitModule>();
        module.RevParse(RevisionId.ToString()).Returns(RevisionId);
        module.GetRevision(RevisionId, shortFormat: true, loadRefs: true).Returns(revision);
        module.CheckBranchFormat(Arg.Any<string>()).Returns(true);

        IGitBranchNameNormaliser normaliser = Substitute.For<IGitBranchNameNormaliser>();
        normaliser.Normalise(Arg.Any<string>(), Arg.Any<GitBranchNameOptions>())
            .Returns(call => call.ArgAt<string>(0));

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.GetService(typeof(IGitBranchNameNormaliser)).Returns(normaliser);
        return (commands, module);
    }
}
