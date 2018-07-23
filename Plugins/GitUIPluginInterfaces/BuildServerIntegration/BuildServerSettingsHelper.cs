using System;
using System.Text.RegularExpressions;

namespace GitUIPluginInterfaces.BuildServerIntegration
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
                var uri = new Uri(url);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}