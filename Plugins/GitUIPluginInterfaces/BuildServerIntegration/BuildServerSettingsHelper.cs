using System;
using System.ComponentModel.Composition;
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
            catch (Exception)
            {
                return false;
            }
        }
    }
}