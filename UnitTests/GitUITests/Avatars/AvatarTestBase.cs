using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using GitUI.Avatars;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    public abstract class AvatarTestBase
    {
        protected const int _size = 16;

        protected const string _email1 = "a@a.a";
        protected const string _email2 = "b@b.b";
        protected const string _email3 = "c@c.c";
        protected const string _email4 = "d@d.d";

        protected Image _img1;
        protected Image _img2;
        protected Image _img3;
        protected Image _img4;

        protected IAvatarProvider _inner;
        protected IAvatarProvider _cache;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _img1 = new Bitmap(_size, _size);
            _img2 = new Bitmap(_size, _size);
            _img3 = new Bitmap(_size, _size);
            _img4 = new Bitmap(_size, _size);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _img1.Dispose();
            _img2.Dispose();
            _img3.Dispose();
            _img4.Dispose();
        }

        [SetUp]
        public virtual void SetUp()
        {
            _inner = Substitute.For<IAvatarProvider>();

            _inner.GetAvatarAsync(_email1, _size).Returns(Task.FromResult(_img1));
            _inner.GetAvatarAsync(_email2, _size).Returns(Task.FromResult(_img2));
            _inner.GetAvatarAsync(_email3, _size).Returns(Task.FromResult(_img3));
            _inner.GetAvatarAsync(_email4, _size).Returns(Task.FromResult(_img4));
        }

#pragma warning disable 4014

        protected async Task MissAsync(string email, Image expected = null)
        {
            _inner.ClearReceivedCalls();

            var actual = await _cache.GetAvatarAsync(email, _size);

            _inner.Received(1).GetAvatarAsync(email, _size);

            if (expected != null)
            {
                Assert.AreSame(expected, actual);
            }
        }

        protected async Task HitAsync(string email, Image expected = null)
        {
            _inner.ClearReceivedCalls();

            var actual = await _cache.GetAvatarAsync(email, _size);

            _inner.Received(0).GetAvatarAsync(email, _size);

            if (expected != null)
            {
                Assert.AreSame(expected, actual);
            }
        }

#pragma warning restore 4014

        protected Stream GetPngStream()
        {
            var stream = new MemoryStream();
            _img1.Save(stream, ImageFormat.Png);
            stream.Position = 0;
            return stream;
        }
    }
}