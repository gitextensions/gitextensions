using GitUI.Avatars;

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
        [TestCase("", "albert.einstein", "AE")]
        [TestCase("", "AlbertEinstein", "AE")]
        [TestCase("", "A-Einstein", "AE")]
        [TestCase("", "Alb-Einstein", "AE")]
        [TestCase("", "AEinstein", "AE")]
        [TestCase("1-pass@noreply.com", "1-pass", "1P")]
        [TestCase("", "1-pass", "1P")]
        [TestCase("1-pass@noreply.com", "", "1P")]
        [TestCase("/*+=/°¨", "'µ*", "'")] // Fallback when no letters or digit found
        [TestCase("/*+=/°¨", "'µ *", "'*")] // Fallback when no letters or digit found
        [TestCase("", "", "?")]
        [TestCase(null, null, "?")]
        public void GetInitialsAndHashCode_return_initials_of_a_user(string email, string name, string expected)
        {
            (string initials, int _) = new InitialsAvatarProvider().GetInitialsAndColorIndex(email, name);

            Assert.AreEqual(expected, initials);
        }
    }
}
