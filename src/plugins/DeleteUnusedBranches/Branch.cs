namespace GitExtensions.Plugins.DeleteUnusedBranches;

public sealed class Branch
{
    public Branch(string name, DateTime date, string author, string message, bool delete, bool isSessionRef = false)
    {
        Name = name;
        Date = date;
        Author = author;
        Message = message;
        Delete = delete;
        IsSessionRef = isSessionRef;
    }

    /// <summary>
    ///  Gets whether this is a ref of an agentic session (see <see cref="GitCommands.AppSettings.SessionRefsFilters"/>), named by its full refname.
    /// </summary>
    public bool IsSessionRef { get; }

    public string Name { get; }
    public DateTime Date { get; }
    public bool Delete { get; set; }
    public string Author { get; }
    public string Message { get; }
}
