namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.ListBox</c>: exists so list-typed items are
///  recognized by the translation walker.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Translations/Xliff/TranslationUtil.cs</c> (type check).
/// </remarks>
public class ListBox : Control
{
    /// <summary>
    ///  Gets the item list; translated in place by the xlf system.
    /// </summary>
    public IList<object> Items { get; } = [];
}
