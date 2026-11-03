using System.Reflection;
using GitCommands;
using GitExtensions.Extensibility.Translations;

namespace GitExtensionsTests;

internal sealed class TranslationTestScope : IDisposable
{
    private static readonly FieldInfo CurrentTranslation = typeof(AppSettings).GetField("_currentTranslation", BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly FieldInfo TranslationCache = typeof(Translator).GetField("_translation", BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly FieldInfo TranslationName = typeof(Translator).GetField("_name", BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly FieldInfo SharedUiStrings = typeof(GitUI.CommandsDialogs.FormBrowse).Assembly
        .GetType("GitUI.TranslatedStrings", throwOnError: true)!.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!;
    private static readonly FieldInfo SharedResourceStrings = typeof(ResourceManager.TranslatedStrings)
        .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)!;
    private readonly object? _currentTranslation = CurrentTranslation.GetValue(null);
    private readonly object? _translationCache = TranslationCache.GetValue(null);
    private readonly object? _translationName = TranslationName.GetValue(null);
    private readonly object? _sharedUiStrings = SharedUiStrings.GetValue(null);
    private readonly object? _sharedResourceStrings = SharedResourceStrings.GetValue(null);

    public TranslationTestScope()
    {
        // A fresh lazy also keeps an uninitialized caller cache from retaining this scope's language.
        SharedUiStrings.SetValue(null, Activator.CreateInstance(SharedUiStrings.FieldType));
        SharedResourceStrings.SetValue(null, Activator.CreateInstance(SharedResourceStrings.FieldType));
    }

    public TranslationTestScope(string name)
        : this()
    {
        AppSettings.CurrentTranslation = name;
    }

    public void Dispose()
    {
        CurrentTranslation.SetValue(null, _currentTranslation);
        TranslationCache.SetValue(null, _translationCache);
        TranslationName.SetValue(null, _translationName);
        SharedUiStrings.SetValue(null, _sharedUiStrings);
        SharedResourceStrings.SetValue(null, _sharedResourceStrings);
    }
}
