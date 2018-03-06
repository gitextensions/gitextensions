using FluentAssertions;
using Gravatar;
using NUnit.Framework;

namespace GravatarTests
{
    [TestFixture]
    public class AvatarImageNameProviderTests
    {
        private AvatarImageNameProvider _provider;

        [SetUp]
        public void Setup()
        {
            _provider = new AvatarImageNameProvider();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void Get_return_null_if_email_not_supplied(string email)
        {
            _provider.Get(email).Should().BeNull();
        }

        [Test]
        public void Get_return_image_name()
        {
            _provider.Get("x@x.com").Should().Be("x@x.com.png");
        }
    }
}