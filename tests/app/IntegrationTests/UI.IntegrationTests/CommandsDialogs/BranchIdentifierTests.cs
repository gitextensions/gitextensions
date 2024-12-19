using FluentAssertions;
using GitExtensions.Extensibility.Git;
using GitUI.LeftPanel;

namespace UITests.CommandsDialogs;

[TestFixture]
public class BranchIdentifierTests
{
    [Test]
    public void Equals_ShouldReturnFalse_WhenObjectIdAndNameAreDifferent()
    {
        BranchIdentifier? branch1 = new(ObjectId.Random(), "branch1");
        BranchIdentifier? branch2 = new(ObjectId.Random(), "branch2");

        // Assert
        branch1.Equals(branch2).Should().BeFalse();
    }

    [Test]
    public void GetHashCode_ShouldReturnSameValue_ForEqualBranches()
    {
        ObjectId? objectId = ObjectId.Random();
        BranchIdentifier? branch1 = new(objectId, "branch");
        BranchIdentifier? branch2 = new(objectId, "branch");

        // Assert
        branch1.GetHashCode().Should().Be(branch2.GetHashCode());
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenBranchIsNull()
    {
        // Assert
        BranchIdentifier.IsValid(null).Should().BeFalse();
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenObjectIdIsDefault()
    {
        BranchIdentifier? branch = new(default, "branch");

        // Assert
        BranchIdentifier.IsValid(branch).Should().BeFalse();
    }

    [Test]
    public void IsValid_ShouldReturnFalse_WhenNameIsNullOrEmpty()
    {
        BranchIdentifier? branch = new(ObjectId.Random(), null);

        // Assert
        BranchIdentifier.IsValid(branch).Should().BeFalse();

        branch = new BranchIdentifier(ObjectId.Random(), string.Empty);

        // Assert
        BranchIdentifier.IsValid(branch).Should().BeFalse();
    }

    [Test]
    public void IsValid_ShouldReturnTrue_WhenObjectIdAndNameAreValid()
    {
        BranchIdentifier? branch = new(ObjectId.Random(), "branch");

        // Assert
        BranchIdentifier.IsValid(branch).Should().BeTrue();
    }
}
