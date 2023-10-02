namespace GitUI.CommandsDialogs;

public partial class FormClone
{
    private record RemoteActionResult<TResult>
    {
        public RemoteActionResult(TResult? result, bool authenticationFail, bool hostKeyFail)
        {
            Result = result;
            AuthenticationFail = authenticationFail;
            HostKeyFail = hostKeyFail;
        }

        public TResult? Result { get; }
        public bool AuthenticationFail { get; }
        public bool HostKeyFail { get; }
    }
}
