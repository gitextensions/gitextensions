namespace GitCommands.Git
{
    /// <summary>Result of a git command.</summary>
    public class GitCommandResult
    {
        /// <summary>Creates a new git command result with the specified outcome.</summary>
        public GitCommandResult(string output, bool wasSuccessful)
        {
            Output = output;
            WasSuccessful = wasSuccessful;
        }

        /// <summary>Indicates whether the git command was successful.</summary>
        public bool WasSuccessful { get; private set; }
        /// <summary>Gets the output text.</summary>
        public string Output { get; private set; }
    }

    //public class GitCommandResult<T> : GitCommandResult
    //{
    //    /// <summary>git command failed and <see cref="Value"/> will be null.</summary>
    //    public GitCommandResult()
    //        : base(false) { }
    //    /// <summary>git command succeeded with the following <see cref="Value"/>.</summary>
    //    public GitCommandResult(T value)
    //        : base(true)
    //    {
    //        Value = value;
    //    }

    //    public T Value { get; private set; }
    //}
}
