using System.Reflection;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitExtensions.Extensibility.Translations.Xliff;

namespace GitExtensionsTests;

[TestFixture]
public sealed class TranslationTestScopeTests
{
    [Test]
    public void Scope_should_restore_an_unset_override_and_the_cached_translation()
    {
        using TranslationTestScope originalState = new();
        FieldInfo currentTranslation = typeof(AppSettings).GetField("_currentTranslation", BindingFlags.Static | BindingFlags.NonPublic)!;
        string originalSetting = AppSettings.Translation;
        try
        {
            currentTranslation.SetValue(null, null);
            AppSettings.Translation = "English";
            IDictionary<string, TranslationFile> originalCache = Translator.GetTranslation("English");
            using (TranslationTestScope scope = new("German"))
            {
                Translator.GetTranslation(AppSettings.CurrentTranslation).Should().NotBeSameAs(originalCache);
            }

            currentTranslation.GetValue(null).Should().BeNull();
            AppSettings.CurrentTranslation.Should().Be("English");
            Translator.GetTranslation("English").Should().BeSameAs(originalCache);
        }
        finally
        {
            AppSettings.Translation = originalSetting;
        }
    }
}
