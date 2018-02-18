using System.Text;

namespace GitUIPluginInterfaces
{
    public struct CmdResult
    {
        public string StdOutput;
        public string StdError;
        public int ExitCode;

        public bool ExitedSuccessfully => ExitCode == 0;

        public string GetString()
        {
            var hasOut = !string.IsNullOrEmpty(StdOutput);
            var hasErr = !string.IsNullOrEmpty(StdError);

            var sb = new StringBuilder();
            if (hasOut)
                sb.Append(StdOutput);
            if (hasErr && hasOut)
                sb.AppendLine();
            if (hasErr)
                sb.Append(StdError);

            return sb.ToString();
        }
    }
}
