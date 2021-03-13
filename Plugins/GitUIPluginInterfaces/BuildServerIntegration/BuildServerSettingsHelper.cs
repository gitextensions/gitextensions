using System;
using System.Diagnostics.CodeAnalysis;
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

        public static bool IsUrlValid([NotNullWhen(returnValue: true)] string? url)
        {
            if (url is null)
            {
                return false;
            }

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
