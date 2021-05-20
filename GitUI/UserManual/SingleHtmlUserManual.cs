using System;
using System.IO;
using GitCommands;

namespace GitUI.UserManual
{
    public class SingleHtmlUserManual : IProvideUserManual
    {
        private static string? _location;

        public static string Location
        {
            get
            {
                if (_location is null)
                {
                    var path = Path.Combine(AppSettings.GetInstallDir(), "help");
                    Uri uri = new(path);
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
                                 Location, string.IsNullOrEmpty(_anchorName) ? "" : "#", _anchorName);
        }
    }
}
