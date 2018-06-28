using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    public abstract class AvatarCacheTestBase : AvatarTestBase
    {
        [Test]
        public async Task GetAvatarAsync_returns_correct_image()
        {
            Assert.AreSame(_img1, await _cache.GetAvatarAsync(_email1, _size));
            Assert.AreSame(_img2, await _cache.GetAvatarAsync(_email2, _size));
            Assert.AreSame(_img3, await _cache.GetAvatarAsync(_email3, _size));
        }

        [Test]
        public async Task ClearCacheAsync_removes_all_images_from_cache()
        {
            await MissAsync(_email1, _img1);
            await MissAsync(_email2, _img2);
            await MissAsync(_email3, _img3);

            await _cache.ClearCacheAsync();

#pragma warning disable 4014
            _inner.Received(1).ClearCacheAsync();
#pragma warning restore 4014

            await MissAsync(_email1, _img1);
            await MissAsync(_email2, _img2);
            await MissAsync(_email3, _img3);
        }
    }
}