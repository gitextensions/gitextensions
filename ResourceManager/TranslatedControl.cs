using System.ComponentModel;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI.Theming;

namespace ResourceManager;

// NOTE do not make this class abstract as it breaks the WinForms designer in VS

/// <summary>
///  Provides translation for Git Extensions <see cref="UserControl"/>s.
/// </summary>
public class TranslatedControl : UserControl, ITranslate
{
    private readonly GitExtensionsControlInitialiser _initialiser;

    protected TranslatedControl()
    {
        _initialiser = new GitExtensionsControlInitialiser(this);
    }

    [Browsable(false)] // because we always read from settings
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Font Font
    {
        get => base.Font;
        set => base.Font = value;
    }

    protected bool IsDesignMode => _initialiser.IsDesignMode;

    protected virtual void OnRuntimeLoad()
    {
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (!IsDesignMode)
        {
            OnRuntimeLoad();
        }
    }

    /// <summary>Performs post-initialisation tasks such as translation.</summary>
    /// <remarks>
    /// <para>Subclasses must ensure this method is called in their constructor, ideally as the final statement.</para>
    /// <para>Requiring this extra life-cycle event allows preparing the UI after any call to <c>InitializeComponent</c>,
    /// but before it is show. The <see cref="UserControl.Load"/> event occurs too late for operations that effect layout.</para>
    /// </remarks>
    protected void InitializeComplete()
    {
        _initialiser.InitializeComplete();

        if (IsDesignMode)
        {
            return;
        }

        this.FixVisualStyle();
    }

    public virtual void AddTranslationItems(ITranslation translation)
    {
        TranslationUtils.AddTranslationItemsFromFields(Name, this, translation);
    }

    public virtual void TranslateItems(ITranslation translation)
    {
        TranslationUtils.TranslateItemsFromFields(Name, this, translation);
    }
}
