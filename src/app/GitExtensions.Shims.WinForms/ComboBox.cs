namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.ComboBox</c>: a headless model of a choice setting
///  control; the Avalonia settings renderer materializes a real control from it.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Settings/ChoiceSetting.cs</c>,
///  <c>GitExtensions.Extensibility/Translations/Xliff/TranslationUtil.cs</c> (type check).
/// </remarks>
public class ComboBox : Control
{
    /// <summary>
    ///  Gets the item list; translated in place by the xlf system.
    /// </summary>
    public IList<object> Items { get; } = [];

    /// <summary>
    ///  Gets or sets the index of the selected item, or <c>-1</c> for no selection.
    /// </summary>
    public int SelectedIndex { get; set; } = -1;
}
