using System;
using System.Text.RegularExpressions;

namespace GitExtensions.Plugins.BuildServer.Core
{
    public static class BuildServerSettingsHelper
    {
        public static bool IsRegexValid(string regexText)
        {
            try
            {
                new Regex(regexText);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsUrlValid(string url)
        {
            try
            {
                _ = new Uri(url);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
