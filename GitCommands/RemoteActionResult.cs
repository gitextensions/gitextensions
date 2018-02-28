using GitUIPluginInterfaces;

namespace GitCommands
{
    public class RemoteActionResult<T>
    {
        public bool HostKeyFail { get; set; }
        public bool AuthenticationFail { get; set; }
        public T Result { get; set; }
        public CmdResult CmdResult;
    }
}
