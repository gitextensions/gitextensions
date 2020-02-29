using System;
using System.Threading.Tasks;
using GitUI.Avatars;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    [TestFixture]
    public sealed class InitialsAvatarGeneratorTests
    {
        [Test]
        public void GetInitialsAndHashCode_return_initials_of_a_user()
        {
            var (initials, _) = new InitialsAvatarGenerator().GetInitialsAndHashCode("albert.einstein@noreply.com", "albert einstein");

            Assert.AreEqual("AE", initials);
        }

        [Test]
        public void GetInitialsAndHashCode_return_the_initial_of_a_user_based_on_its_name()
        {
            var (initials, _) = new InitialsAvatarGenerator().GetInitialsAndHashCode("albert.einstein@noreply.com", "albert");

            Assert.AreEqual("A", initials);
        }

        [Test]
        public void GetInitialsAndHashCode_return_the_initial_of_a_user_based_on_its_email()
        {
            var (initials, _) = new InitialsAvatarGenerator().GetInitialsAndHashCode("albert.einstein@noreply.com", null);

            Assert.AreEqual("A", initials);
        }

        [Test]
        public void GetInitialsAndHashCode_return_question_mark_when_no_data_provided()
        {
            var (initials, _) = new InitialsAvatarGenerator().GetInitialsAndHashCode(null, null);

            Assert.AreEqual("?", initials);
        }
    }
}