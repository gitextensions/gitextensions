namespace GitExtUtils.GitUI.Theming
{
    /// <summary>
    /// GitExtensions' application specific color names
    /// </summary>
    /// <remarks>
    /// Values are stored in AppSettings class. Whenever new name is added here, add default value
    /// to <see cref="AppColorDefaults"/> and \GitUI\Themes\win10default.css
    /// </remarks>
    public enum AppColor
    {
        OtherTag,
        AuthoredHighlight,
        HighlightAllOccurences,
        Tag,
        Graph,
        Branch,
        RemoteBranch,
        DiffSection,
        DiffRemoved,
        DiffRemovedExtra,
        DiffAdded,
        DiffAddedExtra,
    }
}
