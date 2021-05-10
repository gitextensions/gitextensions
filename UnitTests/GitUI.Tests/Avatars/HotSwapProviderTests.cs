using System.Drawing;
using System.Threading.Tasks;
using GitUI.Avatars;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    [TestFixture]
    public class HotSwapProviderTests
    {
        private const int _size = 16;
        private const string _email = "a@a.a";
        private const string _name = "John Lennon";

        private readonly Image _img;

        public HotSwapProviderTests()
        {
            _img = new Bitmap(_size, _size);
        }

        [Test]
        public async Task Returns_null_if_no_provider_is_set()
        {
            HotSwapAvatarProvider provider = new();
            var image = await provider.GetAvatarAsync(_email, _name, 16);
            Assert.Null(image);
        }

        [Test]
        public async Task Returns_the_same_image_as_the_wrapped_provider()
        {
            HotSwapAvatarProvider provider = new();
            var inner = Substitute.For<IAvatarProvider>();
            provider.Provider = inner;

            inner.GetAvatarAsync(_email, _name, _size).Returns(_img);

            var result = await provider.GetAvatarAsync(_email, _name, _size);

            Assert.AreSame(_img, result);
        }
    }
}
