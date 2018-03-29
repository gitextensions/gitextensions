using System;
using FluentAssertions;
using Gravatar;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace GravatarTests
{
    [TestFixture]
    public class AvatarImageNameProviderTests
    {
        private IImageNameProvider _provider;

        [SetUp]
        public void Setup()
        {
            _provider = new AvatarImageNameProvider();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\t")]
        public void Get_throws_if_email_null_or_whitespace(string email)
        {
            Assert.Throws<ArgumentException>(() => _provider.Get(email));
        }

        [Test]
        public void Get_return_image_name()
        {
            _provider.Get("x@x.com").Should().Be("x@x.com.png");
        }
    }
}