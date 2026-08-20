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

        provider.GetInitialsAndColorIndex("nikola@example.com", "Nikola Begovic").initials.Should().Be("NB");
        imageData.Should().NotBeNullOrEmpty();
        AvatarImage.GetPixelSize(imageData).Should().Be(new PixelSize(32, 32));
    }

    [AvaloniaTest]
    public async Task InitialsAvatarProvider_should_render_the_same_avatar_from_a_background_request()
    {
        InitialsAvatarProvider provider = new();
        byte[] expected = (await provider.GetAvatarAsync(
            "nikola@example.com",
            "Nikola Begovic",
            20))!;

        Task<byte[]?> backgroundRequest = Task.Run(
            () => provider.GetAvatarAsync("nikola@example.com", "Nikola Begovic", 20));
        await WaitUntilAsync(() => backgroundRequest.IsCompleted);

        byte[]? actual = await backgroundRequest;
        actual.Should().Equal(expected);
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
    public void AvatarColumnProvider_should_start_at_the_Designer_width_and_follow_the_runtime_row_height()
    {
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        IAvatarCacheCleaner cleaner = Substitute.For<IAvatarCacheCleaner>();
        RevisionGridControl grid = new();
        AvatarColumnProvider column = new(grid, provider, cleaner);

        column.Column.Width.Value.Should().Be(32);

        column.ApplyRowHeight(26);

        column.Column.Width.Value.Should().Be(26);
    }

    [AvaloniaTest]
    public async Task Avatar_cell_should_retain_a_resolved_image_when_reattached_for_the_same_author()
    {
        byte[] imageData = (await new InitialsAvatarProvider().GetAvatarAsync(
            "author@example.com",
            "Author",
            20))!;
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync("author@example.com", "Author", 20)
            .Returns(Task.FromResult<byte[]?>(imageData));
        AvatarColumnProvider.AvatarCell cell = new(provider);
        Window window = new() { Content = cell };
        window.Show();
        try
        {
            cell.Load("author@example.com", "Author", cacheVersion: 0);
            await WaitUntilAsync(() => cell.Source is Bitmap);
            Bitmap resolved = (Bitmap)cell.Source!;

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            cell.Source.Should().BeSameAs(resolved);

            window.Content = cell;
            Dispatcher.UIThread.RunJobs();
            cell.Load("author@example.com", "Author", cacheVersion: 0);

            cell.Source.Should().BeSameAs(resolved);
            _ = provider.Received(1).GetAvatarAsync("author@example.com", "Author", 20);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task Avatar_cell_should_ignore_a_late_result_from_a_recycled_row()
    {
        byte[] firstImageData = (await new InitialsAvatarProvider().GetAvatarAsync(
            "first@example.com",
            "First Author",
            20))!;
        byte[] secondImageData = (await new InitialsAvatarProvider().GetAvatarAsync(
            "second@example.com",
            "Second Author",
            20))!;
        TaskCompletionSource<byte[]?> firstResult = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource<byte[]?> secondResult = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync("first@example.com", "First Author", 20).Returns(firstResult.Task);
        provider.GetAvatarAsync("second@example.com", "Second Author", 20).Returns(secondResult.Task);
        AvatarColumnProvider.AvatarCell cell = new(provider);

        cell.Load("first@example.com", "First Author", cacheVersion: 0);
        cell.Load("second@example.com", "Second Author", cacheVersion: 0);
        secondResult.SetResult(secondImageData);
        await WaitUntilAsync(() => cell.Source is Bitmap);
        Bitmap second = (Bitmap)cell.Source!;

        firstResult.SetResult(firstImageData);
        await WaitUntilAsync(() => firstResult.Task.IsCompleted);
        Dispatcher.UIThread.RunJobs();

        cell.Source.Should().BeSameAs(second);
    }

    [AvaloniaTest]
    public async Task Avatar_cell_should_dispose_the_bitmap_replaced_for_a_different_author()
    {
        InitialsAvatarProvider initials = new();
        byte[] firstImageData = (await initials.GetAvatarAsync(
            "first@example.com",
            "First Author",
            20))!;
        byte[] secondImageData = (await initials.GetAvatarAsync(
            "second@example.com",
            "Second Author",
            20))!;
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync("first@example.com", "First Author", 20)
            .Returns(Task.FromResult<byte[]?>(firstImageData));
        provider.GetAvatarAsync("second@example.com", "Second Author", 20)
            .Returns(Task.FromResult<byte[]?>(secondImageData));
        AvatarColumnProvider.AvatarCell cell = new(provider);

        cell.Load("first@example.com", "First Author", cacheVersion: 0);
        await WaitUntilAsync(() => cell.Source is Bitmap);
        Bitmap replaced = (Bitmap)cell.Source!;

        cell.Load("second@example.com", "Second Author", cacheVersion: 0);
        await WaitUntilAsync(() => cell.Source is Bitmap bitmap && !ReferenceEquals(bitmap, replaced));

        using MemoryStream output = new();
        Action saveReplaced = () => replaced.Save(output, PngBitmapEncoderOptions.Default);
        saveReplaced.Should().Throw<ObjectDisposedException>();
    }

    [AvaloniaTest]
    public async Task Avatar_cache_clear_should_refresh_without_blanking_the_resolved_image()
    {
        byte[] firstImageData = (await new InitialsAvatarProvider().GetAvatarAsync(
            "author@example.com",
            "Author",
            20))!;
        byte[] refreshedImageData = (await new InitialsAvatarProvider().GetAvatarAsync(
            "author@example.com",
            "Changed Author",
            20))!;
        TaskCompletionSource<byte[]?> refreshedResult = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync("author@example.com", "Author", 20)
            .Returns(Task.FromResult<byte[]?>(firstImageData), refreshedResult.Task);
        IAvatarCacheCleaner cleaner = Substitute.For<IAvatarCacheCleaner>();
        RevisionGridControl grid = new();
        AvatarColumnProvider column = new(grid, provider, cleaner);
        AvatarColumnProvider.AvatarCell cell = (AvatarColumnProvider.AvatarCell)column.CreateCell();
        GitRevision revision = CreateRevision();

        column.UpdateCell(cell, revision);
        await WaitUntilAsync(() => cell.Source is Bitmap);
        Bitmap first = (Bitmap)cell.Source!;

        cleaner.CacheCleared += Raise.Event();
        column.UpdateCell(cell, revision);

        cell.Source.Should().BeSameAs(first);
        _ = provider.Received(2).GetAvatarAsync("author@example.com", "Author", 20);

        refreshedResult.SetResult(refreshedImageData);
        await WaitUntilAsync(() => cell.Source is Bitmap bitmap && !ReferenceEquals(bitmap, first));
    }

    [AvaloniaTest]
    public async Task AvatarService_should_render_configured_author_initials_instead_of_the_safety_image()
    {
        AvatarProvider originalProvider = AppSettings.AvatarProvider;
        AvatarFallbackType originalFallback = AppSettings.AvatarFallbackType;
        try
        {
            AppSettings.AvatarProvider = AvatarProvider.None;
            AppSettings.AvatarFallbackType = AvatarFallbackType.AuthorInitials;
            AvatarService.UpdateAvatarProvider();
            await AvatarService.CacheCleaner.ClearCacheAsync();
            byte[] expected = (await new InitialsAvatarProvider().GetAvatarAsync(
                "nikola@example.com",
                "Nikola Begovic",
                20))!;

            byte[]? actual = await AvatarService.DefaultProvider.GetAvatarAsync(
                "nikola@example.com",
                "Nikola Begovic",
                20);

            actual.Should().Equal(expected);
        }
        finally
        {
            AppSettings.AvatarProvider = originalProvider;
            AppSettings.AvatarFallbackType = originalFallback;
            AvatarService.UpdateAvatarProvider();
            await AvatarService.CacheCleaner.ClearCacheAsync();
        }
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
