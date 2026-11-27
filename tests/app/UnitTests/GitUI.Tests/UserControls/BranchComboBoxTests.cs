using GitExtensions.Extensibility.Git;
using GitUI;
using NSubstitute;

namespace GitUITests.UserControls;

[TestFixture]
public class BranchComboBoxTests
{
    private static IGitRef CreateRef(string name, string completeName, bool isHead = false, bool isTag = false, bool isRemote = false)
    {
        IGitRef gitRef = Substitute.For<IGitRef>();
        gitRef.Name.Returns(name);
        gitRef.CompleteName.Returns(completeName);
        gitRef.IsHead.Returns(isHead);
        gitRef.IsTag.Returns(isTag);
        gitRef.IsRemote.Returns(isRemote);
        return gitRef;
    }

    [Test]
    public void GetQualifiedNameIfNumericNonHead_should_return_complete_name_for_numeric_tag()
    {
        IGitRef[] refs = [CreateRef("master", "refs/heads/master", isHead: true), CreateRef("13435", "refs/tags/13435", isTag: true)];

        BranchComboBox.GetQualifiedNameIfNumericNonHead("13435", refs).Should().Be("refs/tags/13435");
        BranchComboBox.GetQualifiedNameIfNumericNonHead(" 13435 ", refs).Should().Be("refs/tags/13435");
    }

    [Test]
    public void GetQualifiedNameIfNumericNonHead_should_keep_numeric_local_branch()
    {
        IGitRef[] refs = [CreateRef("13435", "refs/heads/13435", isHead: true)];

        BranchComboBox.GetQualifiedNameIfNumericNonHead("13435", refs).Should().Be("13435");
    }

    [Test]
    public void GetQualifiedNameIfNumericNonHead_should_prefer_local_branch_over_same_named_tag()
    {
        IGitRef[] refs = [CreateRef("13435", "refs/tags/13435", isTag: true), CreateRef("13435", "refs/heads/13435", isHead: true)];

        BranchComboBox.GetQualifiedNameIfNumericNonHead("13435", refs).Should().Be("13435");
    }

    [TestCase("master")]
    [TestCase("v1.0")]
    [TestCase("origin/13435")]
    [TestCase("")]
    [TestCase("12345 67890")]
    public void GetQualifiedNameIfNumericNonHead_should_not_touch_non_numeric_text(string text)
    {
        IGitRef[] refs = [CreateRef(text, "refs/tags/" + text, isTag: true)];

        BranchComboBox.GetQualifiedNameIfNumericNonHead(text, refs).Should().Be(text);
    }

    [Test]
    public void GetQualifiedNameIfNumericNonHead_should_keep_unknown_numeric_text()
    {
        BranchComboBox.GetQualifiedNameIfNumericNonHead("13435", []).Should().Be("13435");
        BranchComboBox.GetQualifiedNameIfNumericNonHead("13435", null).Should().Be("13435");
    }
}
