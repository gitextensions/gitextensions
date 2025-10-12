namespace GitCommands.Services
{
    /// <summary>
    /// Define a service to show message boxes.
    /// Service declaration must not have direct dependencies on System.Windows.Forms.
    /// Think about testability and separation of concerns.
    /// </summary>
    public interface IMessageBoxService
    {
        public Task ShowInfoMessageAsync(string? text, string? caption);
    }
}
