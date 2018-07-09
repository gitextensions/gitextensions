using System;
using System.Threading.Tasks;
using GitUI.Avatars;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    [TestFixture]
    public sealed class BackupAvatarProviderTests : AvatarTestBase
    {
        public override void SetUp()
        {
            base.SetUp();

            _cache = new BackupAvatarProvider(_inner, _img4);
        }

        [Test]
        public async Task GetAvatarAsync_return_backup_image_if_inner_throws()
        {
            _inner.GetAvatarAsync(Arg.Any<string>(), Arg.Any<int>()).Throws(new Exception());

            Assert.AreSame(_img4, await _cache.GetAvatarAsync(_email1, _size));
        }

        [Test]
        public async Task GetAvatarAsync_returns_image_from_inner()
        {
            await MissAsync(_email1, _img1);
            await MissAsync(_email1, _img1);
            await MissAsync(_email2, _img2);
            await MissAsync(_email2, _img2);
            await MissAsync(_email3, _img3);
            await MissAsync(_email3, _img3);
            await MissAsync(_email4, _img4);
            await MissAsync(_email4, _img4);
            await MissAsync(_email4, _img4);
        }
    }
}