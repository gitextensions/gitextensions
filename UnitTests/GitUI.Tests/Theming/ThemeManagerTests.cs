using System.Collections.Generic;
using System.Drawing;
using FluentAssertions;
using GitCommands;
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

            themeManager.UseInitialTheme.Should().BeTrue();

            themeManager.SetInitialTheme(CreateTheme(InitialColor));

            themeManager.UseInitialTheme.Should().BeTrue();
            themeManager.IsCurrentThemeInitial.Should().BeTrue();
        }

        [Test]
        public void When_using_initial_theme_SetColor_does_not_affect_result()
        {
            var themeManager = new ThemeManager(DefaultTheme);
            themeManager.SetInitialTheme(CreateTheme(InitialColor));

            themeManager.SetColor(AppColor, ModifiedColor);
            themeManager.SetColor(SysColor, ModifiedColor);
            themeManager.IsCurrentThemeInitial.Should().BeTrue();
            themeManager.IsCurrentThemeModified.Should().BeTrue();

            // repeats to make sure UseInitialTheme property does not exhibit any sticky behavior
            int repeats = 3;
            for (int i = 0; i < repeats; i++)
            {
                themeManager.UseInitialTheme = true;
                themeManager.GetColor(AppColor).Should().Be(InitialColor);
                themeManager.GetColor(SysColor).Should().Be(InitialColor);

                themeManager.UseInitialTheme = false;
                themeManager.GetColor(AppColor).Should().Be(ModifiedColor);
                themeManager.GetColor(SysColor).Should().Be(ModifiedColor);
            }
        }

        [Test]
        public void When_using_initial_theme_SetTheme_does_not_affect_result()
        {
            var themeManager = new ThemeManager(DefaultTheme);
            themeManager.SetInitialTheme(CreateTheme(InitialColor));

            themeManager.SetTheme(CreateTheme(ModifiedColor));
            themeManager.IsCurrentThemeInitial.Should().BeFalse();
            themeManager.IsCurrentThemeModified.Should().BeFalse();

            // repeats to make sure UseInitialTheme property does not exhibit any sticky behavior
            int repeats = 3;
            for (int i = 0; i < repeats; i++)
            {
                themeManager.UseInitialTheme = true;
                themeManager.GetColor(AppColor).Should().Be(InitialColor);
                themeManager.GetColor(SysColor).Should().Be(InitialColor);

                themeManager.UseInitialTheme = false;
                themeManager.GetColor(AppColor).Should().Be(ModifiedColor);
                themeManager.GetColor(SysColor).Should().Be(ModifiedColor);
            }
        }

        [Test]
        public void ResetColor_reverts_to_current_theme()
        {
            var themeManager = new ThemeManager(DefaultTheme);

            themeManager.SetInitialTheme(CreateTheme(InitialColor));
            themeManager.SetTheme(CreateTheme(CurrentThemeColor));
            themeManager.SetColor(AppColor, ModifiedColor);
            themeManager.SetColor(SysColor, ModifiedColor);
            themeManager.UseInitialTheme = false;

            themeManager.IsCurrentThemeInitial.Should().BeFalse();
            themeManager.IsCurrentThemeModified.Should().BeTrue();

            themeManager.GetColor(AppColor).Should().Be(ModifiedColor);
            themeManager.GetColor(SysColor).Should().Be(ModifiedColor);

            themeManager.ResetColor(AppColor);
            themeManager.ResetColor(SysColor);

            themeManager.GetColor(AppColor).Should().Be(CurrentThemeColor);
            themeManager.GetColor(SysColor).Should().Be(CurrentThemeColor);
        }

        [Test]
        public void ResetAllColors_reverts_to_current_theme()
        {
            var themeManager = new ThemeManager(DefaultTheme);

            themeManager.SetInitialTheme(CreateTheme(InitialColor));
            themeManager.SetTheme(CreateTheme(CurrentThemeColor));
            themeManager.SetColor(AppColor, ModifiedColor);
            themeManager.SetColor(SysColor, ModifiedColor);
            themeManager.UseInitialTheme = false;

            themeManager.IsCurrentThemeInitial.Should().BeFalse();
            themeManager.IsCurrentThemeModified.Should().BeTrue();

            themeManager.GetColor(AppColor).Should().Be(ModifiedColor);
            themeManager.GetColor(SysColor).Should().Be(ModifiedColor);

            themeManager.ResetAllColors();

            themeManager.GetColor(AppColor).Should().Be(CurrentThemeColor);
            themeManager.GetColor(SysColor).Should().Be(CurrentThemeColor);
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

            themeManager.CurrentTheme.Should().BeNull();

            themeManager.UseInitialTheme = true;

            themeManager.GetColor(AppColor).Should().Be(InitialColor);
            themeManager.GetColor(SysColor).Should().Be(InitialColor);

            themeManager.UseInitialTheme = false;

            themeManager.GetColor(AppColor).Should().Be(DefaultTheme.GetColor(AppColor));
            themeManager.GetColor(SysColor).Should().Be(DefaultTheme.GetColor(SysColor));
        }

        private static ReadOnlyTheme CreateTheme(Color color) =>
            new ReadOnlyTheme(
                new Dictionary<AppColor, Color> { [AppColor] = color },
                new Dictionary<KnownColor, Color> { [SysColor] = color });
    }
}
