using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;

namespace ResourceManager;

// Provides XLF translation for Avalonia controls. Translation category and item keys remain
// the WinForms type and field names, so the existing language files apply unchanged.
public class TranslatedControl : UserControl, ITranslate
{
    /// <summary>Performs post-initialisation tasks such as translation.</summary>
    /// <remarks>Subclasses must ensure this method is called in their constructor, ideally as the final statement.</remarks>
    protected void InitializeComplete()
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);
        AvaloniaTranslationUtils.RemoveTextBlockMnemonicMarkers(this);
        InputAccessibility.Apply(this);
    }

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public virtual void AddTranslationItems(ITranslation translation)
    {
        AvaloniaTranslationUtils.AddTranslationItemsFromFields(GetType().Name, this, translation);
    }

    public virtual void TranslateItems(ITranslation translation)
    {
        AvaloniaTranslationUtils.TranslateItemsFromFields(GetType().Name, this, translation);
    }
}
