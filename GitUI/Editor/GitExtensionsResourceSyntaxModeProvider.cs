using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor
{
    class GitExtensionsResourceSyntaxModeProvider : ISyntaxModeFileProvider
    {
        readonly ICollection<SyntaxMode> _syntaxModes;

        public GitExtensionsResourceSyntaxModeProvider()
        {
            // read syntax modes
            _syntaxModes = typeof(GitExtensionsResourceSyntaxModeProvider).Assembly
                .GetManifestResourceNames()
                .Where(n => n.ToUpperInvariant().EndsWith("-SYNTAXMODE.XSHD"))
                .Select(path => new { Path = path, Name = Regex.Replace(path, ".*?([^.]+)-syntaxmode.xshd$", "$1", RegexOptions.IgnoreCase) })
                .Select(a => new SyntaxMode(a.Path, a.Name, String.Empty))
                .ToArray();
        }

        public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
        {
            return new XmlTextReader(typeof(GitExtensionsResourceSyntaxModeProvider).Assembly.GetManifestResourceStream(syntaxMode.FileName));
        }

        public ICollection<SyntaxMode> SyntaxModes
        {
            get { return _syntaxModes; }
        }

        public void UpdateSyntaxModeList()
        {
            // resources don't change during runtime
        }
    }
}
