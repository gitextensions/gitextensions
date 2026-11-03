using System.Reflection;
using GitCommands;
using GitExtensions.Extensibility.Translations;

namespace GitExtensionsTests;

internal sealed class TranslationTestScope : IDisposable
{
    private static readonly FieldInfo CurrentTranslation = typeof(AppSettings).GetField("_currentTranslation", BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly FieldInfo TranslationCache = typeof(Translator).GetField("_translation", BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly FieldInfo TranslationName = typeof(Translator).GetField("_name", BindingFlags.Static | BindingFlags.NonPublic)!;
    private readonly object? _currentTranslation = CurrentTranslation.GetValue(null);
    private readonly object? _translationCache = TranslationCache.GetValue(null);
    private readonly object? _translationName = TranslationName.GetValue(null);

    public TranslationTestScope()
    {
    }

    public TranslationTestScope(string name)
    {
        AppSettings.CurrentTranslation = name;
    }

    public void Dispose()
    {
        CurrentTranslation.SetValue(null, _currentTranslation);
        TranslationCache.SetValue(null, _translationCache);
        TranslationName.SetValue(null, _translationName);
    }
}
