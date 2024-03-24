using System.Text.RegularExpressions;

namespace GitCommands.Remotes
{
    public abstract class RemoteParser
    {
        protected Match? MatchRegExes(string remoteUrl, Regex[] regExs)
        {
            Match? m = null;
            foreach (Regex regex in regExs)
            {
                m = regex.Match(remoteUrl);
                if (m.Success)
                {
                    break;
                }
            }

            return m;
        }
    }
}
