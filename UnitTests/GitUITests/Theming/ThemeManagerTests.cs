using System;
using System.Collections.Generic;
using System.Drawing;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using NUnit.Framework;

namespace GitUITests.Theming
{
    [TestFixture]
    public class ThemeManagerTests
    {
        private static readonly Color InitialColor = Color.Aqua;
        private static readonly Color CurrentThemeColor = Color.Beige;
        private static readonly Color ModifiedColor = Color.Crimson;

        private const AppColor AppColor = GitExtUtils.GitUI.Theming.AppColor.Branch;
        private const KnownColor SysColor = KnownColor.Control;

        private static readonly DefaultTheme DefaultTheme = new DefaultTheme();

        [Test]
        public void Uses_initial_theme_by_default()
        {
            var themeManager = new ThemeManager(DefaultTheme);

            Assert.That(themeManager.UseInitialTheme, Is.True);

            themeManager.SetInitialTheme(CreateTheme(InitialColor));

            Assert.That(themeManager.UseInitialTheme, Is.True);
            Assert.That(themeManager.IsCurrentThemeInitial(), Is.True);
        }

        [TestCase(ThemeEditVariant.SelectAnotherTheme)]
        [TestCase(ThemeEditVariant.ModifyCurrentTheme)]
        public void When_using_initial_theme_returns_color_from_initial_theme(ThemeEditVariant editVariant)
        {
            var themeManager = new ThemeManager(DefaultTheme);
            themeManager.SetInitialTheme(CreateTheme(InitialColor));

            switch (editVariant)
            {
                case ThemeEditVariant.SelectAnotherTheme:
                    themeManager.SetTheme(CreateTheme(ModifiedColor));
                    Assert.That(themeManager.IsCurrentThemeInitial(), Is.False);
                    Assert.That(themeManager.IsCurrentThemeModified(), Is.False);
                    break;
                case ThemeEditVariant.ModifyCurrentTheme:
                    themeManager.SetColor(AppColor, ModifiedColor);
                    themeManager.SetColor(SysColor, ModifiedColor);
                    Assert.That(themeManager.IsCurrentThemeInitial(), Is.True);
                    Assert.That(themeManager.IsCurrentThemeModified(), Is.True);
                    break;
                default:
                    throw new NotSupportedException();
            }

            // repeats to make sure UseInitialTheme property does not exhibit any sticky behavior
            int repeats = 3;
            for (int i = 0; i < repeats; i++)
            {
                themeManager.UseInitialTheme = true;
                Assert.That(themeManager.GetColor(AppColor), Is.EqualTo(InitialColor));
                Assert.That(themeManager.GetColor(SysColor), Is.EqualTo(InitialColor));

                themeManager.UseInitialTheme = false;
                Assert.That(themeManager.GetColor(AppColor), Is.EqualTo(ModifiedColor));
                Assert.That(themeManager.GetColor(SysColor), Is.EqualTo(ModifiedColor));
            }
        }

        [TestCase(ResetColorsVariant.ResetAllColors)]
        [TestCase(ResetColorsVariant.ResetIndividualColor)]
        public void ResetColor_reverts_to_current_theme(ResetColorsVariant resetVariant)
        {
            var themeManager = new ThemeManager(DefaultTheme);

            themeManager.SetInitialTheme(CreateTheme(InitialColor));
            themeManager.SetTheme(CreateTheme(CurrentThemeColor));
            themeManager.SetColor(AppColor, ModifiedColor);
            themeManager.SetColor(SysColor, ModifiedColor);
            themeManager.UseInitialTheme = false;

            Assert.That(themeManager.IsCurrentThemeInitial(), Is.False);
            Assert.That(themeManager.IsCurrentThemeModified(), Is.True);

            Assert.That(themeManager.GetColor(AppColor), Is.EqualTo(ModifiedColor));
            Assert.That(themeManager.GetColor(SysColor), Is.EqualTo(ModifiedColor));

            switch (resetVariant)
            {
                case ResetColorsVariant.ResetIndividualColor:
                    themeManager.ResetColor(AppColor);
                    themeManager.ResetColor(SysColor);
                    break;
                case ResetColorsVariant.ResetAllColors:
                    themeManager.ResetAllColors();
                    break;
                default:
                    throw new NotSupportedException();
            }

            Assert.That(themeManager.GetColor(AppColor), Is.EqualTo(CurrentThemeColor));
            Assert.That(themeManager.GetColor(SysColor), Is.EqualTo(CurrentThemeColor));
        }

        [Test]
        public void ResetTheme_resets_colors_to_default_theme()
        {
            var themeManager = new ThemeManager(DefaultTheme);

            themeManager.SetInitialTheme(CreateTheme(InitialColor));
            themeManager.SetTheme(CreateTheme(CurrentThemeColor));
            themeManager.SetColor(AppColor, ModifiedColor);
            themeManager.SetColor(SysColor, ModifiedColor);

            themeManager.ResetTheme();

            Assert.That(themeManager.CurrentTheme == null);

            themeManager.UseInitialTheme = true;

            Assert.That(themeManager.GetColor(AppColor), Is.EqualTo(InitialColor));
            Assert.That(themeManager.GetColor(SysColor), Is.EqualTo(InitialColor));

            themeManager.UseInitialTheme = false;

            Assert.That(themeManager.GetColor(AppColor), Is.EqualTo(DefaultTheme.GetColor(AppColor)));
            Assert.That(themeManager.GetColor(SysColor), Is.EqualTo(DefaultTheme.GetColor(SysColor)));
        }

        private static StaticTheme CreateTheme(Color color) =>
            new StaticTheme(
                new Dictionary<AppColor, Color> { [AppColor] = color },
                new Dictionary<KnownColor, Color> { [SysColor] = color });

        public enum ThemeEditVariant
        {
            SelectAnotherTheme,
            ModifyCurrentTheme
        }

        public enum ResetColorsVariant
        {
            ResetIndividualColor,
            ResetAllColors
        }
    }
}
