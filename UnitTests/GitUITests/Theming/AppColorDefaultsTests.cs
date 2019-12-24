using System;
using System.Drawing;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using NUnit.Framework;

namespace GitUITests.Theming
{
    [TestFixture]
    public class AppColorDefaultsTests
    {
        [Test]
        public void Default_values_are_defined_in_AppColorDefaults()
        {
            foreach (AppColor name in Enum.GetValues(typeof(AppColor)))
            {
                Color value = AppColorDefaults.GetBy(name);
                Assert.That(value, Is.Not.EqualTo(AppColorDefaults.FallbackColor));
            }
        }

        [Test]
        public void Default_values_are_specified_in_invariant_theme()
        {
            var controller = new FormThemeEditorController(null, new ThemePersistence());
            var invariantTheme = controller.LoadInvariantTheme(quiet: true);
            foreach (AppColor name in Enum.GetValues(typeof(AppColor)))
            {
                Color value = invariantTheme.GetColor(name);
                Assert.That(value, Is.Not.EqualTo(Color.Empty));
            }
        }
    }
}
