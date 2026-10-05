using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using GitUI.Avatars;
using GitUI.UserControls.RevisionGrid.Columns;
using NSubstitute;
using SkiaSharp;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GitExtensionsTests;

[TestFixture]
public sealed class RevisionGridAvatarPresentationTests
{
    private const int AvatarSize = 20;
    private const string FirstEmail = "first@example.com";
    private const string FirstName = "First Author";
    private const string SecondEmail = "second@example.com";
    private const string SecondName = "Second Author";

    [AvaloniaTest]
    public async Task AvatarCell_Load_should_present_warm_cache_hits_inline_without_a_placeholder_on_recycled_rows()
    {
        byte[] firstImage = CreateImage(Colors.Red);
        byte[] secondImage = CreateImage(Colors.Blue);
        IAvatarProvider inner = Substitute.For<IAvatarProvider>();
        inner.GetAvatarAsync(FirstEmail, FirstName, AvatarSize).Returns(Task.FromResult<byte[]?>(firstImage));
        inner.GetAvatarAsync(SecondEmail, SecondName, AvatarSize).Returns(Task.FromResult<byte[]?>(secondImage));
        AvatarMemoryCache cache = new(inner, capacity: 3);
        IAvatarProvider provider = new SafetynetAvatarProvider(new ChainedAvatarProvider(
            cache,
            new StaticImageAvatarProvider(CreateImage(Colors.Yellow))));
        await provider.GetAvatarAsync(FirstEmail, FirstName, AvatarSize);
        await provider.GetAvatarAsync(SecondEmail, SecondName, AvatarSize);
        using Bitmap placeholder = DecodeImage(CreateImage(Colors.Lime));
        AvatarColumnProvider.AvatarCell cell = new(provider, placeholder);
        List<IImage?> presentedImages = [];
        cell.PropertyChanged += (_, e) =>
        {
            if (e.Property == Image.SourceProperty)
            {
                presentedImages.Add(cell.Source);
            }
        };

        try
        {
            for (int index = 0; index < 40; index++)
            {
                bool first = index % 2 == 0;
                cell.Load(first ? FirstEmail : SecondEmail, first ? FirstName : SecondName, cacheVersion: 0, AvatarSize);

                cell.LastLoadTask.Should().NotBeNull();
                cell.LastLoadTask!.IsCompletedSuccessfully.Should().BeTrue();
                cell.Source.Should().BeOfType<Bitmap>();
                cell.Source.Should().NotBeSameAs(placeholder);
                using SKBitmap pixels = ReadPixels((Bitmap)cell.Source!);
                pixels.GetPixel(AvatarSize / 2, AvatarSize / 2).Should().Be(first ? SKColors.Red : SKColors.Blue);
            }

            presentedImages.Should().HaveCount(40);
            presentedImages.Should().NotContain(image => ReferenceEquals(image, placeholder));
            _ = inner.Received(1).GetAvatarAsync(FirstEmail, FirstName, AvatarSize);
            _ = inner.Received(1).GetAvatarAsync(SecondEmail, SecondName, AvatarSize);
        }
        finally
        {
            cell.Clear();
        }
    }

    [AvaloniaTest]
    [TestCase("#FFF0F0F0")]
    [TestCase("#FF0078D7")]
    [TestCase("#FF202020")]
    [TestCase("#FF416D35")]
    public void AvatarCell_Render_should_cut_the_source_pixel_blocks_out_of_all_four_corners(string backgroundArgb)
    {
        Color background = Color.Parse(backgroundArgb);
        byte[] image = CreateImage(Colors.Red);
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync(FirstEmail, FirstName, AvatarSize).Returns(Task.FromResult<byte[]?>(image));
        AvatarColumnProvider.AvatarCell cell = new(provider);
        Border host = new()
        {
            Background = new SolidColorBrush(background),
            Padding = new Thickness(2),
            Child = cell,
        };
        Window window = new() { Width = AvatarSize + 4, Height = AvatarSize + 4, Content = host };
        window.Show();
        try
        {
            cell.Load(FirstEmail, FirstName, cacheVersion: 0, AvatarSize);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            cell.Bounds.Size.Should().Be(new Size(AvatarSize, AvatarSize));
            Point origin = cell.TranslatePoint(default, window)
                ?? throw new AssertionException("The avatar must be attached to its actual row background.");
            using WriteableBitmap frame = window.CaptureRenderedFrame()
                ?? throw new AssertionException("The real avatar visual tree must render a frame.");
            using SKBitmap pixels = ReadPixels(frame);
            SKColor backdrop = new(background.R, background.G, background.B, background.A);

            for (int y = 0; y < AvatarSize; y++)
            {
                for (int x = 0; x < AvatarSize; x++)
                {
                    bool outerColumn = x == 0 || x == AvatarSize - 1;
                    bool outerRow = y == 0 || y == AvatarSize - 1;
                    bool nearColumn = x < 2 || x >= AvatarSize - 2;
                    bool nearRow = y < 2 || y >= AvatarSize - 2;
                    bool cutCorner = (outerColumn && nearRow) || (outerRow && nearColumn);
                    pixels.GetPixel((int)origin.X + x, (int)origin.Y + y).Should().Be(
                        cutCorner ? backdrop : SKColors.Red,
                        "the source paints perpendicular corner blocks at avatar pixel ({0}, {1})", x, y);
                }
            }
        }
        finally
        {
            cell.Clear();
            window.Close();
        }
    }

    [AvaloniaTest]
    public async Task AvatarCell_Load_should_keep_the_placeholder_only_until_a_genuinely_pending_request_completes()
    {
        byte[] image = CreateImage(Colors.Red);
        TaskCompletionSource<byte[]?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync(FirstEmail, FirstName, AvatarSize).Returns(completion.Task);
        using Bitmap placeholder = DecodeImage(CreateImage(Colors.Lime));
        AvatarColumnProvider.AvatarCell cell = new(provider, placeholder);
        try
        {
            cell.Load(FirstEmail, FirstName, cacheVersion: 0, AvatarSize);
            Task<byte[]?> load = cell.LastLoadTask
                ?? throw new AssertionException("The unresolved avatar request must remain owned by its cell.");

            load.IsCompleted.Should().BeFalse();
            cell.Source.Should().BeSameAs(placeholder);
            await Task.Run(() => completion.SetResult(image));
            await load.WaitAsync(TimeSpan.FromSeconds(10));

            cell.Source.Should().BeOfType<Bitmap>();
            using SKBitmap pixels = ReadPixels((Bitmap)cell.Source!);
            pixels.GetPixel(AvatarSize / 2, AvatarSize / 2).Should().Be(SKColors.Red);
        }
        finally
        {
            cell.Clear();
        }
    }

    [AvaloniaTest]
    [TestCase(false)]
    [TestCase(true)]
    public async Task AvatarCell_Load_should_ignore_a_pending_result_after_a_cached_recycle_or_clear(bool clear)
    {
        byte[] firstImage = CreateImage(Colors.Red);
        byte[] secondImage = CreateImage(Colors.Blue);
        TaskCompletionSource<byte[]?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync(FirstEmail, FirstName, AvatarSize).Returns(completion.Task);
        provider.GetAvatarAsync(SecondEmail, SecondName, AvatarSize).Returns(Task.FromResult<byte[]?>(secondImage));
        AvatarColumnProvider.AvatarCell cell = new(provider);
        try
        {
            cell.Load(FirstEmail, FirstName, cacheVersion: 0, AvatarSize);
            Task<byte[]?> pending = cell.LastLoadTask
                ?? throw new AssertionException("The unresolved avatar request must remain owned by its cell.");
            if (clear)
            {
                cell.Clear();
            }
            else
            {
                cell.Load(SecondEmail, SecondName, cacheVersion: 0, AvatarSize);
                cell.LastLoadTask!.IsCompletedSuccessfully.Should().BeTrue();
            }

            IImage? expected = cell.Source;
            await Task.Run(() => completion.SetResult(firstImage));
            await pending.WaitAsync(TimeSpan.FromSeconds(10));

            cell.Source.Should().BeSameAs(expected);
            if (!clear)
            {
                using SKBitmap pixels = ReadPixels((Bitmap)cell.Source!);
                pixels.GetPixel(AvatarSize / 2, AvatarSize / 2).Should().Be(SKColors.Blue);
            }
        }
        finally
        {
            cell.Clear();
        }
    }

    [AvaloniaTest]
    public async Task AvatarCell_Load_should_preserve_a_resolved_image_during_a_pending_cache_generation_refresh()
    {
        byte[] firstImage = CreateImage(Colors.Red);
        byte[] secondImage = CreateImage(Colors.Blue);
        TaskCompletionSource<byte[]?> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        IAvatarProvider provider = Substitute.For<IAvatarProvider>();
        provider.GetAvatarAsync(FirstEmail, FirstName, AvatarSize)
            .Returns(Task.FromResult<byte[]?>(firstImage), completion.Task);
        AvatarColumnProvider.AvatarCell cell = new(provider);
        try
        {
            cell.Load(FirstEmail, FirstName, cacheVersion: 0, AvatarSize);
            Bitmap first = cell.Source.Should().BeOfType<Bitmap>().Subject;

            cell.Load(FirstEmail, FirstName, cacheVersion: 1, AvatarSize);
            Task<byte[]?> refresh = cell.LastLoadTask
                ?? throw new AssertionException("The generation refresh must retain its real pending request.");
            cell.Source.Should().BeSameAs(first);
            using SKBitmap retained = ReadPixels(first);
            retained.GetPixel(AvatarSize / 2, AvatarSize / 2).Should().Be(SKColors.Red);

            await Task.Run(() => completion.SetResult(secondImage));
            await refresh.WaitAsync(TimeSpan.FromSeconds(10));

            Bitmap refreshed = cell.Source.Should().BeOfType<Bitmap>().Subject;
            refreshed.Should().NotBeSameAs(first);
            using SKBitmap pixels = ReadPixels(refreshed);
            pixels.GetPixel(AvatarSize / 2, AvatarSize / 2).Should().Be(SKColors.Blue);
            Action saveReplaced = () => AvatarImage.Encode(first);
            saveReplaced.Should().Throw<ObjectDisposedException>();
        }
        finally
        {
            cell.Clear();
        }
    }

    private static byte[] CreateImage(Color color)
    {
        using RenderTargetBitmap bitmap = new(new PixelSize(AvatarSize, AvatarSize), new Vector(96, 96));
        using (DrawingContext context = bitmap.CreateDrawingContext())
        {
            context.FillRectangle(new SolidColorBrush(color), new Rect(0, 0, AvatarSize, AvatarSize));
        }

        return AvatarImage.Encode(bitmap);
    }

    private static Bitmap DecodeImage(byte[] bytes)
        => AvatarImage.Decode(bytes)
            ?? throw new AssertionException("The encoded avatar fixture must decode to a real bitmap.");

    private static SKBitmap ReadPixels(Bitmap bitmap)
    {
        using MemoryStream stream = new();
        bitmap.Save(stream, PngBitmapEncoderOptions.Default);
        stream.Position = 0;
        return SKBitmap.Decode(stream)
            ?? throw new AssertionException("The rendered avatar must decode to actual pixel data.");
    }
}
