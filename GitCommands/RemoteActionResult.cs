namespace GitCommands
{
    public class RemoteActionResult<TResult>
    {
        public bool HostKeyFail { get; set; }
        public bool AuthenticationFail { get; set; }
        public TResult Result { get; set; }
    }
}
