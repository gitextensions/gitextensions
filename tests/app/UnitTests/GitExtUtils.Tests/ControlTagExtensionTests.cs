using GitExtUtils.GitUI;

namespace GitExtUtilsTests;

[TestFixture]
public class ControlTagExtensionTests
{
    [Test]
    public void Multiple_values_can_be_set()
    {
        Control control = new();

        control.SetTag(1);
        control.SetTag(2f);

        ClassicAssert.That(control.HasTag<int>(), Is.True);
        ClassicAssert.That(control.GetTag<int>(), Is.EqualTo(1));
        ClassicAssert.That(control.HasTag<float>(), Is.True);
        ClassicAssert.That(control.GetTag<float>(), Is.EqualTo(2f));
    }

    [Test]
    public void When_tag_field_is_occupied_GetTag_returns_default()
    {
        Control control = new();
        control.Tag = new object();

        ClassicAssert.That(control.HasTag<int>(), Is.False);
        ClassicAssert.That(control.GetTag<int>(), Is.EqualTo(default(int)));
    }

    [Test]
    public void When_tag_field_is_occupied_SetTag_replaces_existing_tag()
    {
        Control control = new();
        control.Tag = new object();

        control.SetTag(1);

        ClassicAssert.That(control.HasTag<int>(), Is.True);
        ClassicAssert.That(control.GetTag<int>(), Is.EqualTo(1));
    }

    [Test]
    public void When_another_type_is_stored_at_key_GetTag_return_default()
    {
        Control control = new();
        control.SetTag("key", 1);
        control.SetTag("key", 2f);

        ClassicAssert.That(control.HasTag<int>("key"), Is.False);
        ClassicAssert.That(control.GetTag<int>("key"), Is.EqualTo(default(int)));

        ClassicAssert.That(control.HasTag<float>("key"), Is.True);
        ClassicAssert.That(control.GetTag<float>("key"), Is.EqualTo(2f));
    }
}
