using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gerrit.Server;
using NUnit.Framework;

namespace GerritTests.Server
{
    public static class CommandBuilderWithPrivateSupportTests
    {
        [TestCase("", ExpectedResult = "refs/for/mybranch")]
        [TestCase("wip", ExpectedResult = "refs/for/mybranch%wip")]
        [TestCase("private", ExpectedResult = "refs/for/mybranch%private")]
        public static string Build_given_a_publishType_builds_expected_command(string publishType)
        {
            var sut = new CommandBuilderWithPrivateSupport();

            return sut.WithPublishType(publishType).Build("mybranch");
        }

        [Test]
        public static void Build_given_a_set_of_values_builds_expected_command()
        {
            var sut = new CommandBuilderWithPrivateSupport();

            Assert.AreEqual("refs/for/master%r=myteam",
                sut.WithReviewers("myteam").WithPublishType(string.Empty).Build("master"));
        }
    }
}
