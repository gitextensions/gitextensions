namespace GitCommands.UserRepositoryHistory.Legacy
{
    /// <summary>
    /// This type is necessary to provide backwards compatibility ONLY.
    /// </summary>
    /// <remarks>
    /// This DTO is deserialised from user settings, the order of the properties is significant.
    /// </remarks>
    public class Repository
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public string Anchor { get; set; }
    }
}