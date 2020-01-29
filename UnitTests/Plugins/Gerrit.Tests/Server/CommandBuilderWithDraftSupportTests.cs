using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gerrit.Server;
using NUnit.Framework;

namespace GerritTests.Server
{
    public static class CommandBuilderWithDraftSupportTests
    {
        [TestCase("a, b, c", "fix-7521", ExpectedResult = "refs/for/fix-7521%r=a,r=b,r=c")]
        [TestCase("a|b|c", "fix-7521", ExpectedResult = "refs/for/fix-7521%r=a,r=b,r=c")]
        [TestCase("a b c", "fix-7521", ExpectedResult = "refs/for/fix-7521%r=a,r=b,r=c")]
        [TestCase("a; b;c", "fix-7521", ExpectedResult = "refs/for/fix-7521%r=a,r=b,r=c")]
        [TestCase("q", "fix-7521", ExpectedResult = "refs/for/fix-7521%r=q")]
        [TestCase("", "fix-7521", ExpectedResult = "refs/for/fix-7521")]
        [TestCase(null, "fix-7521", ExpectedResult = "refs/for/fix-7521")]
        public static string Build_WithReviewers_splits_reviewrs_and_builds_expected_command(string reviewer, string branch)
        {
            var sut = new CommandBuilderWithDraftSupport();
            return sut.WithReviewers(reviewer).Build(branch);
        }

        [Test(ExpectedResult = "refs/drafts/master%r=a")]
        public static string Build_when_publishtype_is_drafts_builds_expected_command()
        {
            var sut = new CommandBuilderWithDraftSupport();
            return sut.WithReviewers("a").WithPublishType("drafts").Build("master");
        }

        [Test(ExpectedResult = "refs/for/fix-7521")]
        public static string Build_with_all_values_on_default_builds_expected_command()
        {
            var sut = new CommandBuilderWithDraftSupport();
            return sut.WithReviewers(string.Empty)
                .WithCC(string.Empty)
                .WithTopic(string.Empty)
                .WithPublishType(string.Empty)
                .WithHashTag(string.Empty)
                .Build("fix-7521");
        }

        [Test(ExpectedResult = "refs/for/fix-7521%r=mygroup,cc=team2,topic=ABC-123,hashtag=what")]
        public static string Build_with_values_for_all_options_builds_expected_command()
        {
            var sut = new CommandBuilderWithDraftSupport();
            return sut.WithReviewers("mygroup")
                .WithCC("team2")
                .WithTopic("ABC-123")
                .WithPublishType(string.Empty)
                .WithHashTag("what")
                .Build("fix-7521");
        }
    }
}
