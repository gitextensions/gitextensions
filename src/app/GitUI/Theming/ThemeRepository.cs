using GitExtUtils.GitUI.Theming;

namespace GitUI.Theming;

public interface IThemeRepository
{
    Theme GetTheme(ThemeId themeId, IReadOnlyList<string> variations);

    void Save(Theme theme);

    Theme GetInvariantTheme();

    IEnumerable<ThemeId> GetThemeIds();

    void Delete(ThemeId themeId);
}

public class ThemeRepository : IThemeRepository
{
    private readonly IThemePersistence _persistence;
    private readonly IThemePathProvider _themePathProvider;

    public ThemeRepository(IThemePersistence persistence, IThemePathProvider themePathProvider)
    {
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _themePathProvider = themePathProvider ?? throw new ArgumentNullException(nameof(themePathProvider));
    }

    public ThemeRepository()
        : this(new ThemePersistence(new ThemeLoader(new ThemeCssUrlResolver(new ThemePathProvider()), new ThemeFileReader())), new ThemePathProvider())
    {
    }

    public Theme GetTheme(ThemeId themeId, IReadOnlyList<string> variations)
    {
        string themePath = _themePathProvider.GetThemePath(themeId);
        return _persistence.Load(themePath, themeId, variations);
    }

    public void Save(Theme theme)
    {
        Directory.CreateDirectory(_themePathProvider.UserThemesDirectory);
        _persistence.Save(theme, _themePathProvider.GetThemePath(theme.Id));
    }

    public Theme GetInvariantTheme() =>
        GetTheme(ThemeId.DefaultLight, variations: []);

    public IEnumerable<ThemeId> GetThemeIds() =>
        GetBuiltinThemeIds().Concat(GetUserCustomizedThemeIds());

    public void Delete(ThemeId themeId)
    {
        if (themeId.IsBuiltin)
        {
            throw new InvalidOperationException("Only user-defined theme can be deleted");
        }

        string themePath = _themePathProvider.GetThemePath(themeId);
        File.Delete(themePath);
    }

    private IEnumerable<ThemeId> GetBuiltinThemeIds() =>
        new DirectoryInfo(_themePathProvider.AppThemesDirectory)
            .EnumerateFiles("*" + _themePathProvider.ThemeExtension, SearchOption.TopDirectoryOnly)
            .Select(fileInfo => Path.GetFileNameWithoutExtension(fileInfo.Name))
            .Select(fileName => new ThemeId(fileName, isBuiltin: true))
            .OrderBy(id => id.Name);

    private IEnumerable<ThemeId> GetUserCustomizedThemeIds()
    {
        if (_themePathProvider.UserThemesDirectory is null
            || (new DirectoryInfo(_themePathProvider.UserThemesDirectory) is not DirectoryInfo directory)
            || !directory.Exists)
        {
            return [];
        }

        return directory
            .EnumerateFiles("*" + _themePathProvider.ThemeExtension, SearchOption.TopDirectoryOnly)
            .Select(fileInfo => Path.GetFileNameWithoutExtension(fileInfo.Name))
            .Select(fileName => new ThemeId(fileName));
    }
}
