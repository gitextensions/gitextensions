using System.Threading.Tasks;
using GitUI.Avatars;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    [TestFixture]
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
            await MissAsync(_email1, _img1);
            await HitAsync(_email1, _img1);
        }

        [Test]
        public async Task GetAvatarAsync_calls_inner_when_not_present_in_cache()
        {
            await MissAsync(_email1, _img1);
            await MissAsync(_email2, _img2);
            await MissAsync(_email3, _img3);
            await MissAsync(_email4, _img4);
            await MissAsync(_email1, _img1);
            await MissAsync(_email2, _img2);
            await MissAsync(_email3, _img3);
            await MissAsync(_email4, _img4);
            await HitAsync(_email2, _img2);
            await HitAsync(_email3, _img3);
            await HitAsync(_email4, _img4);
        }

        [Test]
        public async Task GetAvatarAsync_cleans_oldest_images()
        {
            // Populate the cache with three images, so that it is full
            await MissAsync(_email1, _img1);
            await MissAsync(_email2, _img2);
            await MissAsync(_email3, _img3);

            // Try getting the first image again
            await HitAsync(_email1, _img1);

            // At this point image 2 is the oldest

            // Try getting image 4
            await MissAsync(_email4, _img4);

            // That should have pushed images 2 and 3 out of the cache
            await MissAsync(_email2, _img2);
            await MissAsync(_email3, _img3);
        }
    }
}