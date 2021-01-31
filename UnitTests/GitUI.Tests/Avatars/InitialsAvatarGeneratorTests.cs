using GitUI.Avatars;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    [TestFixture]
    public sealed class InitialsAvatarGeneratorTests
    {
        [TestCase("albert.einstein@noreply.com", "albert einstein", "AE")]
        [TestCase("albert.einstein@noreply.com", "albert z middlename einstein (k)", "AE")]
        [TestCase("albert.einstein@noreply.com", "albert einstein (Nobel_price_winner)", "AE")]
        [TestCase("albert.einstein@noreply.com", "EINSTEIN, Albert middlename (Nobel_price_winner)", "EM")]
        [TestCase("albert.einstein@noreply.com", "albert", "Al")]
        [TestCase("albert.einstein@noreply.com", "", "AE")]
        [TestCase("albert.einstein@noreply.com", null, "AE")]
        [TestCase("albert.z.einstein@noreply.com", null, "AE")]
        [TestCase("albert_z_einstein@noreply.com", null, "AE")]
        [TestCase("albert.bose-einstein@noreply.com", null, "AE")]
        [TestCase("albert", "", "Al")]
        [TestCase("albert.einstein", "", "AE")]
        [TestCase(null, null, "?")]
        public void GetInitialsAndHashCode_return_initials_of_a_user(string email, string name, string expected)
        {
            var (initials, _) = new InitialsAvatarProvider().GetInitialsAndHashCode(email, name);

            Assert.AreEqual(expected, initials);
        }
    }
}
