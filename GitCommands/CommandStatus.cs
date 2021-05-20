namespace GitCommands
{
    public readonly struct CommandStatus
    {
        public CommandStatus(bool executed, bool needsGridRefresh)
        {
            Executed = executed;
            NeedsGridRefresh = needsGridRefresh;
        }

        public static implicit operator CommandStatus(bool executed) => new(executed, false);

        public bool Executed { get; }

        public bool NeedsGridRefresh { get; }
    }
}
