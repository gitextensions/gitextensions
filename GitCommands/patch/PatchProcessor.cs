using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PatchApply
{
    public class PatchProcessor
    {
        public List<Patch> ProcessInput(TextReader re, string input, Patch patch)
        {
            var patches = new List<Patch>();
            bool gitPatch = false;
            while (input != null)
            {
                //diff --git a/FileA b/FileB
                //new patch found
                if (input.StartsWith("diff --git "))
                {
                    gitPatch = true;
                    patch = new Patch();
                    patches.Add(patch);

                    Match match = Regex.Match(input,
                                              "[ ][\\\"]{0,1}[a]/(.*)[\\\"]{0,1}[ ][\\\"]{0,1}[b]/(.*)[\\\"]{0,1}");

                    patch.FileNameA = match.Groups[1].Value;
                    patch.FileNameB = match.Groups[2].Value;
                    //patch.FileNameA = input.Substring(input.LastIndexOf(" a/") + 3, input.LastIndexOf(" b/") - (input.LastIndexOf(" a/") + 3));
                    //patch.FileNameB = input.Substring(input.LastIndexOf(" b/") + 3);

                    //The next line tells us what kind of patch
                    //new file mode xxxxxx means new file
                    //delete file mode xxxxxx means delete file
                    //index means -> no new and no delete, edit
                    if ((input = re.ReadLine()) != null)
                    {
                        //WTF! No change
                        if (input.StartsWith("diff --git "))
                        {
                            //No change? lets continue to the next line
                            continue;
                        }

                        //new file!
                        if (input.StartsWith("new file mode "))
                            patch.Type = Patch.PatchType.NewFile;
                        else if (input.StartsWith("deleted file mode "))
                            patch.Type = Patch.PatchType.DeleteFile;
                        else
                            patch.Type = Patch.PatchType.ChangeFile;

                        //we need to move to the line that says 'index'
                        //because we are not sure if we are there yet because
                        //we might point at the new or delete line lines
                        if (!input.StartsWith("index "))
                            if ((input = re.ReadLine()) == null)
                                break;
                    }

                    //The next lines tells us more about the change itself
                    //Read the next
                    if ((input = re.ReadLine()) != null)
                    {
                        //Binary files a/FileA and /dev/null differ
                        //means the file is deleted but the changes are not listed explicid
                        if (input.StartsWith("Binary files a/") && input.EndsWith(" and /dev/null differ"))
                        {
                            patch.File = Patch.FileType.Binary;

                            //Check if the type was set correctly
                            if (patch.Type != Patch.PatchType.DeleteFile)
                                throw new Exception("Change not parsed correct: " + input);

                            patch = null;

                            if ((input = re.ReadLine()) == null)
                                break;

                            //Continue loop, we do not get more info about this change
                            continue;
                        }

                        //Binary files a/FileA and /dev/null differ
                        //means the file is deleted but the changes are not listed explicid
                        if (input.StartsWith("Binary files /dev/null and b/") && input.EndsWith(" differ"))
                        {
                            patch.File = Patch.FileType.Binary;

                            //Check if the type was set correctly
                            if (patch.Type != Patch.PatchType.NewFile)
                                throw new Exception("Change not parsed correct: " + input);

                            //TODO: NOT SUPPORTED!
                            patch.Apply = false;

                            patch = null;

                            if ((input = re.ReadLine()) == null)
                                break;

                            continue;
                        }

                        //GIT binary patch
                        //means the file is binairy 
                        if (input.StartsWith("GIT binary patch"))
                        {
                            patch.File = Patch.FileType.Binary;

                            //TODO: NOT SUPPORTED!
                            patch.Apply = false;

                            patch = null;

                            if ((input = re.ReadLine()) == null)
                                break;

                            continue;
                        }
                    }

                    continue;
                }

                if (!gitPatch || patch != null)
                {
                    //The previous check checked only if the file was binary
                    //--- /dev/null
                    //means there is no old file, so this should be a new file
                    if (input.StartsWith("--- /dev/null"))
                    {
                        if (!gitPatch)
                        {
                            patch = new Patch();
                            patches.Add(patch);
                        }

                        if (gitPatch && patch.Type != Patch.PatchType.NewFile)
                            throw new Exception("Change not parsed correct: " + input);

                        //This line is parsed, NEXT!
                        if ((input = re.ReadLine()) == null)
                            break;

                    }

                    //line starts with --- means, old file name
                    if (input.StartsWith("--- a/") && !input.StartsWith("--- /dev/null"))
                    {
                        if (!gitPatch)
                        {
                            patch = new Patch();
                            patches.Add(patch);
                        }

                        if (gitPatch && patch.FileNameA != (input.Substring(6).Trim()))
                            throw new Exception("Old filename not parsed correct: " + input);

                        patch.FileNameA = (input.Substring(6).Trim());

                        //This line is parsed, NEXT!
                        if ((input = re.ReadLine()) == null)
                            break;

                    }

                    //If there is no 'newfile', reset files
                    if (input.StartsWith("+++ /dev/null"))
                    {
                        if (gitPatch && patch.Type != Patch.PatchType.DeleteFile)
                            throw new Exception("Change not parsed correct: " + input);

                        //This line is parsed, NEXT!
                        if ((input = re.ReadLine()) == null)
                            break;
                    }


                    //line starts with +++ means, new file name
                    //we expect a new file now!
                    if (input.StartsWith("+++ ") && !input.StartsWith("+++ /dev/null"))
                    {
                        Match regexMatch = Regex.Match(input, "[+]{3}[ ][\\\"]{0,1}[b]/(.*)[\\\"]{0,1}");

                        if (gitPatch && patch.FileNameB != (regexMatch.Groups[1].Value.Trim()))
                            throw new Exception("New filename not parsed correct: " + input);

                        patch.FileNameB = (regexMatch.Groups[1].Value.Trim());

                        //This line is parsed, NEXT!
                        if ((input = re.ReadLine()) == null)
                            break;
                    }
                }

                if (patch != null)
                    patch.AppendTextLine(input);

                if ((input = re.ReadLine()) == null)
                    break;
            }

            return patches;
        }
    }
}