using GitUI.Avatars;
using NSubstitute;

namespace GitUITests.Avatars;
public sealed class AvatarMemoryCacheTests : AvatarCacheTestBase
{
    public override void SetUp()
    {
        base.SetUp();

        _cache = new AvatarMemoryCache(_inner, capacity: 3);
    }

    [Test]
    public async Task GetAvatarAsync_calls_inner_the_first_time()
    {
        await MissAsync(_email1, _name1, _img1);
        await HitAsync(_email1, _name1, _img1);
    }

    [Test]
    public async Task GetAvatarAsync_calls_inner_when_not_present_in_cache()
    {
        await MissAsync(_email1, _name1, _img1);
        await MissAsync(_email2, _name2, _img2);
        await MissAsync(_email3, _name3, _img3);
        await MissAsync(_email4, _name4, _img4);
        await MissAsync(_email1, _name1, _img1);
        await MissAsync(_email2, _name2, _img2);
        await MissAsync(_email3, _name3, _img3);
        await MissAsync(_email4, _name4, _img4);
        await HitAsync(_email2, _name2, _img2);
        await HitAsync(_email3, _name3, _img3);
        await HitAsync(_email4, _name4, _img4);
    }

    [Test]
    public async Task GetAvatarAsync_cleans_oldest_images()
    {
        // Populate the cache with three images, so that it is full
        await MissAsync(_email1, _name1, _img1);
        await MissAsync(_email2, _name2, _img2);
        await MissAsync(_email3, _name3, _img3);

        // Try getting the first image again
        await HitAsync(_email1, _name1, _img1);

        // At this point image 2 is the oldest

        // Try getting image 4
        await MissAsync(_email4, _name4, _img4);

        // That should have pushed images 2 and 3 out of the cache
        await MissAsync(_email2, _name2, _img2);
        await MissAsync(_email3, _name3, _img3);
    }

    [Test]
    public async Task ClearCacheAsync_must_not_dispose_an_image_it_handed_out()
    {
        // Own the image instead of reusing one of the shared ones, which the cache would dispose
        // before the sibling tests run
        const string email = "cleared@avatar.com";
        const string name = "Cleared Cache";
        using Bitmap image = new(_size, _size);
        _inner.GetAvatarAsync(email, name, _size).Returns(Task.FromResult<Image?>(image));

        Image? handedOut = await _cache.GetAvatarAsync(email, name, _size);

        await _cacheCleaner.ClearCacheAsync();

        handedOut.Should().BeSameAs(image);
        ((Action)(() => _ = handedOut!.Size)).Should()
            .NotThrow<ArgumentException>("a consumer may still be painting the image the cache handed out");
    }
}
