using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Gravatar;
using NSubstitute;
using NUnit.Framework;

namespace GravatarTests
{
    [TestFixture]
    public class MruImageCacheTests : IDisposable
    {
        private readonly Image _img1 = new Bitmap(16, 16);
        private readonly Image _img2 = new Bitmap(16, 16);
        private readonly Image _img3 = new Bitmap(16, 16);
        private readonly Image _img4 = new Bitmap(16, 16);

        private IImageCache _inner;
        private IImageCache _cache;

        [SetUp]
        public void SetUp()
        {
            _inner = Substitute.For<IImageCache>();

            _inner.GetImage("img1").Returns(_img1);
            _inner.GetImage("img2").Returns(_img2);
            _inner.GetImage("img3").Returns(_img3);
            _inner.GetImage("img4").Returns(_img4);
            _inner.GetImageAsync("img1").Returns(Task.FromResult(_img1));
            _inner.GetImageAsync("img2").Returns(Task.FromResult(_img2));
            _inner.GetImageAsync("img3").Returns(Task.FromResult(_img3));
            _inner.GetImageAsync("img4").Returns(Task.FromResult(_img4));

            _cache = new MruImageCache(_inner, cleanAtSize: 3, cleanToSize: 1);
        }

        public void Dispose()
        {
            _img1.Dispose();
            _img2.Dispose();
            _img3.Dispose();
            _img4.Dispose();
        }

        [Test]
        public void AddImage_calls_inner()
        {
            _cache.AddImage("img1", _img1);

            _inner.Received().AddImage("img1", _img1);
        }

        [Test]
        public void AddImage_stores_image()
        {
            _cache.AddImage("img1", _img1);

            Assert.AreSame(_img1, _cache.GetImage("img1"));

            _inner.DidNotReceive().GetImage("img1");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void AddImage_throws_if_filename_not_supplied(string fileName)
        {
            Assert.ThrowsAsync<ArgumentException>(() => _cache.GetImageAsync(fileName));
        }

        [Test]
        public void GetImage_cleans_oldest_images()
        {
            // Populate the cache with three images, so that it is full
            Assert.AreSame(_img1, _cache.GetImage("img1"));
            _inner.Received(1).GetImage("img1");
            Thread.Sleep(20);
            Assert.AreSame(_img2, _cache.GetImage("img2"));
            _inner.Received(1).GetImage("img2");
            Thread.Sleep(20);
            Assert.AreSame(_img3, _cache.GetImage("img3"));
            _inner.Received(1).GetImage("img3");

            _inner.ClearReceivedCalls();

            // Try getting the first image again
            Assert.AreSame(_img1, _cache.GetImage("img1"));
            _inner.DidNotReceive().GetImage("img1");

            // At this point image 2 is the oldest

            // Try getting image 4
            Assert.AreSame(_img4, _cache.GetImage("img4"));
            _inner.Received(1).GetImage("img4");

            // That should have pushed images 2 and 3 out of the cache
            Assert.AreSame(_img2, _cache.GetImage("img2"));
            _inner.Received(1).GetImage("img2");

            Assert.AreSame(_img3, _cache.GetImage("img3"));
            _inner.Received(1).GetImage("img3");
        }

        [Test]
        public async Task GetImageAsync_cleans_oldest_images()
        {
            // Populate the cache with three images, so that it is full
            Assert.AreSame(_img1, await _cache.GetImageAsync("img1"));
            await _inner.Received(1).GetImageAsync("img1");
            await Task.Delay(20);
            Assert.AreSame(_img2, await _cache.GetImageAsync("img2"));
            await _inner.Received(1).GetImageAsync("img2");
            await Task.Delay(20);
            Assert.AreSame(_img3, await _cache.GetImageAsync("img3"));
            await _inner.Received(1).GetImageAsync("img3");

            _inner.ClearReceivedCalls();

            // Try getting the first image again
            Assert.AreSame(_img1, await _cache.GetImageAsync("img1"));
            await _inner.DidNotReceive().GetImageAsync("img1");

            // At this point image 2 is the oldest

            // Try getting image 4
            Assert.AreSame(_img4, await _cache.GetImageAsync("img4"));
            await _inner.Received(1).GetImageAsync("img4");

            // That should have pushed image 2 out of the cache
            Assert.AreSame(_img2, await _cache.GetImageAsync("img2"));
            await _inner.Received(1).GetImageAsync("img2");

            // That should have pushed image 3 out of the cache
            Assert.AreSame(_img3, await _cache.GetImageAsync("img3"));
            await _inner.Received(1).GetImageAsync("img3");
        }

        [Test]
        public void AddImage_throws_if_stream_null()
        {
            Assert.Throws<ArgumentNullException>(() => _cache.AddImage("file", null));
        }

        [Test]
        public void GetImage_calls_inner_the_first_time()
        {
            _cache.GetImage("img1");

            _inner.Received().GetImage("img1");

            _inner.ClearReceivedCalls();

            _cache.GetImage("img1");

            _inner.DidNotReceive().GetImage("img1");
        }

        [Test]
        public void GetImage_returns_null_for_unknown_image()
        {
            Assert.Null(_cache.GetImage("unknown"));
        }

        [Test]
        public void GetImage_returns_known_image()
        {
            _cache.AddImage("img1", _img1);
            _cache.AddImage("img2", _img2);
            _cache.AddImage("img3", _img3);

            Assert.AreSame(_img1, _cache.GetImage("img1"));
            Assert.AreSame(_img2, _cache.GetImage("img2"));
            Assert.AreSame(_img3, _cache.GetImage("img3"));
        }

        [Test]
        public async Task GetImageAsync_returns_null_for_unknown_image()
        {
            _cache.AddImage("name", _img1);

            Assert.Null(await _cache.GetImageAsync("unknown"));

            await _inner.Received(1).GetImageAsync("unknown");
        }

        [Test]
        public void GetImage_throws_if_filename_not_supplied()
        {
            Assert.Throws<ArgumentException>(() => _cache.GetImage(null));
        }

        [Test]
        public async Task GetImageAsync_returns_known_image()
        {
            _cache.AddImage("img1", _img1);
            _cache.AddImage("img2", _img2);
            _cache.AddImage("img3", _img3);

            Assert.AreSame(_img1, await _cache.GetImageAsync("img1"));
            Assert.AreSame(_img2, await _cache.GetImageAsync("img2"));
            Assert.AreSame(_img3, await _cache.GetImageAsync("img3"));
        }

        [Test]
        public void GetImageAsync_throws_if_filename_not_supplied()
        {
            Assert.ThrowsAsync<ArgumentException>(() => _cache.GetImageAsync(null));
        }

        [Test]
        public async Task ClearAsync_removes_all_images_from_cache()
        {
            _cache.AddImage("img1", _img1);
            _cache.AddImage("img2", _img2);
            _cache.AddImage("img3", _img3);

            await _cache.ClearAsync();
            await _inner.Received(1).ClearAsync();

            await _cache.GetImageAsync("img1");
            await _inner.Received(1).GetImageAsync("img1");
        }

        [Test]
        public async Task DeleteImageAsync_removes_specific_image_from_cache()
        {
            _cache.AddImage("img1", _img1);
            _cache.AddImage("img2", _img2);
            _cache.AddImage("img3", _img3);

            await _cache.DeleteImageAsync("img2");
            await _inner.Received(1).DeleteImageAsync("img2");

            await _cache.GetImageAsync("img1");
            await _inner.DidNotReceive().GetImageAsync("img1");

            await _cache.GetImageAsync("img2");
            await _inner.Received(1).GetImageAsync("img2");

            await _cache.GetImageAsync("img3");
            await _inner.DidNotReceive().GetImageAsync("img3");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void DeleteImage_should_throw_if_filename_not_supplied(string fileName)
        {
            Assert.ThrowsAsync<ArgumentException>(() => _cache.DeleteImageAsync(fileName));
        }
    }
}