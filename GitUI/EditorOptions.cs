using System;
using System.Collections.Generic;

using System.Text;

namespace GitUI
{
    public class EditorOptions
    {
        public static void SetSyntax(ICSharpCode.TextEditor.TextEditorControl editor, string fileName)
        {
            string syntax = "XML";
            if (fileName.LastIndexOf('.') > 0)
            {
                string extension = fileName.Substring(fileName.LastIndexOf('.') + 1).ToUpper();

                switch (extension)
                {
                    case "BAS":
                    case "VBS":
                    case "VB":
                        syntax = "VBNET";
                        break;
                    case "CS":
                        syntax = "C#";
                        break;
                    case "CMD":
                    case "BAT":
                        syntax = "BAT";
                        break;
                    case "C":
                    case "RC":
                    case "IDL":
                    case "H":
                    case "CPP":
                        syntax = "C#";
                        break;
                    default:
                        break;
                }
            }
            editor.SetHighlighting(syntax);
        }
    }
}
