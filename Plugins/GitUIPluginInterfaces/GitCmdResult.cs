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
            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(StdOutput))
            {
                sb.Append(StdOutput);
            }

            if (!string.IsNullOrEmpty(StdError) && !string.IsNullOrEmpty(StdOutput))
            {
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(StdError))
            {
                sb.Append(StdError);
            }

            return sb.ToString();
        }
    }
}
