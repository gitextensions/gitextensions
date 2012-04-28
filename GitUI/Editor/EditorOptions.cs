﻿namespace GitUI.Editor
{
    public class EditorOptions
    {
        public static void SetSyntax(IFileViewer editor, string fileName)
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
                    case "JS":
                        syntax = "JavaScript";
                        break;
                    default:
                        break;
                }
            }
            editor.SetHighlighting(syntax);
        }
    }
}
