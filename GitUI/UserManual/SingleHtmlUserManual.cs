using System;
using System.IO;
using GitCommands;

namespace GitUI.UserManual
{
    public class SingleHtmlUserManual : IProvideUserManual
    {
        private static string _location;
        public static string Location
        {
            get
            {
                if (_location == null)
                {
                    var path = Path.Combine(AppSettings.GetInstallDir(), "help");
                    var uri = new Uri(path);
                    _location = uri.AbsolutePath;
                }

                return _location;
            }
        }

        private readonly string _anchorName;

        public SingleHtmlUserManual(string anchorName)
        {
            _anchorName = anchorName;
        }

        public string GetUrl()
        {
            return string.Format("{0}/index.html{1}{2}",
                                 Location, _anchorName.IsNullOrEmpty() ? "" : "#", _anchorName);
        }
    }
}