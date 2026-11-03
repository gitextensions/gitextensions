using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;

namespace ResourceManager;

/// <summary>
///  Provides translation for Git Extensions <see cref="UserControl"/>s.
/// </summary>
public class TranslatedControl : UserControl, ITranslate
{
    protected virtual void OnRuntimeLoad()
    {
    }

    /// <summary>Performs post-initialisation tasks such as translation.</summary>
    /// <remarks>
    /// <para>Subclasses must ensure this method is called in their constructor, ideally as the final statement.</para>
    /// <para>Requiring this extra life-cycle event allows preparing the UI after any call to <c>InitializeComponent</c>,
    /// but before it is show. The WinForms <c>UserControl.Load</c> event occurs too late for operations that effect layout.</para>
    /// </remarks>
    // Provides XLF translation for Avalonia controls. Translation category and item keys remain
    // the WinForms type and field names, so the existing language files apply unchanged.
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
