using System;

namespace GitUI.UserManual
{
    public class SingleHtmlUserManual : IProvideUserManual
    {
        private readonly string _location = @"file:///D:/data2/projects/gitextensions/GitExtensionsDoc/build/singlehtml";
        private readonly string _anchorName;

        public SingleHtmlUserManual(string anchorName)
        {
            _anchorName = anchorName;
        }

        public string GetUrl()
        {
            return string.Format("{0}/index.html{1}{2}",
                                 _location, _anchorName.IsNullOrEmpty() ? "" : "#", _anchorName);
        }
    }
}