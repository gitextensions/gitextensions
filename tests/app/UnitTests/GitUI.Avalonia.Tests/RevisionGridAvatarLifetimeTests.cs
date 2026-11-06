using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.Avatars;
using GitUI.UserControls.RevisionGrid.Columns;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using SkiaSharp;
using IHotkeySettingsLoader = ResourceManager.IHotkeySettingsLoader;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class RevisionGridAvatarLifetimeTests
{
    private const int AvatarSize = 20;
    private const string AuthorEmail = "author@example.com";
    private const string AuthorName = "Author";
    private JoinableTaskContext? _previousContext;
    private JoinableTaskContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        _previousContext = ThreadHelper.HasJoinableTaskContext ? ThreadHelper.JoinableTaskContext : null;
        _context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
        ThreadHelper.JoinableTaskContext = _context;
    }

    [TearDown]
    public void TearDown()
    {
        ThreadHelper.JoinableTaskContext = _previousContext!;
        _context.Dispose();
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task AvatarColumnProvider_should_not_retain_an_unreachable_owner_or_its_obsolete_cache_subscription(bool showAndClose)
    {
        TrackingCacheCleaner cleaner = new();
        (WeakReference<RevisionGridControl> grid, WeakReference<AvatarColumnProvider> column) = CreateUnreachableOwner(cleaner, showAndClose);
        cleaner.SubscriptionCount.Should().Be(1);

        Dispatcher.UIThread.RunJobs();

        // Headless rendering has no automatic timer tick. Closing queues renderer
        // disposal/composition work that must finish before testing cache ownership.
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
        CollectUnreachableOwners();

        grid.TryGetTarget(out _).Should().BeFalse("a shared avatar cache must not keep an unused or closed owner grid alive");
        column.TryGetTarget(out _).Should().BeFalse("the weak cache subscription must not capture its column through a callback");

        await cleaner.ClearCacheAsync();

        cleaner.SubscriptionCount.Should().Be(0);
        await cleaner.ClearCacheAsync();
        cleaner.SubscriptionCount.Should().Be(0);
    }

    [AvaloniaTest]
    public async Task AvatarColumnProvider_should_refresh_the_live_cache_generation_without_blanking_or_disposing_shared_provider_bytes()
    {
        byte[] firstImage = CreateImage(Colors.Red);
        byte[] secondImage = CreateImage(Colors.Blue);
        TaskCompletionSource<byte[]?> refreshResult = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync(AuthorEmail, AuthorName, AvatarSize)
            .Returns(Task.FromResult<byte[]?>(firstImage), refreshResult.Task);
        TrackingCacheCleaner cleaner = new();
        RevisionGridControl grid = new();
        AvatarColumnProvider column = new(grid, provider, cleaner);
        AvatarColumnProvider.AvatarCell cell = (AvatarColumnProvider.AvatarCell)column.CreateCell();
        GitRevision revision = new(ObjectId.Parse("1234567890abcdef1234567890abcdef12345678"))
        {
            Subject = "Avatar revision",
            Author = AuthorName,
            AuthorEmail = AuthorEmail,
        };

        try
        {
            column.UpdateCell(cell, revision);
            Bitmap first = cell.Source.Should().BeOfType<Bitmap>().Subject;
            column.UpdateCell(cell, revision);
            _ = provider.Received(1).GetAvatarAsync(AuthorEmail, AuthorName, AvatarSize);

            await cleaner.ClearCacheAsync();
            column.UpdateCell(cell, revision);
            Task<byte[]?> refresh = cell.LastLoadTask
                ?? throw new AssertionException("The live column must invalidate its cached cell generation after the real cache-clear event.");

            refresh.IsCompleted.Should().BeFalse();
            cell.Source.Should().BeSameAs(first);
            cleaner.SubscriptionCount.Should().Be(1);
            _ = provider.Received(2).GetAvatarAsync(AuthorEmail, AuthorName, AvatarSize);
            using Bitmap sharedFirst = AvatarImage.Decode(firstImage)
                ?? throw new AssertionException("A cell refresh must leave the provider's encoded cache result usable.");

            await Task.Run(() => refreshResult.SetResult(secondImage));
            await refresh.WaitAsync(TimeSpan.FromSeconds(10));

            Bitmap refreshed = cell.Source.Should().BeOfType<Bitmap>().Subject;
            refreshed.Should().NotBeSameAs(first);
            using SKBitmap pixels = ReadPixels(refreshed);
            pixels.GetPixel(AvatarSize / 2, AvatarSize / 2).Should().Be(SKColors.Blue);
            Action saveReplaced = () => AvatarImage.Encode(first);
            saveReplaced.Should().Throw<ObjectDisposedException>();
            cleaner.SubscriptionCount.Should().Be(1);
            GC.KeepAlive(column);
        }
        finally
        {
            cell.Clear();
            grid.CancelBackgroundTasks();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference<RevisionGridControl> Grid, WeakReference<AvatarColumnProvider> Column) CreateUnreachableOwner(
        IAvatarCacheCleaner cleaner,
        bool showAndClose)
    {
        RevisionGridControl grid = new() { UICommandsSource = CreateCommandsSource() };
        AvatarColumnProvider column = new(grid, Substitute.For<IAvatarProvider>(), cleaner);
        WeakReference<RevisionGridControl> weakGrid = new(grid);
        WeakReference<AvatarColumnProvider> weakColumn = new(column);
        if (showAndClose)
        {
            Window window = new() { Width = 800, Height = 180, Content = grid };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.Close();
            window.Content = null;
            Dispatcher.UIThread.RunJobs();
        }

        grid.CancelBackgroundTasks();
        return (weakGrid, weakColumn);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void CollectUnreachableOwners()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
    }

    private static IGitUICommandsSource CreateCommandsSource()
    {
        IGitUICommandsSource source = Substitute.For<IGitUICommandsSource>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(Substitute.For<IGitModule>());
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(Substitute.For<IHotkeySettingsLoader>());
        source.UICommands.Returns(commands);
        return source;
    }

    private static byte[] CreateImage(Avalonia.Media.Color color)
    {
        using RenderTargetBitmap bitmap = new(new PixelSize(AvatarSize, AvatarSize), new Vector(96, 96));
        using (DrawingContext context = bitmap.CreateDrawingContext())
        {
            context.FillRectangle(new SolidColorBrush(color), new Rect(0, 0, AvatarSize, AvatarSize));
        }

        return AvatarImage.Encode(bitmap);
    }

    private static SKBitmap ReadPixels(Bitmap bitmap)
    {
        using MemoryStream stream = new();
        bitmap.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        return SKBitmap.Decode(stream)
            ?? throw new AssertionException("The completed avatar must decode to actual pixel data.");
    }

    private sealed class TrackingCacheCleaner : IAvatarCacheCleaner
    {
        private EventHandler? _cacheCleared;

        public int SubscriptionCount => _cacheCleared?.GetInvocationList().Length ?? 0;

        public event EventHandler? CacheCleared
        {
            add => _cacheCleared += value;
            remove => _cacheCleared -= value;
        }

        public Task ClearCacheAsync()
        {
            _cacheCleared?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }
}
