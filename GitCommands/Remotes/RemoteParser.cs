using System.Text.RegularExpressions;

namespace GitCommands.Remotes
{
    public abstract class RemoteParser
    {
        protected Match MatchRegExes(string remoteUrl, string[] regExs)
        {
            Match m = null;
            foreach (var regex in regExs)
            {
                m = Regex.Match(remoteUrl, regex);
                if (m.Success)
                {
                    break;
                }
            }

            return m;
        }
    }
}