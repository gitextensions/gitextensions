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
            StringBuilder sb = new StringBuilder();
            if (StdOutput != null && StdOutput.Length > 0)
            {
                sb.Append(StdOutput);
            }

            if (StdError != null && StdError.Length > 0 && StdOutput != null && StdOutput.Length > 0)
            {
                sb.AppendLine();
            }

            if (StdError != null && StdError.Length > 0)
            {
                sb.Append(StdError);
            }

            return sb.ToString();
        }
    }
}
