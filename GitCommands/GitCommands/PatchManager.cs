using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GitCommands
{
    public class PatchManager
    {
        public ChangedFileManager ChangedFileManager { get; set; }

        public void LoadPatchFile(string diff)
        {
            if (diff == null) return;
            string[] difflines = diff.Split('\n');

            ChangedFileManager = new ChangedFileManager();

            ChangedFile oldFile = null;
            ChangedFile newFile = null;

            //while ((input = re.ReadLine()) != null)
            foreach (string input in difflines)
            {
                //the text '\ No newline at end of file' is sometimes added, ignore
                if (input == @"\ No newline at end of file")
                {
                } else
                if (input.StartsWith("diff --git"))
                {
                } else
                //-- means ?? -> ignore
                if (input == "--")
                {
                }
                else
                    //---  means ?? -> ignore
                    if (input == "---")
                    {
                    }
                    else
                        //If there is no 'oldfile', reset files
                        if (input.StartsWith("--- /dev/null"))
                        {
                            oldFile = null;
                            newFile = null;
                        }
                        else
                            //If there is no 'newfile', reset files
                            if (input.StartsWith("+++ /dev/null"))
                            {
                                oldFile.DeleteFile = true;
                                newFile = null;
                            }
                            else
                                //line starts with --- means, old file
                                if (input.StartsWith("--- ") && !input.StartsWith("--- /dev/null"))
                                {
                                    oldFile = ChangedFileManager.AddFile(input.Substring(6));
                                    //LoadFile(oldFile);
                                    newFile = null;
                                }
                                else
                                    //line starts with +++ means, new file
                                    if (input.StartsWith("+++ ") && !input.StartsWith("+++ /dev/null"))
                                    {
                                        newFile = ChangedFileManager.AddFile(input.Substring(6));
                                        if (oldFile == null)
                                        {
                                            newFile.CreateFile = true;
                                        }
                                    }
                                    else
                                    {
                                        //Add line to patch text
                                        if (newFile != null && 
                                            (input.StartsWith("+") ||
                                             input.StartsWith("-") ||
                                             input.StartsWith(" ") ||
                                             input.StartsWith("@") ) )
                                        {
                                            if (input.StartsWith("@@") &&
                                                input.Contains(" @@ "))
                                            {
                                                //input = input.Replace(" @@", " @@\n");
                                                newFile.PatchText += input.Substring(0, input.IndexOf("@@", 4) + 2);
                                            }
                                            else
                                            {
                                                newFile.PatchText += input + "\n";
                                            }
                                        }
                                    }
            }
        }
    }
}
