using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Compat;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.Menus;

internal abstract class ToolStripMenuItemEx : MenuItem, ITranslate
{
    private Func<IGitUICommands>? _getUICommands;

    /// <summary>
    ///  Gets the current instance of the UI commands.
    /// </summary>
    protected IGitUICommands UICommands
        => (_getUICommands ?? throw new InvalidOperationException("The button is not initialized"))();

    /// <summary>
    ///  Gets the current instance of the git module.
    /// </summary>
    protected IGitModule Module => UICommands.Module;

    protected bool HasUICommands => _getUICommands is not null;

    /// <summary>
    ///  Gets the form that is displaying the menu item.
    /// </summary>
    // Avalonia resolves the owning top level from this menu instance instead of a process-global active form.
    protected WinFormsShims.IWin32Window? OwnerForm => TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window;

    protected Window? OwnerWindow => TopLevel.GetTopLevel(this) as Window;

    /// <summary>
    ///  Initializes the menu item.
    /// </summary>
    /// <param name="getUICommands">The method that returns the current instance of UI commands.</param>
    public void Initialize(Func<IGitUICommands> getUICommands)
    {
        Translator.Translate(this, AppSettings.CurrentTranslation);

        _getUICommands = getUICommands;

        OnInitialized();
    }

    /// <summary>
    ///  Allows the menu item to perform any initialization logic.
    /// </summary>
    public new virtual void OnInitialized()
    {
    }

    /// <summary>
    ///  Allows reloading/reassigning the configured shortcut key.
    /// </summary>
    //// <param name="hotkeys"></param>
    public virtual void RefreshShortcutKeys(IEnumerable<HotkeyCommand>? hotkeys)
    {
    }

    /// <summary>
    ///  Allows refreshing the state of the menu item depending on the state of the loaded git repository.
    /// </summary>
    /// <param name="bareRepository"><see langword="true"/> if the current git repository is bare; otherwise, <see langword="false"/>.</param>
    public virtual void RefreshState(bool bareRepository)
    {
    }

    internal void AddControlTranslationItems(ITranslation translation)
    {
        AvaloniaTranslationUtils.AddTranslationItemsFromFields("FormBrowse", this, translation);
    }

    internal void TranslateControlItems(ITranslation translation)
    {
        AvaloniaTranslationUtils.TranslateItemsFromFields("FormBrowse", this, translation);
    }

    void ITranslate.AddTranslationItems(ITranslation translation)
        => AddControlTranslationItems(translation);

    void ITranslate.TranslateItems(ITranslation translation)
        => TranslateControlItems(translation);

    void IDisposable.Dispose()
    {
    }
}
