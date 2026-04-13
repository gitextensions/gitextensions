using GitExtUtils.GitUI;

namespace GitExtUtilsTests;
public class ControlTagExtensionTests
{
    [Test]
    public void Multiple_values_can_be_set()
    {
        using Control control = new();

        control.SetTag(1);
        control.SetTag(2f);

        control.HasTag<int>().Should().BeTrue();
        control.GetTag<int>().Should().Be(1);
        control.HasTag<float>().Should().BeTrue();
        control.GetTag<float>().Should().Be(2f);
    }

    [Test]
    public void When_tag_field_is_occupied_GetTag_returns_default()
    {
        using Control control = new()
        {
            Tag = new object()
        };

        control.HasTag<int>().Should().BeFalse();
        control.GetTag<int>().Should().Be(default(int));
    }

    [Test]
    public void When_tag_field_is_occupied_SetTag_replaces_existing_tag()
    {
        using Control control = new()
        {
            Tag = new object()
        };

        control.SetTag(1);

        control.HasTag<int>().Should().BeTrue();
        control.GetTag<int>().Should().Be(1);
    }

    [Test]
    public void When_another_type_is_stored_at_key_GetTag_return_default()
    {
        using Control control = new();
        control.SetTag("key", 1);
        control.SetTag("key", 2f);

        control.HasTag<int>("key").Should().BeFalse();
        control.GetTag<int>("key").Should().Be(default(int));

        control.HasTag<float>("key").Should().BeTrue();
        control.GetTag<float>("key").Should().Be(2f);
    }
}
