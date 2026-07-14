namespace GitExtensions.Shims.WinForms;

/// <summary>
///  Stand-in for <c>System.Windows.Forms.DataGridViewColumn</c>: exists so grid columns are
///  recognized by the translation walker.
/// </summary>
/// <remarks>
///  Consumed by: <c>GitExtensions.Extensibility/Translations/Xliff/TranslationUtil.cs</c>
///  (type check; reads <see cref="Visible"/> and translates <see cref="HeaderText"/>).
/// </remarks>
public class DataGridViewColumn
{
    /// <summary>
    ///  Gets or sets the column header text; the property translated by the xlf system.
    /// </summary>
    public string HeaderText { get; set; } = string.Empty;

    /// <summary>
    ///  Gets or sets a value indicating whether the column is visible; hidden columns are not translated.
    /// </summary>
    public bool Visible { get; set; } = true;
}
