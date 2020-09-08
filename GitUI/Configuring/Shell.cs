namespace GitUI.Configuring
{
    public interface IShell
    {
        public string Name { get; }

        public string Icon { get; }

        public string Path { get; }

        public string Arguments { get; }

        public bool Default { get; }

        public bool Enabled { get; }
    }

    internal sealed class Shell : IShell
    {
        public string Name { get; set; }

        public string Icon { get; set; }

        public string Path { get; set; }

        public string Arguments { get; set; }

        public bool Default { get; set; }

        public bool Enabled { get; set; }
    }
}
