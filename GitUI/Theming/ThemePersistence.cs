﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    public interface IThemePersistence
    {
        Theme Load(string themeFileName, ThemeId id, IReadOnlyList<string> variations);
        void Save(Theme theme, string themeFileName);
    }

    public class ThemePersistence : IThemePersistence
    {
        private const string Format = ".{0} {{ color: {1}}}";
        private readonly IThemeLoader _themeLoader;

        public ThemePersistence(IThemeLoader themeLoader)
        {
            _themeLoader = themeLoader;
        }

        public Theme Load(string themeFileName, ThemeId themeId, IReadOnlyList<string> variations)
        {
            return _themeLoader.LoadTheme(themeFileName, themeId, allowedClasses: variations);
        }

        public void Save(Theme theme, string themeFileName)
        {
            string serialized = string.Join(
                Environment.NewLine,
                theme.SysColorValues.Select(_ => string.Format(Format, _.Key, FormatColor(_.Value))).Concat(
                    theme.AppColorValues.Select(_ => string.Format(Format, _.Key, FormatColor(_.Value)))));

            File.WriteAllText(themeFileName, serialized);
        }

        private static string FormatColor(Color color)
        {
            int rgb = color.ToArgb() & 0x00_ff_ff_ff;
            return $"#{rgb:x6}";
        }

        internal static class TestAccessor
        {
            public static string FormatColor(Color color) =>
                ThemePersistence.FormatColor(color);
        }
    }
}
