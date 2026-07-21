using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.Avatars;
using GitUI.CommitInfo;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class AvatarTests
{
    [SetUp]
    public void SetUp()
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public async Task InitialsAvatarProvider_should_return_encoded_bitmap_at_requested_size()
    {
        InitialsAvatarProvider provider = new();

        byte[]? imageData = await provider.GetAvatarAsync("nikola@example.com", "Nikola Begovic", 32);

        imageData.Should().NotBeNullOrEmpty();
        AvatarImage.GetPixelSize(imageData).Should().Be(new PixelSize(32, 32));
    }

    [Test]
    public async Task AvatarMemoryCache_should_request_inner_only_once()
    {
        byte[] imageData = [1, 2, 3];
        IAvatarProvider inner = Substitute.For<IAvatarProvider>();
        inner.GetAvatarAsync("author@example.com", "Author", 20).Returns(Task.FromResult<byte[]?>(imageData));
        AvatarMemoryCache cache = new(inner, capacity: 3);

        byte[]? first = await cache.GetAvatarAsync("author@example.com", "Author", 20);
        byte[]? second = await cache.GetAvatarAsync("author@example.com", "Author", 20);

        first.Should().BeSameAs(imageData);
        second.Should().BeSameAs(imageData);
        _ = inner.Received(1).GetAvatarAsync("author@example.com", "Author", 20);
    }

    [AvaloniaTest]
    public async Task AvatarColumnProvider_should_render_provider_bytes()
    {
        InitialsAvatarProvider initials = new();
        byte[] imageData = (await initials.GetAvatarAsync("author@example.com", "Author", 20))
            ?? throw new InvalidOperationException("The initials provider returned no image.");
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync("author@example.com", "Author", 20).Returns(Task.FromResult<byte[]?>(imageData));
        IAvatarCacheCleaner cleaner = Substitute.For<IAvatarCacheCleaner>();
        RevisionGridControl grid = new();
        AvatarColumnProvider column = new(grid, provider, cleaner);
        Image cell = (Image)column.CreateCell();
        GitRevision revision = CreateRevision();

        column.UpdateCell(cell, revision);
        await WaitUntilAsync(() => cell.Source is Bitmap);

        _ = provider.Received(1).GetAvatarAsync("author@example.com", "Author", 20);
        AvatarImage.GetPixelSize(imageData).Should().Be(new PixelSize(20, 20));
    }

    [AvaloniaTest]
    public async Task CommitInfo_should_show_avatar_when_enabled()
    {
        bool originalShowAvatar = AppSettings.ShowAuthorAvatarInCommitInfo;
        AvatarProvider originalProvider = AppSettings.AvatarProvider;
        AvatarFallbackType originalFallback = AppSettings.AvatarFallbackType;
        try
        {
            AppSettings.ShowAuthorAvatarInCommitInfo = true;
            AppSettings.AvatarProvider = AvatarProvider.None;
            AppSettings.AvatarFallbackType = AvatarFallbackType.AuthorInitials;
            AvatarService.UpdateAvatarProvider();
            await AvatarService.CacheCleaner.ClearCacheAsync();

            CommitInfo control = new() { Revision = CreateRevision() };
            AvatarControl avatar = control.GetTestAccessor().Avatar;

            await WaitUntilAsync(() => avatar.GetTestAccessor().Image.Source is Bitmap);

            avatar.IsVisible.Should().BeTrue();
            avatar.GetTestAccessor().Image.Source.Should().BeOfType<Bitmap>();
        }
        finally
        {
            AppSettings.ShowAuthorAvatarInCommitInfo = originalShowAvatar;
            AppSettings.AvatarProvider = originalProvider;
            AppSettings.AvatarFallbackType = originalFallback;
            AvatarService.UpdateAvatarProvider();
        }
    }

    [AvaloniaTest]
    public void AvatarControl_should_preserve_original_translation_keys()
    {
        ITranslation translation = Substitute.For<ITranslation>();
        AvatarControl control = new();

        control.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(AvatarControl), "clearImagecacheToolStripMenuItem", "Text", "Clear image cache");
        translation.Received(1).AddTranslationItem(nameof(AvatarControl), "avatarProviderToolStripMenuItem", "Text", "Avatar provider");
        translation.Received(1).AddTranslationItem(nameof(AvatarControl), "registerGravatarToolStripMenuItem", "Text", "Register at gravatar.com");
    }

    private static GitRevision CreateRevision()
        => new(ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
        {
            Subject = "Avatar revision",
            Author = "Author",
            AuthorEmail = "author@example.com",
            Committer = "Committer",
            CommitterEmail = "committer@example.com",
        };

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        throw new TimeoutException("The avatar operation did not complete.");
    }
}
