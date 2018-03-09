using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GitUI;
using Gravatar;
using GravatarTests.Properties;
using NSubstitute;
using NUnit.Framework;

namespace GravatarTests
{
    [TestFixture]
    public class GravatarServiceTests
    {
        private const string Email = "x@x.com";
        private IImageCache _cache;
        private IImageNameProvider _avatarImageNameProvider;
        private GravatarService _service;

        [SetUp]
        public void Setup()
        {
            _cache = Substitute.For<IImageCache>();
            _avatarImageNameProvider = Substitute.For<IImageNameProvider>();
            _avatarImageNameProvider.Get(Email).Returns($"{Email}.png");

            _service = new GravatarService(_cache, _avatarImageNameProvider);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task GetAvatarAsync_should_not_call_gravatar_if_exist_in_cache()
        {
            var avatar = Resources.User;
            _cache.GetImageAsync(Arg.Any<string>(), null).Returns(avatar);

            var image = await _service.GetAvatarAsync(Email, 1, DefaultImageType.Identicon.ToString());

            image.Should().Be(avatar);
            Received.InOrder(() =>
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await _cache.Received(1).GetImageAsync($"{Email}.png", null);
                });
            });
            await _cache.DidNotReceive().AddImageAsync(Arg.Any<string>(), Arg.Any<Stream>());
        }

        [Ignore("Need to abstract WebClient or replace with HttpClient")]
        public void GetAvatarAsync_should_call_gravatar_if_absent_from_cache()
        {
            ////_service.ConfigureCache("", 1);
            ////var avatar = Resources.User;
            ////_cache.GetImageAsync(Arg.Any<string>(), null).Returns(_ => null, _ => avatar);
            ////
            ////var image = await _service.GetAvatarAsync(Email, 1, DefaultImageType.Identicon);
            ////
            ////image.Should().Be(avatar);
            ////Received.InOrder(async () =>
            ////{
            ////   await _cache.Received(1).GetImageAsync($"{Email}.png", null);
            ////   await _cache.Received(1).AddImageAsync($"{Email}.png", Arg.Any<Stream>());
            ////   await _cache.Received(1).GetImageAsync($"{Email}.png", null);
            ////});
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task RemoveAvatarAsync_should_invoke_cache_remove()
        {
            await _service.DeleteAvatarAsync(Email);

            Received.InOrder(() =>
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
                {
                    await _cache.Received(1).DeleteImageAsync($"{Email}.png");
                });
            });
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("boo")]
        public void GetDefaultImageType_should_return_None_if_requested_type_invalid(string imageType)
        {
            _service.GetDefaultImageType(imageType).Should().Be(DefaultImageType.None);
        }

        [TestCase("MonsterId")]
        [TestCase("monsterid")]
        [TestCase("MONSTERID")]
        public void GetDefaultImageType_should_return_corresponding_enum_case_insensitive(string imageType)
        {
            _service.GetDefaultImageType(imageType).Should().Be(DefaultImageType.MonsterId);
        }

        [Test]
        public void GetDefaultImageType_should_return_corresponding_enum()
        {
            foreach (var imageType in Enum.GetNames(typeof(DefaultImageType)))
            {
                _service.GetDefaultImageType(imageType).Should().Be(Enum.Parse(typeof(DefaultImageType), imageType));
            }
        }
    }
}
