using System.Drawing;
using System.Threading.Tasks;
using GitUI.Avatars;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    [TestFixture]
    public class StaticImageAvatarProviderTests
    {
        private const int _size = 64;

        private const string _email = "a@a.a";
        private const string _name = "John Lennon";

        private readonly Image _img;

        public StaticImageAvatarProviderTests()
        {
            _img = new Bitmap(_size, _size);
        }

        [Test]
        public async Task Original_image_is_returned_if_size_matches()
        {
            StaticImageAvatarProvider provider = new(_img);

            var result = await provider.GetAvatarAsync(_email, _name, _size);

            Assert.AreSame(_img, result);
        }

        [Test]
        public async Task Resized_images_are_cached_and_same_instance_is_returned_on_second_call()
        {
            StaticImageAvatarProvider provider = new(_img);
            var otherSize = 32;

            var result1 = await provider.GetAvatarAsync(_email, _name, otherSize);
            var result2 = await provider.GetAvatarAsync(_email, _name, otherSize);

            Assert.AreSame(result1, result2);
        }
    }
}
