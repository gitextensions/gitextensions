using System;
using GitUI.Avatars;
using NUnit.Framework;

namespace GitUITests.Avatars
{
    [TestFixture]
    public class CustomAvatarTemplateFormatter
    {
        [Test]
        [TestCase("Hallo {val1} test {val2}.", "Demo1", "Demo2", "Hallo Demo1 test Demo2.")]
        [TestCase("Hallo {val1} test {val2.", "Demo1", "Demo2", "Hallo Demo1 test ")]
        [TestCase("Hallo {unknown} test {val2}.", "Demo1", "Demo2", "Hallo  test Demo2.")]
        [TestCase("{val1}{val1}{val2}{val1}{val1}", "-", ".", "--.--")]
        [TestCase("{val1} + {val2}", "{val1}", "{val2}", "{val1} + {val2}")]
        public void Given_test_cases_render_as_expected(string template, string val1, string val2, string expectation)
        {
            var formatter = TemplateFormatter.Create(template, ValueMappingProvider);
            var result = formatter((val1, val2));

            Assert.AreEqual(expectation, result);
        }

        [Test]
        public void Formatter_can_be_reused_with_different_inputs()
        {
            var formatter = TemplateFormatter.Create("Example {val1} Template {val2}", ValueMappingProvider);

            var result1 = formatter(("x", "y"));
            var result2 = formatter(("a", "b"));

            Assert.AreEqual("Example x Template y", result1);
            Assert.AreEqual("Example a Template b", result2);
        }

        /// <summary>
        /// Provides mapping for a given name from the input.
        /// In this case a value tuple with two strings to a string.
        /// </summary>
        /// <param name="name">The name of the variable for which the mapper is requested for.</param>
        /// <returns>The mapper for a requested variable.</returns>
        private static Func<(string val1, string val2), string> ValueMappingProvider(string name)
        {
            return name switch
            {
                "val1" => input => input.val1,
                "val2" => input => input.val2,
                _ => input => null,
            };
        }
    }
}
