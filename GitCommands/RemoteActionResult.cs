
namespace GitCommands
{
    public class RemoteActionResult<R>
    {
        public bool HostKeyFail { get; set; }
        public bool AuthenticationFail { get; set; }
        public R Result { get; set; }
    }
}
