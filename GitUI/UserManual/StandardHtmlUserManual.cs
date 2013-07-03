using System;

namespace GitUI.UserManual
{
    public class StandardHtmlUserManual : IProvideUserManual
    {
        private readonly string _location = @"https://git-extensions-documentation.readthedocs.org/en/latest";
        private readonly string _subFolder;
        private readonly string _anchorName;

        public StandardHtmlUserManual(string subFolder, string anchorName)
        {
            _subFolder = subFolder;
            _anchorName = anchorName;
        }

        public string GetUrl()
        {
            return string.Format("{0}/{1}.html{2}{3}",
                                 _location, _subFolder, _anchorName.IsNullOrEmpty() ? "" : "#", _anchorName);
        }
    }
}