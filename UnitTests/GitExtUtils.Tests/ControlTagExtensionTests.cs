using System.Windows.Forms;
using GitExtUtils.GitUI;
using NUnit.Framework;

namespace GitExtUtilsTests
{
    [TestFixture]
    public class ControlTagExtensionTests
    {
        [Test]
        public void Multiple_values_can_be_set()
        {
            var control = new Control();

            control.SetTag(1);
            control.SetTag(2f);

            Assert.That(control.HasTag<int>(), Is.True);
            Assert.That(control.GetTag<int>(), Is.EqualTo(1));
            Assert.That(control.HasTag<float>(), Is.True);
            Assert.That(control.GetTag<float>(), Is.EqualTo(2f));
        }

        [Test]
        public void When_tag_field_is_occupied_GetTag_returns_default()
        {
            var control = new Control();
            control.Tag = new object();

            Assert.That(control.HasTag<int>(), Is.False);
            Assert.That(control.GetTag<int>(), Is.EqualTo(default(int)));
        }

        [Test]
        public void When_tag_field_is_occupied_SetTag_replaces_existing_tag()
        {
            var control = new Control();
            control.Tag = new object();

            control.SetTag(1);

            Assert.That(control.HasTag<int>(), Is.True);
            Assert.That(control.GetTag<int>(), Is.EqualTo(1));
        }

        [Test]
        public void When_another_type_is_stored_at_key_GetTag_return_default()
        {
            var control = new Control();
            control.SetTag("key", 1);
            control.SetTag("key", 2f);

            Assert.That(control.HasTag<int>("key"), Is.False);
            Assert.That(control.GetTag<int>("key"), Is.EqualTo(default(int)));

            Assert.That(control.HasTag<float>("key"), Is.True);
            Assert.That(control.GetTag<float>("key"), Is.EqualTo(2f));
        }
    }
}
