using System;
using GitCommands;

namespace GitUI.UserManual
{
    public class StandardHtmlUserManual : IProvideUserManual
    {
        private readonly string _subFolder;
        private readonly string _anchorName;

        public StandardHtmlUserManual(string subFolder, string anchorName)
        {
            _subFolder = subFolder;
            _anchorName = anchorName;
        }

        public string GetUrl()
        {
            var subFolder = string.IsNullOrEmpty(_subFolder) ? string.Empty : _subFolder + ".html";

            return (AppSettings.DocumentationBaseUrl + subFolder).Combine("#", _anchorName)!;
        }
    }
}
