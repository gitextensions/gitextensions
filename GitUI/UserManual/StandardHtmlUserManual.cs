﻿using System;

namespace GitUI.UserManual
{
    public class StandardHtmlUserManual : IProvideUserManual
    {
        private const string _location = @"https://git-extensions-documentation.readthedocs.org/en/master/";

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

            return (_location + subFolder).Combine("#", _anchorName)!;
        }
    }
}
