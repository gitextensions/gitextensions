using System;
using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming
{
    public interface IThemeCssUrlResolver
    {
        string ResolveCssUrl(string url);
    }

    public class ThemeCssUrlResolver : IThemeCssUrlResolver
    {
        private const string CssVariableUserThemesDirectory = "{UserAppData}/";
        private readonly IThemePathProvider _themePathProvider;

        public ThemeCssUrlResolver(IThemePathProvider themePathProvider)
        {
            _themePathProvider = themePathProvider;
        }

        public string ResolveCssUrl(string url)
        {
            if (url.EndsWith(_themePathProvider.ThemeExtension, StringComparison.OrdinalIgnoreCase))
            {
                url = url.Substring(0, url.Length - _themePathProvider.ThemeExtension.Length);
            }

            ThemeId id = url.StartsWith(CssVariableUserThemesDirectory)
                ? new ThemeId(url.Substring(CssVariableUserThemesDirectory.Length), isBuiltin: false)
                : new ThemeId(url, isBuiltin: true);

            try
            {
                return _themePathProvider.GetThemePath(id);
            }
            catch (Exception ex)
            {
                throw new ThemeCssUrlResolverException(ex.Message, ex);
            }
        }
    }
}
